using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionApp.Models
{
    public enum TipoEntidadSolicitud
    {
        Cliente,
        Producto,
        Factura
    }

    public enum EstadoSolicitud
    {
        Pendiente,
        Aprobada,
        Rechazada
    }

    [Table("Solicitudes")]
    public class Solicitud
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tipo")]
        public TipoEntidadSolicitud TipoEntidad { get; set; }

        [Required]
        public int EntidadId { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "Datos actuales")]
        public string DatosActuales { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        [Display(Name = "Cambio solicitado")]
        public string CambioPropuesto { get; set; } = string.Empty;

        [Display(Name = "Estado")]
        public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

        // Quien hizo la solicitud
        [Required]
        public string SolicitanteId { get; set; } = string.Empty;
        public virtual Usuario? Solicitante { get; set; }

        [Display(Name = "Fecha de solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        // Quien la aprobó o rechazó
        public string? RevisadoPorId { get; set; }
        public virtual Usuario? RevisadoPor { get; set; }

        [Display(Name = "Fecha de revisión")]
        public DateTime? FechaRevision { get; set; }

        [StringLength(500)]
        [Display(Name = "Comentario del revisor")]
        public string? ComentarioRevision { get; set; }
    }
}