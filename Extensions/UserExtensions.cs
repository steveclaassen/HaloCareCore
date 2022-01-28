using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace HaloCareCore.Extensions
{
    public static class UserExtensions
    {
        //public static AdminRepository _admin = new AdminRepository(context);
        //public static MemoryCache roleCache = new MemoryCache("Role");

        public static bool HasRole(this IPrincipal user, string role)
        {
            try
            {
                var _roles = new List<string>();
                //_roles = (List<string>)HttpContext.Current.Session["Roles"];

                if (_roles != null)
                {
                    if (_roles.Contains(role))
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //public static MemoryCache accessCache = new MemoryCache("Access");

        //HCare-889
        public static bool RoleHasAccess(this IPrincipal user)
        {
            try
            {
                //var usrRole = new UserRole();
                //if (HttpContext.Session["userid"].ToString() != null)
                //{
                //    usrRole = _admin.GetUserRoleById(Guid.Parse(_session.GetString("userid"].ToString()));
                //}
                var role = false;
                //var roles = _admin.GetUserRoles();
                //var roles = (List<Roles>)HttpContext.Current.Session["AllRoles"];
                //role = roles.Where(x => x.Id == usrRole.roleId).Select(x => x.adminRights).FirstOrDefault();
                //
                return role;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}