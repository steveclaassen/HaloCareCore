using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class MedicationHistory
    {
        [Key]
        public int Id { get; set; }

        public Guid dependantId { get; set; }

        [DisplayName("NAPPI code")]
        public string nappiCode { get; set; }

        [DisplayName("Product name")]
        public string productName { get; set; }

        [DisplayName("Medication instructions")]
        public string directions { get; set; }

        [DisplayName("Comment")]
        public string comment { get; set; }

        [DisplayName("Treatment Naive")]
        public bool treatmentNaive { get; set; }

        [DisplayName("Start date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? startDate { get; set; }

        [DisplayName("End date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? endDate { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifieddate { get; set; }

        [DisplayName("Program")]
        public string programType { get; set; }

        [DisplayName("Follow up")]
        public bool followUp { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
        
        //Navigational Properties
        public int? QuestionnaireID { get; set; }
        public bool hasMedication { set; get; }

    }
}