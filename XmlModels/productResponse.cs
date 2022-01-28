using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.XmlModels
{
    public class productResponse
    {
        [JsonProperty("productResponse")]
        public productDetailsResponse detailsResponse { set; get; }
    }
 
    public class productDetailsResponse
    {
        [JsonProperty("productNo")]
        public string productNo { get; set; }//Nappiecode
        
        [JsonProperty("productLabelName")]
        public string productLabelName { get; set; }//product name
        
        [JsonProperty("atcCode")]
        public string atcCode { get; set; }

        [JsonProperty("therapeuticClass6Desc")]
        public string therapeuticClass6Desc { set; get; }

        [JsonProperty("therapeuticClass6")]
        public int therapeuticClass6 { get; set; }

        [JsonProperty("mmapIndicator")]
        public string mmapIndicator { set; get; }

        [JsonProperty("mrpIndicator")]
        public string mrpIndicator { get; set; }

        [JsonProperty("totalPackQuantity")]
        public double totalPackQuantity { get; set; }//packsize

        [JsonProperty("packUOM")]
        public string packUOM { get; set; }

        [JsonProperty("genericIndicator")]
        public string genericIndicator { get; set; }

        [JsonProperty("productSchedule")]
        public string productSchedule { set; get; }

        [JsonProperty("strengthUOM")]
        public string strengthUOM { get; set; }

        [JsonProperty("productStatus")]
        public string productStatus { get; set; }

        [JsonProperty("gpiNo")]
        public string gpiNo { get; set; }

        [JsonProperty("gppcNo")]
        public string gppcNo { get; set; }

        [JsonProperty("dosageForm")]
        public string dosageForm { get; set; }

        [JsonProperty("routeOfAdmin")]
        public string routeOfAdmin { get; set; }

        [JsonProperty("unitDose")]
        public string unitDose { get; set; }

        [JsonProperty("therapeuticClass8")]
        public string therapeuticClass8 { get; set; }

        [JsonProperty("thirdPartyExceptionCode")]
        public string thirdPartyExceptionCode { get; set; }

        [JsonProperty("maintenanceDrug")]
        public string maintenanceDrug { get; set; }

        [JsonProperty("formType")]
        public string formType { get; set; }

        [JsonProperty("singleCombinationCode")]
        public string singleCombinationCode { get; set; }

        [JsonProperty("packageDescription")]
        public string packageDescription { get; set; }

        [JsonProperty("strength")]
        public string strength { get; set; }

        [JsonProperty("packageSize")]
        public string packageSize { get; set; }

        [JsonProperty("packQuantity")]
        public string packQuantity { get; set; }

        [JsonProperty("productName")]
        public string productName { get; set; }

        [JsonProperty("mediscorProductName")]
        public string mediscorProductName { get; set; }

        [JsonProperty("effectiveDate")]
        public DateTime? effectiveDate { get; set; }

        [JsonProperty("kdcNo")]
        public string kdcNo { get; set; }

        [JsonProperty("barcode")]
        public string barcode { get; set; }

        [JsonProperty("brandCode")]
        public string brandCode { get; set; }

        [JsonProperty("deaCode")]
        public string deaCode { get; set; }

        [JsonProperty("otcSchedule")]
        public string otcSchedule { get; set; }

        [JsonProperty("namibianSchedule")]
        public string namibianSchedule { get; set; }

        [JsonProperty("medicineIndicator")]
        public string medicineIndicator { get; set; }

        [JsonProperty("biologicalIndicator")]
        public string biologicalIndicator { get; set; }

        [JsonProperty("mrpCode")]
        public string mrpCode { get; set; }

        [JsonProperty("nrpCode")]
        public string nrpCode { get; set; }

        [JsonProperty("frpCode")]
        public string frpCode { get; set; }
        
        [JsonProperty("section21")]
        public string section21 { get; set; }

        [JsonProperty("manufactureNo")]
        public string manufactureNo { get; set; }

        [JsonProperty("prevProductNo")]
        public string prevProductNo { get; set; }

        [JsonProperty("prevProdFromDate")]
        public DateTime? prevProdFromDate { get; set; }

        [JsonProperty("replacementProductNo")]
        public string replacementProductNo { get; set; }

        [JsonProperty("replacementFromDate")]
        public DateTime? replacementFromDate { get; set; }

        [JsonProperty("discontinueDate")]
        public string discontinueDate { get; set; }

        [JsonProperty("discontinueReasonCode")]
        public string discontinueReasonCode { get; set; }

        [JsonProperty("prodMkDiscUpdateDate")]
        public DateTime? prodMkDiscUpdateDate { get; set; }
        [JsonProperty("mdsDiscUpdateDate")]
        public DateTime? mdsDiscUpdateDate { get; set; }

        [JsonProperty("discReasonCodeInd")]
        public string discReasonCodeInd { get; set; }

        [JsonProperty("discReasonDesc")]
        public string discReasonDesc { get; set; }

        [JsonProperty("packDiscontinueDate")]
        public string packDiscontinueDate { get; set; }

        [JsonProperty("namibiaEffectiveDate")]
        public DateTime? namibiaEffectiveDate { get; set; }

        [JsonProperty("medikreditProductName")]
        public string medikreditProductName { get; set; }

        [JsonProperty("productPriceNote")]
        public string productPriceNote { get; set; }

        [JsonProperty("namibiaProductOnly")]
        public string namibiaProductOnly { get; set; }

        [JsonProperty("section21Date")]
        public DateTime? section21Date { get; set; }

        [JsonProperty("addDate")]
        public string addDate { get; set; }
        [JsonProperty("addTime")]
        public string addTime { get; set; }

        [JsonProperty("addProgram")]
        public string addProgram { get; set; }

        [JsonProperty("addUser")]
        public string addUser { get; set; }

        [JsonProperty("changeDate")]
        public DateTime? changeDate { get; set; }

        [JsonProperty("changeTime")]
        public string changeTime { get; set; }

        [JsonProperty("changeProgram")]
        public string changeProgram { get; set; }
        [JsonProperty("changeUser")]
        public string changeUser { get; set; }

        [JsonProperty("botswanaOnlyIndicator")]
        public string botswanaOnlyIndicator { get; set; }

        [JsonProperty("mediscorOutOfStock")]
        public string mediscorOutOfStock { get; set; }

        [JsonProperty("mplIndicator")]
        public string mplIndicator { get; set; }

        [JsonProperty("stockCode")]
        public string stockCode { get; set; }

        [JsonProperty("chargeableCode")]
        public string chargeableCode { get; set; }

        [JsonProperty("productNumericUse")]
        public string productNumericUse { get; set; }

        [JsonProperty("productInHouseIndicator")]
        public string productInHouseIndicator { get; set; }

        [JsonProperty("therapeuticClass12")]
        public string therapeuticClass12 { get; set; }

        [JsonProperty("productDiscontinueDate")]
        public DateTime? productDiscontinueDate { get; set; }

        [JsonProperty("mediscorOutOfStockFromDate")]
        public DateTime? mediscorOutOfStockFromDate { get; set; }

        [JsonProperty("mediscorOutOfStockNote")]
        public string mediscorOutOfStockNote { get; set; }

        [JsonProperty("mediscorOutOfStockThruDate")]
        public DateTime? mediscorOutOfStockThruDate { get; set; }
        [JsonProperty("mediscorOutOfStockChangeDate")]

        public DateTime? mediscorOutOfStockChangeDate { get; set; }

        [JsonProperty("mediscorOutOfStockChangeTime")]
        public string mediscorOutOfStockChangeTime { get; set; }

        [JsonProperty("countryList")]
        public string countryList { get; set; }
    }
}