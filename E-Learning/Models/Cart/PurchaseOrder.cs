using System.ComponentModel.DataAnnotations;

namespace E_Learning.Models.Cart
{
    public class PurchaseOrder
    {
        [Key]
        public int Id { get; set; }
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public string Email { get; set; }
        public double Price { get; set; }
   
        public int Duration { get; set; }

        // Add properties for PurchasedDate and EndDate
        public string PurchasedDate { get; set; }
        public string EndDate { get; set; }
    }   
}
