using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class UpdateJobStatus
    {
        [Required]
        public Guid? JobId { get; set; }

        [Required]
        public Guid? JobStatus { get; set; }
    }
}
