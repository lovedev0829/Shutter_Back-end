using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
     public class CreateExpression
     {
        public string? ExpressionName { get; set; }

        public string? ExpressionFormulae { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ProductTypeId { get; set; }

    }
}
