using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpoilerBlockerFull.Models
{
    public class Keyword
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual Category Category { get; set; }
    }
}
