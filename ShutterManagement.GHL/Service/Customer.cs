using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.GHL.Service
{
    public class Customer
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string MobilePh { get; set; }
        public bool SyncStatus { get; set; }
        public bool IsFromGHL { get; set; }
    }
}
