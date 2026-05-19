using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using LifeSci360.Identity.API.Models;
using LifeSci360.Identity.API.Data;
using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateUserModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly AppIdentityDbContext _context;

        public CreateUserModel(
            UserManager<User> userManager,
            RoleManager<ApplicationRole> roleManager,
            AppIdentityDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [BindProperty]
        public CreateUserInput Input { get; set; }

        public List<SelectListItem> RoleList { get; set; } = new();
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            LoadRoles();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoadRoles();

            if (!ModelState.IsValid) return Page();

            // Check email not already taken
            var existing = await _userManager.FindByEmailAsync(Input.Email);
            if (existing != null)
            {
                ErrorMessage = "A user with this email already exists.";
                return Page();
            }

            // Validate role exists
            if (!await _roleManager.RoleExistsAsync(Input.Role))
            {
                ErrorMessage = $"Role '{Input.Role}' does not exist.";
                return Page();
            }

            var user = new User
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.FullName,
                PhoneNumber = Input.PhoneNumber,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, Input.TempPassword);

            if (result.Succeeded)
            {
                // Assign role via Roles table
                await _userManager.AddToRoleAsync(user, Input.Role);

                // Immutable Audit Log
                var adminUser = await _userManager.GetUserAsync(User);
                _context.AuditLogs.Add(new AuditLog
                {
                    UserID = adminUser?.Id ?? "system",
                    Action = $"Admin provisioned user: {Input.Email} with role: {Input.Role}",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                SuccessMessage = $"User '{Input.FullName}' provisioned successfully with role '{Input.Role}'.";
                Input = new CreateUserInput(); // clear form
                //return Page();
                return RedirectToPage("/Admin/Index");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }

        private void LoadRoles()
        {
            RoleList = _roleManager.Roles
                .Where(r => r.IsActive)
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList();
        }
    }

    public class CreateUserInput
    {
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(10)]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Please select a role.")]
        public string Role { get; set; }
        [Required(ErrorMessage = "Temporary password is required.")]
        [MinLength(10, ErrorMessage = "Password must be at least 10 characters.")]
        public string TempPassword { get; set; }
    }
}