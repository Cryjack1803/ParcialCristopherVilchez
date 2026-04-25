using System.ComponentModel.DataAnnotations;

namespace ParcialVilchezCristopher_.Models;

public class SolicitudRegistroViewModel
{
    [Required(ErrorMessage = "El monto solicitado es obligatorio.")]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "El monto solicitado debe ser mayor a 0.")]
    [Display(Name = "Monto solicitado")]
    public decimal MontoSolicitado { get; set; }
}