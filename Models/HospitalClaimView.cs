using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class HospitalClaimView
    {
        [DisplayName("Admission date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? adminDate { get; set; }

        [DisplayName("Discharge date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? dischargeDate { get; set; }

        [DisplayName("Vendor")]
        public string vendor { get; set; }

        [DisplayName("Provider")]
        public string provider { get; set; }

        [DisplayName("Admission ICD-10")]
        public string diagnosis { get; set; }

        [DisplayName("Discharge ICD-10")]
        public string finalDiagnosis { get; set; }

        [DisplayName("Procedure code")]
        public string procedureCode { get; set; }

        [DisplayName("Doctor Full Names ")]
        public string Fullname { set; get; }

        [DisplayName("ICD-10 description ")]
        public string AdminCD10Description { set; get; }

        [DisplayName("ICD-10 description ")]
        public string FinalCD10Description { set; get; }
    }
}