using HaloCareCore.DAL;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace HaloCareCore.Controllers
{
    public class ManagementStatusController : Controller
    {
        private IMemberRepository _member;
        private IAdminRepository _admin;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;

        public ManagementStatusController(OH17Context context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _member = new MemberRepository(configuration,context);
            _admin = new AdminRepository(context,configuration);
        }

        public ActionResult Index()
        {
            var model = _member.GetManagementStatusList();
            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.statusTypes = new SelectList(_admin.GetStatusTypes().ToList(), "TypeCode", "TypeName");
            ViewBag.programs = new SelectList(_admin.GetPrograms().ToList(), "code", "programName");
            ViewBag.Code = "";
            ViewBag.Description = "";
            return View();
        }
        [HttpPost]
        public ActionResult Create(ManagementStatus model)
        {
            model.createdBy = User.Identity.Name; //HCare-785
            model.createdDate = DateTime.Now; //HCare-785
            model.statusCode = model.statusCode.ToUpper().Trim(); //hcare-1285
            model.statusName = model.statusName.Trim(); //hcare-1285
            model.statusType = Request.Query["statusTypes"]; //hcare-1298
            model.programCode = Request.Query["programs"]; //hcare-1298
            model.active = true;

            #region duplicate-check
            var validation = _member.GetManagementStatusValidation(); //hcare-1298
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.statusCode.ToLower() == item.statusCode.ToLower().Trim() && model.statusName.ToLower() == item.statusName.ToLower().Trim()) { v1++; break; }
                if (model.statusCode.ToLower() == item.statusCode.ToLower().Trim()) { v2++; }
                if (model.statusName.ToLower() == item.statusName.ToLower().Trim()) { v3++; break; }
            }
            if (v1 > 0)
            {
                ViewBag.statusTypes = new SelectList(_admin.GetStatusTypes().ToList(), "TypeCode", "TypeName");
                ViewBag.programs = new SelectList(_admin.GetPrograms().ToList(), "code", "programName");
                ViewBag.Code = model.statusCode;
                ViewBag.Description = model.statusName;

                return View(model);
            }
            if (v2 > 0)
            {
                ViewBag.statusTypes = new SelectList(_admin.GetStatusTypes().ToList(), "TypeCode", "TypeName");
                ViewBag.programs = new SelectList(_admin.GetPrograms().ToList(), "code", "programName");
                ViewBag.Code = model.statusCode;

                return View(model);
            }
            if (v3 > 0)
            {
                ViewBag.statusTypes = new SelectList(_admin.GetStatusTypes().ToList(), "TypeCode", "TypeName");
                ViewBag.programs = new SelectList(_admin.GetPrograms().ToList(), "code", "programName");
                ViewBag.Description = model.statusName;

                return View(model);
            }
            #endregion

            _member.InsertManagementStatusCode(model);

            return RedirectToAction("Index");
        }

        public ActionResult Edit(string id)
        {
            var model = _member.GetManagementStatus(id);
            ViewBag.statusTypes = new SelectList(_admin.GetStatusTypes().ToList(), "TypeCode", "TypeName", model.statusType);
            ViewBag.programs = new SelectList(_admin.GetPrograms().ToList(), "code", "programName", model.programCode);
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(ManagementStatus model)
        {
            model.statusType = Request.Query["statusTypes"].ToString();
            model.programCode = Request.Query["programs"].ToString();
            model.modifiedBy = User.Identity.Name; //HCare-785
            model.modifiedDate = DateTime.Now; //HCare-785
            model.statusCode = model.statusCode.Trim(); //hcare-1285
            model.statusName = model.statusName.Trim(); //hcare-1285

            #region duplicate-check
            var validation = _member.GetManagementStatusValidation(); //hcare-1298
            var v1 = 0;
            foreach (var item in validation)
            {
                if (model.statusCode != item.statusCode && model.statusName.ToLower() == item.statusName.ToLower().Trim()) { v1++; break; }
            }
            if (v1 > 0)
            {
                ViewBag.statusTypes = new SelectList(_admin.GetStatusTypes().ToList(), "TypeCode", "TypeName", model.statusType);
                ViewBag.programs = new SelectList(_admin.GetPrograms().ToList(), "code", "programName", model.programCode); ;
                ViewBag.Description = model.statusName;

                return View(model);
            }
            #endregion

            _member.UpdateManagementStatusCode(model);

            return RedirectToAction("Index");
        }

        public ActionResult Details(string id)
        {
            var model = _member.GetManagementStatus(id);

            return View(model);
        }

        public ActionResult ManagementStatusCodeCheck(string statusCode) //HCare-956
        {
            var options = _member.GetMStatusByCode(statusCode);

            return Json(options);
        }
        public ActionResult ManagementStatusNameCheck(string statusName) //HCare-956
        {
            var options = _member.GetMStatusByName(statusName);

            return Json(options);
        }


        //=========================================================================================================================================================================//
        //                                                                           ManagementStatus Reasons                                                                      // 
        //=========================================================================================================================================================================//

        public ActionResult msReason_Index()
        {
            var viewModel = new ManagementStatusVM()
            {
                managementStatuses = _member.GetManagementStatus(),
                msReasons = _member.GetManagementStatusReasons(),
            };
            return View(viewModel);
        }

        public ActionResult msReason_Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult msReason_Create(ManagementStatus_DeactivatedReasons model)
        {
            //validation
            var validationName = _member.GetManagementStatusReasonByName(model.name);
            var validationDescription = _member.GetManagementStatusReasonByReason(model.reason);
            var code = Request.Query["reasonCode"];
            var name = Request.Query["name"];
            var description = Request.Query["reason"];

            if (string.IsNullOrEmpty(name.ToString()) && string.IsNullOrEmpty(description.ToString()))
            {
                ViewBag.both = "Please complete the required fields";
                ModelState.AddModelError("name", "Name required!");
                ModelState.AddModelError("reason", "Reason required!");
                return View(model);
            }
            else if (string.IsNullOrEmpty(name.ToString()))
            {
                ViewBag.name = "Please add a name";
                ModelState.AddModelError("name", "Name required!");
                return View(model);
            }
            else if (string.IsNullOrEmpty(description.ToString()))
            {
                ViewBag.description = "Please add a reason";
                ModelState.AddModelError("reason", "Reason already exists!");
                return View(model);
            }
            else
            {
                if (validationName == null && validationDescription == null)
                {
                    model.createdDate = DateTime.Now;
                    model.createdBy = User.Identity.Name;
                    model.active = true;

                    var create = _member.InsertManagementStatusReasons(model);
                    return RedirectToAction("Index", "ManagementStatus");
                }
                else if (validationName != null && validationDescription != null)
                {
                    ViewBag.bothDup = "Both name & reason exist!";
                    ModelState.AddModelError("name", "Code already exists!");
                    ModelState.AddModelError("reason", "Program name already exists!");

                    return View(model);
                }
                else if (validationName != null)
                {
                    ViewBag.codeDup = "Name already exists!";
                    ModelState.AddModelError("name", "Name already exists!");
                    return View(model);
                }
                else if (validationDescription != null)
                {
                    ViewBag.descriptionDup = "Reason already exists!";
                    ModelState.AddModelError("reason", "Reason already exists!");
                    return View(model);

                }
                else
                {
                    return View(model);
                }
            }
        }

        public ActionResult msReason_Edit(int id)
        {
            var details = _member.GetManagementStatusReasonByID(id);
            return View(details);
        }

        [HttpPost]
        public ActionResult msReason_Edit(ManagementStatus_DeactivatedReasons model)
        {
            model.modifiedDate = DateTime.Now;
            model.modifiedBy = User.Identity.Name;

            var update = _member.UpdateManagementStatusReasons(model);

            return RedirectToAction("Index", "ManagementStatus");
        }

        public ActionResult msReason_Details(int id)
        {
            var details = _member.GetManagementStatusReasonByID(id);
            return View(details);
        }



    }
}