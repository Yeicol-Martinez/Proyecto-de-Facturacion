using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionApp.Models
{
    public enum EstadoFactura
    {
        Pendiente,
        Pagada,
        Anulada
    }

    [Table("Facturas")]
    public class Factura
    {
        public int Id { get; set; }

        [Display(Name = "No. Factura")]
        [StringLength(20)]
        public string NumeroFactura { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de emisión es requerida")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Emisión")]
        public DateTime FechaEmision { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [Display(Name = "Estado")]
        public EstadoFactura Estado { get; set; } = EstadoFactura.Pendiente;

        [StringLength(300)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // Subtotales calculados y guardados
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total ITBIS")]
        public decimal TotalItbis { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total")]
        public decimal Total { get; set; }

        // FK Cliente
        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        // Navegación
        public virtual Cliente? Cliente { get; set; }
        public virtual ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
    }

    [Table("DetallesFactura")]
    public class DetalleFactura
    {
        public int Id { get; set; }

        [Required]
        public int FacturaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un producto")]
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, 9999, ErrorMessage = "La cantidad debe ser al menos 1")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "ITBIS (%)")]
        public decimal Itbis { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal => Cantidad * PrecioUnitario;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto ITBIS")]
        public decimal MontoItbis => Subtotal * (Itbis / 100);

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Línea")]
        public decimal TotalLinea => Subtotal + MontoItbis;

        // Navegación
        public virtual Factura? Factura { get; set; }
        public virtual Producto? Producto { get; set; }
    }
}
