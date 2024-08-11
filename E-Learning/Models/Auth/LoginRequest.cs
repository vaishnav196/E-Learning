 
using System.ComponentModel.DataAnnotations;

namespace E_Learning.Models.Auth
{
    public class LoginRequest
    {
        // DTO table
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public string Role { get; set; }
        
    }
}
