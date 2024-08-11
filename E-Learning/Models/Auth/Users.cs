using System.ComponentModel.DataAnnotations;

namespace E_Learning.Models.Auth
{
    // Featching Table
    public class Users
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string Role { get; set; } = "user";
        [Required]
        public string Contact { get; set; }
    }
}
