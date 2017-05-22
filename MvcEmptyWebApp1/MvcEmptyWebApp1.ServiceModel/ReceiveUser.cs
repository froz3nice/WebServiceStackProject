using ServiceStack;
using ServiceStack.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    public class ReceiveUser : IReturn<ReceiveUserResponse>
    {
        public string Username { get; set; }
    }

    public class ReceiveUserResponse
    {
        public UserAuth Data { get; set; }
    }
}
