﻿using CodeMash.MongoDB.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceModel
{
    public class PaymentModel : Entity
    {
        public string CardNumber { get; set; }
        public string CVVCode { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryDay { get; set; }       
    }
}
