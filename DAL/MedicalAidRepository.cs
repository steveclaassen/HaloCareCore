using Dapper;
using HaloCareCore.Models;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Service;
using HaloCareCore.Models.Validation;
using HaloCareCore.Repos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace HaloCareCore.DAL
{
    public class MedicalAidRepository : IMedicalAidRepository
    {
        private OH17Context _context;
        private IConfiguration Configuration;

        public MedicalAidRepository(IConfiguration configuration, OH17Context context)
        {
            this._context = context;
        }

        public Guid InsertMedicalAid(MedicalAid medical)
        {
            medical.MedicalAidID = Guid.NewGuid();
            medical.createdDate = DateTime.Now;
            medical.Active = true;
            _context.MedicalAids.Add(medical);
            Save();
            return medical.MedicalAidID;
        }

        public bool InsertMedicalAidSettings(MedicalAidSettings medical)
        {
            medical.createdDate = DateTime.Now;
            medical.Active = true;
            _context.MedicalAidSettings.Add(medical);
            Save();
            return true;
        }

        public bool UpdateMedicalAid(MedicalAid medical)
        {
            var old = _context.MedicalAids.Where(x => x.MedicalAidID == medical.MedicalAidID).FirstOrDefault();
            old.Name = medical.Name;
            old.medicalAidCode = medical.medicalAidCode;
            old.modifiedBy = medical.modifiedBy;
            old.modifiedDate = DateTime.Now;
            old.Active = medical.Active;
            old.clCarrier = medical.clCarrier;
            old.riskRules = medical.riskRules;
            old.reporting = medical.reporting;
            old.titleSync = medical.titleSync; //Hcare-1040
            old.disclaimer = medical.disclaimer; //Hcare-864
            old.Referral = medical.Referral;
            old.Production = medical.Production; //hcare-1292
            old.Extracts = medical.Extracts; //hcare-1292
            old.Imports = medical.Imports; //hcare-1292
            old.Exports = medical.Exports; //hcare-1292
            old.AdvancedSearch = medical.AdvancedSearch; //hcare-1328
            old.sync = medical.sync;//Hcare-1321
            //old.MedicalAidClaimCode = medical.MedicalAidClaimCode; //HCare-1086

            var result = _context.SaveChanges();
            return result.Success;
        }

        public bool UpdateMedicalAidSettings(MedicalAidSettings medical)
        {
            var model = _context.MedicalAidSettings.Where(x => x.medicalAidId == medical.medicalAidId).FirstOrDefault();

            if (model == null)
            {
                medical.createdDate = DateTime.Now;
                _context.MedicalAidSettings.Add(medical);
                var res = _context.SaveChanges();
                Save();
            }
            else
            {
                model.modifiedDate = DateTime.Now;
                model.daysForPathology = medical.daysForPathology;
                model.daysForCommunicationHiv = medical.daysForCommunicationHiv;
                model.daysForCommunicationGen = medical.daysForCommunicationGen;
                model.overduePathologySms = medical.overduePathologySms;
                model.overdueScriptSms = medical.overdueScriptSms;
                model.hivPreAppovalRequired = medical.hivPreAppovalRequired;
                model.authRequired = medical.authRequired;
                model.Active = medical.Active;
                model.email = medical.email; //hcare-1139
                model.schemeSubject = medical.schemeSubject; //HCare-1197
                var result = _context.SaveChanges();
                return result.Success;
            }
            return true;
        }

        //public MedicalAidViewModel GetMedicalAid(Guid medicalaidID)
        //{
        //    var results = (from m in _context.MedicalAids
        //                   join ms in _context.MedicalAidSettings
        //                   on m.MedicalAidID equals ms.medicalAidId into joinTable
        //                   from z in joinTable.DefaultIfEmpty()
        //                   where m.MedicalAidID == medicalaidID
        //                   where m.Active == true
        //                   select new MedicalAidViewModel
        //                   {
        //                       medicalAid = m,
        //                       medicalAidSettings = z
        //                   }).FirstOrDefault();

        //    return results;
        //}

        public MedicalAidViewModel GetMedicalAid(Guid medicalaidID)
        {
            var results = (from m in _context.MedicalAids
                           join ms in _context.MedicalAidSettings
                           on m.MedicalAidID equals ms.medicalAidId into joinTable
                           from z in joinTable.DefaultIfEmpty()
                           where m.MedicalAidID == medicalaidID
                           select new MedicalAidViewModel
                           {
                               medicalAid = m,
                               medicalAidSettings = z
                           }).FirstOrDefault();

            return results;

        }

        public MedicalAidViewModel GetMedicalAidDetails(Guid medicalaidID)
        {
            var results = (from m in _context.MedicalAids
                           join ms in _context.MedicalAidSettings on m.MedicalAidID equals ms.medicalAidId into joinTable
                           from z in joinTable.DefaultIfEmpty()
                           join a in _context.MedicalAidAccounts on m.AccountCode equals a.Code into aTable
                           from a in aTable.DefaultIfEmpty()
                           join mc in _context.MedicalAidClaimCode on m.medicalAidCode equals mc.medicalAidCode into mcTable
                           from mc in mcTable.DefaultIfEmpty()/* HCare-1086*/
                           where m.MedicalAidID == medicalaidID
                           select new MedicalAidViewModel
                           {
                               medicalAid = m,
                               medicalAidSettings = z,
                               MedicalAidAccount = a,
                               MedicalAidClaimCodes = mc /* HCare-1086*/

                           }).FirstOrDefault();

            return results;

        }

        public MedicalAidViewModel GetMedicalAid(string medicalAidCode)
        {
            var results = (from m in _context.MedicalAids
                           join ms in _context.MedicalAidSettings
                           on m.MedicalAidID equals ms.medicalAidId
                           where m.medicalAidCode == medicalAidCode
                           where m.Active == true
                           select new MedicalAidViewModel
                           {
                               medicalAid = m,
                               medicalAidSettings = ms
                           }).FirstOrDefault();

            return results;
        }

        //HCare-1216
        public MedicalAidViewModel GetMedicalAidName(string medicalAid)
        {
            var results = (from m in _context.MedicalAids
                           join ms in _context.MedicalAidSettings
                           on m.MedicalAidID equals ms.medicalAidId
                           where m.Name == medicalAid
                           where m.Active == true

                           select new MedicalAidViewModel
                           {
                               medicalAid = m,
                               medicalAidSettings = ms
                           }).FirstOrDefault();

            return results;
        }

        public List<MedicalAidViewModel> GetMedicalAids()
        {
            var results = (from m in _context.MedicalAids
                           join ms in _context.MedicalAidSettings
                           on m.MedicalAidID equals ms.medicalAidId
                           select new MedicalAidViewModel
                           {
                               medicalAid = m,
                               medicalAidSettings = ms
                           }).OrderBy(x => x.medicalAid.Name).ToList();

            return results;
        }
        //HCare-1086
        public List<MedicalAidClaimCode> GetMedicalAidCode(string medicalAidCode)
        {
            var results = _context.MedicalAidClaimCode.Where(x => x.medicalAidCode == medicalAidCode).ToList();
            return results;
        }

        public ServiceResult updateQueries_MedicalAid(Queries model)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
            {
                string updateQuery = @"UPDATE Queries
                                     SET Active = 0
                                     From Queries q
                                     INNER JOIN Dependant d ON q.dependentID = d.DependantID
                                     INNER JOIN Member m ON d.memberID = m.memberID
                                     INNER JOIN MedicalAid ma ON m.medicalAidID = ma.MedicalAidID
                                     
                                     WHERE ma.Active = 0";

                var result = db.Execute(updateQuery);
            }

            return _context.SaveChanges();
        }

        public void InsertMedicalAidClaimingCode(MedicalAidClaimCode medicalAidClaimCode)
        {

            medicalAidClaimCode.createdDate = DateTime.Now;
            medicalAidClaimCode.effectiveDate = DateTime.Now;
            medicalAidClaimCode.Active = true;
            _context.MedicalAidClaimCode.Add(medicalAidClaimCode);

            var result = _context.SaveChanges();
            // Save();
        }
        public bool UpdateMedicalAidClaimingCode(MedicalAidClaimCode medicalAidClaimCode)
        {
            var old = _context.MedicalAidClaimCode.FirstOrDefault(x => x.medicalAidCode == medicalAidClaimCode.medicalAidCode);

            old.medicalAidCode = medicalAidClaimCode.medicalAidCode;
            old.claimCode = medicalAidClaimCode.claimCode;
            old.modifiedBy = medicalAidClaimCode.modifiedBy;
            old.modifiedDate = medicalAidClaimCode.modifiedDate;
            old.Active = medicalAidClaimCode.Active;
            var result = _context.SaveChanges();

            return result.Success;

        }
        public bool UpdateMedicalAidClaimCode(MedicalAidClaimCode medicalAidClaimCode)
        {
            var old = _context.MedicalAidClaimCode.Where(x => x.claimCode == medicalAidClaimCode.claimCode).Where(x => x.Active == true).FirstOrDefault();

            old.claimCode = medicalAidClaimCode.claimCode;
            old.modifiedBy = medicalAidClaimCode.modifiedBy;
            old.modifiedDate = medicalAidClaimCode.modifiedDate;
            old.Active = medicalAidClaimCode.Active;
            var result = _context.SaveChanges();

            return result.Success;

        }
        //Hcare-1086
        public string GetClaimCode(string medicalAidCode)
        {
            return _context.MedicalAidClaimCode.Where(x => x.claimCode == medicalAidCode).Where(x => x.Active == true).Select(x => x.claimCode).SingleOrDefault();
        }

        public MedicalAid GetMedicalAidByCode(string code) //HCare-956
        {
            return _context.MedicalAids.Where(x => x.medicalAidCode == code).Where(x => x.Active == true).FirstOrDefault(); //hcare-1298
        }

        public MedicalAid GetMedicalAidByName(string name) //HCare-956
        {
            return _context.MedicalAids.Where(x => x.Name.ToLower() == name.ToLower()).Where(x => x.Active == true).FirstOrDefault(); //hcare-1298
        }

        public MedicalAidPlans GetMedicalAidPlanByCode(string code) //HCare-956
        {
            return _context.MedicalAidPlans.Where(x => x.planCode == code).Where(x => x.active == true).FirstOrDefault(); //hcare-1298
        }

        public MedicalAidPlans GetMedicalAidPlanByName(string name, string medicalaidID) //HCare-956
        {
            return _context.MedicalAidPlans.Where(x => x.Name.ToLower() == name.ToLower()).Where(x=>x.medicalAidId == new Guid(medicalaidID)).Where(x => x.active == true).FirstOrDefault(); //hcare-1298
        }
        public MedicalAidClaimCode GetMedicalClaimCodeByCode(string name) //HCare-1216
        {
            return _context.MedicalAidClaimCode.Where(x => x.claimCode.ToLower() == name.ToLower()).Where(x => x.Active == true).FirstOrDefault(); //hcare-1298
        }


        //================================================================================ Disposables =============================================================================//

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

    }
}