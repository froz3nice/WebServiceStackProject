using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    [Route("/Update",Verbs = "POST")]
    public class Update : IReturn<UpdateResponse>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }
    }

    public class UpdateResponse
    {
        public string Result { get; set; }
    }
}
