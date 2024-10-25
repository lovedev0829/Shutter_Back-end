using ShutterManagement.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels
{
    public class CreatePayloadVm
    {
        public int Type { get; set; }
        public object? Data { get; set; }
        public string? Message { get; set; }
        public string? AdditionalData { get; set; }
    }
}
