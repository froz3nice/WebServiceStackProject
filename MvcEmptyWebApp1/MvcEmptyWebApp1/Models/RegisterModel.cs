using ServiceStack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MvcEmptyWebApp1.Models
{
    public class RegisterModel
    {
        public int Id { get; set; }
        //[Required]
        public string Email { get; set; }
         [Required(ErrorMessage ="Required field")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Required field")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool? AutoLogin { get; set; }
        public string Continue { get; set; }
        [Required(ErrorMessage = "Required field")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Required field")]
        public string LastName { get; set; }
        public bool IfAdmin { get; set; }
    }
}