using LoginPart.Models;
using Microsoft.AspNetCore.Identity;
using LoginPart.Identity;
using LoginPart.Models;

namespace LoginPart.Identity
{
    public interface IJwtTokenService
    {
        string GenerateToken(Users user, string role);

    }
}
