using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LifeSci360.Identity.API.Data;
using LifeSci360.Identity.API.Models;

namespace LifeSci360.Identity.API.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly AppIdentityDbContext _context;

        public IndexModel(UserManager<User> userManager, AppIdentityDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public int TotalUsers { get; set; }
        public int ActiveProtocols { get; set; }
        public int PendingSamples { get; set; }
        public int EnrolledPatients { get; set; }
        public List<UserViewModel> RecentUsers { get; set; } = new();

        public async Task OnGetAsync()
        {
            var users = _userManager.Users.ToList();
            TotalUsers = users.Count;
            
            TotalUsers = await _userManager.Users.CountAsync();


            ActiveProtocols = await _context.Protocols.CountAsync(p => p.Status == "Active");
            PendingSamples = await _context.Samples.CountAsync(s => s.Status == "Pending");
            EnrolledPatients = await _context.Patients.CountAsync();

                var recentUsers = await _userManager.Users
                    .OrderByDescending(u => u.CreatedDate)   
                    .Take(5)                                 
                    .Select(u => new                         
                    {
                       u.FullName,
                       u.Email,
                       u.IsActive,
                       u.CreatedDate
                    })
                    .ToListAsync();

                foreach (var u in recentUsers)
                {
                    var user = await _userManager.FindByEmailAsync(u.Email);
                    var roles = await _userManager.GetRolesAsync(user!);

                    RecentUsers.Add(new UserViewModel
                    {
                        FullName = u.FullName,
                        Email = u.Email,
                        Role = roles.FirstOrDefault() ?? "No Role",
                        IsActive = u.IsActive,
                        CreatedDate = u.CreatedDate
                    });
                }

            //    foreach (var user in users.TakeLast(5))
            //    {
            //        var roles = await _userManager.GetRolesAsync(user);
            //        RecentUsers.Add(new UserViewModel
            //        {
            //            FullName = user.FullName,
            //            Email = user.Email,
            //            Role = roles.FirstOrDefault() ?? "No Role",
            //            IsActive = user.IsActive,
            //            CreatedDate = user.CreatedDate
            //        });
            //    }
        }
    }

    public class UserViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}