using CodeMash.MongoDB.Repository;
using MongoDB.Driver;
using MvcEmptyWebApp1.Models;
using MvcEmptyWebApp1.ServiceModel;
using ServiceStack;
using ServiceStack.Auth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MvcEmptyWebApp1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var repo = new MongoRepository<AuthorizeTokens>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "AuthorizeNetTokens");
            if (repo.Count() == 0)
            {
                AuthorizeTokens model = new AuthorizeTokens()
                {
                    MerchantId = ConfigurationManager.AppSettings["MerchantId"],
                    MerchantPassword = ConfigurationManager.AppSettings["MerchantPassword"]
                };
                repo.InsertOne(model);
            }

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            new AppHost().Init();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
