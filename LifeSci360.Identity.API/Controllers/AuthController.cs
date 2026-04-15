using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using LifeSci360.Identity.API.Data;
using LifeSci360.Identity.API.Models;
using LifeSci360.Shared.DTOs;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly AppIdentityDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<User> userManager,
        RoleManager<ApplicationRole> roleManager,
        AppIdentityDbContext context,
        IConfiguration config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null || !user.IsActive)
            return Unauthorized(new { Message = "Invalid credentials." });

        if (await _userManager.IsLockedOutAsync(user))
            return Unauthorized(new { Message = "Account locked. Try again later." });

        if (!await _userManager.CheckPasswordAsync(user, model.Password))
        {
            // Log failed attempt
            _context.AuditLogs.Add(new AuditLog
            {
                UserID = user.Id,
                Action = "Failed Login Attempt",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Unauthorized(new { Message = "Invalid credentials." });
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Log successful login
        _context.AuditLogs.Add(new AuditLog
        {
            UserID = user.Id,
            Action = "User Logged In",
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Role = roles.FirstOrDefault(),
            FullName = user.FullName
        });
    }

}