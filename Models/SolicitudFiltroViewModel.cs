using System.ComponentModel.DataAnnotations;

namespace ParcialVilchezCristopher_.Models;

public class SolicitudFiltroViewModel
{
    [Display(Name = "Estado")]
    public EstadoSolicitud? Estado { get; set; }

    [Display(Name = "Monto mínimo")]
    public decimal? MontoMinimo { get; set; }

    [Display(Name = "Monto máximo")]
    public decimal? MontoMaximo { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha inicio")]
    public DateTime? FechaInicio { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha fin")]
    public DateTime? FechaFin { get; set; }

    public List<SolicitudCredito> Solicitudes { get; set; } = new();
}