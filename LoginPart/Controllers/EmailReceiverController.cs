using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using LoginPart.Data;
using LoginPart.Identity;
using LoginPart.Models;
using System.Threading.Tasks;

namespace YourService.Controllers
{
    [Route("internal")]
    [ApiController]
    public class EmailReceiver : ControllerBase
    {
        private readonly ILogger<EmailReceiver> _logger;
        private readonly AppDbContext _appDbContext;

        public EmailReceiver(AppDbContext appDbContext, ILogger<EmailReceiver> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        [HttpPost("user-emails")]
        public async Task<IActionResult> ReceiveUsersEmails([FromBody] List<string> emails)
        {
            try
            {
                bool success = true;

                foreach (string email in emails)
                {
                    var existingEmail = await _appDbContext.AllowedEmailAddresses
                        .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());

                    if (existingEmail != null)
                    {
                        _logger.LogInformation($"Email {email} already in allowed list");
                        success = false;
                        continue;
                    }

                    _appDbContext.AllowedEmailAddresses.Add(new AllowedEmailAddress
                    {
                        Email = email,
                        Role = 2
                    });

                    _logger.LogInformation($"Successfully added {email} to allowed list");
                }

                if (success)
                {
                    await _appDbContext.SaveChangesAsync();
                    return Ok(new { status = "success", message = "Emails added to allowed list" });
                }
                else
                {
		    await _appDbContext.SaveChangesAsync();
                    return Ok(new { status = "success", message = "Some emails already existed in the allowed list" });
		}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding email(s) to allowed list");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

    }
}

