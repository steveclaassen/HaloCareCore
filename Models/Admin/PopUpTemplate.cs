using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Admin
{
    public class PopUpTemplate
    {
        public Guid Id { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Title")]
        public string Title { get; set; }

        [DisplayName("Template")]
        public string Template { get; set; }

        [DisplayName("Medical scheme")]
        public List<MedicalAid> MedicalAids { get; set; }
        public string selectedSchemes { get; set; }

        [DisplayName("Option")]
        public List<MedicalAidPlans> Options { get; set; }
        public string selectedOptions { get; set; }

        [DisplayName("Program")]
        public List<Programs> Programs { get; set; }
        public string selectedPrograms { get; set; }


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
    }
}
