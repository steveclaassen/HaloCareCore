using HaloCareCore.Models;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Service;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Repos
{
    public interface IMedicalAidRepository: IDisposable
    {
        Guid InsertMedicalAid(MedicalAid medical);
        bool InsertMedicalAidSettings(MedicalAidSettings medical);
        bool UpdateMedicalAid(MedicalAid medical);
        bool UpdateMedicalAidSettings(MedicalAidSettings medical);
        MedicalAidViewModel GetMedicalAid(Guid medicalaidID);
        MedicalAidViewModel GetMedicalAidDetails(Guid medicalaidID);
        MedicalAidViewModel GetMedicalAid(string medicalAidCode);
        MedicalAidViewModel GetMedicalAidName(string medicalAid);
        List<MedicalAidViewModel> GetMedicalAids();
        ServiceResult updateQueries_MedicalAid(Queries model);
        List<MedicalAidClaimCode> GetMedicalAidCode(string medicalAidCode); //HCare-1086
        void InsertMedicalAidClaimingCode(MedicalAidClaimCode medicalAidClaimCode);
        bool UpdateMedicalAidClaimingCode(MedicalAidClaimCode medicalAidClaimCode);
        bool UpdateMedicalAidClaimCode(MedicalAidClaimCode medicalAidClaimCode);
        string GetClaimCode(string claimcode);//Hcare-1086

        MedicalAid GetMedicalAidByCode(string code); //HCare-956
        MedicalAid GetMedicalAidByName(string name); //HCare-956
        MedicalAidClaimCode GetMedicalClaimCodeByCode(string name);//HCare1216
        MedicalAidPlans GetMedicalAidPlanByCode(string code); //HCare-956
        MedicalAidPlans GetMedicalAidPlanByName(string name, string medicalaidID); //HCare-956






    }
}