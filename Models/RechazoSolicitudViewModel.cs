using System.ComponentModel.DataAnnotations;

namespace ParcialVilchezCristopher_.Models;

public class RechazoSolicitudViewModel
{
    public int SolicitudId { get; set; }

    [Required(ErrorMessage = "El motivo de rechazo es obligatorio.")]
    [StringLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
    [Display(Name = "Motivo de rechazo")]
    public string MotivoRechazo { get; set; } = string.Empty;
}