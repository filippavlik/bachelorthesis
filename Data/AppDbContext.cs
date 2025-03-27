using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LoginPart.Models;

namespace LoginPart.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }

        // Add DbSet for AllowedEmailAddresses
        public DbSet<AllowedEmailAddress> AllowedEmailAddresses { get; set; }
    }
}
