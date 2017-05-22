using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using ServiceStack;
using AuthorizeNet.Api.Controllers.Bases;
using MvcEmptyWebApp1.ServiceModel;
using System.Configuration;
using ServiceStack.Auth;
using CodeMash.MongoDB.Repository;
using MongoDB.Driver;
using AuthorizeNet;
using System;
//using MvcEmptyWebApp1.Models;

namespace MvcEmptyWebApp1.ServiceInterface
{
    class ChargeCreditCardService : Service
    {
        public ChargeCreditCardResponse Any(ChargeCreditCard model)
        {
            var Balance = new MongoRepository<UserBalance>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "UsersBalance");
          
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
            var repo = new MongoRepository<AuthorizeTokens>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "AuthorizeNetTokens");
            string MerchantId = "";
            string MerchantPassword = "";
            foreach (var item in repo)
            {
                MerchantId = item.MerchantId;
                MerchantPassword = item.MerchantPassword;
            }

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = MerchantId,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = MerchantPassword
            };

            var creditCard = new creditCardType
            {
                cardNumber = model.CardNumber,
                expirationDate = model.ExpirationDate,
                cardCode = model.CardCode
            };

            var paymentType = new paymentType { Item = creditCard };

            customerDataType customer = new customerDataType()
            {
                id = model.UserId
            };
            var billingAddress = new customerAddressType
            {
                firstName = "Jonas",
                lastName = "Sniegas",
            };
            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),
                amount = model.Price,
                payment = paymentType,
                billTo = billingAddress,
                customer = customer
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            var controller = new createTransactionController(request);

            ChargeCreditCardResponse cr = new ChargeCreditCardResponse();
            if (model.Username != "tesla")
                controller.Execute();
            else {
                cr.response = "Admin cannot do payments";
                return cr;
            } 

            var response = controller.GetApiResponse();
            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.transactionResponse != null)
                    {
                        cr.response = "Success, Auth Code : " + response.transactionResponse.authCode;
                    }
                    var cacheKey = "HeyYou";
                    Request.RemoveFromCache(base.Cache, cacheKey);
                    foreach (var item in Balance)
                        if (item.Username == model.Username)
                        {
                            if (item.MoneyLeft - model.Price >= 0)
                            {
                                item.MoneyLeft = item.MoneyLeft - model.Price;
                                var filter = Builders<UserBalance>.Filter.Eq(x => x.Id, item.Id);
                                Balance.ReplaceOne(filter, item);
                                cr.AmountLeft = item.MoneyLeft;
                            }
                            else
                            {
                                return new ChargeCreditCardResponse() { response = "Can not execute transaction, insufficient funds" };
                            }
                            break;
                        }
                    //var repo = MongoRepositoryFactory.Create<Transactions>
                    //                ("mongodb://slavka:slavka@ds023465.mlab.com:23465/slavkalopas", "Transactions");

                    var repo2 = new MongoRepository<Transactions>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "Transactions");

                    repo2.InsertOne(new Transactions()
                    {
                        CardNumber = model.CardNumber,
                        Username = model.Username,
                        TransactionId = response.transactionResponse.transId,
                        Amount = model.Price,
                        CreatedOn = DateTime.Now,
                        Description = "Boat",
                        CustomerId = model.UserId,
                        Status = "settledSuccessfully"
                    });
                }
                else
                {
                    if (response.transactionResponse != null)
                    {
                        cr.response = "Transaction Error  ";// + response.transactionResponse.errors[0].errorCode + " " + response.transactionResponse.errors[0].errorText;
                    }
                }
                return cr;
            }
            else
            {
                cr.response = "Something went wrong";
                return cr;
            }
            }            
        }
}
