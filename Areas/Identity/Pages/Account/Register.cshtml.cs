using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParcialVilchezCristopher_.Data;
using ParcialVilchezCristopher_.Models;

namespace ParcialVilchezCristopher_.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public RegisterModel(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress]
        [Display(Name = "Correo electronico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los ingresos mensuales son obligatorios.")]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Los ingresos mensuales deben ser mayores a 0.")]
        [Display(Name = "Ingresos mensuales")]
        public decimal IngresosMensuales { get; set; }

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [StringLength(100, ErrorMessage = "La contrasena debe tener entre {2} y {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contrasena")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contrasena")]
        [Compare(nameof(Password), ErrorMessage = "La contrasena y la confirmacion no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (await _context.Clientes.AnyAsync(c => c.Usuario!.Email == Input.Email))
        {
            ModelState.AddModelError(string.Empty, "Ya existe un cliente asociado a ese correo.");
            return Page();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var user = new IdentityUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        var cliente = new Cliente
        {
            UsuarioId = user.Id,
            IngresosMensuales = Input.IngresosMensuales,
            Activo = true
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(ReturnUrl);
    }
}