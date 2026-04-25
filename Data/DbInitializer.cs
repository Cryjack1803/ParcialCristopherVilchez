using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ParcialVilchezCristopher_.Models;

namespace ParcialVilchezCristopher_.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync("Analista"))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole("Analista"));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        var analista = await EnsureUserAsync(
            userManager,
            userName: "Cristopher vilchz",
            email: "cristopher_vilchez@usmp.pe",
            password: "Analista@2026");

        if (!await userManager.IsInRoleAsync(analista, "Analista"))
        {
            var addRoleResult = await userManager.AddToRoleAsync(analista, "Analista");
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", addRoleResult.Errors.Select(e => e.Description)));
            }
        }

        var clienteUser1 = await EnsureUserAsync(
            userManager,
            userName: "cliente.uno",
            email: "cliente1@creditos.local",
            password: "Cliente@2026");

        var clienteUser2 = await EnsureUserAsync(
            userManager,
            userName: "cliente.dos",
            email: "cliente2@creditos.local",
            password: "Cliente@2026");

        var cliente1 = await EnsureClienteAsync(context, clienteUser1.Id, 3500m, true);
        var cliente2 = await EnsureClienteAsync(context, clienteUser2.Id, 5000m, true);

        if (!await context.SolicitudesCredito.AnyAsync())
        {
            context.SolicitudesCredito.AddRange(
                new SolicitudCredito
                {
                    ClienteId = cliente1.Id,
                    MontoSolicitado = 8000m,
                    FechaSolicitud = DateTime.UtcNow.AddDays(-2),
                    Estado = EstadoSolicitud.Pendiente
                },
                new SolicitudCredito
                {
                    ClienteId = cliente2.Id,
                    MontoSolicitado = 12000m,
                    FechaSolicitud = DateTime.UtcNow.AddDays(-7),
                    Estado = EstadoSolicitud.Aprobado
                });

            await context.SaveChangesAsync();
        }
    }

    private static async Task<IdentityUser> EnsureUserAsync(
        UserManager<IdentityUser> userManager,
        string userName,
        string email,
        string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            return user;
        }

        user = new IdentityUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return user;
    }

    private static async Task<Cliente> EnsureClienteAsync(
        ApplicationDbContext context,
        string usuarioId,
        decimal ingresosMensuales,
        bool activo)
    {
        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
        if (cliente != null)
        {
            return cliente;
        }

        cliente = new Cliente
        {
            UsuarioId = usuarioId,
            IngresosMensuales = ingresosMensuales,
            Activo = activo
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        return cliente;
    }
}
