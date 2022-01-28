using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class ManagementStatusSearch
    {
        [DisplayName("Scheme")]
        public string MedicalScheme { get; set; }

        [DisplayName("Member number")]
        public string MembershipNumber { get; set; }

        [DisplayName("Dep code")]
        public string DependantCode { get; set; }

        [DisplayName("ID/Auth #")]
        public string IDNumber { get; set; }

        [DisplayName("Patient name")]
        public string PatientName { get; set; }

        [DisplayName("Program code")]
        public string ProgramCode { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("ICD-10")]
        public string ICD10 { get; set; }

        [DisplayName("Program start date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ProgramStartDate { get; set; }
        
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EffectiveDate { get; set; }

        public string StatusTypePrevious { get; set; }
        public string StatusCodePrevious { get; set; }

        [DisplayName("Managment status previous")]
        public string ManagementStatusPrevious { get; set; }

        [DisplayName("Start date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ManagementStatusPrevious_StartDate { get; set; }

        [DisplayName("End date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ManagementStatusPrevious_EndDate { get; set; }

        public string StatusTypeCurrent { get; set; }
        public string StatusCodeCurrent { get; set; }

        [DisplayName("Managment status current")]
        public string ManagementStatusCurrent { get; set; }

        [DisplayName("Start date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ManagementStatusCurrent_StartDate { get; set; }

        [DisplayName("End date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ManagementStatusCurrent_EndDate { get; set; }

        //--
        public Guid DependantID { get; set; }
        public Guid MedicalAidID { get; set; }
        public Guid ProgramID { get; set; }



        public bool Active { get; set; }

    }
}
