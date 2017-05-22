using AuthorizeNet;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    [Route("/getAllTransactions")]
    //[Authenticate]
    public class GetAllTransactions : IReturn<GetAllTransactionsResponse>
    {
        public string TransactionType { get; set; }
        public string UserId { get; set; }
    }
    public class GetAllTransactionsResponse
    {
        public List<Transaction> AllTransactions { get; set; }
        public string ErrorMessage { get; set; }
    }
}
