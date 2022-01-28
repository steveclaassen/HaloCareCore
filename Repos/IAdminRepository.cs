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
using System;
using System.Collections.Generic;

namespace HaloCareCore.Repos
{
    interface IAdminRepository : IDisposable
    {
        bool ValidateUser(string username, string password);
        bool CheckSessionLogin(Logins model);
        Users GetUserByUsername(string username);
        bool IsYourLoginStillTrue(string UserId, string SessionId);
        bool IsUserLoggedOnElsewhere(string UserId, string SessionId);
        void LogEveryoneElseOut(string UserId, string SessionId);
        UserAccessViewModel GetUserRights(Guid userId);
        CaseManagers GetCaseManager(string managerNo);
        CaseManagers GetCaseManagerByNumber(string number); //hcare-956
        CaseManagers GetCaseManagerByName(string first, string last); //hcare-956

        string InsertCaseManager(CaseManagers caseManager);
        void InsertCaseManagerHistory(CaseManagerHistory model);
        List<CaseManagers> GetCaseManagers();
        List<CaseManagerHistoryView> GetCaseManagerHistories(Guid depId);
        List<Products> GetProducts();
        List<Products> GetProductsSearch(string nappi, string prodname);
        //HCare-1095
        List<ComorbidConditionExclusions> GetComorbidsSearch(string icd10, string descrip);
        List<ComorbidConditionExclusions> GetICD10ConditionList();
        List<ComorbidConditionExclusions> GetICD10Conditions();
        List<ComorbidConditionExclusions> GetICD10ConditionsByMappingCode(string mappingcode);
        ServiceResult InsertComorbidExclusion(ComorbidConditionExclusions model);
        ServiceResult UpdateComorbidExclusion(ComorbidConditionExclusions model);
        ComorbidConditionExclusions GetComorbidExclusion(int id);
        List<ComorbidConditionExclusions> GetComorbidInformation(string code);
        List<ComorbidConditionExclusions> GetComorbidInfoByName(string name, Guid dependantID);
        ComorbidConditionExclusions GetComobidByICD10(string ICD10Code); //HCare-956

        List<BenefitType> GetBenefitTypes();
        Products GetProduct(string nappiCode);
        Products GetNappiCode(string productName);
        ServiceResult UpdateProduct(Products model);
        Products GetProductByName(string productName);
        Products GetProductByNappi(string NappiCode);
        bool InsertPoduct(Products model);
        List<SmsMessageTemplates> GetSmsMessageTemplates();
        List<SmsMessageTemplates> GetSMSTemplates();
        List<Language> GetsmsLanguage();
        SmsMessageTemplates GetSmsMessageTemplate(int templateID);
        EmailTemplates GetEmailTemplateByID(int templateID);
        bool InsertSmsMessageTemplate(SmsMessageTemplates smsTemplates);
        bool InsertEmailTemplate(EmailTemplates emailTemplates);
        bool GetSMSTemplate(SmsMessageTemplates model);
        bool GetEmailTemplate(EmailTemplates model);
        List<EmailTemplates> GetEmailTemplates();

        List<DoctorTypes> GetDoctorTypes();
        Practices GetPractice(string practiceNo);
        List<Practices> GetPractices();
        bool InsertPractice(Practices practice);
        Doctors GetDoctor(Guid? doctorID);
        Doctors GetDoctor(string surname, string practiceNo);
        DoctorViewModel GetDoctorByPractice(string practiceno);
        List<Doctors> GetDoctors();
        List<Doctors> SearchDoctors(string searchvar);
        Guid InsertDoctor(Doctors doctor);
        DoctorViewModel GetDoctorInformation(Guid id);
        DoctorViewModel GetDoctorsInformationEdit(Guid id); //HCare-1181
        DoctorViewModel GetDoctorsInformationDetails(Guid id); //HCare-1181
        List<Doctors> SearchDoctors(DoctorSearchVars model);
        DoctorInformation GetDoctorInfo(Guid id);
        bool InsertDoctorInformation(DoctorInformation info);
        ServiceResult UpdateDrInfo(DoctorInformation info);
        ServiceResult UpdatetheDoctor(DoctorViewModel model);
        ServiceResult UpdateDoctorsInformation(DoctorViewModel model);
        ServiceResult UpdateDrAddress(DoctorViewModel model);
        ServiceResult UpdateDrContact(DoctorViewModel model);
        Users GetUserByFullName(string firstname, string lastname);
        Users GetUserById(Guid id);
        List<Users> GetUsersList();
        List<Users> GetAllowedUsersList(Guid DependantID, Guid ProgramID);
        List<UserMemberHistory> GetUserMemberHistoryList(Guid dependantID, Guid program); //HCare-1176
        ServiceResult InsertUserMemberHistory(UserMemberHistory model); //HCare-1176
        ServiceResult UpdateUserMemberHistory(UserMemberHistory model); //HCare-1176
        List<UserMemberHistory> GetUserMemberHistory(Guid dependantID, Guid program); //HCare-1176
        UserMemberHistory GetUserMemberByID(int id, Guid pro); //HCare-1176
        UserRoleWorkflowRights GetWorkFlowById(int Id);
        ServiceResult AddUserRole(UserRole userRole);
        ServiceResult AddNewUser(Users user, Guid roleId);
        ServiceResult AddNewRole(Roles role);
        ServiceResult UpdateRole(Roles model);
        ServiceResult UpdateWorkflow(UserRoleWorkflowRights model);
        ServiceResult AddNewRoleLevel(Roles role);
        ServiceResult AddNewRoleWorkFlow(UserRoleWorkflowRights model);
        ServiceResult AddUserSchemeAccess(UserSchemeAccess userAccess);
        ServiceResult AddUserProgramAccess(UserProgramAccess userAccess);
        List<UserSchemeAccess> GetUserAccess(Guid userId);
        List<UserSchemeAccess> GetUserAccess(Guid userId, Guid medicalAidID);
        Roles GetRoleById(Guid roleId);
        Roles GetRoleByName(string role);
        UserRole GetUserRoleById(Guid userId);
        List<Roles> GetUserRoles();
        ServiceResult AddUser(Users user);
        ServiceResult UpdateSmsTemplate(SmsMessageTemplates model);
        ServiceResult UpdateEmailTemplate(EmailTemplates model);
        ServiceResult UpdateAccountTextTemplate(AccountTextTemplates model);
        ServiceResult UpdateAccountEmailTemplate(AccountEmailTemplates model);
        List<Users> GetUsers();
        ServiceResult UpdateDoctor(Doctors doctor);
        string GetRoleUserId(Guid userId);
        ServiceResult UpdateUser(Users user, string RoleId = null);
        ServiceResult UpdateUser(Guid userId, string pasw, string email, bool active, bool searchmanagment, string RoleId = null);
        ServiceResult EditDoctor(Doctors doctors);
        ServiceResult DeleteDoctor(Doctors doctors);
        List<Scripts> GetUnAthourisedScripts();
        List<Scripts> GetRecentScripts();
        List<Scripts> GetOutstandingScripts();
        List<Pathology> GetOutstandingPathology();
        List<OutstandingsView> GetOutstandingScriptsView();
        List<OutstandingsView> GetOutstandingPathologiesView();
        List<UnAuthsScripts> GetOutstandingAuthorisedScripts();
        bool GetUserByUsernameEmail(string inputstr);
        List<AssignmentItemTaskTypes> GetAssignmentItemTaskTypes();
        ServiceResult AddNewTaskType(AssignmentItemTaskTypes assignmentItemTaskTypes);
        AssignmentItemTaskTypes GetTaskTypeById(string taskId);
        ServiceResult UpdateTaskType(AssignmentItemTaskTypes model);
        List<TaskTypeRequirements> GetTaskTypeRequirements();
        List<TaskTypeRequirements> GetTaskTypeRequirementValidation();
        ServiceResult AddNewTaskReq(TaskTypeRequirements model);
        TaskTypeRequirements GetTaskReqById(int id);
        ServiceResult UpdateTaskReq(TaskTypeRequirements model);
        List<AssignmentTypes> GetAssignmentTypes();
        List<AssignmentTypes> GetAssignmentTypeValidation();
        List<WorkflowViewModel> GetAssignmentItemTypes();
        List<WorkflowViewModel> GetAssignmentItemTypes(Guid Id);
        ServiceResult AddNewAssignmentType(AssignmentTypes model);
        List<HospitalizationICD10types> GetHospICD10sByProgram(string programcode);
        ServiceResult AddNewHospICD10(HospitalizationICD10types model); //hcare-831
        HospitalizationICD10types GetHospICD10ById(int id); //Hcare-831
        ServiceResult UpdateHospICD10(HospitalizationICD10types model); //Hcare-831
        AssignmentTypes GetAssignmentTypeById(string assignmentType);
        ServiceResult UpdateAssignmentType(AssignmentTypes model);
        List<AssignmentItemTypes> GetAssignmentItems();
        List<AssignmentItemTypes> GetAssignmentItemValidation();
        List<AssignmentItemTypes> GetAssignmentItemsValidation();
        ServiceResult AddNewAssignmentItem(AssignmentItemTypes model);
        AssignmentItemTypes GetAssignmentItemById(string assignmentItemType);
        ServiceResult UpdateAssignmentItem(AssignmentItemTypes model);
        List<SmsMessageTemplates> GetSMSmessageTemplate();
        List<EmailTemplates> GetemailTemplate();
        ServiceResult InsertForgotPassword(ForgottenPassword model);
        ServiceResult UpdateForgotPassword(ForgottenPassword model);
        ForgottenPassword GetForgotPasswordById(string key);
        ForgottenPassword GetfPassUserId(Guid id);
        Users GetfPassUser(Guid id);
        List<TaskRequirementItemLinking> GetLinkedTasks();
        List<TaskRequirementItemLinking> GetLinkedTasksByID(string itemID);
        List<HospitalizationICD10types> GetHospICD10s(); //Hcare-831
        ServiceResult AddLinkedTasks(TaskRequirementItemLinking model);
        TaskRequirementItemLinking GetLinkedTaskById(int id);
        ServiceResult UpdateLinkedTasks(TaskRequirementItemLinking model);
        List<EducationalNotes> GetEducationalNote();
        ServiceResult AddNewEducationalNote(EducationalNotes model);
        EducationalNotes GetEducationalNoteById(int id);
        ServiceResult UpdateEducationalNote(EducationalNotes model);
        List<HypoRiskRules> GetHypoRiskRules();
        bool GetHypoRiskRule(HypoRiskRules model);
        List<MHRiskRatingRules> GetMHRiskRatingRules();
        ServiceResult AddHypoRiskRules(HypoRiskRules model);
        HypoRiskRules GetHypoRiskRulesById(int riskId);
        ServiceResult UpdateHypoRiskRules(HypoRiskRules model);
        //HCare-840
        List<HIVRiskRatingRules> GetHIVRiskRules();
        bool GetHIVRiskRule(HIVRiskRatingRules model);
        ServiceResult AddHIVRiskRules(HIVRiskRatingRules model);
        HIVRiskRatingRules GetHIVRiskRulesById(int riskId);
        ServiceResult UpdateHIVRiskRules(HIVRiskRatingRules model);
        //HCare-1184
        List<HIVStagingRiskRules> GetHIVStageRiskRules();
        bool GetHIVStageRiskRule(HIVStagingRiskRules model);
        ServiceResult AddHIVStageRiskRules(HIVStagingRiskRules model);
        HIVStagingRiskRules GetHIVStageRiskRulesById(int riskId);
        ServiceResult UpdateHIVStageRiskRules(HIVStagingRiskRules model);
        //end
        List<LifeExpectancyRules> GetLifeRules();
        bool GetLifeRule(LifeExpectancyRules model);
        ServiceResult AddLifeRules(LifeExpectancyRules model);
        LifeExpectancyRules GetLifeRulesById(int riskId);
        ServiceResult UpdateLifeRules(LifeExpectancyRules model);
        bool GetMHRule(MHRiskRatingRules model);
        ServiceResult AddMHRules(MHRiskRatingRules model);
        MHRiskRatingRules GetMHRulesById(int riskId);
        ServiceResult UpdateMHRules(MHRiskRatingRules model);
        List<ClinicalRules> GetClinicalRiskRules();
        List<ClinicalRules> GetClinicalRiskRuleValidation();
        List<ClinicalRules> GetClinicalRiskRulesList();
        ClinicalRules GetClinicalRuleByName(string clinicalRule);
        ServiceResult AddClinicalRules(ClinicalRules model);
        ClinicalRules GetClinicalRulesById(int id);
        ClinicalRules GetClinicalRulesInfo(int id);
        ServiceResult UpdateClinicalRules(ClinicalRules model);
        List<ProgramStatuses> GetProgramStatuses();
        ServiceResult AddProgramStatus(ProgramStatuses model);
        ProgramStatuses GetProgramStatusessById(int id);
        ServiceResult UpdateProgramStatuses(ProgramStatuses model);
        List<StatusTypes> GetStatusTypes();
        List<Accounts> GetAccounts();
        AccountDetailsViewModel GetAccountDetails(Guid Id);
        List<Laboratory> GetListOfLaboratories();
        Laboratory GetLaboratorybyName(string name);
        bool GetLaboratoryName(Laboratory model);
        ServiceResult InsertLaboratory(Laboratory model);
        Laboratory GetLaboratoryById(int id);
        ServiceResult UpdateLaboratory(Laboratory model);
        Guid InsertAccount(Accounts account);
        Accounts GetAccountByName(string name);
        List<MedicalAid> GetNonLinkedSchemes();
        AccountSchemes GetAccountSchemeById(Guid id);
        ServiceResult InsertAccountSchemes(AccountSchemes scheme);
        List<MedicalAidSetupViewModal> GetAccountMedicalAids(Guid accountID);
        ServiceResult InsertAccountPlanRules(AccountPlanRules model);
        ServiceResult InsertAccountManagementStatuses(AccountManagementStatuses model);
        //Hcare-1043
        ServiceResult InsertAccountTextTemplates(AccountTextTemplates model);
        ServiceResult InsertAccountEmailTemplates(AccountEmailTemplates model);
        MedicalAidSetupViewModal GetAccountMedicalAid(Guid Id);
        ServiceResult UpdateAccountSchemes(AccountSchemes scheme);
        MedicalAidPlanPrograms GetPlanProgramByID(Guid planID);
        ServiceResult DeactivateProgramPlans(MedicalAidPlanPrograms model);
        ServiceResult InsertMedicalAidPlanProgram(MedicalAidPlanPrograms prog);
        List<MedicalAidPlanPrograms> GetPlanProgramByScheme(Guid medicalAidId);
        List<ClinicalRulesSetupViewModal> GetClinicalRiskRulesSetup(Guid AccountId, Guid progID, Guid medaid);
        List<ClinicalRulesSetupViewModal> GetClinicalRiskRulesEdit(Guid AccountId, Guid progID, Guid medaid);
        ClinicalRulesSetupViewModal GetAccountClinicalRiskRules(Guid AccountId, Guid progID, Guid medaid, int ruleid);
        List<ClinicalRulesSetupInitialViewModel> GetClinicalRiskRulesSelection(Guid AccountId, Guid medId);
        List<ClinicalRules> GetClinicalRulesByScheme(Guid MedicalAidId);
        //HCare-1087
        List<MedicalAidProgramViewModel> GetMedicalAidPrograms(Guid MedicalAidId);
        MedicalAidPrograms GetMedicalAidProgram(Guid MedicalAidId, string program);
        ServiceResult UpdateMedicalAidPrograms(MedicalAidPrograms model);
        ServiceResult InsertMedicalAidProgram(MedicalAidPrograms prog);
        ServiceResult UpdateClinicalRiskRules(AccountPlanRules model);
        ServiceResult UpdateStatusRules(AccountManagementStatuses model);
        ServiceResult UpdateTextTemplate(AccountTextTemplates model);
        ServiceResult UpdateMedicalAidPlanPrograms(MedicalAidPlanPrograms model);
        List<StatusManagmentViewModel> GetManagementStatusSetup(Guid AccountId, Guid program);
        List<TextTemplateViewModel> GetTextTemplateSetup(Guid AccountId, Guid program);
        List<StatusManagmentViewModel> GetManagementStatusEdit(Guid AccountId, Guid program, Guid medAid);
        StatusManagmentViewModel GetAccountManagementStatus(Guid AccountId, Guid program, Guid medAid, string code);
        TextTemplateViewModel GetAccountTextTemplate(Guid AccountId, Guid program, Guid medAid, int templateId);
        EmailTemplateViewModel GetAccountEmailTemplate(Guid AccountId, Guid program, Guid medAid, int templateId);
        List<TextTemplateViewModel> GetTextTemplatesEdit(Guid AccountId, Guid program, Guid medAid);
        List<EmailTemplateViewModel> GetEmailTemplatesEdit(Guid AccountId, Guid program, Guid medAid);
        List<PlanCheckProgramViewModel> GetMedicalAidPlanPrograms(Guid accountID, Guid medId);
        List<PlanCheckProgramViewModel> GetMedicalAidPlanProgramsEdit(Guid AccountId, Guid optionID);
        List<PlanCheckProgramViewModel> GetMedicalAidPlanProgramsEd(Guid AccountId, Guid optionID);
        //HCare-1083
        List<ChronicMedication> GetChronicMedication();
        List<ChronicMedication> GetChronicMedicationValidation();
        ChronicMedication GetChronicMedicationById(Guid id);
        ChronicMedication GetChronicMedicationByCode(string code);
        ChronicMedication GetChronicMedicationByName(string description);
        ServiceResult InsertChronicMedication(ChronicMedication model);
        ServiceResult UpdateChronicMedication(ChronicMedication model);
        //Hcare-1083 end
        List<PreferredMethodOfContact> GetListofPMOC();
        List<PreferredMethodOfContact> GetPMOCValidation();
        PreferredMethodOfContact GetPMOCbyCode(string code);
        PreferredMethodOfContact GetPMOCbyName(string name);
        ServiceResult InsertPMOC(PreferredMethodOfContact model);
        PreferredMethodOfContact GetPMOCById(int id);
        ServiceResult UpdatePMOC(PreferredMethodOfContact model);
        List<Programs> GetPrograms();
        List<Programs> GetProgramValidation();
        Programs GetProgrambyName(string name);
        Programs GetProgrambyCode(string code);
        Programs GetProgramByICD10(string icd10code);
        ServiceResult InsertProgrm(Programs model);
        Programs GetProgramById(Guid id);
        ServiceResult UpdateProgrm(Programs model);
        List<UserProgramViewModal> GetUserProgramAccess(Guid userID, Guid medID);
        UserProgramAccess GetUserProgramAccessByProgram(Guid userID, Guid medId, Guid programId);
        List<UserSchemeAccessViewModel> GetUsermedicalAidAccess(Guid userID);
        UserSchemeAccess GetAccessByMedicalAidAndUserID(string medicalaidID, string userID);
        ServiceResult UpdateUserProgramAccess(UserProgramAccess model);
        ServiceResult UpdateUserSchemeAccess(UserSchemeAccess model);
        List<Log> GetLogs();//HCare-134
        List<Log> GetLogs(string searchVar);//HCare-134
        List<Icd10Codes> GetListofICD10Codes(); //hcare-1280
        List<Icd10Codes> GetICD10CodeValidation(); //hcare-1280
        ServiceResult InsertICD10Code(Icd10Codes model); //hcare-1280
        ServiceResult UpdateICD10Code(Icd10Codes model); //hcare-1280
        Icd10Codes GetICD10CodeByID(Guid id); //hcare-1280
        Icd10Codes GetICD10ByProgram(string id); //hcare-1280
        #region HCare-1060
        List<NonCLDFlags> GetNonCDLFlags();
        bool GetNonCDLFlag(NonCLDFlags model);
        List<NonCLDFlags> GetNonCDLFlagsList();
        NonCLDFlags GetNonCDLFlagsByName(string name);
        NonCLDFlags GetNonCDLFlagsByCode(string code, string programcode);
        ServiceResult InserNonCDLFlags(NonCLDFlags model);
        NonCLDFlags GetNonCDLFlagsByID(int id);
        ServiceResult UpdateNonCDLFlags(NonCLDFlags model);
        #endregion
        #region disclaimer HCare-864
        List<DisclaimerQuestions> GetDisclaimerQuestions();
        List<DisclaimerQuestions> GetAcknowledgementQuestions();
        ServiceResult InsertDisclaimerQ(DisclaimerQuestions model);
        DisclaimerQuestions GetDisclaimerQById(int id);
        ServiceResult UpdateDisclaimerQ(DisclaimerQuestions model);
        ServiceResult InsertDisclaimerResponse(DisclaimerResponse model);
        List<DisclaimerResponse> GetDisclaimerResponse(Guid dependentid);
        List<DisclaimerFullView> GetDisclaimerResults(Guid dependentid, Guid programcode);
        #endregion
        #region dsm5 HCare-1123
        ServiceResult InsertDSM5Results(MH_DSM5Response model);
        MH_DSM5Response GetDSM5ById(int id);
        ServiceResult UpdateDSM5Result(MH_DSM5Response model);
        List<MH_DSM5Response> GetDSM5Results(Guid dependentid);
        #endregion
        #region schizophrenia HCare-1124
        ServiceResult InsertSchizophreniaResults(MH_SchizophreniaResponse model);
        MH_SchizophreniaResponse GetSchizophreniaById(int id);
        ServiceResult UpdateSchizophreniaResult(MH_SchizophreniaResponse model);
        List<MH_SchizophreniaResponse> GetSchizophreniaResults(Guid dependentid);
        #endregion
        #region bipolar HCare-1125
        ServiceResult InsertBipolarResults(MH_BipolarResponse model);
        MH_BipolarResponse GetBipolarById(int id);
        ServiceResult UpdateBipolarResult(MH_BipolarResponse model);
        List<MH_BipolarResponse> GetBipolarResults(Guid dependentid);
        #endregion
        #region depression HCare-1126
        ServiceResult InsertDepressionResults(MH_DepressionResponse model);
        MH_DepressionResponse GetDepressionById(int id);
        ServiceResult UpdateDepressionResult(MH_DepressionResponse model);
        List<MH_DepressionResponse> GetDepressionResults(Guid dependentid);
        #endregion
        #region attachment-types HCare-1154
        ServiceResult InsertAttachmentType(AttachmentTypes model);
        AttachmentTypes GetAttachmentTypeById(string id);
        ServiceResult UpdateAttachmentType(AttachmentTypes model);
        List<AttachmentTypes> GetAttachmentTypes();
        List<AttachmentTypes> GetAttachmentTypeValidation(); //hcare-1298
        AttachmentTypes GetAttachmentTypeByCode(string code);
        AttachmentTypes GetAttachmentTypeByName(string name);
        #endregion
        #region New DSM5 HCare-1205
        ServiceResult InsertNEWDSM5Results(MH_DSM5ResponseNEW model);
        MH_DSM5ResponseNEW GetNEWDSM5ById(int id);
        ServiceResult UpdateNEWDSM5Result(MH_DSM5ResponseNEW model);
        List<MH_DSM5ResponseNEW> GetNEWDSM5Results(Guid dependentid);
        #endregion
        #region DSM5 Score HCare-1206
        List<MH_DSM5Score> GetDSM5ScoreHistory(Guid dependentid);
        ServiceResult InsertDSM5Score(MH_DSM5Score model);
        MH_DSM5Score GetDSM5ScoreByID(int id);
        DSM5ScoreVM GetDSM5ScoreInformation(int id);
        List<DSM5ScoreVM> GetDSM5ScoreDetails(int id);
        ServiceResult UpdateDSM5Score(MH_DSM5Score model);
        ServiceResult InsertDSM5ScoreHistory(MH_DSM5ScoreHistory model);
        #endregion
        SmsMessageTemplates GetTextTemplateByTitle(string title); //HCare-956
        SmsMessageTemplates GetTextTemplateByMessage(string template); //HCare-956
        EmailTemplates GetEmailTemplateByTitle(string title); //HCare-956
        AssignmentTypes GetAssignmentTypeByCode(string code); //HCare-956
        AssignmentTypes GetAssignmentTypeByName(string description); //HCare-956
        AssignmentItemTypes GetAssignmentItemTypeByCode(string assignmentItemType); //HCare-956
        AssignmentItemTypes GetAssignmentItemTypeByName(string itemDescription); //HCare-956
        TaskTypeRequirements GetTaskRequirementByName(string requirementText); //HCare-956
        #region hcare-1134
        //List<WorkflowSettings> GetWorkFlowSettings();
        List<wfSettings> GetWFSettings();
        List<wfUserQueue> GetWFUserQueueList();
        wfUserQueue GetWFUserQueueByQueueID(Guid queueID);
        List<WorkflowVM> GetWorkFlowList(); //HCare-1134
        List<wfQueueVM> GetWFUserQueue(); //HCare-1134
        ServiceResult InsertWorkflowSetting(WorkflowSettings model);
        ServiceResult UpdateWorkflowSetting(WorkflowSettings model);
        WorkflowSettings GetWorkflowSettingById(int id);
        wfViewModel GetWorkflowSettingsById(Guid id);
        wfViewModel GetWorkflowSettingDetails(Guid id);
        List<wfViewModel> GetWorkflowSettingsByQueue(Guid queueid);
        WorkflowSettingsVM GetWFSettingsVM(int id);
        List<wfSettings> GetUserQueues();
        ServiceResult UpdateWFUserID(int id, Guid? userID);
        ServiceResult UpdateUserQueueShuffleDate(Guid userID, Guid queueID);
        List<wfViewModel> GetWorkFlowSettings();
        wfSettings GetWorkFlowSettingInformation(Guid medicalaidID, Guid programID, string managmentStatus, string riskRating);
        ServiceResult InsertWFSetting(wfSettings model);
        ServiceResult UpdateWFSetting(wfSettings model);
        List<wfSettings> DeleteWFSetting(Guid queueID);
        List<wfUserQueue> GetUserQueueList();
        wfUserQueue GetUserQueueListByQueueID(Guid queueID);
        ServiceResult InsertWFUserQueue(wfUserQueue model);
        ServiceResult DeleteWFUserQueue(Guid queueID);
        ServiceResult UpdateWFUserQueue(wfUserQueue model);
        wfQueueVM GetWFUserQueueById(int id);
        List<string> GetUserInRole(string username); //HCare-1250
        #endregion
        //HCare-1250
        bool IsUserInRole(string username, string roleName);
        bool IsUserHasRoleAccess(string username);
        List<HivcomorbidRules> GetHivcomorbidRules();
        ServiceResult InsertHivComorbidRules(HivcomorbidRules hivcomorbidRules);
        List<HIVStages> GetHIVStages(); //HCare-1000
        List<ComorbidConditionExclusions> GetComorbidConditionExclusions(); //HCare-1000
        List<HivcomorbidRules> GetHivComorbidRulesValidation(); //HCare-1440
        ServiceResult UpdateHivComorbidRules(HivcomorbidRules hivcomorbidRules);
        HivcomorbidRules GetHivComorbidRules(int hivcomorbidRules);
        string GetHivComorbidRulesByName(string Name);
        List<MedicalAidVM> GetMedicalAidInformation(Guid userID); //hcare-1346
        List<ICD10Master> GetICD10MasteSearch(string ICD10Code, string ICD10Description); //HCare-199
        ICD10Master GetICD10MasterById(string ICD10Code); //HCare-199
        List<ManagementStatus> GetManagementStatusList(); //hcare-1363
        List<Log> GetSystemLogTables(); //hcare-1442
        List<Log> GetSystemLogColumns(string table); //hcare-1442
        List<Log> GetSystemLogRecords(string table); //hcare-1442
        List<SystemLogResultsVM> GetSystemLogSearchResults(string table, string column, string recordID, string currentValue, string createdDate); //hcare-1442
        #region hcare-1374: template-pop-ups
        List<PopUpTemplate> GetPopUpTemplates();
        List<PopUpTemplate> GetPopUpTemplateList();
        ServiceResult InsertPopUpTemplate(PopUpTemplate model);
        ServiceResult UpdatePopUpTemplate(PopUpTemplate model);
        List<PopUpTemplate> GetPopUpTemplateByMemberInformation(Guid medicalaidID, string option, Guid programID);
        PopUpTemplate GetPopUpTemplateByID(Guid id);
        PopUpTemplate GetPopUpTemplateDetails(Guid id);
        #endregion
        List<RiskRatingMonitor> GetRiskRatingMonitorIndex(); //hcare-1395
        List<RiskRatingMonitor> GetRiskRatingMonitorValidation(); //hcare-1395
        RiskRatingMonitor GetRiskRatingMonitorByID(Guid id); //hcare-1395
        RiskRatingMonitor GetRiskRatingMonitorDetailsByID(Guid id); //hcare-1395
        RiskRatingMonitor GetRiskRatingMonitorDetails(Guid id); //hcare-1395
        ServiceResult InsertRiskRatingMonitor(RiskRatingMonitor model); //hcare-1395
        ServiceResult UpdateRiskRatingMonitor(RiskRatingMonitor model); //hcare-1395
        RiskRatingMonitor GetRiskRatingMonitorByName(string name); //hcare-1395
        List<AttachmentTemplate> GetAttachmentTemplateIndex(); //hcare-1380
        List<AttachmentTemplate> GetAttachmentTemplateValidation(); //hcare-1380
        List<AttachmentTemplate> GetAttachmentTemplateByAccount(Guid medicalaidID, Guid programID); //hcare-1380
        AttachmentTemplate GetAttachmentTemplateByID(Guid id); //hcare-1380
        AttachmentTemplate GetAttachmentTemplateDetails(Guid id); //hcare-1380
        AttachmentTemplate GetTemplateByName(string name); //hcare-1380
        List<AttachmentTemplateVM> GetAttachmentTemplatesEdit(Guid accountID, Guid programID, Guid medicalaidID); //hcare-1380
        AttachmentTemplateVM GetAccountAttachmentTemplate(Guid accountID, Guid programID, Guid medicalaidID, Guid templateID); //hcare-1380
        ServiceResult InsertAttachmentTemplate(AttachmentTemplate model); //hcare-1380
        ServiceResult UpdateAttachmentTemplate(AttachmentTemplate model); //hcare-1380
        EmailVM GetEmailDetailByID(int id); //hcare-1380
        ServiceResult InsertAccountAttachmentTemplate(AccountAttachmentTemplates model); //hcare-1380
        ServiceResult UpdateAccountAttachmentTemplate(AccountAttachmentTemplates model); //hcare-1380
        List<EmailAttachmentHistory> GetEmailAttachmentByDependantID(Guid dependantid, Guid programid, DateTime today); //hcare-1380
        List<EmailAttachmentHistory> GetEmailAttachmentHistoryByEmailID(int id); //hcare-1380
        EmailAttachmentHistory GetEmailAttachmentByID(Guid templateid, Guid dependantid, Guid programid); //hcare-1380
        ServiceResult InsertEmailAttachmentHistory(EmailAttachmentHistory model); //hcare-1380
        ServiceResult UpdateEmailAttachmentHistory(EmailAttachmentHistory model); //hcare-1380
        ServiceResult ResetAttachmentEmailHistory(Guid dependantid, Guid programid); //hcare-1380
        SmsMessages GetTextMessageByID(int id); //hcare-1378
        List<EmailLayout> GetEmailLayoutIndex(); //hcare-1384
        List<EmailLayout> GetEmailLayoutValidation(); //hcare-1384
        EmailLayout GetEmailLayoutByID(Guid id); //hcare-1384
        EmailLayout GetEmailLayoutDetails(Guid id); //hcare-1384
        EmailLayout GetEmailLayoutByName(string name); //hcare-1384
        List<EmailLayout> GetEmailLayoutByAccount(Guid medicalaidID, Guid programID); //hcare-1384
        List<EmailLayoutVM> GetEmailLayoutByAccountID(Guid accountID, Guid programID, Guid medicalaidID); //hcare-1384
        EmailLayoutVM GetEmailLayoutForAccount(Guid accountID, Guid programID, Guid medicalaidID, Guid templateID); //hcare-1380
        ServiceResult InsertEmailLayout(EmailLayout model); //hcare-1384
        ServiceResult UpdateEmailLayout(EmailLayout model); //hcare-1384
        ServiceResult InsertAccountEmailLayout(AccountEmailLayout model); //hcare-1384
        ServiceResult UpdateAccountEmailLayout(AccountEmailLayout model); //hcare-1384
        List<DoctorEmailVM> GetDoctorDetails(string doctorname, string practicenumber, string practicename); //hcare-1391
        DoctorEmailVM GetDoctorByDoctorID(Guid doctorID); //hcare-1391
        MedicalAidSettings GetMedicalAidSettings(Guid medicalaidID); //hcare-1380
        List<ICD10Master> GetICD10Masters(); //HCare-1489
    }
}
