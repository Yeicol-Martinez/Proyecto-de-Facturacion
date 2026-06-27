using FacturacionApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionApp.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

        public CuentaController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> ConfigurarAdmin()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count > 0)
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfigurarAdmin(string nombreCompleto, string email, string password, string confirmarPassword)
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count > 0)
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(nombreCompleto) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "❌ Todos los campos son requeridos.");
                return View();
            }

            if (password != confirmarPassword)
            {
                ModelState.AddModelError("", "❌ Las contraseñas no coinciden.");
                return View();
            }

            if (password.Length < 6)
            {
                ModelState.AddModelError("", "❌ La contraseña debe tener al menos 6 caracteres.");
                return View();
            }

            var admin = new Usuario
            {
                UserName = email,
                Email = email,
                NombreCompleto = nombreCompleto,
                EmailConfirmed = true
            };

            var resultado = await _userManager.CreateAsync(admin, password);

            if (resultado.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
                TempData["Exito"] = "✅ Cuenta de Administrador creada. Ya puedes iniciar sesión.";
                return RedirectToAction("Login");
            }

            foreach (var error in resultado.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }

        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var resultado = await _signInManager.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "❌ Correo o contraseña incorrectos.");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}