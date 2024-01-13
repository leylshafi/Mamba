using Mamba.Models.Base;

namespace Mamba.Models
{
    public class Service:BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
