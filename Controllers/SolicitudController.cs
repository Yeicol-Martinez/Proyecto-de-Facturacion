using FacturacionApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacturacionApp.Controllers
{
    [Authorize]
    public class SolicitudController : Controller
    {
        private readonly FacturacionContext _db;
        private readonly UserManager<Usuario> _userManager;

        public SolicitudController(FacturacionContext db, UserManager<Usuario> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ===================== SOLICITAR (Empleado / Auxiliar) =====================

        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> SolicitarCliente(int id)
        {
            var cliente = await _db.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            ViewBag.DatosActuales = $"Nombre: {cliente.Nombre} | RNC/Cédula: {cliente.RncCedula} | " +
                                     $"Email: {cliente.Email} | Dirección: {cliente.Direccion} | Teléfono: {cliente.Telefono}";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> SolicitarCliente(int id, string cambioPropuesto)
        {
            var cliente = await _db.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            string datosActuales = $"Nombre: {cliente.Nombre} | RNC/Cédula: {cliente.RncCedula} | " +
                                    $"Email: {cliente.Email} | Dirección: {cliente.Direccion} | Teléfono: {cliente.Telefono}";

            if (string.IsNullOrWhiteSpace(cambioPropuesto))
            {
                ModelState.AddModelError("", "❌ Debes describir el cambio que necesitas.");
                ViewBag.DatosActuales = datosActuales;
                return View();
            }

            var usuario = await _userManager.GetUserAsync(User);
            _db.Solicitudes.Add(new Solicitud
            {
                TipoEntidad = TipoEntidadSolicitud.Cliente,
                EntidadId = cliente.Id,
                DatosActuales = datosActuales,
                CambioPropuesto = cambioPropuesto,
                SolicitanteId = usuario!.Id
            });
            await _db.SaveChangesAsync();

            TempData["Exito"] = "✅ Solicitud enviada al Director de Operaciones.";
            return RedirectToAction("Index", "Cliente");
        }

        [Authorize(Roles = "AuxiliarInventario")]
        public async Task<IActionResult> SolicitarProducto(int id)
        {
            var producto = await _db.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            ViewBag.DatosActuales = $"Nombre: {producto.Nombre} | Descripción: {producto.Descripcion} | " +
                                     $"Precio: RD$ {producto.PrecioUnitario:N2} | ITBIS: {producto.Itbis}% | Stock: {producto.Stock}";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AuxiliarInventario")]
        public async Task<IActionResult> SolicitarProducto(int id, string cambioPropuesto)
        {
            var producto = await _db.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            string datosActuales = $"Nombre: {producto.Nombre} | Descripción: {producto.Descripcion} | " +
                                    $"Precio: RD$ {producto.PrecioUnitario:N2} | ITBIS: {producto.Itbis}% | Stock: {producto.Stock}";

            if (string.IsNullOrWhiteSpace(cambioPropuesto))
            {
                ModelState.AddModelError("", "❌ Debes describir el cambio que necesitas.");
                ViewBag.DatosActuales = datosActuales;
                return View();
            }

            var usuario = await _userManager.GetUserAsync(User);
            _db.Solicitudes.Add(new Solicitud
            {
                TipoEntidad = TipoEntidadSolicitud.Producto,
                EntidadId = producto.Id,
                DatosActuales = datosActuales,
                CambioPropuesto = cambioPropuesto,
                SolicitanteId = usuario!.Id
            });
            await _db.SaveChangesAsync();

            TempData["Exito"] = "✅ Solicitud enviada al Administrador.";
            return RedirectToAction("Index", "Producto");
        }

        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> SolicitarFactura(int id)
        {
            var factura = await _db.Facturas.Include(f => f.Cliente).FirstOrDefaultAsync(f => f.Id == id);
            if (factura == null) return NotFound();

            ViewBag.DatosActuales = $"No. Factura: {factura.NumeroFactura} | Cliente: {factura.Cliente?.Nombre} | " +
                                     $"Fecha: {factura.FechaEmision:dd/MM/yyyy} | Total: RD$ {factura.Total:N2} | Estado: {factura.Estado}";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> SolicitarFactura(int id, string cambioPropuesto)
        {
            var factura = await _db.Facturas.Include(f => f.Cliente).FirstOrDefaultAsync(f => f.Id == id);
            if (factura == null) return NotFound();

            string datosActuales = $"No. Factura: {factura.NumeroFactura} | Cliente: {factura.Cliente?.Nombre} | " +
                                    $"Fecha: {factura.FechaEmision:dd/MM/yyyy} | Total: RD$ {factura.Total:N2} | Estado: {factura.Estado}";

            if (string.IsNullOrWhiteSpace(cambioPropuesto))
            {
                ModelState.AddModelError("", "❌ Debes describir el cambio que necesitas.");
                ViewBag.DatosActuales = datosActuales;
                return View();
            }

            var usuario = await _userManager.GetUserAsync(User);
            _db.Solicitudes.Add(new Solicitud
            {
                TipoEntidad = TipoEntidadSolicitud.Factura,
                EntidadId = factura.Id,
                DatosActuales = datosActuales,
                CambioPropuesto = cambioPropuesto,
                SolicitanteId = usuario!.Id
            });
            await _db.SaveChangesAsync();

            TempData["Exito"] = "✅ Solicitud enviada al Administrador.";
            return RedirectToAction("Detalle", "Factura", new { id = factura.Id });
        }

        // ===================== BANDEJAS (Director / Admin) =====================

        [Authorize(Roles = "DirectorOperaciones,Admin")]
        public async Task<IActionResult> BandejaClientes()
        {
            var solicitudes = await _db.Solicitudes
                .Include(s => s.Solicitante)
                .Where(s => s.TipoEntidad == TipoEntidadSolicitud.Cliente)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
            return View(solicitudes);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BandejaAdmin()
        {
            var solicitudes = await _db.Solicitudes
                .Include(s => s.Solicitante)
                .Where(s => s.TipoEntidad == TipoEntidadSolicitud.Producto || s.TipoEntidad == TipoEntidadSolicitud.Factura)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
            return View(solicitudes);
        }

        public async Task<IActionResult> MisSolicitudes()
        {
            var usuario = await _userManager.GetUserAsync(User);
            var solicitudes = await _db.Solicitudes
                .Include(s => s.RevisadoPor)
                .Where(s => s.SolicitanteId == usuario!.Id)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
            return View(solicitudes);
        }

        // ===================== APROBAR / RECHAZAR =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id, string? comentario)
        {
            var solicitud = await _db.Solicitudes.FindAsync(id);
            if (solicitud == null) return NotFound();
            if (!PuedeRevisar(solicitud)) return Forbid();

            var usuario = await _userManager.GetUserAsync(User);
            solicitud.Estado = EstadoSolicitud.Aprobada;
            solicitud.RevisadoPorId = usuario!.Id;
            solicitud.FechaRevision = DateTime.Now;
            solicitud.ComentarioRevision = comentario;
            await _db.SaveChangesAsync();

            TempData["Exito"] = "✅ Solicitud aprobada. Aplica el cambio desde la pantalla de edición correspondiente.";
            return RedirectToAction(VolverABandeja(solicitud.TipoEntidad));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, string comentario)
        {
            var solicitud = await _db.Solicitudes.FindAsync(id);
            if (solicitud == null) return NotFound();
            if (!PuedeRevisar(solicitud)) return Forbid();

            if (string.IsNullOrWhiteSpace(comentario))
            {
                TempData["Error"] = "❌ Debes explicar el motivo del rechazo.";
                return RedirectToAction(VolverABandeja(solicitud.TipoEntidad));
            }

            var usuario = await _userManager.GetUserAsync(User);
            solicitud.Estado = EstadoSolicitud.Rechazada;
            solicitud.RevisadoPorId = usuario!.Id;
            solicitud.FechaRevision = DateTime.Now;
            solicitud.ComentarioRevision = comentario;
            await _db.SaveChangesAsync();

            TempData["Exito"] = "✅ Solicitud rechazada.";
            return RedirectToAction(VolverABandeja(solicitud.TipoEntidad));
        }

        private bool PuedeRevisar(Solicitud solicitud)
        {
            if (solicitud.TipoEntidad == TipoEntidadSolicitud.Cliente)
                return User.IsInRole("DirectorOperaciones") || User.IsInRole("Admin");
            return User.IsInRole("Admin");
        }

        private string VolverABandeja(TipoEntidadSolicitud tipo) =>
            tipo == TipoEntidadSolicitud.Cliente ? "BandejaClientes" : "BandejaAdmin";
    }
}