using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System.Collections.Generic;

namespace HaloCareCore.Models.ViewModels
{
    public class EnquirySearchVM
    {
        public EnquirySearch EnquirySearch { get; set; }
        public List<EnquiryResultsVM> EnquirySearches { get; set; }

        public List<MedicalAid> MedicalAids { get; set; }
        public string[] SelectedMedicalAids { get; set; }

        public List<Programs> Programs { get; set; }
        public string[] SelectedPrograms { get; set; }

        public List<QueryTypes> QueryTypes { get; set; }
        public string[] SelectedQueryTypes { get; set; }

        public List<Priorities> QueryPriorities { get; set; }
        public string[] SelectedQueryPriorities { get; set; }

        public List<Validation.ManagementStatus> ManagementStatus { get; set; }
        public string[] SelectedManagementStatus { get; set; }

        public bool filter { get; set; }

    }
}
