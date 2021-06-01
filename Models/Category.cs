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
        public int TMDbId { get; set; }
        public bool IsImported { get; set; }
        public string Type { get; set; }
        public string CoverColor { get; set; }
        public string UserId { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public string Overview { get; set; }
        public virtual AppUser User { get; set; }
        public virtual ICollection<Keyword> Keywords { get; set; }
    }
}
