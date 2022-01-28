using HaloCareCore.Models;
using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Questionnaire;
using HaloCareCore.Models.Reports;
using HaloCareCore.Models.Script;
using HaloCareCore.Models.Service;
using HaloCareCore.Models.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace HaloCareCore.DAL
{
    public class OH17Context : DbContext, IDisposable
    {

        public OH17Context(DbContextOptions options) : base(options)
        {
        }

        public DbSet<MedicalAid> MedicalAids { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Dependant> Dependants { get; set; }
        public DbSet<Programs> Program { get; set; }
        public DbSet<Address> Address { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Origin> Origins { get; set; }
        public DbSet<Icd10Codes> Icd10Codes { get; set; }
        public DbSet<MemberInformation> MemberInformation { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<DependantType> DependantType { get; set; }
        public DbSet<Demographic> Demographic { get; set; }
        public DbSet<Sex> Sex { get; set; }
        public DbSet<Authorisations> Authorisations { get; set; }
        public DbSet<Scripts> Scripts { get; set; }
        public DbSet<ScriptItems> ScriptItems { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Assignments> Assignments { get; set; }
        public DbSet<AuthorizationLetters> AuthorizationLetters { get; set; }
        public DbSet<Notes> Notes { get; set; }
        public DbSet<Queries> Queries { get; set; }
        public DbSet<SmsMessages> SmsMessages { get; set; }
        public DbSet<SmsMessageTemplates> SmsMessageTemplates { get; set; }
        public DbSet<Doctors> Doctors { get; set; }
        public DbSet<Practices> Practices { get; set; }
        public DbSet<AssignmentItemTypes> AssignmentItemTypes { get; set; }
        public DbSet<AssignmentTypes> AssignmentTypes { get; set; }
        public DbSet<AttachmentTypes> AttachmentTypes { get; set; }
        public DbSet<Attachments> Attachments { get; set; }
        public DbSet<DoctorTypes> DoctorTypes { get; set; }
        public DbSet<ManagementStatus> ManagementStatus { get; set; }
        public DbSet<NoteTypes> NoteTypes { get; set; }
        public DbSet<QueryTypes> QueryTypes { get; set; }
        public DbSet<Pathology> Pathology { get; set; }
        public DbSet<CaseManagers> CaseManagers { get; set; }
        public DbSet<MedicalAidSettings> MedicalAidSettings { get; set; }
        public DbSet<AuthorisationCodes> AuthorisationCodes { get; set; }
        public DbSet<DoctorInformation> DoctorInformation { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<DependentPrograms> DependentPrograms { get; set; }
        public DbSet<Priorities> Priorities { get; set; }
        public DbSet<Emails> Emails { get; set; }
        public DbSet<ClaimsHistory> ClaimsHistory { get; set; }
        public DbSet<ScriptTypes> ScriptTypes { get; set; }
        public DbSet<HospitalizationClaims> HospitalizationClaims { get; set; }
        public DbSet<HospitalizationAuths> HospitalizationAuths { get; set; }
        public DbSet<HospitalizationClaimEvents> HospitalizationClaimEvents { get; set; }
        public DbSet<HospitalizationAuthEvents> HospitalizationAuthEvents { get; set; }
        public DbSet<Clinical> Clinical { get; set; }
        public DbSet<PatientManagementStatusHistory> PatientManagementStatusHistory { get; set; }
        public DbSet<CoMorbidTypes> CoMorbidTypes { get; set; }
        public DbSet<CoMormidConditions> CoMorbidConditions { get; set; }
        public DbSet<PathologyTypes> PathologyTypes { get; set; }
        public DbSet<ClinicalRules> ClinicalRules { get; set; }
        public DbSet<ClinicalRuleTypes> ClinicalRuleTypes { get; set; }
        public DbSet<ClinicalMessageInstructions> ClinicalMessageInstructions { get; set; }
        public DbSet<PatientClinicalRulesHistory> PatientClinicalRulesHistory { get; set; }
        public DbSet<AssignmentItems> AssignmentItems { get; set; }
        public DbSet<CoMorbidTypeRules> CoMorbidTypeRules { get; set; }
        public DbSet<CoMorbidClinicalRules> CoMorbidClinicalRules { get; set; }
        public DbSet<MedicalAidPlans> MedicalAidPlans { get; set; }
        public DbSet<PatientPlanHistory> PatientPlanHistory { get; set; }
        public DbSet<PayPoints> PayPoints { get; set; }
        public DbSet<PayPointHistory> PayPointHistory { get; set; }
        public DbSet<RoleAccess> RoleAccess { get; set; }
        public DbSet<UserRoleRights> UserRoleRights { get; set; }
        public DbSet<CaseManagerHistory> CaseManagerHistory { get; set; }
        public DbSet<PatientDoctorHistory> PatientDoctorHistory { get; set; }
        public DbSet<EnrolmentStepsMonitor> EnrolmentStepsMonitor { get; set; }
        public DbSet<ClinicalHistoryQuestionaire> ClinicalHistoryQuestionaire { get; set; }
        public DbSet<Allergies> Allergies { get; set; }
        public DbSet<MedicationHistory> MedicationHistory { get; set; }
        public DbSet<PatientProgramHistory> PatientProgramHistory { get; set; }
        public DbSet<BenefitType> BenefitType { get; set; }
        public DbSet<ScriptItemTypes> ScriptItemTypes { get; set; }
        public DbSet<RiskReportingData> RiskReportingData { get; set; }
        public DbSet<MedicalAidClaimCode> MedicalAidClaimCode { get; set; }
        public DbSet<EnquiryTypes> EnquiryTypes { get; set; }
        public DbSet<DispensingProviders> DispensingProviders { get; set; } //HCare-1474
        public DbSet<PatientDispensingProviderHistory> PatientDispensingProviderHistory { get; set; } //HCare-1474
        public DbSet<ScriptReferences> ScriptReferences { get; set; }
        public DbSet<AssignmentItemLogs> AssignmentItemLogs { get; set; }
        public DbSet<AssignmentItemTaskTypes> AssignmentItemTaskTypes { get; set; }
        public DbSet<AssignmentItemTasks> AssignmentItemTasks { get; set; }
        public DbSet<TaskTypeRequirements> TaskTypeRequirements { get; set; }
        public DbSet<EmailTemplates> EmailTemplates { get; set; }
        public DbSet<TaskRequirementItemLinking> TaskRequirementItemLinking { get; set; }
        public DbSet<QuestionnaireTemplate> QuestionnaireTemplates { get; set; }
        public DbSet<PatientQuestionnaireResponse> PatientQuestionnaireResponses { get; set; }
        public DbSet<Logins> Logins { get; set; }
        public DbSet<EnquiryComments> EnquiryComments { get; set; }
        public DbSet<DoctorQuestionnaireResults> DoctorQuestionnaireResults { get; set; }
        public DbSet<ForgottenPassword> ForgottenPasswords { get; set; }
        public DbSet<EducationalNotes> EducationalNotes { get; set; }
        public DbSet<MedicalAidPrograms> MedicalAidPrograms { get; set; }
        public DbSet<Podiatry> Podiatries { get; set; }
        public DbSet<AutoNeropathy> AutoNeropathies { get; set; }
        public DbSet<Vision> Visions { get; set; }
        public DbSet<Hypoglycaemia> Hypoglycaemias { get; set; }
        public DbSet<QuestionnaireOther> QuestionnaireOthers { get; set; }
        public DbSet<NewBorn> NewBorns { get; set; }
        public DbSet<PathologyIndex> PathologyIndex { get; set; }
        public DbSet<PathologyItems> PathologyItems { get; set; }
        public DbSet<PathologyTestTypes> PathologyTestTypes { get; set; }
        public DbSet<RiskRatingTypes> RiskRatingTypes { get; set; }
        public DbSet<PatientRiskRatingHistory> PatientRiskRatingHistory { get; set; }
        public DbSet<HypoglymiaRiskHistory> HypoglymiaRiskHistory { get; set; }
        public DbSet<LifeExpectancyRules> LifeExpectancyRules { get; set; }
        public DbSet<HypoRiskRules> HypoRiskRules { get; set; }
        public DbSet<PatientDisclaimer> PatientDisclaimers { get; set; }
        public DbSet<ProgramICD10Codes> ProgramICD10Codes { get; set; }
        public DbSet<ProgramStatuses> ProgramStatuses { get; set; }
        public DbSet<StatusTypes> StatusTypes { get; set; }
        public DbSet<PathologyFields> pathologyFields { get; set; }
        public DbSet<PatientAddressHistory> PatientAddressHistories { get; set; }
        public DbSet<PatientContactHistory> PatientContactHistories { get; set; }
        public DbSet<Laboratory> laboratories { get; set; }
        public DbSet<MembershipNoHistory> MembershipNoHistory { get; set; }
        public DbSet<MemberImports> MemberImports { get; set; }
        public DbSet<Accounts> Accounts { get; set; }
        public DbSet<AccountSchemes> AccountSchemes { get; set; }
        public DbSet<AccessServices> AccessServices { get; set; }
        public DbSet<SchemeAccess> SchemeAccess { get; set; }
        public DbSet<PreferredMethodOfContact> preferredMethodOfContacts { get; set; }
        public DbSet<MedicalAidPlanPrograms> MedicalAidPlanPrograms { get; set; }
        public DbSet<AccountPlanRules> AccountPlanRules { get; set; }
        public DbSet<AccountManagementStatuses> AccountManagementStatuses { get; set; }
        public DbSet<ManagementStatus_DeactivatedReasons> managementStatus_DeactivatedReasons { get; set; }
        public DbSet<UserSchemeAccess> UserSchemeAccess { get; set; }
        public DbSet<PatientStatusHistory> PatientStatusHistory { get; set; }
        public DbSet<PatientStatus> PatientStatus { get; set; }
        public DbSet<OtherMedicalHistory> otherMedicalHistories { get; set; }
        public DbSet<UserProgramAccess> UserProgramAccess { get; set; }
        public DbSet<UserRoleWorkflowRights> UserRoleWorkflowRights { get; set; }
        public DbSet<Log> Log { get; set; }
        public DbSet<HIVRiskRatingRules> HIVRiskRatingRules { get; set; }
        public DbSet<NonCLDFlags> NonCLDFlags { get; set; }

        public DbSet<EmailAttachments> EmailAttachments { get; set; }
        public DbSet<AccountTextTemplates> AccountTextTemplates { get; set; }
        public DbSet<AccountEmailTemplates> AccountEmailTemplates { get; set; }
        public DbSet<DiabeticDairy> DiabeticDairy { get; set; }
        public DbSet<DisclaimerResponse> DisclaimerResponse { get; set; }
        public DbSet<DisclaimerQuestions> DisclaimerQuestions { get; set; }
        public DbSet<MedicalAidAccount> MedicalAidAccounts { get; set; }
        public DbSet<MH_DSM5Response> MH_DSM5Responses { get; set; }
        public DbSet<MH_DSM5ResponseNEW> MH_DSM5ResponsesNEW { get; set; }
        public DbSet<MH_DSM5Score> MH_DSM5Scores { get; set; }
        public DbSet<MH_DSM5ScoreHistory> MH_DSM5ScoreHistories { get; set; }
        public DbSet<MH_SchizophreniaResponse> MH_SchizophreniaResponses { get; set; }
        public DbSet<MH_BipolarResponse> MH_BipolarResponses { get; set; }
        public DbSet<MH_DepressionResponse> MH_DepressionResponses { get; set; }
        public DbSet<ChronicMedication> ChronicMedication { get; set; }
        public DbSet<DoctorReferral> DoctorReferrals { get; set; }
        public DbSet<PatientProgramSubHistory> PatientProgramSubHistory { get; set; } //HCare-1095
        public DbSet<ComorbidConditionExclusions> ComorbidConditionExclusions { get; set; } //HCare-1095
        public DbSet<PatientStagingHistory> PatientStagingHistory { get; set; } //HCare-1014
        public DbSet<UserMemberHistory> UserMemberHistories { get; set; } //HCare-1176

        public DbSet<WorkflowSettings> WorkflowSettings { get; set; } //HCare-1134
        public DbSet<WorkflowLog> WorkflowLogs { get; set; } //HCare-1134

        public DbSet<wfSettings> wfSettings { get; set; } //HCare-1134
        public DbSet<wfUserQueue> wfUserQueues { get; set; } //HCare-1134
        public DbSet<wfQueue> wfQueues { get; set; } //HCare-1134
        public DbSet<wfUserLog> wfUserLogs { get; set; } //HCare-1134

        public DbSet<Hba1cRangeHistory> Hba1cRangeHistory { get; set; }//Hcare-1241
        public DbSet<HIVStagingRiskRules> HIVStagingRiskRules { get; set; }//HCare-1184
        public DbSet<MentalHealthDiagnosis> MentalHealthDiagnoses { get; set; }//HCare-1194
        public DbSet<DoctorsInformation> DoctorsInformation { get; set; }//HCare-1181


        #region diabetes care plan  1092
        public DbSet<CarePlanPathology> CarePlanPathologies { set; get; }
        public DbSet<NutritionAndLifestyle> nutritionAndLifestyles { set; get; }
        public DbSet<ClinicalAddition> clinicalAdditions { set; get; }
        public DbSet<Medicine> medicines { set; get; }
        public DbSet<Visit> visits { set; get; }
        public DbSet<paediatric> paediatrics { set; get; }
        public DbSet<MHRiskRatingRules> MHRiskRatingRules { get; set; }

        #endregion
        //HCare-1175
        public DbSet<Referral> referrals { get; set; }
        public DbSet<ReferralFrom> referralFroms { set; get; } //HCare-1144

        public DbSet<HospitalizationICD10types> HospitalizationICD10types { get; set; } //HCare-831

        public DbSet<PatientDiagnosis> PatientDiagnoses { get; set; } //hcare-1312

        public DbSet<EmployerGroups> EmployerGroups { get; set; }
        public DbSet<ProductionWorkItems> ProductionWorkItems { get; set; }

        public DbSet<HivcomorbidRules> HivcomorbidRules { set; get; } //HCare-1000
        public DbSet<HIVStages> HIVStages { get; set; }
        public DbSet<NextOFKin> NextOFKins { get; set; } //hcare-1361
        public DbSet<EmployerMaster> EmployerMasters { get; set; } //Hcare-923

        public DbSet<PathologyAssignments> PathologyAssignments { get; set; }

        public DbSet<InsulinDependance> InsulinDependances { get; set; } //hcare-673
        public DbSet<InsulinDependanceHistory> InsulinDependanceHistories { get; set; } //hcare-673
   
        public DbSet<Functions> Functions { set; get; }

        public DbSet<AccessFunctions> AccessFunctions { set; get; }

        public DbSet<ProfileLock> ProfileLocks { set; get; }

        public DbSet<ICD10Master> IC10Masters { set; get; } //HCare-199

        public DbSet<PopUpTemplate> PopUpTemplates { get; set; } //hcare-1374
        public DbSet<MemberRecord> MemberRecords { get; set; } //hcare-1374
        public DbSet<HC_Member_Claims> HC_Member_Claims { set; get; } //HCare-1362

        public DbSet<PathologyAutomatedTemplate> pathologyAutomatedTemplates { set; get; }
        public DbSet<AttachmentTemplate> AttachmentTemplates { get; set; } //hcare-1380
        public DbSet<AccountAttachmentTemplates> AccountAttachmentTemplates { get; set; } //hcare-1380
        public DbSet<EmailAttachmentHistory> emailAttachmentHistories { get; set; } //hcare-1380
        public DbSet<EmailLayout> EmailLayouts { get; set; } //hcare-1384
        public DbSet<AccountEmailLayout> AccountEmailLayouts { get; set; } //hcare-1384
        public DbSet<RiskRatingMonitor> RiskRatingMonitors { set; get; } //hcare-1395
        public DbSet<ICD10OnDischargeSetup> ICD10OnDischargeSetups { set; get; } //HCare-1481
        public DbSet<AssignmentLock> assignmentLocks { set; get; } //HCare-93
        public DbSet<TariffCode> TariffCodes { set; get; } //HCare-1017

        public DbSet<TariffClaimHistory> TariffClaimHistories { set; get; } //HCare-1016

        public DbSet<BulkEmail> BulkEmails { get; set; } //hcare-1483

        public DbSet<MedicalAidProgramIcd10codes> MedicalAidProgramIcd10codes { get; set; } //Hcare-1447

        public new ServiceResult SaveChanges()
        {
            var result = new ServiceResult() { Errors = new List<ServiceError>(), Success = true };
            try
            {
                foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified))
                {
                    // For each changed record, get the audit record entries and add them
                    foreach (Log x in GetAuditRecordsForChange(ent))
                    {
                        this.Log.Add(x);
                    }
                }
                base.SaveChanges();
            }
            catch (DbUpdateException dupe)
            {
                result.Errors.Add(new ServiceError { Message = dupe.InnerException.InnerException.Message });
                result.Success = false;
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Errors.Add(new ServiceError { Message = e.Message });
            }

            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This needs to go before the other rules!

            //modelBuilder.Entity<IdentityUser>().ToTable("Users").Property(p => p.Id).HasColumnName("UserID");
            //modelBuilder.Entity<ApplicationUser>().ToTable("Users").Property(p => p.Id).HasColumnName("UserID");

            //modelBuilder.Entity<IdentityUserRole>()
            //.ToTable("UserRole").HasKey(r => new { r.UserId, r.RoleId });

            // modelBuilder.Entity<IdentityRole>().ToTable("Roles").Property(p => p.Id).HasColumnName("Id");
            //  modelBuilder.Entity<IdentityUserLogin>().ToTable("Logins").HasKey(p => p.UserId);

            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }


        private List<Log> GetAuditRecordsForChange(EntityEntry dbEntry)
        {
            List<Log> result = new List<Log>();

            DateTime changeTime = DateTime.Now;

            // Get the Table() attribute, if one exists
            TableAttribute tableAttr = dbEntry.Entity.GetType().GetCustomAttributes(typeof(TableAttribute), false).SingleOrDefault() as TableAttribute;

            // Get table name (if it has a Table attribute, use that, otherwise get the pluralized name)
            string tableName = tableAttr != null ? tableAttr.Name : dbEntry.Entity.GetType().Name;
            string keyName = string.Empty;
            try
            {
                // Get primary key value (If you have more than one key column, this will need to be adjusted)
                keyName = dbEntry.Entity.GetType().GetProperties().Single(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0).Name;
            }
            catch
            {

            }

            if (dbEntry.State == EntityState.Added)
            {
                // For Inserts, just add the whole record
                // If the entity implements IDescribableEntity, use the description from Describe(), otherwise use ToString()
                result.Add(new Log()
                {
                    LogID = Guid.NewGuid(),
                    EventType = "A", // Added
                    TableName = tableName,
                    RecordID = string.IsNullOrEmpty(keyName) ? "NA" : dbEntry.CurrentValues.GetValue<object>(keyName).ToString(),  // Again, adjust this if you have a multi-column key
                    ColumnName = "*ALL",    // Or make it nullable, whatever you want
                    NewValue = (dbEntry.CurrentValues.ToObject() is IDescribableEntity) ? (dbEntry.CurrentValues.ToObject() as IDescribableEntity).Description : dbEntry.CurrentValues.ToObject().ToString(),
                    Created_by = "Logger",
                    Created_date = changeTime
                }
                    );
            }
            else if (dbEntry.State == EntityState.Deleted)
            {
                // Same with deletes, do the whole record, and use either the description from Describe() or ToString()
                result.Add(new Log()
                {
                    LogID = Guid.NewGuid(),
                    EventType = "D", // Deleted
                    TableName = tableName,
                    RecordID = string.IsNullOrEmpty(keyName) ? "NA" : dbEntry.CurrentValues.GetValue<object>(keyName).ToString(),
                    ColumnName = "*ALL",
                    NewValue = (dbEntry.OriginalValues.ToObject() is IDescribableEntity) ? (dbEntry.OriginalValues.ToObject() as IDescribableEntity).Description : dbEntry.OriginalValues.ToObject().ToString(),
                    Created_by = "Logger",
                    Created_date = changeTime
                }
                    );
            }
            else if (dbEntry.State == EntityState.Modified)
            {
                foreach (var propertyName in dbEntry.Entity.GetType().GetTypeInfo().DeclaredProperties)
                {
                    // For updates, we only want to capture the columns that actually changed
                    if (!object.Equals(dbEntry.OriginalValues.GetValue<object>(propertyName.Name), dbEntry.CurrentValues.GetValue<object>(propertyName.Name)))
                    {
                        result.Add(new Log()
                        {
                            LogID = Guid.NewGuid(),
                            EventType = "M",    // Modified
                            TableName = tableName,
                            RecordID = string.IsNullOrEmpty(keyName) ? "NA" : dbEntry.CurrentValues.GetValue<object>(keyName).ToString(),
                            ColumnName = propertyName.Name,
                            OriginalValue = dbEntry.OriginalValues.GetValue<object>(propertyName.Name) == null ? null : dbEntry.OriginalValues.GetValue<object>(propertyName.Name).ToString(),
                            NewValue = dbEntry.CurrentValues.GetValue<object>(propertyName.Name) == null ? null : dbEntry.CurrentValues.GetValue<object>(propertyName.Name).ToString(),
                            Created_by = "Logger",
                            Created_date = changeTime
                        }
                            );
                    }
                }
            }
            // Otherwise, don't do anything, we don't care about Unchanged or Detached entities

            //hc.data.warehouse.log
            //if (dbEntry.State == EntityState.Added || dbEntry.State == EntityState.Modified || dbEntry.State == EntityState.Deleted)
            //{
            //    result.Add(new Log()
            //    {
            //        LogID = Guid.NewGuid(),
            //        EventType = "A", // Added
            //        TableName = tableName,
            //        RecordID = string.IsNullOrEmpty(keyName) ? "NA" : dbEntry.CurrentValues.GetValue<object>(keyName).ToString(),  // Again, adjust this if you have a multi-column key
            //        ColumnName = "*ALL",    // Or make it nullable, whatever you want
            //        NewValue = (dbEntry.CurrentValues.ToObject() is IDescribableEntity) ? (dbEntry.CurrentValues.ToObject() as IDescribableEntity).Description : dbEntry.CurrentValues.ToObject().ToString(),
            //        Created_by = "Logger",
            //        Created_date = changeTime
            //    });

            //}





            return result;
        }

        public interface IDescribableEntity
        {
            [StringLength(200, MinimumLength = 4)]
            string Title { get; set; }

            [StringLength(5000, MinimumLength = 4)]
            string Description { get; set; }

            [StringLength(400, MinimumLength = 4)]
            string Summary { get; set; }
        }
        public DbSet<WorkflowUser> WorkflowUsers { get; set; }

    }
}