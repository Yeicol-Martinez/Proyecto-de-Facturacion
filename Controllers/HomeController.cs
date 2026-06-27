using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacturacionApp.Models;
using System.Diagnostics;

namespace FacturacionApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly FacturacionContext _db;

        public HomeController(FacturacionContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalFacturas = await _db.Facturas.CountAsync();
            ViewBag.TotalClientes = await _db.Clientes.CountAsync();
            ViewBag.TotalProductos = await _db.Productos.CountAsync();
            ViewBag.TotalPendiente = await _db.Facturas
                .Where(f => f.Estado == EstadoFactura.Pendiente)
                .SumAsync(f => (decimal?)f.Total) ?? 0;
            ViewBag.TotalCobrado = await _db.Facturas
                .Where(f => f.Estado == EstadoFactura.Pagada)
                .SumAsync(f => (decimal?)f.Total) ?? 0;

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}