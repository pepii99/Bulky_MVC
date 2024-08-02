using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Bulky.Models
{
    public class Applicationuser : IdentityUser
    {
        [Required]
        public string? Name { get; set; }

        public string? City { get; set; }
        public string? StreetAddress { get; set; }
        public string? PostalCode { get; set; }
    }
}
