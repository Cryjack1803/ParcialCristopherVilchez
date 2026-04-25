using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcialVilchezCristopher_.Data;
using ParcialVilchezCristopher_.Models;
using ParcialVilchezCristopher_.Services;

namespace ParcialVilchezCristopher_.Controllers;

[Authorize(Roles = "Analista")]
public class AnalistaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly SolicitudesCacheService _solicitudesCache;

    public AnalistaController(ApplicationDbContext context, SolicitudesCacheService solicitudesCache)
    {
        _context = context;
        _solicitudesCache = solicitudesCache;
    }

    public async Task<IActionResult> Index()
    {
        var solicitudes = await _context.SolicitudesCredito
            .AsNoTracking()
            .Include(s => s.Cliente)
            .ThenInclude(c => c!.Usuario)
            .Where(s => s.Estado == EstadoSolicitud.Pendiente)
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync();

        return View(solicitudes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id)
    {
        var solicitud = await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .ThenInclude(c => c!.Usuario)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud is null)
        {
            return NotFound();
        }

        if (solicitud.Estado != EstadoSolicitud.Pendiente)
        {
            TempData["Error"] = "No se puede procesar una solicitud que ya fue aprobada o rechazada.";
            return RedirectToAction(nameof(Index));
        }

        var ingresosMensuales = solicitud.Cliente?.IngresosMensuales ?? 0m;
        if (solicitud.MontoSolicitado > ingresosMensuales * 5)
        {
            TempData["Error"] = "No se puede aprobar la solicitud porque el monto excede 5 veces los ingresos mensuales del cliente.";
            return RedirectToAction(nameof(Index));
        }

        solicitud.Estado = EstadoSolicitud.Aprobado;
        solicitud.MotivoRechazo = null;
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(solicitud.Cliente?.UsuarioId))
        {
            await _solicitudesCache.InvalidateSolicitudesAsync(solicitud.Cliente.UsuarioId);
        }

        TempData["Success"] = "Solicitud aprobada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Rechazar(int id)
    {
        var solicitud = await _context.SolicitudesCredito
            .AsNoTracking()
            .Include(s => s.Cliente)
            .ThenInclude(c => c!.Usuario)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud is null)
        {
            return NotFound();
        }

        if (solicitud.Estado != EstadoSolicitud.Pendiente)
        {
            TempData["Error"] = "No se puede procesar una solicitud que ya fue aprobada o rechazada.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Solicitud = solicitud;
        return View(new RechazoSolicitudViewModel { SolicitudId = solicitud.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(RechazoSolicitudViewModel model)
    {
        var solicitud = await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .ThenInclude(c => c!.Usuario)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId);

        if (solicitud is null)
        {
            return NotFound();
        }

        if (solicitud.Estado != EstadoSolicitud.Pendiente)
        {
            TempData["Error"] = "No se puede procesar una solicitud que ya fue aprobada o rechazada.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Solicitud = solicitud;
            return View(model);
        }

        solicitud.Estado = EstadoSolicitud.Rechazado;
        solicitud.MotivoRechazo = model.MotivoRechazo;
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(solicitud.Cliente?.UsuarioId))
        {
            await _solicitudesCache.InvalidateSolicitudesAsync(solicitud.Cliente.UsuarioId);
        }

        TempData["Success"] = "Solicitud rechazada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}