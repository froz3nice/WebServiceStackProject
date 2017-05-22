using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    public class Receive : IReturn<ReceiveResponse>
    {
        public string Id { get; set; }
    }

    public class ReceiveResponse
    {
        public List<Project> Data { get; set; }
    }
}
