using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class MHDiagnosisVM
    {
        public int Id { get; set; }
        public Guid DependantID { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("ICD-10 code")]
        public string ICD10Code { get; set; }

        [DisplayName("Mapping code")]
        public string MappingCode { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Formulary code")]
        public string ForumlaryCode { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EffectiveDate { get; set; }

        [DisplayName("End date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }

        [DisplayName("Comment")]
        public string Comment { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}