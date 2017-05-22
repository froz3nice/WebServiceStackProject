using CodeMash.MongoDB.Repository;
using MongoDB.Driver;
using MvcEmptyWebApp1.ServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceInterface
{
    class UpdateProjectService : Service
    {
        public UpdateResponse Any(Update up)
        {
            var repo = MongoRepositoryFactory.Create<Project>
               ("mongodb://slavka:slavka@ds023465.mlab.com:23465/slavkalopas");
            var foundItem = repo.FindOneById(up.Id);
            foundItem.Name = up.Name;
            foundItem.Surname = up.Surname;
            foundItem.Age = up.Age;
            var filter = Builders<Project>.Filter.Eq(x => x.Id, up.Id);
            repo.ReplaceOne(filter, foundItem);           
            UpdateResponse upResp = new UpdateResponse();
            return upResp;
        }
    }
}
