using HaloCareCore.DAL;
using HaloCareCore.Models.Management;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace HaloCareCore.Controllers
{
    public class CaseManagerController : Controller
    {
        private IAdminRepository _admin;
        private IMemberRepository _member;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CaseManagerController(OH17Context context, IConfiguration configuration)
        {
            _admin = new AdminRepository(context,configuration);
            _member = new MemberRepository(configuration, context);
        }
        // GET: CaseManager
        public ActionResult Index()
        {
            var model = _admin.GetCaseManagers();
            return View(model);
        }

        public ActionResult AddFromList(Guid DependentID)
        {
            var model = new CaseManagerHistory();
            model.dependantId = DependentID;
            ViewBag.caseMangers = new SelectList(_admin.GetCaseManagers(), "caseManagerNo", "cmFullName");
            return View(model);
        }

        [HttpPost]
        public ActionResult AddFromList(CaseManagerHistory model)
        {
            model.createdBy = User.Identity.Name;
            model.caseManagerId = Request.Query["caseMangers"].ToString();
            _admin.InsertCaseManagerHistory(model);

            return RedirectToAction("patientDashboard", "Member", new { DependentID = model.dependantId });
        }

        #region HCare-1176

        public ActionResult History(Guid DependentID, Guid pro)
        {
            var model = _admin.GetUserMemberHistoryList(DependentID, pro);
            return View(model);
        }

        public ActionResult AddCaseManagerToProfile(Guid DependentID, Guid pro)
        {
            var model = new UserMemberHistory();
            var program = _member.GetPrograms().Where(x => x.programID == pro).FirstOrDefault();
            var dependant = _member.GetDependentDetails(DependentID, pro);

            model.Users = _admin.GetAllowedUsersList(dependant.medicalAid.MedicalAidID, pro);
            model.DependantID = DependentID;
            //ViewBag.Users = new SelectList(_admin.GetUsersList().Where(x => x.Active == true), "userID", "userFullName");
            ViewBag.Users = new SelectList(_admin.GetAllowedUsersList(dependant.medicalAid.MedicalAidID, pro).Where(x => x.Active == true), "userID", "userFullName"); //HCare-1176
            ViewBag.Program = program.programID;
            return View(model);
        }

        [HttpPost]
        public ActionResult AddCaseManagerToProfile(UserMemberHistory model)
        {
            var program = _member.GetPrograms().Where(x => x.programID.ToString() == Request.Query["pro"]).FirstOrDefault();

            model.TaskID = null;
            model.AssignmentID = null;
            model.EffectiveDate = DateTime.Now;
            model.CreatedBy = User.Identity.Name;
            model.CreatedDate = DateTime.Now;
            model.UserID = new Guid(Request.Query["Users"]);
            var user = _admin.GetUserById(model.UserID);
            model.UserFullName = user.userFullName;
            model.Program = program.code;
            model.Comment = null;
            model.Active = true;

            _admin.InsertUserMemberHistory(model);

            return RedirectToAction("patientDashboard", "Member", new { DependentID = model.DependantID, pro = program.programID });

        }

        public ActionResult EditCaseManagerOnProfile(int Id, Guid pro)
        {
            var model = _admin.GetUserMemberByID(Id, pro);
            var dependant = _member.GetDependentDetails(model.DependantID, pro);
            //ViewBag.Users = new SelectList(_admin.GetUsersList(), "userID", "userFullName", model.UserID);
            ViewBag.Users = new SelectList(_admin.GetAllowedUsersList(dependant.medicalAid.MedicalAidID, pro).Where(x => x.Active == true), "userID", "userFullName", model.UserID); //HCare-1176

            var program = _member.GetPrograms().Where(x => x.code == model.Program).FirstOrDefault();
            ViewBag.pro = program.programID;

            return View(model);
        }

        [HttpPost]
        public ActionResult EditCaseManagerOnProfile(UserMemberHistory model)
        {
            var program = _member.GetPrograms().Where(x => x.programID.ToString() == Request.Query["pro"]).FirstOrDefault();

            if (!String.IsNullOrEmpty(Request.Query["TaskID"])) { model.TaskID = Convert.ToInt32(Request.Query["TaskID"]); }
            if (!String.IsNullOrEmpty(Request.Query["AssignmentID"])) { model.AssignmentID = Convert.ToInt32(Request.Query["AssignmentID"]); }

            model.ModifiedDate = DateTime.Now;
            model.ModifiedBy = User.Identity.Name;
            model.UserID = new Guid(Request.Query["Users"]);
            var user = _admin.GetUserById(model.UserID);
            model.UserFullName = user.userFullName;

            _admin.UpdateUserMemberHistory(model);

            return RedirectToAction("History", "CaseManager", new { DependentID = model.DependantID, pro = program.programID });
        }

        public ActionResult DetailsOfCaseManagerOnProfile(int Id, Guid pro)
        {
            var model = _admin.GetUserMemberByID(Id, pro);
            ViewBag.Users = new SelectList(_admin.GetUsersList(), "userID", "userFullName", model.UserID);
            var program = _member.GetPrograms().Where(x => x.code == model.Program).FirstOrDefault();
            ViewBag.pro = program.programID;

            return View(model);
        }


        #endregion


        // GET: CaseManager/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CaseManager/Create
        public ActionResult Create(Guid? DependentID)
        {
            return View();
        }

        // POST: CaseManager/Create
        [HttpPost]
        public ActionResult Create(CaseManagers model)
        {
            try
            {
                var caseManager = _admin.GetCaseManager(model.caseManagerNo.ToString());
                if (caseManager == null)
                {
                    model.createdBy = User.Identity.Name;
                    model.createdDate = DateTime.Now;
                    model.Active = true;
                    _admin.InsertCaseManager(model);
                }

                if (Request.Query["DependentID"].ToString() != null)
                {
                    var casehistory = new CaseManagerHistory();
                    casehistory.caseManagerId = caseManager.caseManagerNo;
                    casehistory.createdBy = User.Identity.Name;
                    _admin.InsertCaseManagerHistory(casehistory);

                    return RedirectToAction("patientDashboard", "Member", new { DependentID = casehistory.dependantId });
                }
                //redirect to list
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: CaseManager/Edit/5
        public ActionResult Edit(string id)
        {
            var model = _admin.GetCaseManager(id);
            return View(model);
        }

        // POST: CaseManager/Edit/5
        [HttpPost]
        public ActionResult Edit(CaseManagers model)
        {
            try
            {
                model.modifiedBy = User.Identity.Name;
                model.modifiedDate = DateTime.Now;
                var result = _member.UpdateCaseManager(model);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: CaseManager/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CaseManager/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult CMNumberCheck(string cmNumber)
        {
            var options = _admin.GetCaseManagerByNumber(cmNumber);

            return Json(options);
        }
        public ActionResult NameCheck(string firstName, string lastName)
        {
            var options = _admin.GetCaseManagerByName(firstName, lastName);

            return Json(options);
        }


    }
}
