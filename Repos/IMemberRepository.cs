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
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace HaloCareCore.Repos
{
    public interface IMemberRepository : IDisposable
    {

        List<MedicalAidPlans> GetPlans(Guid MedicalAidId);
        List<EnquiryFullViewModel> GetEnquiryFullView(int queryID, Guid depID);
        IEnumerable<Dependant> GetPatients();
        IEnumerable<Member> GetMembers();
        Member GetMemberByMembershipNo(string membershipNo);
        List<Programs> GetPrograms();
        List<Programs> GetAllowedPrograms();
        List<Programs> GetAllowedProgramsPerScheme(Guid medId);
        List<Programs> GetAllowedProgramsPerUser(Guid medID, Guid userID);
        List<Programs> GetAllowedProgramsPerUserList(Guid userID); //hcare-1289
        List<MemberBasicViewModel> GetPatientProgramHistoryById(Guid dependantid);
        List<PatientProgramHistory> GetPatientProgramHistory();
        PatientProgramHistory GetPatientProgramHistoryByID(int id);
        List<AccessServices> GetAccessServices();
        List<NoteTypes> GetNoteTypes();
        List<MedicalAid> GetMedicalAids();
        List<MedicalAid> GetMedicalAidsByUser(Guid userID);
        List<MedicalAidAccount> GetMedicalAidAccounts();
        List<MedicalAidPlans> GetMedicalAidPlans();
        List<MedicalAidPlans> GetMedicalAidPlansByMedicalAidId(Guid medId);
        List<Origin> GetOrigin();
        List<Language> GetLanguage();
        List<Demographic> GetDemographic();
        List<QueryTypes> GetQueryTypes();
        List<EnquiryTypes> GetQueryTypesBySource(string source);
        List<DependantType> GetDependantType();
        List<Dependant> GetRecentEnrollments();
        List<ManagementStatus> GetManagementStatus();
        List<ManagementStatus> GetManagementStatusValidation();
        List<ManagementStatus> GetManagementStatuses(); //Hcare-1250
        List<ManagementStatus> GetManagementStatusList();
        List<ManagementStatus> GetManagementStatusByCode();
        List<ManagementStatus> GetManagementStatus_noPendingStatus();
        List<ManagementStatus> GetManagementStatus_noClosedStatus();
        ManagementStatus GetManagementStatus(string statusCode);
        ManagementStatus GetManagementStatusDescription(string statusName);
        List<Dependant> GetInProgressEnrol();
        PatientProgramHistory GetProgramHistoryById(int id);
        ServiceResult UpdateProgramHistory(PatientProgramHistory model);
        ServiceResult UpdatePatientProgramHistory(int id, string icd10, DateTime? enddate, string user);
        ServiceResult UpdatePreviousPatientProgramHistory(PatientProgramHistory model);
        ServiceResult UpdateLastPatientProgramHistory(PatientProgramHistory model);
        EnrolmentViewModel GetDependentDetails(Guid dependantID, Guid? pro);
        EnrolmentViewModel GetDependentMedicalAid(Guid dependantID, Guid pro);

        List<DoctorHistoryView> GetDrHistory(Guid dependantID);
        List<DoctorHistoryView> GetDoctorHistory(Guid dependantID);
        Member GetMemberByDependantID(Guid DepID);
        List<ManagementStatus> GetManagementStatusesByMedicalAid(Guid MedId);
        List<ManagementStatus> GetManagementStatusesByMAandPro(Guid medID, Guid proID);
        //Hcare-1043
        List<SmsMessageTemplates> GetSmsMessagesByMedicalAid(Guid MedId, Guid pro);
        List<EmailTemplates> GetEmailMessagesByMedicalAid(Guid MedId, Guid pro);
        List<Queries> GetQueriesByDependant(Guid dependantID);
        List<PatientManagementStatusHistory> GrabManagementStatusByDependantID(Guid dependantID);
        ManagementStatusVM GetManagmentStatusByCode(string code);
        List<PatientManagementStatusHistory> GetManagementStatusByDependentId(Guid dependantID);

        List<PatientManagementStatusHistory> GetManagementStatusByDependent(Guid dependantID, Guid Id);
        List<PatientManagementStatusHistory> GetManagementStatusByDependentId(Guid dependantID, Guid Id);
        List<ManagementStatusVM> GetManagmentStatusInformation(Guid dependantID);
        PatientManagementStatusHistory GetManagementStatusById(int id);
        DoctorQuestionnaireResults GetQuestionnaireById(int id);
        DoctorQuestionnaireViewModel GetFullQuestionnaireById(int id);
        ServiceResult UpdateManagementStatus(PatientManagementStatusHistory model);
        ServiceResult UpdateMSRecord(PatientManagementStatusHistory model); //hcare-1265
        ServiceResult UpdateManagementStatusCode(ManagementStatus model);
        ServiceResult InsertManagementStatus(PatientManagementStatusHistory model);
        ServiceResult InsertManagementStatusCode(ManagementStatus model);
        ManagementStatusVM GetManagementStatusByInformation(Guid dependantID, int id);

        ManagementStatus GetMStatusByCode(string code); //HCare-956
        ManagementStatus GetMStatusByName(string description); //HCare-956

        EnrolmentViewModel GetDependentDetails(string membershipno);
        List<AdvancedSearchResultModel> GetAdvancedSearchresults(AdvancedSearchView model);
        List<ScriptViewModel> GetScriptItems(int scriptNo);
        List<ScriptViewModel> GetScriptItems(Guid depID);
        List<ScriptViewModel> GetGeneralScriptItems(Guid depID);
        List<ScriptViewModel> GetDiabetesScriptItems(Guid depID);
        List<ScriptViewModel> GetAllDiabetesScriptItems(Guid depID);
        List<ScriptViewModel> GetHIVScriptItems(Guid depID);
        List<ScriptViewModel> GetAllHIVScriptItems(Guid depID);
        List<ScriptViewModel> GetMHScriptItems(Guid depID); //HCare-1183
        List<ScriptViewModel> GetAllMHScriptItems(Guid depID); //HCare-1183
        List<ScriptViewModel> GetScriptItemsFull(Guid depID);
        List<ScriptViewModel> GetScriptItemsMultiple(Guid depID, int scriptID);
        ScriptItems GetScriptItem(int itemNo);
        ServiceResult UpdateScriptItem(ScriptItems model);
        List<Products> GetActiveProducts();
        List<Products> GetActiveProductsByName(string productName);
        string GetPayPoint(Guid depId);
        List<MemberSearchViewModel> GetDependents(string membershipno);

        List<PatientManagementStatusHistory> GetDependentsById(Guid depId, Guid? id);
        MemberBasicViewModel GetDependentsBasicById(Guid depId);

        void InsertQuery(Queries model);
        void InsertMessage(SmsMessages model);
        ServiceResult InsertText_HCDWL(string depdendantID, string mobileNo, string effectiveDate, int template, string message, string status, string reference, string programID, string createdBy, string createdDate);
        void InsertEmailMessage(Emails model);
        // HCare-860
        void InsertEmailAttachment(ComsViewModel model, List<IFormFile> file);
        // HCare-860
        void AddAttachmentPath(int emailId, string path);
        List<Sex> GetSex();
        List<Priorities> GetPriorities();
        List<SmsMessageTemplates> GetTemplates();
        List<SmsMessageTemplates> GetTemplateByID(int tempId);
        List<EmailTemplates> GetEmailTemplates();
        List<EmailTemplates> GetEmailTemplateByID(int tempId);
        List<Icd10Codes> GetIcd10Codes();
        List<Icd10Codes> GetIcd10CodesByCode(string code);
        ServiceResult InsertTask(AssignmentItemTasks task);
        Member GetMembers(Guid memberId);
        List<SmsMessages> GetSmsMessages(Guid depId, Guid? pro); //1254
        List<Emails> GetEmails(Guid depId, Guid? pro); //1254
        List<Notes> GetNotes(Guid DepID);
        Notes GetNote(int Id);
        ServiceResult UpdateNote(Notes model);
        List<Pathology> GetPathology(Guid DepID);
        List<Pathology> GetGeneralPathology(Guid DepID);
        List<Pathology> GetHyperlipidaemiaPathology(Guid DepID);
        List<Pathology> GetDiabetesPathology(Guid DepID);
        List<Pathology> GetHIVPathology(Guid DepID);
        Pathology GetPathologyById(int Id);
        List<Pathology> GetGeneralPathologyForDependant(Guid DepID);
        List<Pathology> GetHba1cByPatient(Guid depId);
        List<Pathology> GetHyperlipidaemiaPathologyForDependant(Guid DepID);
        List<Pathology> GetDiabetesPathologyForDependant(Guid DepID);
        List<Pathology> GetHIVPathologyForDependant(Guid DepID);
        List<PatientProgramHistory> GetProgramHistory(Guid dependantID);
        List<PatientProgramHistory> GetProgramHistory(Guid dependantID, Guid Id);
        List<PatientProgramHistory> GetValidProgramHistory(Guid dependantID, Guid Id);
        List<PatientProgramHistory> GetMHDiagnosisHistory(Guid dependantID, Guid Id);
        List<PatientProgramVM> Get_MH_Diagnosis_History(Guid dependantID, Guid Id);
        List<PatientProgramVM> Get_Program_History(Guid dependantID, Guid Id);
        List<PatientProgramVM> Get_MH_Program_Diagnosis_History(Guid dependantID, Guid Id);
        List<PatientProgramHistory> GetMHDiagnosisHistory_pb(Guid dependantID, Guid Id);
        List<PatientProgramHistory> GetPatientProgramHistory(Guid dependantID);
        List<PatientProgramHistory> GetPatientProgramHistory(Guid dependantID, Guid Id);
        List<PatientProgramSubHistory> GetPatientProgramSubHistory(Guid dependantID);
        List<PatientProgramSubHistory> GetPatientProgramSubHistory(Guid dependantID, Guid Id);
        List<PatientProgramSubHistory> GetProgramSubHistory(Guid dependantID, Guid Id); //Hcare-1122
        List<PatientHistoryVM> GetProgramChangesHistory(Guid dependantID, Guid Id); //Hcare-1195
        List<PatientHistoryVM> GetPatientProgramChangeHistory(Guid dependantID, Guid Id); //Hcare-1203
        PatientProgramHistory GetProgramLatest(Guid dependantID, Guid Id);
        PatientProgramHistory GetLatestPatientProgram(Guid dependantID, Guid Id);
        List<PatientProgramHistory> GetProgramHistories(Guid dependantID);
        List<MembershipNoHistory> GetMembershipNumberHistory(Guid dependantID);
        List<MemberImports> GetMemberImports(Guid dependantID);
        ServiceResult UpdatePathology(Pathology model);
        List<ClinicalRules> GetClinicalRules();

        List<DoctorQuestionnaireResults> GetDrQuesResults(Guid depId);
        List<ScriptReferences> GetScriptReference(Guid depId);
        Clinical GetClinicalExamById(int Id);
        ClinicalViewModel GetClinicalInfo(Guid depID, Guid? pro);
        string getPlanCode(Guid dependantID);
        string GetClaimCode(string planname);
        string GetClaimCodeByMedicalAidId(string medcode);
        List<MedicalAidClaimCode> GetMedicalAidClaimCode();
        string GetPatientPlan(Guid depID);
        PatientPlanHistory GetPatientPlanByDependant(Guid dependantID); //hcare-1374
        string GetServicePlanCode(string planname);
        string GetMembers(string membership_no, Guid medicalaidID);
        string GetDependants(Guid memberID, string dep_code);
        string GetDependantByIDNo(string idno);
        List<Dependant> GetDependantByMembershipDepCodeAidCode(string membershipNo, string DepCode, string medAidCode);
        Guid InsertMembers(Member member);
        Guid InsertNote(Notes note);
        Guid InsertDependant(Dependant dependent);
        Guid InsertMemberData(MemberImports dependent);
        ServiceResult UpdateMemberData(MemberImports member);
        Guid InsertContact(Contact contact);
        Guid InsertContact_MI(ComsViewModel model);
        Guid InsertAddress(Address address);
        Guid InsertMemberInformation(MemberInformation meminfo);
        ServiceResult InsertPathology(Pathology pathology);
        int InsertScript(Scripts script);
        ServiceResult InsertScriptItem(ScriptItems items);
        ServiceResult InsertScriptReference(ScriptReferences model);
        void InsertAttachment(Attachments attachment);
        ServiceResult InsertProgramHistory(PatientProgramHistory model);
        ServiceResult InsertAuthLetter(AuthorizationLetters letter);
        void DeleteMembers(Guid memberId);
        void UpdateMembers(Member member);
        void UpdateMemberInformation(MemberInformation member);
        ServiceResult UpdateScriptItems(List<ScriptViewModel> items, string status);
        ServiceResult UpdateScript(Scripts model);
        ServiceResult InsetEnquiryComment(EnquiryComments model);
        MemberInformation GetMemberInformation(Guid DependentID);
        Address GetAddress(Address address);
        Address GetAddressById(Guid Id);
        Contact GetContactById(Guid Id);
        MemberInformation GetContactById_MI(Guid Id);
        ComsViewModel GetContactByDependentID(Guid DependentID);
        ComsViewModel GetAddressByDependentID(Guid DependentID);
        List<AttachmentTypes> GetAttachmentTypes();
        string GetAttachmentType(string attachmenttype);
        List<Attachments> GetAttachments(Guid dependentID);
        Attachments GetAttachment(int id);
        ServiceResult UpdateAttachment(Attachments model);
        Address GetAddress(Guid depID);
        Contact GetContact(Contact contact);
        Contact GetContact(Guid depID);
        MemberInformation GetContact_MI(Guid depID);
        List<Scripts> GetScripts(Guid DepID);
        Scripts GetScript(int scriptID);
        List<ScriptTypes> GetScriptTypes();
        PatientProfileViewModel GetProfile(Guid dependentId);
        ServiceResult InsertPaypointHistory(PayPointHistory model);
        ServiceResult InsertDoctorHistory(PatientDoctorHistory model);
        List<AuthorizationLetters> GetAuthorizationLetters(Guid dependentId);
        List<DoctorQuestionnaireResults> GetDoctorQuestionResultsById(Guid dependentId);
        List<CoMorbidTypes> GetCoMorbids();
        List<string> GetCoMorbidList();
        List<CoMorbidConditionVM> GetCoMorbids_Excluded(Guid depId);
        List<CoMorbidConditionVM> GetCoMorbidsNotLinkedToDependant(Guid depId);
        List<CoMorbidConditionVM> GetNewCoMorbidsNotLinkedToDependant(Guid depId);
        List<CoMorbidConditionVM> GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis(Guid depId);
        List<CoMorbidConditionVM> GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis_ProgramHistory(Guid depId);
        List<CoMorbidConditionVM> GetNewCoMorbidsNotLinkedToDependant_MH_Diagnosis_SubHistory(Guid depId);
        //List<CoMorbidConditionVM> GetCoMorbidsICD10NotLinkedToDependant(string condition);
        List<CoMorbidConditionVM> GetCoMorbidsICD10NotLinkedToDependant(string condition, Guid dependantID);
        List<CoMorbidConditionVM> GetNewCoMorbidsICD10NotLinkedToDependant(string condition, Guid dependantID);
        List<CoMorbidConditionVM> GetNewCoMorbidsICD10NotLinkedToDependant_MH(string condition, Guid dependantID);
        List<CoMorbidConditionVM> GetNewCoMorbidsICD10NotLinkedToDependant_MH_ProgramHistory(string condition, Guid dependantID);
        List<CoMorbidConditionVM> GetNewCoMorbidsICD10NotLinkedToDependant_MH_SubHistory(string condition, Guid dependantID);
        List<CoMorbidConditionVM> GetComorbidInformation(Guid dependantID);
        List<CoMorbidConditionVM> GetNewComorbidInformation(Guid dependantID);

        CoMorbidConditionVM GetCoMorbids_ExcludedByID(int id);
        CoMorbidConditionVM GetNewCoMorbids_ExcludedByID(int id);
        CoMorbidConditionVM GetMHDiagnosisInformation(string code, string icd10);
        CoMorbidConditionVM GetNewCoMorbids_ExcludedByConditionCode(string code);
        List<CoMormidConditions> GetCoMorbid();
        List<CoMorbidClinicalRulesView> GetCoMorbidRules(string ruletype);

        ServiceResult InsertComorbidCondition(CoMormidConditions model);
        ServiceResult UpdatePatientProgramSubHistory(PatientProgramSubHistory model);
        List<MentalHealthDiagnosis> GetMentalHealthHistoryByDependant(Guid dependantID, Guid Id);
        ServiceResult InsertMentalHealthDiagnosis(MentalHealthDiagnosis model);
        ServiceResult UpdateMentalHealthDiagnosis(MHDiagnosisViewModel model);
        List<PatientProgramSubHistory> GetPatientProgramSubHistoryList(Guid dependantID);
        List<CoMorbidConditionVM> GetPatientProgramHistoryList_Both(Guid depId);
        PatientProgramSubHistory GetPatientProgramSubHistoryByID(int id);
        MHDiagnosisViewModel GetMentalHealthDiagnosisByID(int id);

        CoMormidConditions GetCoMorbidByID(int id);
        int GetCoMorbidByName(string name);
        CoMorbidTypes GetCoMorbidTypeByID(int id);
        ComorbidConditionExclusions GetCoMorbidExclusionsByID(int id);
        ComorbidConditionExclusions GetICD10Info(string id);
        ComorbidConditionExclusions GetCMByMappingCode(string mappingCode);
        ServiceResult UpdateCoMorbidCondition(CoMormidConditions model);
        ServiceResult InsertAuthorization(Authorisations model);
        List<CoMorbidClinicalRules> GetCoMorbidRules();

        ServiceResult UpdateDependant(Dependant member);
        ServiceResult InsertMembershipHistory(MembershipNoHistory model);

        List<AssignmentView> GetNewAssignments(Guid depID);
        List<AssignmentView> GetActiveOnlyAssignments(Guid dependentId);
        List<AssignmentView> GetActiveAssignments(Guid dependentId);
        List<AssignmentView> GetActivePatientAssignments(Guid dependentId);
        List<AssignmentView> GetActivePatientAssignments(Guid dependentId, Guid Id);
        List<AssignmentView> GetClosedPatientAssignments(Guid dependentId);
        List<AssignmentView> GetClosedPatientAssignments(Guid dependentId, Guid Id);
        List<Assignments> GetActiveSelectAssignments(Guid dependentId);
        List<Assignments> GetActiveMemberAssignments(Guid dependentId);
        List<CaseManagers> GetCaseManagers();
        List<Assignments> GetAllActiveAssignments();
        List<AssignmentView> GetClosedAssignments(Guid dependentId);
        List<AssignmentTypes> GetAssignmentTypes();
        List<AssignmentItemTypes> GetAssignmentItemTypes();
        List<AssignmentItemTypes> GetAssignmentItemTypesForUser(Guid userID);
        List<AssignmentItemTypes> GetAssignmentItems();
        Users GetUserNameByUserID(Guid userID);
        wfSettings GetQueueNameByQueueID(Guid queueID);
        List<AssignmentItemTypes> GetAssignmentItemTypesByProgram(Guid pro);

        List<AssignmentItemTaskTypes> GetAssignmentTaskTypes();
        List<TaskTypeRequirements> GetTaskRequirement();
        List<TaskRequirementItemLinking> GetTaskRequirements(string itemType);
        ServiceResult InsertAssignment(AssignmentsView model);
        List<RulesInstructions> GetRulesHistory(Guid dependantID);
        List<ClinicalRulesVM> GetClinicalRulesHistory(Guid dependantID);
        List<ClinicalRulesVM> GetClinicalRulesByDependant(Guid dependantID, string programCode);
        ServiceResult InsertClinicalRule(PatientClinicalRulesHistory model);
        List<Pathology> GetLaboratories();
        List<PathologyFields> GetPathologyFields();
        Queries GetQueryById(int Id);
        ServiceResult InsertPlanHistory(PatientPlanHistory model);
        List<Queries> GetActiveQueries();
        List<Queries> GetQueryStatus();
        List<EnquiriesSearch> GetActiveSQueries();
        List<AdvancedSearchResultModel> GetFilteredAdSearch();
        List<Queries> GetSearchQueries(string querytype, string membershipNo = "");
        List<EnquiriesSearch> GetSearchQueriesResults(string querytype = "", string membershipNo = "", string medicalaid = "", string memberStatus = "", string queryPriority = "", string queryStatus = "");
        List<ProductionSearchView> GetProductionSearchResults(string medicalaid = "", string users = "", string fromDate = "", string toDate = "", string filter = "");
        List<PathologySearch> GetActivePathologyResults();
        List<PathologySearch> GetPathologySearchResults(string medicalaid = "", string program = "", string pathologyfield = "", string fromDate = "", string toDate = "");
        List<PathologySearch> GetDependantPathologySearchResults(Guid DependantID, string pathologyfield = "", string fromDate = "", string toDate = "");
        List<PathologySortVM> GetSortedPathologySearchResults(Guid DependantID, string pathologyfield = "", string fromDate = "", string toDate = "");

        List<EnquiriesSearch> GetAllQueries(); //HCare-680(Enquiry search enhanced)
        ServiceResult UpdateCaseManager(CaseManagers model);
        ServiceResult InsertEnrolmentMonitor(EnrolmentStepsMonitor model);
        EnrolmentStepsMonitor GetEnrolmentStep(Guid depId);
        ServiceResult UpdateEnrolmentStep(EnrolmentStepsMonitor model);
        ServiceResult InsertClinicalExam(Clinical model);
        ServiceResult UpdateClinical(Clinical model);
        ServiceResult UpdateQuery(Queries model);
        ServiceResult UpdateContact(Contact model);
        ServiceResult UpdateContact_MI(ComsViewModel model);
        ServiceResult UpdateAddress(Address model);
        ServiceResult UpdateAddress_MI(ComsViewModel model);

        ServiceResult InsertProgram(Programs model);

        Assignments GetAssignment(int Id);
        AssignmentView GetAssignment(Guid depID, string itemType);
        AssignmentView GetAssignment(Guid depID, string itemType, Guid? pro);
        List<AssignmentSearch> GetAssignmentSearchResults(string medicalaid = "", string program = "", string assignmentitem = "", string fromDate = "", string toDate = "");

        ServiceResult UpdateAssignment(Assignments model);

        ServiceResult InsertRiskRating(PatientRiskRatingHistory model);
        ServiceResult UpdateRiskRating(PatientRiskRatingHistory model);
        List<HospitalizationClaims> GetHospitalizationClaims(string membershipNo, string dependantCode);
        List<HospitalClaimView> GetHospitalizationClaim(string membershipNo, string dependantCode);
        List<HospitalizationClaimEvents> GetClaimEvents(string claimsNo);
        List<HospitalizationAuthEvents> GetAuthEvents(string epiAuth);
        ServiceResult InsertMedicationHistory(MedicationHistory model);
        ServiceResult InsertDrQuestionnaire(DoctorQuestionnaireResults model);
        ServiceResult UpdateMedicationHistory(MedicationHistory model);

        AssignmentItemLogs GetAssignmentItemLogs(int id);
        ServiceResult InsertAssignmentLog(AssignmentItemLogs model);
        ServiceResult UpdateAssignmentLog(AssignmentItemLogs model);
        Contact GetDrInformation(Guid doctorId);

        PatientQuestionnaireResponse GetDisclaimerResultsById(int id);
        List<PatientQuestionnaireResponse> GetDisclaimerResults(Guid dependentId);
        List<disclaimerViewModel> GetDisclaimerViewResults(Guid dependentId);
        List<disclaimerViewModel> GetDisclaimerViewDistinct(Guid dependentId);
        List<PatientQuestionnaireResponse> GetDisclaimerViewList(Guid dependentId);
        ServiceResult InsertDisclaimerResults(PatientQuestionnaireResponse model);
        ServiceResult UpdateDisclaimerResults(PatientQuestionnaireResponse model);

        Dependant GetDependantByDependantID(Guid depId);
        Dependant GetDependantDetails(Guid depId, Guid? pro);
        MemberImports GetMemberImportDetails(Guid depId);

        PatientStatusHistory GetPatientStatus(Guid depID);
        ServiceResult InsertPatientStatus(PatientStatusHistory model);

        List<PatientRiskRatingHistory> GetPatientRiskRating(Guid dependantID);
        PatientRiskRatingHistory GetPatientRiskRatingByID(int id);

        List<HypoglymiaRiskHistory> GetHypoRiskRating(Guid dependantID);
        List<RiskRatingTypes> GetRiskRatingTypes();
        List<PatientRiskRatingHistory> GetPatientRiskRatingMultipleReasons(Guid dependantID, Guid? pro);
        PatientRiskRatingHistory GetRiskRatingByID(int? id);

        List<ManagementStatus_DeactivatedReasons> GetManagementStatusReasons();
        ManagementStatus_DeactivatedReasons GetManagementStatusReasonByReason(string reason);
        ManagementStatus_DeactivatedReasons GetManagementStatusReasonByName(string name);
        ServiceResult InsertManagementStatusReasons(ManagementStatus_DeactivatedReasons model);
        ManagementStatus_DeactivatedReasons GetManagementStatusReasonByID(int id);
        ServiceResult UpdateManagementStatusReasons(ManagementStatus_DeactivatedReasons model);

        OtherMedicalHistory GetOtherMedicalHistoryById(int id);
        List<OtherMedicalHistory> GetOtherMedicalHistory(Guid depId);
        ServiceResult InsertOtherMedicalHistory(OtherMedicalHistory model);
        ServiceResult UpdateOtherMedicalHistory(OtherMedicalHistory model);

        List<Clinical> GetClinicalExam(Guid depId);
        List<Clinical> GetGeneralClinicalExam(Guid depId);
        List<Clinical> GetDiabetesClinicalExam(Guid depId);
        List<Clinical> GetHivClinicalExam(Guid depId);

        List<ComorbidView> GetComorbidItems(Guid depID, Guid? pro = null);
        List<CoMormidConditions> CoMorbid_Validation(Guid depID);
        List<CoMormidConditions> GetCoMormidConditions();
        List<ComorbidView> GetGeneralComorbidItems(Guid depID, Guid? pro = null);
        MedicationHistory GetMedicationHistory(int Id);
        List<MedicationHistory> GetMedicationHistory();
        List<MedicationHistory> GetMedicationHistories(Guid DepId);
        List<MedicationHistory> GetGeneralMedicationHistories(Guid DepId);
        List<MedicationHistory> GetHIVMedicationHistories(Guid DepId);

        List<Allergies> GetAllergies(Guid DepId);
        List<Allergies> GetGeneralAllergies(Guid DepId);
        Allergies GetAllergyById(int Id);
        Allergies GetAllergyByName(string allergy);
        ServiceResult InsertAllergy(Allergies model);
        ServiceResult UpdateAllergy(Allergies model);

        ClinicalHistoryQuestionaire getQuestionnaire(Guid Id);
        ClinicalHistoryQuestionaire GetSocialRecord(int id);
        List<ClinicalHistoryQuestionaire> GetClinicalHistory(Guid Id);
        List<ClinicalHistoryQuestionaire> GetSocialHistory(Guid DepId);
        List<ClinicalHistoryQuestionaire> GetGeneralSocialHistory(Guid DepId);
        ServiceResult InsertClinicalHistoryQuestionnaire(ClinicalHistoryQuestionaire model);
        ServiceResult UpdateQuestionnaire(ClinicalHistoryQuestionaire model);
        ServiceResult UpdateSocialRecord(ClinicalHistoryQuestionaire model);

        List<HospitalizationAuths> GetHospitalizationAuths(string membershipNo, string dependantCode);
        List<HospitalizationAuths> GetGeneralHospitalizationAuths(string membershipNo, string dependantCode);
        HospitalizationAuths GetHospitalizationAuthByID(int id);
        ServiceResult UpdateHospitalizationAuth(HospitalizationAuths model);
        ServiceResult InsertHospitalizationAuths(HospitalizationAuths model);

        QuestionnaireOther GetQuestionnaireOtherResultById(int id);
        List<QuestionnaireOther> GetQuestionnaireOtherResults(Guid depId);
        QuestionnaireOther GetTuberculosisResults(Guid depId); //HCare-1276
        List<QuestionnaireOther> GetGeneralQuestionnaireOtherResults(Guid depId);
        List<QuestionnaireOther> GetDiabetesQuestionnaireOtherResults(Guid depId);
        List<QuestionnaireOther> GetHIVQuestionnaireOtherResults(Guid depId);
        ServiceResult InsertQuestionnaireOtherResults(QuestionnaireOther model);
        ServiceResult UpdateQuestionnaireOtherResult(QuestionnaireOther model);

        List<QuestionnaireOther> GetOtherResults(Guid depId);
        List<QuestionnaireOther> GetPregnancyResults(Guid depId);

        NewBorn GetNewBornResultById(int id);
        List<NewBorn> GetNewBornResults(Guid depId);
        ServiceResult InsertNewBornResults(NewBorn model);
        ServiceResult UpdateNewBornResult(NewBorn model);

        PatientDiagnosis GetDiagnosisById(int id);
        List<PatientDiagnosis> GetDiagnosisPrograms();
        List<PatientDiagnosis> GetDiagnosisResults(Guid depId);
        List<PatientDiagnosis> GetDiabetesDiagnosisResults(Guid depId);
        List<PatientDiagnosis> GetHIVDiagnosisResults(Guid depId);
        ServiceResult InsertDiagnosisResults(PatientDiagnosis model);
        ServiceResult UpdateDiagnosisResult(PatientDiagnosis model);
        PatientDiagnosis GetPatientDiagnosisResult(Guid dependantID, string programCode); //hcare-863

        Vision GetVisionResultById(int id);
        List<Vision> GetVisionResults(Guid depId);
        ServiceResult InsertVisionResults(Vision model);
        ServiceResult UpdateVisionResult(Vision model);

        Podiatry GetPodiatryResultById(int id);
        List<Podiatry> GetPodiatryResults(Guid depId);
        ServiceResult InsertPodiatryResults(Podiatry model);
        ServiceResult UpdatePodiatryResult(Podiatry model);

        AutoNeropathy GetAutoNeroResultById(int id);
        List<AutoNeropathy> GetAutoNeroResults(Guid depId);
        ServiceResult InsertAutoNeroResults(AutoNeropathy model);
        ServiceResult UpdateAutoNeroResult(AutoNeropathy model);

        Hypoglycaemia GetHypoglycaemiaResultById(int id);
        List<Hypoglycaemia> GetHypoglycaemiaResults(Guid depId);
        ServiceResult InsertHypoglycaemiaResults(Hypoglycaemia model);
        ServiceResult UpdateHypoglycaemiaResult(Hypoglycaemia model);

        List<MedicalAid> GetMedicalAidList();
        List<MedicalAidVM> GetMedicalAidPlansList();
        List<MedicalAidVM> GetMedicalAidPlanValidation();
        List<MedicalAidVM> GetAllowedMedicalAidPlan(); //hcare-1288
        MedicalAid GetMedicalAidById(Guid id);
        MedicalAidPlans GetMedicalAidPlanById(Guid id);
        List<MedicalAidVM> GetMedicalAidPlanDetails(Guid id);
        MedicalAidVM GetMedicalAidPlanDetail(Guid id);

        ServiceResult InsertSchemeOption(MedicalAidPlans model);
        ServiceResult UpdateSchemeOption(MedicalAidVM model);



        //HCare-1061
        List<DiabeticDairy> GetDiabeticDairyByDependant(Guid dependantID);
        List<DiabeticDairy> GetDiabeticDairy();
        ServiceResult InsertDiabeticDairy(DiabeticDairy model);
        ServiceResult UpdateDiabeticDairy(DiabeticDairy model);

        #region HCare-1014
        string GetPatientStagingHistoryByDependant(Guid dependantID);
        #endregion

        #region doctor-referral hcare-1136
        ServiceResult InsertDoctorReferral(DoctorReferral model);
        DoctorReferral GetDoctorReferralById(int id);
        ServiceResult UpdateDoctorReferral(DoctorReferral model);
        List<DoctorReferral> GetDoctorReferralResults(Guid dependentid);
        #endregion

        #region pathology-type hcare-970
        ServiceResult InsertPathologyType(PathologyTypes model);
        PathologyTypes GetPathologyTypeById(string id);
        ServiceResult UpdatePathologyType(PathologyTypes model);
        List<PathologyTypes> GetPathologyTypes();
        List<PathologyTypes> GetPathologyTypeList();
        List<PathologyTypes> GetPathologyTypeValidation();
        PathologyTypes GetPathologyTypeByCode(string code);
        PathologyTypes GetPathologyTypeByName(string name);
        #endregion

        #region Diabetes Care plan HCare-1092
        ServiceResult InsertCarePathology(CarePlanPathology model);
        List<CarePlanPathology> GetCarePathology(Guid dep, Guid? pro);
        CarePlanPathology GetCarePathologyByID(int id);

        ServiceResult UpdateCarePlanPathology(CarePlanPathology model);
        ServiceResult InsertMedicine(Medicine model);
        ServiceResult UpdateMedicines(Medicine model);
        List<Medicine> GetMedicinesByDependentID(Guid deptID, Guid? pro);
        Medicine GetMedicineByID(int id);

        List<ClinicalAddition> GetClinicalAdditionByClinicalID(Guid depId, Guid? pro);
        ServiceResult InsertClinicalAddition(ClinicalAddition model);
        ServiceResult UpdateClinicalAddition(ClinicalAddition model);
        ClinicalAddition GetClinicalAdditionByID(int id);
        List<NutritionAndLifestyle> GetNutritionandlifestyleByDependentID(Guid depId, Guid? pro);
        ServiceResult InsertNutritionAndLifestyle(NutritionAndLifestyle model);
        ServiceResult UpdateNutritionAndLifestyle(NutritionAndLifestyle model);
        NutritionAndLifestyle GetNutritionAndLifestyleByID(int id);
        Visit GetVisitByID(int id);
        List<Visit> GetVisitByDependentID(Guid depId, Guid? pro);
        ServiceResult InsertVisit(Visit model);
        ServiceResult UpdateVisit(Visit model);
        paediatric GetPaediatricByID(int id);
        List<paediatric> GetPaediatricByDependentID(Guid depId, Guid? pro);
        ServiceResult Insertpaediatric(paediatric model);
        ServiceResult UpdatePaediatrict(paediatric model);
        #endregion

        List<CommunicationLogVM> getCommunicationLog(Guid dep, Guid pro);
        List<CommunicationLogVM> getAllCommunicationLog(CommunicationLogVM model);

        List<Referral> getReferral();
        string getReferralByDepandent(Guid? dep);
        string getReferralByMedicalAid(Guid medAid);

        List<RiskSearch> GetRiskRatingSearchResults(string medicalaid = "", string program = "", string riskrating = "", string fromDate = "", string toDate = ""); //hcare-1138
        List<ManagementStatusSearch> GetManagementStatusSearchResults(string medicalaid = "", string program = "", string managementstatus_current = "", string fromdate_current = "", string todate_current = "", string managementstatus_previous = ""); //hcare-1267
        List<EnquiryResultsVM> GetEnquirySearchResults(string medicalaid = "", string program = "", string managementstatus = "", string querypriorities = "", string querytypes = "", string fromdate = "", string todate = ""); //hcare-874

        List<ProductionResultsVM> GetProductionSearchResults(string medicalaid = "", string program = "", string users = "", string workitems = "", string fromdate = "", string todate = ""); //hcare-1289

        List<WorkflowUser> GetUserWorkflowRecords(Guid UserID);
        List<WorkflowUser> InsertUserWorkflowRecords(Guid userid, string medicalscheme, string program, string managementStatus, string fromDate, string toDate, string riskRating, string assignment, string instruction);
        List<wfQueue> InsertWFQueue(Guid queueID, string medicalscheme, string program, string managementStatus, string fromDate, string toDate, string riskRating, string assignment, string instruction);
        List<wfUserVM> GetUserWorkflowIndex(Guid UserID);
        wfUserVM GetUserWorkflowTask(Guid dependantID);
        List<wfUserVM> GetUWFList(Guid UserID);
        List<WorkflowUser> DeleteWorkflow(Guid UserID);
        wfQueue GetUserWFById(int Id, Guid UserID, Guid medicalaidID, Guid programID, string managementstatus, string membershipnumber, string dependantcode, string riskrating);
        ServiceResult UpdateWorkflowUser(wfQueue model);
        ServiceResult InsertUserWorkflowLog(wfUserLog model);
        List<wfQueue> GetWFQueueByUserAndQueue(Guid userID, Guid QueueID);
        wfUserQueue GetUserQueueInfo(Guid UserID, Guid QueueID);
        ServiceResult RemoveWFQueueFromUser(Guid UserID, Guid QueueID);
        ServiceResult UpdateWFQueueForUser(Guid UserID, Guid QueueID, Guid newUserID);

        #region HCare-1196 
        Pathology GetHIVPathologyForManualStaging(Guid DepID);
        ServiceResult InsertHIVPatientStagingHistoryManualStaging(PatientStagingHistory patientStagingHistory);
        ServiceResult UpdateHIVPatientStagingHistoryForManualStaging(PatientStagingHistory patientStagingHistory);
        PatientStagingHistory GetPatientStagingHistoryById(int Id);
        IEnumerable<PatientStagingHistoryViewModel> GetPatientStagingAndReasonsHistorybyDependentId(Guid dependantID);
        #endregion

        //HCare-1241
        List<Hba1cRangeHistory> GetHba1cRangeHistory(Guid dependantID);

        List<MHDiagnosisVM> GetMHDiagnosis(Guid dependantID); //hcare-1194
        List<MentalHealthDiagnosis> GetAvailableMHDiagnosisForDependantID(Guid dependantID); //hcare-1203

        List<ReferralFrom> getReferralFrom(); //HCare-1144
        string getReferralFromByDepandent(Guid? dep, Guid pro);//HCare-1144

        List<ProductionWorkItems> GetProductionWorkItems(); //hcare-1289
        NextOFKin GetNextOfKinByID(Guid id); //hcare-1361
        List<NextOFKin> GetNextOfKinInformation(Guid dependantID); //hcare-1361
        ServiceResult InsertNextOfKin(NextOFKin model); //hcare-1361
        ServiceResult UpdateNextOfKin(NextOFKin model); //hcare-1361
        NextOFKin GetNOKValidation01(string firstname, string lastname);  //hcare-1361
        NextOFKin GetNOKValidation02(string firstname, string lastname, string telephone);  //hcare-1361
        NextOFKin GetNOKValidation03(string firstname, string lastname, string telephone, string email);  //hcare-1361
        NextOFKin GetNOKValidation04(string firstname, string lastname, string telephone, string email, string relation);  //hcare-1361
        List<NextOFKin> GetNextOfKinValidation();  //hcare-1361
        NextOFKin GetNextOfKinByDependantID(Guid dependantID); //hcare-1363

        MedicalAidPatientVM GetMedicalAidByDependantID(Guid dependantID); //hcare-1363
        MemberRecord GetMemberRecordByDependantID(Guid dependantID, Guid programID, string username);
        ServiceResult InsertMemberRecord(MemberRecord model);
        ServiceResult UpdateMemberRecord(MemberRecord model);
        //void AttachEmailAttachment(ComsViewModel model, string[] file );
        ServiceResult InsertEmail(Emails model); //hcare-1380
        ServiceResult InsertSMS(SmsMessages model); //hcare-1380

        List<EmailLayout> GetEmailLayoutByID(Guid id);

        PatientDoctorHistory GetPatientDoctorHistory(Guid dependantID);  //hcare-1391
        Emails GetMemberEmailByID(int emailID); //hcare-1448
        ServiceResult UpdateEmailStatus(Emails model); //hcare-1448
        MedicalAid GetMedicalAidByName(string name);

        bool ImportCommunication(ImportCommunicationModel importCommunicationModel);
        bool GetMemberBydependentIDAndMemberNumber(Guid depID, string memberNumber);
        


        void Save();
    }
}