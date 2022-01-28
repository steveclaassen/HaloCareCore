using HaloCareCore.Models.Communications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class AuthViewDetailModel
    {
        public EnrolmentViewModel member { get; set; }
        public Assignments assignment { get; set; }
        public AssignmentItems items { get; set; }
    }
}