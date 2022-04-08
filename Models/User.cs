using Microsoft.AspNetCore.Identity;
 
namespace NotMyShows.Models
{
    public class User : IdentityUser
    {
        public UserProfile UserProfile { get; set; }
    }
}