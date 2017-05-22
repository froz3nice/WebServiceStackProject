using CodeMash.MongoDB.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    [CollectionName("Transactions")]
    public class Transactions : Entity
    {
        public string CustomerId { get; set; }
        public string Username { get; set; }
        public string TransactionId { get; set; }
        public string CardNumber { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }
}
