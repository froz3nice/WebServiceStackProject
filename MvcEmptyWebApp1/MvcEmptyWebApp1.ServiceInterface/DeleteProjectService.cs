using CodeMash.MongoDB.Repository;
using MvcEmptyWebApp1.ServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceInterface
{
    class DeleteProjectService : Service
    {
        public DeleteResponse Post(Delete del)
        {
            var repo = MongoRepositoryFactory.Create<Project>
                ("mongodb://slavka:slavka@ds023465.mlab.com:23465/slavkalopas");
            repo.DeleteOne(x => x.Id == del.Id);
            DeleteResponse Dp = new DeleteResponse();
            Dp.DeletedId = del.Id;
            return Dp;
        }
    }
}
