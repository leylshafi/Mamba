using Mamba.Models.Base;

namespace Mamba.Models
{
    public class Position:BaseEntity
    {
        public string Name { get; set; }

        public List<Employee>? Employees { get; set; }
    }
}
