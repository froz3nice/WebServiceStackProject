using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Funq;
using ServiceStack;
using ServiceStack.Mvc;
using MvcEmptyWebApp1.ServiceInterface;
using ServiceStack.Auth;
using ServiceStack.Caching;
using ServiceStack.Authentication.MongoDb;
using System.Configuration;
using MongoDB.Driver;
using ServiceStack.Configuration;
using CodeMash.MongoDB.Repository;

namespace MvcEmptyWebApp1
{
    public class AppHost : AppHostBase
    {
        private JsonServiceClient Jsonclient = new JsonServiceClient("http://localhost:49469/api/");
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost()
            : base("MvcEmptyWebApp1", typeof(ReceiveProjectService).Assembly)
        {          
        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            SetConfig(new HostConfig
            {
                HandlerFactoryPath = "api",
            });
            //Config examples
            //this.Plugins.Add(new PostmanFeature());
            //this.Plugins.Add(new CorsFeature());

            ControllerBuilder.Current.SetControllerFactory(new FunqControllerFactory(container));

            Plugins.Add(new AuthFeature(() => new AuthUserSession(),
             new IAuthProvider[] {
                new CredentialsAuthProvider(), 
                //HTML Form post of UserName/Password credentials
              })
            {
                HtmlRedirect = "/Home/Login"
            });
         
            
            Plugins.Add(new RegistrationFeature());
            container.Register<ICacheClient>(new MemoryCacheClient());

            var mongoDbAddress = ConfigurationManager.AppSettings["Database"];
            var client = new MongoClient(mongoDbAddress);
            var database = client.GetDatabase("PaymentsInfo");
            container.Register<IUserAuthRepository>(c => new MongoDbAuthRepositoryAsync(database, true));
            var authRepo = (MongoDbAuthRepositoryAsync)container.Resolve<IUserAuthRepository>();
            authRepo.CreateMissingCollections();
           
            //container.Register<IUserAuthRepository>(c => new MongoDbAuthRepositoryAsync("mongodb://slavka:slavka@ds023465.mlab.com:23465/slavkalopas", true));
            //var userRep = new MongoDbAuthRepository("mongodb://slavka:slavka@ds023465.mlab.com:23465/slavkalopas","Users") ;
            //container.Register<IUserAuthRepository>(userRep);
        }
    }


}