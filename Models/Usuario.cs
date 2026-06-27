using Microsoft.AspNetCore.Identity;

namespace FacturacionApp.Models
{
    public class Usuario : IdentityUser
    {
        public string NombreCompleto { get; set; } = string.Empty;
    }
}