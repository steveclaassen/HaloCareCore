using HaloCareCore.Models.Management;
using HaloCareCore.Models.Script;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class ScriptCreationViewModel
    {
        public Scripts script { get; set; }
        public List<Scripts> scripts { get; set; }

        public ScriptViewModel item { get; set; }
        public List<ScriptViewModel> items { get; set; }
        public List<ScriptViewModel> DiabetesItems { get; set; }
        public List<ScriptViewModel> HIVItems { get; set; }
        public List<ScriptViewModel> MHItems { get; set; }

        public List<ScriptItems> theScriptItems { get; set; }

        public DoctorViewModel doctor { get; set; }
        public List<Products> products { get; set; }

        [DisplayName("Current script")]
        public int currentScript { get; set; }
    }
}