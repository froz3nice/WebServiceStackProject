using AuthorizeNet;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using CodeMash.MongoDB.Repository;
using MongoDB.Driver;
using MvcEmptyWebApp1.ServiceModel;
using ServiceStack;
using ServiceStack.Auth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceInterface
{
    class RefundPaymentService : Service
    {
        public RefundPaymentResponse Any(RefundPayment model)
        {
            var repo = new MongoRepository<AuthorizeTokens>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "AuthorizeNetTokens");
            string MerchantId = "";
            string MerchantPassword = "";
            foreach (var item in repo)
            {
                MerchantId = item.MerchantId;
                MerchantPassword = item.MerchantPassword;
            }
            ReportingGateway gate = new ReportingGateway(MerchantId, MerchantPassword, ServiceMode.Test);
            var details = gate.GetTransactionDetails(model.TransactionId);
            
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = ConfigurationManager.AppSettings["MerchantId"],
                ItemElementName = ItemChoiceType.transactionKey,
                Item = ConfigurationManager.AppSettings["MerchantPassword"]
            };

            var creditCard = new creditCardType
            {
                cardNumber = details.CardNumber,
                expirationDate = details.CardExpiration
            };

            //standard api call to retrieve response
            var paymentType = new paymentType { Item = creditCard };
            customerDataType customer = new customerDataType()
            {
                id = model.UserId
            };
            string UserID = gate.GetTransactionDetails(model.TransactionId).CustomerID;
            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.refundTransaction.ToString(),    // refund type
                payment = paymentType,
                amount = details.AuthorizationAmount,
                refTransId = details.TransactionID,
                customer = customer
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            var controller = new createTransactionController(request);
            
            controller.Execute();
            
            var response = controller.GetApiResponse();
            RefundPaymentResponse resp = new RefundPaymentResponse();
            
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response.transactionResponse != null)
                {
                    resp.Message ="Successfully refunded";
                }
                var cacheKey = "HeyYou";
                base.Request.RemoveFromCache(base.Cache, cacheKey);
                var Balance = new MongoRepository<UserBalance>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "UsersBalance");
                var users = new MongoRepository<UserAuth>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "UserAuth");
                //int userId = int.Parse(UserID);
                //var Username = (from i in users
                //                where i.Id == userId
                //                select i.UserName).FirstOrDefault();
                //foreach (var item in Balance)
                //{
                //    if (item.Username == Username)
                //    {
                //        item.MoneyLeft = item.MoneyLeft + model.Amount;
                //        var filter = Builders<UserBalance>.Filter.Eq(x => x.Id, item.Id);
                //        Balance.ReplaceOne(filter, item);
                //        resp.AmountLeft = item.MoneyLeft;
                //        break;
                //    }
                //}
                //var repo = MongoRepositoryFactory.Create<Transactions>
                //("mongodb://slavka:slavka@ds023465.mlab.com:23465/slavkalopas", "RefundedTransactions");
                var repo2 = new MongoRepository<Transactions>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "RefundedTransactions");

                repo2.InsertOne(new Transactions()
                {
                    CardNumber = model.CardNumber,
                    TransactionId = response.transactionResponse.transId,
                    Amount = details.AuthorizationAmount,
                    CreatedOn = DateTime.Now,
                    Description = "Boat",
                    CustomerId = UserID,
                    Status = "Refunded"
                });
                //var repoToDelete = MongoRepositoryFactory.Create<Transactions>
                //("mongodb://slavka:slavka@ds023465.mlab.com:23465/slavkalopas", "Transactions");
                var repoDelete = new MongoRepository<Transactions>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "Transactions");

                repoDelete.DeleteOne(x => x.TransactionId == model.TransactionId);
            }
            else if (response != null)
            {
                if (response.transactionResponse != null)
                {
                    resp.Message = "Transaction Error : " + response.transactionResponse.errors[0].errorCode + " " + response.transactionResponse.errors[0].errorText;
                }
            }

            return resp;
        }
    }
}
