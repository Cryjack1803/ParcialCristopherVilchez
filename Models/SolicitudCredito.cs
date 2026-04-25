using System.ComponentModel.DataAnnotations;

namespace ParcialVilchezCristopher_.Models;

public enum EstadoSolicitud
{
    Pendiente = 0,
    Aprobado = 1,
    Rechazado = 2
}

public class SolicitudCredito
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "El monto solicitado debe ser mayor a 0.")]
    public decimal MontoSolicitado { get; set; }

    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    [StringLength(500)]
    public string? MotivoRechazo { get; set; }

    public Cliente? Cliente { get; set; }

    public bool PuedeSerAprobada(decimal ingresosMensuales)
    {
        return MontoSolicitado <= ingresosMensuales * 5;
    }
}
