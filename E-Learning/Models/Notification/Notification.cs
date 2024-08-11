using System.ComponentModel.DataAnnotations;

namespace E_Learning.Models.Notification
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
