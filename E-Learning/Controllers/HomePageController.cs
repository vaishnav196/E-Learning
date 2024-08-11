using E_Learning.Models.Cart;
using E_Learning.Models.Comments;
using E_Learning.Models.Course;
using E_Learning.Models.Promotion;
using E_Learning.Models.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace E_Learning.Controllers
{
    [Authorize]
    public class HomePageController : Controller
    {
        private readonly HttpClient client;

        public HomePageController()
        {

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            client = new HttpClient(clientHandler);
        }
        
        public IActionResult Index()
        {

            // promotion url ->https://localhost:44342/api/Promotion/GetPromotion
            string url = "https://localhost:44342/api/Promotion/GetPromotion";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata = response.Content.ReadAsStringAsync().Result;
                var courses = JsonConvert.DeserializeObject<List<Promotion>>(jsondata);
                return View(courses);
            }

            return View();
           
        }
        // ------------------------------------------------ Course Manangement 
        public IActionResult Courses()    // Call API 
        {
            string url = "https://localhost:44342/api/Course/GetAllCourse";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata = response.Content.ReadAsStringAsync().Result;
                var courses = JsonConvert.DeserializeObject<List<AddCourseCategory>>(jsondata);
                return View(courses);
            }

            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        // ------------------------------------------------ Quiz Manangement 

        [HttpPost]
        public IActionResult ONE()
        {
            var id = Request.Form["Id"];  // here is posting data fron view to action 
            HttpContext.Session.SetString("id", id);
            return RedirectToAction("CourseMainDetails");
        }

        /*    public IActionResult CourseMainDetails()

			{
				 int obj = int.Parse(HttpContext.Session.GetString("id"));

				string url = $"https://localhost:44342/api/Course/GetCourseById/{obj}";
				HttpResponseMessage response = client.GetAsync(url).Result;

				if (response.IsSuccessStatusCode)
				{
					string jsondata = response.Content.ReadAsStringAsync().Result;
					var course = JsonConvert.DeserializeObject<AddCourseCategory>(jsondata);
					return View(course);
				}
				return View();
			}*/
        /*public IActionResult CourseMainDetails()
		{
			int obj = int.Parse(HttpContext.Session.GetString("id"));

			string url = $"https://localhost:44342/api/Course/GetCourseById/{obj}";
			HttpResponseMessage response = client.GetAsync(url).Result;

			if (response.IsSuccessStatusCode)
			{
				string jsondata = response.Content.ReadAsStringAsync().Result;
				var course = JsonConvert.DeserializeObject<AddCourseCategory>(jsondata);

				// Fetch comments for the course
				string commentsUrl = $"https://localhost:44342/api/Comments/GetCommentsByCourseName?courseName={course.CourseName}";
				HttpResponseMessage commentsResponse = client.GetAsync(commentsUrl).Result;

                // Fetch reviews for the course
                string reviewsUrl = $"https://localhost:44342/api/Review/GetReviewsByCourseName?courseName={course.CourseName}";
                HttpResponseMessage reviewsResponse = client.GetAsync(reviewsUrl).Result;
                  
				List<Comments> comments = new List<Comments>();
				if (commentsResponse.IsSuccessStatusCode)
				{
					string commentsJsonData = commentsResponse.Content.ReadAsStringAsync().Result;
					comments = JsonConvert.DeserializeObject<List<Comments>>(commentsJsonData);
				}

				// Create the view model
				var viewModel = new CourseDetailsViewModel
				{
					Course = course,
					Comments = comments,
                    Rating = // here store the average rating of the course
				};

				return View(viewModel);
			}
			return View();
		}*/


        public IActionResult CourseMainDetails()
        {
            int obj = int.Parse(HttpContext.Session.GetString("id"));

            string url = $"https://localhost:44342/api/Course/GetCourseById/{obj}";
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                string jsonData = response.Content.ReadAsStringAsync().Result;
                var course = JsonConvert.DeserializeObject<AddCourseCategory>(jsonData);

                // Fetch comments for the course
                string commentsUrl = $"https://localhost:44342/api/Comments/GetCommentsByCourseName?courseName={course.CourseName}";
                HttpResponseMessage commentsResponse = client.GetAsync(commentsUrl).Result;

                List<Comments> comments = new List<Comments>();
                if (commentsResponse.IsSuccessStatusCode)
                {
                    string commentsJsonData = commentsResponse.Content.ReadAsStringAsync().Result;
                    comments = JsonConvert.DeserializeObject<List<Comments>>(commentsJsonData);
                }

                // Fetch average rating for the course
                string reviewsUrl = $"https://localhost:44342/api/Review/GetReviewAvg?courseName={course.CourseName}";
                HttpResponseMessage reviewsResponse = client.GetAsync(reviewsUrl).Result;

                int averageRating = 0;
                if (reviewsResponse.IsSuccessStatusCode)
                {
                    string reviewsJsonData = reviewsResponse.Content.ReadAsStringAsync().Result;
                    double averageRatingDouble = JsonConvert.DeserializeObject<double>(reviewsJsonData);
                    averageRating = (int)Math.Round(averageRatingDouble);
                }

                // Create the view model
                var viewModel = new CourseDetailsViewModel
                {
                    Course = course,
                    Comments = comments,
                    Rating = averageRating
                };

                return View(viewModel);
            }
            return View();
        }


        public IActionResult ContactUs()
        {
            return View();
        }
       
        // ------------------------------------------------  Cart Managment 

        public IActionResult Cart()
        {
            // Get Api
            // url -> https://localhost:44342/api/Cart/FetchCart?Email=khrish%40gmail
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var url = $"https://localhost:44342/api/Cart/FetchCart?Email={userEmail}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata = response.Content.ReadAsStringAsync().Result;
                var course = JsonConvert.DeserializeObject<List<Cart>>(jsondata);
                return View(course);
            }
            return View();
        }

        [HttpPost]
        public IActionResult Cart(string CourseName, string InstructorName, double Price ,int Duration)
        {
            // Get User Email from Session
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var data = new Cart
            {
                Email = userEmail,
                InstructorName = InstructorName,
                CourseName = CourseName,
                Price = Price,
                Duration = Duration
            };
            // api cart 
            string url = "https://localhost:44342/api/Cart/AddToCart";
            var jsondata = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                // Toster For notification (Optional)
                TempData["SuccessMessage"] = "Your course has been added to the cart successfully!";
                return RedirectToAction("CourseMainDetails");
            }


            return View();
        }

        public IActionResult RemoveCartItem(int id)
        {
            // Your logic to remove the item from the cart using the id
            // url -> https://localhost:44342/api/Cart/DeleteItem/15

            string url = $" https://localhost:44342/api/Cart/DeleteItem/{id}";
            HttpResponseMessage response = client.DeleteAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Cart");
            }
            return RedirectToAction("Cart");
        }
    
    
      // ---------------------------------------------- PDF View
      public IActionResult Invoice() {
            return View();
        }


        // ------------------------- PlaceOrder function
        [HttpPost]
        public async Task<IActionResult> PlaceOrderFun()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var url = $"https://localhost:44342/api/Cart/ProceedToOrder?Email={userEmail}";
            HttpResponseMessage response = await client.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return View();
        }
    
    
        // --------------------------------  USER DASHBOARD
        public IActionResult Dashboard()
        {
            // api call ->https://localhost:44342/api/Cart/Order_History?Email=tan%40gmail 
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var url = $"https://localhost:44342/api/Cart/Order_History?Email={userEmail}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata = response.Content.ReadAsStringAsync().Result;
                var course = JsonConvert.DeserializeObject<List<PurchaseOrder>>(jsondata);
                return View(course);
            }
            return View();
        }


        public IActionResult ViewCourse()
        {
            // api call ->https://localhost:44342/api/Cart/Order_History?Email=tan%40gmail 
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var url = $"https://localhost:44342/api/Cart/Order_History?Email={userEmail}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata = response.Content.ReadAsStringAsync().Result;
                var course = JsonConvert.DeserializeObject<List<PurchaseOrder>>(jsondata);
                return View(course);
            }
            return View();
        }

        /*       public IActionResult ViewCourseContent(string CourseName)
              {
                  // url -> https://localhost:44342/api/Cart/GetSubCourseAuth?Email=tan%40gmail&CourseName=python
                  var userEmail = HttpContext.Session.GetString("UserEmail");
                  var url = $"https://localhost:44342/api/Cart/GetSubCourseAuth?Email={userEmail}&CourseName={CourseName}";
                  HttpResponseMessage response = client.GetAsync(url).Result;
                  if (response.IsSuccessStatusCode)
                  {
                      string jsondata = response.Content.ReadAsStringAsync().Result;
                      var course = JsonConvert.DeserializeObject<List<AddCourseContent>>(jsondata);
                      return View(course);
                  }

                  return View();
              }*/


        //public IActionResult ViewCourseContent(string CourseName)
        //{
        //    var userEmail = HttpContext.Session.GetString("UserEmail");
        //    var url = $"https://localhost:44342/api/Cart/GetSubCourseAuth?Email={userEmail}&CourseName={CourseName}";
        //    HttpResponseMessage response = client.GetAsync(url).Result;

        //    if (response.IsSuccessStatusCode)
        //    {
        //        string jsonData = response.Content.ReadAsStringAsync().Result;
        //        var courseContent = JsonConvert.DeserializeObject<List<AddCourseContent>>(jsonData);

        //        // Assuming you have another API call or method to get quizzes related to the course
        //        var quizzesUrl = $"https://localhost:44342/api/Course/GetQuizByCourse?CourseName={CourseName}";
        //        HttpResponseMessage quizzesResponse = client.GetAsync(quizzesUrl).Result;
        //        List<Quiz> quizzes = new List<Quiz>();
        //        if (quizzesResponse.IsSuccessStatusCode)
        //        {
        //            string quizzesJsonData = quizzesResponse.Content.ReadAsStringAsync().Result;
        //            quizzes = JsonConvert.DeserializeObject<List<Quiz>>(quizzesJsonData);
        //        }

        //        var viewModel = new CourseContentQuizViewModel
        //        {
        //            CourseContent = courseContent,
        //            Quizzes = quizzes
        //        };

        //        return View(viewModel);
        //    }

        //    return View(new CourseContentQuizViewModel { CourseContent = new List<AddCourseContent>(), Quizzes = new List<Quiz>() });
        //}

        public IActionResult ViewCourseContent(string CourseName)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var url = $"https://localhost:44342/api/Cart/GetSubCourseAuth?Email={userEmail}&CourseName={CourseName}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata = response.Content.ReadAsStringAsync().Result;
                var courseContent = JsonConvert.DeserializeObject<List<AddCourseContent>>(jsondata);

                // Fetch quizzes for the given course
                var quizUrl = $"https://localhost:44342/api/Course/GetQuizByCourse?CourseName={CourseName}";
                HttpResponseMessage quizResponse = client.GetAsync(quizUrl).Result;
                List<Quiz> quizzes = new List<Quiz>();

                if (quizResponse.IsSuccessStatusCode)
                {
                    string quizJsonData = quizResponse.Content.ReadAsStringAsync().Result;
                    quizzes = JsonConvert.DeserializeObject<List<Quiz>>(quizJsonData);
                }

                var viewModel = new CourseContentQuizViewModel
                {
                    CourseContent = courseContent,
                    Quizzes = quizzes
                };

                HttpContext.Session.SetString("CourseName", CourseName);

                return View(viewModel);
            }

            return View();
        }


        public IActionResult Certificate()
        {
            var userName = HttpContext.Session.GetString("Username");
            var courseName = HttpContext.Session.GetString("CourseName");

            var model = new CertificateViewModel
            {
                UserName = userName,
                CourseName = courseName,
                Date = DateTime.Now.ToString("MMMM dd, yyyy")
            };

            return View(model);
        }

        // ------------------------------------------------- Create Comment 




     /*   string url = "https://localhost:44342/api/Auth/AddUser";
        var jsondata = JsonConvert.SerializeObject(u);
        StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
        HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login", "Auth");
    }
            return View();*/

    [HttpPost]
    public IActionResult CreateComment(string courseName,string comment,string status="")
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var url ="https://localhost:44342/api/Comments/AddComment";
            var data = new Comments
            {
                UserComment = comment,
                userEmail = userEmail,
                courseName = courseName,
                status = status
            };
            var jsondata = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }


        // ------------------------------------------------- Create Rateing

        [HttpPost]
        public IActionResult CreateRateing(string courseName, int rating)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var url = "https://localhost:44342/api/Review/AddReview";
            var data = new Review
            {
                CourseName = courseName,
                ReviewerEmail = userEmail,
                Rating = rating
            };
            var jsondata = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }
        public IActionResult Calender()
        {
            return View();
        }
    }
}
