using System;
using System.ComponentModel.DataAnnotations;

namespace StockManager.ViewModels
{
    public class StockMovement
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Producto")]
        public int ProductId { get; set; }

        public Product? Product { get; set; }

        [Required]
        [Display(Name = "Fecha")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Cantidad (+ ingreso / - egreso)")]
        public int Quantity { get; set; }

        [StringLength(200)]
        [Display(Name = "Detalle / Nota")]
        public string? Note { get; set; }
    }
}