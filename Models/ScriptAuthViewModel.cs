using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class ScriptAuthViewModel
    {
        public Scripts script { get; set; }
        public List<ScriptViewModel> items { get; set; }
        public DoctorViewModel doctor { get; set; }
        public List<Products> products { get; set; }
        public List<Pathology> pathology { get; set; } 
        public EnrolmentViewModel member { get; set; }
        public List<Clinical> clinicals { get; set; }
        public List<ScriptViewModel> scriptItems { get; set; }
    }
}