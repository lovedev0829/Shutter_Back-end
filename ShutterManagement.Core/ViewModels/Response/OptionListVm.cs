using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class OptionListVm
    {
        public Guid OptionId { get; set; }
        public string? OptionName { get; set; }
        public string? OptionType { get; set; }
        public string? Description { get; set; }
        public decimal? AdditionalCost { get; set; }
        public bool? IsActive { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? ProductTypeId { get; set; }
        public bool? IsDefault { get; set; }
    }
}
