using E_Learning.Models.Course;

namespace E_Learning.Models.Comments
{
	public class CourseDetailsViewModel
	{
		public AddCourseCategory Course { get; set; }
		public List<Comments> Comments { get; set; }

		public int Rating { get; set; }
	}
}
