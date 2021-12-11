using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Person
    {
        public long Id { get; set; }
        public string? Surname { get; set; }
        public string? GivenName { get; set; }
        public string? MiddleName { get; set; }
    }
}
