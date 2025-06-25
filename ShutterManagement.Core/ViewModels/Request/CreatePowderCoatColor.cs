using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreatePowderCoatColor
    {
        [Required]
        [MaxLength(100)]
        public string? ColorName { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cost must be a positive number")]
        public decimal? Cost { get; set; }
    }
} 