using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcialVilchezCristopher_.Data;
using ParcialVilchezCristopher_.Models;
using ParcialVilchezCristopher_.Services;

namespace ParcialVilchezCristopher_.Controllers;

[Authorize]
public class SolicitudesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly SolicitudesCacheService _solicitudesCache;
    private readonly UserManager<IdentityUser> _userManager;

    public SolicitudesController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        SolicitudesCacheService solicitudesCache)
    {
        _context = context;
        _userManager = userManager;
        _solicitudesCache = solicitudesCache;
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

        var solicitudes = await _solicitudesCache.GetSolicitudesAsync(usuarioId);
        if (solicitudes is null)
        {
            solicitudes = await _context.SolicitudesCredito
                .AsNoTracking()
                .Where(s => s.ClienteId == cliente.Id)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            await _solicitudesCache.SetSolicitudesAsync(usuarioId, solicitudes);
        }

        IEnumerable<SolicitudCredito> query = solicitudes;

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

        filtro.Solicitudes = query
            .OrderByDescending(s => s.FechaSolicitud)
            .ToList();

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

        HttpContext.Session.SetInt32("UltimaSolicitudId", solicitud.Id);
        HttpContext.Session.SetString("UltimaSolicitudMonto", solicitud.MontoSolicitado.ToString("F2"));

        return View(solicitud);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var usuarioId = _userManager.GetUserId(User);
        if (usuarioId is null)
        {
            return Challenge();
        }

        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (cliente is null)
        {
            ModelState.AddModelError(string.Empty, "No existe un cliente asociado al usuario autenticado.");
        }
        else if (!cliente.Activo)
        {
            ModelState.AddModelError(string.Empty, "El cliente no se encuentra activo y no puede registrar solicitudes.");
        }
        else if (await _context.SolicitudesCredito.AnyAsync(s => s.ClienteId == cliente.Id && s.Estado == EstadoSolicitud.Pendiente))
        {
            ModelState.AddModelError(string.Empty, "Ya existe una solicitud pendiente para este cliente.");
        }

        return View(new SolicitudRegistroViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SolicitudRegistroViewModel model)
    {
        var usuarioId = _userManager.GetUserId(User);
        if (usuarioId is null)
        {
            return Challenge();
        }

        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (cliente is null)
        {
            ModelState.AddModelError(string.Empty, "No existe un cliente asociado al usuario autenticado.");
            return View(model);
        }

        if (!cliente.Activo)
        {
            ModelState.AddModelError(string.Empty, "El cliente no se encuentra activo y no puede registrar solicitudes.");
        }

        if (await _context.SolicitudesCredito.AnyAsync(s => s.ClienteId == cliente.Id && s.Estado == EstadoSolicitud.Pendiente))
        {
            ModelState.AddModelError(string.Empty, "No se permite registrar mas de una solicitud pendiente por cliente.");
        }

        var limite = cliente.IngresosMensuales * 10;
        if (model.MontoSolicitado > limite)
        {
            ModelState.AddModelError(nameof(model.MontoSolicitado), $"El monto solicitado no puede superar 10 veces los ingresos mensuales del cliente ({limite:C}).");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var solicitud = new SolicitudCredito
        {
            ClienteId = cliente.Id,
            MontoSolicitado = model.MontoSolicitado,
            FechaSolicitud = DateTime.UtcNow,
            Estado = EstadoSolicitud.Pendiente
        };

        _context.SolicitudesCredito.Add(solicitud);
        await _context.SaveChangesAsync();
        await _solicitudesCache.InvalidateSolicitudesAsync(usuarioId);

        ViewBag.SuccessMessage = "La solicitud fue registrada correctamente en estado Pendiente.";
        ModelState.Clear();
        return View(new SolicitudRegistroViewModel());
    }
}