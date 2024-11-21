using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetAllAddress
    {

        public string? ContactId { get; set; }

        public string? Label { get; set; }


        public string? AddressLine1 { get; set; }


        public string? AddressLine2 { get; set; }

        public string? City { get; set; }

       

        public string? State { get; set; }

      
        public string? ZipCode { get; set; }


        public string? Country { get; set; }
        public bool? IsActive { get; set; }
    }
}
