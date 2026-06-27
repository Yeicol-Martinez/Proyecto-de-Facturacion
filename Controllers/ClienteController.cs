using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacturacionApp.Models;

namespace FacturacionApp.Controllers
{
    [Authorize]
    public class ClienteController : Controller
    {
        private readonly FacturacionContext _db;

        public ClienteController(FacturacionContext db)
        {
            _db = db;
        }

        // Todos los roles pueden ver
        public async Task<IActionResult> Index()
        {
            var clientes = await _db.Clientes.OrderBy(c => c.Nombre).ToListAsync();
            return View(clientes);
        }

        // Solo Admin y DirectorOperaciones pueden crear
        [Authorize(Roles = "Admin,DirectorOperaciones")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,DirectorOperaciones")]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                bool existe = await _db.Clientes.AnyAsync(c => c.RncCedula == cliente.RncCedula);
                if (existe)
                {
                    ModelState.AddModelError("RncCedula", "❌ Ya existe un cliente con ese RNC/Cédula.");
                    return View(cliente);
                }

                _db.Clientes.Add(cliente);
                await _db.SaveChangesAsync();
                TempData["Exito"] = $"✅ Cliente '{cliente.Nombre}' creado exitosamente.";
                return RedirectToAction("Index");
            }
            return View(cliente);
        }

        // Solo Admin y DirectorOperaciones pueden editar
        [Authorize(Roles = "Admin,DirectorOperaciones")]
        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _db.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,DirectorOperaciones")]
        public async Task<IActionResult> Edit(int id, Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                bool existe = await _db.Clientes.AnyAsync(c => c.RncCedula == cliente.RncCedula && c.Id != id);
                if (existe)
                {
                    ModelState.AddModelError("RncCedula", "❌ Ya existe otro cliente con ese RNC/Cédula.");
                    return View(cliente);
                }

                _db.Update(cliente);
                await _db.SaveChangesAsync();
                TempData["Exito"] = $"✅ Cliente '{cliente.Nombre}' actualizado.";
                return RedirectToAction("Index");
            }
            return View(cliente);
        }

        // Solo Admin puede eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _db.Clientes.FindAsync(id);
            if (cliente != null)
            {
                bool tieneFacturas = await _db.Facturas.AnyAsync(f => f.ClienteId == id);
                if (tieneFacturas)
                {
                    TempData["Error"] = "❌ No se puede eliminar: el cliente tiene facturas registradas.";
                    return RedirectToAction("Index");
                }

                _db.Clientes.Remove(cliente);
                await _db.SaveChangesAsync();
                TempData["Exito"] = "✅ Cliente eliminado.";
            }
            return RedirectToAction("Index");
        }
    }
}