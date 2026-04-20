using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LifeSci360.Identity.API.Models;
using LifeSci360.Identity.API.Data;
using LifeSci360.Shared.DTOs;

namespace LifeSci360.Identity.API.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly AppIdentityDbContext _context;

        public LoginModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            AppIdentityDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public LoginRequest Input { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);

            // Check user exists and is active
            if (user == null || !user.IsActive)
            {
                ErrorMessage = "Invalid credentials.";
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Audit log — successful login
                _context.AuditLogs.Add(new AuditLog
                {
                    UserID = user.Id,
                    Action = "User Logged In Successfully",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                // Get role and redirect to correct dashboard
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                return role switch
                {
                    "Admin" => RedirectToPage("/Admin/Index"),
                    "ResearchScientist" => RedirectToPage("/Dashboard/ResearchScientist"),
                    "LabTechnician" => RedirectToPage("/Dashboard/LabTechnician"),
                    "ClinicalTrialManager" => RedirectToPage("/Dashboard/ClinicalManagerProtocol"),
                    "RegulatoryOfficer" => RedirectToPage("/Dashboard/RegulatoryOfficer"),
                    "DataManager" => RedirectToPage("/Dashboard/DataManager"),
                    _ => RedirectToPage("/Index")
                };
            }

            if (result.IsLockedOut)
            {
                // Audit log — lockout
                _context.AuditLogs.Add(new AuditLog
                {
                    UserID = user.Id,
                    Action = "Account Locked Out",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                ErrorMessage = "Account locked due to multiple failed attempts. Try again in 15 minutes.";
                return Page();
            }

            // Audit log — failed attempt
            _context.AuditLogs.Add(new AuditLog
            {
                UserID = user.Id,
                Action = "Failed Login Attempt",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            ErrorMessage = "Invalid credentials.";
            return Page();
        }
    }
}