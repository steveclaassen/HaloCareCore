using HaloCareCore.DAL;
using HaloCareCore.Models.Validation;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace HaloCareCore.Controllers
{//HCare-1095
    public class ComorbidsController : Controller
    {
        private IAdminRepository _admin;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ComorbidsController(OH17Context context, IConfiguration configuration)
        {
            this._admin = new AdminRepository(context, configuration);
        }

        public ActionResult Index()
        {
            //var model = _admin.GetComorbidsSearch("", "");
            var model = _admin.GetICD10ConditionList(); //HCare-1157

            return View(model);
        }

        //[HttpPost]
        //public ActionResult Index(ComorbidConditionExclusions model)
        //{
        //    var icd10 = "";
        //    var conditionname = "";

        //    if (Request.Query["icd10"].ToString() != null)
        //        icd10 = Request.Query["icd10"].ToString();
        //    if (Request.Query["conditionname"].ToString() != null)
        //        conditionname = Request.Query["conditionname"].ToString();

        //    var products = _admin.GetComorbidsSearch(icd10, conditionname);

        //    return View(products);

        //}

        public ActionResult Create()
        {
            ViewBag.MappingCode = "";
            ViewBag.Description = "";
            ViewBag.FormularyCode = "";
            ViewBag.ICD10 = "";

            return View();
        }

        [HttpPost]
        public ActionResult Create(ComorbidConditionExclusions model)
        {
            model.CreatedBy = User.Identity.Name;
            model.mappingCode = model.mappingCode.Trim(); //hcare-1285
            model.mappingDescription = model.mappingDescription.Trim(); //hcare-1285
            model.formularyCode = model.formularyCode.Trim(); //hcare-1285
            model.ICD10Code = model.ICD10Code.Trim(); //hcare-1285

            #region duplicate-check
            var validation = _admin.GetICD10ConditionsByMappingCode(model.mappingCode); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.mappingCode.ToLower() == item.mappingCode.ToLower().Trim() &&
                    model.mappingDescription.ToLower() == item.mappingDescription.ToLower().Trim() &&
                    model.formularyCode.ToLower() == item.formularyCode.ToLower().Trim() &&
                    model.ICD10Code.ToLower() == item.ICD10Code.ToLower().Trim()) { v1++; }
            }
            if (v1 > 0)
            {
                ViewBag.MappingCode = model.mappingCode;
                ViewBag.Description = model.mappingDescription;
                ViewBag.FormularyCode = model.formularyCode;
                ViewBag.ICD10 = model.ICD10Code;

                return View(model);
            }
            #endregion

            _admin.InsertComorbidExclusion(model);

            return RedirectToAction("Index", new { icd10 = model.ICD10Code });
        }

        public ActionResult Edit(int id)
        {
            var model = _admin.GetComorbidExclusion(id);
            ViewBag.MappingCode = "";
            ViewBag.Description = "";
            ViewBag.FormularyCode = "";
            ViewBag.ICD10 = "";

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ComorbidConditionExclusions model)
        {
            model.ModifiedBy = User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            model.mappingCode = model.mappingCode.Trim(); //hcare-1285
            model.mappingDescription = model.mappingDescription.Trim(); //hcare-1285
            model.formularyCode = model.formularyCode.Trim(); //hcare-1285
            model.ICD10Code = model.ICD10Code.Trim(); //hcare-1285

            #region duplicate-check
            var validation = _admin.GetICD10ConditionsByMappingCode(model.mappingCode); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.id != item.id &&
                    model.mappingCode.ToLower() == item.mappingCode.ToLower().Trim() &&
                    model.mappingDescription.ToLower() == item.mappingDescription.ToLower().Trim() &&
                    model.formularyCode.ToLower() == item.formularyCode.ToLower().Trim() &&
                    model.ICD10Code.ToLower() == item.ICD10Code.ToLower().Trim()) { v1++; }
            }
            if (v1 > 0)
            {
                ViewBag.MappingCode = model.mappingCode;
                ViewBag.Description = model.mappingDescription;
                ViewBag.FormularyCode = model.formularyCode;
                ViewBag.ICD10 = model.ICD10Code;

                return View(model);
            }
            #endregion

            var res = _admin.UpdateComorbidExclusion(model);

            return RedirectToAction("Details", new { code = model.mappingCode });

        }

        public ActionResult Details(string code)
        {
            var model = _admin.GetComorbidInformation(code);

            return View(model);
        }

        public ActionResult ICD10CodeCheck(string ICD10Code) //HCare-956
        {
            var options = _admin.GetComobidByICD10(ICD10Code);

            return Json(options);
        }

    }
}