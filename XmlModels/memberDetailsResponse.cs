using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.XmlModels
{
    public class memberDetailsResponse
    {
        [JsonProperty("memberDetailsResponse")]
        public DetailsResponse DetailsResponse { get; set; }
    }
    public class DetailsResponse
    {

        [JsonProperty("result")]
        public result result { get; set; }
    }
    public class result
    {
        [JsonProperty("memberNumber")]
        public string MemberNumber { get; set; }

        [JsonProperty("schemeName")]
        public string SchemeName { get; set; }

        [JsonProperty("schemeOption")]
        public string SchemeOption { get; set; }

        [JsonProperty("optionStatus")]
        public string OptionStatus { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("firstname")]
        public string Firstname { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("dependantNumber")]
        public string DependantNumber { get; set; }

        [JsonProperty("memberStatus")]
        public string MemberStatus { get; set; }

        [JsonProperty("eligibilityStart")]
        public string EligibilityStart { get; set; }

        [JsonProperty("eligibilityEnd")]
        public string EligibilityEnd { get; set; }

        [JsonProperty("dateOfBirth")]
        public string DateOfBirth { get; set; }

        [JsonProperty("IDNumber")]
        public string IDNumber { get; set; }

        [JsonProperty("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("addressLine2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("telephoneNumber")]
        public string TelephoneNumber { get; set; }

        [JsonProperty("cellphone")]
        public string Cellphone { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("employerCode")]
        public string EmployerCode { get; set; }
    }
}