using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServiceStack.Mvc;
using MvcEmptyWebApp1.Models;
using ServiceStack;
using MvcEmptyWebApp1.ServiceModel;
using AuthorizeNet;
using ServiceStack.Auth;
using System.Web.Security;
using System.Configuration;
using AuthorizeNet.Api.Controllers.Bases;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using System.Web.Helpers;
using CodeMash.MongoDB.Repository;
using MongoDB.Driver;

namespace MvcEmptyWebApp1.Controllers
{
    public class HomeController : ServiceStackController
    {
        private JsonServiceClient client = new JsonServiceClient("http://localhost:49469/api/");
        private IAuthRepository AuthRepo { get; set; }

        public ActionResult Register()
        {
            return View(new RegisterModel());
        }
        [AllowAnonymous]
        [HttpPost]
        [ActionName("Register")]
        public ActionResult register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                try {
                    client.Post(new Register
                    {
                        AutoLogin = true,
                        DisplayName = model.FirstName,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserName = model.UserName,
                        Email = model.Email,
                        Password = model.Password,
                    });
                    Authenticate(model.UserName, model.Password);
                    var Balance = new MongoRepository<UserBalance>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "UsersBalance");
                    var Transactions = new MongoRepository<Transactions>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "Transactions");
                    
                    Balance.InsertOne(new UserBalance()
                    {
                        MoneyLeft = 9001,
                        Username = model.UserName
                    });
                    var RedirectUrl = Url.Action("TransactionList", "Auth");                   
                    return Redirect(RedirectUrl);
                }
                catch (Exception e)
                {
                    TempData["message"] = "Enter another username";
                    return View("Register", new RegisterModel());
                }
            }
            else {
                return View("Register", new RegisterModel());
            }
        }

        public ActionResult LogIn()
        {
            if(GetSession().UserName != null)
            {
                if (GetSession().HasRole("Admin"))
                {
                    return RedirectToAction("Customers", "Admin");
                }
                else
                {
                    return RedirectToAction("TransactionList", "Auth");
                }
            }
            return View();
        }

        public void Authenticate(string username, string password)
        {
            using (var authService = ResolveService<AuthenticateService>())
            {
                try
                {
                    var response = authService.Authenticate(new Authenticate
                    {
                        provider = CredentialsAuthProvider.Name,
                        UserName = username,
                        Password = password,
                        RememberMe = true,
                    });
                    FormsAuthentication.SetAuthCookie(username, true);
                }
                catch (Exception e)
                {
                    TempData["error"] = e.Message;
                }
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ActionName("LogIn")]
        public ActionResult Login(LogInModel model)
        {
            if (ModelState.IsValid)
            {
                Authenticate(model.Username, model.Password);
                string RedirectUrl = "";
                        
                if(GetSession().HasRole("Admin"))
                        RedirectUrl = Url.Action("Customers", "Admin");
                else RedirectUrl = Url.Action("TransactionList", "Auth");
                //var SignInUrl = string.Format("~/api/auth/credentials?UserName={0}&Password={1}&continue{2}&RememberMe{3}",
                //    model.Username, model.Password, RedirectUrl, model.RememberMe);
                return Redirect(RedirectUrl);
                    }
            return View();
        }

        public ActionResult AuthorizePayments()
        {
            var message = TempData["message"] as string;
            return View();
        }


        //CRUD--------------------------------------------------------------------------------------------
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(PaymentDetails model)
        {
            if (ModelState.IsValid && model.Surname != null && model.Name != null)
            {
                Create cr = new Create { Name = model.Name, Surname = model.Surname, Age = model.Age };
                var response = client.Post(cr);
                return RedirectToAction("Index");
            }
            return View();
        }

        public ActionResult Results()
        {
            if (ModelState.IsValid)
            {
                var response = client.Get(new Receive());
            
                var list = (from a in response.Data
                            select new PaymentDetails
                            {
                                Id = a.Id,
                                Name = a.Name,
                                Surname = a.Surname,
                                Age = a.Age
                            }).ToList();
                return View(list);
            }
            return View();
        }

        [HttpGet]
        public ActionResult Update(PaymentDetails model)
        {
            return View(model);
        }

        [HttpPost]
        [ActionName("Update")]
        public ActionResult UpdatePOST(PaymentDetails model)
        {
            if (ModelState.IsValid)
            {
                Update cr = new Update {Id = model.Id, Name = model.Name, Surname = model.Surname, Age = model.Age };
                var response = client.Post(cr);

                return RedirectToAction("Results");
            }
            return View();
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            DeleteResponse pay = new DeleteResponse { DeletedId = id };
            return View(pay);
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeletePost(string id)
        {
            if (ModelState.IsValid)
            {
                var response = client.Post(new Delete{ Id = id });
                return RedirectToAction("Results");
            }
            return View();
        }


        public ActionResult Index()
        {
            var Users = new MongoRepository<UserAuth>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "UserAuth");
            var AdminUsername = "tesla";

            if (Users.Count() == 0)
            {
                client.Post(new Register
                {
                    AutoLogin = true,
                    UserName = AdminUsername,
                    Email = "labas@gmail.com",
                    Password = "galvis",
                });

                var user = (from i in Users
                            where i.UserName == AdminUsername
                            select i).SingleOrDefault();
                user.Roles = new List<string>() { "Admin" };
                var filter = Builders<UserAuth>.Filter.Eq(x => x.Id, user.Id);
                Users.ReplaceOne(filter, user);
            }
            return View("Index");
        }

    }
}