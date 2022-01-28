using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class AccountVM
    {
        public Guid AccountID { get; set; }

        [DisplayName("System account name")]
        public string Name { get; set; }

        [DisplayName("System account description")]
        public string Description { get; set; }

        public List<Accounts> Accounts { get; set; }

        public List<MedicalAid> MedicalAids { get; set; }
        public string[] SelectedMedicalAids { get; set; }

        public List<Programs> Programs { get; set; }
        public string[] SelectedPrograms { get; set; }

        public List<MedicalAidPlans> Options { get; set; }
        public string[] SelectedOptions { get; set; }


    }
}
