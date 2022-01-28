using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class reportHIV
    {
        public Dependant dependant { get; set; }
        public Contact contact { get; set; }
        public List<Programs> program { get; set; }
        public List<ManagementStatus> managementStatus { get; set; }
        public List<MedicalAid> medicalAid { get; set; }
        public List<CaseManagers> caseManagerName { get; set; }

        [DisplayName("Member #")]
        public string membershipNumber { get; set; }

        [DisplayName("Doctor")]
        public string doctorName { get; set; }    
        

    }


}