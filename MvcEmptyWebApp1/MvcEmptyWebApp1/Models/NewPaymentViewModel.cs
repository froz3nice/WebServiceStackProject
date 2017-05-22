using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MvcEmptyWebApp1.Models
{
    public class NewPaymentViewModel : BalanceBaseModel
    {
        [Required(ErrorMessage ="Enter a valid Credit card number")]
        [CreditCard]
        [DisplayName("Card number")]
        public string CardNumber { get; set; }

        [Required]
        [DisplayName("Expiration date (yy/mm)")]
        public string ExpirationYear { get; set; }

        [Required]
        public string ExpirationMonth { get; set; }

        [Required(ErrorMessage ="Card code field is not filled")]
        [MinLength(3,ErrorMessage = "card code is not valid")]
        [MaxLength(4, ErrorMessage = "card code is not valid")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Card code must be numeric")]
        [DisplayName("Card code")]
        public string CardCode { get; set; }

        [Required(ErrorMessage ="Price field is not filled")]
        public decimal Price { get; set; }

        public IEnumerable<string> GetAllYears()
        {
            for (int i = DateTime.Now.Year-2000; i < DateTime.Now.Year+6-2000; i++)
            {
                yield return i.ToString();
            }
        }
        public IEnumerable<string> GetMonths()
        {
            for (int i = 1; i < 13; i++)
            {
                if (i < 10)
                    yield return "0" + i.ToString();
                else yield return i.ToString();
            }
        }

    }
}