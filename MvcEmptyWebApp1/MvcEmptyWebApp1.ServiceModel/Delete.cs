using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    public class Delete : IReturn<DeleteResponse>
    {
        public string Id { get; set; }
    }

    public class DeleteResponse
    {
        public string DeletedId { get; set; }
    }
    
}
