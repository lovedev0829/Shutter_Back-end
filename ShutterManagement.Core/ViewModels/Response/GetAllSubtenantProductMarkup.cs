using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetAllSubtenantProductMarkup
    {
        public Guid? Id { get; set; }

        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }

        public decimal? Markup { get; set; }
    }
}
