using ServiceStack;
using AuthorizeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    //[Authenticate]
    [Route("/RefundPayment")]
    public class RefundPayment : IReturn<RefundPaymentResponse>
    {
        public string TransactionId { get; set; }
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public decimal Amount { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
    }

    public class RefundPaymentResponse
    {
       public string Message { get; set; }
       public decimal AmountLeft { get; set; }
    }
}
