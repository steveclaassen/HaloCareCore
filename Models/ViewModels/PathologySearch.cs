using HaloCareCore.Models.Patient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class PathologySearch
    {

        [DisplayName("Scheme")]
        public string MedicalAidScheme { get; set; }

        public Guid MedicalAidID { get; set; }

        [DisplayName("Program")]
        public string ProgramName { get; set; }

        [DisplayName("Pathology field")]
        public string PathologyField { get; set; }

        [DisplayName("From date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FromDate { get; set; }

        [DisplayName("To date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ToDate { get; set; }

        //-->> search results

        public Guid DependantID { get; set; }

        public int PathologyID { get; set; }

        [DisplayName("Member #")]
        public string MembershipNumber { get; set; }

        [DisplayName("Dep code")]
        public string DependantCode { get; set; }

        [DisplayName("ID/Auth")]
        public string IDNumber { get; set; }

        [DisplayName("First name")]
        public string FirstName { get; set; }

        [DisplayName("Last name")]
        public string LastName { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EffectiveDate { get; set; }

        [DisplayName("Pathology lab")]
        public string PathologyLab { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        public decimal? CD4count { get; set; }
        public DateTime? CD4countEffectiveDate { get; set; }

        public decimal? CD4percentage { get; set; }
        public DateTime? CD4percentageEffectiveDate { get; set; }

        public decimal? Viralload { get; set; }
        public DateTime? ViralloadEffectiveDate { get; set; }

        public decimal? Haemoglobin { get; set; }
        public DateTime? HaemoglobinEffectiveDate { get; set; }

        public decimal? Bilirubin { get; set; }
        public DateTime? BilirubinEffectiveDate { get; set; }

        public decimal? TotalCholestrol { get; set; }
        public DateTime? TotalCholestrolEffectiveDate { get; set; }

        public decimal? HDL { get; set; }
        public DateTime? HDLEffectiveDate { get; set; }

        public decimal? LDL { get; set; }
        public DateTime? LDLEffectiveDate { get; set; }

        public decimal? Triglycerides { get; set; }
        public DateTime? TriglyceridesEffectiveDate { get; set; }

        public decimal? Glucose { get; set; }
        public DateTime? GlucoseEffectiveDate { get; set; }

        public decimal? HbA1c { get; set; }
        public DateTime? HbA1cEffectiveDate { get; set; }

        public decimal? ALT { get; set; }
        public DateTime? ALTEffectiveDate { get; set; }

        public decimal? AST { get; set; }
        public DateTime? ASTEffectiveDate { get; set; }

        public decimal? Urea { get; set; }
        public DateTime? UreaEffectiveDate { get; set; }

        public decimal? Creatinine { get; set; }
        public DateTime? CreatinineEffectiveDate { get; set; }

        public decimal? eGFR { get; set; }
        public DateTime? eGFREffectiveDate { get; set; }

        public decimal? UAlbumintoCreatratio { get; set; }
        public DateTime? UAlbumintoCreatratioEffectiveDate { get; set; }

        public decimal? systolicBP { get; set; }
        public DateTime? systolicBPEffectiveDate { get; set; }
        public decimal? diastolicBP { get; set; }
        public DateTime? diastolicBPEffectiveDate { get; set; }

        public decimal? FEV1 { get; set; }
        public DateTime? FEV1EffectiveDate { get; set; }

        public decimal? Eosinophylia { get; set; }
        public DateTime? EosinophyliaEffectiveDate { get; set; }

        public string HIVElisa { get; set; }
        public DateTime HIVElisaEffectiveDate { get; set; }

        public string NormalGTT { get; set; }
        public DateTime NormalGTTEffectiveDate { get; set; }

        public string AbnormalGTT { get; set; }
        public DateTime AbnormalGTTEffectiveDate { get; set; }

        public decimal? UCreatinine { get; set; }
        public DateTime? UCreatinineEffectiveDate { get; set; }

        public decimal? SAlbumin { get; set; }
        public DateTime? SAlbuminEffectiveDate { get; set; }

        public decimal? UAlbuminuria { get; set; }
        public DateTime? UAlbuminuriaEffectiveDate { get; set; }

        public List<PathologyFields> pathologyFields { get; set; }


    }
}
