using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ParcialVilchezCristopher_.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Los ingresos mensuales deben ser mayores a 0.")]
    public decimal IngresosMensuales { get; set; }

    public bool Activo { get; set; }

    public IdentityUser? Usuario { get; set; }
    public ICollection<SolicitudCredito> SolicitudesCredito { get; set; } = new List<SolicitudCredito>();
}
