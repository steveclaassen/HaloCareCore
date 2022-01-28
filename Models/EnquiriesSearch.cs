using HaloCareCore.Models.Communications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Globalization;


namespace HaloCareCore.Models
{
    public class EnquiriesSearch
    {
        [DisplayName("Medical scheme")]
        public string medicalAid { get; set; }

        public Guid medicalAidId { get; set; }

        public Guid programID {get;set;}

        [DisplayName("Active")]
        public bool medicalAidStatus { get; set; }

        [DisplayName("Member #")]
        public string membershipNo { get; set; }

        [DisplayName("ID/Auth #")]
        public string IdNumber { get; set; }

        [DisplayName("Dependant code")]
        public string dependantCode { get; set; }

        [DisplayName("Member name")]
        public string memberName { get; set; }

        [DisplayName("Patient")]
        public string memberName_UC
        {
            get
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(memberName.ToUpper().Substring(0, 1) + memberName.ToLower().Substring(1));
            }
        }

        [DisplayName("Reference")]
        public string queryReference { get; set; }

        [DisplayName("Source")]
        public string querySubject { get; set; }

        [DisplayName("Query code")]
        public string enquiryBy { get; set; }

        [DisplayName("Query description")]
        public string queryDescription { get; set; }

        [DisplayName("Query type")]
        public string queryCode { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }
        
        [DisplayName("Query")]
        public string query { get; set; }

        [DisplayName("Solution")]
        public string querySolution { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Initials")]
        public string userInitial
        {
            get
            {
                return createdBy.ToUpper().Substring(0, 1);
            }
        }


        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Priority")]
        public string queryPriority { get; set; }

        [DisplayName("Status")]
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

        [DisplayName("Source")]
        public string querySource { get; set; }
        
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

        [DisplayName("Comment")]
        public string comment { get; set; }

        [DisplayName("Dependant ID")]
        public Guid dependantId { get; set; }

        [DisplayName("Query ID")]
        public int queryId { get; set; }

        [DisplayName("Status")]
        public string memberStatus { get; set; }

        [DisplayName("Program")]
        public Guid? ProgramID { get; set; }

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



    }
}