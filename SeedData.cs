using Microsoft.AspNetCore.Identity;

public static class SeedData
{
    public static async Task Inicializar(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "DirectorOperaciones", "AuxiliarInventario", "Empleado" };

        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }
}