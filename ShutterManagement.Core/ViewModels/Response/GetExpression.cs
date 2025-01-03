using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetExpression
    {
        public Guid? ExpressionId { get; set; }
        public string? ExpressionName { get; set; }
        public string? ExpressionFormulae { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? OptionId { get; set; }
        public string? ProductName { get; set; }
        public Guid? ProductTypeId { get; set; }
        public string? ProductTypeName { get; set; }
        public string[]? formulaeArray { get; set; }
        public bool? IsActive { get; set; }
        public Guid? TenantId { get; set; }
    }
}
