using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Communications
{
    public class PathologyAutomatedTemplate
    {
        public Guid Id { get; set; }

        [DisplayName("Template")]
        public string Template { get; set; }

        [DisplayName("Medical aid")]
        public List<MedicalAid> MedicalAids { get; set; }
        public string selectedSchemes { get; set; }

        [DisplayName("Option(s)")]
        public List<MedicalAidPlans> Options { get; set; }
        public string selectedOptions { get; set; }

        [DisplayName("Program")]
        public List<Programs> Programs { get; set; }
        public string selectedPrograms { get; set; }

        [DisplayName("Management status")]
        public List<ManagementStatus> ManagmentStatus { get; set; }
        public string selectedManagmentStatus{ get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }
        
        [DisplayName("Active")]
        public bool Active { get; set; }
        [NotMapped]
        public List<ManagementStatus> ManagmentSatus { set; get; }

    }
}