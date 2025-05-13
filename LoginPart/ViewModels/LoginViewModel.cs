using System.ComponentModel.DataAnnotations;

namespace LoginPart.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-mail je povinný.")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage ="Heslo je povinné.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name ="Pamatovat si mě?")]
        public bool RememberMe { get; set; }
	
	public string UserType { get; set; }
    }
}
