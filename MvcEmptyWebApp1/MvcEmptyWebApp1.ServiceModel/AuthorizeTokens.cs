using CodeMash.MongoDB.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    public class AuthorizeTokens : Entity
    {
        public string MerchantId { get; set; }
        public string MerchantPassword { get; set; }
    }
}
