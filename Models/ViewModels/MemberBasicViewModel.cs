using HaloCareCore.Models.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class MemberBasicViewModel
    {
        public Guid MedicalAidID { get; set; }
        public Guid dependantId { get; set; }
        public string membershipNo { get; set; }
        public string dependentCode { get; set; }
        public string fistname { get; set; }
        public string lastname { get; set; }
        public string title { get; set; }
        public string birthDate { get; set; }
        public string IDNo { get; set; }
        public string Scheme { get; set; }
        public string Option { get; set; }
        public string EmployerGroup { get; set; }
        public Guid programID { get; set; }
        public string programCode { get; set; }
        public string programName { get; set; }
        public bool hiv { get; set; }
        public Guid hivK { get; set; }
        public bool diab { get; set; }
        public Guid diabK { get; set; }
        public bool MentalHealth { get; set; }
        public Guid MentalHealthK { get; set; }



    }
}