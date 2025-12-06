using Microsoft.AspNetCore.Identity;

namespace Systeme_RH.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? EmployeId{get; set;}

    }
}
