using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateJob : CreateJob
    {
        [DefaultValue("")]
        [Required]
        public Guid? JobId { get; set; }

        [DefaultValue("")]
        [Required]
        public Guid? JobStatus { get; set; }



    }
}
