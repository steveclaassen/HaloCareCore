using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using HaloCareCore.DAL;
using HaloCareCore.Repos;
using Microsoft.Extensions.Configuration;

namespace HaloCareCore.Filters
{
    public class CustomActionFilter : ActionFilterAttribute, IActionFilter
    {
        private IAdminRepository _admin;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private readonly IConfiguration Configuration;

        public CustomActionFilter(OH17Context context)
        {
            _admin = new AdminRepository(context, Configuration);
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (_session.GetString("sessionid") == null)
                _session.SetString("sessionid", "empty");

            if (_session.GetString("userid") == null)
                _session.SetString("userid", "empty");

            // check to see if your ID in the Logins table has 
            // LoggedIn = true - if so, continue, otherwise, redirect to Login page.
            if (_admin.IsYourLoginStillTrue
            (_session.GetString("userid").ToString(), _session.GetString("sessionid").ToString()))
            {
                // check to see if your user ID is being used elsewhere under a different session ID
                if (!_admin.IsUserLoggedOnElsewhere
                (_session.GetString("userid").ToString(), _session.GetString("sessionid").ToString()))
                {
                }
                else
                {
                    // if it is being used elsewhere, update all their 
                    // Logins records to LoggedIn = false, except for your session ID
                    _admin.LogEveryoneElseOut
                    (_session.GetString("userid").ToString(), _session.GetString("sessionid").ToString());
                }
            }
            else
            {
                //FormsAuthentication.SignOut();
                RouteValueDictionary redirectTargetDictionary = new RouteValueDictionary();
                redirectTargetDictionary.Add("action", "Login");
                redirectTargetDictionary.Add("controller", "Account");
                redirectTargetDictionary.Add("area", "");

                filterContext.Result = new RedirectToRouteResult(redirectTargetDictionary);
            }
        }
    }
}