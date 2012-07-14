using System.ComponentModel.DataAnnotations;

namespace Compilify.Web.Models
{
    public class SignInViewModel
    {
        [Display(Name = "Open id")]
        public string OpenId { get; set; }
        
        [Required]
        [Display(Name = "Username")]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }
        
        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}