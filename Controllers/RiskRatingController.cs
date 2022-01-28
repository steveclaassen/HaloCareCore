using HaloCareCore.DAL;
using HaloCareCore.Models.Management;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace HaloCareCore.Controllers
{
    public class RiskRatingController : Controller
    {
        private IAdminRepository _admin;
        private IMemberRepository _member;
        private IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RiskRatingController(OH17Context context, IConfiguration configuration)
        {
            _admin = new AdminRepository(context, configuration);
            _member = new MemberRepository(configuration, context);
        }

        public ActionResult Index()
        {
            var model = _admin.GetHypoRiskRules();
            return View(model);
        }

        public ActionResult MHIndex()
        {
            var model = _admin.GetMHRiskRatingRules();
            return View(model);
        }

        public ActionResult MHRiskRatingRuleAdd()
        {
            ViewBag.Depression = "";
            ViewBag.Schizophrenia = "";
            ViewBag.Bipolar = "";
            ViewBag.Higher = "";
            ViewBag.Lower = "";
            ViewBag.Risk = "";
            ViewBag.Suicide = "";

            return View();
        }
        [HttpPost]
        public ActionResult MHRiskRatingRuleAdd(MHRiskRatingRules model)
        {
            var exists = _admin.GetMHRule(model); //hcare-1298
            var riskrating = _member.GetRiskRatingTypes().Where(x => x.RiskType == model.rating).Select(x => x.RiskName).FirstOrDefault();
            if (!exists)
            {
                model.createdBy = User.Identity.Name;
                model.createdDate = DateTime.Now;
                model.effectiveDate = DateTime.Now;
                model.active = true;
                _admin.AddMHRules(model);

                return RedirectToAction("Index", "Rules");
            }
            else
            {
                ViewBag.Rule = "This combination of rule already exists!";
                ViewBag.Depression = model.Depression;
                ViewBag.Schizophrenia = model.Schizophrenia;
                ViewBag.Bipolar = model.Bipolar;
                ViewBag.Higher = model.gtValue;
                ViewBag.Lower = model.ltValue;
                ViewBag.Risk = riskrating;
                ViewBag.Suicide = model.suicidal;
                return View(model);
            }
        }

        public ActionResult MHRiskRatingRuleEdit(int id)
        {
            var model = _admin.GetMHRulesById(id);
            ViewBag.Depression = "";
            ViewBag.Schizophrenia = "";
            ViewBag.Bipolar = "";
            ViewBag.Higher = "";
            ViewBag.Lower = "";
            ViewBag.Risk = "";
            ViewBag.Suicide = "";
            return View(model);
        }
        [HttpPost]
        public ActionResult MHRiskRatingRuleEdit(MHRiskRatingRules model)
        {
            var exists = _admin.GetMHRule(model); //hcare-1298
            var riskrating = _member.GetRiskRatingTypes().Where(x => x.RiskType == model.rating).Select(x => x.RiskName).FirstOrDefault();
            if (!exists)
            {
                model.modifiedBy = User.Identity.Name;
                model.modifiedDate = DateTime.Now;

                _admin.UpdateMHRules(model);

                return RedirectToAction("Index", "Rules");
            }
            else
            {
                ViewBag.Rule = "This combination of rule already exists!";
                ViewBag.Depression = model.Depression;
                ViewBag.Schizophrenia = model.Schizophrenia;
                ViewBag.Bipolar = model.Bipolar;
                ViewBag.Higher = model.gtValue;
                ViewBag.Lower = model.ltValue;
                ViewBag.Risk = riskrating;
                ViewBag.Suicide = model.suicidal;

                return View(model);
            }
        }

        public ActionResult Create()
        {
            ViewBag.Rule = "";
            ViewBag.ltAge = "";
            ViewBag.gtAge = "";
            ViewBag.Insulin = "";
            ViewBag.Sulphonylureas = "";
            ViewBag.Glucose = "";
            ViewBag.Renal = "";
            ViewBag.Hypo = "";

            return View();
        }
        [HttpPost]
        public ActionResult Create(HypoRiskRules model)
        {
            var exists = _admin.GetHypoRiskRule(model); //hcare-1298
            if (!exists)
            {
                model.Glucose = model.Glucose.Trim(); //hcare-1285
                model.createdBy = User.Identity.Name;
                model.createdDate = DateTime.Now;
                model.active = true;
                _admin.AddHypoRiskRules(model);

                return RedirectToAction("Index", "Rules");
            }
            else
            {
                ViewBag.Rule = "This combination of rule already exists!";
                ViewBag.ltAge = model.ltAge;
                ViewBag.gtAge = model.gtAge;
                ViewBag.Insulin = model.Insulin;
                ViewBag.Sulphonylureas = model.Sulphonylureas;
                ViewBag.Glucose = model.Glucose;
                ViewBag.Renal = model.Renal;
                ViewBag.Hypo = model.hypo;

                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {
            var model = _admin.GetHypoRiskRulesById(id);
            ViewBag.Rule = "";
            ViewBag.ltAge = "";
            ViewBag.gtAge = "";
            ViewBag.Insulin = "";
            ViewBag.Sulphonylureas = "";
            ViewBag.Glucose = "";
            ViewBag.Renal = "";
            ViewBag.Hypo = "";

            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(HypoRiskRules model)
        {
            var exists = _admin.GetHypoRiskRule(model); //hcare-1298
            if (!exists)
            {
                model.Glucose = model.Glucose.Trim(); //hcare-1285
                model.modifiedBy = User.Identity.Name;
                model.modifiedDate = DateTime.Now;
                _admin.UpdateHypoRiskRules(model);

                return RedirectToAction("Index", "Rules");
            }
            else
            {
                ViewBag.Rule = "This combination of rule already exists!";
                ViewBag.ltAge = model.ltAge;
                ViewBag.gtAge = model.gtAge;
                ViewBag.Insulin = model.Insulin;
                ViewBag.Sulphonylureas = model.Sulphonylureas;
                ViewBag.Glucose = model.Glucose;
                ViewBag.Renal = model.Renal;
                ViewBag.Hypo = model.hypo;

                return View(model);
            }
        }
        public ActionResult Details(int id)
        {
            var model = _admin.GetHypoRiskRulesById(id);
            return View(model);
        }

        public ActionResult LifeIndex()
        {
            var model = _admin.GetLifeRules();
            return View(model);
        }

        public ActionResult LifeCreate()
        {
            ViewBag.Rule = "";
            ViewBag.Gender = "";
            ViewBag.Risk = "";
            ViewBag.Smoker = "";
            ViewBag.Hypertension = "";
            ViewBag.Hychol = "";
            ViewBag.gtHbA1c = "";
            ViewBag.ltHbA1c = "";
            ViewBag.gtAge = "";
            ViewBag.ltAge = "";

            return View();
        }
        [HttpPost]
        public ActionResult LifeCreate(LifeExpectancyRules model)
        {
            var exists = _admin.GetLifeRule(model); //hcare-1298
            var gender = _member.GetSex().Where(x => x.sex == model.gender).Select(x => x.sexName).FirstOrDefault();
            var riskrating = _member.GetRiskRatingTypes().Where(x => x.RiskType == model.RiskId).Select(x => x.RiskName).FirstOrDefault();

            if (!exists)
            {
                model.createdBy = User.Identity.Name;
                model.createdDate = DateTime.Now;
                model.active = true;
                _admin.AddLifeRules(model);

                return RedirectToAction("Index", "Rules");
            }
            else
            {
                ViewBag.Rule = "This combination of rule already exists!";
                ViewBag.Gender = gender;
                ViewBag.Risk = riskrating;
                ViewBag.Smoker = model.smoker;
                ViewBag.Hypertension = model.hypertension;
                ViewBag.Hychol = model.hychol;
                ViewBag.gtHbA1c = model.gtHba1C;
                ViewBag.ltHbA1c = model.ltHba1C;
                ViewBag.gtAge = model.gtAge;
                ViewBag.ltAge = model.ltAge;

                model.ltHba1C = model.ltHba1C;

                return View(model);
            }
        }

        public ActionResult LifeEdit(int id)
        {
            var model = _admin.GetLifeRulesById(id);
            return View(model);
        }
        [HttpPost]
        public ActionResult LifeEdit(LifeExpectancyRules model) //HCare-1215
        {
            var exists = _admin.GetLifeRule(model); //hcare-1298
            var gender = _member.GetSex().Where(x => x.sex == model.gender).Select(x => x.sexName).FirstOrDefault();
            var riskrating = _member.GetRiskRatingTypes().Where(x => x.RiskType == model.RiskId).Select(x => x.RiskName).FirstOrDefault();

            if (!exists)
            {
                model.modifiedBy = User.Identity.Name;
                model.modifiedDate = DateTime.Now;
                _admin.UpdateLifeRules(model);

                return RedirectToAction("Index", "Rules");
            }
            else
            {
                ViewBag.Rule = "This combination of rule already exists!";
                ViewBag.Gender = gender;
                ViewBag.Risk = riskrating;
                ViewBag.Smoker = model.smoker;
                ViewBag.Hypertension = model.hypertension;
                ViewBag.Hychol = model.hychol;
                ViewBag.gtHbA1c = model.gtHba1C;
                ViewBag.ltHbA1c = model.ltHba1C;
                ViewBag.gtAge = model.gtAge;
                ViewBag.ltAge = model.ltAge;

                return View(model);
            }

        }

        public ActionResult LifeDetails(int id)
        {
            var model = _admin.GetLifeRulesById(id);
            return View(model);
        }
    }
}