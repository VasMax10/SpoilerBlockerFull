using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpoilerBlockerFull.Models
{
    public class Category
    {
        public Category()
        {
            Keywords = new List<Keyword>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public virtual AppUser User { get; set; }
        public virtual ICollection<Keyword> Keywords { get; set; }
    }
}
