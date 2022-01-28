using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.XmlModels
{
    class memberDetailsRequest
    {
        [JsonProperty("memberDetailsRequest")]
        public memberDetails memberDetails { get; set; }
    }
    class memberDetails
    {
        [JsonProperty("originID")]
        public string OriginID { get; set; }

        [JsonProperty("schemeCode")]
        public string SchemeCode { get; set; }

        [JsonProperty("familyId")]
        public string FamilyId { get; set; }

        [JsonProperty("dependantNo")]
        public string DependantNo { get; set; }

        [JsonProperty("option")]
        public string Option { get; set; }

    }
}