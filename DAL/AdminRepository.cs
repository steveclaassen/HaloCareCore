using Dapper;
using HaloCareCore.Models;
using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Questionnaire;
using HaloCareCore.Models.Script;
using HaloCareCore.Models.Service;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using HaloCareCore.Repos;
using HaloCareCore.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace HaloCareCore.DAL
{
    public class AdminRepository : IAdminRepository
    {
        private OH17Context _context;
        private PasswordManager _pwManager = new PasswordManager();
        private IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;

        public AdminRepository(OH17Context context, IConfiguration _configuration)
        {
            this._context = context;
            Configuration = _configuration;
        }

        public bool IsYourLoginStillTrue(string userId, string sid)
        {
            List<Logins> logins = (from i in _context.Logins
                                   where i.LoggedIn == true &&
                                   i.UserId.ToString() == userId && i.SessionId == sid
                                   select i).ToList();
            return logins.Any();
        }

        public bool IsUserLoggedOnElsewhere(string userId, string sid)
        {
            IEnumerable<Logins> logins = (from i in _context.Logins
                                          where i.LoggedIn == true &&
                                          i.UserId.ToString() == userId && i.SessionId != sid
                                          select i).AsEnumerable();
            return logins.Any();
        }

        public void LogEveryoneElseOut(string userId, string sid)
        {
            IEnumerable<Logins> logins = (from i in _context.Logins
                                          where i.LoggedIn == true &&
                                          i.UserId.ToString() == userId &&
                                          i.SessionId != sid // need to filter by user ID
                                          select i).AsEnumerable();

            foreach (Logins item in logins)
            {
                item.LoggedIn = false;
            }

            _context.SaveChanges();
        }

        public bool ValidateUser(string username, string password)
        {
            Users user = _context.Users.Where(x => x.username == username).FirstOrDefault();
            if (user == null)
            {
                return false;
            }

            if (!_pwManager.IsPasswordMatch(password, user.salt, user.password))
            {
                return false;
            }

            return user != null;
        }

        public Users GetUserByUsername(string username)
        {
            var user = _context.Users.Where(x => x.username == username || x.email == username).FirstOrDefault();
            if (user == null)
                return null;
            return user;
        }

        public bool CheckSessionLogin(Logins model)
        {
            var loginrecord = _context.Logins.Where(x => x.UserId == model.UserId).FirstOrDefault();
            if (loginrecord == null)
            {
                _context.Logins.Add(model);
            }
            else
            {
                loginrecord.SessionId = model.SessionId;
            }

            _context.SaveChanges();

            return true;
        }

        public UserAccessViewModel GetUserRights(Guid userId)
        {
            var results = (from u in _context.Users
                           where u.Active == true
                           where u.userID == userId
                           select new UserAccessViewModel
                           {
                               userId = userId,
                               rights = _context.UserSchemeAccess.Where(x => x.userId == userId).Where(x => x.Active == true).Select(x => x.medicalAidId).ToList(),
                               accessList = _context.UserProgramAccess.Where(x => x.userId == userId).Where(x => x.Active == true).ToList(),
                           }).FirstOrDefault();

            if (results != null)
            {
                results.accessList = (from r in results.accessList
                                      where results.rights.Contains(r.medicalAidId)
                                      select r).ToList();
            }


            return results;
        }

        public List<CaseManagers> GetCaseManagers()
        {
            var caseManagers = _context.CaseManagers.OrderByDescending(x => x.createdDate).OrderByDescending(x => x.Active == true).ToList();
            return caseManagers;
        }

        public CaseManagers GetCaseManager(string managerNo)
        {
            var caseManager = _context.CaseManagers.Where(x => x.caseManagerNo == managerNo).FirstOrDefault();
            if (caseManager == null)
            {
                return null;
            }
            else
            {
                return caseManager;
            }
        }

        public CaseManagers GetCaseManagerByNumber(string number)
        {
            var result = _context.CaseManagers.Where(x => x.caseManagerNo == number).FirstOrDefault();
            return result;
        }

        public CaseManagers GetCaseManagerByName(string firstname, string lastname)
        {
            var result = _context.CaseManagers.Where(x => x.caseManagerName.ToLower() == firstname.ToLower()).Where(x => x.caseManagerSurname.ToLower() == lastname.ToLower()).FirstOrDefault();
            return result;
        }


        public string InsertCaseManager(CaseManagers caseManager)
        {
            _context.CaseManagers.Add(caseManager);
            Save();
            return caseManager.caseManagerNo;
        }

        public void InsertCaseManagerHistory(CaseManagerHistory model)
        {
            model.active = true;
            model.createdDate = DateTime.Now;
            model.effectiveDate = DateTime.Now;

            _context.CaseManagerHistory.Add(model);
            Save();
        }

        public List<Products> GetProducts()
        {
            return _context.Products.Where(x => x.Active == true).OrderBy(x => x.productName).ToList();
        }

        public List<Products> GetProductsSearch(string nappi, string prodname)
        {
            var result = new List<Products>();

            if (!string.IsNullOrEmpty(prodname))
            {
                result = _context.Products.Where(x => x.productName.ToLower().Contains(prodname.ToLower())).ToList();
            }
            else if (!string.IsNullOrEmpty(nappi))
            {
                if (!result.Any())
                {
                    result = _context.Products.Where(x => x.nappiCode.Contains(nappi)).ToList();
                }
                else
                {
                    result = result.Where(x => x.nappiCode.Contains(nappi)).ToList();
                }

            }
            else
            {
                result = _context.Products.ToList();
            }
            return result;
        }

        //HCare-1095
        public List<ComorbidConditionExclusions> GetComorbidsSearch(string icd10, string descrip)
        {
            var result = new List<ComorbidConditionExclusions>();

            if (icd10 == "" && descrip == "")
            {
                result = _context.ComorbidConditionExclusions.Take(100).ToList();
            }
            if (!string.IsNullOrEmpty(icd10))
            {
                result = _context.ComorbidConditionExclusions.Where(x => x.ICD10Code.ToLower().Contains(icd10.ToLower())).ToList();
                if (!string.IsNullOrEmpty(descrip))
                {
                    result = result.Where(x => x.mappingDescription.Contains(descrip)).ToList();
                }
                return result;
            }
            if (!string.IsNullOrEmpty(descrip) && string.IsNullOrEmpty(icd10))
            {
                result = _context.ComorbidConditionExclusions.Where(x => x.mappingDescription.Contains(descrip)).ToList();
            }
            return result;
        }
        public List<ComorbidConditionExclusions> GetICD10ConditionList() //HCare-1157
        {
            var results = _context.ComorbidConditionExclusions.ToList();
            //List<ComorbidConditionExclusions> distinctList = results.GroupBy(p => p.mappingCode).Select(g => g.First()).ToList();
            List<ComorbidConditionExclusions> distinctList = results.GroupBy(p => new { p.mappingCode, p.Active }).Select(g => g.First()).ToList();

            results = distinctList.OrderByDescending(x => x.Active).ThenBy(x => x.mappingCode).ToList();

            return results;
        }
        public List<ComorbidConditionExclusions> GetICD10Conditions() //HCare-859
        {
            return _context.ComorbidConditionExclusions.Where(x => x.Active == true).OrderBy(x => x.mappingCode).ToList();

        }

        public List<ComorbidConditionExclusions> GetICD10ConditionsByMappingCode(string mappingcode)
        {
            return _context.ComorbidConditionExclusions.Where(x => x.mappingCode == mappingcode).Where(x => x.Active == true).ToList(); //hcare-1298
        }
        public List<BenefitType> GetBenefitTypes()
        {
            return _context.BenefitType.Where(x => x.active == true).ToList();
        }

        public Products GetProduct(string nappiCode)
        {
            var product = _context.Products.Where(x => x.nappiCode == nappiCode).FirstOrDefault();
            return product;
        }
        //HCare-1095
        public ComorbidConditionExclusions GetComorbidExclusion(int id)
        {
            var result = _context.ComorbidConditionExclusions.Where(x => x.id == id).FirstOrDefault();
            return result;
        }
        public List<ComorbidConditionExclusions> GetComorbidInformation(string code) //HCare-1157
        {
            return _context.ComorbidConditionExclusions.Where(x => x.mappingCode == code).ToList();
        }
        public List<ComorbidConditionExclusions> GetComorbidInfoByName(string name, Guid dependantID) //HCare-859
        {
            return _context.ComorbidConditionExclusions.Where(x => x.mappingCode == name).ToList();

        }
        public ComorbidConditionExclusions GetComobidByICD10(string ICD10Code) //HCare-956
        {
            return _context.ComorbidConditionExclusions.Where(x => x.ICD10Code.ToLower().Replace(" ", "") == ICD10Code).Where(x => x.Active == true).FirstOrDefault();
        }

        public Products GetNappiCode(string productName)
        {
            var product = _context.Products.Where(x => x.productName == productName).FirstOrDefault();
            return product;
        }

        public Products GetProductByName(string productName)
        {
            var product = _context.Products.Where(x => x.productName.Contains(productName)).FirstOrDefault();
            return product;
        }

        public Products GetProductByNappi(string NappiCode)
        {
            var product = _context.Products.Where(x => x.nappiCode == NappiCode).FirstOrDefault();
            return product;
        }

        public bool InsertPoduct(Products model)
        {
            model.Active = true;
            model.createdDate = DateTime.Now;
            _context.Products.Add(model);
            Save();
            return true;
        }
        //HCare-1095
        public ServiceResult InsertComorbidExclusion(ComorbidConditionExclusions model)
        {
            model.Active = true;
            model.CreatedDate = DateTime.Now;
            _context.ComorbidConditionExclusions.Add(model);
            return _context.SaveChanges();
        }

        public List<SmsMessageTemplates> GetSmsMessageTemplates()
        {
            var results = _context.SmsMessageTemplates.OrderByDescending(x => x.title).OrderByDescending(x => x.Active == true).ToList();
            foreach (var result in results)
            {
                result.program = _context.Program.Where(x => x.code == result.program).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;

        }

        public List<SmsMessageTemplates> GetSMSTemplates()
        {
            return _context.SmsMessageTemplates.Where(x => x.Active == true).ToList(); //hcare-1298
        }

        public SmsMessageTemplates GetSmsMessageTemplate(int templateID)
        {
            return _context.SmsMessageTemplates.Where(x => x.templateID == templateID).FirstOrDefault();
        }

        public List<Language> GetsmsLanguage()
        {
            return _context.Language.OrderBy(x => x.languageName).ToList();
        }

        public bool InsertSmsMessageTemplate(SmsMessageTemplates model)
        {
            _context.SmsMessageTemplates.Add(model);
            Save();
            return true;
        }

        public EmailTemplates GetEmailTemplateByID(int templateID)
        {
            return _context.EmailTemplates.Where(x => x.templateID == templateID).FirstOrDefault();
        }

        public bool InsertEmailTemplate(EmailTemplates model)
        {

            _context.EmailTemplates.Add(model);
            var s = _context.SaveChanges();
            return true;
        }


        public bool GetSMSTemplate(SmsMessageTemplates model)
        {
            var result = _context.SmsMessageTemplates.Where(x => x.title.ToLower().Trim() == model.title.ToLower().Trim())
                //.Where(x => x.Languages == model.Languages) // the nulls are causing this to break
                .Where(x => x.program == model.program)
                .Where(x => x.template.ToLower().Trim() == model.template.ToLower().Trim())
                .Where(x => x.templateID != model.templateID).ToList().Count();

            if (result > 0) { return true; } else { return false; }

        }


        public bool GetEmailTemplate(EmailTemplates model)
        {
            var result = _context.EmailTemplates.Where(x => x.title == model.title)
                .Where(x => x.templateBody == model.templateBody)
                .Where(x => x.program == model.program)
                .Where(x => x.templateID != model.templateID).ToList().Count();

            if (result > 0) { return true; } else { return false; }

        }

        public List<EmailTemplates> GetEmailTemplates()
        {
            return _context.EmailTemplates.Where(x => x.Active == true).ToList(); //hcare-1298
        }
        public Practices GetPractice(string practiceNo)
        {
            return _context.Practices.Where(x => x.practiceNo == practiceNo).FirstOrDefault();
        }

        public List<Practices> GetPractices()
        {
            return _context.Practices.Where(x => x.active == true).ToList();
        }

        public bool InsertPractice(Practices practice)
        {
            practice.active = true;
            practice.createdDate = DateTime.Now;
            _context.Practices.Add(practice);
            Save();
            return true;
        }

        public Doctors GetDoctor(Guid? doctorID)
        {
            return _context.Doctors.Where(x => x.doctorID == doctorID).FirstOrDefault();
        }

        public DoctorViewModel GetDoctorByPractice(string practiceno)
        {
            var results = (from d in _context.Doctors
                           join di in _context.DoctorInformation
                           on d.doctorID equals di.doctorID into din
                           from di in din.DefaultIfEmpty()
                           join a in _context.Address
                           on di.addressID equals a.addressID into ad
                           from a in ad.DefaultIfEmpty()
                           join c in _context.Contacts
                           on di.contactID equals c.ContactID into co
                           from c in co.DefaultIfEmpty()
                           join cp in _context.Practices
                           on d.practiceNo equals cp.practiceNo into pr
                           from cp in pr.DefaultIfEmpty()
                           where d.active == true
                           where d.practiceNo.Contains(practiceno)
                           select new DoctorViewModel
                           {
                               doctor = d,
                               address = a,
                               contact = c,
                               practices = cp,

                           }).FirstOrDefault();

            return results;
        }

        public Doctors GetDoctor(string surname, string practiceNo)
        {
            return _context.Doctors.Where(x => x.drLastName == surname).Where(x => x.practiceNo == practiceNo).FirstOrDefault();
        }

        //public List<Doctors> GetDoctors()
        //{
        //	return _context.Doctors.Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList();
        //}

        public List<Doctors> GetDoctors() //HCare-1169 //HCare-1181
        {
            string sql = string.Format(@"SELECT top 100 * from Doctors Where active = 1");

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<Doctors>)db.Query<Doctors>(sql, null, commandTimeout: 1000).ToList();
                db.Close();

                return results.OrderByDescending(x => x.createdDate).ToList();
            }

        }

        public List<Doctors> SearchDoctors(string searchvar)
        {
            return _context.Doctors.Where(x => x.active == true).Where(x => x.practiceNo.ToUpper().Contains(searchvar.ToUpper()) || x.drFirstName.ToUpper().Contains(searchvar.ToUpper()) || x.drLastName.ToUpper().Contains(searchvar.ToUpper())).OrderByDescending(x => x.createdDate).ToList();
        }

        public Guid InsertDoctor(Doctors doctor)
        {
            doctor.doctorID = Guid.NewGuid();
            doctor.active = true;
            doctor.createdDate = DateTime.Now;
            _context.Doctors.Add(doctor);
            Save();
            return doctor.doctorID;
        }

        public ServiceResult UpdateDoctor(Doctors doctor)
        {
            var old = _context.Doctors.Where(x => x.doctorID == doctor.doctorID).FirstOrDefault();
            old.title = doctor.title;
            old.sex = doctor.sex;
            old.drFirstName = doctor.drFirstName;
            old.drLastName = doctor.drLastName;
            old.language = doctor.language;
            old.position = doctor.position;
            old.practiceNo = doctor.practiceNo;
            old.idNo = doctor.idNo;
            old.active = doctor.active;

            return _context.SaveChanges();
        }

        public DoctorInformation GetDoctorInfo(Guid id)
        {
            return _context.DoctorInformation.Where(x => x.doctorID == id).FirstOrDefault();
        }

        public DoctorViewModel GetDoctorInformation(Guid id)
        {
            var results = (from d in _context.Doctors
                           join di in _context.DoctorInformation
                           on d.doctorID equals di.doctorID into din
                           from di in din.DefaultIfEmpty()
                           join a in _context.Address
                           on di.addressID equals a.addressID into ad
                           from a in ad.DefaultIfEmpty()
                           join c in _context.Contacts
                           on di.contactID equals c.ContactID into co
                           from c in co.DefaultIfEmpty()
                           join cp in _context.Practices
                           on d.practiceNo equals cp.practiceNo into pr
                           from cp in pr.DefaultIfEmpty()
                           where d.doctorID == id
                           select new DoctorViewModel
                           {
                               doctor = d,
                               address = a,
                               contact = c,
                               practices = cp,
                               dependants = (from de in _context.Dependants
                                             join mi in _context.MemberInformation
                                             on de.DependantID equals mi.dependantID
                                             where mi.doctorID == id
                                             select de).ToList()
                           }).FirstOrDefault();
            return results;
        }
        public DoctorViewModel GetDoctorsInformationEdit(Guid id) //HCare-1181
        {
            var results = (from d in _context.Doctors
                           join di in _context.DoctorsInformation on d.doctorID equals di.DoctorID into din
                           from di in din.DefaultIfEmpty()
                           join cp in _context.Practices on d.practiceNo equals cp.practiceNo into pr
                           from cp in pr.DefaultIfEmpty()
                           where d.doctorID == id
                           select new DoctorViewModel
                           {
                               doctor = d,
                               doctorsInformation = di,
                               practices = cp,
                               dependants = (from de in _context.Dependants
                                             join dh in _context.PatientDoctorHistory on de.DependantID equals dh.dependantId into dhd
                                             from dh in dhd.DefaultIfEmpty()
                                             where dh.doctorId == id
                                             select de).ToList()
                           }).FirstOrDefault();

            return results;
        }

        public DoctorViewModel GetDoctorsInformationDetails(Guid id) //HCare-1181
        {
            string sql = string.Format(@"select dependantId[DependantID] 
                                        from PatientDoctorHistory 
                                        where effectiveDate in (select max(effectiveDate) from PatientDoctorHistory group by dependantId)
                                        and doctorId = '{0}'", id);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var members = (List<DoctorViewModel>)db.Query<DoctorViewModel>(sql, null, commandTimeout: 500);
                db.Close();

                string memberlist = string.Empty;
                foreach (var item in members)
                {
                    memberlist += "'" + item.DependantID + "',";
                }

                var results = (from d in _context.Doctors
                               join di in _context.DoctorsInformation on d.doctorID equals di.DoctorID into din
                               from di in din.DefaultIfEmpty()
                               join cp in _context.Practices on d.practiceNo equals cp.practiceNo into pr
                               from cp in pr.DefaultIfEmpty()
                               where d.doctorID == id

                               select new DoctorViewModel
                               {
                                   doctor = d,
                                   doctorsInformation = di,
                                   practices = cp,
                                   dependants = (from de in _context.Dependants
                                                 join dh in _context.PatientDoctorHistory on de.DependantID equals dh.dependantId into dhd
                                                 from dh in dhd.DefaultIfEmpty()
                                                 where dh.doctorId == id
                                                 where memberlist.Contains(dh.dependantId.ToString())
                                                 select de).ToList()

                               }).FirstOrDefault();

                results.doctor.language = _context.Language.Where(x => x.languageName == results.doctor.language).Select(x => x.languageCode).FirstOrDefault();
                results.dependants = results.dependants.GroupBy(p => p.DependantID).Select(g => g.First()).ToList(); //hcare-1181: update

                return results;

            }
        }

        public List<Doctors> SearchDoctors(DoctorSearchVars model) //HCare-1181
        {
            var results = new List<DoctorViewModel>();
            if (!String.IsNullOrEmpty(model.practiceNo))
            {
                results = (from d in _context.Doctors
                           where d.active == true
                           where d.practiceNo == model.practiceNo
                           select new DoctorViewModel
                           {
                               doctor = d,
                           }).OrderByDescending(x => x.doctor.createdDate).ToList();

            }
            else if (!String.IsNullOrEmpty(model.LastName))
            {
                results = (from d in _context.Doctors
                           where d.active == true
                           where d.drLastName.ToLower().Contains(model.LastName.ToLower())    //HCare-1290
                           select new DoctorViewModel
                           {
                               doctor = d,
                           }).OrderByDescending(x => x.doctor.createdDate).ToList();
            }

            return results.Select(x => x.doctor).OrderByDescending(x => x.createdDate).ToList();
        }

        //public List<Doctors> SearchDoctors(DoctorSearchVars model)
        //{
        //    var results = new List<DoctorViewModel>();
        //    if (!String.IsNullOrEmpty(model.practiceName))
        //    {
        //        results = (from d in _context.Doctors
        //                   join cp in _context.Practices
        //                   on d.practiceNo equals cp.practiceNo into pr
        //                   from cp in pr.DefaultIfEmpty()
        //                   where cp.practiceName != null
        //                   where d.active == true
        //                   select new DoctorViewModel
        //                   {
        //                       doctor = d,
        //                       practices = cp
        //                   }).OrderByDescending(x => x.doctor.createdDate).ToList();

        //        results = results.Where(x => x.practices.practiceName.ToLower().Contains(model.practiceName.ToLower())).ToList();
        //    }
        //    else
        //    {
        //        results = (from d in _context.Doctors
        //                   join cp in _context.Practices
        //                   on d.practiceNo equals cp.practiceNo into pr
        //                   from cp in pr.DefaultIfEmpty()
        //                   select new DoctorViewModel
        //                   {
        //                       doctor = d,
        //                       practices = cp
        //                   }).OrderByDescending(x => x.doctor.createdDate).ToList();
        //    }

        //    try
        //    {
        //        if (!String.IsNullOrEmpty(model.FirstName))
        //        {
        //            results = results.Where(x => x.doctor.drFirstName != null).ToList();
        //            results = results.Where(x => x.doctor.drFirstName.ToLower().Contains(model.FirstName.ToLower())).ToList();
        //        }
        //    }
        //    catch
        //    {
        //        results = results.Where(x => x.doctor.drFirstName != null).ToList();
        //        results = results.Where(x => x.doctor.drFirstName.ToUpper().Contains(model.FirstName.ToUpper())).ToList();
        //    }
        //    if (!String.IsNullOrEmpty(model.LastName))
        //    {
        //        results = results.Where(x => x.doctor.drLastName != null).ToList();
        //        results = results.Where(x => x.doctor.drLastName.ToLower().Contains(model.LastName.ToLower())).ToList();
        //    }

        //    if (!String.IsNullOrEmpty(model.practiceNo))
        //    {
        //        results = results.Where(x => x.doctor.practiceNo != null).ToList();
        //        results = results.Where(x => x.doctor.practiceNo.ToLower().Contains(model.practiceNo.ToLower())).ToList();

        //    }
        //    return results.Select(x => x.doctor).OrderByDescending(x => x.createdDate).ToList();
        //}

        public bool InsertDoctorInformation(DoctorInformation info)
        {
            info.DoctorInformationID = Guid.NewGuid();
            _context.DoctorInformation.Add(info);
            Save();
            return true;
        }

        public ServiceResult UpdateDrInfo(DoctorInformation info)
        {
            var old = _context.DoctorInformation.Where(x => x.DoctorInformationID == info.DoctorInformationID).FirstOrDefault();
            old.addressID = info.addressID;
            old.contactID = info.contactID;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateDrContact(DoctorViewModel model)
        {
            var old = _context.Contacts.Where(x => x.ContactID == model.contact.ContactID).FirstOrDefault();
            if (old != null)
            {
                //old.ContactID = model.address.addressID;
                old.contactName = model.contact.contactName;
                old.landLine = model.contact.landLine;
                old.cell = model.contact.cell;
                old.fax = model.contact.fax;
                old.workNo = model.contact.workNo;
                old.email = model.contact.email;
                old.modifiedBy = model.contact.modifiedBy;
                old.modifiedDate = DateTime.Now;

                return _context.SaveChanges();
            }
            else
            {
                model.contact.createdBy = model.contact.modifiedBy;
                model.contact.createdDate = DateTime.Now;
                model.contact.Active = true;

                _context.Contacts.Add(model.contact);
                Save();
            }

            return null;
        }

        public ServiceResult UpdatetheDoctor(DoctorViewModel model)
        {
            var old = _context.Doctors.Where(x => x.doctorID == model.doctor.doctorID).FirstOrDefault();
            old.doctorID = model.doctor.doctorID;
            old.title = model.doctor.title;
            old.drFirstName = model.doctor.drFirstName;
            old.drLastName = model.doctor.drLastName;
            old.sex = model.doctor.sex;
            old.language = model.doctor.language;
            old.practiceNo = model.doctor.practiceNo;
            old.idNo = model.doctor.idNo;
            old.modifiedBy = model.doctor.modifiedBy;
            old.modifiedDate = model.doctor.modifiedDate;
            old.active = model.doctor.active;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateDoctorsInformation(DoctorViewModel model)
        {

            //HCare-1453
            var old = _context.DoctorsInformation.Where(x => x.DoctorID == model.doctorsInformation.DoctorID).FirstOrDefault();
            if (old == null)
            {
                var info = new DoctorsInformation();
                info.DoctorID = model.doctorsInformation.DoctorID;
                info.ContactName = model.doctorsInformation.ContactName;
                info.MobileNumber = model.doctorsInformation.MobileNumber;
                info.HomeNumber = model.doctorsInformation.HomeNumber;
                info.WorkNumber = model.doctorsInformation.WorkNumber;
                info.Email = model.doctorsInformation.Email;
                info.Fax = model.doctorsInformation.Fax;
                info.CreatedBy = model.doctorsInformation.ModifiedBy;
                info.CreatedDate = DateTime.Now;

                var res = _context.DoctorsInformation.Add(info);
                return _context.SaveChanges();
            }
            else
            {
                old.DoctorID = model.doctorsInformation.DoctorID;
                old.ContactName = model.doctorsInformation.ContactName;
                old.MobileNumber = model.doctorsInformation.MobileNumber;
                old.HomeNumber = model.doctorsInformation.HomeNumber;
                old.WorkNumber = model.doctorsInformation.WorkNumber;
                old.Email = model.doctorsInformation.Email;
                old.Fax = model.doctorsInformation.Fax;
                old.ModifiedBy = model.doctorsInformation.ModifiedBy;
                old.ModifiedDate = model.doctorsInformation.ModifiedDate;

                return _context.SaveChanges();

            }

        }

        public ServiceResult UpdateDrAddress(DoctorViewModel model)
        {
            var old = _context.Address.Where(x => x.addressID == model.address.addressID).FirstOrDefault();
            if (old != null)
            {
                //old.addressID = model.address.addressID;
                old.line1 = model.address.line1;
                old.line2 = model.address.line2;
                old.line3 = model.address.line3;
                old.city = model.address.city;
                old.province = model.address.province;
                old.zip = model.address.zip;
                old.modifiedBy = model.address.modifiedBy;
                old.modifiedDate = DateTime.Now;

                return _context.SaveChanges();
            }
            else
            {
                model.address.createdBy = model.contact.modifiedBy;
                model.address.createdDate = DateTime.Now;
                model.address.Active = true;

                _context.Address.Add(model.address);
                Save();
            }

            return null;
        }

        public List<DoctorTypes> GetDoctorTypes()
        {
            return _context.DoctorTypes.OrderBy(x => x.typeDescription).Where(x => x.active == true).ToList();
        }

        public Users GetUserByFullName(string firstname, string lastname)
        {
            var user = _context.Users.Where(x => x.Firstname == firstname).Where(x => x.Lastname == lastname).FirstOrDefault();
            if (user == null)
                return null;
            return user;
        }
        public Users GetUserById(Guid id)
        {
            var user = _context.Users.Where(x => x.userID == id).FirstOrDefault();
            if (user == null)
                return null;
            return user;
        }

        public List<Users> GetUsersList()
        {
            var user = _context.Users.OrderBy(x => x.Firstname).ThenByDescending(x => x.Active == true).ToList();
            return user;
        }

        public List<Users> GetAllowedUsersList(Guid MedicalID, Guid ProgramID)
        {
            var users = _context.Users.Where(x => x.Active == true).ToList();

            var results = new List<Users>();

            foreach (var user in users)
            {
                var rights = GetUserRights(user.userID);
                foreach (var r in rights.accessList)
                {
                    if (MedicalID == r.medicalAidId && ProgramID == r.programId)
                    {
                        results.Add(user);
                    }
                }

            }

            return results;
        }

        #region HCare-1176
        public List<UserMemberHistory> GetUserMemberHistoryList(Guid dependantID, Guid program)
        {
            var pro = _context.Program.Where(x => x.programID == program).FirstOrDefault();
            var results = _context.UserMemberHistories.Where(x => x.DependantID == dependantID).Where(x => x.Program == pro.code).OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Active == true).ToList();
            foreach (var result in results)
            {
                result.Program = _context.Program.Where(x => x.code == result.Program).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }

        public ServiceResult InsertUserMemberHistory(UserMemberHistory model)
        {
            _context.UserMemberHistories.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult UpdateUserMemberHistory(UserMemberHistory model)
        {
            var old = _context.UserMemberHistories.Where(x => x.Id == model.Id).FirstOrDefault();
            if (old != null)
            {
                old.ModifiedDate = model.ModifiedDate;
                old.ModifiedBy = model.ModifiedBy;
                old.UserID = model.UserID;
                old.UserFullName = model.UserFullName;
                old.Program = model.Program;
                old.Comment = model.Comment;
                old.Active = model.Active;

                return _context.SaveChanges();
            }

            return null;
        }

        public List<UserMemberHistory> GetUserMemberHistory(Guid dependantID, Guid program)
        {
            var programCode = _context.Program.Where(x => x.programID == program).Select(x => x.code).FirstOrDefault();
            var results = (from um in _context.UserMemberHistories
                           where um.DependantID == dependantID
                           where um.Program == programCode
                           select um).OrderByDescending(x => x.EffectiveDate).ToList();

            return results;
        }

        public UserMemberHistory GetUserMemberByID(int id, Guid pro)
        {
            return _context.UserMemberHistories.Where(x => x.Id == id).FirstOrDefault();
        }
        #endregion

        public Users GetUserByName(string name)
        {
            var user = _context.Users.Where(x => x.username == name).FirstOrDefault();
            if (user == null)
                return null;
            return user;
        }

        public ServiceResult AddUserRole(UserRole userRole)
        {
            _context.UserRole.Add(userRole);
            return _context.SaveChanges();
        }

        //HCare-1250

        public bool IsUserInRole(string username, string roleName)
        {
                var results = (from user in _context.Users
                               join role in _context.UserRole on user.userID equals role.userId
                               join userrole in _context.Roles on role.roleId equals userrole.Id
                               where user.username == username && userrole.name == roleName
                               select userrole.name).Any();

                return results;
        }

        public List<string> GetUserInRole(string username)
        {
                var results = (from user in _context.Users
                               join role in _context.UserRole on user.userID equals role.userId
                               join userrole in _context.Roles on role.roleId equals userrole.Id
                               where user.username == username
                               select userrole.name);

                return results.ToList();
        }

        public bool IsUserHasRoleAccess(string username)
        {
                var res = (from user in _context.Users
                           join role in _context.UserRole on user.userID equals role.userId
                           join userrole in _context.Roles on role.roleId equals userrole.Id
                           where user.username == username
                           select userrole.name).Any();

                return res;
        }

        public ServiceResult AddNewUser(Users user, Guid roleId)
        {
            user.createdDate = DateTime.Now;
            user.Active = true;
            var role = GetRoleById(roleId);
            var result = new ServiceResult { Errors = new List<ServiceError>(), Success = true };

            //Get the salt and password for user
            var salt = String.Empty;
            var pwHash = _pwManager.GeneratePasswordHash(user.password, out salt);
            user.password = pwHash;
            user.salt = salt;

            //First add the user
            var addUserResult = AddUser(user);

            if (!addUserResult.Success)
            {
                return addUserResult;
            }

            var userRole = new UserRole { Id = Guid.NewGuid(), roleId = roleId, userId = user.userID };
            var addUserRoleResult = AddUserRole(userRole);

            if (!addUserRoleResult.Success)
            {
                return addUserRoleResult;
            }

            return _context.SaveChanges();
        }

        public ServiceResult AddNewRole(Roles role)
        {
            var result = new ServiceResult { Errors = new List<ServiceError>(), Success = true };
            _context.Roles.Add(role);

            return _context.SaveChanges();
        }

        public ServiceResult UpdateRole(Roles model)
        {
            var old = _context.Roles.Where(x => x.Id == model.Id).FirstOrDefault();
            if (old != null)
            {
                old.name = model.name;
                old.admin = model.admin;
                old.adminRights = model.adminRights;
                old.active = model.active;

                return _context.SaveChanges();
            }

            return null;
        }

        public UserRoleWorkflowRights GetWorkFlowById(int Id)
        {
            return _context.UserRoleWorkflowRights.Where(x => x.Id == Id).FirstOrDefault();
        }

        public ServiceResult UpdateWorkflow(UserRoleWorkflowRights model)
        {
            var old = _context.UserRoleWorkflowRights.Where(x => x.Id == model.Id).FirstOrDefault();
            if (old != null)
            {
                old.roleId = model.roleId;
                old.assignmentTypeId = model.assignmentTypeId;
                old.modifiedBy = model.modifiedBy;
                old.modifiedDate = DateTime.Now;
                old.active = model.active;

                return _context.SaveChanges();
            }

            return null;
        }

        public ServiceResult AddNewRoleLevel(Roles role)
        {
            var result = new ServiceResult { Errors = new List<ServiceError>(), Success = true };
            _context.Roles.Add(role);

            return _context.SaveChanges();
        }

        public ServiceResult AddNewRoleWorkFlow(UserRoleWorkflowRights model)
        {
            var result = new ServiceResult { Errors = new List<ServiceError>(), Success = true };
            _context.UserRoleWorkflowRights.Add(model);

            return _context.SaveChanges();
        }
        public ServiceResult AddUserSchemeAccess(UserSchemeAccess userAccess)
        {
            _context.UserSchemeAccess.Add(userAccess);
            return _context.SaveChanges();
        }

        public List<UserSchemeAccess> GetUserAccess(Guid userId)
        {
            var results = _context.UserSchemeAccess.Where(x => x.userId == userId).ToList();
            return results;
        }

        public List<UserSchemeAccess> GetUserAccess(Guid userId, Guid medicalAidID)
        {
            var results = _context.UserSchemeAccess.Where(x => x.userId == userId).Where(x => x.medicalAidId == medicalAidID).ToList();
            return results;
        }

        public UserProgramAccess GetUserProgramAccessByProgram(Guid userId, Guid medId, Guid programId)
        {
            var results = _context.UserProgramAccess.Where(x => x.userId == userId).Where(x => x.programId == programId).Where(x => x.medicalAidId == medId).FirstOrDefault();
            return results;
        }

        public ServiceResult AddUserProgramAccess(UserProgramAccess userAccess)
        {
            _context.UserProgramAccess.Add(userAccess);
            return _context.SaveChanges();
        }

        public Roles GetRoleById(Guid roleId)
        {
            var res = _context.Roles.Where(x => x.Id == roleId).FirstOrDefault();
            return res;
        }

        public Roles GetRoleByName(string role)
        {
            return _context.Roles.Where(x => x.name.ToLower().Replace(" ", "") == role).FirstOrDefault();
        }

        //HCare-1047
        public UserRole GetUserRoleById(Guid userId)
        {

            _context.Entry(_context.UserRole.Where(x => x.userId == userId).FirstOrDefault()).Reload();
            var res = _context.UserRole.Where(x => x.userId == userId).FirstOrDefault();
            return res;
        }

        public List<UserRoleRights> GetRoleRights(Guid roleId)
        {
            var res = _context.UserRoleRights.Where(x => x.roleId == roleId).Where(x => x.active == true).ToList();
            _context.Dispose();
            return res;
        }

        public ServiceResult AddUser(Users user)
        {
            var result = new ServiceResult { Errors = new List<ServiceError>(), Success = true };
            var User = GetUserByUsername(user.username);
            if (User == null)
            {
                user.userID = Guid.NewGuid();
                _context.Users.Add(user);
                return _context.SaveChanges();
            }
            else
            {
                result.Errors.Add(new ServiceError { Message = "The user already exists." });
                return result;
            }
        }

        public ServiceResult UpdateSmsTemplate(SmsMessageTemplates model)
        {
            var old = _context.SmsMessageTemplates.Where(x => x.templateID == model.templateID).FirstOrDefault();
            old.program = model.program;
            old.smsLanguage = model.smsLanguage;
            old.template = model.template;
            old.title = model.title;
            old.Active = model.Active;
            old.modifiedDate = DateTime.Now;
            old.modifiedBy = model.modifiedBy;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateEmailTemplate(EmailTemplates model)
        {
            var old = _context.EmailTemplates.Where(x => x.templateID == model.templateID).FirstOrDefault();
            old.templateSubject = model.templateSubject;
            old.templateHeading = model.templateHeading;
            old.title = model.title;
            old.templateBody = model.templateBody;
            old.program = model.program;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateAccountTextTemplate(AccountTextTemplates model)
        {
            var old = _context.AccountTextTemplates.Where(x => x.Id == model.Id).FirstOrDefault();
            old.Active = model.Active;
            old.modifiedDate = DateTime.Now;
            old.modifiedBy = model.modifiedBy;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateAccountEmailTemplate(AccountEmailTemplates model)
        {
            var old = _context.AccountEmailTemplates.Where(x => x.Id == model.Id).FirstOrDefault();
            old.Active = model.Active;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = DateTime.Now;

            return _context.SaveChanges();
        }

        public List<Roles> GetUserRoles()
        {
            string sql = string.Format(@"SELECT * FROM Roles WHERE Active = 1 ORDER BY name");

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<Roles>)db.Query<Roles>(sql, null, commandTimeout: 500).Distinct().ToList();
                db.Close();

                return results;
            }

        }

        public List<Users> GetUsers()
        {
            return _context.Users.OrderByDescending(x => x.createdDate).OrderByDescending(x => x.Active == true).ToList();
        }

        public string GetRoleUserId(Guid userId)
        {
            var roleId = _context.UserRole.Where(x => x.userId == userId).Select(x => x.roleId).FirstOrDefault();
            return _context.Roles.Where(x => x.Id == roleId).Select(x => x.name).FirstOrDefault();
        }

        public ServiceResult UpdateUser(Users model, string RoleId = null)
        {
            var old = _context.Users.Where(x => x.userID == model.userID).FirstOrDefault();

            if (old.password != model.password)
            {
                old.password = model.password;
                var salt = string.Empty;
                var pwHash = _pwManager.GeneratePasswordHash(old.password, out salt);
                old.password = pwHash;
                old.salt = salt;
            }
            old.Active = model.Active;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.email = model.email;

            var result = Guid.NewGuid();
            _context.SaveChanges();
            if (RoleId != null)
            {
                var role = GetUserRoleById(model.userID);
                role.roleId = new Guid(RoleId);

            }
            return _context.SaveChanges();
        }

        public ServiceResult UpdateUser(Guid userId, string pasw, string email, bool active, bool searchmanagement, string RoleId = null)
        {
            var userData = _context.Users.Where(x => x.userID == userId).FirstOrDefault();

            if (userData.password != pasw)
            {
                userData.password = pasw;
                var salt = string.Empty;
                var pwHash = _pwManager.GeneratePasswordHash(userData.password, out salt);
                userData.password = pwHash;
                userData.salt = salt;
            }
            userData.Active = active;
            userData.modifiedBy = _session.GetString("fullName");
            userData.modifiedDate = DateTime.Now;
            userData.email = email;
            userData.SearchManagement = searchmanagement; //hcare-1289

            var result = Guid.NewGuid();
            _context.SaveChanges();
            if (RoleId != null)
            {
                var role = GetUserRoleById(userId);
                role.roleId = new Guid(RoleId);
            }
            return _context.SaveChanges();
        }

        //HCare-1095
        public ServiceResult UpdateComorbidExclusion(ComorbidConditionExclusions model)
        {
            var old = _context.ComorbidConditionExclusions.Where(x => x.id == model.id).FirstOrDefault();
            old.mappingCode = model.mappingCode;
            old.Active = model.Active;
            old.mappingDescription = model.mappingDescription;
            old.formularyCode = model.formularyCode;
            old.ICD10Code = model.ICD10Code;
            old.ModifiedDate = model.ModifiedDate;
            old.ModifiedBy = model.ModifiedBy;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateProduct(Products model)
        {
            var old = _context.Products.Where(x => x.nappiCode == model.oldNappicode).FirstOrDefault();
            old.packSize = model.packSize;
            old.Active = model.Active;
            old.productName = model.productName;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.atcCode = model.atcCode;
            old.therapeuticClass6Desc = model.therapeuticClass6Desc;
            old.therapeuticClass6 = model.therapeuticClass6;
            old.mmapIndicator = model.mmapIndicator;
            old.mrpIndicator = model.mrpIndicator;
            old.packUOM = model.packUOM;
            old.genericIndicator = model.genericIndicator;
            old.productSchedule = model.productSchedule;
            //old.strengthUOM = model.strengthUOM;
            old.productStatus = model.productStatus;


            var res = _context.SaveChanges();

            return _context.SaveChanges();
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public List<CaseManagerHistoryView> GetCaseManagerHistories(Guid depId)
        {
            var results = (from cmh in _context.CaseManagerHistory
                           join cm in _context.CaseManagers
                           on cmh.caseManagerId equals cm.caseManagerNo into din
                           from cm in din.DefaultIfEmpty()
                           select new CaseManagerHistoryView
                           {
                               caseManagerNo = cmh.caseManagerId,
                               caseManagerName = cm.caseManagerName,
                               effectiveDate = cmh.effectiveDate,
                               createdBy = cmh.createdBy
                           }).OrderByDescending(x => x.effectiveDate).ToList();
            return results;
        }

        public ServiceResult DeleteDoctor(Doctors doctors)
        {
            var oldDoctor = _context.Doctors.Where(x => x.doctorID == doctors.doctorID).FirstOrDefault();
            oldDoctor = doctors;

            return _context.SaveChanges();
        }

        public ServiceResult EditDoctor(Doctors doctors)
        {
            var oldDoctor = _context.Doctors.Where(x => x.doctorID == doctors.doctorID).FirstOrDefault();
            oldDoctor = doctors;

            return _context.SaveChanges();
        }

        public List<Scripts> GetUnAthourisedScripts()
        {
            return _context.Scripts.Where(x => x.Status == "TBA").Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).ToList();
        }
        public List<Scripts> GetRecentScripts()
        {
            var month = DateTime.Now.AddMonths(-1);
            return _context.Scripts.Where(x => x.createdDate > month).Where(x => x.active == true).OrderByDescending(x => x.effectiveDate).ToList();
        }

        public List<Scripts> GetOutstandingScripts()
        {
            var model = from p in _context.Scripts
                        group p by p.dependentID into grp
                        let MaxOrderDatePerPerson = grp.Max(g => g.effectiveDate)
                        from p in grp
                        where p.effectiveDate == MaxOrderDatePerPerson
                        select p;

            var month = DateTime.Now;
            return model.Where(x => x.effectiveDate.AddMonths(x.repeats) < month).OrderByDescending(x => x.effectiveDate).ToList();
        }

        public List<OutstandingsView> GetOutstandingScriptsView()
        {
            var month = DateTime.Now;

            var results = (from s in _context.Scripts
                           join d in _context.Dependants
                           on s.dependentID equals d.DependantID
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           join med in _context.MedicalAids
                           on m.medicalAidID equals med.MedicalAidID
                           join dr in _context.Doctors
                           on s.practiceNo equals dr.practiceNo into dri
                           from dr in dri.DefaultIfEmpty()
                           join drin in _context.DoctorInformation
                           on dr.doctorID equals drin.doctorID into drinfo
                           from drin in drinfo.DefaultIfEmpty()
                           join c in _context.Contacts
                           on drin.contactID equals c.ContactID into ci
                           from c in ci.DefaultIfEmpty()
                           group new { s, d, m, med, dr, drin, c } by s.dependentID into grp
                           select grp.OrderByDescending(g => g.s.effectiveDate).FirstOrDefault()).Where(x => x.s.effectiveDate.AddMonths(x.s.repeats) < month)
                           .Select(p => new OutstandingsView
                           {
                               membershipNo = p.m.membershipNo,
                               dependantCode = p.d.dependentCode,
                               firstName = p.d.firstName,
                               lastName = p.d.lastName,
                               idNumber = p.d.idNumber,
                               schemeName = p.med.Name,
                               schemeOption = _context.PatientPlanHistory.Where(x => x.dependantId == p.d.DependantID).OrderByDescending(x => x.effectiveDate).Where(x => x.active == true).Select(x => x.planName).FirstOrDefault(),
                               registeredProgram = _context.PatientProgramHistory.Where(x => x.dependantId == p.d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.programCode).FirstOrDefault(),
                               status = _context.PatientManagementStatusHistory.Where(x => x.dependantId == p.d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.managementStatusCode).FirstOrDefault(),
                               drEmail = (p.c != null ? p.c.email : ""),
                               expiredDate = p.s.effectiveDate.AddMonths( p.s.repeats),
                               TreatingDrName = p.dr.drFirstName + " " + p.dr.drLastName,
                               TreatingDrBHF = p.dr.practiceNo,
                               dependantID = p.d.DependantID,
                               overdue = (p.s.effectiveDate.AddMonths(p.s.repeats) - DateTime.Now).Days + "Days",
                               monthsOutstanding = ((p.s.effectiveDate.AddMonths( p.s.repeats) - DateTime.Now).Days/(365.25 / 12)) + " Months"
                           }).OrderBy(x => x.expiredDate).ToList();

            return results;
        }

        public List<OutstandingsView> GetOutstandingPathologiesView()
        {
            var month = DateTime.Now;

            var results = (from s in _context.Pathology
                           join d in _context.Dependants
                           on s.dependentID equals d.DependantID
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           join med in _context.MedicalAids
                           on m.medicalAidID equals med.MedicalAidID
                           group new { s, d, m, med } by s.dependentID into grp
                           select grp.OrderByDescending(g => g.s.effectiveDate).FirstOrDefault()).Where(x => x.s.effectiveDate.AddMonths(6) < month)
                           .Select(p => new OutstandingsView
                           {
                               membershipNo = p.m.membershipNo,
                               dependantCode = p.d.dependentCode,
                               firstName = p.d.firstName,
                               lastName = p.d.lastName,
                               idNumber = p.d.idNumber,
                               schemeName = p.med.Name,
                               schemeOption = _context.PatientPlanHistory.Where(x => x.dependantId == p.d.DependantID).OrderByDescending(x => x.effectiveDate).Where(x => x.active == true).Select(x => x.planName).FirstOrDefault(),
                               registeredProgram = _context.PatientProgramHistory.Where(x => x.dependantId == p.d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.programCode).FirstOrDefault(),
                               status = _context.PatientManagementStatusHistory.Where(x => x.dependantId == p.d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.managementStatusCode).FirstOrDefault(),
                               drEmail = "",
                               expiredDate = p.s.effectiveDate.AddMonths(6),
                               TreatingDrName = "",
                               TreatingDrBHF = "",
                               overdue = (p.s.effectiveDate.AddMonths(6) - DateTime.Now).Days + "Days",
                               monthsOutstanding = ((p.s.effectiveDate.AddMonths(6) - DateTime.Now).Days / (365.25 / 12)) + " Months",
                               dependantID = p.d.DependantID,
                           }).OrderBy(x => x.expiredDate).ToList();
            return results;
        }

        public List<Pathology> GetOutstandingPathology()
        {
            var model = from p in _context.Pathology
                        group p by p.dependentID into grp
                        let MaxOrderDatePerPerson = grp.Max(g => g.effectiveDate)
                        from p in grp
                        where p.effectiveDate == MaxOrderDatePerPerson
                        select p;
            var month = DateTime.Now;
            return model.Where(x => x.effectiveDate.AddMonths(11) < month).OrderByDescending(x => x.effectiveDate).ToList();
        }

        public List<UnAuthsScripts> GetOutstandingAuthorisedScripts()
        {
            var month = DateTime.Now;
            var greatDate = DateTime.Now.AddMonths(-6);

            var results = (from s in _context.Scripts
                           join d in _context.Dependants
                           on s.dependentID equals d.DependantID
                           join m in _context.Members
                           on d.memberID equals m.memberID
                           join med in _context.MedicalAids
                           on m.medicalAidID equals med.MedicalAidID
                           where s.Status == "TBA"
                           where s.active == true
                           where s.effectiveDate > greatDate
                           group new { s, d, m, med } by s.dependentID into grp
                           select grp.OrderByDescending(g => g.s.effectiveDate).FirstOrDefault())
                          .Select(p => new UnAuthsScripts
                          {
                              membershipNo = p.m.membershipNo,
                              dependantCode = p.d.dependentCode,
                              schemeName = p.med.Name,
                              effectiveDate = p.s.effectiveDate,
                              dependantID = p.d.DependantID,
                              scriptId = p.s.scriptID,
                              createdDate = p.s.createdDate,
                              overdue = ( p.s.effectiveDate - DateTime.Now).Days + " Days",
                              memStatus = _context.PatientManagementStatusHistory.Where(x => x.dependantId == p.d.DependantID).OrderByDescending(x => x.effectiveDate).Select(x => x.managementStatusCode).FirstOrDefault(),
                          }).OrderBy(x => x.effectiveDate).ToList();

            results = results.Where(x => x.memStatus != "DEC" && x.memStatus != "DEA" && x.memStatus != "SUS" && x.memStatus != "RES" && x.memStatus != "DME").ToList();





            return results;
        }

        //==>>
        public List<AssignmentItemTaskTypes> GetAssignmentItemTaskTypes()
        {
            var taskTypes = _context.AssignmentItemTaskTypes.OrderByDescending(x => x.taskDescription).OrderByDescending(x => x.active == true).ToList();

            return taskTypes;
        }

        public ServiceResult AddNewTaskType(AssignmentItemTaskTypes model)
        {
            _context.AssignmentItemTaskTypes.Add(model);

            return _context.SaveChanges();

        }

        public AssignmentItemTaskTypes GetTaskTypeById(string taskId)
        {
            return _context.AssignmentItemTaskTypes.Where(x => x.taskId.Contains(taskId)).FirstOrDefault();
        }

        public ServiceResult UpdateTaskType(AssignmentItemTaskTypes model)
        {
            var old = _context.AssignmentItemTaskTypes.Where(x => x.taskId == model.taskId).FirstOrDefault();

            old.taskDescription = model.taskDescription;
            old.active = model.active;

            return _context.SaveChanges();
        }

        //==>>
        public List<TaskTypeRequirements> GetTaskTypeRequirements()
        {
            var taskReq = _context.TaskTypeRequirements.OrderByDescending(x => x.requirementText).OrderByDescending(x => x.active == true).ToList();

            return taskReq;
        }
        public List<TaskTypeRequirements> GetTaskTypeRequirementValidation()
        {
            return _context.TaskTypeRequirements.Where(x => x.active == true).ToList();
        }

        public ServiceResult AddNewTaskReq(TaskTypeRequirements model)
        {
            _context.TaskTypeRequirements.Add(model);

            return _context.SaveChanges();
        }

        public TaskTypeRequirements GetTaskReqById(int id)
        {

            var result = _context.TaskTypeRequirements.Where(x => x.id == id).FirstOrDefault();
            if (result == null)
                return null;
            return result;

        }

        public ServiceResult UpdateTaskReq(TaskTypeRequirements model)
        {
            var old = _context.TaskTypeRequirements.Where(x => x.id == model.id).FirstOrDefault();

            old.templateID = model.templateID;
            old.requirementText = model.requirementText;
            old.active = model.active;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;

            return _context.SaveChanges();

        }

        //==>>
        public List<AssignmentTypes> GetAssignmentTypes()
        {
            return _context.AssignmentTypes.OrderByDescending(x => x.assignmentDescription).OrderByDescending(x => x.active == true).ToList();
        }
        public List<AssignmentTypes> GetAssignmentTypeValidation()
        {
            return _context.AssignmentTypes.Where(x => x.active == true).ToList();
        }
        public List<WorkflowViewModel> GetAssignmentItemTypes()
        {
            var results = _context.AssignmentItemTypes.OrderByDescending(x => x.assignmentItemType).OrderByDescending(x => x.active == true).ToList();
            var workflows = new List<WorkflowViewModel>();
            foreach (var res in results)
            {
                var work = new WorkflowViewModel
                {
                    assignmentTypeId = res.assignmentItemType,
                    description = res.itemDescription,
                    selected = false,
                };

                workflows.Add(work);
            }

            return workflows;
        }

        public List<WorkflowViewModel> GetAssignmentItemTypes(Guid Id)
        {
            var workflows = (from s in _context.UserRoleWorkflowRights
                             join ma in _context.AssignmentItemTypes on s.assignmentTypeId equals ma.assignmentItemType
                             where s.roleId == Id
                             where ma.active == true
                             where s.active == true
                             group new { s, ma } by ma.itemDescription into grp
                             select grp.FirstOrDefault())
                          .Select(p => new WorkflowViewModel
                          {
                              Id = p.s.Id,
                              roleId = Id,
                              assignmentTypeId = p.s.assignmentTypeId,
                              description = p.ma.itemDescription,
                              selected = p.s.active,
                              active = p.ma.active, //hcare-1010 
                          }).ToList();


            string[] ids = new string[workflows.Count()];

            var results = new List<WorkflowViewModel>();

            for (int id = 0; id < workflows.Count(); id++)
            {
                ids[id] = workflows[id].assignmentTypeId;
            }

            foreach (var workflow in workflows)
            {
                results.Add(workflow);
            }

            var assignmentTypes = _context.AssignmentItemTypes.OrderByDescending(x => x.assignmentItemType).Where(x => x.active == true).ToList(); //HCare-1010

            foreach (var types in assignmentTypes)
            {
                if (!ids.Contains(types.assignmentItemType))
                {
                    var work = new WorkflowViewModel
                    {
                        roleId = Id,
                        assignmentTypeId = types.assignmentItemType,
                        description = types.itemDescription,
                        selected = false,
                    };

                    results.Add(work);
                }
            }

            return results;
        }

        public ServiceResult AddNewAssignmentType(AssignmentTypes model)
        {
            var result = _context.AssignmentTypes.Add(model);

            return _context.SaveChanges();

        }

        public List<HospitalizationICD10types> GetHospICD10sByProgram(string programcode)
        {
            return _context.HospitalizationICD10types.Where(x => x.programCode == programcode).ToList();
        }

        public ServiceResult AddNewHospICD10(HospitalizationICD10types model)
        {
            model.createdDate = DateTime.Now;
            var result = _context.HospitalizationICD10types.Add(model);

            return _context.SaveChanges();

        }

        public AssignmentTypes GetAssignmentTypeById(string assignmentType)
        {
            return _context.AssignmentTypes.Where(x => x.assignmentType.Contains(assignmentType)).FirstOrDefault();
        }

        public HospitalizationICD10types GetHospICD10ById(int id)
        {
            return _context.HospitalizationICD10types.Where(x => x.id == id).FirstOrDefault();
        }

        public ServiceResult UpdateAssignmentType(AssignmentTypes model)
        {
            var old = _context.AssignmentTypes.Where(x => x.assignmentType == model.assignmentType).FirstOrDefault();
            old.assignmentType = model.assignmentType;
            old.assignmentDescription = model.assignmentDescription;
            old.active = model.active;
            return _context.SaveChanges();
        }

        public ServiceResult UpdateHospICD10(HospitalizationICD10types model)
        {
            var old = _context.HospitalizationICD10types.Where(x => x.id == model.id).FirstOrDefault();
            old.icdcode = model.icdcode;
            old.programCode = model.programCode;
            old.assignmentItemType = model.assignmentItemType;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = DateTime.Now;
            old.Active = model.Active;
            return _context.SaveChanges();
        }

        //==>>
        public List<AssignmentItemTypes> GetAssignmentItems()
        {
            var results = _context.AssignmentItemTypes.OrderByDescending(x => x.itemDescription).OrderByDescending(x => x.active == true).ToList();
            foreach (var result in results)
            {
                result.program = _context.Program.Where(x => x.code == result.program).Select(x => x.ProgramName).FirstOrDefault();
                result.sourceType = _context.AssignmentTypes.Where(x => x.assignmentType == result.sourceType).Select(x => x.assignmentDescription).FirstOrDefault();
            }

            return results;
        }
        public List<AssignmentItemTypes> GetAssignmentItemValidation()
        {
            return _context.AssignmentItemTypes.OrderByDescending(x => x.itemDescription).OrderByDescending(x => x.active == true).ToList();
        }

        public List<AssignmentItemTypes> GetAssignmentItemsValidation()
        {
            return _context.AssignmentItemTypes.OrderByDescending(x => x.itemDescription).OrderByDescending(x => x.active == true).ToList();
        }

        public ServiceResult AddNewAssignmentItem(AssignmentItemTypes model)
        {
            model.active = true;

            var result = _context.AssignmentItemTypes.Add(model);

            return _context.SaveChanges();
        }

        public AssignmentItemTypes GetAssignmentItemById(string assignmentItemType)
        {
            return _context.AssignmentItemTypes.Where(x => x.assignmentItemType.Contains(assignmentItemType)).FirstOrDefault();
        }

        public ServiceResult UpdateAssignmentItem(AssignmentItemTypes model)
        {
            var old = _context.AssignmentItemTypes.Where(x => x.assignmentItemType == model.assignmentItemType).FirstOrDefault();

            old.assignmentItemType = model.assignmentItemType;
            old.itemDescription = model.itemDescription;
            old.active = model.active;
            old.program = model.program;
            old.sourceType = model.sourceType;
            old.followUp = model.followUp;
            old.capture = model.capture;
            old.attach = model.attach;
            old.requirements = model.requirements;
            old.diabetes = model.diabetes;
            old.hiv = model.hiv;
            old.hypertension = model.hypertension;
            old.oncology = model.oncology;
            old.mental = model.mental;

            return _context.SaveChanges();
        }

        //==>>
        public List<SmsMessageTemplates> GetSMSmessageTemplate()
        {
            var smsMessage = _context.SmsMessageTemplates.Where(x => x.Active == true).OrderBy(x => x.templateID).ToList();

            return smsMessage;
        }

        public List<EmailTemplates> GetemailTemplate()
        {
            var results = _context.EmailTemplates.OrderByDescending(x => x.title).OrderByDescending(x => x.Active == true).ToList();
            foreach (var result in results)
            {
                result.program = _context.Program.Where(x => x.code == result.program).Select(x => x.ProgramName).FirstOrDefault();
            }
            return results;
        }

        //==>>
        public ServiceResult InsertForgotPassword(ForgottenPassword model)
        {
            var result = _context.ForgottenPasswords.Add(model);

            return _context.SaveChanges();

        }

        public ServiceResult UpdateForgotPassword(ForgottenPassword model)
        {
            var old = _context.ForgottenPasswords.Where(x => x.ForgottenPasswordID == model.ForgottenPasswordID).FirstOrDefault();

            old.active = model.active;


            return _context.SaveChanges();

        }

        public ForgottenPassword GetForgotPasswordById(string key)
        {
            var result = _context.ForgottenPasswords.Where(x => x.key == key).FirstOrDefault();

            return result;
        }

        public ForgottenPassword GetfPassUserId(Guid id)
        {
            var result = _context.ForgottenPasswords.Where(x => x.userID == id).FirstOrDefault();

            return result;
        }

        public Users GetfPassUser(Guid id)
        {
            var result = _context.Users.Where(x => x.userID == id).FirstOrDefault();

            return result;
        }


        //==>>
        //TaskRequirementItemLinking
        //==>>
        public List<TaskRequirementItemLinking> GetLinkedTasks()
        {
            var results = _context.TaskRequirementItemLinking.OrderByDescending(x => x.active == true).ThenBy(x => x.itemId).ThenBy(x => x.taskSequence).ToList();

            foreach (var result in results)
            {
                result.itemId = _context.AssignmentItemTypes.Where(x => x.assignmentItemType == result.itemId).Select(x => x.itemDescription).FirstOrDefault();
                result.taskType = _context.AssignmentItemTaskTypes.Where(x => x.taskId == result.taskType).Select(x => x.taskDescription).FirstOrDefault();

            }


            return results;
        }

        public List<TaskRequirementItemLinking> GetLinkedTasksByID(string itemID)
        {
            return _context.TaskRequirementItemLinking.Where(x => x.itemId == itemID).ToList();

        }

        public List<HospitalizationICD10types> GetHospICD10s() //Hcare-831
        {
            var results = _context.HospitalizationICD10types.OrderByDescending(x => x.Active == true).ThenBy(x => x.id).ToList();

            foreach (var result in results)
            {
                result.assignmentItemType = _context.AssignmentItemTypes.Where(x => x.assignmentItemType == result.assignmentItemType).Select(x => x.itemDescription).FirstOrDefault();
            }

            return results;
        }


        public ServiceResult AddLinkedTasks(TaskRequirementItemLinking model)
        {
            model.active = true;
            var result = _context.TaskRequirementItemLinking.Add(model);

            return _context.SaveChanges();
        }

        public TaskRequirementItemLinking GetLinkedTaskById(int id)
        {
            var result = _context.TaskRequirementItemLinking.SingleOrDefault(x => x.id == id);

            return result;
        }

        public ServiceResult UpdateLinkedTasks(TaskRequirementItemLinking model)
        {

            var old = _context.TaskRequirementItemLinking.Where(x => x.id == model.id).FirstOrDefault();

            old.itemId = model.itemId;
            old.taskType = model.taskType;
            old.requirementId = model.requirementId;
            old.templateID = model.templateID;
            old.active = model.active;
            old.taskSequence = model.taskSequence;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;

            return _context.SaveChanges();
        }

        //==>>
        //Educational Notes
        //==>>
        public List<EducationalNotes> GetEducationalNote()
        {
            return _context.EducationalNotes.Where(x => x.Active == true).ToList(); //hcare-1298
        }

        public ServiceResult AddNewEducationalNote(EducationalNotes model)
        {
            var result = _context.EducationalNotes.Add(model);

            return _context.SaveChanges();
        }

        public EducationalNotes GetEducationalNoteById(int id)
        {
            var result = _context.EducationalNotes.SingleOrDefault(x => x.Id == id);

            return result;
        }

        public ServiceResult UpdateEducationalNote(EducationalNotes model)
        {
            var old = _context.EducationalNotes.Where(x => x.Id == model.Id).FirstOrDefault();
            old.title = model.title;
            old.note = model.note;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        //==>>

        public List<HypoRiskRules> GetHypoRiskRules()
        {
            return _context.HypoRiskRules.OrderByDescending(x => x.active == true).ThenBy(x => x.effectiveDate).ToList();
        }

        public bool GetHypoRiskRule(HypoRiskRules model)
        {
            var result = _context.HypoRiskRules.Where(x => x.ltAge == model.ltAge)
                .Where(x => x.gtAge == model.gtAge)
                .Where(x => x.Insulin == model.Insulin)
                .Where(x => x.Sulphonylureas == model.Sulphonylureas)
                .Where(x => x.Glucose == model.Glucose)
                .Where(x => x.Renal == model.Renal)
                .Where(x => x.hypo == model.hypo)
                .Where(x => x.id != model.id)
                .Where(x => x.active == true) //hcare-1298
                .ToList().Count();

            if (result > 0) { return true; } else { return false; }

        }
        public List<MHRiskRatingRules> GetMHRiskRatingRules()
        {
            return _context.MHRiskRatingRules.OrderByDescending(x => x.active == true).ThenBy(x => x.effectiveDate).ToList();
        }

        public ServiceResult AddHypoRiskRules(HypoRiskRules model)
        {
            _context.HypoRiskRules.Add(model);
            return _context.SaveChanges();
        }

        public HypoRiskRules GetHypoRiskRulesById(int riskId)
        {
            return _context.HypoRiskRules.Where(x => x.id == riskId).FirstOrDefault();
        }
        public ServiceResult UpdateHypoRiskRules(HypoRiskRules model)
        {
            var old = GetHypoRiskRulesById(model.id);
            old.Insulin = model.Insulin;
            old.Glucose = model.Glucose;
            old.effectiveDate = model.effectiveDate;
            old.Sulphonylureas = model.Sulphonylureas;
            old.Renal = model.Renal;
            old.ltAge = model.ltAge;
            old.gtAge = model.gtAge;
            old.hypo = model.hypo;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.active = model.active;

            return _context.SaveChanges();
        }
        //HCare-840
        public List<HIVRiskRatingRules> GetHIVRiskRules()
        {
            return _context.HIVRiskRatingRules.OrderByDescending(x => x.active == true).ThenBy(x => x.effectiveDate).ToList();
        }

        public bool GetHIVRiskRule(HIVRiskRatingRules model)
        {
            var result = _context.HIVRiskRatingRules.Where(x => x.CD4Count == model.CD4Count)
                .Where(x => x.CD4Percentage == model.CD4Percentage)
                .Where(x => x.viralLoad == model.viralLoad)
                .Where(x => x.ltValue == model.ltValue)
                .Where(x => x.gtValue == model.gtValue)
                .Where(x => x.stage == model.stage)
                .Where(x => x.id != model.id)
                .Where(x => x.active == true) //hcare-1298
                .ToList().Count();

            if (result > 0) { return true; } else { return false; }

        }
        public ServiceResult AddHIVRiskRules(HIVRiskRatingRules model)
        {
            //HCare-839 additions
            var exists = _context.HIVRiskRatingRules
                .Where(x => x.CD4Count == model.CD4Count)
                .Where(x => x.CD4Percentage == model.CD4Percentage)
                .Where(x => x.viralLoad == model.viralLoad)
                .Where(x => x.ltValue == model.ltValue)
                .Where(x => x.gtValue == model.gtValue)
                .Where(x => x.stage == model.stage)
                .Where(x => x.active == true) //hcare-1298
                .FirstOrDefault();

            if (exists == null)
            {
                _context.HIVRiskRatingRules.Add(model);
                return _context.SaveChanges();
            }
            else
            {
                var result = new ServiceResult() { Errors = new List<ServiceError>(), Success = false };
                result.Errors.Add(new ServiceError { Message = "Rule already exists" });
                return result;
            }
        }

        public HIVRiskRatingRules GetHIVRiskRulesById(int riskId)
        {
            return _context.HIVRiskRatingRules.Where(x => x.id == riskId).FirstOrDefault();
        }
        public ServiceResult UpdateHIVRiskRules(HIVRiskRatingRules model)
        {
            var exists = _context.HIVRiskRatingRules
                .Where(x => x.CD4Count == model.CD4Count)
                .Where(x => x.CD4Percentage == model.CD4Percentage)
                .Where(x => x.viralLoad == model.viralLoad)
                .Where(x => x.ltValue == model.ltValue)
                .Where(x => x.gtValue == model.gtValue)
                .Where(x => x.stage == model.stage)
                .Where(x => x.active == model.active).FirstOrDefault();

            if (exists == null)
            {
                var old = GetHIVRiskRulesById(model.id);
                old.CD4Count = model.CD4Count;
                old.CD4Percentage = model.CD4Percentage;
                old.viralLoad = model.viralLoad;
                old.gtValue = model.gtValue;
                old.ltValue = model.ltValue;
                old.stage = model.stage;
                old.modifiedBy = model.modifiedBy;
                old.modifiedDate = DateTime.Now;
                old.active = model.active;

                return _context.SaveChanges();
            }
            else
            {
                var result = new ServiceResult() { Errors = new List<ServiceError>(), Success = false };
                result.Errors.Add(new ServiceError { Message = "Rule already exists" });
                return result;
            }


        }
        //HCare-840

        //HCare-1184
        public List<HIVStagingRiskRules> GetHIVStageRiskRules()
        {
            return _context.HIVStagingRiskRules.OrderByDescending(x => x.active == true).ThenBy(x => x.stage).ToList();
        }

        public bool GetHIVStageRiskRule(HIVStagingRiskRules model)
        {
            var result = _context.HIVStagingRiskRules.Where(x => x.stage == model.stage)
                .Where(x => x.cdl == model.cdl)
                .Where(x => x.renal == model.renal)
                .Where(x => x.cancer == model.cancer)
                .Where(x => x.TB == model.TB).Where(x => x.id != model.id)
                .Where(x => x.active == true) //hcare-1298
                .ToList().Count();

            if (result > 0) { return true; } else { return false; }
        }

        public ServiceResult AddHIVStageRiskRules(HIVStagingRiskRules model)
        {
            var exists = _context.HIVStagingRiskRules
                .Where(x => x.TB == model.TB)
                .Where(x => x.renal == model.renal)
                .Where(x => x.cancer == model.cancer)
                .Where(x => x.cdl == model.cdl)
                .Where(x => x.stage == model.stage)
                .Where(x => x.active == true) //hcare-1298
                .FirstOrDefault();

            if (exists == null)
            {
                _context.HIVStagingRiskRules.Add(model);
                return _context.SaveChanges();
            }
            else
            {
                var result = new ServiceResult() { Errors = new List<ServiceError>(), Success = false };
                result.Errors.Add(new ServiceError { Message = "Rule already exists" });
                return result;
            }
        }
        public HIVStagingRiskRules GetHIVStageRiskRulesById(int riskId)
        {
            return _context.HIVStagingRiskRules.Where(x => x.id == riskId).FirstOrDefault();
        }
        public ServiceResult UpdateHIVStageRiskRules(HIVStagingRiskRules model)
        {
            var exists = _context.HIVStagingRiskRules
                .Where(x => x.cdl == model.cdl)
                .Where(x => x.TB == model.TB)
                .Where(x => x.cancer == model.cancer)
                .Where(x => x.renal == model.renal)
                .Where(x => x.stage == model.stage)
                .Where(x => x.active == model.active)
                .Where(x => x.RiskId == model.RiskId)
                .FirstOrDefault();//HCare-1269

            if (exists == null)
            {
                var old = GetHIVStageRiskRulesById(model.id);
                old.cdl = model.cdl;
                old.RiskId = model.RiskId;//Hcare-1269
                old.TB = model.TB;
                old.cancer = model.cancer;
                old.renal = model.renal;
                old.stage = model.stage;
                old.modifiedBy = model.modifiedBy;
                old.modifiedDate = DateTime.Now;
                old.active = model.active;

                return _context.SaveChanges();
            }
            else
            {
                var result = new ServiceResult() { Errors = new List<ServiceError>(), Success = false };
                result.Errors.Add(new ServiceError { Message = "Rule already exists" });
                return result;
            }
        }
        //end

        public List<LifeExpectancyRules> GetLifeRules()
        {
            return _context.LifeExpectancyRules
                .OrderByDescending(x => x.active == true)
                .ThenBy(x => x.RiskId.ToLower() == "r")
                .ThenBy(x => x.RiskId.ToLower() == "y")
                .ThenBy(x => x.RiskId.ToLower() == "g")
                .ThenBy(x => x.gender.ToLower() == "f")
                .ThenBy(x => x.gender.ToLower() == "m")
                .ToList();

        }

        public bool GetLifeRule(LifeExpectancyRules model)
        {
            var result = _context.LifeExpectancyRules.Where(x => x.gender == model.gender)
                .Where(x => x.RiskId == model.RiskId)
                .Where(x => x.smoker == model.smoker)
                .Where(x => x.hypertension == model.hypertension)
                .Where(x => x.hychol == model.hychol)
                .Where(x => x.gtHba1C == model.gtHba1C)
                .Where(x => x.ltHba1C == model.ltHba1C)
                .Where(x => x.gtAge == model.gtAge)
                .Where(x => x.ltAge == model.ltAge)
                .Where(x => x.id != model.id)
                .Where(x => x.active == true) //hcare-1298
                .ToList().Count();

            if (result > 0)
                return true;
            else
                return false;

        }

        public ServiceResult AddLifeRules(LifeExpectancyRules model)
        {
            _context.LifeExpectancyRules.Add(model);
            return _context.SaveChanges();
        }

        public LifeExpectancyRules GetLifeRulesById(int riskId)
        {
            return _context.LifeExpectancyRules.Where(x => x.id == riskId).FirstOrDefault();
        }
        public ServiceResult UpdateLifeRules(LifeExpectancyRules model)
        {
            var old = GetLifeRulesById(model.id);
            old.gender = model.gender;
            old.gtAge = model.gtAge;
            old.gtHba1C = model.gtHba1C;
            old.ltAge = model.ltAge;
            old.ltHba1C = model.ltHba1C;
            old.hychol = model.hychol;
            old.hypertension = model.hypertension;
            old.smoker = model.smoker;
            old.RiskId = model.RiskId;
            old.effectiveDate = model.effectiveDate;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = DateTime.Now;
            old.active = model.active;

            return _context.SaveChanges();
        }

        public bool GetMHRule(MHRiskRatingRules model)
        {
            var result = _context.MHRiskRatingRules.Where(x => x.Bipolar == model.Bipolar)
                .Where(x => x.Schizophrenia == model.Schizophrenia)
                .Where(x => x.Depression == model.Depression)
                .Where(x => x.ltValue == model.ltValue)
                .Where(x => x.gtValue == model.gtValue)
                .Where(x => x.rating == model.rating)
                .Where(x => x.suicidal == model.suicidal)
                .Where(x => x.id != model.id)
                .Where(x => x.active == true) //hcare-1298
                .ToList().Count();

            if (result > 0) { return true; } else { return false; }

        }
        public ServiceResult AddMHRules(MHRiskRatingRules model)
        {
            _context.MHRiskRatingRules.Add(model);
            return _context.SaveChanges();
        }
        public MHRiskRatingRules GetMHRulesById(int riskId)
        {
            return _context.MHRiskRatingRules.Where(x => x.id == riskId).FirstOrDefault();
        }
        public ServiceResult UpdateMHRules(MHRiskRatingRules model)
        {
            var old = GetMHRulesById(model.id);
            old.Depression = model.Depression;
            old.Schizophrenia = model.Schizophrenia;
            old.Bipolar = model.Bipolar;
            old.ltValue = model.ltValue;
            old.gtValue = model.gtValue;
            old.active = model.active;
            old.suicidal = model.suicidal;
            old.rating = model.rating;
            old.effectiveDate = model.effectiveDate;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = DateTime.Now;
            old.active = model.active;

            return _context.SaveChanges();
        }

        public List<ClinicalRules> GetClinicalRiskRules()
        {
            return _context.ClinicalRules.OrderByDescending(x => x.active == true)
                .ThenBy(x => x.ruleType)
                .ThenBy(x => x.ruleName)
                .ToList();
        }

        public List<ClinicalRules> GetClinicalRiskRuleValidation()
        {
            return _context.ClinicalRules.Where(x => x.active == true).ToList();
        }
        public List<ClinicalRules> GetClinicalRiskRulesList()
        {
            var results = _context.ClinicalRules.OrderByDescending(x => x.active == true).ThenBy(x => x.ruleType).ThenBy(x => x.ruleName).ToList();
            foreach (var result in results)
            {
                result.ruleType = _context.Program.Where(x => x.code == result.ruleType).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }

        public ClinicalRules GetClinicalRuleByName(string clinicalRule)
        {
            return _context.ClinicalRules.Where(x => x.ruleName.ToLower() == clinicalRule.ToLower()).FirstOrDefault();
        }

        public ServiceResult AddClinicalRules(ClinicalRules model)
        {
            _context.ClinicalRules.Add(model);
            return _context.SaveChanges();
        }
        public ClinicalRules GetClinicalRulesById(int id)
        {
            return _context.ClinicalRules.Where(x => x.id == id).FirstOrDefault();
        }

        public ClinicalRules GetClinicalRulesInfo(int id)
        {
            var results = _context.ClinicalRules.Where(x => x.id == id).FirstOrDefault();
            results.assignmentItem = _context.AssignmentItemTypes.Where(x => x.assignmentItemType == results.assignmentItem).Select(x => x.itemDescription).FirstOrDefault();

            return results;

        }
        public ServiceResult UpdateClinicalRules(ClinicalRules model)
        {
            var old = GetClinicalRulesById(model.id);
            old.male = model.male;
            old.female = model.female;
            old.greater = model.greater;
            old.gtMessage = model.gtMessage;
            old.less = model.less;
            old.ltMessage = model.ltMessage;
            old.ruleType = model.ruleType;
            old.ruleName = model.ruleName;
            old.pathologyField = model.pathologyField;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.hasAssignment = model.hasAssignment;
            old.assignmentItem = model.assignmentItem;
            old.Instruction = model.Instruction;
            old.active = model.active;

            return _context.SaveChanges();
        }

        public bool GetUserByUsernameEmail(string inputstr)
        {
            var user = _context.Users.Where(x => x.username == inputstr || x.email == inputstr).ToList();
            if (user.Count() == 0)
                return false;

            return true;
        }

        public List<ProgramStatuses> GetProgramStatuses()
        {
            return _context.ProgramStatuses.OrderByDescending(x => x.active == true)
                .ThenBy(x => x.programCode)
                .ThenBy(x => x.statusCode)
                .ToList();
        }

        public ServiceResult AddProgramStatus(ProgramStatuses model)
        {
            _context.ProgramStatuses.Add(model);

            return _context.SaveChanges();
        }
        public ProgramStatuses GetProgramStatusessById(int id)
        {
            return _context.ProgramStatuses.Where(x => x.Id == id).FirstOrDefault();
        }

        public ServiceResult UpdateProgramStatuses(ProgramStatuses model)
        {
            var old = GetProgramStatusessById(model.Id);
            old.programCode = model.programCode;
            old.statusCode = model.statusCode;
            old.active = model.active;

            return _context.SaveChanges();
        }



        public List<Accounts> GetAccounts()
        {
            return _context.Accounts.ToList();
        }

        public AccountDetailsViewModel GetAccountDetails(Guid Id)
        {
            var acc = _context.Accounts.Where(x => x.Id == Id).FirstOrDefault();

            var medaids = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAids
                           on s.MedicalAidId equals ma.MedicalAidID
                           join mp in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals mp.MedicalAidID
                           join map in _context.MedicalAidPlans
                           on mp.planID equals map.Id
                           where s.AccountId == Id
                           where ma.Active == true
                           where s.Active == true
                           where map.active == true
                           where mp.Active == true
                           group new { s, ma, mp, map } by map.Name into grp
                           select grp.FirstOrDefault())
                           .Select(p => new AccountMedicalAidViewModel
                           {
                               accountId = p.s.AccountId,
                               medicalAidId = p.s.MedicalAidId,
                               optionId = p.map.Id,
                               MedicalAidName = p.ma.Name,
                               OptionName = p.map.Name,
                               active = p.mp.Active,
                           }).ToList();


            Guid[] ids = new Guid[medaids.Count()];

            var clinicals = new List<AccountMedicalAidViewModel>();

            for (int id = 0; id < medaids.Count(); id++)
            {
                ids[id] = medaids[id].optionId;
            }

            foreach (var medaid in medaids)
            {
                clinicals.Add(medaid);
            }


            var meds = (from s in _context.AccountSchemes
                        join ma in _context.MedicalAids
                        on s.MedicalAidId equals ma.MedicalAidID
                        join map in _context.MedicalAidPlans
                        on ma.MedicalAidID equals map.medicalAidId
                        where s.AccountId == Id
                        where ma.Active == true
                        where s.Active == true
                        where map.active == true
                        group new { s, ma, map } by map.Name into grp
                        select grp.FirstOrDefault())
                           .Select(p => new AccountMedicalAidViewModel
                           {
                               accountId = p.s.AccountId,
                               medicalAidId = p.s.MedicalAidId,
                               optionId = p.map.Id,
                               MedicalAidName = p.ma.Name,
                               OptionName = p.map.Name,
                               active = false,
                           }).ToList();

            foreach (var med in meds)
            {
                if (!ids.Contains(med.optionId))
                {
                    clinicals.Add(med);
                }
            }

            var results = new AccountDetailsViewModel()
            {
                accountID = acc.Id,
                name = acc.name,
                description = acc.description,
                medicalAids = clinicals,
            };

            return results;
        }

        public List<StatusTypes> GetStatusTypes()
        {
            return _context.StatusTypes.ToList();
        }

        //=========================================================================================================================================================================//
        //                                                                               Laboratories                                                                              // 
        //=========================================================================================================================================================================//

        public List<Laboratory> GetListOfLaboratories()
        {
            return _context.laboratories.OrderByDescending(x => x.active == true)
                .ThenBy(x => x.name)
                .ToList();
        }

        public Laboratory GetLaboratorybyName(string name)
        {
            return _context.laboratories.Where(x => x.name.ToLower() == name.ToLower()).Where(x => x.active == true).FirstOrDefault(); //hcare-1298
        }

        public bool GetLaboratoryName(Laboratory model)
        {
            var result = _context.laboratories.Where(x => x.name == model.name)
                .Where(x => x.id != model.id)
                .Where(x => x.active == true) //hcare-1298
                .ToList().Count();

            if (result > 0) { return true; } else { return false; }

        }
        public ServiceResult InsertLaboratory(Laboratory model)
        {
            _context.laboratories.Add(model);
            return _context.SaveChanges();
        }

        public Laboratory GetLaboratoryById(int id)
        {
            return _context.laboratories.Where(x => x.id == id).FirstOrDefault();
        }

        public ServiceResult UpdateLaboratory(Laboratory model)
        {
            var old = _context.laboratories.Where(x => x.id == model.id).FirstOrDefault();
            old.name = model.name;
            old.telephoneNo = model.telephoneNo;
            old.email = model.email;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.active = model.active;
            return _context.SaveChanges();
        }


        //=========================================================================================================================================================================//
        //                                                                                     Accounts                                                                            // 
        //=========================================================================================================================================================================//

        public Guid InsertAccount(Accounts account)
        {
            account.createdDate = DateTime.Now;
            account.Active = true;
            _context.Accounts.Add(account);
            Save();
            return account.Id;
        }


        public Accounts GetAccountByName(string name)
        {
            return _context.Accounts.Where(x => x.name.ToUpper() == name.ToUpper()).FirstOrDefault();
        }

        public List<MedicalAid> GetNonLinkedSchemes()
        {
            var schemes = _context.AccountSchemes.Select(x => x.MedicalAidId).ToList();
            var results = (from ma in _context.MedicalAids
                           where ma.Active == true
                           where !schemes.Contains(ma.MedicalAidID)
                           select ma).OrderBy(x => x.Name).ToList();

            return results;
        }

        public AccountSchemes GetAccountSchemeById(Guid id)
        {
            return _context.AccountSchemes.Where(x => x.Id == id).FirstOrDefault();
        }



        public ServiceResult InsertAccountSchemes(AccountSchemes scheme)
        {
            scheme.createdDate = DateTime.Now;
            _context.AccountSchemes.Add(scheme);
            return _context.SaveChanges();
        }

        public ServiceResult InsertMedicalAidPlanProgram(MedicalAidPlanPrograms prog)
        {
            var old = _context.MedicalAidPlanPrograms.Where(x => x.planID == prog.planID).Where(x => x.MedicalAidID == prog.MedicalAidID).Where(x => x.programID == prog.programID).FirstOrDefault();
            if (old == null)
            {
                prog.createdDate = DateTime.Now;
                _context.MedicalAidPlanPrograms.Add(prog);
                return _context.SaveChanges();
            }
            else
            {
                ServiceResult serv = new ServiceResult();
                serv.Errors.Add(new ServiceError() { Message = "Already exists" });
                return serv;
            }

        }

        public ServiceResult InsertMedicalAidProgram(MedicalAidPrograms prog)
        {
            var id = _context.MedicalAidPrograms.OrderByDescending(x => x.id).Select(x => x.id).FirstOrDefault();
            id++;

            string sql = string.Format(@"INSERT INTO MedicalAidPrograms Values ({0},'{1}','{2}','{3}','{4}',{5},'{6}', '{7}')", id, prog.medicalAidId, prog.program, prog.createdBy, prog.createdDate.ToString("dd MMM yyyy"), 1, prog.codeType, prog.referralFrom); //HCare-1213

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                db.Execute(sql, null);
                db.Close();

                return null;
            }


        }

        public List<MedicalAidPlanPrograms> GetPlanProgramByScheme(Guid medicalAidId)
        {
            return _context.MedicalAidPlanPrograms.Where(x => x.MedicalAidID == medicalAidId).Where(x => x.Active == true).ToList();
        }

        public MedicalAidPlanPrograms GetPlanProgramByID(Guid planID)
        {
            return _context.MedicalAidPlanPrograms.Where(x => x.Id == planID).FirstOrDefault();
        }

        public ServiceResult UpdateAccountSchemes(AccountSchemes scheme)
        {
            var old = _context.AccountSchemes.Where(x => x.Id == scheme.Id).FirstOrDefault();
            old.Authorizations = scheme.Authorizations;
            old.modifiedBy = scheme.modifiedBy;
            old.modifiedDate = scheme.modifiedDate;
            old.Active = scheme.Active;
            return _context.SaveChanges();
        }

        public ServiceResult DeactivateProgramPlans(MedicalAidPlanPrograms model)
        {
            var old = _context.MedicalAidPlanPrograms.Where(x => x.Id == model.Id).FirstOrDefault();
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.Active = false;
            return _context.SaveChanges();
        }

        public List<MedicalAidSetupViewModal> GetAccountMedicalAids(Guid accountID)
        {
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAids
                           on s.MedicalAidId equals ma.MedicalAidID
                           where s.AccountId == accountID
                           where ma.Active == true
                           where s.Active == true
                           group new { s, ma } by s.Id into grp
                           select grp.FirstOrDefault())
                         .Select(p => new MedicalAidSetupViewModal
                         {
                             Id = p.s.Id,
                             medicalAidID = p.ma.MedicalAidID,
                             Name = p.ma.Name,
                             allowAuth = p.s.Authorizations,
                         }).OrderBy(x => x.Name).ToList();

            return results;
        }

        public List<ClinicalRulesSetupViewModal> GetClinicalRiskRulesSetup(Guid AccountId, Guid progID, Guid medaid)
        {
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join cl in _context.ClinicalRules
                           on pr.code equals cl.ruleType
                           where cl.active == true
                           where s.AccountId == AccountId
                           where s.MedicalAidId == medaid
                           where pr.programID == progID
                           where ma.Active == true
                           where s.Active == true
                           select new ClinicalRulesSetupViewModal
                           {
                               accountId = AccountId,
                               programId = pr.programID,
                               Added = false,
                               ruleName = cl.ruleName,
                               ruleType = cl.ruleType,
                               female = cl.female,
                               male = cl.male,
                               greater = cl.greater,
                               less = cl.less,
                               gtMessage = cl.gtMessage,
                               ltMessage = cl.ltMessage,
                               pathologyField = cl.pathologyField,
                               id = cl.id,
                               mediaidId = ma.MedicalAidID,
                           }).GroupBy(x => x.ruleName).Select(g => g.FirstOrDefault()).ToList();

            return results;
        }

        public List<ClinicalRules> GetClinicalRulesByScheme(Guid MedicalAidId)
        {
            var results = (from s in _context.AccountPlanRules
                           join cl in _context.ClinicalRules
                           on s.RuleId equals cl.id
                           join map in _context.MedicalAidPlans
                           on s.MedicalAidId equals map.medicalAidId
                           where s.MedicalAidId == MedicalAidId
                           where cl.active == true
                           where map.active == true
                           where s.Active == true
                           group cl by cl.id into uniqueIds
                           select uniqueIds.FirstOrDefault()).ToList();

            return results;
        }

        //HCare-1087
        public List<MedicalAidProgramViewModel> GetMedicalAidPrograms(Guid MedicalAidId)
        {

            var results = (from s in _context.MedicalAidPrograms
                           join pr in _context.Program
                           on s.program equals pr.code
                           where pr.Active == true
                           where s.medicalAidId == MedicalAidId.ToString()
                           select new MedicalAidProgramViewModel
                           {
                               id = s.id,
                               medicalAidId = s.medicalAidId,
                               program = pr.code,
                               codeType = s.codeType,
                               Active = s.Active,
                               referralFrom = s.referralFrom

                           }).GroupBy(x => x.program).Select(g => g.FirstOrDefault()).ToList();


            string[] ids = new string[results.Count()];

            var clinicals = new List<MedicalAidProgramViewModel>();
            clinicals.AddRange(results);

            for (int id = 0; id < results.Count(); id++)
            {
                ids[id] = results[id].program;
            }

            var rules = _context.Program.Where(x => x.Active == true).ToList();
            foreach (var rule in rules)
            {
                if (ids.Contains(rule.code))
                {
                }
                else
                {
                    clinicals.Add(new MedicalAidProgramViewModel
                    {
                        id = 0,
                        medicalAidId = null,
                        program = rule.code,
                        codeType = rule.ProgramName,
                        Active = false
                    });
                }
            }

            return clinicals;
        }

        public MedicalAidPrograms GetMedicalAidProgram(Guid MedicalAidId, string program)
        {
            return _context.MedicalAidPrograms.Where(x => x.medicalAidId == MedicalAidId.ToString()).Where(x => x.program == program).FirstOrDefault();
        }

        public List<ClinicalRulesSetupViewModal> GetClinicalRiskRulesEdit(Guid AccountId, Guid progID, Guid medaid)
        {

            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join ap in _context.AccountPlanRules
                           on s.MedicalAidId equals ap.MedicalAidId
                           join cl in _context.ClinicalRules
                           on ap.RuleId equals cl.id
                           where cl.active == true
                           where s.AccountId == AccountId
                           where s.MedicalAidId == medaid
                           where pr.programID == progID
                           where ma.Active == true
                           where s.Active == true
                           where ap.Active == true
                           select new ClinicalRulesSetupViewModal
                           {
                               planruleID = ap.Id,
                               accountId = AccountId,
                               programId = pr.programID,
                               Added = ap.Active,
                               ruleName = cl.ruleName,
                               ruleType = cl.ruleType,
                               female = cl.female,
                               male = cl.male,
                               greater = cl.greater,
                               less = cl.less,
                               gtMessage = cl.gtMessage,
                               ltMessage = cl.ltMessage,
                               pathologyField = cl.pathologyField,
                               id = cl.id,
                               mediaidId = ma.MedicalAidID
                           }).GroupBy(x => x.ruleName).Select(g => g.FirstOrDefault()).ToList();

            int[] ids = new int[results.Count()];

            var clinicals = new List<ClinicalRulesSetupViewModal>();

            for (int id = 0; id < results.Count(); id++)
            {
                ids[id] = results[id].id;
            }

            var rules = GetClinicalRiskRules().Where(x => x.active == true).Where(x => x.ruleType == _context.Program.Where(p => p.programID == progID).Select(p => p.code).FirstOrDefault()).ToList();
            foreach (var rule in rules)
            {
                if (ids.Contains(rule.id))
                {
                    clinicals.Add(new ClinicalRulesSetupViewModal
                    {
                        accountId = AccountId,
                        programId = progID,
                        Added = true,
                        ruleName = rule.ruleName,
                        ruleType = rule.ruleType,
                        female = rule.female,
                        male = rule.male,
                        greater = rule.greater,
                        less = rule.less,
                        gtMessage = rule.gtMessage,
                        ltMessage = rule.ltMessage,
                        pathologyField = rule.pathologyField,
                        id = rule.id,
                        mediaidId = medaid
                    });
                }
                else
                {
                    clinicals.Add(new ClinicalRulesSetupViewModal
                    {
                        accountId = AccountId,
                        programId = progID,
                        Added = false,
                        ruleName = rule.ruleName,
                        ruleType = rule.ruleType,
                        female = rule.female,
                        male = rule.male,
                        greater = rule.greater,
                        less = rule.less,
                        gtMessage = rule.gtMessage,
                        ltMessage = rule.ltMessage,
                        pathologyField = rule.pathologyField,
                        id = rule.id,
                        mediaidId = medaid
                    });
                }
            }

            return clinicals;
        }

        public ClinicalRulesSetupViewModal GetAccountClinicalRiskRules(Guid AccountId, Guid progID, Guid medaid, int ruleid)
        {

            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join ap in _context.AccountPlanRules
                           on s.MedicalAidId equals ap.MedicalAidId
                           join cl in _context.ClinicalRules
                           on ap.RuleId equals ruleid
                           where cl.active == true
                           where s.AccountId == AccountId
                           where s.MedicalAidId == medaid
                           where pr.programID == progID
                           where ma.Active == true
                           where s.Active == true
                           select new ClinicalRulesSetupViewModal
                           {
                               planruleID = ap.Id,
                               accountId = AccountId,
                               programId = pr.programID,
                               Added = ap.Active,
                               ruleName = cl.ruleName,
                               ruleType = cl.ruleType,
                               female = cl.female,
                               male = cl.male,
                               greater = cl.greater,
                               less = cl.less,
                               gtMessage = cl.gtMessage,
                               ltMessage = cl.ltMessage,
                               pathologyField = cl.pathologyField,
                               id = cl.id,
                               mediaidId = ma.MedicalAidID,
                           }).GroupBy(x => x.ruleName).Select(g => g.FirstOrDefault()).FirstOrDefault();


            return results;
        }

        public List<ClinicalRulesSetupInitialViewModel> GetClinicalRiskRulesSelection(Guid AccountId, Guid medId)
        {
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join ap in _context.AccountPlanRules
                           on s.MedicalAidId equals ap.MedicalAidId into acr
                           from ed in acr.DefaultIfEmpty()
                           where s.AccountId == AccountId
                           where s.MedicalAidId == medId
                           where ma.Active == true
                           where s.Active == true
                           select new ClinicalRulesSetupInitialViewModel
                           {
                               accountID = AccountId,
                               programID = pr.programID,
                               done = ed == null ? false : true,
                               programName = pr.ProgramName,
                           }).GroupBy(x => x.programID).Select(g => g.FirstOrDefault()).ToList();

            return results;
        }

        public List<StatusManagmentViewModel> GetManagementStatusSetup(Guid AccountId, Guid program)
        {

            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join cl in _context.ManagementStatus
                           on pr.code equals cl.programCode
                           where cl.active == true
                           where s.AccountId == AccountId
                           where pr.programID == program
                           where ma.Active == true
                           where s.Active == true
                           select new StatusManagmentViewModel
                           {
                               accountId = AccountId,
                               programId = pr.programID,
                               medicalAidId = ma.MedicalAidID,
                               Added = false,
                               statusName = cl.statusName,
                               statusCode = cl.statusCode,
                           }).GroupBy(x => x.statusCode).Select(g => g.FirstOrDefault()).ToList();

            return results;
        }
        //HCare-1043
        public List<TextTemplateViewModel> GetTextTemplateSetup(Guid AccountId, Guid program)
        {

            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join cl in _context.SmsMessageTemplates
                           on pr.code equals cl.program
                           where cl.Active == true
                           where s.AccountId == AccountId
                           //where pr.programID == program HCare-1098
                           where ma.Active == true
                           where s.Active == true
                           select new TextTemplateViewModel
                           {
                               accountId = AccountId,
                               programId = pr.programID,
                               medicalAidId = ma.MedicalAidID,
                               Added = false,
                               templateName = cl.title,
                               textMessage = cl.template,
                               templateId = cl.templateID
                           }).GroupBy(x => x.templateId).Select(g => g.FirstOrDefault()).ToList().Where(x => x.programId == program).ToList();

            return results;
        }

        //HCare-1043
        public TextTemplateViewModel GetAccountTextTemplate(Guid AccountId, Guid program, Guid medAid, int templateId)
        {
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join cl in _context.AccountTextTemplates
                           on new { A = s.AccountId, B = s.MedicalAidId, C = pr.programID } equals new { A = cl.AccountId, B = cl.SchemeId, C = cl.ProgramId }//HCare-1027 //HCare-1098
                           where s.AccountId == AccountId
                           where pr.programID == program
                           where cl.SchemeId == medAid
                           where cl.templateId == templateId
                           where ma.Active == true
                           where s.Active == true
                           select new TextTemplateViewModel
                           {
                               id = cl.Id,
                               accountId = AccountId,
                               programId = pr.programID,
                               medicalAidId = ma.MedicalAidID,
                               Added = cl.Active,
                               templateId = cl.templateId,
                           }).GroupBy(x => x.templateId).Select(g => g.FirstOrDefault()).FirstOrDefault();

            return results;
        }

        //HCare-1043
        public EmailTemplateViewModel GetAccountEmailTemplate(Guid AccountId, Guid program, Guid medAid, int templateId)
        {
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join cl in _context.AccountEmailTemplates
                           on new { A = s.AccountId, B = s.MedicalAidId, C = pr.programID } equals new { A = cl.AccountId, B = cl.SchemeId, C = cl.ProgramId }//HCare-1027
                           where s.AccountId == AccountId
                           where pr.programID == program
                           where cl.SchemeId == medAid
                           where cl.templateId == templateId
                           where ma.Active == true
                           where s.Active == true
                           select new EmailTemplateViewModel
                           {
                               id = cl.Id,
                               accountId = AccountId,
                               programId = pr.programID,
                               medicalAidId = ma.MedicalAidID,
                               Added = cl.Active,
                               templateId = cl.templateId,
                           }).GroupBy(x => x.templateId).Select(g => g.FirstOrDefault()).FirstOrDefault();

            return results;
        }

        public StatusManagmentViewModel GetAccountManagementStatus(Guid AccountId, Guid program, Guid medAid, string code)
        {
            //HCare-1020
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join cl in _context.AccountManagementStatuses
                           on new { A = s.AccountId, B = s.MedicalAidId } equals new { A = cl.AccountId, B = cl.SchemeId }//HCare-1027
                           where s.AccountId == AccountId
                           where pr.programID == program
                           where cl.SchemeId == medAid
                           where cl.statusCode == code
                           where ma.Active == true
                           where s.Active == true
                           select new StatusManagmentViewModel
                           {
                               id = cl.Id,
                               accountId = AccountId,
                               programId = pr.programID,
                               medicalAidId = ma.MedicalAidID,
                               Added = cl.Active,
                               statusCode = cl.statusCode,
                           }).GroupBy(x => x.statusCode).Select(g => g.FirstOrDefault()).FirstOrDefault();

            return results;
        }
        //HCare-1043
        public List<TextTemplateViewModel> GetTextTemplatesEdit(Guid AccountId, Guid program, Guid medAid)
        {
            //HCare-1043
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join cl in _context.AccountTextTemplates
                           on new { A = s.AccountId, B = s.MedicalAidId, C = pr.programID } equals new { A = cl.AccountId, B = cl.SchemeId, C = cl.ProgramId } //HCare-1027, 1098
                           where s.AccountId == AccountId
                           where pr.programID == program
                           where cl.SchemeId == medAid
                           where ma.Active == true
                           where s.Active == true
                           select new TextTemplateViewModel
                           {
                               id = cl.Id,
                               accountId = AccountId,
                               programId = pr.programID,
                               medicalAidId = ma.MedicalAidID,
                               Added = cl.Active,
                               templateId = cl.templateId,
                           }).GroupBy(x => x.templateId).Select(g => g.FirstOrDefault()).ToList();

            int[] ids = new int[results.Count()];

            var clinicals = new List<TextTemplateViewModel>();

            for (int id = 0; id < results.Count(); id++)
            {
                ids[id] = results[id].templateId;
            }

            var rules = _context.SmsMessageTemplates.Where(x => x.Active == true).Where(x => x.program == _context.Program.Where(p => p.programID == program).Select(p => p.code).FirstOrDefault() || x.program == null).ToList(); //HCare-1098
            foreach (var rule in rules)
            {
                if (ids.Contains(rule.templateID))
                {
                    clinicals.Add(new TextTemplateViewModel
                    {
                        id = results.Where(x => x.medicalAidId == medAid).Where(x => x.templateId == rule.templateID).Where(x => x.programId == program).Select(x => x.id).FirstOrDefault(),
                        accountId = AccountId,
                        programId = program,
                        Added = results.Where(x => x.medicalAidId == medAid).Where(x => x.templateId == rule.templateID).Where(x => x.programId == program).Select(x => x.Added).FirstOrDefault(),
                        templateId = rule.templateID,
                        templateName = rule.title,
                        medicalAidId = medAid,
                        textMessage = rule.template,
                    });
                }
                else
                {
                    clinicals.Add(new TextTemplateViewModel
                    {
                        accountId = AccountId,
                        programId = program,
                        Added = false,
                        templateId = rule.templateID,
                        templateName = rule.title,
                        medicalAidId = medAid,
                        textMessage = rule.template,
                    });
                }
            }

            return clinicals;
        }

        //HCare-1043
        public List<EmailTemplateViewModel> GetEmailTemplatesEdit(Guid AccountId, Guid program, Guid medAid)
        {
            //HCare-1043
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join cl in _context.AccountEmailTemplates
                           on new { A = s.AccountId, B = s.MedicalAidId, C = pr.programID } equals new { A = cl.AccountId, B = cl.SchemeId, C = cl.ProgramId } //HCare-1027
                           where s.AccountId == AccountId
                           where pr.programID == program
                           where cl.SchemeId == medAid
                           where ma.Active == true
                           where s.Active == true
                           select new EmailTemplateViewModel
                           {
                               id = cl.Id,
                               accountId = AccountId,
                               programId = pr.programID,
                               medicalAidId = ma.MedicalAidID,
                               Added = cl.Active,
                               templateId = cl.templateId,
                           }).GroupBy(x => x.templateId).Select(g => g.FirstOrDefault()).ToList();

            int[] ids = new int[results.Count()];

            var clinicals = new List<EmailTemplateViewModel>();

            for (int id = 0; id < results.Count(); id++)
            {
                ids[id] = results[id].templateId;
            }

            var rules = _context.EmailTemplates.Where(x => x.Active == true).Where(x => x.program == _context.Program.Where(p => p.programID == program).Select(p => p.code).FirstOrDefault() || x.program == null).ToList();
            foreach (var rule in rules)
            {
                if (ids.Contains(rule.templateID))
                {
                    clinicals.Add(new EmailTemplateViewModel
                    {
                        id = results.Where(x => x.medicalAidId == medAid).Where(x => x.templateId == rule.templateID).Select(x => x.id).FirstOrDefault(),
                        accountId = AccountId,
                        programId = program,
                        Added = results.Where(x => x.medicalAidId == medAid).Where(x => x.templateId == rule.templateID).Select(x => x.Added).FirstOrDefault(),
                        templateId = rule.templateID,
                        subject = rule.templateHeading,
                        medicalAidId = medAid,
                        body = rule.templateBody,
                        title = rule.title
                    });
                }
                else
                {
                    clinicals.Add(new EmailTemplateViewModel
                    {
                        accountId = AccountId,
                        programId = program,
                        Added = false,
                        templateId = rule.templateID,
                        subject = rule.templateHeading,
                        medicalAidId = medAid,
                        body = rule.templateBody,
                        title = rule.title
                    });
                }
            }

            return clinicals;
        }

        public List<StatusManagmentViewModel> GetManagementStatusEdit(Guid AccountId, Guid program, Guid medAid)
        {
            //HCare-1020
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join cl in _context.AccountManagementStatuses
                           on new { A = s.AccountId, B = s.MedicalAidId } equals new { A = cl.AccountId, B = cl.SchemeId } //HCare-1027
                           where s.AccountId == AccountId
                           where pr.programID == program
                           where cl.SchemeId == medAid
                           where ma.Active == true
                           where s.Active == true
                           select new StatusManagmentViewModel
                           {
                               id = cl.Id,
                               accountId = AccountId,
                               programId = pr.programID,
                               medicalAidId = ma.MedicalAidID,
                               Added = cl.Active,
                               statusCode = cl.statusCode,
                           }).GroupBy(x => x.statusCode).Select(g => g.FirstOrDefault()).ToList();

            string[] ids = new string[results.Count()];

            var clinicals = new List<StatusManagmentViewModel>();

            for (int id = 0; id < results.Count(); id++)
            {
                ids[id] = results[id].statusCode;
            }

            var rules = _context.ManagementStatus.Where(x => x.active == true).Where(x => x.programCode == _context.Program.Where(p => p.programID == program).Select(p => p.code).FirstOrDefault()).ToList();
            foreach (var rule in rules)
            {
                if (ids.Contains(rule.statusCode))
                {
                    clinicals.Add(new StatusManagmentViewModel
                    {
                        id = results.Where(x => x.medicalAidId == medAid).Where(x => x.statusCode == rule.statusCode).Select(x => x.id).FirstOrDefault(),
                        accountId = AccountId,
                        programId = program,
                        Added = results.Where(x => x.medicalAidId == medAid).Where(x => x.statusCode == rule.statusCode).Select(x => x.Added).FirstOrDefault(),
                        statusCode = rule.statusCode,
                        statusName = rule.statusName,
                        medicalAidId = medAid,
                    });
                }
                else
                {
                    clinicals.Add(new StatusManagmentViewModel
                    {
                        accountId = AccountId,
                        programId = program,
                        Added = false,
                        statusCode = rule.statusCode,
                        statusName = rule.statusName,
                        medicalAidId = medAid,
                    });
                }
            }

            return clinicals;
        }

        public List<PlanCheckProgramViewModel> GetMedicalAidPlanPrograms(Guid AccountId, Guid medId)
        {

            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join cl in _context.MedicalAidPlans
                           on ma.planID equals cl.Id
                           where cl.active == true
                           where s.AccountId == AccountId
                           where ma.Active == true
                           where s.Active == true
                           select new PlanCheckProgramViewModel
                           {
                               accountID = s.AccountId,
                               planProgramId = ma.Id,
                               planName = cl.Name,
                               programName = pr.ProgramName,
                               include = true,
                               medId = medId,
                           }).OrderBy(x => x.planName).ToList();

            return results;
        }

        public List<UserSchemeAccessViewModel> GetUsermedicalAidAccess(Guid userID)
        {
            var results = (from s in _context.UserSchemeAccess
                           join ma in _context.MedicalAids
                           on s.medicalAidId equals ma.MedicalAidID
                           where s.userId == userID
                           where s.Active == true
                           select new UserSchemeAccessViewModel
                           {
                               userId = s.userId,
                               medicalAidId = s.medicalAidId,
                               Id = s.Id,
                               medicalAidName = ma.Name,
                               hasAccess = s.Active, //hcare-1288

                           }).OrderByDescending(x => x.hasAccess == true).ThenBy(x => x.medicalAidName).ToList(); //hcare-1288

            Guid[] ids = new Guid[results.Count()];

            var clinicals = new List<UserSchemeAccessViewModel>();

            for (int id = 0; id < results.Count(); id++)
            {
                ids[id] = results[id].medicalAidId;
            }

            foreach (var result in results)
            {
                clinicals.Add(result);
            }

            var medicalAids = _context.MedicalAids.ToList(); //hcare-1288 - removed-active-check
            foreach (var medicalAid in medicalAids)
            {
                if (!ids.Contains(medicalAid.MedicalAidID))
                {
                    clinicals.Add(new UserSchemeAccessViewModel
                    {
                        userId = userID,
                        medicalAidId = medicalAid.MedicalAidID,
                        medicalAidName = medicalAid.Name,
                        hasAccess = false,
                    });
                }
            }


            return clinicals;
        }
        public UserSchemeAccess GetAccessByMedicalAidAndUserID(string medicalaidID, string userID)
        {
            return _context.UserSchemeAccess.Where(x => x.medicalAidId == new Guid(medicalaidID)).Where(x => x.userId == new Guid(userID)).FirstOrDefault();
        }

        public List<UserProgramViewModal> GetUserProgramAccess(Guid userID, Guid medID)
        {

            var results = (from s in _context.UserProgramAccess
                           join ma in _context.Program
                           on s.programId equals ma.programID
                           where s.userId == userID
                           where s.medicalAidId == medID
                           where s.Active == true
                           select new UserProgramViewModal
                           {
                               userId = s.userId,
                               programId = s.programId,
                               Id = s.Id,
                               medicalAidId = medID,
                               programName = ma.ProgramName,
                               hasAccess = ma.Active,
                           }).OrderBy(x => x.programName).ToList();

            Guid[] ids = new Guid[results.Count()];

            var clinicals = new List<UserProgramViewModal>();

            for (int id = 0; id < results.Count(); id++)
            {
                ids[id] = results[id].programId;
            }

            foreach (var result in results)
            {
                clinicals.Add(result);
            }

            var programs = _context.Program.Where(x => x.Active == true).ToList();
            foreach (var program in programs)
            {
                if (!ids.Contains(program.programID))
                {
                    clinicals.Add(new UserProgramViewModal
                    {
                        userId = userID,
                        medicalAidId = medID,
                        programId = program.programID,
                        programName = program.ProgramName,
                        hasAccess = false,
                    });
                }
            }

            return clinicals;
        }

        public List<PlanCheckProgramViewModel> GetMedicalAidPlanProgramsEdit(Guid AccountId, Guid optionID)
        {

            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join cl in _context.MedicalAidPlans
                           on ma.planID equals cl.Id
                           where cl.active == true
                           where s.AccountId == AccountId
                           where cl.Id == optionID
                           where s.Active == true
                           select new PlanCheckProgramViewModel
                           {
                               optionId = cl.Id,
                               medicalAidID = ma.MedicalAidID,
                               accountID = s.AccountId,
                               planProgramId = ma.Id,
                               programID = ma.programID,
                               planName = cl.Name,
                               programName = pr.ProgramName,
                               include = ma.Active,
                           }).OrderBy(x => x.planName).ToList();

            Guid[] ids = new Guid[results.Count()];

            var clinicals = new List<PlanCheckProgramViewModel>();

            for (int id = 0; id < results.Count(); id++)
            {
                ids[id] = results[id].programID;
            }

            foreach (var result in results)
            {
                clinicals.Add(result);
            }
            if (results.Count != 0)
            {
                var programs = GetPrograms();
                foreach (var program in programs)
                {
                    if (!ids.Contains(program.programID))
                    {
                        clinicals.Add(new PlanCheckProgramViewModel
                        {
                            optionId = optionID,
                            medicalAidID = results[0].medicalAidID,
                            accountID = AccountId,
                            programID = program.programID,
                            planName = results[0].planName,
                            programName = program.ProgramName,
                            include = false,
                        });
                    }
                }
            }

            return clinicals;
        }

        public List<PlanCheckProgramViewModel> GetMedicalAidPlanProgramsEd(Guid AccountId, Guid optionID)
        {

            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms
                           on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program
                           on ma.programID equals pr.programID
                           join cl in _context.MedicalAidPlans
                           on ma.planID equals cl.Id
                           where cl.active == true
                           where s.AccountId == AccountId
                           where cl.Id == optionID
                           where s.Active == true
                           where ma.Active == true
                           select new PlanCheckProgramViewModel
                           {
                               optionId = cl.Id,
                               medicalAidID = ma.MedicalAidID,
                               accountID = s.AccountId,
                               planProgramId = ma.Id,
                               programID = ma.programID,
                               planName = cl.Name,
                               programName = pr.ProgramName,
                               include = ma.Active,
                           }).OrderBy(x => x.planName).ToList();

            return results;
        }

        public MedicalAidSetupViewModal GetAccountMedicalAid(Guid Id)
        {
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAids
                           on s.MedicalAidId equals ma.MedicalAidID
                           where s.Id == Id
                           where ma.Active == true
                           where s.Active == true
                           select new MedicalAidSetupViewModal
                           {
                               Id = s.Id,
                               medicalAidID = ma.MedicalAidID,
                               Name = ma.Name,
                               allowAuth = s.Authorizations,
                           }).OrderBy(x => x.Name).FirstOrDefault();

            return results;
        }


        public ServiceResult InsertAccessServices(AccessServices services)
        {
            services.Id = Guid.NewGuid();
            services.createdDate = DateTime.Now;
            _context.AccessServices.Add(services);
            return _context.SaveChanges();
        }

        public ServiceResult InsertAccountPlanRules(AccountPlanRules model)
        {
            model.Id = Guid.NewGuid();
            model.createdDate = DateTime.Now;
            _context.AccountPlanRules.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult InsertAccountManagementStatuses(AccountManagementStatuses model)
        {
            model.Id = Guid.NewGuid();
            model.createdDate = DateTime.Now;
            _context.AccountManagementStatuses.Add(model);
            return _context.SaveChanges();
        }

        //HCare-1043
        public ServiceResult InsertAccountTextTemplates(AccountTextTemplates model)
        {
            model.Id = Guid.NewGuid();
            model.createdDate = DateTime.Now;
            _context.AccountTextTemplates.Add(model);
            return _context.SaveChanges();
        }

        //HCare-1043
        public ServiceResult InsertAccountEmailTemplates(AccountEmailTemplates model)
        {
            model.Id = Guid.NewGuid();
            model.createdDate = DateTime.Now;
            _context.AccountEmailTemplates.Add(model);
            return _context.SaveChanges();
        }


        //HCare-1083
        public List<ChronicMedication> GetChronicMedication()
        {
            return _context.ChronicMedication.OrderByDescending(x => x.description).OrderByDescending(x => x.Active == true).ToList();
        }
        public List<ChronicMedication> GetChronicMedicationValidation()
        {
            return _context.ChronicMedication.Where(x => x.Active == true).ToList();
        }
        public ChronicMedication GetChronicMedicationById(Guid id)
        {
            return _context.ChronicMedication.Where(x => x.Id == id).FirstOrDefault();
        }
        public ChronicMedication GetChronicMedicationByCode(string code)
        {
            return _context.ChronicMedication.Where(x => x.nappiStart.ToLower() == code.ToLower()).Where(x => x.Active == true).FirstOrDefault(); //hcare-1298
        }
        public ChronicMedication GetChronicMedicationByName(string description)
        {
            return _context.ChronicMedication.Where(x => x.description.ToLower().Replace(" ", "") == description.ToLower()).Where(x => x.Active == true).FirstOrDefault(); //hcare-1298
        }
        public ServiceResult InsertChronicMedication(ChronicMedication model)
        {
            model.Id = Guid.NewGuid();
            model.createdDate = DateTime.Now;
            _context.ChronicMedication.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateChronicMedication(ChronicMedication model)
        {
            var old = _context.ChronicMedication.Where(x => x.Id == model.Id).FirstOrDefault();
            old.nappiStart = model.nappiStart;
            old.description = model.description;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.Active = model.Active;
            return _context.SaveChanges();
        }
        //Hcare-1083 end


        //=========================================================================================================================================================================//
        //                                                                                     PMOC                                                                                // 
        //=========================================================================================================================================================================//

        public List<PreferredMethodOfContact> GetListofPMOC()
        {
            return _context.preferredMethodOfContacts.OrderByDescending(x => x.pmocDescription).OrderByDescending(x => x.active == true).ToList();
        }

        public List<PreferredMethodOfContact> GetPMOCValidation()
        {
            return _context.preferredMethodOfContacts.Where(x => x.active == true).ToList(); //hcare-1298
        }

        public PreferredMethodOfContact GetPMOCbyCode(string code)
        {
            return _context.preferredMethodOfContacts.Where(x => x.pmocCode.ToLower() == code.ToLower()).Where(x => x.active == true).FirstOrDefault(); //hcare-1298
        }

        public PreferredMethodOfContact GetPMOCbyName(string name)
        {
            return _context.preferredMethodOfContacts.Where(x => x.pmocDescription.ToLower() == name.ToLower()).Where(x => x.active == true).FirstOrDefault(); //hcare-1298
        }

        public ServiceResult InsertPMOC(PreferredMethodOfContact model)
        {
            _context.preferredMethodOfContacts.Add(model);
            return _context.SaveChanges();
        }

        public PreferredMethodOfContact GetPMOCById(int id)
        {
            return _context.preferredMethodOfContacts.Where(x => x.id == id).FirstOrDefault();
        }

        public ServiceResult UpdatePMOC(PreferredMethodOfContact model)
        {
            var old = _context.preferredMethodOfContacts.Where(x => x.id == model.id).FirstOrDefault();
            old.pmocDescription = model.pmocDescription;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.active = model.active;
            return _context.SaveChanges();
        }


        //=========================================================================================================================================================================//
        //                                                                                  Programs                                                                               // 
        //=========================================================================================================================================================================//


        public List<Programs> GetPrograms()
        {
            return _context.Program.OrderBy(x => x.ProgramName).ThenByDescending(x => x.Active == true).ToList();
        }
        public List<Programs> GetProgramValidation()
        {
            return _context.Program.Where(x => x.Active == true).ToList();
        }
        public Programs GetProgrambyName(string name)
        {
            return _context.Program.Where(x => x.ProgramName.ToLower() == name.ToLower()).Where(x => x.Active == true).FirstOrDefault(); //hcare-1298
        }

        public Programs GetProgrambyCode(string code)
        {
            return _context.Program.Where(x => x.code.ToLower() == code.ToLower()).Where(x => x.Active == true).FirstOrDefault(); //hcare-1298
        }

        public Programs GetProgramByICD10(string icd10code)
        {
            return _context.Program.Where(x => x.icd10code.ToLower() == icd10code.ToLower()).Where(x => x.Active == true).FirstOrDefault(); //hcare-1298
        }

        public ServiceResult InsertProgrm(Programs model)
        {
            _context.Program.Add(model);
            return _context.SaveChanges();
        }

        public Programs GetProgramById(Guid id)
        {
            return _context.Program.Where(x => x.programID == id).FirstOrDefault();
        }
        public Programs GetProgramByName(Guid id)
        {
            return _context.Program.Where(x => x.programID == id).FirstOrDefault();
        }

        public ServiceResult UpdateProgrm(Programs model)
        {
            var old = _context.Program.Where(x => x.programID == model.programID).FirstOrDefault();
            old.ProgramName = model.ProgramName;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.Active = model.Active;
            old.icd10code = model.icd10code;
            return _context.SaveChanges();
        }

        public ServiceResult UpdateClinicalRiskRules(AccountPlanRules model)
        {
            var old = _context.AccountPlanRules.Where(x => x.RuleId == model.RuleId).Where(x => x.AccountId == model.AccountId).Where(x => x.MedicalAidId == model.MedicalAidId).FirstOrDefault();
            old.Active = model.Active;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            return _context.SaveChanges();
        }

        public ServiceResult UpdateStatusRules(AccountManagementStatuses model)
        {
            var old = _context.AccountManagementStatuses.Where(x => x.Id == model.Id).FirstOrDefault();
            old.Active = model.Active;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            return _context.SaveChanges();
        }
        //Hcare-1043
        public ServiceResult UpdateTextTemplate(AccountTextTemplates model)
        {
            var old = _context.AccountTextTemplates.Where(x => x.Id == model.Id).FirstOrDefault();
            old.Active = model.Active;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            return _context.SaveChanges();
        }
        public ServiceResult UpdateMedicalAidPlanPrograms(MedicalAidPlanPrograms model)
        {
            var old = _context.MedicalAidPlanPrograms.Where(x => x.Id == model.Id).FirstOrDefault();
            old.Active = model.Active;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            return _context.SaveChanges();
        }

        //Hcare-1087
        public ServiceResult UpdateMedicalAidPrograms(MedicalAidPrograms model)
        {
            var old = _context.MedicalAidPrograms.Where(x => x.id == model.id).FirstOrDefault();
            old.Active = model.Active;
            old.referralFrom = model.referralFrom;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateUserSchemeAccess(UserSchemeAccess model)
        {
            var old = _context.UserSchemeAccess.Where(x => x.Id == model.Id).FirstOrDefault();
            old.Active = model.Active;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            return _context.SaveChanges();
        }

        public ServiceResult UpdateUserProgramAccess(UserProgramAccess model)
        {
            var old = _context.UserProgramAccess.Where(x => x.Id == model.Id).FirstOrDefault();
            old.Active = model.Active;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            return _context.SaveChanges();
        }

        #region HCare-1060 NON CDL Flags
        public List<NonCLDFlags> GetNonCDLFlags()
        {
            return _context.NonCLDFlags.OrderByDescending(x => x.flagCode).OrderByDescending(x => x.active == true).ToList();
        }

        public bool GetNonCDLFlag(NonCLDFlags model)
        {
            var result = _context.NonCLDFlags.Where(x => x.flagCode == model.flagCode)
                .Where(x => x.programCode == model.programCode)
                .Where(x => x.Id != model.Id)
                .Where(x => x.active == true) //hcare-1298
                .ToList().Count();

            if (result > 0) { return true; } else { return false; }

        }
        public List<NonCLDFlags> GetNonCDLFlagsList()
        {
            var results = _context.NonCLDFlags.OrderByDescending(x => x.flagCode).OrderByDescending(x => x.active == true).ToList();
            foreach (var result in results)
            {
                result.programCode = _context.Program.Where(x => x.code == result.programCode).Select(x => x.ProgramName).FirstOrDefault();
            }

            return results;
        }

        public NonCLDFlags GetNonCDLFlagsByName(string name)
        {
            return _context.NonCLDFlags.Where(x => x.flagCode.ToLower() == name.ToLower()).FirstOrDefault();
        }

        public NonCLDFlags GetNonCDLFlagsByCode(string code, string programcode)
        {
            return _context.NonCLDFlags.Where(x => x.flagCode.ToLower() == code.ToLower()).Where(x => x.programCode == programcode).Where(x => x.active == true).FirstOrDefault(); //hcare-1298
        }

        public ServiceResult InserNonCDLFlags(NonCLDFlags model)
        {
            _context.NonCLDFlags.Add(model);
            return _context.SaveChanges();
        }

        public NonCLDFlags GetNonCDLFlagsByID(int id)
        {
            return _context.NonCLDFlags.Where(x => x.Id == id).FirstOrDefault();
        }

        public ServiceResult UpdateNonCDLFlags(NonCLDFlags model)
        {
            var old = _context.NonCLDFlags.Where(x => x.Id == model.Id).FirstOrDefault();
            old.flagCode = model.flagCode;
            old.programCode = model.programCode;
            old.active = model.active;
            old.modifiedDate = model.modifiedDate;
            old.modifiedBy = model.modifiedBy;
            return _context.SaveChanges();
        }

        #endregion

        #region Disclaimer HCare-864
        public List<DisclaimerQuestions> GetDisclaimerQuestions()
        {
            return _context.DisclaimerQuestions.Where(x => x.Active == true).Where(x => x.Title.ToLower().Contains("q")).OrderBy(x => x.Title).ToList();

        }

        public List<DisclaimerQuestions> GetAcknowledgementQuestions()
        {
            return _context.DisclaimerQuestions.Where(x => x.Active == true).Where(x => x.Title.ToLower().Contains("a")).OrderBy(x => x.Title).ToList();

        }

        public ServiceResult InsertDisclaimerQ(DisclaimerQuestions model)
        {
            _context.DisclaimerQuestions.Add(model);
            return _context.SaveChanges();
        }

        public DisclaimerQuestions GetDisclaimerQById(int id)
        {
            return _context.DisclaimerQuestions.Where(x => x.Id == id).FirstOrDefault();
        }

        public ServiceResult UpdateDisclaimerQ(DisclaimerQuestions model)
        {
            var old = _context.DisclaimerQuestions.Where(x => x.Id == model.Id).FirstOrDefault();
            old.QuestionNumber = model.QuestionNumber;
            old.Title = model.Title;
            old.EnglishQuestion = model.EnglishQuestion;
            old.AfrikaansQuestion = model.AfrikaansQuestion;
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.Active = model.Active;
            return _context.SaveChanges();
        }

        public ServiceResult InsertDisclaimerResponse(DisclaimerResponse model)
        {
            _context.DisclaimerResponse.Add(model);

            return _context.SaveChanges();
        }

        public List<DisclaimerResponse> GetDisclaimerResponse(Guid dependentid)
        {
            return _context.DisclaimerResponse.Where(x => x.DependantID == dependentid).Where(x => x.Active == true).ToList();

        }
        public List<DisclaimerFullView> GetDisclaimerResults(Guid dependentid, Guid programcode)
        {
            var listcount = GetDisclaimerQuestions().Count() + 1;
            var programCode = _context.Program.Where(x => x.programID == programcode).Select(x => x.code).FirstOrDefault();
            var results = (from dr in _context.DisclaimerResponse
                           join dq in _context.DisclaimerQuestions on dr.Question equals dq.Title
                           where dr.DependantID == dependentid
                           where dr.Program == programCode
                           where dr.Active == true

                           select new DisclaimerFullView()
                           {
                               DependantID = dr.DependantID,
                               Id = dr.Id,
                               TaskID = dr.TaskID,
                               AssignmentID = dr.AssignmentID,
                               QuestionNumber = dr.Question,
                               EnglishQuestion = dq.EnglishQuestion,
                               AfrikaansQuestion = dq.AfrikaansQuestion,
                               Yes = dr.Yes,
                               No = dr.No,
                               Comment = dr.Comment,
                               CreatedBy = dr.CreatedBy,
                               CreatedDate = dr.CreatedDate,
                               Program = dr.Program,
                               Active = dr.Active
                           }).OrderByDescending(x => x.CreatedDate).Take(listcount).ToList();

            var newresult = results.OrderBy(x => x.QuestionNumber).ToList();


            return newresult;

        }

        #endregion

        #region dsm5 HCare-1123
        public ServiceResult InsertDSM5Results(MH_DSM5Response model)
        {
            _context.MH_DSM5Responses.Add(model);
            return _context.SaveChanges();
        }

        public MH_DSM5Response GetDSM5ById(int id)
        {
            return _context.MH_DSM5Responses.Where(x => x.Id == id).FirstOrDefault();
        }

        public ServiceResult UpdateDSM5Result(MH_DSM5Response model)
        {
            var old = _context.MH_DSM5Responses.Where(x => x.Id == model.Id).FirstOrDefault();

            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.SubstanceAbuse = model.SubstanceAbuse;
            old.Depression = model.Depression;
            old.DepressionSC = model.DepressionSC;
            old.Interest = model.Interest;
            old.InterestSC = model.InterestSC;
            old.WeightLoss = model.WeightLoss;
            old.WeightLossSC = model.WeightLossSC;
            old.Psychomotor = model.Psychomotor;
            old.PsychomotorSC = model.PsychomotorSC;
            old.Tiredness = model.Tiredness;
            old.TirednessSC = model.TirednessSC;
            old.SelfWorth = model.SelfWorth;
            old.SelfWorthSC = model.SelfWorthSC;
            old.Concentration = model.Concentration;
            old.ConcentrationSC = model.ConcentrationSC;
            old.Suicidal = model.Suicidal;
            old.SuicidalSC = model.SuicidalSC;
            old.TotalScore = model.TotalScore;
            old.Outcome = model.Outcome;
            old.FollowUp = model.FollowUp;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        public List<MH_DSM5Response> GetDSM5Results(Guid dependentid)
        {
            return _context.MH_DSM5Responses.Where(x => x.DependantID == dependentid).Where(x => x.Active == true).ToList();

        }
        #endregion

        #region schizophrenia HCare-1124
        public ServiceResult InsertSchizophreniaResults(MH_SchizophreniaResponse model)
        {
            _context.MH_SchizophreniaResponses.Add(model);
            return _context.SaveChanges();
        }

        public MH_SchizophreniaResponse GetSchizophreniaById(int id)
        {
            return _context.MH_SchizophreniaResponses.Where(x => x.Id == id).FirstOrDefault();
        }

        public ServiceResult UpdateSchizophreniaResult(MH_SchizophreniaResponse model)
        {
            var old = _context.MH_SchizophreniaResponses.Where(x => x.Id == model.Id).FirstOrDefault();

            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.Depression = model.Depression;
            old.DepressionSC = model.DepressionSC;
            old.Hopelessness = model.Hopelessness;
            old.HopelessnessSC = model.HopelessnessSC;
            old.SelfDepreciation = model.SelfDepreciation;
            old.SelfDepreciationSC = model.SelfDepreciationSC;
            old.GuiltyIdeas = model.GuiltyIdeas;
            old.GuiltyIdeasSC = model.GuiltyIdeasSC;
            old.PathologicalGuilt = model.PathologicalGuilt;
            old.PathologicalGuiltSC = model.PathologicalGuiltSC;
            old.MorningDepression = model.MorningDepression;
            old.MorningDepressionSC = model.MorningDepressionSC;
            old.EarlyWakening = model.EarlyWakening;
            old.EarlyWakeningSC = model.EarlyWakeningSC;
            old.Suicidal = model.Suicidal;
            old.SuicidalSC = model.SuicidalSC;
            old.SuicidalCMT = model.SuicidalCMT;
            old.ObservedDepression = model.ObservedDepression;
            old.ObservedDepressionSC = model.ObservedDepressionSC;
            old.Outcome = model.Outcome;
            old.TotalScore = model.TotalScore;
            old.Comment = model.Comment;
            old.FollowUp = model.FollowUp;
            old.Active = model.Active;
            return _context.SaveChanges();
        }

        public List<MH_SchizophreniaResponse> GetSchizophreniaResults(Guid dependentid)
        {
            return _context.MH_SchizophreniaResponses.Where(x => x.DependantID == dependentid).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).ToList();

        }

        #endregion

        #region bipolar HCare-1125
        public ServiceResult InsertBipolarResults(MH_BipolarResponse model)
        {
            _context.MH_BipolarResponses.Add(model);
            return _context.SaveChanges();
        }

        public MH_BipolarResponse GetBipolarById(int id)
        {
            return _context.MH_BipolarResponses.Where(x => x.Id == id).FirstOrDefault();
        }

        public ServiceResult UpdateBipolarResult(MH_BipolarResponse model)
        {
            var old = _context.MH_BipolarResponses.Where(x => x.Id == model.Id).FirstOrDefault();

            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.Depression = model.Depression;
            old.DepressionSC = model.DepressionSC;
            old.Insomnia = model.Insomnia;
            old.InsomniaSC = model.InsomniaSC;
            old.Appetite = model.Appetite;
            old.AppetiteSC = model.AppetiteSC;
            old.SocialEngagement = model.SocialEngagement;
            old.SocialEngagementSC = model.SocialEngagementSC;
            old.Activity = model.Activity;
            old.ActivitySC = model.ActivitySC;
            old.Motivation = model.Motivation;
            old.MotivationSC = model.MotivationSC;
            old.Concentration = model.Concentration;
            old.ConcentrationSC = model.ConcentrationSC;
            old.Anxiety = model.Anxiety;
            old.AnxietySC = model.AnxietySC;
            old.Pleasure = model.Pleasure;
            old.PleasureSC = model.PleasureSC;
            old.Emotion = model.Emotion;
            old.EmotionSC = model.EmotionSC;
            old.Worthlessness = model.Worthlessness;
            old.WorthlessnessSC = model.WorthlessnessSC;
            old.Helplessness = model.Helplessness;
            old.HelplessnessSC = model.HelplessnessSC;
            old.Suicidal = model.Suicidal;
            old.SuicidalSC = model.SuicidalSC;
            old.SuicidalCMT = model.SuicidalCMT;
            old.Guilt = model.Guilt;
            old.GuiltSC = model.GuiltSC;
            old.Psychotic = model.Psychotic;
            old.PsychoticSC = model.PsychoticSC;
            old.Irritability = model.Irritability;
            old.IrritabilitySC = model.IrritabilitySC;
            old.Lability = model.Lability;
            old.LabilitySC = model.LabilitySC;
            old.IncMotorDrive = model.IncMotorDrive;
            old.IncMotorDriveSC = model.IncMotorDriveSC;
            old.IncSpeech = model.IncSpeech;
            old.IncSpeechSC = model.IncSpeechSC;
            old.Agitation = model.Agitation;
            old.AgitationSC = model.AgitationSC;
            old.Outcome = model.Outcome;
            old.TotalScore = model.TotalScore;
            old.Comment = model.Comment;
            old.FollowUp = model.FollowUp;
            old.Active = model.Active;
            return _context.SaveChanges();
        }

        public List<MH_BipolarResponse> GetBipolarResults(Guid dependentid)
        {
            return _context.MH_BipolarResponses.Where(x => x.DependantID == dependentid).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).ToList();

        }

        #endregion

        #region depression HCare-1126
        public ServiceResult InsertDepressionResults(MH_DepressionResponse model)
        {
            _context.MH_DepressionResponses.Add(model);
            return _context.SaveChanges();
        }

        public MH_DepressionResponse GetDepressionById(int id)
        {
            return _context.MH_DepressionResponses.Where(x => x.Id == id).FirstOrDefault();
        }

        public ServiceResult UpdateDepressionResult(MH_DepressionResponse model)
        {
            var old = _context.MH_DepressionResponses.Where(x => x.Id == model.Id).FirstOrDefault();

            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.Depression = model.Depression;
            old.DepressionSC = model.DepressionSC;
            old.Guilt = model.Guilt;
            old.GuiltSC = model.GuiltSC;
            old.Depression = model.Depression;
            old.DepressionSC = model.DepressionSC;
            old.Suicidal = model.Suicidal;
            old.SuicidalSC = model.SuicidalSC;
            old.Depression = model.Depression;
            old.DepressionSC = model.DepressionSC;
            old.InsomniaEarlyNight = model.InsomniaEarlyNight;
            old.InsomniaEarlyNightSC = model.InsomniaEarlyNightSC;
            old.InsomniaMiddleNight = model.InsomniaMiddleNight;
            old.InsomniaMiddleNightSC = model.InsomniaMiddleNightSC;
            old.InsomniaEarlyMorning = model.InsomniaEarlyMorning;
            old.InsomniaEarlyMorningSC = model.InsomniaEarlyMorningSC;
            old.Activity = model.Activity;
            old.ActivitySC = model.ActivitySC;
            old.Psychomotor = model.Psychomotor;
            old.PsychomotorSC = model.PsychomotorSC;
            old.Agitation = model.Agitation;
            old.AgitationSC = model.AgitationSC;
            old.AnxietyPsychic = model.AnxietyPsychic;
            old.AnxietyPsychicSC = model.AnxietyPsychicSC;
            old.AnxietySomatic = model.AnxietySomatic;
            old.AnxietySomaticSC = model.AnxietySomaticSC;
            old.SomaticSymGastro = model.SomaticSymGastro;
            old.SomaticSymGastroSC = model.SomaticSymGastroSC;
            old.SomaticSymGeneral = model.SomaticSymGeneral;
            old.SomaticSymGeneralSC = model.SomaticSymGeneralSC;
            old.Sexology = model.Sexology;
            old.SexologySC = model.SexologySC;
            old.Hypochondriasis = model.Hypochondriasis;
            old.HypochondriasisSC = model.HypochondriasisSC;
            old.WeightLoss = model.WeightLoss;
            old.WeightLossSC = model.WeightLossSC;
            old.Insight = model.Insight;
            old.InsightSC = model.InsightSC;
            old.Outcome = model.Outcome;
            old.TotalScore = model.TotalScore;
            old.Comment = model.Comment;
            old.FollowUp = model.FollowUp;
            old.Active = model.Active;
            return _context.SaveChanges();
        }

        public List<MH_DepressionResponse> GetDepressionResults(Guid dependentid)
        {
            return _context.MH_DepressionResponses.Where(x => x.DependantID == dependentid).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).ToList();

        }

        #endregion

        #region attachment-types HCare-1154
        public ServiceResult InsertAttachmentType(AttachmentTypes model)
        {
            _context.AttachmentTypes.Add(model);
            return _context.SaveChanges();
        }

        public AttachmentTypes GetAttachmentTypeById(string id)
        {
            return _context.AttachmentTypes.Where(x => x.attachmentType == id).FirstOrDefault();
        }

        public AttachmentTypes GetAttachmentTypeByCode(string code)
        {
            return _context.AttachmentTypes.Where(x => x.attachmentType == code).FirstOrDefault();
        }

        public AttachmentTypes GetAttachmentTypeByName(string name)
        {
            return _context.AttachmentTypes.Where(x => x.typeDescription == name).Where(x => x.active == true).FirstOrDefault(); //hcare-1298
        }


        public ServiceResult UpdateAttachmentType(AttachmentTypes model)
        {
            var old = _context.AttachmentTypes.Where(x => x.attachmentType == model.attachmentType).FirstOrDefault();

            old.modifiedDate = model.modifiedDate;
            old.modifiedBy = model.modifiedBy;
            old.typeDescription = model.typeDescription;
            old.assignmentItemType = model.assignmentItemType;
            old.active = model.active;

            return _context.SaveChanges();
        }

        public List<AttachmentTypes> GetAttachmentTypes()
        {
            return _context.AttachmentTypes.OrderByDescending(x => x.active).ThenBy(x => x.typeDescription).ToList();

        }

        public List<AttachmentTypes> GetAttachmentTypeValidation()
        {
            return _context.AttachmentTypes.Where(x => x.active == true).ToList();
        }
        #endregion

        #region new dsm5 HCare-1205
        public ServiceResult InsertNEWDSM5Results(MH_DSM5ResponseNEW model)
        {
            _context.MH_DSM5ResponsesNEW.Add(model);
            return _context.SaveChanges();
        }

        public MH_DSM5ResponseNEW GetNEWDSM5ById(int id)
        {
            return _context.MH_DSM5ResponsesNEW.Where(x => x.Id == id).FirstOrDefault();
        }

        public ServiceResult UpdateNEWDSM5Result(MH_DSM5ResponseNEW model)
        {
            var old = _context.MH_DSM5ResponsesNEW.Where(x => x.Id == model.Id).FirstOrDefault();

            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.DepressionOne = model.DepressionOne;
            old.DepressionOneSC = model.DepressionOneSC;
            old.DepressionTwo = model.DepressionTwo;
            old.DepressionTwoSC = model.DepressionTwoSC;
            old.AngerOne = model.AngerOne;
            old.AngerOneSC = model.AngerOneSC;
            old.ManiaOne = model.ManiaOne;
            old.ManiaOneSC = model.ManiaOneSC;
            old.ManiaTwo = model.ManiaTwo;
            old.ManiaTwoSC = model.ManiaTwoSC;
            old.AnxietyOne = model.AnxietyOne;
            old.AnxietyOneSC = model.AnxietyOneSC;
            old.AnxietyTwo = model.AnxietyTwo;
            old.AnxietyTwoSC = model.AnxietyTwoSC;
            old.AnxietyThree = model.AnxietyThree;
            old.AnxietyThreeSC = model.AnxietyThreeSC;
            old.SomaticOne = model.SomaticOne;
            old.SomaticOneSC = model.SomaticOneSC;
            old.SomaticTwo = model.SomaticTwo;
            old.SomaticTwoSC = model.SomaticTwoSC;
            old.SuicidalOne = model.SuicidalOne;
            old.SuicidalOneSC = model.SuicidalOneSC;
            old.PsychosisOne = model.PsychosisOne;
            old.PsychosisOneSC = model.PsychosisOneSC;
            old.PsychosisTwo = model.PsychosisTwo;
            old.PsychosisTwoSC = model.PsychosisTwoSC;
            old.SleepOne = model.SleepOne;
            old.SleepOneSC = model.SleepOneSC;
            old.MemoryOne = model.MemoryOne;
            old.MemoryOneSC = model.MemoryOneSC;
            old.BehaviourOne = model.BehaviourOne;
            old.BehaviourOneSC = model.BehaviourOneSC;
            old.BehaviourTwo = model.BehaviourTwo;
            old.BehaviourTwoSC = model.BehaviourTwoSC;
            old.DissociationOne = model.DissociationOne;
            old.DissociationOneSC = model.DissociationOneSC;
            old.PersonalityOne = model.PersonalityOne;
            old.PersonalityOneSC = model.PersonalityOneSC;
            old.PersonalityTwo = model.PersonalityTwo;
            old.PersonalityTwoSC = model.PersonalityTwoSC;
            old.SubstanceAbuse = model.SubstanceAbuse;
            old.TotalScore = model.TotalScore;
            old.Outcome = model.Outcome;
            old.FollowUp = model.FollowUp;
            old.RiskID = model.RiskID;
            old.DSM5ScoreID = model.DSM5ScoreID;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        public List<MH_DSM5ResponseNEW> GetNEWDSM5Results(Guid dependentid)
        {
            var results = _context.MH_DSM5ResponsesNEW.Where(x => x.DependantID == dependentid).Where(x => x.Active == true).OrderByDescending(x => x.CreatedDate).ToList();
            return results;
        }
        #endregion

        #region dsm5-score HCare-1206
        public List<MH_DSM5Score> GetDSM5ScoreHistory(Guid dependentid)
        {
            return _context.MH_DSM5Scores.Where(x => x.DependantID == dependentid).Where(x => x.Active == true).OrderByDescending(x => x.EffectiveDate).ToList();
        }

        public ServiceResult InsertDSM5Score(MH_DSM5Score model)
        {
            _context.MH_DSM5Scores.Add(model);
            return _context.SaveChanges();
        }

        public MH_DSM5Score GetDSM5ScoreByID(int id)
        {
            return _context.MH_DSM5Scores.Where(x => x.Id == id).FirstOrDefault();
        }
        public DSM5ScoreVM GetDSM5ScoreInformation(int id)
        {
            string sql = string.Format(@"SELECT ds.Id, ds.DependantID, ds.AssignmentID, ds.TaskID, ds.Score, ds.Reason, ds.Comment, ds.EffectiveDate, ds.CreatedDate, ds.CreatedBy, ds.ModifiedDate, ds.ModifiedBy,
                                        ds.Program, ds.Active, ds.RiskID, sh.CreatedDate, sh.CreatedBy, sh.OldScore, sh.OldReason, sh.ModifiedDate, sh.ModifiedBy, sh.NewScore, sh.NewReason, sh.Comment  
                                        FROM MH_DSM5Score ds
                                        LEFT OUTER JOIN MH_DSM5ScoreHistory sh on ds.RiskID = sh.Id
                                        WHERE ds.Id = '{0}'", id);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<DSM5ScoreVM>)db.Query<DSM5ScoreVM>(sql, null, commandTimeout: 500).ToList();
                db.Close();

                return results.FirstOrDefault();
            }
        }

        public List<DSM5ScoreVM> GetDSM5ScoreDetails(int id)
        {
            string sql = string.Format(@"SELECT sh.CreatedDate, sh.CreatedBy, sh.OldScore, sh.OldReason, sh.ModifiedDate, sh.ModifiedBy, sh.NewScore, sh.NewReason, sh.Comment  
                                        FROM MH_DSM5Score ds
                                        INNER JOIN MH_DSM5ScoreHistory sh on ds.Id = sh.RiskID
                                        WHERE ds.Id = '{0}'", id);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<DSM5ScoreVM>)db.Query<DSM5ScoreVM>(sql, null, commandTimeout: 500).ToList();
                db.Close();

                return results.OrderByDescending(x => x.ModifiedDate).ToList();
            }
        }


        public ServiceResult UpdateDSM5Score(MH_DSM5Score model)
        {
            var old = _context.MH_DSM5Scores.Where(x => x.Id == model.Id).FirstOrDefault();

            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.Comment = model.Comment;
            old.Score = model.Score;
            old.Reason = model.Reason;
            old.Program = model.Program;
            old.RiskID = model.RiskID;
            old.EffectiveDate = model.EffectiveDate;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        public ServiceResult InsertDSM5ScoreHistory(MH_DSM5ScoreHistory model)
        {
            _context.MH_DSM5ScoreHistories.Add(model);
            return _context.SaveChanges();
        }

        #endregion
        public SmsMessageTemplates GetTextTemplateByTitle(string title) //HCare-956
        {
            return _context.SmsMessageTemplates.Where(x => x.title == title).Where(x => x.Active == true).FirstOrDefault(); //hcare-1298
        }

        public SmsMessageTemplates GetTextTemplateByMessage(string template) //HCare-956
        {
            return _context.SmsMessageTemplates.Where(x => x.template == template).Where(x => x.Active == true).FirstOrDefault(); //hcare-1298
        }

        public EmailTemplates GetEmailTemplateByTitle(string title) //HCare-956
        {
            return _context.EmailTemplates.Where(x => x.title == title).FirstOrDefault();
        }

        public AssignmentTypes GetAssignmentTypeByCode(string code) //HCare-956
        {
            return _context.AssignmentTypes.Where(x => x.assignmentType == code).FirstOrDefault();
        }

        public AssignmentTypes GetAssignmentTypeByName(string description) //HCare-956
        {
            return _context.AssignmentTypes.Where(x => x.assignmentDescription == description).Where(x => x.active == true).FirstOrDefault(); //hcare-1298
        }

        public AssignmentItemTypes GetAssignmentItemTypeByCode(string assignmentItemType) //HCare-956
        {
            return _context.AssignmentItemTypes.Where(x => x.assignmentItemType == assignmentItemType).FirstOrDefault();
        }

        public AssignmentItemTypes GetAssignmentItemTypeByName(string itemDescription) //HCare-956
        {
            return _context.AssignmentItemTypes.Where(x => x.itemDescription == itemDescription).Where(x => x.active == true).FirstOrDefault(); //hcare-1298
        }
        public TaskTypeRequirements GetTaskRequirementByName(string requirementText) //HCare-956
        {
            return _context.TaskTypeRequirements.Where(x => x.requirementText == requirementText).Where(x => x.active == true).FirstOrDefault(); //hcare-1298
        }

        #region hcare-1134
        //public List<WorkflowSettings> GetWorkFlowSettings()
        //{
        //	var results = _context.WorkflowSettings.OrderByDescending(x => x.UserID).ThenByDescending(x => x.Assignment).ThenByDescending(x => x.Active == true).ToList();
        //	return results;
        //}

        public List<wfSettings> GetWFSettings()
        {
            return _context.wfSettings.Where(x => x.Active == true).GroupBy(p => p.QueueID).Select(q => q.FirstOrDefault()).ToList(); //hcare-1298

        }

        public List<wfUserQueue> GetWFUserQueueList()
        {
            return _context.wfUserQueues.Where(x => x.Active == true).ToList();
        }

        public wfUserQueue GetWFUserQueueByQueueID(Guid queueID)
        {
            return _context.wfUserQueues.Where(x => x.Active == true).Where(x => x.QueueID == queueID).FirstOrDefault();
        }

        //public List<WorkflowVM> GetWorkFlowList() //HCare-1134
        //{
        //    var results = (from wf in _context.WorkflowSettings
        //                   join u in _context.Users on wf.UserID equals u.userID
        //                   join ma in _context.MedicalAids on wf.MedicalAidID equals ma.MedicalAidID
        //                   join pr in _context.Program on wf.ProgramID equals pr.programID
        //                   join ms in _context.ManagementStatus on wf.ManagementStatus equals ms.statusCode
        //                   join rr in _context.RiskRatingTypes on wf.RiskRating equals rr.RiskType into rrt from rr in rrt.DefaultIfEmpty()
        //                   join at in _context.AssignmentItemTypes on wf.Assignment equals at.assignmentItemType into att from at in att.DefaultIfEmpty()

        //                   select new WorkflowVM
        //                   {
        //                       Id = wf.Id,
        //                       UserFirstName = u.Firstname,
        //                       UserLastName = u.Lastname,
        //                       MedicalScheme = ma.Name,
        //                       Program = pr.ProgramName,
        //                       ManagementStatus = ms.statusName,
        //                       FromDate = wf.FromDate,
        //                       ToDate = wf.ToDate,
        //                       RiskRating = rr.RiskName,
        //                       AssignmentItemType = at.itemDescription,
        //                       Active = wf.Active,

        //                   }).OrderByDescending(x => x.Active).ThenBy(x => x.UserFirstName).ToList();

        //    return results;
        //}

        //public List<WorkflowVM> GetWorkFlowList() //HCare-1134
        //{
        //    string sql = string.Format(@"SELECT wfs.Id[Id], u.Firstname[UserFirstName], u.Lastname[UserLastName], ma.Name[MedicalScheme], p.ProgramName[Program], ms.statusName[ManagementStatus], wfs.FromDate[FromDate], wfs.ToDate[ToDate],
        //                                wfs.RiskRating[RiskRating], ait.itemDescription[AssignmentItemType], wfs.Active[Active]
        //                                FROM WorkflowSettings wfs
        //                                INNER JOIN Users u ON wfs.UserID = u.userID
        //                                INNER JOIN MedicalAid ma ON wfs.MedicalAidID = ma.MedicalAidID
        //                                INNER JOIN Programs p ON wfs.ProgramID = p.programID
        //                                INNER JOIN ManagementStatus ms ON wfs.ManagementStatus = ms.statusCode
        //                                LEFT OUTER JOIN RiskRatingTypes rr ON wfs.RiskRating = rr.RiskType
        //                                LEFT OUTER JOIN AssignmentItemTypes ait ON wfs.Assignment = ait.assignmentItemType");

        //    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
        //    {
        //        db.Open();
        //        var results = (List<WorkflowVM>)db.Query<WorkflowVM>(sql, null, commandTimeout: 500).ToList();
        //        db.Close();

        //        foreach (var result in results)
        //        {
        //            var riskrating = result.RiskRating;

        //            if (result.RiskRating.Contains(','))
        //            {
        //                result.RiskRating = "Multiple";
        //            }
        //            else
        //            {
        //                result.RiskRating = _context.RiskRatingTypes.Where(x => x.RiskType == result.RiskRating).Select(x => x.RiskName).FirstOrDefault();
        //            }
        //        }

        //        return results.OrderByDescending(x => x.Active).ThenBy(x => x.UserFirstName).ToList();
        //    }
        //}


        public List<WorkflowVM> GetWorkFlowList() //HCare-1134
        {
            string sql = string.Format(@"SELECT wfs.QueueID, ma.Name[MedicalScheme], p.ProgramName[Program], ms.statusName[ManagementStatus], wfs.FromDate[FromDate], wfs.ToDate[ToDate],
                                        wfs.RiskRating[RiskRating], ait.itemDescription[AssignmentItemType], wfs.Active[Active]
                                        FROM wfSettings wfs
                                        INNER JOIN MedicalAid ma ON wfs.MedicalAidID = ma.MedicalAidID
                                        INNER JOIN Programs p ON wfs.ProgramID = p.programID
                                        INNER JOIN ManagementStatus ms ON wfs.ManagementStatus = ms.statusCode
                                        LEFT OUTER JOIN RiskRatingTypes rr ON wfs.RiskRating = rr.RiskType
                                        LEFT OUTER JOIN AssignmentItemTypes ait ON wfs.Assignment = ait.assignmentItemType");

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<WorkflowVM>)db.Query<WorkflowVM>(sql, null, commandTimeout: 500).ToList();
                db.Close();

                foreach (var result in results)
                {
                    var riskrating = result.RiskRating;

                    if (result.RiskRating.Contains(','))
                    {
                        result.RiskRating = "Multiple";
                    }

                }

                return results.OrderByDescending(x => x.Active).ThenBy(x => x.UserFirstName).ToList();
            }
        }

        public List<wfQueueVM> GetWFUserQueue() //HCare-1134
        {
            string sql = string.Format(@"SELECT DISTINCT wfq.Id[Id], wfs.QueueName, u.Firstname + ' ' + u.Lastname[UserName], wfs.CreatedBy, wfs.CreatedDate, wfq.Active
                                        FROM wfUserQueue wfq
                                        INNER JOIN wfSettings wfs ON wfq.QueueID = wfs.QueueID
                                        INNER JOIN Users u on wfq.UserID = u.userID");

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<wfQueueVM>)db.Query<wfQueueVM>(sql, null, commandTimeout: 500).ToList();
                db.Close();

                return results.OrderByDescending(x => x.Active).ThenBy(x => x.UserName).ThenBy(x => x.QueueName).ToList();
            }
        }

        public ServiceResult InsertWorkflowSetting(WorkflowSettings model)
        {
            _context.WorkflowSettings.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateWorkflowSetting(WorkflowSettings model)
        {
            var old = _context.WorkflowSettings.Where(x => x.Id == model.Id).FirstOrDefault();

            old.UserID = model.UserID;
            old.MedicalAidID = model.MedicalAidID;
            old.ProgramID = model.ProgramID;
            old.ManagementStatus = model.ManagementStatus;
            old.FromDate = model.FromDate;
            old.ToDate = model.ToDate;
            old.RiskRating = model.RiskRating;
            old.Assignment = model.Assignment;
            old.Instruction = model.Instruction;
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.Active = model.Active;

            return _context.SaveChanges();
        }
        public WorkflowSettings GetWorkflowSettingById(int id)
        {
            return _context.WorkflowSettings.Where(x => x.Id == id).FirstOrDefault();
        }

        public wfViewModel GetWorkflowSettingsById(Guid id)
        {
            string sql = string.Format(@"SELECT wfs.QueueID[QueueID],wfs.QueueName[QueueName], wfs.QueueName[QueueName], ma.Name[MedicalScheme], ma.MedicalAidID[MedicalAidID], p.ProgramName[Program], p.programID[ProgramID], wfs.ManagementStatus[ManagementStatus], --ms.statusName[ManagementStatus], 
										wfs.FromDate[FromDate], wfs.ToDate[ToDate], wfs.RiskRating[RiskRating], wfs.Assignment[AssignmentItemType], wfs.Instruction[Instruction], 
										wfs.CreatedBy[CreatedBy], wfs.CreatedDate[CreatedDate], wfs.ModifiedBy[ModifiedBy], wfs.ModifiedDate[ModifiedDate], wfs.Active[Active], wfs.PathologyField[PathologyField], wfs.Less[Less], wfs.Greater[Greater]
                                        FROM wfSettings wfs
                                        INNER JOIN MedicalAid ma ON wfs.MedicalAidID = ma.MedicalAidID
                                        INNER JOIN Programs p ON wfs.ProgramID = p.programID
                                        LEFT OUTER JOIN ManagementStatus ms ON wfs.ManagementStatus = ms.statusCode
                                        LEFT OUTER JOIN RiskRatingTypes rr ON wfs.RiskRating = rr.RiskType
                                        LEFT OUTER JOIN AssignmentItemTypes ait ON wfs.Assignment = ait.assignmentItemType
										WHERE wfs.QueueID = '{0}'", id);


            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<wfViewModel>)db.Query<wfViewModel>(sql, null, commandTimeout: 500).ToList();
                db.Close();

                foreach (var result in results)
                {
                    if (result.ManagementStatus != null)
                    {
                        string[] mstatus = result.ManagementStatus.Split(',');
                        string concatenation = string.Empty;
                        foreach (string s in mstatus)
                        {
                            concatenation += _context.ManagementStatus.Where(x => x.statusCode == s).Select(x => x.statusCode).FirstOrDefault() + ",";
                        }

                        result.ManagementStatus = concatenation.TrimEnd(',');
                    }

                    if (result.AssignmentItemType != null)
                    {
                        string[] assignment = result.AssignmentItemType.Split(',');
                        string concatenation = string.Empty;
                        foreach (string s in assignment)
                        {
                            concatenation += _context.AssignmentItemTypes.Where(x => x.assignmentItemType == s).Select(x => x.assignmentItemType).FirstOrDefault() + ",";
                        }

                        result.AssignmentItemType = concatenation.TrimEnd(',');
                    }
                }

                return results.FirstOrDefault();
            }
        }

        public wfViewModel GetWorkflowSettingDetails(Guid id)
        {
            string sql = string.Format(@"SELECT wfs.QueueID[QueueID],wfs.QueueName[QueueName], wfs.QueueName[QueueName], ma.Name[MedicalScheme], ma.MedicalAidID[MedicalAidID], p.ProgramName[Program], p.programID[ProgramID], wfs.ManagementStatus[ManagementStatus], --ms.statusName[ManagementStatus], 
										wfs.FromDate[FromDate], wfs.ToDate[ToDate], wfs.RiskRating[RiskRating], wfs.Assignment[AssignmentItemType], wfs.Instruction[Instruction], 
										wfs.CreatedBy[CreatedBy], wfs.CreatedDate[CreatedDate], wfs.ModifiedBy[ModifiedBy], wfs.ModifiedDate[ModifiedDate], wfs.Active[Active], wfs.PathologyField[PathologyField], wfs.Less[Less], wfs.Greater[Greater]
                                        FROM wfSettings wfs
                                        INNER JOIN MedicalAid ma ON wfs.MedicalAidID = ma.MedicalAidID
                                        INNER JOIN Programs p ON wfs.ProgramID = p.programID
                                        LEFT OUTER JOIN ManagementStatus ms ON wfs.ManagementStatus = ms.statusCode
                                        LEFT OUTER JOIN RiskRatingTypes rr ON wfs.RiskRating = rr.RiskType
                                        LEFT OUTER JOIN AssignmentItemTypes ait ON wfs.Assignment = ait.assignmentItemType
										WHERE wfs.QueueID = '{0}'", id);


            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<wfViewModel>)db.Query<wfViewModel>(sql, null, commandTimeout: 500).ToList();
                db.Close();

                foreach (var result in results)
                {
                    if (result.ManagementStatus != null)
                    {
                        string[] mstatus = result.ManagementStatus.Split(',');
                        string concatenation = string.Empty;
                        foreach (string s in mstatus)
                        {
                            concatenation += _context.ManagementStatus.Where(x => x.statusCode == s).Select(x => x.statusName).FirstOrDefault() + ",";
                        }

                        result.ManagementStatus = concatenation.TrimEnd(',');
                    }
                }

                return results.FirstOrDefault();
            }
        }

        public List<wfViewModel> GetWorkflowSettingsByQueue(Guid id)
        {
            string sql = string.Format(@"SELECT wfs.QueueID[QueueID],wfs.QueueName[QueueName], wfs.QueueName[QueueName], ma.Name[MedicalScheme], ma.MedicalAidID[MedicalAidID], p.ProgramName[Program], p.programID[ProgramID], ms.statusName[ManagementStatus], 
										wfs.FromDate[FromDate], wfs.ToDate[ToDate], wfs.RiskRating[RiskRating], wfs.Assignment[AssignmentItemType], wfs.Instruction[Instruction], 
										wfs.CreatedBy[CreatedBy], wfs.CreatedDate[CreatedDate], wfs.ModifiedBy[ModifiedBy], wfs.ModifiedDate[ModifiedDate], wfs.Active[Active], wfs.PathologyField[PathologyField], wfs.Less[Less], wfs.Greater[Greater]

                                        FROM wfSettings wfs
                                        INNER JOIN MedicalAid ma ON wfs.MedicalAidID = ma.MedicalAidID
                                        INNER JOIN Programs p ON wfs.ProgramID = p.programID
                                        LEFT OUTER JOIN ManagementStatus ms ON wfs.ManagementStatus = ms.statusCode
                                        LEFT OUTER JOIN RiskRatingTypes rr ON wfs.RiskRating = rr.RiskType
                                        LEFT OUTER JOIN AssignmentItemTypes ait ON wfs.Assignment = ait.assignmentItemType
										WHERE wfs.QueueID = '{0}'", id);


            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<wfViewModel>)db.Query<wfViewModel>(sql, null, commandTimeout: 500).ToList();
                db.Close();

                return results;
            }
        }
        public WorkflowSettingsVM GetWFSettingsVM(int id)
        {
            var results = (from wf in _context.WorkflowSettings
                           join u in _context.Users on wf.UserID equals u.userID
                           join ma in _context.MedicalAids on wf.MedicalAidID equals ma.MedicalAidID
                           join pr in _context.Program on wf.ProgramID equals pr.programID
                           join ms in _context.ManagementStatus on wf.ManagementStatus equals ms.statusCode
                           join rr in _context.RiskRatingTypes on wf.RiskRating equals rr.RiskType
                           join at in _context.AssignmentItemTypes on wf.Assignment equals at.assignmentItemType into att
                           from at in att.DefaultIfEmpty()

                           where wf.Id == id

                           select new WorkflowSettingsVM
                           {
                               WorkflowSetting = wf

                           }).FirstOrDefault();

            return results;

        }


        public List<wfSettings> GetUserQueues()
        {
            return _context.wfSettings.Where(x => x.Active == true).GroupBy(p => p.QueueID).Select(g => g.FirstOrDefault()).ToList();
        }



        public ServiceResult InsertWFSetting(wfSettings model)
        {
            _context.wfSettings.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult UpdateWFSetting(wfSettings model)
        {
            var old = _context.wfSettings.Where(x => x.QueueID == model.QueueID).FirstOrDefault();

            old.QueueName = model.QueueName;
            old.MedicalAidID = model.MedicalAidID;
            old.ProgramID = model.ProgramID;
            old.ManagementStatus = model.ManagementStatus;
            old.FromDate = model.FromDate;
            old.ToDate = model.ToDate;
            old.RiskRating = model.RiskRating;
            old.Assignment = model.Assignment;
            old.Instruction = model.Instruction;
            old.PathologyField = model.PathologyField;
            old.Less = model.Less;
            old.Greater = model.Greater;
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        public List<wfSettings> DeleteWFSetting(Guid queueid)
        {
            string sql = string.Format(@"DELETE from wfSettings WHERE QueueID = '{0}'", queueid);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = db.Query<wfSettings>(sql).ToList();
                db.Close();

                return results;
            }
        }

        public List<wfUserQueue> GetUserQueueList()
        {
            return _context.wfUserQueues.Where(x => x.Active == true).ToList(); //hcare-1298
        }
        public wfUserQueue GetUserQueueListByQueueID(Guid queueID)
        {
            return _context.wfUserQueues.Where(x => x.QueueID == queueID).FirstOrDefault();
        }

        public ServiceResult InsertWFUserQueue(wfUserQueue model)
        {
            _context.wfUserQueues.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult DeleteWFUserQueue(Guid queueID)
        {
            var old = _context.wfUserQueues.Where(x => x.QueueID == queueID).FirstOrDefault();

            _context.wfUserQueues.Remove(old);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateWFUserQueue(wfUserQueue model)
        {
            var old = _context.wfUserQueues.Where(x => x.Id == model.Id).FirstOrDefault();

            old.UserID = model.UserID;
            old.QueueID = model.QueueID;
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateWFUserID(int id, Guid? userID)
        {
            var old = _context.wfQueues.Where(x => x.Id == id).FirstOrDefault();

            old.UserID = userID;

            return _context.SaveChanges();
        }

        public ServiceResult UpdateUserQueueShuffleDate(Guid userID, Guid queueID)
        {
            var old = _context.wfUserQueues.Where(x => x.UserID == userID).Where(x => x.QueueID == queueID).Where(x => x.Active == true).FirstOrDefault();

            old.ShuffleDate = DateTime.Now;

            return _context.SaveChanges();
        }

        public List<wfViewModel> GetWorkFlowSettings() //HCare-1134
        {
            string sql = string.Format(@"SELECT wfs.QueueName[QueueName],wfs.QueueID[QueueID],wfs.QueueID[QueueID], ma.Name[MedicalScheme], p.ProgramName[Program], wfs.ManagementStatus[ManagementStatus], wfs.FromDate[FromDate], wfs.ToDate[ToDate],
                                        wfs.RiskRating[RiskRating], wfs.Assignment[AssignmentItemType], wfs.Active[Active]
                                        FROM wfSettings wfs
                                        INNER JOIN MedicalAid ma ON wfs.MedicalAidID = ma.MedicalAidID
                                        INNER JOIN Programs p ON wfs.ProgramID = p.programID
                                        LEFT OUTER JOIN ManagementStatus ms ON wfs.ManagementStatus = ms.statusCode
                                        LEFT OUTER JOIN RiskRatingTypes rr ON wfs.RiskRating = rr.RiskType
                                        LEFT OUTER JOIN AssignmentItemTypes ait ON wfs.Assignment = ait.assignmentItemType");

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<wfViewModel>)db.Query<wfViewModel>(sql, null, commandTimeout: 500).ToList();
                db.Close();

                foreach (var result in results)
                {
                    #region risk-rating
                    var riskrating = result.RiskRating;

                    if (result.RiskRating != null)
                    {
                    }
                    else
                    {
                        result.RiskRating = "-";
                    }
                    #endregion
                    #region management-status
                    if (result.ManagementStatus != null)
                    {
                        string[] mstatus = result.ManagementStatus.Split(',');
                        string concatenation = string.Empty;
                        foreach (string s in mstatus)
                        {
                            concatenation += _context.ManagementStatus.Where(x => x.statusCode == s).Select(x => x.statusName).FirstOrDefault() + ",";
                        }

                        result.ManagementStatus = concatenation.TrimEnd(',');
                    }
                    else
                    {
                        result.ManagementStatus = "-";
                    }
                    #endregion
                    #region assignment
                    if (result.AssignmentItemType != null)
                    {
                        string[] assignment = result.AssignmentItemType.Split(',');
                        string concatenation = string.Empty;
                        foreach (string s in assignment)
                        {
                            concatenation += _context.AssignmentItemTypes.Where(x => x.assignmentItemType == s).Select(x => x.itemDescription).FirstOrDefault() + ",";
                        }

                        result.AssignmentItemType = concatenation.TrimEnd(',');
                    }
                    else
                    {
                        result.AssignmentItemType = "-";
                    }
                    #endregion

                }

                List<wfViewModel> distinct = results.GroupBy(p => p.QueueID).Select(g => g.First()).ToList();

                return distinct;
            }
        }
        public wfSettings GetWorkFlowSettingInformation(Guid medicalaidID, Guid programID, string managementStatus, string riskRating)
        {
            return _context.wfSettings.Where(x => x.Active).Where(x => x.MedicalAidID == medicalaidID).Where(x => x.ProgramID == programID).Where(x => x.ManagementStatus == managementStatus).Where(x => x.RiskRating == riskRating).FirstOrDefault();
        }

        public wfQueueVM GetWFUserQueueById(int id)
        {
            var results = (from uwf in _context.wfUserQueues
                           join wfs in _context.wfSettings on uwf.QueueID equals wfs.QueueID
                           join u in _context.Users on uwf.UserID equals u.userID

                           where uwf.Id == id

                           select new wfQueueVM
                           {
                               Id = uwf.Id,
                               UserID = uwf.UserID,
                               QueueID = uwf.QueueID,
                               QueueName = wfs.QueueName,
                               UserName = u.Firstname + " " + u.Lastname,
                               CreatedBy = uwf.CreatedBy,
                               CreatedDate = uwf.CreatedDate,
                               ModifiedBy = uwf.ModifiedBy,
                               ModifiedDate = uwf.ModifiedDate,
                               Active = uwf.Active

                           }).FirstOrDefault();

            return results;

        }
        #endregion

        public List<Log> GetLogs() //HCare-134
        {
            return _context.Log.Take(100).ToList();
        }
        public List<HivcomorbidRules> GetHivcomorbidRules()
        {
            return _context.HivcomorbidRules.OrderByDescending(x => x.active == true).ThenBy(x => x.stage).ThenBy(x => x.Comorbid).ToList();
        }

        public ServiceResult InsertHivComorbidRules(HivcomorbidRules model)
        {
            _context.HivcomorbidRules.Add(model);
            var res = _context.SaveChanges();

            return res;
        }
        public List<Log> GetLogs(string searchVar) //HCare-134
        {
            var results = _context.Log
                .Where(x => x.TableName.ToLower().Contains(searchVar)
                || x.ColumnName.ToLower().Contains(searchVar)
                || x.NewValue.ToLower().Contains(searchVar)
                || x.RecordID.Contains(searchVar)).ToList(); //hcare-1442

            return results;
        }
        public ServiceResult UpdateHivComorbidRules(HivcomorbidRules model)
        {
            var old = _context.HivcomorbidRules.Where(x => x.Id == model.Id).FirstOrDefault();
            old.Comorbid = model.Comorbid;
            old.stage = model.stage;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.active = model.active;

            return _context.SaveChanges();
        }
        public HivcomorbidRules GetHivComorbidRules(int hivcomorbidRules)
        {
            return _context.HivcomorbidRules.Where(x => x.Id == hivcomorbidRules).SingleOrDefault();
        }

        public List<HIVStages> GetHIVStages() //HCare-1000
        {
            return _context.HIVStages.Where(x => x.Active == true).ToList();
        }

        public List<ComorbidConditionExclusions> GetComorbidConditionExclusions()//HCare-1000
        {
            var results = _context.ComorbidConditionExclusions.Where(x => x.Active == true).Distinct().ToList();

            return results;
        }
        public List<HivcomorbidRules> GetHivComorbidRulesValidation()//HCare-1440
        {
            return _context.HivcomorbidRules.Where(x => x.active == true).ToList();
        }

        public string GetHivComorbidRulesByName(string Name)
        {
            return _context.HivcomorbidRules.Where(x => x.active == true).Where(x => x.Comorbid == Name).Select(x => x.Comorbid).SingleOrDefault();
        }

        public List<MedicalAidVM> GetMedicalAidInformation(Guid userID) //hcare-1346
        {
            var results = (from s in _context.UserSchemeAccess
                           join ma in _context.MedicalAids
                           on s.medicalAidId equals ma.MedicalAidID
                           where s.userId == userID
                           where s.Active == true
                           select new MedicalAidVM
                           {
                               Id = s.Id,
                               MedicalAidId = s.medicalAidId,
                               MedicalAidCode = ma.medicalAidCode,
                               MedicalAidName = ma.Name,
                               Active = ma.Active,
                               UserAccess = s.Active,
                               CLCarrier = ma.clCarrier,

                           }).OrderByDescending(x => x.Active == true).ThenBy(x => x.MedicalAidName).ToList();

            Guid[] ids = new Guid[results.Count()];

            var clinicals = new List<MedicalAidVM>();

            for (int id = 0; id < results.Count(); id++)
            {
                ids[id] = results[id].MedicalAidId;
            }

            foreach (var result in results)
            {
                clinicals.Add(result);
            }

            var medicalAids = _context.MedicalAids.ToList();
            foreach (var medicalAid in medicalAids)
            {
                if (!ids.Contains(medicalAid.MedicalAidID))
                {
                    clinicals.Add(new MedicalAidVM
                    {
                        MedicalAidId = medicalAid.MedicalAidID,
                        MedicalAidCode = medicalAid.medicalAidCode,
                        MedicalAidName = medicalAid.Name,
                        Active = medicalAid.Active,
                        UserAccess = false,
                        CLCarrier = medicalAid.clCarrier,
                    });
                }
            }


            return clinicals;
        }
        public List<Icd10Codes> GetListofICD10Codes() //hcare-1280
        {
            return _context.Icd10Codes.OrderByDescending(x => x.Active == true).ThenBy(x => x.codeType).ThenBy(x => x.icd10CodeID).ToList();
        }

        public List<Icd10Codes> GetICD10CodeValidation() //hcare-1280
        {
            return _context.Icd10Codes.Where(x => x.Active == true).ToList();
        }

        public ServiceResult InsertICD10Code(Icd10Codes model) //hcare-1280
        {
            _context.Icd10Codes.Add(model);
            return _context.SaveChanges();
        }

        public ServiceResult UpdateICD10Code(Icd10Codes model) //hcare-1280
        {
            var old = _context.Icd10Codes.Where(x => x.icd10CodeID == model.icd10CodeID).FirstOrDefault();

            old.icd10CodeID = model.icd10CodeID;
            old.codeType = model.codeType;
            old.subcode = model.subcode;
            old.modifiedBy = model.modifiedBy;
            old.modifiedDate = model.modifiedDate;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        public Icd10Codes GetICD10CodeByID(Guid id) //hcare-1280
        {
            return _context.Icd10Codes.Where(x => x.Id == id).FirstOrDefault();
        }
        public Icd10Codes GetICD10ByProgram(string id) //hcare-1280
        {
            return _context.Icd10Codes.Where(x => x.icd10CodeID == id).FirstOrDefault();
        }

        //HCare-199
        public List<ICD10Master> GetICD10MasteSearch(string ICD10Code, string ICD10Description)
        {
            var result = new List<ICD10Master>();
            if (!string.IsNullOrEmpty(ICD10Description))
            {
                result = _context.IC10Masters.Where(x => x.ICD10Description.ToLower().Contains(ICD10Description.ToLower())).ToList();
            }
            else if (!string.IsNullOrEmpty(ICD10Code))
            {
                if (!result.Any())
                {
                    result = _context.IC10Masters.Where(x => x.ICD10Code.Contains(ICD10Code)).ToList();
                }
                else
                {
                    result = result.Where(x => x.ICD10Code.Contains(ICD10Code)).ToList();
                }
            }
            else
            {
                result = _context.IC10Masters.ToList();
            }
            return result;

        }
        public ICD10Master GetICD10MasterById(string ICD10Code)
        {
            return _context.IC10Masters.Where(x => x.ICD10Code == ICD10Code).SingleOrDefault();
        }
        public List<PopUpTemplate> GetPopUpTemplates()
        {
            var results = _context.PopUpTemplates.OrderBy(x => x.Title).ToList(); //hcare-1374
            foreach (var result in results)
            {
                var new_result = new PopUpTemplate();
                var medicalaid_list = result.selectedSchemes.Replace(" ", "").Split(',').ToList();
                foreach (var med in medicalaid_list)
                {
                    var scheme = GetMedicalAidById(new Guid(med));
                    new_result.selectedSchemes += scheme.Name + ",";
                }

                var option_list = result.selectedOptions.Replace(" ", "").Split(',').ToList();
                foreach (var opt in option_list)
                {
                    var option = GetMedicalAidPlanByName(opt);
                    new_result.selectedOptions += option.planCode + " - " + option.Name + ",";
                }

                var program_list = result.selectedPrograms.Replace(" ", "").Split(',').ToList();
                foreach (var prg in program_list)
                {
                    var program = GetProgramById(new Guid(prg));
                    new_result.selectedPrograms += program.ProgramName + ",";
                }

                result.selectedSchemes = new_result.selectedSchemes.TrimEnd(',');
                result.selectedOptions = new_result.selectedOptions.TrimEnd(',');
                result.selectedPrograms = new_result.selectedPrograms.TrimEnd(',');
            }

            return results.OrderByDescending(x => x.Active == true).ToList();

        }
        public List<PopUpTemplate> GetPopUpTemplateList()
        {
            return _context.PopUpTemplates.Where(x => x.Active == true).OrderBy(x => x.Title).ToList(); //hcare-1374
        }

        public List<ManagementStatus> GetManagementStatusList() //hcare-1363
        {
            return _context.ManagementStatus.Where(x => x.active == true).ToList();
        }

        public ServiceResult InsertPopUpTemplate(PopUpTemplate model)
        {
            var result = _context.PopUpTemplates.Add(model); //hcare-1374
            return _context.SaveChanges();
        }

        #region hcare-1380: email-attachment
        public List<AttachmentTemplate> GetAttachmentTemplateIndex() //hcare-1380
        {
            var results = _context.AttachmentTemplates.ToList();
            foreach (var r in results)
            {
                if (!String.IsNullOrEmpty(r.Program)) { r.Program = _context.Program.Where(x => x.programID == new Guid(r.Program)).Select(x => x.ProgramName).FirstOrDefault(); }
            }

            return results.OrderByDescending(x => x.Active == true).ThenBy(x => x.Program).ThenBy(x => x.AttachmentName).ToList();
        }
        public List<AttachmentTemplate> GetAttachmentTemplateValidation() //hcare-1380
        {
            return _context.AttachmentTemplates.Where(x => x.Active == true).ToList();
        }
        public List<AttachmentTemplate> GetAttachmentTemplateByAccount(Guid medicalaidID, Guid programID) //hcare-1380
        {
            var results = _context.AttachmentTemplates.Where(x => x.Active == true).ToList();
            var valids = _context.AccountAttachmentTemplates.Where(x => x.SchemeID == medicalaidID).Where(x => x.Active == true).Where(x => x.ProgramID == programID).Select(x => x.TemplateID).ToList();
            var filteredRes = (from r in results
                               where valids.Contains(r.Id)
                               select r).ToList();
            return filteredRes;
        }
        public AttachmentTemplate GetAttachmentTemplateByID(Guid id) //hcare-1380
        {
            return _context.AttachmentTemplates.Where(x => x.Id == id).FirstOrDefault();
        }
        public AttachmentTemplate GetAttachmentTemplateDetails(Guid id) //hcare-1380
        {
            var results = _context.AttachmentTemplates.Where(x => x.Id == id).FirstOrDefault();
            if (!String.IsNullOrEmpty(results.Program)) { results.Program = _context.Program.Where(x => x.programID == new Guid(results.Program)).Select(x => x.ProgramName).FirstOrDefault(); }

            return results;
        }
        public AttachmentTemplate GetTemplateByName(string name) //hcare-1380
        {
            return _context.AttachmentTemplates.Where(x => x.AttachmentName.ToLower() == name.ToLower()).Where(x => x.Active == true).FirstOrDefault();
        }
        public List<AttachmentTemplateVM> GetAttachmentTemplatesEdit(Guid accountID, Guid programID, Guid medicalaidID) //hcare-1380
        {
            //HCare-1043
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program on ma.programID equals pr.programID
                           join aa in _context.AccountAttachmentTemplates on new { A = s.AccountId, B = s.MedicalAidId, C = pr.programID } equals new { A = aa.AccountID, B = aa.SchemeID, C = aa.ProgramID }

                           where s.AccountId == accountID
                           where pr.programID == programID
                           where aa.SchemeID == medicalaidID

                           where ma.Active == true
                           where s.Active == true

                           select new AttachmentTemplateVM
                           {
                               Id = aa.Id,
                               AccountID = accountID,
                               ProgramID = pr.programID,
                               MedicalAidID = ma.MedicalAidID,
                               Added = aa.Active,
                               TemplateID = aa.TemplateID,
                           }).GroupBy(x => x.TemplateID).Select(g => g.FirstOrDefault()).ToList();

            Guid[] ids = new Guid[results.Count()];

            var clinicals = new List<AttachmentTemplateVM>();

            for (int id = 0; id < results.Count(); id++)
            {
                ids[id] = results[id].TemplateID;
            }

            var rules = _context.AttachmentTemplates.Where(x => x.Active == true).Where(x => x.Program == _context.Program.Where(p => p.programID == programID).Select(p => p.programID.ToString()).FirstOrDefault() || x.Program == null).ToList(); //HCare-1098
            foreach (var rule in rules)
            {
                if (ids.Contains(rule.Id))
                {
                    clinicals.Add(new AttachmentTemplateVM
                    {
                        Id = results.Where(x => x.MedicalAidID == medicalaidID).Where(x => x.TemplateID == rule.Id).Where(x => x.ProgramID == programID).Select(x => x.Id).FirstOrDefault(),
                        AccountID = accountID,
                        ProgramID = programID,
                        MedicalAidID = medicalaidID,
                        Added = results.Where(x => x.MedicalAidID == medicalaidID).Where(x => x.TemplateID == rule.Id).Where(x => x.ProgramID == programID).Select(x => x.Added).FirstOrDefault(),
                        TemplateID = rule.Id,
                        AttachmentName = rule.AttachmentName,
                        AttachmentFile = rule.FileName,
                    });
                }
                else
                {
                    clinicals.Add(new AttachmentTemplateVM
                    {
                        AccountID = accountID,
                        ProgramID = programID,
                        MedicalAidID = medicalaidID,
                        Added = false,
                        TemplateID = rule.Id,
                        AttachmentName = rule.AttachmentName,
                        AttachmentFile = rule.FileName,
                    });
                }
            }

            return clinicals.OrderBy(x => x.AttachmentName).ToList();
        }
        public AttachmentTemplateVM GetAccountAttachmentTemplate(Guid accountID, Guid programID, Guid medicalaidID, Guid templateID) //hcare-1380
        {
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program on ma.programID equals pr.programID
                           join aa in _context.AccountAttachmentTemplates on new { A = s.AccountId, B = s.MedicalAidId, C = pr.programID } equals new { A = aa.AccountID, B = aa.SchemeID, C = aa.ProgramID }
                           where s.AccountId == accountID
                           where pr.programID == programID
                           where aa.SchemeID == medicalaidID
                           where aa.TemplateID == templateID

                           where ma.Active == true
                           where s.Active == true
                           select new AttachmentTemplateVM
                           {
                               Id = aa.Id,
                               AccountID = accountID,
                               ProgramID = pr.programID,
                               MedicalAidID = ma.MedicalAidID,
                               TemplateID = aa.TemplateID,
                               Added = aa.Active,
                           }).GroupBy(x => x.TemplateID).Select(g => g.FirstOrDefault()).FirstOrDefault();

            return results;
        }
        public ServiceResult InsertAttachmentTemplate(AttachmentTemplate model) //hcare-1380
        {
            _context.AttachmentTemplates.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateAttachmentTemplate(AttachmentTemplate model) //hcare-1380
        {
            var old = _context.AttachmentTemplates.Where(x => x.Id == model.Id).FirstOrDefault();

            old.AttachmentName = model.AttachmentName;
            old.AttachmentType = model.AttachmentType;
            old.Root = model.Root;
            old.FileName = model.FileName;
            old.Program = model.Program;
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = model.ModifiedDate;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        public EmailVM GetEmailDetailByID(int id) //hcare-1380
        {
            var results = (from e in _context.Emails
                           where e.emailId == id

                           select new EmailVM
                           {
                               Id = e.emailId,
                               DependantID = e.dependantID,
                               ProgramID = e.programId,
                               Subject = e.subject,
                               EmailAddressTo = e.emailTo,
                               EmailAddressCc = e.cc,
                               EmailMessage = e.emailMassage,
                               AttachmentName = "",
                               FileName = "",
                               EffectiveDate = e.effectivedate,
                               CreatedDate = e.createdDate,
                               CreatedBy = e.createdBy,
                               ModifiedDate = e.modifiedDate,
                               ModifiedBy = e.modifiedBy,
                               Status = e.status,
                               Program = _context.Program.Where(x => x.programID == e.programId).Select(x => x.ProgramName).FirstOrDefault(),

                           }).FirstOrDefault();

            //var emailtemplates = _context.emailAttachmentHistories.Where(x => x.EmailID == results.Id).ToList();
            //var count = 0;
            //foreach (var item in emailtemplates)
            //{
            //    count++;
            //    var eainformation = _context.AttachmentTemplates.Where(x => x.Id == item.TemplateID).FirstOrDefault();
            //    results.AttachmentName += count + ". (b)Attachment name:(/b) " + eainformation.AttachmentName + ", (b)File name:(/b) (" + eainformation.FileName + ") " + '|';
            //    results.FileName = eainformation.FileName;
            //}

            //results.AttachmentName = results.AttachmentName.TrimEnd('|');

            return results;
        }
        public ServiceResult InsertAccountAttachmentTemplate(AccountAttachmentTemplates model) //hcare-1380
        {
            model.Id = Guid.NewGuid();
            model.CreatedDate = DateTime.Now;
            _context.AccountAttachmentTemplates.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateAccountAttachmentTemplate(AccountAttachmentTemplates model) //hcare-1380
        {
            var old = _context.AccountAttachmentTemplates.Where(x => x.Id == model.Id).FirstOrDefault();
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = DateTime.Now;
            old.Active = model.Active;

            return _context.SaveChanges();
        }
        public PopUpTemplate GetPopUpTemplateByID(Guid id)
        {
            return _context.PopUpTemplates.SingleOrDefault(x => x.Id == id); //hcare-1374
        }

        public PopUpTemplate GetPopUpTemplateDetails(Guid id)
        {
            var result = _context.PopUpTemplates.Where(x => x.Id == id).FirstOrDefault(); //hcare-1374

            var new_result = new PopUpTemplate();
            var medicalaid_list = result.selectedSchemes.Replace(" ", "").Split(',').ToList();
            foreach (var med in medicalaid_list)
            {
                var scheme = GetMedicalAidById(new Guid(med));
                new_result.selectedSchemes += scheme.Name + ",";
            }

            var option_list = result.selectedOptions.Replace(" ", "").Split(',').ToList();
            foreach (var opt in option_list)
            {
                var option = GetMedicalAidPlanByName(opt);
                new_result.selectedOptions += option.planCode + " - " + option.Name + ",";
            }

            var program_list = result.selectedPrograms.Replace(" ", "").Split(',').ToList();
            foreach (var prg in program_list)
            {
                var program = GetProgramById(new Guid(prg));
                new_result.selectedPrograms += program.ProgramName + ",";
            }

            result.selectedSchemes = new_result.selectedSchemes.TrimEnd(',');
            result.selectedOptions = new_result.selectedOptions.TrimEnd(',');
            result.selectedPrograms = new_result.selectedPrograms.TrimEnd(',');

            return result;

        }

        public List<PopUpTemplate> GetPopUpTemplateByMemberInformation(Guid medicalaidID, string option, Guid programID)
        {
            string sql = string.Format(@"select * 
                                        from PopUpTemplate 
                                        where selectedSchemes like '{0}'
                                        and selectedOptions like '%{1}%'
                                        and selectedPrograms like '%{2}%'
                                        and Active = 1", medicalaidID, option, programID);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<PopUpTemplate>)db.Query<PopUpTemplate>(sql, null, commandTimeout: 500).Distinct().ToList();
                db.Close();

                return results;
            }
        }

        public MedicalAid GetMedicalAidById(Guid id)
        {
            return _context.MedicalAids.FirstOrDefault(x => x.MedicalAidID == id); //hcare-1374
        }
        public MedicalAidPlans GetMedicalAidPlanById(Guid id)
        {
            return _context.MedicalAidPlans.FirstOrDefault(x => x.Id == id); //hcare-1374
        }

        public MedicalAidPlans GetMedicalAidPlanByName(string name)
        {
            return _context.MedicalAidPlans.FirstOrDefault(x => x.Name.Replace(" ", "") == name.Replace(" ", "")); //hcare-1374
        }
        #region hcare-1384: email-layout
        public List<RiskRatingMonitor> GetRiskRatingMonitorIndex() //hcare-1395
        {
            var results = _context.RiskRatingMonitors.ToList();
            foreach (var r in results)
            {
                if (!String.IsNullOrEmpty(r.Program)) { r.Program = _context.Program.Where(x => x.programID == new Guid(r.Program)).Select(x => x.ProgramName).FirstOrDefault(); }
                if (!String.IsNullOrEmpty(r.From)) { r.From = _context.RiskRatingTypes.Where(x => x.RiskType == r.From).Select(x => x.RiskName.ToLower()).FirstOrDefault(); }
                if (!String.IsNullOrEmpty(r.To)) { r.To = _context.RiskRatingTypes.Where(x => x.RiskType == r.To).Select(x => x.RiskName.ToLower()).FirstOrDefault(); }
            }

            return results.OrderByDescending(x => x.Active == true).ThenBy(x => x.Program).ThenBy(x => x.Description).ToList();
        }
        public List<RiskRatingMonitor> GetRiskRatingMonitorValidation() //hcare-1395
        {
            return _context.RiskRatingMonitors.Where(x => x.Active == true).ToList();
        }
        public RiskRatingMonitor GetRiskRatingMonitorByID(Guid id) //hcare-1395
        {
            return _context.RiskRatingMonitors.Where(x => x.Id == id).FirstOrDefault();
        }
        public RiskRatingMonitor GetRiskRatingMonitorDetailsByID(Guid id) //hcare-1395
        {
            var results = _context.RiskRatingMonitors.Where(x => x.Id == id).FirstOrDefault();

            if (!String.IsNullOrEmpty(results.Program)) { results.Program = _context.Program.Where(x => x.programID == new Guid(results.Program)).Select(x => x.ProgramName).FirstOrDefault(); }
            if (!String.IsNullOrEmpty(results.From)) { results.From = _context.RiskRatingTypes.Where(x => x.RiskType == results.From).Select(x => x.RiskName.ToLower()).FirstOrDefault(); }
            if (!String.IsNullOrEmpty(results.To)) { results.To = _context.RiskRatingTypes.Where(x => x.RiskType == results.To).Select(x => x.RiskName.ToLower()).FirstOrDefault(); }

            return results;

        }
        public RiskRatingMonitor GetRiskRatingMonitorDetails(Guid id) //hcare-1395
        {
            var results = _context.RiskRatingMonitors.Where(x => x.Id == id).FirstOrDefault();
            if (!String.IsNullOrEmpty(results.Program)) { results.Program = _context.Program.Where(x => x.programID == new Guid(results.Program)).Select(x => x.ProgramName).FirstOrDefault(); }

            return results;
        }
        public ServiceResult InsertRiskRatingMonitor(RiskRatingMonitor model) //hcare-1395
        {
            _context.RiskRatingMonitors.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateRiskRatingMonitor(RiskRatingMonitor model) //hcare-1395
        {
            var old = _context.RiskRatingMonitors.Where(x => x.Id == model.Id).FirstOrDefault();
            old.Description = model.Description;
            old.From = model.From;
            old.To = model.To;
            old.Percentage = model.Percentage;
            old.Program = model.Program;
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = DateTime.Now;
            old.Active = model.Active;

            return _context.SaveChanges();
        }
        public RiskRatingMonitor GetRiskRatingMonitorByName(string name) //hcare-1395
        {
            return _context.RiskRatingMonitors.Where(x => x.Description.ToLower() == name.ToLower()).Where(x => x.Active == true).FirstOrDefault();
        }

        #endregion



        public List<Log> GetSystemLogTables() //hcare-1442
        {
            string sql = string.Format(@"select name[TableName] from SYSOBJECTS where xtype = 'U' order by name asc");

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<Log>)db.Query<Log>(sql, null, commandTimeout: 1000).ToList();
                db.Close();

                return results;
            }
        }
        public List<Log> GetSystemLogColumns(string table) //hcare-1442
        {
            string sql = string.Format(@"select distinct ColumnName from Log where TableName = '{0}' order by ColumnName asc", table);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<Log>)db.Query<Log>(sql, null, commandTimeout: 1000).ToList();
                db.Close();

                return results;
            }
        }
        public List<Log> GetSystemLogRecords(string table) //hcare-1442
        {
            string sql = string.Format(@"select distinct RecordID from Log where TableName = '{0}' order by RecordID asc", table);

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<Log>)db.Query<Log>(sql, null, commandTimeout: 1000).ToList();
                db.Close();

                return results;
            }
        }

        public List<SystemLogResultsVM> GetSystemLogSearchResults(string table, string column, string recordID, string currentValue, string createdDate) //hcare-1442
        {
            string[] column_list = column.Split(',');

            string columns = string.Empty;
            foreach (var c in column_list)
            {
                columns += "'" + c + "',";
            }

            string sql = string.Format(@"select LogID, RecordID, EventType, TableName, ColumnName, NewValue[CurrentValue], Created_date[CreatedDate]
                                        from Log where TableName like '{0}' and Created_date >= '{1}'", table, createdDate);

            if (!String.IsNullOrEmpty(column))
            {
                sql = sql + string.Format(" AND ColumnName in ({0})", columns.TrimEnd(','));
            }
            if (!String.IsNullOrEmpty(recordID))
            {
                sql = sql + string.Format(" AND RecordID LIKE '%{0}%'", recordID);
            }
            if (!String.IsNullOrEmpty(currentValue))
            {
                sql = sql + string.Format(" AND NewVAlue LIKE '%{0}%'", currentValue);
            }

            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                db.Open();
                var results = (List<SystemLogResultsVM>)db.Query<SystemLogResultsVM>(sql, null, commandTimeout: 1000).ToList();
                db.Close();

                return results.OrderByDescending(x => x.CreatedDate).ToList();
            }
        }

        public List<EmailAttachmentHistory> GetEmailAttachmentByDependantID(Guid dependantID, Guid programID, DateTime today) //hcare-1380
        {
            var results = _context.emailAttachmentHistories.Where(x => x.DependantID == dependantID).Where(x => x.ProgramID == programID).Where(x => x.CreatedDate >= today).Where(x => x.Active == true).Where(x => x.Sent == false).ToList();
            return results;
        }
        public List<EmailAttachmentHistory> GetEmailAttachmentHistoryByEmailID(int id) //hcare-1380
        {
            return _context.emailAttachmentHistories.Where(x => x.EmailID == id).Where(x => x.Active == true).Where(x => x.Sent == true).ToList();
        }
        public EmailAttachmentHistory GetEmailAttachmentByID(Guid tempalteid, Guid dependantid, Guid programid) //hcare-1380
        {
            return _context.emailAttachmentHistories.Where(x => x.TemplateID == tempalteid).Where(x => x.DependantID == dependantid).Where(x => x.ProgramID == programid).Where(x => x.Sent == false).FirstOrDefault();
        }
        public ServiceResult InsertEmailAttachmentHistory(EmailAttachmentHistory model) //hcare-1380
        {
            _context.emailAttachmentHistories.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateEmailAttachmentHistory(EmailAttachmentHistory model) //hcare-1380
        {
            var old = _context.emailAttachmentHistories.Where(x => x.Id == model.Id).FirstOrDefault();
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = DateTime.Now;
            old.Active = model.Active;
            old.EmailID = model.EmailID;
            old.Sent = model.Sent;

            return _context.SaveChanges();
        }
        public ServiceResult ResetAttachmentEmailHistory(Guid dependantID, Guid programID) //hcare-1380
        {
            var results = _context.emailAttachmentHistories.Where(x => x.DependantID == dependantID).Where(x => x.ProgramID == programID).Where(x => x.Active == true).Where(x => x.Sent == false).ToList();
            foreach (var result in results)
            {
                result.Active = false;
            }

            return _context.SaveChanges();
        }

        public SmsMessages GetTextMessageByID(int id) //hcare-1378
        {
            var results = _context.SmsMessages.Where(x => x.messageID == id).FirstOrDefault();

            return results;
        }
        #endregion

        #region hcare-1384: email-layout
        public List<EmailLayout> GetEmailLayoutIndex() //hcare-1384
        {
            var results = _context.EmailLayouts.ToList();
            foreach (var r in results)
            {
                if (!String.IsNullOrEmpty(r.Program)) { r.Program = _context.Program.Where(x => x.programID == new Guid(r.Program)).Select(x => x.ProgramName).FirstOrDefault(); }
                if (!String.IsNullOrEmpty(r.LayoutType)) { r.LayoutType = char.ToUpper(r.LayoutType[0]) + r.LayoutType.Substring(1); }
                if (!String.IsNullOrEmpty(r.LayoutSize)) { r.LayoutSize = char.ToUpper(r.LayoutSize[0]) + r.LayoutSize.Substring(1); }
            }

            return results.OrderByDescending(x => x.Active == true).ThenBy(x => x.Program).ThenBy(x => x.Description).ToList();
        }
        public List<EmailLayout> GetEmailLayoutValidation() //hcare-1384
        {
            return _context.EmailLayouts.Where(x => x.Active == true).ToList();
        }
        public EmailLayout GetEmailLayoutByID(Guid id) //hcare-1384
        {
            return _context.EmailLayouts.Where(x => x.Id == id).FirstOrDefault();
        }
        public EmailLayout GetEmailLayoutDetails(Guid id) //hcare-1384
        {
            var results = _context.EmailLayouts.Where(x => x.Id == id).FirstOrDefault();
            if (!String.IsNullOrEmpty(results.Program)) { results.Program = _context.Program.Where(x => x.programID == new Guid(results.Program)).Select(x => x.ProgramName).FirstOrDefault(); }

            return results;
        }
        public EmailLayout GetEmailLayoutByName(string name) //hcare-1384
        {
            return _context.EmailLayouts.Where(x => x.Description.ToLower() == name.ToLower()).Where(x => x.Active == true).FirstOrDefault();
        }
        public List<EmailLayout> GetEmailLayoutByAccount(Guid medicalaidID, Guid programID) //hcare-1384
        {
            var results = _context.EmailLayouts.Where(x => x.Active == true).ToList();
            var valids = _context.AccountEmailLayouts.Where(x => x.SchemeID == medicalaidID).Where(x => x.Active == true).Where(x => x.ProgramID == programID).Select(x => x.LayoutID).ToList();
            var filteredRes = (from r in results
                               where valids.Contains(r.Id)
                               select r).ToList();
            return filteredRes;
        }
        public List<EmailLayoutVM> GetEmailLayoutByAccountID(Guid accountID, Guid programID, Guid medicalaidID) //hcare-1384
        {
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program on ma.programID equals pr.programID
                           join ae in _context.AccountEmailLayouts on new { A = s.AccountId, B = s.MedicalAidId, C = pr.programID } equals new { A = ae.AccountID, B = ae.SchemeID, C = ae.ProgramID }

                           where s.AccountId == accountID
                           where pr.programID == programID
                           where ae.SchemeID == medicalaidID

                           where ma.Active == true
                           where s.Active == true

                           select new EmailLayoutVM
                           {
                               Id = ae.Id,
                               AccountID = accountID,
                               ProgramID = pr.programID,
                               MedicalAidID = ma.MedicalAidID,
                               Added = ae.Active,
                               LayoutID = ae.LayoutID,

                           }).GroupBy(x => x.LayoutID).Select(g => g.FirstOrDefault()).ToList();

            Guid[] ids = new Guid[results.Count()];

            var clinicals = new List<EmailLayoutVM>();

            for (int id = 0; id < results.Count(); id++)
            {
                ids[id] = results[id].LayoutID;
            }

            var rules = _context.EmailLayouts.Where(x => x.Active == true).Where(x => x.Program == _context.Program.Where(p => p.programID == programID).Select(p => p.programID.ToString()).FirstOrDefault() || x.Program == null).ToList(); //HCare-1098
            foreach (var rule in rules)
            {
                if (ids.Contains(rule.Id))
                {
                    clinicals.Add(new EmailLayoutVM
                    {
                        Id = results.Where(x => x.MedicalAidID == medicalaidID).Where(x => x.LayoutID == rule.Id).Where(x => x.ProgramID == programID).Select(x => x.Id).FirstOrDefault(),
                        AccountID = accountID,
                        ProgramID = programID,
                        MedicalAidID = medicalaidID,
                        Added = results.Where(x => x.MedicalAidID == medicalaidID).Where(x => x.LayoutID == rule.Id).Where(x => x.ProgramID == programID).Select(x => x.Added).FirstOrDefault(),
                        LayoutID = rule.Id,
                        LayoutDescription = rule.Description,
                        AttachmentFile = rule.FileName,
                        LayoutType = rule.LayoutType,
                    });
                }
                else
                {
                    clinicals.Add(new EmailLayoutVM
                    {
                        AccountID = accountID,
                        ProgramID = programID,
                        MedicalAidID = medicalaidID,
                        Added = false,
                        LayoutID = rule.Id,
                        LayoutDescription = rule.Description,
                        AttachmentFile = rule.FileName,
                        LayoutType = rule.LayoutType,
                    });
                }
            }

            return clinicals.OrderBy(x => x.LayoutDescription).ToList();
        }
        public EmailLayoutVM GetEmailLayoutForAccount(Guid accountID, Guid programID, Guid medicalaidID, Guid templateID) //hcare-1384
        {
            var results = (from s in _context.AccountSchemes
                           join ma in _context.MedicalAidPlanPrograms on s.MedicalAidId equals ma.MedicalAidID
                           join pr in _context.Program on ma.programID equals pr.programID
                           join ae in _context.AccountEmailLayouts on new { A = s.AccountId, B = s.MedicalAidId, C = pr.programID } equals new { A = ae.AccountID, B = ae.SchemeID, C = ae.ProgramID }

                           where s.AccountId == accountID
                           where pr.programID == programID
                           where ae.SchemeID == medicalaidID
                           where ae.LayoutID == templateID

                           where ma.Active == true
                           where s.Active == true
                           select new EmailLayoutVM
                           {
                               Id = ae.Id,
                               AccountID = accountID,
                               ProgramID = pr.programID,
                               MedicalAidID = ma.MedicalAidID,
                               LayoutID = ae.LayoutID,
                               Added = ae.Active,

                           }).GroupBy(x => x.LayoutID).Select(g => g.FirstOrDefault()).FirstOrDefault();

            return results;
        }

        public ServiceResult InsertEmailLayout(EmailLayout model) //hcare-1384
        {
            _context.EmailLayouts.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateEmailLayout(EmailLayout model) //hcare-1384
        {
            var old = _context.EmailLayouts.Where(x => x.Id == model.Id).FirstOrDefault();
            old.Description = model.Description;
            old.LayoutType = model.LayoutType;
            old.LayoutSize = model.LayoutSize;
            old.LayoutHeight = model.LayoutHeight;
            old.LayoutWidth = model.LayoutWidth;
            old.AttachmentType = model.AttachmentType;
            old.FileName = model.FileName;
            old.FileExtension = model.FileExtension;
            old.Program = model.Program;
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = DateTime.Now;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        public ServiceResult InsertAccountEmailLayout(AccountEmailLayout model) //hcare-1384
        {
            model.Id = Guid.NewGuid();
            model.CreatedDate = DateTime.Now;
            _context.AccountEmailLayouts.Add(model);
            return _context.SaveChanges();
        }
        public ServiceResult UpdateAccountEmailLayout(AccountEmailLayout model) //hcare-1384
        {
            var old = _context.AccountEmailLayouts.Where(x => x.Id == model.Id).FirstOrDefault();
            old.ModifiedBy = model.ModifiedBy;
            old.ModifiedDate = DateTime.Now;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        #endregion

        public List<DoctorEmailVM> GetDoctorDetails(string doctorname, string practicenumber, string practicename) //hcare-1391
        {
            if (!String.IsNullOrEmpty(practicenumber))
            {
                var results = (from d in _context.Doctors
                               join di in _context.DoctorsInformation on d.doctorID equals di.DoctorID into din
                               from di in din.DefaultIfEmpty()
                               join cp in _context.Practices on d.practiceNo equals cp.practiceNo into pr
                               from cp in pr.DefaultIfEmpty()

                               where d.active == true
                               where d.practiceNo.Contains(practicenumber)

                               select new DoctorEmailVM
                               {
                                   DoctorID = d.doctorID,
                                   DoctorName = d.drLastName,
                                   PracticeNumber = d.practiceNo,
                                   PracticeName = cp.practiceName,
                                   EmailAddress = di.Email,

                               }).ToList();

                foreach (var result in results)
                {
                    if (String.IsNullOrEmpty(result.DoctorName)) { result.DoctorName = "-"; }
                    if (String.IsNullOrEmpty(result.PracticeNumber)) { result.PracticeNumber = "-"; }
                    if (String.IsNullOrEmpty(result.PracticeName)) { result.PracticeName = "-"; }
                    if (String.IsNullOrEmpty(result.EmailAddress)) { result.EmailAddress = "-"; }
                }

                return results;
            }
            else if (!String.IsNullOrEmpty(doctorname))
            {
                var results = (from d in _context.Doctors
                               join di in _context.DoctorsInformation on d.doctorID equals di.DoctorID into din
                               from di in din.DefaultIfEmpty()
                               join cp in _context.Practices on d.practiceNo equals cp.practiceNo into pr
                               from cp in pr.DefaultIfEmpty()

                               where d.active == true
                               where d.drLastName.Replace(" ", "").ToLower().Contains(doctorname.Replace(" ", "").ToLower())

                               select new DoctorEmailVM
                               {
                                   DoctorID = d.doctorID,
                                   DoctorName = d.drLastName,
                                   PracticeNumber = d.practiceNo,
                                   PracticeName = cp.practiceName,
                                   EmailAddress = di.Email,

                               }).ToList();

                foreach (var result in results)
                {
                    if (String.IsNullOrEmpty(result.DoctorName)) { result.DoctorName = "-"; }
                    if (String.IsNullOrEmpty(result.PracticeNumber)) { result.PracticeNumber = "-"; }
                    if (String.IsNullOrEmpty(result.PracticeName)) { result.PracticeName = "-"; }
                    if (String.IsNullOrEmpty(result.EmailAddress)) { result.EmailAddress = "-"; }
                }

                return results;
            }
            else if (!String.IsNullOrEmpty(practicename))
            {
                var results = (from d in _context.Doctors
                               join di in _context.DoctorsInformation on d.doctorID equals di.DoctorID into din
                               from di in din.DefaultIfEmpty()
                               join cp in _context.Practices on d.practiceNo equals cp.practiceNo into pr
                               from cp in pr.DefaultIfEmpty()

                               where d.active == true
                               where cp.practiceName.Contains(practicename)

                               select new DoctorEmailVM
                               {
                                   DoctorID = d.doctorID,
                                   DoctorName = d.drLastName,
                                   PracticeNumber = d.practiceNo,
                                   PracticeName = cp.practiceName,
                                   EmailAddress = di.Email,

                               }).ToList();

                foreach (var result in results)
                {
                    if (String.IsNullOrEmpty(result.DoctorName)) { result.DoctorName = "-"; }
                    if (String.IsNullOrEmpty(result.PracticeNumber)) { result.PracticeNumber = "-"; }
                    if (String.IsNullOrEmpty(result.PracticeName)) { result.PracticeName = "-"; }
                    if (String.IsNullOrEmpty(result.EmailAddress)) { result.EmailAddress = "-"; }
                }

                return results;
            }
            else
            {
                return null;
            }

        }
        public DoctorEmailVM GetDoctorByDoctorID(Guid doctorID) //hcare-1391
        {
            var results = (from d in _context.Doctors
                           join di in _context.DoctorsInformation on d.doctorID equals di.DoctorID into din
                           from di in din.DefaultIfEmpty()
                           join cp in _context.Practices on d.practiceNo equals cp.practiceNo into pr
                           from cp in pr.DefaultIfEmpty()

                           where d.active == true
                           where d.doctorID == doctorID

                           select new DoctorEmailVM
                           {
                               DoctorID = d.doctorID,
                               DoctorName = d.drLastName,
                               PracticeNumber = d.practiceNo,
                               PracticeName = cp.practiceName,
                               EmailAddress = di.Email,

                           }).FirstOrDefault();

            return results;

        }

        public MedicalAidSettings GetMedicalAidSettings(Guid medicalaidID) //hcare-1380
        {
            return _context.MedicalAidSettings.Where(x => x.medicalAidId == medicalaidID).FirstOrDefault();
        }

        public ServiceResult UpdatePopUpTemplate(PopUpTemplate model)
        {
            var old = _context.PopUpTemplates.Where(x => x.Id == model.Id).FirstOrDefault(); //hcare-1374
            old.Title = model.Title;
            old.Template = model.Template;
            old.selectedSchemes = model.selectedSchemes;
            old.selectedOptions = model.selectedOptions;
            old.selectedPrograms = model.selectedPrograms;
            old.Type = model.Type;
            old.ModifiedDate = model.ModifiedDate;
            old.ModifiedBy = model.ModifiedBy;
            old.Active = model.Active;

            return _context.SaveChanges();
        }

        public List<ICD10Master> GetICD10Masters()
        {
            return _context.IC10Masters.Where(x => x.Active == true).ToList();
        }
    }
}