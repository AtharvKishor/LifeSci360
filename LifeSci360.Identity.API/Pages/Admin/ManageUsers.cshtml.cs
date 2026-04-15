using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LifeSci360.Identity.API.Models;
using LifeSci360.Identity.API.Data;

namespace LifeSci360.Identity.API.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ManageUsersModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly AppIdentityDbContext _context;

        public ManageUsersModel(
            UserManager<User> userManager,
            RoleManager<ApplicationRole> roleManager,
            AppIdentityDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public List<UserListItem> Users { get; set; } = new();
        public List<string> RoleList { get; set; } = new();
        public string SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            RoleList = _roleManager.Roles
                .Where(r => r.IsActive)
                .Select(r => r.Name)
                .ToList();

            await LoadUsers();
        }

        // Change Role
        public async Task<IActionResult> OnPostChangeRoleAsync(
            string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.Email != "admin@lifesci360.com")
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);

                var adminUser = await _userManager.GetUserAsync(User);
                _context.AuditLogs.Add(new AuditLog
                {
                    UserID = adminUser?.Id ?? "system",
                    Action = $"Admin changed role of {user.Email} to: {newRole}",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Toggle Active/Inactive
        public async Task<IActionResult> OnPostToggleStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);

                var adminUser = await _userManager.GetUserAsync(User);
                _context.AuditLogs.Add(new AuditLog
                {
                    UserID = adminUser?.Id ?? "system",
                    Action = $"Admin {(user.IsActive ? "activated" : "deactivated")} user: {user.Email}",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Delete user
        public async Task<IActionResult> OnPostDeleteAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.Email != "admin@lifesci360.com")
            {
                var adminUser = await _userManager.GetUserAsync(User);
                _context.AuditLogs.Add(new AuditLog
                {
                    UserID = adminUser?.Id ?? "system",
                    Action = $"Admin deleted user: {user.Email}",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                await _userManager.DeleteAsync(user);
            }

            return RedirectToPage();
        }

        private async Task LoadUsers()
        {
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserListItem
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = roles.FirstOrDefault() ?? "No Role",
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    IsSystemAdmin = user.Email == "admin@lifesci360.com"
                });
            }
        }
    }

    public class UserListItem
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsSystemAdmin { get; set; }
    }
}