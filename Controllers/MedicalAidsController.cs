using HaloCareCore.DAL;
using HaloCareCore.Filters;
using HaloCareCore.Models;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;

namespace HaloCareCore.Controllers
{
    public class MedicalAidsController : Controller
    {
        private IMemberRepository _member;
        private IMedicalAidRepository _medical;
        private IAdminRepository _admin;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MedicalAidsController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, OH17Context context)
        {
            this._medical = new MedicalAidRepository(configuration,context);
            this._member = new MemberRepository(configuration,context);
            this._admin = new AdminRepository(context, configuration);
        }


        //================================================================================== Medical scheme ==================================================================================//

        public ActionResult Index()
        {
            List<MedicalAid> model = _member.GetMedicalAids();
            return View(model);
        }

        public ActionResult IndexVM()
        {
            var UserName = User.Identity.Name;
            var user = _admin.GetUserByUsername(UserName);

            var viewModel = new SchemeOptionViewModel()
            {
                MedicalAidList = _admin.GetMedicalAidInformation(user.userID).OrderByDescending(x=>x.Active).ThenByDescending(x=>x.UserAccess).ThenBy(x=>x.MedicalAidName).ToList(), //hcare-1346
                medicalAidVMs = _member.GetAllowedMedicalAidPlan(), //hcare-1288
            };
            return View(viewModel);
        }

        public ActionResult Create()
        {

            var model = new MedicalAidViewModel();
            ViewBag.referalID = new SelectList(_member.getReferral(), "referral", "referral");
            //model.MedicalAidAccounts = _member.GetMedicalAidAccounts();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MedicalAidViewModel model)
        {
            //HCare-1175
            if (Request.Query["referalID"].ToString() != null) { model.medicalAid.Referral = Request.Query["referalID"].ToString(); }

            var medicalaid = _medical.GetMedicalAid(model.medicalAid.medicalAidCode);
            if (medicalaid != null)
            {
                ViewBag.Error = "Medical scheme code is already loaded on the system";
                ViewBag.referalID = new SelectList(_member.getReferral(), "referral", "referral");
                return View(model);
            }

            //HCare-1216
            var medicalaidName = _medical.GetMedicalAidName(model.medicalAid.Name);
            if (medicalaidName != null)
            {
                ViewBag.Error = "Medical scheme name is already loaded on the system";
                ViewBag.referalID = new SelectList(_member.getReferral(), "referral", "referral");
                return View(model);
            }

            //HCare-1086
            if (model.MedicalAidClaimCodes.claimCode != null)
            {
                if (_medical.GetClaimCode(model.MedicalAidClaimCodes.claimCode).Count() > 0)
                {
                    ViewBag.referalID = new SelectList(_member.getReferral(), "referral", "referral");
                    ViewBag.Error = "Claim code already exists!";
                    return View(model);
                }
            }

            model.medicalAid.medicalAidCode = model.medicalAid.medicalAidCode.ToUpper().Trim(); //hcare-1298
            model.medicalAid.Name = model.medicalAid.Name.Trim(); //hcare-1298
            model.medicalAid.createdBy = User.Identity.Name;
            model.medicalAid.disclaimer = true; //HCare-864
            model.medicalAidSettings.medicalAidId = _medical.InsertMedicalAid(model.medicalAid);

            model.medicalAidSettings.createdBy = User.Identity.Name;
            _medical.InsertMedicalAidSettings(model.medicalAidSettings);

            //HCare-1086
            model.MedicalAidClaimCodes.createdBy = User.Identity.Name;
            model.MedicalAidClaimCodes.medicalAidCode = model.medicalAid.medicalAidCode;

            _medical.InsertMedicalAidClaimingCode(model.MedicalAidClaimCodes);

            return RedirectToAction("IndexVM");
        }

        public ActionResult Edit(Guid id)
        {
            MedicalAidViewModel viewModel = _medical.GetMedicalAidDetails(id);
            viewModel.Programs = _admin.GetMedicalAidPrograms(id);
            var medicalAid = _member.GetMedicalAidById(id);
            ViewBag.AccountCode = new SelectList(_member.GetMedicalAidAccounts(), "Code", "Name", medicalAid.AccountCode);

            ViewBag.referalID = new MultiSelectList(_member.getReferral(), "Referral", "referral", _member.getReferralByMedicalAid(medicalAid.MedicalAidID).ToString().Split(',').ToList());

            viewModel.referralFrom = _member.getReferralFrom().ToList();

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult Edit(List<MedicalAidProgramViewModel> Programs, MedicalAidViewModel model)
        {
            var claimcode = _member.GetClaimCodeByMedicalAidId(model.medicalAid.medicalAidCode);

            model.medicalAidSettings.modifiedBy = User.Identity.Name;
            model.medicalAidSettings.medicalAidId = model.medicalAid.MedicalAidID;
            model.medicalAid.modifiedBy = User.Identity.Name;
            model.medicalAid.AccountCode = Request.Query["AccountCode"];
            model.medicalAid.medicalAidCode = model.medicalAid.medicalAidCode.ToUpper().Trim(); //hcare-1298
            model.medicalAid.Name = model.medicalAid.Name.Trim(); //hcare-1298
            if (Request.Query["selectedReferals"].ToString() != null) { model.medicalAid.Referral = Request.Query["selectedReferals"].ToString(); }//1175

            #region duplicate-check: hcare-1298
            var validation = _member.GetMedicalAidList().Where(x => x.Active == true).ToList();
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if ((model.medicalAid.MedicalAidID != item.MedicalAidID && model.medicalAid.Name == item.Name) || (model.medicalAid.MedicalAidID != item.MedicalAidID && model.medicalAid.medicalAidCode == item.medicalAidCode)) { v1++; break; }
                if (model.medicalAid.MedicalAidID != item.MedicalAidID && model.medicalAid.Name == item.Name) { v2++; break; }
                if (model.medicalAid.MedicalAidID != item.MedicalAidID && model.medicalAid.medicalAidCode == item.medicalAidCode) { v3++; break; }
            }
            if (v1 > 0)
            {
                ViewBag.referalID = new SelectList(_member.getReferral(), "referral", "referral");
                model.referralFrom = _member.getReferralFrom().ToList();
                ViewBag.Error = "Medical aid name & medical aid account already exists!";

                return View(model);
            }
            else if (v2 > 0)
            {
                ViewBag.referalID = new SelectList(_member.getReferral(), "referral", "referral");
                model.referralFrom = _member.getReferralFrom().ToList();
                ViewBag.Error = "Medical aid name already exists!";

                return View(model);
            }
            else if (v3 > 0)
            {
                ViewBag.referalID = new SelectList(_member.getReferral(), "referral", "referral");
                model.referralFrom = _member.getReferralFrom().ToList();
                ViewBag.Error = "Medical aid account already exists!";

                return View(model);
            }
            #endregion

            if (model.MedicalAidClaimCodes.claimCode != null)
            {
                #region duplicate-check-claim-code: hcare-1298
                var ccvalidation = _member.GetMedicalAidClaimCode();
                var cv1 = 0;
                foreach (var item in ccvalidation)
                {
                    var medid = _medical.GetMedicalAidByCode(item.medicalAidCode);
                    if (medid != null) { if (model.medicalAid.MedicalAidID != medid.MedicalAidID && model.MedicalAidClaimCodes.claimCode == item.claimCode) { cv1++; break; } }
                }
                if (cv1 > 0)
                {
                    ViewBag.referalID = new SelectList(_member.getReferral(), "referral", "referral");
                    model.referralFrom = _member.getReferralFrom().ToList();
                    ViewBag.Error = "Claim code already exists!";
                    return View(model);
                }
                #endregion

                //if (!_medical.GetClaimCode(model.MedicalAidClaimCodes.claimCode).Count() && _member.GetClaimCodeByMedicalAidId(model.medicalAid.medicalAidCode) != model.MedicalAidClaimCodes.claimCode)
                //{
                //    ViewBag.referalID = new SelectList(_member.getReferral(), "referral", "referral");
                //    ViewBag.Error = "Claim code already exists!";
                //    return View(model);
                //}
            }
            if (Request.Query["referalID"].ToString() != null) { model.medicalAid.Referral = Request.Query["referalID"].ToString(); }//HCare-1175


            if (_medical.UpdateMedicalAid(model.medicalAid))
            {
                model.medicalAid.modifiedBy = User.Identity.Name;
                _medical.UpdateMedicalAidSettings(model.medicalAidSettings);

                var results = _member.GetClaimCodeByMedicalAidId(model.medicalAid.medicalAidCode);

                if (results.Count()==0)
                {
                    model.MedicalAidClaimCodes.createdBy = User.Identity.Name;
                    model.MedicalAidClaimCodes.medicalAidCode = model.medicalAid.medicalAidCode;
                    _medical.InsertMedicalAidClaimingCode(model.MedicalAidClaimCodes);
                }
                else
                {
                    model.MedicalAidClaimCodes.modifiedBy = User.Identity.Name;
                    model.MedicalAidClaimCodes.modifiedDate = DateTime.Now;
                    model.MedicalAidClaimCodes.medicalAidCode = model.medicalAid.medicalAidCode;
                    if (model.medicalAid.Active == true) { model.MedicalAidClaimCodes.Active = true; } else { model.MedicalAidClaimCodes.Active = false; }
                    if (model.MedicalAidClaimCodes.Active == false)
                    {
                        var claimcodelist = _member.GetMedicalAidClaimCode();
                        foreach (var cc in claimcodelist)
                        {
                            if (cc.claimCode == model.MedicalAidClaimCodes.claimCode)
                            {
                                model.MedicalAidClaimCodes.Active = false;
                                _medical.UpdateMedicalAidClaimCode(model.MedicalAidClaimCodes);
                            }
                        }

                    }
                    else
                    {
                        _medical.UpdateMedicalAidClaimingCode(model.MedicalAidClaimCodes);
                    }
                }


            }
            //Hcare-1087
            var medicalAid = model.medicalAid.MedicalAidID;
            int i = 0;//loop through the referals 
            foreach (var row in model.Programs)
            {

                if (Request.Query["Programs[" + i + "].referralFrom"].ToString() != null) { row.referralFrom = Request.Query["Programs[" + i + "].referralFrom"].ToString(); }//1144


                //HCare-1086
                if (row.medicalAidId != null)
                {
                    var item = _admin.GetMedicalAidProgram(medicalAid, row.program);

                    if (item != null)
                    {
                        if (item.Active != row.Active || item.referralFrom != row.referralFrom)
                        {
                            _admin.UpdateMedicalAidPrograms(new MedicalAidPrograms()
                            {
                                Active = row.Active,
                                program = row.program,
                                codeType = row.codeType,
                                medicalAidId = medicalAid.ToString(),
                                id = item.id,
                                referralFrom = row.referralFrom
                            });
                        }
                    }
                }
                else
                {
                    if (row.Active)
                    {
                        var response = _admin.InsertMedicalAidProgram(new MedicalAidPrograms()
                        {
                            id = 0,
                            Active = row.Active,
                            program = row.program,
                            codeType = row.codeType,
                            medicalAidId = medicalAid.ToString(),
                            createdBy = User.Identity.Name,
                            createdDate = DateTime.Now,
                        });
                    }
                }

                i++;
            }

            return RedirectToAction("IndexVM");
        }

        public ActionResult Details(Guid id)
        {
            MedicalAidViewModel medicalAid = _medical.GetMedicalAid(id);

            return View(medicalAid);
        }

        public ActionResult Delete(Guid id)
        {
            var medicalAid = _medical.GetMedicalAid(id);
            if (medicalAid == null)
            {
                return NotFound();
            }
            return View(medicalAid);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            var medical = _medical.GetMedicalAid(id);
            medical.medicalAid.Active = false;
            medical.medicalAid.Active = false;
            medical.medicalAid.modifiedBy = User.Identity.Name;
            medical.medicalAidSettings.modifiedBy = User.Identity.Name;
            _medical.UpdateMedicalAid(medical.medicalAid);
            _medical.UpdateMedicalAidSettings(medical.medicalAidSettings);
            return RedirectToAction("IndexVM");
        }

        public ActionResult MedicalAidCodeCheck(string code) //HCare-956
        {
            var options = _medical.GetMedicalAidByCode(code);

            return Json(options);
        }
        public ActionResult MedicalAidNameCheck(string name) //HCare-956
        {
            var options = _medical.GetMedicalAidByName(name);

            return Json(options);
        }
        public ActionResult CarrierCodeCheck(string name) //HCare-1216
        {

            var options = _medical.GetMedicalClaimCodeByCode(name);

            return Json(options);
        }


        //================================================================================== Scheme option ==================================================================================//
        public ActionResult SchemeOption_Create()
        {
            var model = new MedicalAidVM();
            model.MedicalAids = _member.GetMedicalAids();
            ViewBag.Code = "";
            ViewBag.Description = "";
            ViewBag.MedicalAid = "";

            return View(model);
        }
        [HttpPost]
        public ActionResult SchemeOption_Create(MedicalAidVM model)
        {
            var medicalAidPlan = new MedicalAidPlans();
            medicalAidPlan.Id = Guid.NewGuid();
            medicalAidPlan.planCode = model.PlanCode.Trim(); //hcare-1285
            medicalAidPlan.Name = model.PlanName.Trim(); //hcare-1285
            medicalAidPlan.medicalAidId = new Guid(Request.Query["MedicalAidName"].ToString());
            medicalAidPlan.createdBy = User.Identity.Name;
            medicalAidPlan.createdDate = DateTime.Now;
            medicalAidPlan.effectiveDate = DateTime.Now;
            medicalAidPlan.active = true;

            #region duplicate-check
            var validation = _member.GetMedicalAidPlanValidation(); //hcare-1298
            var medicalscheme = _member.GetMedicalAids().Where(x => x.MedicalAidID == medicalAidPlan.medicalAidId).Select(x => x.Name).FirstOrDefault();
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (medicalAidPlan.planCode.ToLower() == item.PlanCode.ToLower().Trim() && medicalAidPlan.Name.ToLower() == item.PlanName.ToLower().Trim() && medicalAidPlan.medicalAidId == item.MedicalAidId) { v1++; break; }
                if (medicalAidPlan.planCode.ToLower() == item.PlanCode.ToLower().Trim()) { v2++; }
                if (medicalAidPlan.Name.ToLower() == item.PlanName.ToLower().Trim() && medicalAidPlan.medicalAidId == item.MedicalAidId) { v3++; break; }
            }
            if (v1 > 0)
            {
                model.MedicalAids = _member.GetMedicalAids();
                ViewBag.Code = medicalAidPlan.planCode;
                ViewBag.Description = medicalAidPlan.Name;
                ViewBag.MedicalAid = medicalscheme;

                return View(model);
            }
            if (v2 > 0)
            {
                model.MedicalAids = _member.GetMedicalAids();
                ViewBag.Code = medicalAidPlan.planCode;

                return View(model);
            }
            if (v3 > 0)
            {
                model.MedicalAids = _member.GetMedicalAids();
                ViewBag.Description = medicalAidPlan.Name;
                ViewBag.MedicalAid = medicalscheme;

                return View(model);
            }
            #endregion

            _member.InsertSchemeOption(medicalAidPlan);

            return RedirectToAction("IndexVM");

        }

        public ActionResult SchemeOption_Edit(Guid id)
        {
            var model = new MedicalAidVM();
            var plan = _member.GetMedicalAidPlanDetail(id);

            model.PlanCode = plan.PlanCode;
            model.PlanName = plan.PlanName;
            model.MedicalAidName = plan.MedicalAidId.ToString();
            model.Active = plan.Active;

            model.MedicalAids = _member.GetMedicalAids();

            var medicalAid = _member.GetMedicalAidPlanDetail(id);
            ViewBag.medicalAid = new SelectList(_member.GetMedicalAidList(), "MedicalAidID", "Name", null, medicalAid.MedicalAidId.ToString());

            return View(model);

        }
        [HttpPost]
        public ActionResult SchemeOption_Edit(MedicalAidVM model)
        {
            model.ModifiedBy = User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            model.MedicalAidId = Guid.Parse(model.MedicalAidName);
            model.PlanCode = model.PlanCode.Trim(); //hcare-1285
            model.PlanName = model.PlanName.Trim(); //hcare-1285

            #region duplicate-check
            //hcare-1306 - amendment made to initial duplication check
            var validation = _member.GetMedicalAidPlanValidation(); //hcare-1298
            var medicalscheme = _member.GetMedicalAids().Where(x => x.MedicalAidID == model.MedicalAidId).Select(x => x.Name).FirstOrDefault();
            var v1 = 0;
            var v2 = 0;
            var v3 = 0;
            foreach (var item in validation)
            {
                if (model.Id != item.Id && model.PlanName.ToLower() == item.PlanName.ToLower().Trim() && model.MedicalAidId == item.MedicalAidId && model.PlanCode == item.PlanCode) { v1++; break; }
                if (model.Id != item.Id && model.PlanName.ToLower() == item.PlanName.ToLower().Trim() && model.MedicalAidId == item.MedicalAidId) { v2++; break; }
                if (model.Id != item.Id && model.PlanCode.ToLower() == item.PlanCode.ToLower().Trim()) { v3++; break; }
            }
            if (v1 > 0)
            {
                model.MedicalAids = _member.GetMedicalAids();
                ViewBag.Description = model.PlanName;
                ViewBag.Code = model.PlanCode;
                ViewBag.MedicalAid = medicalscheme;

                return View(model);
            }
            if (v2 > 0)
            {
                model.MedicalAids = _member.GetMedicalAids();
                ViewBag.Description = model.PlanName;
                ViewBag.MedicalAid = medicalscheme;

                return View(model);
            }
            if (v3 > 0)
            {
                model.MedicalAids = _member.GetMedicalAids();
                ViewBag.Code = model.PlanCode;

                return View(model);
            }
            #endregion

            var update = _member.UpdateSchemeOption(model);
            return RedirectToAction("IndexVM");

        }

        public ActionResult SchemeOption_Details(Guid id)
        {
            var viewModel = new SchemeOptionViewModel()
            {
                medicalAidPlan = _member.GetMedicalAidPlanById(id),
                medicalAid = _member.GetMedicalAidById(id),
                medicalAidVM = _member.GetMedicalAidPlanDetail(id)
            };

            return View(viewModel);

        }

        public ActionResult PlanCodeCheck(string code) //HCare-956
        {
            var options = _medical.GetMedicalAidPlanByCode(code);

            return Json(options);
        }
        public ActionResult PlanNameCheck(string name, string medicalaidID) //HCare-956
        {
            var options = _medical.GetMedicalAidPlanByName(name, medicalaidID);

            return Json(options);
        }

        //================================================================================= disposables =================================================================================//

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
