using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionApp.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre del Cliente")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RNC o Cédula es requerido")]
        [StringLength(20, ErrorMessage = "No puede exceder 20 caracteres")]
        [Display(Name = "RNC / Cédula")]
        public string RncCedula { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        [StringLength(100)]
        [Display(Name = "Correo Electrónico")]
        public string? Email { get; set; }

        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        // Navegación
        public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    }
}
