using System.ComponentModel.DataAnnotations;

namespace LoginPart.ViewModels
{
    public class ForgotPasswordViewModel
    {
            [Required]
            [EmailAddress]
            public string Email { get; set; }       
    }
}
