using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacturacionApp.Models;

namespace FacturacionApp.Controllers
{
    [Authorize]
    public class ProductoController : Controller
    {
        private readonly FacturacionContext _db;

        public ProductoController(FacturacionContext db)
        {
            _db = db;
        }

        // Todos los roles pueden ver
        public async Task<IActionResult> Index()
        {
            var productos = await _db.Productos.OrderBy(p => p.Nombre).ToListAsync();
            return View(productos);
        }

        // Admin y AuxiliarInventario pueden crear
        [Authorize(Roles = "Admin,AuxiliarInventario")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,AuxiliarInventario")]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (ModelState.IsValid)
            {
                _db.Productos.Add(producto);
                await _db.SaveChangesAsync();
                TempData["Exito"] = $"✅ Producto '{producto.Nombre}' creado.";
                return RedirectToAction("Index");
            }
            return View(producto);
        }

        // Solo Admin puede editar
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _db.Productos.FindAsync(id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (ModelState.IsValid)
            {
                _db.Update(producto);
                await _db.SaveChangesAsync();
                TempData["Exito"] = $"✅ Producto '{producto.Nombre}' actualizado.";
                return RedirectToAction("Index");
            }
            return View(producto);
        }

        // Solo Admin puede eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _db.Productos.FindAsync(id);
            if (producto != null)
            {
                bool tieneDetalles = await _db.DetallesFactura.AnyAsync(d => d.ProductoId == id);
                if (tieneDetalles)
                {
                    TempData["Error"] = "❌ No se puede eliminar: el producto está en facturas registradas.";
                    return RedirectToAction("Index");
                }

                _db.Productos.Remove(producto);
                await _db.SaveChangesAsync();
                TempData["Exito"] = "✅ Producto eliminado.";
            }
            return RedirectToAction("Index");
        }
    }
}