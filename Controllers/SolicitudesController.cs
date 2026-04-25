using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcialVilchezCristopher_.Data;
using ParcialVilchezCristopher_.Models;

namespace ParcialVilchezCristopher_.Controllers;

[Authorize]
public class SolicitudesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public SolicitudesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(SolicitudFiltroViewModel filtro)
    {
        if (filtro.MontoMinimo.HasValue && filtro.MontoMinimo.Value < 0)
        {
            ModelState.AddModelError(nameof(filtro.MontoMinimo), "El monto mínimo no puede ser negativo.");
        }

        if (filtro.MontoMaximo.HasValue && filtro.MontoMaximo.Value < 0)
        {
            ModelState.AddModelError(nameof(filtro.MontoMaximo), "El monto máximo no puede ser negativo.");
        }

        if (filtro.FechaInicio.HasValue && filtro.FechaFin.HasValue && filtro.FechaInicio.Value.Date > filtro.FechaFin.Value.Date)
        {
            ModelState.AddModelError(nameof(filtro.FechaFin), "La fecha inicio no puede ser mayor que la fecha fin.");
        }

        var usuarioId = _userManager.GetUserId(User);
        if (usuarioId is null)
        {
            return Challenge();
        }

        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (cliente is null)
        {
            filtro.Solicitudes = new List<SolicitudCredito>();
            return View(filtro);
        }

        var query = _context.SolicitudesCredito
            .AsNoTracking()
            .Include(s => s.Cliente)
            .Where(s => s.ClienteId == cliente.Id);

        if (ModelState.IsValid)
        {
            if (filtro.Estado.HasValue)
            {
                query = query.Where(s => s.Estado == filtro.Estado.Value);
            }

            if (filtro.MontoMinimo.HasValue)
            {
                query = query.Where(s => s.MontoSolicitado >= filtro.MontoMinimo.Value);
            }

            if (filtro.MontoMaximo.HasValue)
            {
                query = query.Where(s => s.MontoSolicitado <= filtro.MontoMaximo.Value);
            }

            if (filtro.FechaInicio.HasValue)
            {
                var inicio = filtro.FechaInicio.Value.Date;
                query = query.Where(s => s.FechaSolicitud >= inicio);
            }

            if (filtro.FechaFin.HasValue)
            {
                var finExclusivo = filtro.FechaFin.Value.Date.AddDays(1);
                query = query.Where(s => s.FechaSolicitud < finExclusivo);
            }
        }

        filtro.Solicitudes = await query
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync();

        return View(filtro);
    }

    public async Task<IActionResult> Details(int id)
    {
        var usuarioId = _userManager.GetUserId(User);
        if (usuarioId is null)
        {
            return Challenge();
        }

        var solicitud = await _context.SolicitudesCredito
            .AsNoTracking()
            .Include(s => s.Cliente)
            .ThenInclude(c => c!.Usuario)
            .FirstOrDefaultAsync(s => s.Id == id && s.Cliente!.UsuarioId == usuarioId);

        if (solicitud is null)
        {
            return NotFound();
        }

        return View(solicitud);
    }
}