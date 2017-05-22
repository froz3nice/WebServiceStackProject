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
    public class AuthController : ServiceStackController
    {
        private JsonServiceClient client = new JsonServiceClient("http://localhost:49469/api/");

        private decimal MoneyLeft()
        {
            var repo = new MongoRepository<UserBalance>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "UsersBalance");
            decimal moneyLeft = 0;
            foreach (var item in repo)
            {
                if (item.Username == GetSession().UserName)
                {
                    moneyLeft = item.MoneyLeft;
                    return moneyLeft;
                }
            }
            return 0;
        }

        // GET: Auth
        public ActionResult Index()
        {
            return View(new AmountLeftmodel() { AmountLeft = MoneyLeft(), Name = GetSession().FirstName });
        }
        
        public ActionResult LogOut()
        {
            var LogoutUrl = "~/api/auth/logout";
            return Redirect(LogoutUrl);
        }

        public ActionResult TransactionList(PaymentTransactionsModel model)
        {   
            var response = client.Get(new GetOnlyUserTransactions()
            {
                UserId = GetSession().UserAuthId,
            });
            TempData["message2"] = GetSession().FirstName;
            model.AmountLeft = MoneyLeft();
            model.Transactions = response.AllTransactions;
            model.Name = GetSession().FirstName;
            return View(model);    
        }
        
        [HttpPost]
        [ActionName("TransactionList")]
        [ValidateAntiForgeryToken]
        public ActionResult TransactionListx(PaymentTransactionsModel model)
        {
            var response = client.Post(new GetOnlyUserTransactions()
            {
                TransactionType = model.SelectedType,
                UserId = GetSession().UserAuthId,
            });
            TempData["message2"] = GetSession().FirstName;
            PaymentTransactionsModel paymentModel = new PaymentTransactionsModel();
            paymentModel.Transactions = response.AllTransactions;
            paymentModel.AmountLeft = MoneyLeft();
            paymentModel.Name = GetSession().FirstName;
            return View(paymentModel);      
        }
       
        public ActionResult TransactionDetails(string TransactionID)
        {
            TempData["message2"] = GetSession().FirstName;
            
            ReportingGateway gate = new ReportingGateway
            (ConfigurationManager.AppSettings["MerchantId"], ConfigurationManager.AppSettings["MerchantPassword"], ServiceMode.Test);
            Transaction transaction = gate.GetTransactionDetails(TransactionID);
            var dto = transaction.ConvertTo<TransactionDeltailsModel>();
            dto.AmountLeft = MoneyLeft();
            dto.Name = GetSession().FirstName;
            return View(dto);
        }

        public ActionResult NewPayment()
        {
            TempData["message2"] = GetSession().FirstName;
            return View(new NewPaymentViewModel() { AmountLeft = MoneyLeft(),Name = GetSession().FirstName });
        }

        [HttpPost]
        [ActionName("NewPayment")]
        [ValidateAntiForgeryToken]
        public ActionResult NewPaymentPOST(NewPaymentViewModel model)
        {
            try {
                TempData["message2"] = GetSession().FirstName;
                if (ModelState.IsValid)
                {
                    IAuthSession session = GetSession();
                    ChargeCreditCard creditCard = new ChargeCreditCard
                    {
                        CardCode = model.CardCode,
                        CardNumber = model.CardNumber,
                        ExpirationDate = model.ExpirationMonth + model.ExpirationYear,
                        Price = model.Price,
                        Username = GetSession().UserName,
                        UserId = session.UserAuthId
                    };
                    var response = client.Post(creditCard);
                    TempData["message"] = response.response;
                    return RedirectToAction("AuthorizePayments");
                }
            }catch(Exception e)
            {
                return View(new NewPaymentViewModel() { AmountLeft = MoneyLeft(),Name = GetSession().FirstName });
            }
            return View(new NewPaymentViewModel() {AmountLeft = MoneyLeft(), Name = GetSession().FirstName });
        }

        public ActionResult AuthorizePayments()
        {
            TempData["message2"] = GetSession().FirstName;
            var message = TempData["message"] as string;
            return View(new AmountLeftmodel() {AmountLeft = MoneyLeft(), Name = GetSession().FirstName });
        }
    }

    //public class LayoutInjecterAttribute : ActionFilterAttribute
    //{
    //    private readonly string _masterName;
    //    public LayoutInjecterAttribute(string masterName)
    //    {
    //        _masterName = masterName;
    //    }

    //    public override void OnActionExecuted(ActionExecutedContext filterContext)
    //    {
    //        base.OnActionExecuted(filterContext);
    //        var result = filterContext.Result as ViewResult;
    //        if (result != null)
    //        {
    //            result.MasterName = _masterName;
    //        }
    //    }
    //}
}