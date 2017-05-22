using AuthorizeNet;
using MvcEmptyWebApp1.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcEmptyWebApp1.Models
{
    public class PaymentTransactionsModel : BalanceBaseModel
    {
        public string CustomerID { get; set; }
        public string TransactionID { get; set; }
        public List<Transactions> Transactions { get; set; } 
        public SelectList AllTypes { get; set; }
        public string SelectedType { get; set; }      

        public SelectList GetTypes()
        {
            AllTypes = new SelectList(new[]
            {
                new SelectListItem {Text = "settledSuccessfully",Value = "Paid" },
                new SelectListItem {Text = "Voided",Value = "Voided" },
                new SelectListItem {Text = "Refund",Value = "Refunded" },
                new SelectListItem {Text = "pending",Value = "pending" }
            }, "Text", "Value");
            return AllTypes;
        }

        public SelectList GetTypesForUser()
        {
            AllTypes = new SelectList(new[]
            {
                new SelectListItem {Text = "settledSuccessfully",Value = "Paid" },
                new SelectListItem {Text = "Refund",Value = "Refunded" }
            }, "Text", "Value");
            return AllTypes;
        }

        //public List<Charge> Charges { get; set; }
        //public string ID { get; set; }
        //public string PaymentMethod { get; set; }
        //public DateTime SettledOn { get; set; }
        //public string State { get; set; }

    }
}