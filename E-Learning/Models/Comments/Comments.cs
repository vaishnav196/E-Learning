using System.ComponentModel.DataAnnotations;

namespace E_Learning.Models.Comments
{
	public class Comments
	{
		[Key]
		public int CommentId { get; set; }

		public string UserComment { get; set; }

		public string userEmail { get; set; }

		public string courseName { get; set; }

		public string status { get; set; }
	}
}
