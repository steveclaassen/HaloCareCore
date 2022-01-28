using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class AccountSetupStep2ViewModal
    {
        public Guid accountId { get; set; }
        [DisplayName("Scheme Options")]

        public List<MedicalAidSetupViewModal> medicalAids { get; set; }
    }

    public class MedicalAidSetupViewModal
    {
        public Guid Id { get; set; }
        public Guid medicalAidID { get; set; }
        [DisplayName("Scheme Name")]
        public string Name { get; set; }
        [DisplayName("Allow Authorisations")]
        public bool allowAuth { get; set; }
        [DisplayName("Scheme Programs")]
        public List<Programs> schemePrograms { get; set; }
        public string[] selectedPrograms{ get; set; }
        [DisplayName("Scheme Options")]
        public List<MedicalAidPlans> schemePlans { get; set; }
        public string[] selectedSchemeOptions { get; set; }
        public string status { get; set; }
    }
}