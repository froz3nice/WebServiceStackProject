using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcEmptyWebApp1.Models
{
    public class CustomersDetailsModel
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string LastName { get; set; }
        public Dictionary<string, string> Meta { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string PrimaryEmail { get; set; }
        public List<string> Roles { get; set; }
        public string UserName { get; set; }
    }
}