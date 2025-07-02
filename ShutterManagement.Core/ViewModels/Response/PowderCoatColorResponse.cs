using System;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class PowderCoatColorResponse
    {
        public Guid Id { get; set; }
        public string? ColorName { get; set; }
        public string? Description { get; set; }
        public decimal? Cost { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDelete { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}