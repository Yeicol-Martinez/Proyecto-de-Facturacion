using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FacturacionApp.Models;

namespace FacturacionApp.Controllers
{
    [Authorize]
    public class FacturaController : Controller
    {
        private readonly FacturacionContext _db;
        private readonly UserManager<Usuario> _userManager;

        public FacturaController(FacturacionContext db, UserManager<Usuario> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Todos los roles pueden ver
        public async Task<IActionResult> Index(string? estado, int? clienteId)
        {
            var query = _db.Facturas
                .Include(f => f.Cliente)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoFactura>(estado, out var estadoEnum))
                query = query.Where(f => f.Estado == estadoEnum);

            if (clienteId.HasValue)
                query = query.Where(f => f.ClienteId == clienteId.Value);

            ViewBag.Clientes = new SelectList(_db.Clientes.OrderBy(c => c.Nombre), "Id", "Nombre");
            ViewBag.EstadoFiltro = estado;
            ViewBag.ClienteIdFiltro = clienteId;

            var facturas = await query
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();

            return View(facturas);
        }

        // Admin y Empleado pueden crear facturas
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Create()
        {
            CargarListas();
            var factura = new Factura
            {
                FechaEmision = DateTime.Today,
                FechaVencimiento = DateTime.Today.AddDays(30)
            };
            return View(factura);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Empleado")]
        public async Task<IActionResult> Create(Factura factura, int[] productoIds, int[] cantidades)
        {
            ModelState.Remove("Detalles");
            ModelState.Remove("NumeroFactura");

            if (productoIds == null || productoIds.Length == 0)
                ModelState.AddModelError("", "❌ Debe agregar al menos un producto a la factura.");

            if (ModelState.IsValid)
            {
                int consecutivo = await _db.Facturas.CountAsync() + 1;
                factura.NumeroFactura = $"FAC-{consecutivo:D6}";

                decimal subtotal = 0;
                decimal totalItbis = 0;

                for (int i = 0; i < productoIds.Length; i++)
                {
                    var producto = await _db.Productos.FindAsync(productoIds[i]);
                    if (producto == null) continue;

                    int cantidad = cantidades[i];
                    decimal lineaSubtotal = cantidad * producto.PrecioUnitario;
                    decimal lineaItbis = lineaSubtotal * (producto.Itbis / 100);

                    var detalle = new DetalleFactura
                    {
                        ProductoId = producto.Id,
                        Cantidad = cantidad,
                        PrecioUnitario = producto.PrecioUnitario,
                        Itbis = producto.Itbis
                    };

                    factura.Detalles.Add(detalle);
                    subtotal += lineaSubtotal;
                    totalItbis += lineaItbis;
                }

                factura.Subtotal = subtotal;
                factura.TotalItbis = totalItbis;
                factura.Total = subtotal + totalItbis;

                _db.Facturas.Add(factura);
                await _db.SaveChangesAsync();

                TempData["Exito"] = $"✅ Factura {factura.NumeroFactura} creada exitosamente.";
                return RedirectToAction("Detalle", new { id = factura.Id });
            }

            CargarListas();
            return View(factura);
        }

        // Todos pueden ver el detalle
        public async Task<IActionResult> Detalle(int id)
        {
            var factura = await _db.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factura == null) return NotFound();
            return View(factura);
        }

        // Solo Admin puede editar una factura ya emitida
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarFactura(int id)
        {
            var factura = await _db.Facturas
                .Include(f => f.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factura == null) return NotFound();

            if (factura.Estado == EstadoFactura.Anulada)
            {
                TempData["Error"] = "❌ No se puede editar una factura anulada.";
                return RedirectToAction("Detalle", new { id });
            }

            CargarListas();
            return View(factura);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarFactura(int id, Factura factura, int[] productoIds, int[] cantidades)
        {
            var facturaDb = await _db.Facturas
                .Include(f => f.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (facturaDb == null) return NotFound();

            ModelState.Remove("Detalles");
            ModelState.Remove("NumeroFactura");

            if (productoIds == null || productoIds.Length == 0)
                ModelState.AddModelError("", "❌ Debe haber al menos un producto en la factura.");

            if (!ModelState.IsValid)
            {
                CargarListas();
                factura.Id = id;
                factura.NumeroFactura = facturaDb.NumeroFactura;
                return View(factura);
            }

            // ---- Snapshot ANTES de modificar (para la auditoría) ----
            var clienteAntes = await _db.Clientes.FindAsync(facturaDb.ClienteId);
            string clienteAntesNombre = clienteAntes?.Nombre ?? "(desconocido)";
            DateTime fechaEmisionAntes = facturaDb.FechaEmision;
            DateTime? fechaVencimientoAntes = facturaDb.FechaVencimiento;
            string observacionesAntes = facturaDb.Observaciones ?? "";
            decimal totalAntes = facturaDb.Total;
            string detalleAntes = string.Join(" | ", facturaDb.Detalles.Select(d =>
                $"{d.Cantidad}x {d.Producto?.Nombre} @ RD${d.PrecioUnitario:N2}"));

            // ---- Aplicar cambios de cabecera ----
            facturaDb.ClienteId = factura.ClienteId;
            facturaDb.FechaEmision = factura.FechaEmision;
            facturaDb.FechaVencimiento = factura.FechaVencimiento;
            facturaDb.Observaciones = factura.Observaciones;

            // ---- Reconstruir el detalle de productos ----
            _db.DetallesFactura.RemoveRange(facturaDb.Detalles);
            facturaDb.Detalles.Clear();

            decimal subtotal = 0;
            decimal totalItbis = 0;
            var nombresDespues = new List<string>();

            for (int i = 0; i < productoIds.Length; i++)
            {
                var producto = await _db.Productos.FindAsync(productoIds[i]);
                if (producto == null) continue;

                int cantidad = cantidades[i];
                decimal lineaSubtotal = cantidad * producto.PrecioUnitario;
                decimal lineaItbis = lineaSubtotal * (producto.Itbis / 100);

                facturaDb.Detalles.Add(new DetalleFactura
                {
                    ProductoId = producto.Id,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.PrecioUnitario,
                    Itbis = producto.Itbis
                });

                subtotal += lineaSubtotal;
                totalItbis += lineaItbis;
                nombresDespues.Add($"{cantidad}x {producto.Nombre} @ RD${producto.PrecioUnitario:N2}");
            }

            facturaDb.Subtotal = subtotal;
            facturaDb.TotalItbis = totalItbis;
            facturaDb.Total = subtotal + totalItbis;

            await _db.SaveChangesAsync();

            // ---- Registrar en auditoría solo lo que realmente cambió ----
            var usuario = await _userManager.GetUserAsync(User);
            var clienteDespues = await _db.Clientes.FindAsync(facturaDb.ClienteId);
            string detalleDespues = string.Join(" | ", nombresDespues);

            var cambios = new List<AuditoriaFactura>();

            void RegistrarSiCambio(string campo, string antes, string despues)
            {
                if (antes != despues)
                {
                    cambios.Add(new AuditoriaFactura
                    {
                        FacturaId = facturaDb.Id,
                        UsuarioId = usuario!.Id,
                        CampoModificado = campo,
                        ValorAnterior = antes,
                        ValorNuevo = despues
                    });
                }
            }

            RegistrarSiCambio("Cliente", clienteAntesNombre, clienteDespues?.Nombre ?? "(desconocido)");
            RegistrarSiCambio("Fecha de Emisión", fechaEmisionAntes.ToString("dd/MM/yyyy"), facturaDb.FechaEmision.ToString("dd/MM/yyyy"));
            RegistrarSiCambio("Fecha de Vencimiento", fechaVencimientoAntes?.ToString("dd/MM/yyyy") ?? "(sin fecha)", facturaDb.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "(sin fecha)");
            RegistrarSiCambio("Observaciones", observacionesAntes, facturaDb.Observaciones ?? "");
            RegistrarSiCambio("Detalle de Productos", detalleAntes, detalleDespues);
            RegistrarSiCambio("Total", $"RD$ {totalAntes:N2}", $"RD$ {facturaDb.Total:N2}");

            if (cambios.Any())
            {
                _db.AuditoriasFactura.AddRange(cambios);
                await _db.SaveChangesAsync();
            }

            TempData["Exito"] = $"✅ Factura {facturaDb.NumeroFactura} actualizada. Se registraron {cambios.Count} cambio(s) en la auditoría.";
            return RedirectToAction("Detalle", new { id = facturaDb.Id });
        }

        // Solo Admin puede cambiar estado
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CambiarEstado(int id, EstadoFactura nuevoEstado)
        {
            var factura = await _db.Facturas.FindAsync(id);
            if (factura == null) return NotFound();

            if (factura.Estado == EstadoFactura.Anulada)
            {
                TempData["Error"] = "❌ No se puede modificar una factura anulada.";
                return RedirectToAction("Detalle", new { id });
            }

            factura.Estado = nuevoEstado;
            await _db.SaveChangesAsync();

            TempData["Exito"] = $"✅ Factura {factura.NumeroFactura} marcada como {nuevoEstado}.";
            return RedirectToAction("Detalle", new { id });
        }

        // Solo Admin puede eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var factura = await _db.Facturas
                .Include(f => f.Detalles)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factura != null)
            {
                if (factura.Estado == EstadoFactura.Pagada)
                {
                    TempData["Error"] = "❌ No se puede eliminar una factura pagada.";
                    return RedirectToAction("Index");
                }

                _db.DetallesFactura.RemoveRange(factura.Detalles);
                _db.Facturas.Remove(factura);
                await _db.SaveChangesAsync();
                TempData["Exito"] = "✅ Factura eliminada.";
            }
            return RedirectToAction("Index");
        }

        // Todos pueden ver el reporte
        public async Task<IActionResult> Reporte()
        {
            var facturas = await _db.Facturas
                .Include(f => f.Cliente)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();

            return View(facturas);
        }

        // AJAX — todos pueden consultar productos
        [HttpGet]
        public async Task<IActionResult> ObtenerProducto(int id)
        {
            var producto = await _db.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            return Json(new
            {
                id = producto.Id,
                nombre = producto.Nombre,
                precioUnitario = producto.PrecioUnitario,
                itbis = producto.Itbis
            });
        }

        private void CargarListas()
        {
            ViewBag.ClienteId = new SelectList(_db.Clientes.OrderBy(c => c.Nombre), "Id", "Nombre");
            ViewBag.Productos = _db.Productos.OrderBy(p => p.Nombre).ToList();
        }
    }
}