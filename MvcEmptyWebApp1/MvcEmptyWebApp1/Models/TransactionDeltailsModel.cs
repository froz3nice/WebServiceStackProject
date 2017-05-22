using AuthorizeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcEmptyWebApp1.Models
{
    public class TransactionDeltailsModel : BalanceBaseModel
    {
        public decimal AuthorizationAmount { get; set; }
        public string AuthorizationCode { get; set; }
        public string AVSCode { get; set; }
        public string AVSResponse { get; }
        public DateTime BatchSettledOn { get; set; }
        public string BatchSettlementID { get; set; }
        public string BatchSettlementState { get; set; }
        public Address BillingAddress { get; set; }
        public string CardExpiration { get; set; }
        public string CardNumber { get; set; }
        public string CardResponse { get; }
        public string CardResponseCode { get; set; }
        public string CardType { get; set; }
        public string CAVVCode { get; set; }
        public string CAVVResponse { get; }
        public string CustomerEmail { get; set; }
        public string CustomerID { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string Description { get; set; }
        public decimal Duty { get; set; }
        public string DutyDescription { get; set; }
        public BankAccount eCheckBankAccount { get; set; }
        public string FirstName { get; set; }
        public IList<string> FraudFilters { get; set; }
        public string InvoiceNumber { get; set; }
        public bool IsRecurring { get; set; }
        public string LastName { get; set; }
        public IList<LineItem> LineItems { get; }
        public string OrderDescription { get; set; }
        public string PONumber { get; set; }
        public decimal RequestedAmount { get; set; }
        public int ResponseCode { get; set; }
        public string ResponseReason { get; set; }
        public decimal SettleAmount { get; set; }
        public decimal Shipping { get; set; }
        public Address ShippingAddress { get; set; }
        public string ShippingDescription { get; set; }
        public string Status { get; set; }
        public SubscriptionPayment Subscription { get; set; }
        public decimal Tax { get; set; }
        public string TaxDescription { get; set; }
        public bool TaxExempt { get; set; }
        public string TransactionID { get; set; }
        public string TransactionType { get; set; }
    }
}