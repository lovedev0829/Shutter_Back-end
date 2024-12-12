using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateEstimate
    {
        [Default("")]
        public Guid? JobId { get; set; }
        [Default("")]
        public string? EstimateName { get; set; }
        [Default("")]
        public Guid[]? WindowProductId { get; set; } = new Guid[] {};
    }
}
