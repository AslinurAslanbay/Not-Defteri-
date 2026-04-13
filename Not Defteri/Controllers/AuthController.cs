using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotDefteriMvc.Data;
using NotDefteriMvc.Models;
using NotDefteriMvc.Services;
using NotDefteriMvc.ViewModels;

namespace NotDefteriMvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Notes");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userName = model.UserName.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null || !PasswordHasher.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Kullanici adi veya sifre hatali.");
                return View(model);
            }

            await SignInAsync(user);
            TempData["SuccessMessage"] = $"Hos geldiniz. Kullanici numaraniz: {user.Id}";

            if (HttpContext.Session.GetString(NotesController.DraftSessionKey) != null)
            {
                return RedirectToAction("Create", "Notes");
            }

            return RedirectToAction("Index", "Notes");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Notes");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userName = model.UserName.Trim();
            var existingUser = await _context.Users.AnyAsync(u => u.UserName == userName);
            if (existingUser)
            {
                ModelState.AddModelError(nameof(model.UserName), "Bu kullanici adi zaten kullaniliyor.");
                return View(model);
            }

            var user = new User
            {
                UserName = userName,
                PasswordHash = PasswordHasher.Hash(model.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await SignInAsync(user);
            TempData["SuccessMessage"] = $"Kayit tamamlandi. Kullanici numaraniz: {user.Id}";

            if (HttpContext.Session.GetString(NotesController.DraftSessionKey) != null)
            {
                return RedirectToAction("Create", "Notes");
            }

            return RedirectToAction("Index", "Notes");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Notes");
            }

            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userName = model.UserName.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                ModelState.AddModelError(nameof(model.UserName), "Bu kullanici adi bulunamadi.");
                return View(model);
            }

            user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["AuthMessage"] = "Sifreniz basariyla guncellendi. Yeni sifrenizle giris yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        private async Task SignInAsync(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true
                });
        }
    }
}
