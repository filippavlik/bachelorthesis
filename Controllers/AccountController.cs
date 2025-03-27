﻿using LoginPart.Data;
using LoginPart.Identity;
using LoginPart.Models;
using LoginPart.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using reCAPTCHA.AspNetCore;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UsersApp.Controllers
{

    public class AccountController : Controller
    {
	    
	private readonly string m_redirectAdmin;
	private readonly string m_redirectReferee;

	private readonly IConfiguration m_configuration;
        private readonly SignInManager<Users> m_signInManager;
        private readonly UserManager<Users> m_userManager;
        private readonly LoginPart.Data.AppDbContext m_appDbContext;

        private readonly LoginPart.Identity.IEmailSender m_emailSender;
        private readonly IJwtTokenService m_jwtTokenService; 

        public AccountController(IConfiguration configuration,SignInManager<Users> signInManager, UserManager<Users> userManager, LoginPart.Data.AppDbContext appDbContext, LoginPart.Identity.IEmailSender emailSender,LoginPart.Identity.IJwtTokenService jwtTokenService)
        {
	    this.m_configuration = configuration;
            this.m_signInManager = signInManager;
            this.m_userManager = userManager;
            this.m_emailSender = emailSender;
            this.m_appDbContext = appDbContext;
            this.m_jwtTokenService = jwtTokenService;
	    this.m_redirectAdmin = GetDockerSecret("RedirectAdmin");
	    this.m_redirectReferee = GetDockerSecret("RedirectReferee");
        }

        #region PRIVATE METHODS
        private async Task<string?> GetRoleForEmail(string email)
        {
            var roleMapping = new Dictionary<int, string>
                    {
                        { 0, "MainAdmin" },
                        { 1, "Admin" },
                        { 2, "Referee" }
                    };

            var role = await m_appDbContext.AllowedEmailAddresses
                .Where(a => a.Email == email)
                .Select(a => new { a.Role })
                .FirstOrDefaultAsync();

            return role != null && roleMapping.ContainsKey(role.Role) ? roleMapping[role.Role] : null;
	}
	private string GetDockerSecret(string secretName)
	{
    		string path = $"/run/secrets/{secretName}";
	    	return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path).Trim() : null;
	}

        #endregion PRIVATE METHODS

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the user is locked out before attempting to sign in
                var user = await m_userManager.FindByNameAsync(model.Email);
                if (user != null && await m_userManager.IsLockedOutAsync(user))
                {
                    ModelState.AddModelError("", "Account is locked. Please try again later.");
                    return View(model);
                }

                var result = await m_signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
		
                if (result.Succeeded)
                {
                    var role = await GetRoleForEmail(model.Email);

                    if (role != null)
                    {
                        if (!await m_userManager.IsInRoleAsync(user, role))
				{
				    await m_userManager.AddToRoleAsync(user, role);
				}
				var roles = await m_userManager.GetRolesAsync(user);

				if (roles.Any())
				{
				    // Use your existing method to generate the token
        		            var token = m_jwtTokenService.GenerateToken(user, roles.FirstOrDefault());

        		            // Set token in HTTP-only cookie
         			    Response.Cookies.Append("auth_token", token, new CookieOptions
        				{
            					HttpOnly = true,
            					Secure = true,
            					SameSite = SameSiteMode.Lax,
            					Expires = DateTime.Now.AddMinutes(30), // Match the token expiration time
            					Domain = ".rozhodcipraha.cz", // Allow sharing across subdomains
        					Path = "/" // Ensures the cookie is accessible across all paths in the domain
						});
				    //todo if someone have two roles
				    // Determine the redirect URL based on the user's role
				    switch (roles.FirstOrDefault())
				    {
					case "MainAdmin":
					case "Admin":
					    return Redirect(m_redirectAdmin); // AdminPart container
					    break;
					case "Referee":
					    return Redirect(m_redirectReferee); // RefereePart container
					    break;
					default:
					    return Redirect("/Home/Index"); // Default fallback
					    break;
				    }
                        	   
				}
                       
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // If no valid role, log the user out and show an error message
                        await m_signInManager.SignOutAsync();
                        ModelState.AddModelError("", "Your email is not associated with a valid role.");
                        return View(model);
                    }
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "Account locked due to multiple failed attempts. Please try again later.");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
	       	    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Users user = new Users
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                };

                var result = await m_userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
	            		
		    try
		    {
                    	// Generate email confirmation token
                    	var token = await m_userManager.GenerateEmailConfirmationTokenAsync(user);

                    	// Create confirmation link
                    	var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        	new { userId = user.Id, token = token }, Request.Scheme);

                    	// Send email with confirmation link
                    	await m_emailSender.SendEmailAsync(user.Email, "Confirm your email",
                        	$"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
		    }
		    catch(Exception emptySendGrid)
		    {
		    	ModelState.AddModelError("", "Error in proccess of sending email , please contact the support!");
                    	return View(model);
		    }

                    // Redirect to a page that tells the user to check their email
                    return RedirectToAction("RegisterConfirmation", "Account", new { email = user.Email });
                }
                else
                {
                    ModelState.AddModelError("", "Error in proccess of creating user , please contact the support!");
			    return View(model);
                }
            }
	    return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await m_userManager.FindByIdAsync(userId);
            if (user == null)
            {
		return NotFound("Unable to load user.");
            }

            var result = await m_userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View("ConfirmEmail");
            }
            else
            {
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult RegisterConfirmation(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await m_userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await m_userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

		try
		{
                	// Generate password reset token
                	var token = await m_userManager.GeneratePasswordResetTokenAsync(user);

                	// Create reset password link
                	var resetLink = Url.Action("ResetPassword", "Account",
                    		new { email = model.Email, token = token }, Request.Scheme);

                	// Send email with reset link
                	await m_emailSender.SendEmailAsync(model.Email, "Reset your password",
   				$"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
	    	}
		catch(ArgumentNullException emptySendGrid)
                {
                        ModelState.AddModelError("", "Error in proccess of sending email , please contact the support!");
                        return View(model);
                }
                // Redirect to forgot password confirmation page
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        public IActionResult ResetPassword(string email, string token)
        {
            if (email == null || token == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await m_userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                var result = await m_userManager.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }
		else
		{
                    ModelState.AddModelError("", "Error in proccess of reseting password , please contact the support!");
		}

            }

            return View(model);
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await m_signInManager.SignOutAsync();
	    // Clear the auth cookie
        	Response.Cookies.Delete("auth_token", new CookieOptions
        	{
            		HttpOnly = true,
            		Secure = true,
            		SameSite = SameSiteMode.Lax,
            		Domain = ".rozhodcipraha.cz"
        	});
            return RedirectToAction("Index", "Home");
        }
    }
}
