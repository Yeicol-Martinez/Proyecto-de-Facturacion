using FacturacionApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacturacionApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditoriaController : Controller
    {
        private readonly FacturacionContext _db;

        public AuditoriaController(FacturacionContext db)
        {
            _db = db;
        }

        // GET: /Auditoria  -> historial completo
        public async Task<IActionResult> Index()
        {
            var auditorias = await _db.AuditoriasFactura
                .Include(a => a.Usuario)
                .Include(a => a.Factura)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            return View(auditorias);
        }

        // GET: /Auditoria/Factura/5 -> historial de una factura específica
        public async Task<IActionResult> Factura(int id)
        {
            var auditorias = await _db.AuditoriasFactura
                .Include(a => a.Usuario)
                .Where(a => a.FacturaId == id)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            ViewBag.FacturaId = id;
            var factura = await _db.Facturas.FindAsync(id);
            ViewBag.NumeroFactura = factura?.NumeroFactura;

            return View(auditorias);
        }
    }
}