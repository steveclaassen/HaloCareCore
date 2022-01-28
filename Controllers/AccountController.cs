using HaloCareCore.DAL;
using HaloCareCore.Models;
using HaloCareCore.Models.Management;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Security.Claims;

namespace HaloCareCore.Controllers
{
    public class AccountController : Controller
    {
        private IAdminRepository _admin;
        private IClinicalRepository _clinical;
       // private IMemberRepository _member;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;

        public AccountController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, OH17Context context)
        {
            _admin = new AdminRepository(context, configuration);
            _clinical = new ClinicalRepository(context, configuration);
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        [AllowAnonymous]
        public ActionResult Login()
        {
            var model = new UserLoginModel();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Login(UserLoginModel model)
        {

            if (ModelState.IsValid)
            {
                if (_admin.ValidateUser(model.Username, model.Password))
                {
                    _session.SetString("SessionId", _httpContextAccessor.HttpContext.Session.Id);

                    Logins login = new();
                    var userInfo = _admin.GetUserByUsername(model.Username);

                    login.UserId = userInfo.userID;
                    _session.SetString("Init", userInfo.Firstname.Substring(0, 1) + userInfo.Lastname.Substring(0, 1));
                    _session.SetString("fullName", userInfo.Firstname + " " + userInfo.Lastname);
                    _session.SetString("emailAddress", userInfo.email);
                    _session.SetString("userName", userInfo.username);

                    _session.SetString("userid", login.UserId.ToString());
                    login.SessionId = _session.GetString("SessionId");
                    login.LoggedIn = true;
                    _admin.CheckSessionLogin(login);
                    var claims = new[] { new Claim(ClaimTypes.Name, model.Username),
                            new Claim(ClaimTypes.Role, "SomeRoleName") };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var props = new AuthenticationProperties();
                    props.IsPersistent = model.RememberMe;

                    HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity));

                    //FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);

                    //return RedirectToAction("Index", "Home", new { Area = "HaloCare" });

                    _session.SetString("Role", _admin.GetUserInRole(model.Username).FirstOrDefault()); //HCare-1250
                    //_session.SetString("AllRoles", _admin.GetUserRoles());//HCare-1250

                    //List<AccessFunctionsViewModel> accessFunctions = new List<AccessFunctionsViewModel>();


                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Could not log in. Invalid credentials used or you do not have access to this system");
                }
            }



            return View(model);
        }

    }
}
