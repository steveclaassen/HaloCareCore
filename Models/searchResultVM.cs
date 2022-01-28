using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class searchResultVM
    {
        public AdvancedSearchView advancedSearchView { get; set; }

        public List<AdvancedSearchResultModel> advancedSearchResultModels { get; set; }
        public AdvancedSearchResultModel advancedSearchResultModel{ get; set; }



    }
}