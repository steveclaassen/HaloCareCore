using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class UserEditViewModel
    {
        public Guid userID { get; set; }
        [DisplayName("Username")]
        public string username { get; set; }
        [DisplayName("First name")]
        public string firstname { get; set; }
        [DisplayName("Last name")]
        public string lastname { get; set; }
        [DisplayName("Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Invalid Email Address")]
        public string email { get; set; }
        [DisplayName("Password")]
        public string password { get; set; }

        [DisplayName("User Role")]
        public List<Roles> roles { get; set; }
        public Guid selectedRole { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
        
        [DisplayName("Search management")]
        public bool SearchManagement { get; set; } //hcare-1289

        [DisplayName("Bulk email")]
        public bool BulkEmail { get; set; } //hcare-1483

        [DisplayName("Schemes")]
        public List<UserSchemeAccessViewModel> schemes { get; set; }
        public List<Guid> selectedSchemes { get; set; }

        [DisplayName("Programs")]
        public List<UserProgramViewModal> programs { get; set; }
        public List<Guid> selectedPrograms { get; set; }

        //HCare-996

        public AccessFunctions roleViews { set; get; }
        public List<Functions> functions { set; get; }
        public List<AccessFunctions> roleViewss { set; get; }

    }
}