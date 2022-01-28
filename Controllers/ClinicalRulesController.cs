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
    public class ClinicalRulesController : Controller
    {
        private IAdminRepository _admin;
        private IMemberRepository _member;
        private IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClinicalRulesController(OH17Context context, IConfiguration _configuration)
        {
            _admin = new AdminRepository(context, _configuration);
            _member = new MemberRepository(_configuration, context);

        }

        public ActionResult Index()
        {
            var model = _admin.GetClinicalRiskRulesList();
            return View(model);
        }

        public ActionResult Create()
        {
            var model = new ClinicalRules();
            model.Assignments = _member.GetAssignmentItemTypes();
            model.Pathologies = _member.GetPathologyFields();
            model.Programs = _member.GetPrograms();

            ViewBag.AllFields = "";
            ViewBag.Description = "";
            ViewBag.Higher = "";
            ViewBag.Lower = "";
            ViewBag.Assignment = "";
            ViewBag.Pathology = "";
            ViewBag.Program = "";

            return View(model);
        }
        [HttpPost]
        public ActionResult Create(ClinicalRules model)
        {
            model.ruleName = model.ruleName.Trim(); //hcare-1285
            if (!String.IsNullOrEmpty(model.gtMessage)) { model.gtMessage = model.gtMessage.Trim(); } //hcare-1285
            if (!String.IsNullOrEmpty(model.ltMessage)) { model.ltMessage = model.ltMessage.Trim(); } //hcare-1285
            if (!String.IsNullOrEmpty(model.Instruction)) { model.Instruction = model.Instruction.Trim(); } //hcare-1344
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.active = true;

            #region duplicate-check
            var validation = _admin.GetClinicalRiskRuleValidation().Where(x => x.pathologyField == model.pathologyField).ToList(); //hcare-1298
            var assignment = _member.GetAssignmentItemTypes().Where(x => x.assignmentItemType == model.assignmentItem).Select(x => x.itemDescription).FirstOrDefault();
            var pathology = _member.GetPathologyFields().Where(x => x.PathologyField == model.pathologyField).Select(x => x.DisplayName).FirstOrDefault();
            var program = _member.GetPrograms().Where(x => x.code == model.ruleType).FirstOrDefault();
            var v1 = 0;
            var v2 = 0;
            foreach (var item in validation)
            {
                if (model.greater == item.greater &&
                    model.less == item.less &&
                    model.ruleType == item.ruleType &&
                    model.male == item.male &&
                    model.female == item.female &&
                    model.pathologyField == item.pathologyField &&
                    model.assignmentItem == item.assignmentItem
                    ) { v1++; break; }

                if (model.ruleName.ToLower() == item.ruleName.ToLower().Trim() && model.ruleType == item.ruleType) { v2++; break; }
            }
            if (v1 > 0)
            {
                model.Assignments = _member.GetAssignmentItemTypes();
                model.Pathologies = _member.GetPathologyFields();
                model.Programs = _member.GetPrograms();
                ViewBag.AllFields = model.ruleName;
                ViewBag.Description = model.ruleName;
                ViewBag.Higher = model.greater;
                ViewBag.Lower = model.less;
                ViewBag.Assignment = assignment;
                ViewBag.Pathology = pathology;
                ViewBag.Program = program.ProgramName;
                return View(model);
            }
            if (v2 > 0)
            {
                model.Assignments = _member.GetAssignmentItemTypes();
                model.Pathologies = _member.GetPathologyFields();
                model.Programs = _member.GetPrograms();
                ViewBag.Description = model.ruleName;
                ViewBag.Assignment = model.assignmentItem;
                return View(model);
            }
            #endregion

            _admin.AddClinicalRules(model);

            return RedirectToAction("Index", "Rules");
        }

        public ActionResult Edit(int id)
        {
            var model = _admin.GetClinicalRulesById(id);

            model.Assignments = _member.GetAssignmentItemTypes();
            model.Pathologies = _member.GetPathologyFields();
            model.Programs = _member.GetPrograms();

            ViewBag.AllFields = "";
            ViewBag.Description = "";
            ViewBag.Higher = "";
            ViewBag.Lower = "";
            ViewBag.Assignment = "";
            ViewBag.Pathology = "";
            ViewBag.Program = "";

            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(ClinicalRules model)
        {
            model.ruleName = model.ruleName.Trim(); //hcare-1285
            if (!String.IsNullOrEmpty(model.gtMessage)) { model.gtMessage = model.gtMessage.Trim(); } //hcare-1285
            if (!String.IsNullOrEmpty(model.ltMessage)) { model.ltMessage = model.ltMessage.Trim(); } //hcare-1285
            if (!String.IsNullOrEmpty(model.Instruction)) { model.Instruction = model.Instruction.Trim(); } //hcare-1344
            model.modifiedDate = DateTime.Now;
            model.modifiedBy = User.Identity.Name;

            #region duplicate-check
            var validation = _admin.GetClinicalRiskRuleValidation().Where(x => x.pathologyField == model.pathologyField).ToList(); //hcare-1298
            var assignment = _member.GetAssignmentItemTypes().Where(x => x.assignmentItemType == model.assignmentItem).Select(x => x.itemDescription).FirstOrDefault();
            var pathology = _member.GetPathologyFields().Where(x => x.PathologyField == model.pathologyField).Select(x => x.DisplayName).FirstOrDefault();
            var program = _member.GetPrograms().Where(x => x.code == model.ruleType).FirstOrDefault();

            var v1 = 0;
            var v2 = 0;
            foreach (var item in validation)
            {
                if (model.id != item.id &&
                    model.greater == item.greater &&
                    model.less == item.less &&
                    model.ruleType == item.ruleType &&
                    model.male == item.male &&
                    model.female == item.female &&
                    model.pathologyField == item.pathologyField &&
                    model.assignmentItem == item.assignmentItem
                    ) { v1++; break; }

                if (model.id != item.id && model.ruleType == item.ruleType && model.ruleName.ToLower() == item.ruleName.ToLower().Trim()) { v2++; break; }
            }
            if (v1 > 0)
            {
                model.Assignments = _member.GetAssignmentItemTypes();
                model.Pathologies = _member.GetPathologyFields();
                model.Programs = _member.GetPrograms();
                ViewBag.AllFields = model.ruleName;
                ViewBag.Description = model.ruleName;
                ViewBag.Higher = model.greater;
                ViewBag.Lower = model.less;
                ViewBag.Assignment = assignment;
                ViewBag.Pathology = pathology;
                ViewBag.Program = program.ProgramName;
                return View(model);
            }
            if (v2 > 0)
            {
                model.Assignments = _member.GetAssignmentItemTypes();
                model.Pathologies = _member.GetPathologyFields();
                model.Programs = _member.GetPrograms();
                ViewBag.Description = model.ruleName;
                ViewBag.Assignment = model.assignmentItem;
                return View(model);
            }
            #endregion

            _admin.UpdateClinicalRules(model);

            return RedirectToAction("Index", "Rules");
        }

        public ActionResult Details(int id)
        {
            var model = _admin.GetClinicalRulesInfo(id);
            return View(model);
        }

    }
}