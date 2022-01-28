using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class ComorbidView
    {
        public int id { get; set; }
        public string coMorbidId { get; set; }
        public Guid dependantID { get; set; }

        [DisplayName("Program")]
        public string programType { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? effectiveDate { get; set; }

        [DisplayName("CoMorbid treatment")]
        public string CoMorbidTreatement { get; set; }

        [DisplayName("Treatment end date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? treatementEndDate { get; set; }

        [DisplayName("Diagnosis date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? diagnosisDate { get; set; }

        [DisplayName("ICD-10")] //HCare-958
        public string ICD10Code { get; set; }

        [DisplayName("Comment")]
        public string generalComments { get; set; }

        [DisplayName("coMorbid condition")]
        public bool hasCoMorbid { get; set; }

        [DisplayName("Follow up")]
        public bool followUp { get; set; }
    }
}