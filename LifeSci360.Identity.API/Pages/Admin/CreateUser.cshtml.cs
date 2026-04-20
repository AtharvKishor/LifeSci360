using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LifeSci360.Identity.API.Models;
using LifeSci360.Identity.API.Data;

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

        public List<string> RoleList { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        // Form fields
        [BindProperty] public string FullName { get; set; } = string.Empty;
        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string PhoneNumber { get; set; } = string.Empty;
        [BindProperty] public string Role { get; set; } = string.Empty;
        [BindProperty] public string Password { get; set; } = string.Empty;
        [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;

        public void OnGet()
        {
            LoadRoles();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoadRoles();

            // Validate passwords match
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return Page();
            }

            // Check email not already taken
            var existing = await _userManager.FindByEmailAsync(Email);
            if (existing != null)
            {
                ErrorMessage = "A user with this email already exists.";
                return Page();
            }

            // Create the user
            var newUser = new User
            {
                UserName = Email,
                Email = Email,
                FullName = FullName,
                PhoneNumber = PhoneNumber,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser, Password);

            if (!result.Succeeded)
            {
                ErrorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                return Page();
            }

            // Assign role
            if (!string.IsNullOrEmpty(Role))
                await _userManager.AddToRoleAsync(newUser, Role);

            // Audit log
            var adminUser = await _userManager.GetUserAsync(User);
            _context.AuditLogs.Add(new AuditLog
            {
                UserID = adminUser?.Id ?? "system",
                Action = $"Admin created new user: {Email} with role: {Role}",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Redirect back to manage users with success
            return RedirectToPage("/Admin/ManageUsers");
        }

        private void LoadRoles()
        {
            RoleList = _roleManager.Roles
                .Where(r => r.IsActive && r.Name != "Admin")
                .Select(r => r.Name!)
                .ToList();
        }
    }
}