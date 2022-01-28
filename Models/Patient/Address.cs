using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class Address
    {
        [Key]
        public Guid addressID { get; set; }

        [DisplayName("Building")]
        public string line1 { get; set; }

        [DisplayName("Street")]
        public string line2 { get; set; }

        [DisplayName("Suburb")]
        public string line3 { get; set; }

        [DisplayName("City")]
        public string city { get; set; }

        [DisplayName("Province")]
        public string province { get; set; }

        [DisplayName("Postal code")]
        public string zip { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

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

        [DisplayName("Active")]
        public bool Active { get; set; }

        [DisplayName("Address type")]
        public string AddressType { get; set; }

        [DisplayName("iSeries Building")]
        public string ISeriesLine1 { get; set; }

        [DisplayName("iSeries Street")]
        public string ISeriesLine2 { get; set; }

        [DisplayName("iSeries Suburb")]
        public string ISeriesLine3 { get; set; }

        [DisplayName("iSeries City")]
        public string ISeriesCity { get; set; }

        [DisplayName("iSeries Province")]
        public string ISeriesProvince { get; set; }

        [DisplayName("iSeries Postal code")]
        public string ISeriesZip { get; set; }
    }
}