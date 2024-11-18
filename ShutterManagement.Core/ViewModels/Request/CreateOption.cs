using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateOption
    {

        public string? OptionName { get; set; }

        public string? OptionType { get; set; }

        public string? Description { get; set; }

        public decimal? AdditionalCost { get; set; }

        public string ProductTypeId { get; set; }
    }
}
