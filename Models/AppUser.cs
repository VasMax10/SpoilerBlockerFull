using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpoilerBlockerFull.Models
{
    public class AppUser : IdentityUser
    {
        public AppUser()
        {
            Categories = new List<Category>();
        }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string NickName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
    }
}
