using HaloCareCore.Models.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class AuthorisationViewModel
    {
        public EnrolmentViewModel memberinformation { get; set; }
        public Scripts script { get; set; }
        public List<ScriptViewModel> scriptItems { get; set; }
    }
}