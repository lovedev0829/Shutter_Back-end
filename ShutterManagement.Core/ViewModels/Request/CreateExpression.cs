using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace ShutterManagement.Core.ViewModels.Request
{
     public class CreateExpression
     {
        [Default("")]
        [Required]
        public string? ExpressionName { get; set; }

        [Default("")]
        [Required]
        public string? ExpressionFormulae { get; set; }

        [Default("")]
        //[Required]
        public Guid? ProductId { get; set; } = Guid.Empty;

        [Default("")]
        [Required]
        public Guid? ProductTypeId { get; set; }
        [Default("")]
        public string? OptionId { get; set; } = "";

        [Default("")]
        [Required]
        public string[]? FormulaeArray { get; set; }
        public string[]? displayArray { get; set; }

    }
}
