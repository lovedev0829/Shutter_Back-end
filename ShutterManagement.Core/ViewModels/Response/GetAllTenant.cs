using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetAllTenant
    {
        public Guid? TenantId { get; set; }
        public string? FirstName { get; set; }

      
        public string? MiddleName { get; set; }
       
        public string? LastName { get; set; }
      
        public string? Code { get; set; }
      
        public string? URL { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Logo { get; set; }

        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public bool? IsActive { get; set; }

        public string? UserFirstName { get; set; }

        public string? UserMiddleName { get; set; }

    
        public string? UserLastName { get; set; }
      
        public string? UserEmail { get; set; }

     
        public string? UserPhoneNumber { get; set; }
    }
}
