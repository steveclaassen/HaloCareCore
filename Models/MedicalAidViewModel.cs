using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class MedicalAidViewModel
    {
        public MedicalAid medicalAid { get; set; }

        public MedicalAidSettings medicalAidSettings { get; set; }

        public MedicalAidAccount MedicalAidAccount { get; set; }
        public List<MedicalAidAccount> MedicalAidAccounts { get; set; }

        public List<MedicalAidProgramViewModel> Programs { get; set; }//HCare-1087
        public MedicalAidClaimCode MedicalAidClaimCodes { get; set; }//HCare-1086
        public List<Referral> referrals { get; set; }
        public List<ReferralFrom> referralFrom { get; set; }

    }
}