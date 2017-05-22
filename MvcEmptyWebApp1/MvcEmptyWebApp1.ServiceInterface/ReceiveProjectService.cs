using CodeMash.MongoDB.Repository;
using MvcEmptyWebApp1.ServiceInterface;
using MvcEmptyWebApp1.ServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceInterface
{
    public class ReceiveProjectService : Service
    {
        public ReceiveResponse Get(Receive rec)
        {
            var repo = MongoRepositoryFactory.Create<Project>
                           ("mongodb://slavka:slavka@ds023465.mlab.com:23465/slavkalopas");
            return new ReceiveResponse()
            {
                Data = repo.Find(x => true)
            };
        }
    }
}
