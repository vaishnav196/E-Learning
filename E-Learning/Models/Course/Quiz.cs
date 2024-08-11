using System.ComponentModel.DataAnnotations;

namespace E_Learning.Models.Course
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }
        public string CourseName { get; set; }
        public string Subcourse { get; set; }
        public string title { get; set; }
        public string question { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public string answer { get; set; }
    }
}
