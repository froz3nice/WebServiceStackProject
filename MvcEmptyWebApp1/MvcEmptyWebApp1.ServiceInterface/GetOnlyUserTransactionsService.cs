using AuthorizeNet;
using CodeMash.MongoDB.Repository;
using MongoDB.Driver;
using MvcEmptyWebApp1.ServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceInterface
{
    class GetOnlyUserTransactionsService : Service
    {
        public GetOnlyUserTransactionsResponse Any(GetOnlyUserTransactions model)
        {
       //     ReportingGateway gate = new ReportingGateway
       //(ConfigurationManager.AppSettings["MerchantId"], ConfigurationManager.AppSettings["MerchantPassword"], ServiceMode.Test);
       //     var Settled = gate.GetTransactionList();
       //     var Unsettled = gate.GetUnsettledTransactionList();
       //     var AllTransactions = Settled.Concat(Unsettled).ToList();
            var repo = new MongoRepository<Transactions>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "Transactions");

            var PaidTransactions = repo.Find(x => x.CustomerId == model.UserId).ToList();

            var repoRefunded = new MongoRepository<Transactions>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "RefundedTransactions");

            var refunded = repoRefunded.Find(x => x.CustomerId == model.UserId).ToList();

            var UserTransactions = PaidTransactions.Concat(refunded).ToList();
            //foreach (var item in AllTransactions)
            //{
            //    var details = gate.GetTransactionDetails(item.TransactionID);
            //    if (details.CustomerID == model.UserId)
            //    {
            //        UserTransactions.Add(item);
            //    }
            //break;
            //}
            switch (model.TransactionType)
                {
                    case "settledSuccessfully":
                    UserTransactions = PaidTransactions;
                        break;

                    case "Refund":
                    UserTransactions = refunded;                                          
                        break;
                }
            GetOnlyUserTransactionsResponse response = new GetOnlyUserTransactionsResponse
            {
                AllTransactions = UserTransactions,
            };
                return response;
            }
        }
    }
