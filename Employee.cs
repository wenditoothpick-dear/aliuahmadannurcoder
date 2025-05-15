using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleAdoDotNet
{
    public class Employee
    {
        public Guid id { get; set; }
        public string firstname { get; set; } = default!;
        public string lastname { get; set; } = default!;
        public string email { get; set; } = default!;
        public string department { get; set; } = default!;
        public DateTime hiredate { get; set; }
        public decimal salary { get; set; }
    }
}
