using System.ComponentModel.DataAnnotations;

namespace E_Learning.Models.Cart
{
    public class Cart
    {

        [Key]
        public int Id { get; set; }
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public string Email { get; set; }
        public double Price { get; set; }
        public int Duration { get; set; }
    }
}
