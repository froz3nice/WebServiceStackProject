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
    class InsertProjectService : Service
    {
        public CreateResponse Any(Create request)
        {
            var repo = MongoRepositoryFactory.Create<Project>
                           ("mongodb://slavka:slavka@ds023465.mlab.com:23465/slavkalopas");
            var project = new Project { Name = request.Name, Surname = request.Surname, Age = request.Age };
            var insertedProject = repo.InsertOne(project);

            var response = repo.FindOneById(insertedProject.Id);
            return new CreateResponse {Result = response.Id };
        }
    }
}
