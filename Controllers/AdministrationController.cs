using HaloCareCore.DAL;
using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace HaloCareCore.Controllers
{
    public class AdministrationController : Controller
    {
        private IAdminRepository _admin;
        private IMemberRepository _member;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        public AdministrationController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, OH17Context context)
        {
            _admin = new AdminRepository(context, configuration);
            _member = new MemberRepository(configuration, context);
        }
        protected override void Dispose(bool disposing)
        {
            _admin.Dispose();
        }

        public ActionResult Index()
        {
            return View();
        }


        //========================================================================= education-note =========================================================================//
        public ActionResult educationalNote_Index()
        {
            var note = _admin.GetEducationalNote();

            return View(note);
        }

        public ActionResult educationalNote_Create()
        {
            ViewBag.NoteTitle = "";
            ViewBag.NoteTemplate = "";

            return View();
        }
        [HttpPost]
        public ActionResult educationalNote_Create(EducationalNotes model)
        {
            model.title = model.title.Trim(); //hcare-1285
            model.note = model.note.Trim(); //hcare-1285
            model.createdDate = DateTime.Now;
            model.createdBy = User.Identity.Name;
            model.Active = true;

            #region duplicate-check
            var validation = _admin.GetEducationalNote(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.title.ToLower() == item.title.ToLower().Trim() && model.note.ToLower() == item.note.ToLower().Trim()) { v1++; break; }
                if (model.title.ToLower() == item.title.ToLower().Trim()) { v2++; }
                if (model.note.ToLower() == item.note.ToLower().Trim()) { v3++; break; }
            }
            if (v1 > 0)
            {
                ViewBag.NoteTitle = model.title;
                ViewBag.NoteTemplate = model.note;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.NoteTitle = model.title;
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.NoteTemplate = model.note;
                return View(model);
            }

            #endregion

            var result = _admin.AddNewEducationalNote(model);

            return RedirectToAction("Index", "Template");
        }

        public ActionResult educationalNote_Edit(int id)
        {
            var model = _admin.GetEducationalNoteById(id);
            ViewBag.NoteTitle = "";
            ViewBag.NoteTemplate = "";
            return View(model);
        }
        [HttpPost]
        public ActionResult educationalNote_Edit(EducationalNotes model)
        {
            model.title = model.title.Trim(); //hcare-1285
            model.note = model.note.Trim(); //hcare-1285
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;

            #region duplicate-check
            var validation = _admin.GetEducationalNote(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.Id != item.Id && model.title.ToLower() == item.title.ToLower().Trim() && model.note.ToLower() == item.note.ToLower().Trim()) { v1++; break; }
                if (model.Id != item.Id && model.title.ToLower() == item.title.ToLower().Trim()) { v2++; }
                if (model.Id != item.Id && model.note.ToLower() == item.note.ToLower().Trim()) { v3++; break; }
            }
            if (v1 > 0)
            {
                ViewBag.NoteTitle = model.title;
                ViewBag.NoteTemplate = model.note;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.NoteTitle = model.title;
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.NoteTemplate = model.note;
                return View(model);
            }

            #endregion

            var result = _admin.UpdateEducationalNote(model);

            return RedirectToAction("Index", "Template");
        }


        //=========================================================================== laboratory ===========================================================================//
        public ActionResult LaboratoryIndex()
        {
            var labList = _admin.GetListOfLaboratories();
            return View(labList);
        }

        public ActionResult LaboratoryCreate()
        {
            ViewBag.Rule = "";
            ViewBag.Description = "";

            return View();
        }
        [HttpPost]
        public ActionResult LaboratoryCreate(Laboratory model)
        {
            var exists = _admin.GetLaboratoryName(model); //hcare-1298
            if (!exists)
            {
                model.name = model.name.Trim(); //hcare-1285
                model.telephoneNo = model.telephoneNo.Trim(); //hcare-1285
                model.email = model.email.Trim(); //hcare-1285
                model.createdBy = User.Identity.Name;
                model.createdDate = DateTime.Now;
                model.active = true;

                _admin.InsertLaboratory(model);

                return RedirectToAction("Index", "Other");
            }
            else
            {
                ViewBag.Rule = "duplicate";
                ViewBag.Description = model.name;

                return View(model);
            }
        }

        public ActionResult LaboratoryEdit(int id)
        {
            var labDetails = _admin.GetLaboratoryById(id);
            ViewBag.Rule = "";
            ViewBag.Description = "";

            return View(labDetails);
        }
        [HttpPost]
        public ActionResult LaboratoryEdit(Laboratory model)
        {
            var exists = _admin.GetLaboratoryName(model); //hcare-1298
            if (!exists)
            {
                model.name = model.name.Trim(); //hcare-1285
                model.telephoneNo = model.telephoneNo.Trim(); //hcare-1285
                model.email = model.email.Trim(); //hcare-1285
                model.modifiedDate = DateTime.Now;
                model.modifiedBy = User.Identity.Name;

                _admin.UpdateLaboratory(model);

                return RedirectToAction("Index", "Other");
            }
            else
            {
                ViewBag.Rule = "duplicate";
                ViewBag.Description = model.name;

                return View(model);
            }
        }

        public ActionResult LaboratoryDetails(int id)
        {
            var labDetails = _admin.GetLaboratoryById(id);
            return View(labDetails);
        }

        public ActionResult LaboratoryNameCheck(string name) //HCare-956
        {
            var options = _admin.GetLaboratorybyName(name);

            return Json(options);
        }


        //======================================================================= chronic-medication =======================================================================//
        public ActionResult ChronicMedicationIndex() //HCare-1083
        {
            var model = _admin.GetChronicMedication();
            return View(model);
        }

        public ActionResult ChronicMedicationCreate()
        {
            ViewBag.NAPPI = "";
            ViewBag.Description = "";

            return View();
        }
        [HttpPost]
        public ActionResult ChronicMedicationCreate(ChronicMedication model)
        {
            model.createdDate = DateTime.Now;
            model.createdBy = User.Identity.Name;
            model.nappiStart = model.nappiStart.Trim(); //hcare-1285
            model.description = model.description.Trim(); //hcare-1285
            model.Active = true;

            #region duplicate-check
            var validation = _admin.GetChronicMedicationValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.nappiStart.ToLower() == item.nappiStart.ToLower().Trim() && model.description.ToLower().Trim() == item.description.ToLower().Trim()) { v1++; break; }
                if (model.nappiStart.ToLower() == item.nappiStart.ToLower().Trim()) { v2++; }
                if (model.description.ToLower() == item.description.ToLower().Trim()) { v3++; break; }
            }
            if (v1 > 0)
            {
                ViewBag.NAPPI = model.nappiStart;
                ViewBag.Description = model.description;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.NAPPI = model.nappiStart;
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.Description = model.description;
                return View(model);
            }
            #endregion

            var result = _admin.InsertChronicMedication(model);
            return RedirectToAction("ChronicMedicationIndex", "Administration");
        }

        public ActionResult ChronicMedicationEdit(Guid Id)
        {
            var model = _admin.GetChronicMedicationById(Id);
            return View(model);
        }
        [HttpPost]
        public ActionResult ChronicMedicationEdit(ChronicMedication model)
        {
            model.modifiedBy = User.Identity.Name;
            model.modifiedDate = DateTime.Now;
            model.nappiStart = model.nappiStart.Trim(); //hcare-1285
            model.description = model.description.Trim(); //hcare-1285

            #region duplicate-check
            var validation = _admin.GetChronicMedicationValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.Id != item.Id && model.nappiStart.ToLower() == item.nappiStart.ToLower().Trim() && model.description.ToLower().Trim() == item.description.ToLower().Trim()) { v1++; break; }
                if (model.Id != item.Id && model.nappiStart.ToLower() == item.nappiStart.ToLower().Trim()) { v2++; }
                if (model.Id != item.Id && model.description.ToLower() == item.description.ToLower().Trim()) { v3++; break; }
            }
            if (v1 > 0)
            {
                ViewBag.NAPPI = model.nappiStart;
                ViewBag.Description = model.description;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.NAPPI = model.nappiStart;
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.Description = model.description;
                return View(model);
            }
            #endregion

            var response = _admin.UpdateChronicMedication(model);

            return RedirectToAction("ChronicMedicationIndex", "Administration");
        }

        public ActionResult ChronicMedicationCodeCheck(string nappiStart) //HCare-956
        {
            var options = _admin.GetChronicMedicationByCode(nappiStart);

            return Json(options);
        }
        public ActionResult ChronicMedicationNameCheck(string description) //HCare-956
        {
            var options = _admin.GetChronicMedicationByName(description);

            return Json(options);
        }


        //============================================================================== pmoc ==============================================================================//
        public ActionResult pmocIndex()
        {
            var index = _admin.GetListofPMOC();
            return View(index);
        }

        public ActionResult pmocCreate()
        {
            ViewBag.Code = "";
            ViewBag.Description = "";

            return View();
        }
        [HttpPost]
        public ActionResult pmocCreate(PreferredMethodOfContact model)
        {
            model.pmocCode = model.pmocCode.Trim(); //hcare-1285
            model.pmocDescription = model.pmocDescription.Trim(); //hcare-1285
            model.createdDate = DateTime.Now;
            model.createdBy = User.Identity.Name;
            model.active = true;

            #region duplicate-check
            var validation = _admin.GetPMOCValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.pmocCode.ToLower() == item.pmocCode.ToLower().Trim() && model.pmocDescription.ToLower() == item.pmocDescription.ToLower().Trim()) { v1++; break; }
                if (model.pmocCode.ToLower() == item.pmocCode.ToLower().Trim()) { v2++; }
                if (model.pmocDescription.ToLower() == item.pmocDescription.ToLower().Trim()) { v3++; }
            }
            if (v1 > 0)
            {
                ViewBag.Code = model.pmocCode;
                ViewBag.Description = model.pmocDescription;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.Code = model.pmocCode;
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.Description = model.pmocDescription;
                return View(model);
            }
            #endregion

            var result = _admin.InsertPMOC(model);
            return RedirectToAction("Index", "Other");

        }
        public ActionResult pmocCreateNew()
        {
            return View();
        }

        public ActionResult pmocEdit(int id)
        {
            var details = _admin.GetPMOCById(id);
            ViewBag.Code = "";
            ViewBag.Description = "";

            return View(details);
        }
        [HttpPost]
        public ActionResult pmocEdit(PreferredMethodOfContact model)
        {
            model.pmocCode = model.pmocCode.Trim(); //hcare-1285
            model.pmocDescription = model.pmocDescription.Trim(); //hcare-1285
            model.modifiedDate = DateTime.Now;
            model.modifiedBy = User.Identity.Name;

            #region duplicate-check
            var validation = _admin.GetPMOCValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.id != item.id && model.pmocCode.ToLower() == item.pmocCode.ToLower().Trim() && model.pmocDescription.ToLower() == item.pmocDescription.ToLower().Trim()) { v1++; break; }
                if (model.id != item.id && model.pmocCode.ToLower() == item.pmocCode.ToLower().Trim()) { v2++; }
                if (model.id != item.id && model.pmocDescription.ToLower() == item.pmocDescription.ToLower().Trim()) { v3++; }
            }
            if (v1 > 0)
            {
                ViewBag.Code = model.pmocCode;
                ViewBag.Description = model.pmocDescription;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.Code = model.pmocCode;
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.Description = model.pmocDescription;
                return View(model);
            }
            #endregion

            var update = _admin.UpdatePMOC(model);
            return RedirectToAction("Index", "Other");
        }

        public ActionResult pmocDetails(int id)
        {
            var details = _admin.GetPMOCById(id);
            return View(details);
        }

        public ActionResult PMOCCodeCheck(string pmocCode) //HCare-956
        {
            var options = _admin.GetPMOCbyCode(pmocCode);

            return Json(options);
        }
        public ActionResult PMOCNameCheck(string pmocDescription) //HCare-956
        {
            var options = _admin.GetPMOCbyName(pmocDescription);

            return Json(options);
        }


        //============================================================================= program =============================================================================//
        public ActionResult Program_Index()
        {
            var index = _admin.GetPrograms();
            return View(index);
        }

        public ActionResult Program_Create()
        {
            ViewBag.Code = "";
            ViewBag.Description = "";
            ViewBag.ICD10 = "";

            return View();
        }
        [HttpPost]
        public ActionResult Program_Create(Programs model)
        {
            model.programID = Guid.NewGuid();
            model.code = model.code.ToUpper().Trim(); //hcare-1285
            model.ProgramName = model.ProgramName.Trim(); //hcare-1285
            model.icd10code = model.icd10code.ToUpper().Trim(); //hcare-1285
            model.createdDate = DateTime.Now;
            model.createdBy = User.Identity.Name;
            model.Active = true;

            #region duplicate-check
            var validation = _admin.GetProgramValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            var v4 = 0;
            foreach (var item in validation)
            {
                if (model.code.ToLower() == item.code.ToLower().Trim() && model.ProgramName.ToLower() == item.ProgramName.ToLower().Trim() && model.icd10code.ToLower() == item.icd10code.ToLower().Trim()) { v1++; break; }
                if (model.code.ToLower() == item.code.ToLower().Trim()) { v2++; }
                if (model.ProgramName.ToLower() == item.ProgramName.ToLower().Trim()) { v3++; }
                if (model.icd10code.ToLower() == item.icd10code.ToLower().Trim()) { v4++; break; }
            }
            if (v1 > 0)
            {
                ViewBag.Code = model.code;
                ViewBag.Description = model.ProgramName;
                ViewBag.ICD10 = model.icd10code;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.Code = model.code;
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.Description = model.ProgramName;
                return View(model);
            }
            if (v4 > 0)
            {
                ViewBag.ICD10 = model.icd10code;
                return View(model);
            }
            #endregion

            var result = _admin.InsertProgrm(model);
            return RedirectToAction("Program_Index", "Administration");

        }

        public ActionResult Program_Edit(Guid id)
        {
            var details = _admin.GetProgramById(id);
            return View(details);
        }
        [HttpPost]
        public ActionResult Program_Edit(Programs model)
        {
            model.code = model.code.ToUpper().Trim(); //hcare-1285
            model.ProgramName = model.ProgramName.Trim(); //hcare-1285
            if (!String.IsNullOrEmpty(model.icd10code)) { model.icd10code = model.icd10code.ToUpper().Trim(); } //hcare-1285
            model.modifiedDate = DateTime.Now;
            model.modifiedBy = User.Identity.Name;

            #region duplicate-check
            var validation = _admin.GetProgramValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            var v4 = 0;
            foreach (var item in validation)
            {
                if (model.programID != item.programID && model.code.ToLower() == item.code.ToLower().Trim() && model.ProgramName.ToLower() == item.ProgramName.ToLower().Trim() && model.icd10code.ToLower() == item.icd10code.ToLower().Trim()) { v1++; break; }
                if (model.programID != item.programID && model.code.ToLower() == item.code.ToLower().Trim()) { v2++; }
                if (model.programID != item.programID && model.ProgramName.ToLower() == item.ProgramName.ToLower().Trim()) { v3++; }
                if (!String.IsNullOrEmpty(model.icd10code) && model.programID != item.programID && model.icd10code.ToLower() == item.icd10code.ToLower().Trim()) { v4++; break; }
            }
            if (v1 > 0)
            {
                ViewBag.Code = model.code;
                ViewBag.Description = model.ProgramName;
                ViewBag.ICD10 = model.icd10code;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.Code = model.code;
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.Description = model.ProgramName;
                return View(model);
            }
            if (v4 > 0)
            {
                ViewBag.ICD10 = model.icd10code;
                return View(model);
            }
            #endregion

            var update = _admin.UpdateProgrm(model);

            return RedirectToAction("Program_Index", "Administration");
        }

        public ActionResult Program_Details(Guid id)
        {
            var details = _admin.GetProgramById(id);
            return View(details);
        }

        public ActionResult ProgramNameCheck(string ProgramName) //HCare-956
        {
            var options = _admin.GetProgrambyName(ProgramName);

            return Json(options);
        }
        public ActionResult ProgramCodeCheck(string code) //HCare-956
        {
            var options = _admin.GetProgrambyCode(code);

            return Json(options);
        }
        public ActionResult ProgramICD10Check(string icd10code) //HCare-956
        {
            var options = _admin.GetProgramByICD10(icd10code);

            return Json(options);
        }


        //========================================================================= hiv-risk-rating =========================================================================//
        public ActionResult HIVRiskRatingRulesList() //HCare-840
        {
            var modal = _admin.GetHIVRiskRules();
            return View(modal);
        }

        public ActionResult HIVRiskRatingRulesAdd()
        {
            var modal = new HIVRiskRatingRules();
            ViewBag.Rule = "";
            ViewBag.CD4Count = "";
            ViewBag.CD4Percentage = "";
            ViewBag.ViralLoad = "";
            ViewBag.Higher = "";
            ViewBag.Lower = "";
            ViewBag.Stage = "";
            return View(modal);
        }
        [HttpPost]
        public ActionResult HIVRiskRatingRulesAdd(HIVRiskRatingRules model)
        {
            var exists = _admin.GetHIVRiskRule(model); //hcare-1298
            if (!exists)
            {
                model.createdBy = User.Identity.Name;
                model.createdDate = DateTime.Now;
                model.effectiveDate = DateTime.Now;
                model.active = true;

                _admin.AddHIVRiskRules(model);

                return RedirectToAction("Index", "Rules");
            }
            else
            {
                ViewBag.Rule = "duplicate";
                ViewBag.CD4Count = model.CD4Count;
                ViewBag.CD4Percentage = model.CD4Percentage;
                ViewBag.ViralLoad = model.viralLoad;
                ViewBag.Higher = model.gtValue;
                ViewBag.Lower = model.ltValue;
                ViewBag.Stage = model.stage;

                return View(model);
            }
        }

        public ActionResult HIVRiskRatingRulesEdit(int id)
        {
            var modal = _admin.GetHIVRiskRulesById(id);
            ViewBag.Rule = "";
            ViewBag.CD4Count = "";
            ViewBag.CD4Percentage = "";
            ViewBag.ViralLoad = "";
            ViewBag.Higher = "";
            ViewBag.Lower = "";
            ViewBag.Stage = "";
            return View(modal);
        }
        [HttpPost]
        public ActionResult HIVRiskRatingRulesEdit(HIVRiskRatingRules model)
        {
            var exists = _admin.GetHIVRiskRule(model); //hcare-1298
            if (!exists)
            {
                model.modifiedBy = User.Identity.Name;
                model.modifiedDate = DateTime.Now;

                _admin.UpdateHIVRiskRules(model);

                return RedirectToAction("Index", "Rules");
            }
            else
            {
                ViewBag.Rule = "duplicate";
                ViewBag.CD4Count = model.CD4Count;
                ViewBag.CD4Percentage = model.CD4Percentage;
                ViewBag.ViralLoad = model.viralLoad;
                ViewBag.Higher = model.gtValue;
                ViewBag.Lower = model.ltValue;
                ViewBag.Stage = model.stage;
                return View(model);
            }
        }

        public ActionResult HIVRiskRatingRulesDetails(int id)
        {
            var modal = _admin.GetHIVRiskRulesById(id);
            return View(modal);
        }

        #region Hcare-1184
        public ActionResult HIVStageRiskRatingRulesList()
        {
            var modal = _admin.GetHIVStageRiskRules();
            return View(modal);
        }

        public ActionResult HIVStageRiskAdd()
        {
            var modal = new HIVStageRiskViewModal();
            modal.rules = new HIVStagingRiskRules();
            modal.ratingTypes = _member.GetRiskRatingTypes();
            return View(modal);
        }
        [HttpPost]
        public ActionResult HIVStageRiskAdd(HIVStageRiskViewModal model)
        {
            var exists = _admin.GetHIVStageRiskRule(model.rules); //hcare-1298
            if (!exists)
            {
                model.rules.stage = model.rules.stage.Trim(); //hcare-1285
                model.rules.RiskId = model.rules.RiskId.Trim(); //hcare-1251
                model.rules.createdBy = User.Identity.Name;
                model.rules.createdDate = DateTime.Now;
                model.rules.active = true;

                _admin.AddHIVStageRiskRules(model.rules);

                return RedirectToAction("Index", "Rules");
            }
            else
            {
                ViewBag.Rule = "Already exists";
                model.ratingTypes = _member.GetRiskRatingTypes();
                return View(model);
            }
        }

        public ActionResult HIVStageRiskEdit(int id)
        {
            var modal = new HIVStageRiskViewModal();
            modal.rules = _admin.GetHIVStageRiskRulesById(id);
            modal.ratingTypes = _member.GetRiskRatingTypes();
            return View(modal);
        }

        [HttpPost]
        public ActionResult HIVStageRiskEdit(HIVStageRiskViewModal model)
        {
            var exists = _admin.GetHIVStageRiskRule(model.rules); //hcare-1298
            if (!exists)
            {
                model.rules.stage = model.rules.stage.Trim(); //hcare-1285
                model.rules.RiskId = model.rules.RiskId.Trim(); //hcare-1251
                model.rules.modifiedBy = User.Identity.Name;
                model.rules.modifiedDate = DateTime.Now;

                _admin.UpdateHIVStageRiskRules(model.rules);

                return RedirectToAction("Index", "Rules");
            }
            else
            {
                ViewBag.Rule = "already exists";
                model.ratingTypes = _member.GetRiskRatingTypes();
                return View(model);
            }
        }

        public ActionResult HIVStageRiskDetails(int id)
        {
            var modal = _admin.GetHIVStageRiskRulesById(id);
            return View(modal);
        }
        #endregion

        #region NonCDLFlags  HCare-1060

        public ActionResult NonCDLFlagsIndex()
        {
            var modal = _admin.GetNonCDLFlagsList();

            return View(modal);
        }
        public ActionResult NonCDLFlags_Create(int? id, string error)
        {
            var programs = _admin.GetPrograms().ToList();
            var viewModel = new NonCDLFlagViewModel
            {
                nonCLDFlags = new NonCLDFlags(),
                programs = programs
            };
            ViewBag.Code = "";
            ViewBag.Program = "";

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]  //HCare-1060
        public ActionResult NonCDLFlags_Create(NonCDLFlagViewModel model)
        {
            var exists = _admin.GetNonCDLFlag(model.nonCLDFlags); //hcare-1298
            var program = _admin.GetProgrambyCode(model.nonCLDFlags.programCode);
            if (!exists)
            {
                model.nonCLDFlags.flagCode = model.nonCLDFlags.flagCode.Trim(); //hcare-1285
                model.nonCLDFlags.createdDate = DateTime.Now;
                model.nonCLDFlags.createdBy = User.Identity.Name;
                model.nonCLDFlags.active = true;

                _admin.InserNonCDLFlags(model.nonCLDFlags);

                return RedirectToAction("Index", "Other");
            }
            else
            {
                model.programs = _admin.GetPrograms().ToList();
                ViewBag.Code = model.nonCLDFlags.flagCode;
                ViewBag.Program = program.ProgramName;

                return View(model);
            }
        }

        public ActionResult NonCDLFlags_Edit(int id, string error)
        {
            var nonCDLFlag = _admin.GetNonCDLFlags().SingleOrDefault(x => x.Id == id);

            if (nonCDLFlag == null)
                return NotFound();

            var viewModel = new NonCDLFlagViewModel
            {
                nonCLDFlags = nonCDLFlag,
                programs = _admin.GetPrograms().ToList()
            };
            ViewBag.Code = "";
            ViewBag.Program = "";

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NonCDLFlags_Edit(NonCDLFlagViewModel model)
        {
            var exists = _admin.GetNonCDLFlag(model.nonCLDFlags); //hcare-1298
            var program = _admin.GetProgrambyCode(model.nonCLDFlags.programCode);
            if (!exists)
            {
                model.nonCLDFlags.flagCode = model.nonCLDFlags.flagCode.Trim(); //hcare-1285
                model.nonCLDFlags.modifiedDate = DateTime.Now;
                model.nonCLDFlags.modifiedBy = User.Identity.Name;

                _admin.UpdateNonCDLFlags(model.nonCLDFlags);

                return RedirectToAction("Index", "Other");
            }
            else
            {
                model.programs = _admin.GetPrograms().ToList();
                ViewBag.Code = model.nonCLDFlags.flagCode;
                ViewBag.Program = program.ProgramName;

                return View(model);
            }
        }

        public ActionResult NonCDLFlags_Details(int id)
        {
            var details = _admin.GetNonCDLFlagsByID(id);
            if (details == null)
                return NotFound();

            return View(details);
        }

        public ActionResult CDLFlagCheck(string code, string program) //HCare-956
        {
            var options = _admin.GetNonCDLFlagsByCode(code, program);

            return Json(options);
        }

        #endregion

        #region disclaimer HCare-864
        public ActionResult DisclaimerQ_Index(DisclaimerVM model)
        {
            model.DisclaimerQuestions = _admin.GetDisclaimerQuestions();
            model.AcknowledgementQuestions = _admin.GetAcknowledgementQuestions();

            return View(model);
        }

        public ActionResult DisclaimerQ_Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DisclaimerQ_Create(DisclaimerQuestions model)
        {
            model.CreatedDate = DateTime.Now;
            model.CreatedBy = User.Identity.Name;
            model.Active = true;

            var insert = _admin.InsertDisclaimerQ(model);

            return RedirectToAction("DisclaimerQ_Index", "Administration");

        }

        public ActionResult DisclaimerQ_Edit(int id)
        {
            var details = _admin.GetDisclaimerQById(id);
            return View(details);
        }

        [HttpPost]
        public ActionResult DisclaimerQ_Edit(DisclaimerQuestions model)
        {
            model.ModifiedDate = DateTime.Now;
            model.ModifiedBy = User.Identity.Name;

            var update = _admin.UpdateDisclaimerQ(model);

            return RedirectToAction("DisclaimerQ_Index", "Administration");
        }

        public ActionResult DisclaimerQ_Details(int id)
        {
            var details = _admin.GetDisclaimerQById(id);
            return View(details);
        }


        #endregion

        #region attachment-types hcare-1154
        public ActionResult AttachmentType_Index(AttachmentTypes model)
        {
            var index = _admin.GetAttachmentTypes();
            return View(index);
        }

        public ActionResult AttachmentType_Create()
        {
            var viewModel = new AttachmentTypeViewModel
            {
                attachmentTypes = new AttachmentTypes(),
                itemTypes = _admin.GetAssignmentItemsValidation().ToList()
            };
            ViewBag.Code = "";
            ViewBag.Description = "";

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult AttachmentType_Create(AttachmentTypeViewModel model)
        {
            model.attachmentTypes.attachmentType = model.attachmentTypes.attachmentType.Trim(); //hcare-1285
            model.attachmentTypes.typeDescription = model.attachmentTypes.typeDescription.Trim(); //hcare-1285
            model.attachmentTypes.createdDate = DateTime.Now;
            model.attachmentTypes.createdBy = User.Identity.Name;
            model.attachmentTypes.active = true;

            #region duplicate-check
            var validation = _admin.GetAttachmentTypeValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.attachmentTypes.attachmentType.ToLower() == item.attachmentType.ToLower().Trim() && model.attachmentTypes.typeDescription.ToLower() == item.typeDescription.ToLower().Trim()) { v1++; break; }
                if (model.attachmentTypes.attachmentType.ToLower() == item.attachmentType.ToLower().Trim()) { v2++; }
                if (model.attachmentTypes.typeDescription.ToLower() == item.typeDescription.ToLower().Trim()) { v3++; }
            }
            if (v1 > 0)
            {
                ViewBag.Code = model.attachmentTypes.attachmentType;
                ViewBag.Description = model.attachmentTypes.typeDescription;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.Code = model.attachmentTypes.attachmentType;
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.Code = model.attachmentTypes.typeDescription;
                return View(model);
            }
            #endregion

            var insert = _admin.InsertAttachmentType(model.attachmentTypes);

            return RedirectToAction("Index", "Other");

        }

        public ActionResult AttachmentType_Edit(string id)
        {
            var details = _admin.GetAttachmentTypeById(id);
            ViewBag.Code = "";
            ViewBag.Description = "";
            var viewModel = new AttachmentTypeViewModel
            {
                attachmentTypes = details,
                itemTypes = _admin.GetAssignmentItemsValidation().ToList()
            };
            ViewBag.Code = "";
            ViewBag.Program = "";

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult AttachmentType_Edit(AttachmentTypeViewModel model)
        {
            model.attachmentTypes.typeDescription = model.attachmentTypes.typeDescription.Trim(); //hcare-1285
            model.attachmentTypes.modifiedDate = DateTime.Now;
            model.attachmentTypes.modifiedBy = User.Identity.Name;

            #region duplicate-check
            var validation = _admin.GetAttachmentTypeValidation(); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.attachmentTypes.attachmentType.ToLower() != item.attachmentType.ToLower().Trim() && model.attachmentTypes.typeDescription.ToLower() == item.typeDescription.ToLower().Trim()) { v1++; break; }
            }
            if (v1 > 0)
            {
                ViewBag.Description = model.attachmentTypes.typeDescription;
                return View(model);
            }
            #endregion
            var update = _admin.UpdateAttachmentType(model.attachmentTypes);

            return RedirectToAction("Index", "Other");
        }

        public ActionResult AttachmentType_Details(string id)
        {
            var details = _admin.GetAttachmentTypeById(id);
            return View(details);
        }

        public ActionResult AttachmentTypeCodeCheck(string code)
        {
            var options = _admin.GetAttachmentTypeByCode(code);

            return Json(options);
        }

        public ActionResult AttachmentTypeNameCheck(string name)
        {
            var options = _admin.GetAttachmentTypeByName(name);

            return Json(options);
        }

        #endregion

        #region hcare-1000 & hcare-1440: hiv comorbid rules

        public ActionResult hiv_comorbid_rule_create() //hcare-1440
        {
            var model = new HIVCormodisViewModel
            {
                HIVStages = _admin.GetHIVStages(),
                ComorbidConditionExclusions = _admin.GetComorbidConditionExclusions().OrderBy(x => x.mappingDescription).ToList(),
                HivcomorbidRules = new HivcomorbidRules()
            };

            return View(model);
        }
        [HttpPost]
        public ActionResult hiv_comorbid_rule_create(HIVCormodisViewModel model) //hcare-1440
        {
            model.HivcomorbidRules.createdBy = User.Identity.Name;
            model.HivcomorbidRules.createdDate = DateTime.Now;
            model.HivcomorbidRules.active = true;

            #region duplicate-check
            var validation = _admin.GetHivComorbidRulesValidation();
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.HivcomorbidRules.Comorbid.ToLower() == item.Comorbid.ToLower()) { v1++; }
            }
            if (v1 > 0)
            {
                model.HIVStages = _admin.GetHIVStages();
                model.ComorbidConditionExclusions = _admin.GetComorbidConditionExclusions().OrderBy(x => x.mappingDescription).ToList();
                model.HivcomorbidRules = new HivcomorbidRules();
                ViewBag.Validation = "duplicate";

                return View(model);
            }
            #endregion

            _admin.InsertHivComorbidRules(model.HivcomorbidRules);

            return RedirectToAction("Index", "Rules");
        }

        public ActionResult hiv_comorbid_rule_edit(int id) //hcare-1440
        {
            var model = new HIVCormodisViewModel
            {
                HIVStages = _admin.GetHIVStages(),
                ComorbidConditionExclusions = _admin.GetComorbidConditionExclusions().OrderBy(x => x.mappingDescription).ToList(),
                HivcomorbidRules = _admin.GetHivComorbidRules(id),
            };

            return View(model);
        }
        [HttpPost]
        public ActionResult hiv_comorbid_rule_edit(HIVCormodisViewModel model) //hcare-1440
        {
            model.HivcomorbidRules.modifiedBy = User.Identity.Name;
            model.HivcomorbidRules.modifiedDate = DateTime.Now;

            #region duplicate-check
            var validation = _admin.GetHivComorbidRulesValidation();
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.HivcomorbidRules.Id != item.Id && model.HivcomorbidRules.Comorbid.ToLower() == item.Comorbid.ToLower()) { v1++; }
            }
            if (v1 > 0)
            {
                model.HIVStages = _admin.GetHIVStages();
                model.ComorbidConditionExclusions = _admin.GetComorbidConditionExclusions().OrderBy(x => x.mappingDescription).ToList();
                model.HivcomorbidRules = _admin.GetHivComorbidRules(model.HivcomorbidRules.Id);
                ViewBag.Validation = "duplicate";

                return View(model);
            }
            #endregion

            _admin.UpdateHivComorbidRules(model.HivcomorbidRules);

            return RedirectToAction("Index", "Rules");
        }

        public ActionResult hiv_comorbid_rule_details(int id) //hcare-1440
        {
            var model = new HIVCormodisViewModel
            {
                HivcomorbidRules = _admin.GetHivComorbidRules(id),
            };

            return View(model);
        }

        public ActionResult CheckComorbid(string Name)
        {
            var options = _admin.GetHivComorbidRulesByName(Name);

            return Json(options);
        }

        //No need for Details 
        //public ActionResult HIVCormodisDetails(int id)
        //{
        //    var HIVCormodis = _admin.GetHivComorbidRules(id);
        //    if (HIVCormodis == null)
        //        return NotFound();

        //    return View(HIVCormodis);
        //}

        #endregion

        #region icd-10 codes hcare-1280
        public ActionResult ICD10_Create()
        {
            var model = new ICD10CodeVM();
            model.programs = _admin.GetProgramValidation();

            ViewBag.Program = "";
            ViewBag.Code = "";


            return View(model);
        }
        [HttpPost]
        public ActionResult ICD10_Create(ICD10CodeVM model)
        {
            var program = _admin.GetProgramById(new Guid(model.icd10Code.codeType));

            model.icd10Code.Id = Guid.NewGuid();
            model.icd10Code.icd10CodeID = model.icd10Code.icd10CodeID.Trim(); //hcare-1285
            model.icd10Code.codeType = program.ProgramName;
            model.icd10Code.subcode = program.code;
            model.icd10Code.createdDate = DateTime.Now;
            model.icd10Code.createdBy = User.Identity.Name;
            model.icd10Code.Active = true;

            #region duplicate-check
            var validation = _admin.GetICD10CodeValidation(); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.icd10Code.icd10CodeID.ToLower() == item.icd10CodeID.ToLower().Trim() && model.icd10Code.codeType.ToLower() == item.codeType.ToLower().Trim()) { v1++; break; }
            }
            if (v1 > 0)
            {
                model.programs = _admin.GetProgramValidation();

                ViewBag.Code = model.icd10Code.icd10CodeID;
                ViewBag.Program = model.icd10Code.codeType;

                return View(model);
            }
            #endregion

            var insert = _admin.InsertICD10Code(model.icd10Code);

            return RedirectToAction("Index", "Other");

        }

        public ActionResult ICD10_Edit(Guid id)
        {
            var model = new ICD10CodeVM();
            model.icd10Code = _admin.GetICD10CodeByID(id);
            model.programs = _admin.GetProgramValidation();

            ViewBag.Description = "";
            ViewBag.Code = "";

            return View(model);
        }
        [HttpPost]
        public ActionResult ICD10_Edit(ICD10CodeVM model)
        {
            //var program = _admin.GetProgramById(new Guid(model.icd10Code.codeType));
            var program = _admin.GetProgrambyName(model.icd10Code.codeType);

            model.icd10Code.icd10CodeID = model.icd10Code.icd10CodeID.Trim(); //hcare-1285
            model.icd10Code.codeType = program.ProgramName;
            model.icd10Code.subcode = program.code;
            model.icd10Code.modifiedDate = DateTime.Now;
            model.icd10Code.modifiedBy = User.Identity.Name;

            #region duplicate-check
            var validation = _admin.GetICD10CodeValidation(); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.icd10Code.Id != item.Id && model.icd10Code.icd10CodeID.ToLower() == item.icd10CodeID.ToLower().Trim() && model.icd10Code.codeType.ToLower() == item.codeType.ToLower().Trim()) { v1++; break; }
            }
            if (v1 > 0)
            {
                model.programs = _admin.GetProgramValidation();

                ViewBag.Code = model.icd10Code.icd10CodeID;
                ViewBag.Program = model.icd10Code.codeType;

                return View(model);
            }
            #endregion
            var update = _admin.UpdateICD10Code(model.icd10Code);

            return RedirectToAction("Index", "Other");
        }

        public ActionResult ICD10_Details(Guid id)
        {
            var model = new ICD10CodeVM();
            model.icd10Code = _admin.GetICD10CodeByID(id);

            return View(model);
        }

        public ActionResult ICD10CodeCheck(string code)
        {
            var options = _admin.GetICD10ByProgram(code);

            return Json(options);
        }

        #endregion

        #region hcare-1395
        public ActionResult Risk_Rating_Monitor_Create() //hcare-1395
        {
            var model = new RiskRatingMonitor();
            model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();
            model.RiskRatingTypes = _member.GetRiskRatingTypes().Where(x => x.RiskName.ToLower() != "none").ToList();

            return View(model);
        }
        [HttpPost]
        public ActionResult Risk_Rating_Monitor_Create(RiskRatingMonitor model) //hcare-1395
        {
            model.Id = Guid.NewGuid();
            model.Description = model.Description.Trim();
            if (!String.IsNullOrEmpty(Request.Query["Program"])) { model.Program = Request.Query["Program"]; } else { model.Program = null; }
            model.CreatedBy = User.Identity.Name;
            model.CreatedDate = DateTime.Now;
            model.Active = true;

            #region duplicate-check
            var validation = _admin.GetRiskRatingMonitorValidation();
            var v1 = 0;
            var v2 = 0;
            foreach (var item in validation)
            {
                if (model.Description.ToLower() == item.Description.ToLower().Trim()) { v1++; }
                if (model.From.ToLower() == item.From.ToLower() && model.To.ToLower() == item.To.ToLower() && model.Percentage == item.Percentage && model.Program == item.Program) { v2++; }
            }
            if (v1 > 0)
            {
                model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();
                model.RiskRatingTypes = _member.GetRiskRatingTypes().Where(x => x.RiskName.ToLower() != "none").ToList();
                ViewBag.Validation = "duplicate";

                return View(model);
            }
            if (v2 > 0)
            {
                model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();
                model.RiskRatingTypes = _member.GetRiskRatingTypes().Where(x => x.RiskName.ToLower() != "none").ToList();
                ViewBag.Validation = "duplicate";

                return View(model);
            }
            #endregion

            _admin.InsertRiskRatingMonitor(model);

            return RedirectToAction("Index", "Rules");
        }

        public ActionResult Risk_Rating_Monitor_Edit(Guid id) //hcare-1395
        {
            var model = _admin.GetRiskRatingMonitorByID(id);
            model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();
            model.RiskRatingTypes = _member.GetRiskRatingTypes().Where(x => x.RiskName.ToLower() != "none").ToList();

            return View(model);
        }
        [HttpPost]
        public ActionResult Risk_Rating_Monitor_Edit(RiskRatingMonitor model) //hcare-1395
        {
            model.Description = model.Description.Trim();
            if (!String.IsNullOrEmpty(Request.Query["Program"])) { model.Program = Request.Query["Program"]; } else { model.Program = null; }
            model.ModifiedBy = User.Identity.Name;
            model.ModifiedDate = DateTime.Now;

            #region duplicate-check
            var validation = _admin.GetRiskRatingMonitorValidation();
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.Id != item.Id && model.Description.ToLower() == item.Description.ToLower().Trim()) { v1++; }
            }
            if (v1 > 0)
            {
                model.Programs = _admin.GetPrograms().OrderBy(x => x.ProgramName).Where(x => x.Active == true).ToList();
                model.RiskRatingTypes = _member.GetRiskRatingTypes().Where(x => x.RiskName.ToLower() != "none").ToList();
                ViewBag.Validation = "duplicate";

                return View(model);
            }
            #endregion

            var result = _admin.UpdateRiskRatingMonitor(model);

            return RedirectToAction("Index", "Rules");
        }

        public ActionResult Risk_Rating_Monitor_Details(Guid id) //hcare-1395
        {
            var model = _admin.GetRiskRatingMonitorDetailsByID(id);

            return View(model);
        }

        public ActionResult RiskMonitorValidation(string name) //hcare-1395
        {
            var options = _admin.GetRiskRatingMonitorByName(name);

            return Json(options);
        }
        public ActionResult RiskMonitorDuplication(string from, string to, string percentage, string program) //hcare-1395
        {
            if (String.IsNullOrEmpty(program)) { program = null; }

            var options = _admin.GetRiskRatingMonitorValidation();
            foreach (var item in options)
            {
                if (from == item.From && to == item.To && Convert.ToInt32(percentage) == item.Percentage && program == item.Program)
                {
                    return Json(item);
                }
            }

            return Json("clear");
        }

        #endregion

    }
}