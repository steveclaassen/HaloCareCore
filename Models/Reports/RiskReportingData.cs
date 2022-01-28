using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Reports
{
    public class RiskReportingData
    {
        public Guid id { get; set; }

        [DisplayName("Year")]
        public int year { get; set; }

        [DisplayName("Month")]
        public int month { get; set; }

        [DisplayName("Area")]
        public string area { get; set; }

        [DisplayName("Source")]
        public string source { get; set; }

        [DisplayName("Name")]
        public string name { get; set; }

        [DisplayName("Subname")]
        public string subName { get; set; }

        [DisplayName("Field")]
        public string fieldName { get; set; }

        [DisplayName("Other")]
        public string other { get; set; }

        [DisplayName("Created date")]
        public DateTime createdDate { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        public int? valueInt { get; set; }

        public double? valueDouble { get; set; }
    }
}