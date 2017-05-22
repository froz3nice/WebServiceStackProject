using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    //[Authenticate]
   // [Route("/chargeCreditCard", Verbs = "POST")]
    public class ChargeCreditCard : IReturn<ChargeCreditCardResponse>
    {
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CardCode { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatenOn { get; set; }
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
    }

    public class ChargeCreditCardResponse
    {
        public string response { get; set; }
        public decimal AmountLeft { get; set;}
    }
}
