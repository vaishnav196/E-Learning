using Microsoft.AspNetCore.Mvc;
using E_Learning.Models.Auth;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
namespace E_Learning.Controllers
{
    public class AuthController : Controller
    {

        private readonly HttpClient client;
        public AuthController() {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            client = new HttpClient(clientHandler);
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Users u )
        {
            string url = "https://localhost:44342/api/Auth/AddUser";
            var jsondata = JsonConvert.SerializeObject(u);
            StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }


        [HttpPost]
        public IActionResult Login(LoginRequest lg)
        {
            var url = $"https://localhost:44342/api/Auth/Login?Email={lg.Email}&password={lg.Password}";
            var json = JsonConvert.SerializeObject(lg);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, data).Result;

            if (response.IsSuccessStatusCode)
            {
                var userData = response.Content.ReadAsStringAsync().Result;
                var user = JsonConvert.DeserializeObject<Users>(userData);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                
                };

                if (user.Role == "Admin")
                {
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait();

                    HttpContext.Session.SetString("Username", user.Name);
                    //
                    return RedirectToAction("Index", "Admin");
                }
                else if (user.Role == "user")
                {
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait();

                    HttpContext.Session.SetString("Username", user.Name);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    //
                    return RedirectToAction("Index", "HomePage");
                }
                else
                {
                    return Unauthorized("Invalid role");
                }

            }
            return View();
        }


 
        // ---------------------------------------- AUTH
        public async Task GoogleLogin()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (result?.Principal != null)
            {
                var claims = result.Principal.Identities.FirstOrDefault().Claims;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
              
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var phoneNumber = claims.FirstOrDefault(c => c.Type == "phone_number")?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    HttpContext.Session.SetString("UserEmail", email);
                    
                }
                 // TODO 
                // if doesnt exist then register it
               
                // Assuming you have a method to get the mobile number, if required.
                // var mobile = GetMobileNumberForUser(email);
                // if (!string.IsNullOrEmpty(mobile))
                // {
                //     HttpContext.Session.SetString("Mobile", mobile);
                // }

                // Redirect to HomeController/Index
                return RedirectToAction("Index", "HomePage");
            }

            return RedirectToAction("Login", "Auth");
        }


        public IActionResult SignOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var storedcookies = Request.Cookies.Keys;
            foreach (var cookie in storedcookies)
            {
                Response.Cookies.Delete(cookie);
            }
            return RedirectToAction("Login", "Auth");
        }




    }

}
