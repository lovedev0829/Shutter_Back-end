using System;
using System.ComponentModel.DataAnnotations;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdatePowderCoatColor : CreatePowderCoatColor
    {
        [Required]
        public Guid? Id { get; set; }
    }
}