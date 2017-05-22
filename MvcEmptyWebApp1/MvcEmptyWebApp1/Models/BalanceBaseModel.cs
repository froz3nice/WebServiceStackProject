using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcEmptyWebApp1.Models
{
    public abstract class BalanceBaseModel
    {
        public decimal AmountLeft { get; set; }
        public string Name { get; set; }
    }
}