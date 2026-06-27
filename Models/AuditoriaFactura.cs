using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionApp.Models
{
    [Table("AuditoriasFactura")]
    public class AuditoriaFactura
    {
        public int Id { get; set; }

        [Required]
        public int FacturaId { get; set; }
        public virtual Factura? Factura { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        public virtual Usuario? Usuario { get; set; }

        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(150)]
        [Display(Name = "Campo modificado")]
        public string CampoModificado { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Valor anterior")]
        public string ValorAnterior { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Valor nuevo")]
        public string ValorNuevo { get; set; } = string.Empty;
    }
}