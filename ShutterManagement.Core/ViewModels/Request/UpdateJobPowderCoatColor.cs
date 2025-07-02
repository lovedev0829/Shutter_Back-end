using System;
using System.ComponentModel.DataAnnotations;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateJobPowderCoatColor
    {
        [Required]
        public Guid JobId { get; set; }
        
        public Guid? PowderCoatColorId { get; set; }
    }
} 