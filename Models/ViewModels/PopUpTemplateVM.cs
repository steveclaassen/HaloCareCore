using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class PopUpTemplateVM
    {
        public string program_pop_type { get; set; }
        public string profile_pop_type { get; set; }
        public string program_pop_title { get; set; }
        public string profile_pop_title { get; set; }
        public string program_pop_template { get; set; }
        public string profile_pop_template { get; set; }

        public string program_pop_checkbox { get; set; }
        public string profile_pop_checkbox { get; set; }
        
        public string medicalAid { get; set; }
        public string option { get; set; }
        public string programID { get; set; }
        public string programName { get; set; }
    }
}
