using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class EmailTemplatesViewModel
    {
        public EmailTemplates emailTemplates { set; get; }
        public Programs program { set; get; }
        public List<Programs> programs { set; get; }
    }
}