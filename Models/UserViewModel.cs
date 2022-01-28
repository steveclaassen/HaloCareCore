using HaloCareCore.Models.Management;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class UserViewModel
    {
        public Users user { get; set; }
        public ForgottenPassword forgottenPassword { get; set; }

        [DisplayName("User role")]
        public string userRole { get; set; }

    }
}