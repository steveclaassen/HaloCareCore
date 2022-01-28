using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Communications
{
    public class Queries
    {
        [Key]
        [Required]
        [DisplayName("Query ID")]
        public int queryID { get; set; }

        [Required]
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [Required]
        [DisplayName("Query type")]
        public string queryType { get; set; }

        [Required]
        [DisplayName("Enquiry by")]
        public string enquiryBy { get; set; }

        [DisplayName("Query subject")]
        public string querySubject { get; set; }

        [Required]
        [DisplayName("Message")]
        public string query { get; set; }

        [DisplayName("Solution")]
        public string querySolution { get; set; }

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

        [DisplayName("Resolved by")]
        public string resolvedBy { get; set; }

        [DisplayName("Resolution date")]
        [DataType(DataType.DateTime)]

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? resolutionDate { get; set; }

        [Required]
        public bool Active { get; set; }

        [DisplayName("Priority")]
        public string priority { get; set; }

        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        [DisplayName("Comment")]
        public string comment { get; set; }

        [DisplayName("Follow up")]
        public bool queryStatus { get; set; }

        public string EnquiryStatus
        {
            get
            {
                if (queryStatus == true)
                {
                    return ("Follow up");
                }
                else
                {


                    return ("Closed");
                }
            }
        }

        [DisplayName("Follow up date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? followUpDate { get; set; }

        [DisplayName("Reference #")]
        public string ReferenceNumber { get; set; }

        [DisplayName("Owner")]
        public string Owner { get; set; }

        [DisplayName("Query age")]
        [DisplayFormat(DataFormatString = "{0:d\\d}" + " " + "{0:h\\h}" + " " + "{0:m\\m}", ApplyFormatInEditMode = true)]
        public TimeSpan Time
        {
            get
            {
                if (queryStatus == true)
                {
                    DateTime now = DateTime.Now;

                    var startTime = Convert.ToDateTime(effectiveDate);
                    var endTime = Convert.ToDateTime(now);
                    var difference = startTime - endTime;

                    return difference;
                }
                else
                {
                    DateTime now = DateTime.Now;

                    var startTime = Convert.ToDateTime(effectiveDate);
                    var endTime = Convert.ToDateTime(resolutionDate);
                    var difference = endTime - startTime;

                    return difference;
                }
            }
        }
        public Guid? programId { get; set; }
    }
}