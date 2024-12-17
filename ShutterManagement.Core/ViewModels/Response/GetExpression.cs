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
        public bool? IsActive { get; set; }
    }
}
