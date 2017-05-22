using ServiceStack;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    [Route("/create",Verbs = "POST")]
    public class Create : IReturn<CreateResponse>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }
    }
    public class CreateResponse
    {
        public string Result { get; set; }
    }
    public class ClearDataFromCache : IReturn<ClearDataFromCacheResponse>
    {
    }
    public class ClearDataFromCacheResponse
    {
    }
}
