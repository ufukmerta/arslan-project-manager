//using ArslanProjectManager.WEB.Data;
//using ArslanProjectManager.WEB.Models;
using ArslanProjectManager.WEB.Services;
using ArslanProjectManager.WEB.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ArslanProjectManager.WEB.Controllers
{
    public class UserController : Controller
    {
        /*private readonly ProjectManagerDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;
        public UserController(ProjectManagerDbContext dbContext, IPasswordHasher passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }
        
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string? email = User.Identity?.Name;
            var user = await _dbContext.Users
                .Include(x => x.TeamUsers)
                .ThenInclude(tu => tu.Team)
                .ThenInclude(t => t.Projects)
                .ThenInclude(p => p.ProjectTasks)
                .Where(x => x.Email == email)
                .Select(x => new UserProfileViewModel
                {
                    Name = x.Name,
                    Surname = x.Surname,
                    Email = x.Email,
                    RegisterDate = x.RegisterDate,
                    CurrentTeam = x.TeamUsers.FirstOrDefault().Team.TeamName,
                    Role = x.Teams.Any() ? "Team Manager" : "Team Member",
                    TotalProjects = x.TeamUsers.SelectMany(tu => tu.Team.Projects).Count(),
                    CompletedProjects = x.TeamUsers.SelectMany(tu => tu.Team.Projects)
                        .Count(p => p.ProjectTasks.All(t => t.Board.BoardName == "Done")),
                    TotalTasks = x.TeamUsers.SelectMany(tu => tu.Team.Projects)
                        .SelectMany(p => p.ProjectTasks)
                        .Count(t => t.AppointeeId == x.TeamUsers.FirstOrDefault().TeamUserId),
                    CompletedTasks = x.TeamUsers.SelectMany(tu => tu.Team.Projects)
                        .SelectMany(p => p.ProjectTasks)
                        .Count(t => t.AppointeeId == x.TeamUsers.FirstOrDefault().TeamUserId && t.Board.BoardName == "Done")
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? ReturnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = ReturnUrl };
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !_passwordHasher.VerifyPassword(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "User");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            var user = new User
            {
                Name = model.Name,
                Surname = model.Surname,
                Email = model.Email,
                Password = _passwordHasher.HashPassword(model.Password),
                RegisterDate = DateTime.UtcNow
            };

            try
            {
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error occurred while registering. Please try again.");
                return View(model);
            }
    }*/
    }
}
