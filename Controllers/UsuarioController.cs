using FacturacionApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FacturacionApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsuarioController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuarioController(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Usuario
        public async Task<IActionResult> Index()
        {
            var usuarios = _userManager.Users.ToList();
            var modelo = new List<UsuarioViewModel>();

            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                modelo.Add(new UsuarioViewModel
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Email = u.Email ?? "",
                    Rol = roles.FirstOrDefault() ?? "Sin rol"
                });
            }

            return View(modelo);
        }

        // GET: /Usuario/Crear
        public IActionResult Crear()
        {
            CargarRoles();
            return View();
        }

        // POST: /Usuario/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(string nombreCompleto, string email, string password, string confirmarPassword, string rol)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(rol))
            {
                ModelState.AddModelError("", "❌ Todos los campos son requeridos.");
                CargarRoles();
                return View();
            }

            if (password != confirmarPassword)
            {
                ModelState.AddModelError("", "❌ Las contraseñas no coinciden.");
                CargarRoles();
                return View();
            }

            var usuarioExistente = await _userManager.FindByEmailAsync(email);
            if (usuarioExistente != null)
            {
                ModelState.AddModelError("", "❌ Ya existe un usuario con ese correo.");
                CargarRoles();
                return View();
            }

            var usuario = new Usuario
            {
                UserName = email,
                Email = email,
                NombreCompleto = nombreCompleto,
                EmailConfirmed = true
            };

            var resultado = await _userManager.CreateAsync(usuario, password);

            if (resultado.Succeeded)
            {
                await _userManager.AddToRoleAsync(usuario, rol);
                TempData["Exito"] = $"✅ Usuario {nombreCompleto} creado con rol {rol}.";
                return RedirectToAction("Index");
            }

            foreach (var error in resultado.Errors)
                ModelState.AddModelError("", error.Description);

            CargarRoles();
            return View();
        }

        // GET: /Usuario/Editar/id
        public async Task<IActionResult> Editar(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(usuario);
            CargarRoles(roles.FirstOrDefault());

            var modelo = new UsuarioViewModel
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email ?? "",
                Rol = roles.FirstOrDefault() ?? ""
            };

            return View(modelo);
        }

        // POST: /Usuario/Editar/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(string id, string nombreCompleto, string rol)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            usuario.NombreCompleto = nombreCompleto;
            await _userManager.UpdateAsync(usuario);

            // Actualizar rol
            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, rol);

            TempData["Exito"] = $"✅ Usuario actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // POST: /Usuario/Eliminar/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            // No permitir eliminar el propio Admin
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual?.Id == id)
            {
                TempData["Error"] = "❌ No puedes eliminar tu propia cuenta.";
                return RedirectToAction("Index");
            }

            await _userManager.DeleteAsync(usuario);
            TempData["Exito"] = "✅ Usuario eliminado correctamente.";
            return RedirectToAction("Index");
        }

        private void CargarRoles(string? rolSeleccionado = null)
        {
            var roles = _roleManager.Roles.ToList();

            var lista = roles.Select(r =>
            {
                string texto = r.Name switch
                {
                    "Admin" => "👑 Administrador",
                    "DirectorOperaciones" => "👔 Director de Operaciones",
                    "AuxiliarInventario" => "📦 Auxiliar de Inventario",
                    "Empleado" => "👤 Empleado",
                    _ => r.Name ?? ""
                };

                return new SelectListItem
                {
                    Value = r.Name,
                    Text = texto,
                    Selected = r.Name == rolSeleccionado
                };
            }).ToList();

            ViewBag.Roles = lista;
        }
    }
}