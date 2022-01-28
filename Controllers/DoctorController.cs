using HaloCareCore.DAL;
using HaloCareCore.Models;
using HaloCareCore.Models.Management;
using HaloCareCore.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HaloCareCore.Controllers
{
    public class DoctorController : Controller
    {
        private IMemberRepository _member;
        private IAdminRepository _admin;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DoctorController(OH17Context context, IConfiguration configuration)
        {
            _member = new MemberRepository(configuration, context);
            _admin = new AdminRepository(context, configuration);
        }

        public ActionResult Index()
        {
            var model = _admin.GetDoctors().Take(50);
            return View(model);
        }

        public ActionResult Create(Guid? DependentID)
        {
            ViewBag.Language = new SelectList(_member.GetLanguage(), "languageName", "languageName");
            ViewBag.DoctorType = new SelectList(_admin.GetDoctorTypes(), "doctorType", "typeDescription");
            ViewBag.Gender = new SelectList(_member.GetSex(), "sex", "sexName");
            return View();
        }
        [HttpPost]
        public ActionResult Create(DoctorViewModel model)
        {
            try
            {
                var practice = model.practices;
                var doctor = model.doctor;
                var address = model.address;
                var contact = model.contact;
                var practiceExists = _admin.GetPractice(practice.practiceNo);
                if (practiceExists == null)
                {
                    practice.createdBy = User.Identity.Name;
                    _admin.InsertPractice(practice);
                }
                var dr = _admin.GetDoctor(doctor.drLastName, practice.practiceNo);
                if (dr == null)
                {
                    doctor.sex = Request.Query["Gender"].ToString();
                    doctor.practiceNo = practice.practiceNo;
                    doctor.language = Request.Query["language"].ToString();
                    doctor.createdBy = User.Identity.Name;
                    Guid drId = _admin.InsertDoctor(doctor);
                    Guid addressID = Guid.NewGuid();
                    Guid contactId = Guid.NewGuid();
                    var addressc = _member.GetAddress(address);
                    if (addressc == null)
                    {
                        address.createdBy = User.Identity.Name;
                        address.createdDate = DateTime.Now;
                        address.Active = true;
                        addressID = _member.InsertAddress(address);
                    }
                    else
                    {
                        addressID = addressc.addressID;
                    }

                    var contactc = _member.GetContact(contact);
                    if (contactc == null)
                    {
                        contact.createdBy = User.Identity.Name;
                        contact.createdDate = DateTime.Now;
                        contact.Active = true;
                        contactId = _member.InsertContact(contact);
                    }
                    else
                    {
                        contactId = contactc.ContactID;
                    }

                    var info = new DoctorInformation();
                    info.doctorID = drId;
                    info.contactID = contactId;
                    info.addressID = addressID;
                    _admin.InsertDoctorInformation(info);
                }
                else
                {

                    ViewBag.Language = new SelectList(_member.GetLanguage(), "languageName", "languageName");
                    ViewBag.DoctorType = new SelectList(_admin.GetDoctorTypes(), "doctorType", "typeDescription");
                    ViewBag.Gender = new SelectList(_member.GetSex(), "sex", "sexName");
                    ModelState.AddModelError("GeneralError", "Doctor already loaded on the system");
                    return View(model);
                }
                ViewBag.Language = new SelectList(_member.GetLanguage(), "languageName", "languageName");
                ViewBag.DoctorType = new SelectList(_admin.GetDoctorTypes(), "doctorType", "typeDescription");
                ViewBag.Gender = new SelectList(_member.GetSex(), "sex", "sexName");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Language = new SelectList(_member.GetLanguage(), "languageName", "languageName");
                ViewBag.DoctorType = new SelectList(_admin.GetDoctorTypes(), "doctorType", "typeDescription");
                ViewBag.Gender = new SelectList(_member.GetSex(), "sex", "sexName");
                ModelState.AddModelError("GeneralError", ex.Message);
                return View(model);
            }
        }

        public ActionResult Edit(Guid id)
        {
            var model = _admin.GetDoctorsInformationEdit(id); //HCare-1181
            model.Languages = _member.GetLanguage();
            model.Gender = _member.GetSex();


            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(DoctorViewModel model)
        {

            model.doctorsInformation.ModifiedBy = User.Identity.Name;
            model.doctorsInformation.ModifiedDate = DateTime.Now;
            model.doctorsInformation.DoctorID = model.doctor.doctorID;
            model.doctorsInformation.Active = _admin.GetDoctorsInformationEdit(model.doctor.doctorID).doctor.active; //HCare-1453
            var doctorinformation_update = _admin.UpdateDoctorsInformation(model);

            return RedirectToAction("Details", new { id = model.doctorsInformation.DoctorID });

        }

        public ActionResult Details(Guid id)
        {
            var model = _admin.GetDoctorsInformationDetails(id); //HCare-1181
            return View(model);
        }

        public ActionResult Search()
        {
            var model = new List<Doctors>();

            if (Request.Query["SearchVar"].ToString() != null)
            {
                model = _admin.SearchDoctors(Request.Query["SearchVar"].ToString());
            }
            else
            {
                model = _admin.GetDoctors().Take(50).ToList();
            }
            return View("Index", model);
        }

        public ActionResult SearchDoctor()
        {
            var model = new DoctorViewModel();
            if (!String.IsNullOrEmpty(Request.Query["PracticeNo"]))
            {

                model = _admin.GetDoctorByPractice(Request.Query["PracticeNo"].ToString());
                if (model != null)
                {
                    if (Request.Query["depenID"].ToString() != null)
                    {
                        model.depID = Request.Query["depenID"].ToString();
                    }
                    model.practices = new Practices();
                    model.practices.practiceNo = model.doctor.practiceNo;
                    ViewBag.Language = new SelectList(_member.GetLanguage(), "languageName", "languageName", model.doctor.language);
                    ViewBag.DoctorType = new SelectList(_admin.GetDoctorTypes(), "doctorType", "typeDescription");
                    ViewBag.Gender = new SelectList(_member.GetSex(), "sex", "sexName", model.doctor.sex);
                    return View("Create", model);
                }
                else
                {
                    ModelState.AddModelError("", "No Doctor Found");
                    if (Request.Query["depID"].ToString() != null)
                    {
                        ViewBag.Language = new SelectList(_member.GetLanguage(), "languageName", "languageName");
                        ViewBag.DoctorType = new SelectList(_admin.GetDoctorTypes(), "doctorType", "typeDescription");
                        ViewBag.Gender = new SelectList(_member.GetSex(), "sex", "sexName");
                        return View("Create", new { DependentID = Request.Query["depID"] });
                    }
                    else if (!String.IsNullOrEmpty(Request.Query["depenID"]))
                    {
                        ViewBag.Language = new SelectList(_member.GetLanguage(), "languageName", "languageName");
                        ViewBag.DoctorType = new SelectList(_admin.GetDoctorTypes(), "doctorType", "typeDescription");
                        ViewBag.Gender = new SelectList(_member.GetSex(), "sex", "sexName");
                        return View("Create", new { DependentID = Request.Query["depenID"] });
                    }
                    else
                    {
                        ViewBag.Language = new SelectList(_member.GetLanguage(), "languageName", "languageName");
                        ViewBag.DoctorType = new SelectList(_admin.GetDoctorTypes(), "doctorType", "typeDescription");
                        ViewBag.Gender = new SelectList(_member.GetSex(), "sex", "sexName");
                        return View("Create", model);
                    }
                }

            }
            else
            {
                if (Request.Query["depID"].ToString() != null)
                {
                    ViewBag.Language = new SelectList(_member.GetLanguage(), "languageName", "languageName");
                    ViewBag.DoctorType = new SelectList(_admin.GetDoctorTypes(), "doctorType", "typeDescription");
                    ViewBag.Gender = new SelectList(_member.GetSex(), "sex", "sexName");
                    return View("Create", new { DependentID = Request.Query["depID"] });
                }
                else
                {
                    ViewBag.Language = new SelectList(_member.GetLanguage(), "languageName", "languageName");
                    ViewBag.DoctorType = new SelectList(_admin.GetDoctorTypes(), "doctorType", "typeDescription");
                    ViewBag.Gender = new SelectList(_member.GetSex(), "sex", "sexName");
                    return View("Create", model);
                }
            }


        }

    }
}
