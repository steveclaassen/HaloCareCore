using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class UserCreationCreateModel
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
        [DisplayName("Scheme Access")]
        public List<MedicalAid> medicalAids { get; set; }
        public string[] selectedSchemes { get; set; }

        [DisplayName("Scheme Programs")]
        public List<Programs> schemePrograms { get; set; }
        public string[] selectedPrograms { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        public bool SearchManagement { get; set; } //hcare-1289
        public bool BulkEmail { get; set; } //hcare-1483


        //HCare-996
        public AccessFunctions roleViews { set; get; }
        public List<Functions> functions { set; get; }
    }
}