using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class MedicalAidVM
    {
        public Guid Id{ get; set; }

        public Guid MedicalAidId { get; set; }

        [DisplayName("Account")]
        public string MedicalAidCode { get; set; } //hcare-1346

        [DisplayName("Medical scheme")]
        public string MedicalAidName { get; set; }

        [Required]
        [DisplayName("Plan code")]
        public string PlanCode { get; set; }

        [Required]
        [DisplayName("Plan name")]
        public string PlanName { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        public bool CLCarrier { get; set; } //hcare-1346
        public bool UserAccess { get; set; } //hcare-1346

        public List<MedicalAid> MedicalAids { get; set; }


    }
}
