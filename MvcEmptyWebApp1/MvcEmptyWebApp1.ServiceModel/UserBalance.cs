using CodeMash.MongoDB.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    public class UserBalance : Entity
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public decimal MoneyLeft { get; set; }
    }
}
