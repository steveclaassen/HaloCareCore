using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Script
{
    public class Products
    {
        [Key]
        [Required]
        [DisplayName("NAPPI")]
        public string nappiCode { get; set; }

        [Required]
        [DisplayName("Description")]
        public string productName { get; set; }

        [Required]
        [DisplayName("Pack size")]
        public int packSize { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool Active { get; set; }

        public bool Insulin { get; set; }

        public bool Sulponylureas { get; set; }

        [DisplayName("ATC code")]
        public string atcCode { get; set; }
        [DisplayName("Therapeutic class description")]
        public string therapeuticClass6Desc { set; get; }

        [DisplayName("Therapeutic class 6")]
        public int therapeuticClass6 { get; set; }

        [DisplayName("MMAP indicator")]
        public string mmapIndicator { set; get; }

        [DisplayName("MRP indicator")]
        public string mrpIndicator { get; set; }

        [DisplayName("Pack uom")]
        public string packUOM { get; set; }

        [DisplayName("Generic indicator ")]
        public string genericIndicator { get; set; }

        [DisplayName("Schedule")]
        public string productSchedule { set; get; }

        [DisplayName("Strength")]
        public string strength { get; set; }

        public string productStatus { set; get; }

        [NotMapped]
        public string oldNappicode { set; get; }
        [DisplayName("Therapeutic class 12")]
        public string therapeuticClass12 { get; set; }
        [DisplayName("Dosage form")]
        public string dosageForm { get; set; }
    }
}