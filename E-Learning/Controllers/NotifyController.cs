using E_Learning.Models.Auth;
using E_Learning.Models.Course;
using E_Learning.Models.Notification;
using E_Learning.Models.Promotion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace E_Learning.Controllers
{
    [Authorize]
    public class NotifyController : Controller
    {

        HttpClient client;
        public NotifyController()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            client = new HttpClient(clientHandler);
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNotification(Notification notification)
        {
            string url = "https://localhost:44342/api/Notification/PostNotification";
            var jsondata = JsonConvert.SerializeObject(notification);
            StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                // Fetch users
                var users = await client.GetFromJsonAsync<IEnumerable<Users>>("https://localhost:44342/api/Auth/GetAllUsers");

                if (users != null)
                {
                    var emailRequest = new EmailRequest
                    {
                        ToEmails = users.Select(u => u.Email).ToList(),
                        Subject = "New Notification: " + notification.Title,
                        Body = $@"
                    <p>Dear User,</p>
                    <p>We are excited to bring to your attention a new notification: <strong>{notification.Title}</strong>.</p>
                    <p><strong>Details:</strong></p>
                    <p>{notification.Content}</p>
                    <p>Best regards,<br>The Masstech Team</p>
                    <p><strong>P.S.</strong> Have questions? Feel free to reply to this email or visit our FAQ page/Contact Us page for more information.</p>
                        "
                    };

                    var emailResponse = await client.PostAsJsonAsync("https://localhost:44342/api/Email/send", emailRequest);

                    if (emailResponse.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Notification created and email sent successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Notification created but failed to send email.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Notification created but failed to fetch users.";
                }

                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Failed to create notification.";
            return View(notification); // Return the model to preserve input
        }

        //[HttpPost]
        //public IActionResult AddNotification(Notification notification)
        //{
        //    string url = "https://localhost:44342/api/Notification/PostNotification";
        //    var jsondata = JsonConvert.SerializeObject(notification);
        //    StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
        //    HttpResponseMessage response = client.PostAsync(url, content).Result;

        //    if (response.IsSuccessStatusCode)
        //    {
        //        TempData["SuccessMessage"] = "Notification created successfully!";
        //        return RedirectToAction("Index");
        //    }

        //    TempData["ErrorMessage"] = "Failed to create notification.";
        //    return View(notification); // Return the model to preserve input
        //}



        public IActionResult AddPromotion()
        {
            return View();
        }


        [HttpPost]
        public IActionResult CreatePromotion(Promotion promotion)
        {
            string url = "https://localhost:44342/api/Promotion/PostPromotion";
            var jsondata = JsonConvert.SerializeObject(promotion);
            StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Promotion created successfully!";
                return RedirectToAction("AddPromotion");
            }

            TempData["ErrorMessage"] = "Failed to create promotion.";
            return View(promotion); // Return the model to preserve input
        }



    }
}
