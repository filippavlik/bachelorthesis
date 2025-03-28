using Microsoft.AspNetCore.Identity;

namespace LoginPart.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
    }
}
