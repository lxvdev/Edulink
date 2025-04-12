using System;

namespace Edulink.Models
{
    public class Computer
    {
        public string Name { get; set; }
        public Guid ID { get; set; }
        public bool IsTeacher { get; set; }

        public Computer() { }
    }
}
