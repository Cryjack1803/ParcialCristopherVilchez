using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParcialVilchezCristopher_.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public LoginModel(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El correo o usuario es obligatorio.")]
        [Display(Name = "Correo o usuario")]
        public string Identifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contrasena")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            Response.Redirect(returnUrl ?? Url.Content("~/"));
            return;
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        var normalizedIdentifier = Input.Identifier.Trim();
        var user = await _userManager.FindByEmailAsync(normalizedIdentifier)
            ?? await _userManager.FindByNameAsync(normalizedIdentifier);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Intento de inicio de sesion no valido.");
            ReturnUrl = returnUrl;
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("Usuario autenticado.");
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Cuenta bloqueada.");
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "Intento de inicio de sesion no valido.");
        ReturnUrl = returnUrl;
        return Page();
    }
}