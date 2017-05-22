using AuthorizeNet;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    public class GetOnlyUserTransactions : IReturn<GetOnlyUserTransactionsResponse>
    {
        public string TransactionType { get; set; }
        public string UserId { get; set; }
    }

    public class GetOnlyUserTransactionsResponse
    {
        public List<Transactions> AllTransactions { get; set; }
        public string ErrorMessage { get; set; }
    }
}
