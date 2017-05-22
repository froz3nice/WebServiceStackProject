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
    public class ReceiveUserService : Service
    {
        public ReceiveUserResponse Any(ReceiveUser model)
        {
            var repo = new MongoRepository<UserAuth>(new MongoUrl(ConfigurationManager.AppSettings["Database"]), "UserAuth");
            return new ReceiveUserResponse()
            {
                Data = repo.Find(x => x.UserName == model.Username).FirstOrDefault()
            };
        }
    }
}
