using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

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

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Email address")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}