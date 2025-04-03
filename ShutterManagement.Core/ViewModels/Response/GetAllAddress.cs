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

        public Guid AddressId { get; set; }
        public string? ContactId { get; set; }
        public string? Label { get; set; }
        public string? StreetAddress { get; set; }
        public string? Notes { get; set; }
        public string? City { get; set; }
        public Guid? StateId { get; set; }
        public string? StateName { get; set; }
        public string? ZipCode { get; set; }
        public string? AccessCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsBillingAddress { get; set; }
    }
}
