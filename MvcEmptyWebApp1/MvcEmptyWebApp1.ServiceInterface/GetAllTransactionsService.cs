using AuthorizeNet;
using CodeMash.MongoDB.Repository;
using MongoDB.Driver;
using MvcEmptyWebApp1.ServiceModel;
using ServiceStack;
using ServiceStack.Auth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceInterface
{
    public class GetAllTransactionsService : Service
    {
        private List<Transaction> AllTransactions = new List<Transaction>();

        //[Authenticate]
        public object Any(GetAllTransactions model)
        {
            var cacheKey = "HeyYou";
            if (model.TransactionType != null)
            {
                GetTransactions();
                base.Request.RemoveFromCache(base.Cache, cacheKey);
                switch (model.TransactionType)
                {
                    case "settledSuccessfully":
                        AllTransactions = (from i in AllTransactions
                                           where i.Status == "settledSuccessfully"
                                           select i).ToList();
                        break;

                    case "Voided":

                        AllTransactions = (from i in AllTransactions
                                           where i.Status == "voided"
                                           select i).ToList();
                        break;

                    case "Refund":

                        AllTransactions = (from i in AllTransactions
                                           where i.Status == "refundSettledSuccessfully"
                                           select i).ToList();
                        break;

                    case "pending":

                        AllTransactions = (from i in AllTransactions
                                           where i.Status != "settledSuccessfully" &&
                                           i.Status != "voided" && i.Status != "refundSettledSuccessfully" && i.Status != "generalError"
                                           select i).ToList();
                        break;
                }
                GetAllTransactionsResponse responseDto = new GetAllTransactionsResponse() { AllTransactions = AllTransactions, ErrorMessage = "no" };
                return responseDto;
            }            
                return base.Request.ToOptimizedResultUsingCache(base.Cache, cacheKey, () =>
                {
                    try {
                        GetTransactions();
                        return new GetAllTransactionsResponse() { AllTransactions = AllTransactions, ErrorMessage = "no" };
                    }
                    catch (Exception e)
                    {
                        GetAllTransactionsResponse error = new GetAllTransactionsResponse() {ErrorMessage = e.Message };
                        return error;
                    }
                                       
                });              
        }

        public void GetTransactions()
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
            var Settled = gate.GetTransactionList();
            var Unsettled = gate.GetUnsettledTransactionList();
            AllTransactions = Settled.Concat(Unsettled).ToList();
        }
    }
}
