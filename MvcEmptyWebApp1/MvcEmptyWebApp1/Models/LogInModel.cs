using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MvcEmptyWebApp1.Models
{
    public class LogInModel
    {
        [Required(ErrorMessage ="Required field")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Required field")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Continue { get; set; }
        [DisplayName("Remember me")]
        public bool RememberMe { get; set; }
    }
}