using HaloCareCore.DAL;
using HaloCareCore.Models;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Validation;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;

namespace HaloCareCore.Controllers
{
    public class TasksController : Controller
    {
        private IAdminRepository _admin;
        private OH17Context context;
        private readonly IConfiguration Configuration;
        public TasksController()
        {
            _admin = new AdminRepository(context, Configuration);
        }

        //========================================================================= assignment-types =========================================================================//
        public ActionResult Index_AssignmentType()
        {
            var viewModel = new AssignmentTypeViewModel()
            {
                assignmentTypes = _admin.GetAssignmentTypes(),
                assignmentItemTypes = _admin.GetAssignmentItems(),
                assignmentItemTaskTypes = _admin.GetAssignmentItemTaskTypes(),
                taskTypeRequirements = _admin.GetTaskTypeRequirements(),
                taskRequirementItemLinkings = _admin.GetLinkedTasks(),
                hospitalizationICD10types = _admin.GetHospICD10s()
            };
            return View(viewModel);
        }

        public ActionResult Create_AssignmentType()
        {
            ViewBag.Code = "";
            ViewBag.Description = "";
            ViewBag.CodeDescription = "";

            return View();
        }
        [HttpPost]
        public ActionResult Create_AssignmentType(AssignmentTypes model)
        {
            model.assignmentType = model.assignmentType.Trim(); //hcare-1285
            model.assignmentDescription = model.assignmentDescription.Trim(); //hcare-1285
            model.active = true;

            #region duplicate-check
            var validation = _admin.GetAssignmentTypeValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.assignmentType.ToLower() == item.assignmentType.ToLower().Trim() && model.assignmentDescription.ToLower() == item.assignmentDescription.ToLower().Trim()) { v1++; }
                if (model.assignmentType.ToLower() == item.assignmentType.ToLower().Trim()) { v2++; }
                if (model.assignmentDescription.ToLower() == item.assignmentDescription.ToLower().Trim()) { v3++; }
            }
            if (v1 > 0)
            {
                ViewBag.CodeDescription = model.assignmentType + " " + model.assignmentDescription;
                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.Code = model.assignmentType;
                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.Description = model.assignmentDescription;
                return View(model);
            }
            #endregion

            var result = _admin.AddNewAssignmentType(model);

            return RedirectToAction("Index_AssignmentType");
        }

        public ActionResult Edit_AssignmentType(string id)
        {
            var model = _admin.GetAssignmentTypeById(id);
            ViewBag.Description = "";
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit_AssignmentType(AssignmentTypes model)
        {
            model.assignmentType = model.assignmentType.Trim(); //hcare-1285
            model.assignmentDescription = model.assignmentDescription.Trim(); //hcare-1285

            #region duplicate-check
            var validation = _admin.GetAssignmentTypeValidation(); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.assignmentType != item.assignmentType.Trim() && model.assignmentDescription.ToLower() == item.assignmentDescription.ToLower().Trim()) { v1++; }
            }
            if (v1 > 0)
            {
                ViewBag.Description = model.assignmentDescription;
                return View(model);
            }
            #endregion

            var result = _admin.UpdateAssignmentType(model);

            return RedirectToAction("Index_AssignmentType");
        }

        public ActionResult AssignmentTypeCodeCheck(string assignmentType) //HCare-956
        {
            var options = _admin.GetAssignmentTypeByCode(assignmentType);

            return Json(options);
        }

        public ActionResult AssignmentTypeNameCheck(string assignmentDescription) //HCare-956
        {
            var options = _admin.GetAssignmentTypeByName(assignmentDescription);

            return Json(options);
        }


        //========================================================================= assignment-items =========================================================================//
        public ActionResult Index_AssignmentItem()
        {
            var model = _admin.GetAssignmentItems();

            return View(model);
        }

        public ActionResult Create_AssignmentItem()
        {
            ViewBag.Code = "";
            ViewBag.Description = "";
            ViewBag.CodeDescription = "";
            ViewBag.sourceType = new SelectList(_admin.GetAssignmentTypes(), "assignmentType", "assignmentDescription");
            ViewBag.programList = new SelectList(_admin.GetPrograms(), "code", "ProgramName"); //Hcare-1048

            return View();
        }
        [HttpPost]
        public ActionResult Create_AssignmentItem(AssignmentItemTypes model)
        {
            model.assignmentItemType = model.assignmentItemType.Trim(); //hcare-1285
            model.itemDescription = model.itemDescription.Trim(); //hcare-1285
            model.active = true;

            #region duplicate-check
            var validation = _admin.GetAssignmentItemValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.assignmentItemType.ToLower() == item.assignmentItemType.ToLower().Trim() && model.itemDescription.ToLower() == item.itemDescription.ToLower().Trim()) { v1++; }
                if (model.assignmentItemType.ToLower() == item.assignmentItemType.ToLower().Trim()) { v2++; }
                if (model.itemDescription.ToLower() == item.itemDescription.ToLower().Trim()) { v3++; }
            }
            if (v1 > 0)
            {
                ViewBag.sourceType = new SelectList(_admin.GetAssignmentTypes(), "assignmentType", "assignmentDescription");
                //ViewBag.programList = new SelectList(_admin.GetPrograms(), "code", "ProgramName"); //Hcare-1048
                ViewBag.CodeDescription = model.assignmentItemType + " " + model.itemDescription;

                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.sourceType = new SelectList(_admin.GetAssignmentTypes(), "assignmentType", "assignmentDescription");
                //ViewBag.programList = new SelectList(_admin.GetPrograms(), "code", "ProgramName"); //Hcare-1048
                ViewBag.Code = model.assignmentItemType;

                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.sourceType = new SelectList(_admin.GetAssignmentTypes(), "assignmentType", "assignmentDescription");
                //ViewBag.programList = new SelectList(_admin.GetPrograms(), "code", "ProgramName"); //Hcare-1048
                ViewBag.Description = model.itemDescription;

                return View(model);
            }
            #endregion
            
            var result = _admin.AddNewAssignmentItem(model);

            return RedirectToAction("Index_AssignmentType");
        }

        public ActionResult Edit_AssignmentItem(string id)
        {
            var model = _admin.GetAssignmentItemById(id);

            //ViewBag.programList = new SelectList(_admin.GetPrograms(), "code", "ProgramName", model.program); //Hcare-1048
            ViewBag.sourceType = new SelectList(_admin.GetAssignmentTypes().Where(x=>x.active==true), "assignmentType", "assignmentDescription", model.sourceType);
            ViewBag.Description = "";

            return View(model);
        }
        [HttpPost]
        public ActionResult Edit_AssignmentItem(AssignmentItemTypes model)
        {
            model.assignmentItemType = model.assignmentItemType.Trim(); //hcare-1285
            model.itemDescription = model.itemDescription.Trim(); //hcare-1285

            #region duplicate-check
            var validation = _admin.GetAssignmentItemValidation(); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.assignmentItemType != item.assignmentItemType.Trim() && model.itemDescription.ToLower() == item.itemDescription.ToLower().Trim()) { v1++; }
            }
            if (v1 > 0)
            {
                //ViewBag.programList = new SelectList(_admin.GetPrograms(), "code", "ProgramName", model.program); //Hcare-1048
                ViewBag.sourceType = new SelectList(_admin.GetAssignmentTypes(), "assignmentType", "assignmentDescription", model.sourceType);
                ViewBag.Description = model.itemDescription;

                return View(model);
            }
            #endregion

            var result = _admin.UpdateAssignmentItem(model);

            return RedirectToAction("Index_AssignmentType");
        }

        public ActionResult AssignmentItemTypeCodeCheck(string assignmentItemType) //HCare-956
        {
            var options = _admin.GetAssignmentItemTypeByCode(assignmentItemType);

            return Json(options);
        }

        public ActionResult AssignmentItemTypeNameCheck(string itemDescription) //HCare-956
        {
            var options = _admin.GetAssignmentItemTypeByName(itemDescription);

            return Json(options);
        }


        //============================================================================ task-types ============================================================================//
        public ActionResult Index_TaskType()
        {
            var model = _admin.GetAssignmentItemTaskTypes();
            return View(model);
        }

        public ActionResult Create_TaskType()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create_TaskType(AssignmentItemTaskTypes model)
        {
            model.active = true;
            var result = _admin.AddNewTaskType(model);

            return RedirectToAction("Index_TaskType");
        }

        public ActionResult Edit_TaskType(string id)
        {
            var model = _admin.GetTaskTypeById(id);

            return RedirectToAction("Index_AssignmentType");
        }
        [HttpPost]
        public ActionResult Edit_TaskType(AssignmentItemTaskTypes model)
        {
            try
            {
                var result = _admin.UpdateTaskType(model);

                return RedirectToAction("Index_AssignmentType");
            }
            catch
            {
                return View();
            }
        }


        //======================================================================== task-requirements ========================================================================//
        public ActionResult Index_TaskRequirement()
        {
            var model = _admin.GetTaskTypeRequirements();
            return View(model);
        }

        public ActionResult Create_TaskRequirement()
        {
            ViewBag.Description = "";
            return View();
        }
        [HttpPost]
        public ActionResult Create_TaskRequirement(TaskTypeRequirements model)
        {
            model.createdDate = DateTime.Now;
            model.createdBy = User.Identity.Name;
            model.requirementText = model.requirementText.Trim(); //hcare-1285
            model.active = true;
            
            #region duplicate-check
            var validation = _admin.GetTaskTypeRequirementValidation(); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.requirementText.ToLower() == item.requirementText.ToLower().Trim()) { v1++; }
            }
            if (v1 > 0)
            {
                ViewBag.Description = model.requirementText;
                return View(model);
            }
            #endregion

            var result = _admin.AddNewTaskReq(model);

            return RedirectToAction("Index_AssignmentType");
        }

        public ActionResult Edit_TaskRequirement(int id)
        {
            var model = _admin.GetTaskReqById(id);

            ViewBag.templateID = new SelectList(_admin.GetSMSmessageTemplate(), "templateID", "template");
            ViewBag.emailTemplatesID = new SelectList(_admin.GetemailTemplate(), "templateID", "template");
            ViewBag.Description = "";

            return View(model);
        }
        [HttpPost]
        public ActionResult Edit_TaskRequirement(TaskTypeRequirements model)
        {
            model.modifiedDate = DateTime.Now;
            model.modifiedBy = User.Identity.Name;
            model.requirementText = model.requirementText.Trim(); //hcare-1285

            #region duplicate-check
            var validation = _admin.GetTaskTypeRequirementValidation(); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.id != item.id && model.requirementText.ToLower() == item.requirementText.ToLower().Trim()) { v1++; }
            }
            if (v1 > 0)
            {
                ViewBag.templateID = new SelectList(_admin.GetSMSmessageTemplate(), "templateID", "template");
                ViewBag.emailTemplatesID = new SelectList(_admin.GetemailTemplate(), "templateID", "template");
                ViewBag.Description = model.requirementText;

                return View(model);
            }
            #endregion

            var result = _admin.UpdateTaskReq(model);

            return RedirectToAction("Index_AssignmentType");
        }


        //=========================================================================== task-linking ===========================================================================//
        public ActionResult Index_TaskLinking()
        {
            var model = _admin.GetLinkedTasks().Where(x => x.active == true);

            return View(model);
        }

        public ActionResult Create_TaskLinking()
        {
            ViewBag.itemId = new SelectList(_admin.GetAssignmentItems(), "assignmentItemType", "itemDescription");
            ViewBag.taskType = new SelectList(_admin.GetAssignmentItemTaskTypes(), "taskId", "taskDescription");
            ViewBag.Item = "";
            ViewBag.Type = "";
            ViewBag.Requirement = "";

            return View();
        }
        [HttpPost]
        public ActionResult Create_TaskLinking(TaskRequirementItemLinking model)
        {
            model.active = true;
            model.createdBy = User.Identity.Name;
            model.createdDate = DateTime.Now;
            model.templateID = model.templateID.Trim(); //hcare-1285

            #region duplicate-check
            var taskdescription = _admin.GetAssignmentItems().Where(x => x.assignmentItemType == model.itemId).Select(x=>x.itemDescription).FirstOrDefault();
            var validation = _admin.GetLinkedTasksByID(model.itemId); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.itemId == item.itemId && model.taskType == item.taskType && model.requirementId == item.requirementId) { v1++; break; }
            }
            if (v1 > 0)
            {
                var itemID = _admin.GetAssignmentItems().Where(x => x.assignmentItemType == model.itemId).Select(x => x.itemDescription).FirstOrDefault();
                var tasktype = _admin.GetAssignmentItemTaskTypes().Where(x => x.taskId == model.taskType).Select(x => x.taskDescription).FirstOrDefault();

                ViewBag.itemId = new SelectList(_admin.GetAssignmentItems(), "assignmentItemType", "itemDescription");
                ViewBag.taskType = new SelectList(_admin.GetAssignmentItemTaskTypes(), "taskId", "taskDescription");
                ViewBag.Item = itemID;
                ViewBag.Type = tasktype;
                ViewBag.Requirement = model.requirementId;

                return View(model);
            }
            #endregion

            var result = _admin.AddLinkedTasks(model);

            return RedirectToAction("Index_AssignmentType");
        }

        public ActionResult Edit_TaskLinking(int id)
        {
            var model = _admin.GetLinkedTaskById(id);

            ViewBag.itemId = new SelectList(_admin.GetAssignmentItems(), "assignmentItemType", "itemDescription");
            ViewBag.taskType = new SelectList(_admin.GetAssignmentItemTaskTypes(), "taskId", "taskDescription");

            return View(model);
        }
        [HttpPost]
        public ActionResult Edit_TaskLinking(TaskRequirementItemLinking model)
        {
            model.modifiedDate = DateTime.Now;
            model.modifiedBy = User.Identity.Name;
            model.templateID = model.templateID.Trim(); //hcare-1285

            #region duplicate-check
            var taskdescription = _admin.GetAssignmentItems().Where(x => x.assignmentItemType == model.itemId).Select(x => x.itemDescription).FirstOrDefault();
            var validation = _admin.GetLinkedTasksByID(model.itemId); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.id != item.id && model.itemId == item.itemId && model.taskType == item.taskType && model.requirementId == item.requirementId) { v1++; break; }
            }
            if (v1 > 0)
            {
                var itemID = _admin.GetAssignmentItems().Where(x => x.assignmentItemType == model.itemId).Select(x => x.itemDescription).FirstOrDefault();
                var tasktype = _admin.GetAssignmentItemTaskTypes().Where(x => x.taskId == model.taskType).Select(x => x.taskDescription).FirstOrDefault();

                ViewBag.itemId = new SelectList(_admin.GetAssignmentItems(), "assignmentItemType", "itemDescription");
                ViewBag.taskType = new SelectList(_admin.GetAssignmentItemTaskTypes(), "taskId", "taskDescription");
                ViewBag.Item = itemID;
                ViewBag.Type = tasktype;
                ViewBag.Requirement = model.requirementId;

                return View(model);
            }
            #endregion

            var result = _admin.UpdateLinkedTasks(model);

            return RedirectToAction("Index_AssignmentType");

        }

        public ActionResult TaskRequirementNameCheck(string requirementText) //HCare-956
        {
            var options = _admin.GetTaskRequirementByName(requirementText);

            return Json(options);
        }


        //========================================================================= hospital-icd10 =========================================================================//
        //Hcare-831
        public ActionResult Create_HospIcd10()
        {
            ViewBag.assignmentItemType = new SelectList(_admin.GetAssignmentItems().Where(x=>x.active==true), "assignmentItemType", "itemDescription");
            ViewBag.programCode = new SelectList(_admin.GetPrograms().Where(x=>x.Active==true), "code", "ProgramName");
            ViewBag.icdcode = new SelectList(_admin.GetICD10Masters().Where(x=>x.Active==true), "ICD10Code", "ICD10Code");
            ViewBag.Program = "";
            ViewBag.ICD10 = "";
            ViewBag.Assignment = "";

            return View();
        }
        [HttpPost]
        public ActionResult Create_HospIcd10(HospitalizationICD10types model)
        {
            model.createdBy = User.Identity.Name;
            model.icdcode = model.icdcode.Trim(); //hcare-1285
            model.Active = true;

            #region duplicate-check
            var validation = _admin.GetHospICD10sByProgram(model.programCode); //hcare-1298 - checked
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.programCode == item.programCode && model.icdcode == item.icdcode.Trim() && model.assignmentItemType == item.assignmentItemType) { v1++; break; }
            }
            if (v1 > 0)
            {
                var assignment = _admin.GetAssignmentItems().Where(x => x.assignmentItemType == model.assignmentItemType).Select(x => x.itemDescription).FirstOrDefault();
                var program = _admin.GetProgrambyCode(model.programCode);

                ViewBag.assignmentItemType = new SelectList(_admin.GetAssignmentItems(), "assignmentItemType", "itemDescription");
                ViewBag.programCode = new SelectList(_admin.GetPrograms(), "code", "ProgramName");
                ViewBag.Program = program.ProgramName;
                ViewBag.ICD10 = model.icdcode;
                ViewBag.Assignment = assignment;

                return View(model);
            }
            #endregion

            var result = _admin.AddNewHospICD10(model);

            return RedirectToAction("Index_AssignmentType");
        }

        public ActionResult Edit_HospIcd10(int id)
        {
            var model = _admin.GetHospICD10ById(id);
            ViewBag.assignmentItemType = new SelectList(_admin.GetAssignmentItems().Where(x=>x.active==true), "assignmentItemType", "itemDescription");
            ViewBag.programCode = new SelectList(_admin.GetPrograms().Where(x=>x.Active==true), "code", "ProgramName");
            ViewBag.icdcode = new SelectList(_admin.GetICD10Masters().Where(x=>x.Active==true), "ICD10Code", "ICD10Code");

            ViewBag.Program = "";
            ViewBag.ICD10 = "";
            ViewBag.Assignment = "";

            return View(model);
        }
        [HttpPost]
        public ActionResult Edit_HospIcd10(HospitalizationICD10types model)
        {
            model.modifiedBy = User.Identity.Name;
            model.icdcode = model.icdcode.Trim(); //hcare-1285

            #region duplicate-check
            var validation = _admin.GetHospICD10sByProgram(model.programCode); //hcare-1298 - checked
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.id != item.id && model.programCode == item.programCode && model.icdcode == item.icdcode.Trim() && model.assignmentItemType == item.assignmentItemType) { v1++; break; }
            }
            if (v1 > 0)
            {
                var assignment = _admin.GetAssignmentItems().Where(x => x.assignmentItemType == model.assignmentItemType).Select(x => x.itemDescription).FirstOrDefault();
                var program = _admin.GetProgrambyCode(model.programCode);

                ViewBag.assignmentItemType = new SelectList(_admin.GetAssignmentItems(), "assignmentItemType", "itemDescription");
                ViewBag.programCode = new SelectList(_admin.GetPrograms(), "code", "ProgramName");
                ViewBag.Program = program.ProgramName;
                ViewBag.ICD10 = model.icdcode;
                ViewBag.Assignment = assignment;

                return View(model);
            }
            #endregion

            var result = _admin.UpdateHospICD10(model);

            return RedirectToAction("Index_AssignmentType");
        }

    }
}