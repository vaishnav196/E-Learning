using E_Learning.Models.Cart;
using E_Learning.Models.Course;
using E_Learning.Models.Dashboard;
using E_Learning.Models.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;

using E_Learning.Models.Auth;

namespace E_Learning.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly HttpClient client;
        public AdminController()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            client = new HttpClient(clientHandler);
            client = new HttpClient(clientHandler)
            {
                BaseAddress = new System.Uri("https://localhost:44342/api/Course/") // Base URL of the external API 
            };
        }


        public async Task<IActionResult> Index()
        {

            // Consume first API
            var registrationResponse = await client.GetAsync("https://localhost:44342/api/Chart/registrations-per-month");
            if (!registrationResponse.IsSuccessStatusCode)
            {
                // Handle error
                return View("Error");
            }
            var registrationJsonString = await registrationResponse.Content.ReadAsStringAsync();
            var registrationData = System.Text.Json.JsonSerializer.Deserialize<List<RegistrationData>>(registrationJsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Consume second API
            var monthlyAmountResponse = await client.GetAsync("https://localhost:44342/api/Chart/MonthlyTotals");
            if (!monthlyAmountResponse.IsSuccessStatusCode)
            {
                // Handle error
                return View("Error");
            }
            var monthlyAmountJsonString = await monthlyAmountResponse.Content.ReadAsStringAsync();
            var monthlyAmountData = System.Text.Json.JsonSerializer.Deserialize<List<MonthlyAmountViewModel>>(monthlyAmountJsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var viewModel = new CombinedViewModel
            {
                RegistrationData = registrationData,
                MonthlyAmountData = monthlyAmountData,
                TotalRegisteredStudents = registrationData.Sum(r => r.Count), // Calculate total registered
                TotalRevenue = monthlyAmountData.Sum(m => (decimal)m.TotalAmount)
            };

            return View(viewModel);
        }

        public IActionResult AddMaterial()
        {
            return View();
        }

        // ------------------------------------------------- Course Manuplation
        public IActionResult AddCourse()
        {
            return View();
        }

        /*[HttpPost]
		public IActionResult AddCourse(AddCourseCategory addcate)
		{
			string url = "https://localhost:44342/api/Course/AddCategory";
			var jsondata = JsonConvert.SerializeObject(addcate);
			StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
			HttpResponseMessage response = client.PostAsync(url, content).Result;
			if (response.IsSuccessStatusCode)
			{
				TempData["SuccessMessage"] = "Course added successfully!";
				return RedirectToAction("AddCourse"); // Redirect to avoid form resubmission

			}
			else
			{
				TempData["ErrorMessage"] = "Failed to add course.";
				return RedirectToAction("AddCourse");
			}
		}*/


        public IActionResult AddCourseCont()
        {
            // here calling api 
            string url = "https://localhost:44342/api/Course/GetCourseName";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata = response.Content.ReadAsStringAsync().Result;
                var courseNames = JsonConvert.DeserializeObject<List<string>>(jsondata);
                if (courseNames != null)
                {
                    ViewBag.CourseNames = courseNames;
                    return View();
                }
            }
            return View();
        }


        [HttpPost]

        public async Task<IActionResult> AddCourseAsync(AddCourseCategory addcate)
        {
            string url = "https://localhost:44342/api/Course/AddCategory";
            var jsondata = JsonConvert.SerializeObject(addcate);
            StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var users = await client.GetFromJsonAsync<IEnumerable<Users>>("https://localhost:44342/api/Auth/GetAllUsers");

                if (users != null)
                {
                    var emailRequest = new EmailRequest
                    {
                        ToEmails = users.Select(u => u.Email).ToList(),
                        Subject = "Exciting New Course Available at an Unbeatable Price!",
                        Body = $@"
                    <p>Dear User,</p>
                    <p>We are thrilled to announce the launch of our brand new course: <strong>{addcate.CourseName}</strong>!</p>
                    <h3>Why You Should Enroll:</h3>
                    <ul>
                        <li><strong>Engaging Content:</strong> Our course is packed with practical insights and engaging content designed to help you master {addcate.CourseName}.</li>
                        <li><strong>Expert Instructor:</strong> Learn from the best in the field, with guiding you through every step of the way.</li>
                        <li><strong>Affordable Pricing:</strong> At just {addcate.Price}, you get access to top-quality education without breaking the bank. We believe in providing value, and our pricing reflects that commitment.</li>
                        <li><strong>Flexible Learning:</strong> Learn at your own pace with our flexible online platform. Whether you prefer to study in the mornings, evenings, or weekends, this course is designed to fit into your schedule.</li>
                    </ul>
                    <h3>Course Highlights:</h3>
                    <ul>
                        <li><strong>Comprehensive Curriculum:</strong> Covering all the essential topics you need to succeed.</li>
                        <li><strong>Interactive Sessions:</strong> Engage with the instructor and fellow students through live Q&A sessions.</li>
                        <li><strong>Hands-on Projects:</strong> Apply what you've learned in real-world scenarios.</li>
                    </ul>
                    <h3>Special Launch Offer:</h3>
                    <p>To celebrate the launch of <strong>{addcate.CourseName}</strong>, we are offering an exclusive discount for the first 30 enrollees. Don't miss out on this opportunity to enhance your skills at an unbeatable price!</p>
                    <h3>How to Enroll:</h3>
                    <ol>
                        <li><strong>Visit our website:</strong> [Insert Website Link]</li>
                        <li><strong>Register:</strong> Create an account or log in if you already have one.</li>
                        <li><strong>Enroll in the Course:</strong> Find <strong>{addcate.InstructorName}</strong> and click 'Enroll Now'.</li>
                        <li><strong>Start Learning:</strong> Begin your journey to mastering {addcate.CourseDescription}.</li>
                    </ol>
                    <p>We can't wait to see you in the course!</p>
                    <p>Best regards,<br>The Masstech Team</p>
                    <p><strong>P.S.</strong> Have questions? Feel free to reply to this email or visit our FAQ page/Contact Us page for more information.</p>"
                    };

                    var emailResponse = await client.PostAsJsonAsync("https://localhost:44342/api/Email/send", emailRequest);

                    if (!emailResponse.IsSuccessStatusCode)
                    {
                        TempData["ErrorMessage"] = "Failed to send email notification.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to fetch users.";
                }

                TempData["SuccessMessage"] = "Course added successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add course.";
            }

            return RedirectToAction("AddCourse"); // Redirect to avoid form resubmission
        }

        [HttpPost]
        public IActionResult AddCourseCont(AddCourseContent addcont)
        {
            string url = "https://localhost:44342/api/Course/AddContent";
            var jsondata = JsonConvert.SerializeObject(addcont);
            StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Sub Category added successfully";
                return RedirectToAction("AddCourseCont");
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add sub category.";
                return RedirectToAction("AddCourseCont");
            }
        }



        //--------------------------------------------------------------------Abhang
        public async Task<IActionResult> AddQuizes()
        {
            var courseNames = await client.GetFromJsonAsync<List<string>>("GetCourseNameABH");
            ViewBag.CourseNames = courseNames;
            return View();
        }

        // Action to get subcourses based on a course name
        [HttpGet]
        public async Task<JsonResult> GetSubcourses(string courseName)
        {
            var subcourses = await client.GetFromJsonAsync<List<string>>($"GetSubCourseABH?CourseName={courseName}");
            return Json(subcourses);
        }

        // Action to get titles based on a course name and subcourse
        [HttpGet]
        public async Task<JsonResult> GetTitles(string courseName, string subcourse)
        {
            var titles = await client.GetFromJsonAsync<List<string>>($"GetTitleABH?CourseName={courseName}&SubCourseName={subcourse}");
            return Json(titles);
        }

        [HttpPost]
        public async Task<JsonResult> SetQuizCount(int no_of_quiz, string courseName, string subcourse, string title)
        {
            try
            {
                HttpContext.Session.SetInt32("QuizCount", no_of_quiz);
                HttpContext.Session.SetInt32("CurrentQuiz", 1);
                HttpContext.Session.SetString("CourseName", courseName);
                HttpContext.Session.SetString("Subcourse", subcourse);
                HttpContext.Session.SetString("Title", title);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Handle exception if necessary
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> AddQuiz(string question, string Option1, string Option2, string Option3, string Option4, string answer)
        {
            try
            {
                var quizData = new Quiz
                {
                    CourseName = HttpContext.Session.GetString("CourseName"),
                    Subcourse = HttpContext.Session.GetString("Subcourse"),
                    title = HttpContext.Session.GetString("Title"),
                    question = question,
                    Option1 = Option1,
                    Option2 = Option2,
                    Option3 = Option3,
                    Option4 = Option4,
                    answer = answer
                };

                var apiUrl = "https://localhost:44342/api/Course/AddQuizABH";
                var response = await client.PostAsJsonAsync(apiUrl, quizData);
                if (response.IsSuccessStatusCode)
                {
                    var currentQuiz = HttpContext.Session.GetInt32("CurrentQuiz") ?? 0;
                    var quizCount = HttpContext.Session.GetInt32("QuizCount") ?? 0;

                    if (currentQuiz < quizCount)
                    {
                        HttpContext.Session.SetInt32("CurrentQuiz", currentQuiz + 1);
                        return Json(new { success = true });
                    }
                    else
                    {
                        HttpContext.Session.Clear();
                        return Json(new { success = true, message = "All questions have been added." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Error adding quiz." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ---------------------------------------------- History of Product 
        [HttpGet]
        public IActionResult ShowOrderHistory()
        {
            string url = "https://localhost:44342/api/Cart/FetchOrderedDetails";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata = response.Content.ReadAsStringAsync().Result;
                var courses = JsonConvert.DeserializeObject<List<PurchaseOrder>>(jsondata);
                return View(courses);
            }

            return View();
        }

        // ------------------------------------------   CONTENT MODERATION
        public IActionResult ContentModeration()
        {
            // api call  
            string url = "https://localhost:44342/api/Comments/GetAllUnapporveComments";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata = response.Content.ReadAsStringAsync().Result;
                var comments = JsonConvert.DeserializeObject<List<Comments>>(jsondata);
                return View(comments);
            }
            return View();
        }

        [HttpPost]
        public IActionResult Approve(int id)
        {
            // Perform the approval logic here, e.g., calling an API to approve the comment
            string url = $"https://localhost:44342/api/Comments/ApproveComments/{id}";
            HttpResponseMessage response = client.PostAsync(url, null).Result;

            if (response.IsSuccessStatusCode)
            {
                // Redirect back to the content moderation page
                return RedirectToAction("ContentModeration");
            }
            else
            {
                // Handle the error (e.g., display an error message)
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult DeleteComment(int id)
        {

            string url = $"https://localhost:44342/api/Comments/DeleteComments/{id}";
            HttpResponseMessage response = client.DeleteAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                // Redirect back to the content moderation page
                return RedirectToAction("ContentModeration");
            }
            else
            {
                // Handle the error (e.g., display an error message)
                return View("Error");
            }

        }


        //-------------------------------------------------  User Management
        public IActionResult UserManagement()
        {
            // api call  
            string url = "https://localhost:44342/api/Auth/GetAllUsers";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata = response.Content.ReadAsStringAsync().Result;
                var users = JsonConvert.DeserializeObject<List<Users>>(jsondata);
                return View(users);
            }
            return View();
        }

        [HttpPost]
        public IActionResult BlockUser(string email)
        {
            using (var client = new HttpClient())
            {
                string url = $"https://localhost:44342/api/UserMangment/BlockUser?email={Uri.EscapeDataString(email)}";

                var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    // Redirect back to the content moderation page
                    return RedirectToAction("UserManagement");
                }
                else
                {
                    // Handle the error (e.g., display an error message)
                    return View("Error");
                }
            }
        }

		//------------------------------------------------ Course Management
		public IActionResult CourseManagement()
		{
			// API call
			string url = "https://localhost:44342/api/ManageCourse/GetCourseName";
			HttpResponseMessage response = client.GetAsync(url).Result;
			if (response.IsSuccessStatusCode)
			{
				string jsondata = response.Content.ReadAsStringAsync().Result;
				var courses = JsonConvert.DeserializeObject<List<string>>(jsondata);
				return View(courses);
			}
			return View(new List<string>());
		}

        // getSub course by CourseName
        //https://localhost:44342/api/ManageCourse/GetSubCourseBYCorurse
        [HttpGet]
        public IActionResult GetSubCourseBYCorurse(string CourseName)
        {
            // api operation
            string url = $"https://localhost:44342/api/ManageCourse/GetSubCourseBYCorurse/{CourseName}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata = response.Content.ReadAsStringAsync().Result;
                var subcourses = JsonConvert.DeserializeObject<List<AddCourseContent>>(jsondata);
                return View(subcourses);
            }
            return View();
        }


        // Update subCourse 
        [HttpPost]
        public IActionResult Edit(int id, string Title, string MetaUrl , string SubCourse , string CourseName,string Question)
        {
        
            var subcourse = new AddCourseContent
            {
                Id = id,
                Title = Title,
                MetaUrl = MetaUrl,
                SubCourse = SubCourse,
                CourseName = CourseName,
                Question = Question
            };
            if (ModelState.IsValid)
            {
                var url = "https://localhost:44342/api/ManageCourse/UpdateSubCourse";
                var jsonData = JsonConvert.SerializeObject(subcourse);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = client.PutAsync(url, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("GetSubCourseBYCorurse", new { CourseName = subcourse.CourseName });
                }
            }
            return View(subcourse);
 
        }


        // delete SubCourse 
        [HttpPost]
        public IActionResult Delete(int id,string CourseName)
        {
            var url = $"https://localhost:44342/api/ManageCourse/DeleteSubCourse/{id}";
            var response = client.DeleteAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetSubCourseBYCorurse", new { CourseName = CourseName });
            }
            return View();
        }

        [HttpPost]
		public IActionResult DeleteCourse(int id)
		{
			string url = $"https://localhost:44342/api/Course/DeleteCourse/{id}";
			HttpResponseMessage response = client.DeleteAsync(url).Result;

			if (response.IsSuccessStatusCode)
			{
				// Redirect back to the content moderation page
				return RedirectToAction("CourseManagement");
			}
			else
			{
				// Handle the error (e.g., display an error message)
				return View("Error");
			}
		}

		//public IActionResult EditCourse(int id)
		//{
		//	string url = $"https://localhost:44342/api/Course/GetCourseById/{id}";
		//	HttpResponseMessage response = client.GetAsync(url).Result;
		//	if (response.IsSuccessStatusCode)
		//	{
		//		string jsondata = response.Content.ReadAsStringAsync().Result;
		//		var course = JsonConvert.DeserializeObject<Course>(jsondata);
		//		return View(course);
		//	}
		//	return View();
		//}
 
 
    
    }
}
