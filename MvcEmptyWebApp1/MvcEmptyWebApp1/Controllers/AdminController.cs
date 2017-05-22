using AuthorizeNet;
using CodeMash.MongoDB.Repository;
using MongoDB.Driver;
using MvcEmptyWebApp1.Models;
using MvcEmptyWebApp1.ServiceModel;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcEmptyWebApp1.Controllers
{
    [Authenticate]
    [RequiredRole("Admin")]
    public class AdminController : ServiceStackController
    {
        private JsonServiceClient client = new JsonServiceClient("http://localhost:49469/api/");

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LogOut()
        {
            var LogoutUrl = "~/api/auth/logout";
            return Redirect(LogoutUrl);
        }

        public ActionResult Customers()
        {
            var repo = new MongoRepository<UserAuth>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "UserAuth");
            var users = repo.Find(x => true).ToList();
            var Users = new List<RegisterModel>();
            foreach (var item in users)
            {
                var dto = item.ConvertTo<RegisterModel>();
                foreach (var role in item.Roles)
                {
                    if (role == "Admin")
                        dto.IfAdmin = true;
                    else dto.IfAdmin = false;
                }
                Users.Add(dto);
            }
            return View(Users);
        }

        public ActionResult Settings()
        {
            var repo = new MongoRepository<AuthorizeTokens>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "AuthorizeNetTokens");
            foreach (var item in repo)
            {
                AuthorizeTokens model = new AuthorizeTokens()
                {
                    MerchantId = item.MerchantId,
                    MerchantPassword = item.MerchantPassword,
                    Id = item.Id
                };
                return View(model);
            }
            return View(new AuthorizeTokens());
        }

        [HttpPost]
        public ActionResult Settings(AuthorizeTokens model)
        {
            try {
                var repo = new MongoRepository<AuthorizeTokens>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "AuthorizeNetTokens");
                foreach (var item in repo)
                {                  
                    var filter = Builders<AuthorizeTokens>.Filter.Eq(x => x.Id,model.Id);
                    repo.ReplaceOne(filter,model);
                }
                var resp = client.Post(new ClearDataFromCache(){ });
                ViewData["Message"] = "Successfully saved!";
            }
            catch(Exception e)
            {
                ViewData["Message"] = e.Message;
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult CustomerDetails(string UserName)
        {
            var response = client.Get(new ReceiveUser() { Username = UserName });
            var dto = response.Data.ConvertTo<CustomersDetailsModel>();           
            return View(dto);
        }

        [HttpPost]
        [ActionName("CustomerDetails")]
        public ActionResult CustomerDetailsPost(string UserName)
        {
            ViewData["UserName"] = UserName;
            return RedirectToAction("DeleteCustomer");
        }

        public ActionResult DeleteCustomer(string UserName)
        {
            return View();
        }

        [HttpPost]
        [ActionName("DeleteCustomer")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCustomer2(string UserName)
        {
            var repo = new MongoRepository<UserAuth>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "UserAuth");
            repo.DeleteOne(x => x.UserName == UserName);
            var CustomerID = (from i in repo
                              where i.UserName == UserName
                              select i.Id).SingleOrDefault().ToString();
            var repo2 = new MongoRepository<UserBalance>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "UsersBalance");
            repo2.DeleteOne(x => x.Username == UserName);
            //var transactions = new MongoRepository<Transactions>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "Transactions");
            //transactions.DeleteMany(x => x.Id == CustomerID);
            return RedirectToAction("Customers");
        }

        public ActionResult AllTransactionList(PaymentTransactionsModel model)
        {
            try
            {
                var response = client.Send<GetAllTransactionsResponse>(new GetAllTransactions(){ });
                TempData["message2"] = GetSession().FirstName;
                if (response.ErrorMessage == "no")
                {
                    return View(Tuple.Create(response.AllTransactions, model));
                }
                else return RedirectToAction("Error");
            }
            catch (Exception e)
            {
                var RedirectUrl = Url.Action("LogIn", "Home");
                return Redirect(RedirectUrl);
            }
        }

        [HttpPost]
        [ActionName("AllTransactionList")]
        public ActionResult TransactionList([Bind(Prefix = "Item2")] PaymentTransactionsModel model)
        {
            var response = client.Get(new GetAllTransactions()
            {
                TransactionType = model.SelectedType
            });
            if (response.ErrorMessage == "no")
            {
                return View(Tuple.Create(response.AllTransactions, new PaymentTransactionsModel()));
            }
            else return RedirectToAction("Error"); 
        }

        public ActionResult AreYourReallyWantToRefund(PaymentTransactionsModel model)
        {
            var ItemToRefund = TempData["Transaction"];
            return View(model);
        }

        [HttpPost]
        [ActionName("AreYourReallyWantToRefund")]
        public ActionResult AreYourReallyWantToRefundPost(PaymentTransactionsModel model)
        {
            var getTransactionsResponse = client.Get(new GetAllTransactions()
            {
                TransactionType = model.SelectedType
            });
            var refundDetails = (from i in getTransactionsResponse.AllTransactions
                                 where i.TransactionID == model.TransactionID
                                 select i).SingleOrDefault();            

            var refundPayment = new RefundPayment
            {
                TransactionId = model.TransactionID,
                Amount = refundDetails.AuthorizationAmount,
                CardNumber = refundDetails.CardNumber,
                ExpirationDate = refundDetails.CardExpiration,
            };

            var RefundResponseDTO = client.Post(refundPayment);
            TempData["message"] = RefundResponseDTO.Message;
            return RedirectToAction("AuthorizePayments");
        }

        public ActionResult TransactionDetails(string TransactionID)
        {
            ReportingGateway gate = new ReportingGateway
            (ConfigurationManager.AppSettings["MerchantId"], ConfigurationManager.AppSettings["MerchantPassword"], ServiceMode.Test);
            Transaction transaction = gate.GetTransactionDetails(TransactionID);
            var dto = transaction.ConvertTo<TransactionDeltailsModel>();
            return View(dto);
        }
        public ActionResult AuthorizePayments()
        {
            var message = TempData["message"] as string;
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}