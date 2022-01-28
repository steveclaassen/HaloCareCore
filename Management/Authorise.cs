using HaloCareCore.DAL;
using HaloCareCore.Helpers;
using HaloCareCore.Models;
using HaloCareCore.Models.Script;
using HaloCareCore.Repos;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.IO;
using System.Xml;

namespace HaloCareCore.Management
{
    public class Authorise
    {
        private IClinicalRepository _clinical;
        private IMemberRepository _member;
        private readonly IConfiguration Configuration;
        private OH17Context context;
        public Authorise()
        {
            _clinical = new ClinicalRepository(context, Configuration);
            _member = new MemberRepository(Configuration, context);
        }

        public XmlDocument GetMediscorXml(ScriptAuthViewModel ScriptAuthmodel, string TT = "")
        {
            string namespaceURI = "http://uddi.commupoint.com";

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml("<d xmlns=\"" + namespaceURI + "\"></d>");
            XmlNode xmlNode = (XmlNode)xmlDocument.DocumentElement;
            XmlElement element1 = xmlDocument.CreateElement("sd", namespaceURI);
            XmlElement element2 = xmlDocument.CreateElement("mn", namespaceURI);
            element2.InnerText = ScriptAuthmodel.member.member.membershipNo;
            element1.AppendChild((XmlNode)element2);
            XmlElement element3 = xmlDocument.CreateElement("mac", namespaceURI);
            var m = new MemberRepository(Configuration, context);
            element3.InnerText = m.GetClaimCode(m.getPlanCode(ScriptAuthmodel.member.dependent.DependantID));
            element1.AppendChild((XmlNode)element3);
            XmlElement element4 = xmlDocument.CreateElement("dc", namespaceURI);
            element4.InnerText = ScriptAuthmodel.member.dependent.dependentCode;
            element1.AppendChild((XmlNode)element4);
            XmlElement element5 = xmlDocument.CreateElement("ed", namespaceURI);
            element5.InnerText = ScriptAuthmodel.script.effectiveDate.ToString("dd-MMM-yyyy");
            element1.AppendChild((XmlNode)element5);
            XmlElement element6 = xmlDocument.CreateElement("so", namespaceURI);
            element6.InnerText = m.GetServicePlanCode(m.getPlanCode(ScriptAuthmodel.member.dependent.DependantID)); ;
            element1.AppendChild((XmlNode)element6);
            XmlElement element7 = xmlDocument.CreateElement("man", namespaceURI);
            element7.InnerText = ScriptAuthmodel.member.MedicalAids[0].Name;
            element1.AppendChild((XmlNode)element7);
            XmlElement element8 = xmlDocument.CreateElement("re", namespaceURI);
            element8.InnerText = ScriptAuthmodel.script.active.ToString();
            element1.AppendChild((XmlNode)element8);
            XmlElement element9 = xmlDocument.CreateElement("tt", namespaceURI);
            element9.InnerText = TT;
            element1.AppendChild((XmlNode)element9);
            xmlNode.AppendChild((XmlNode)element1);
            XmlElement element10 = xmlDocument.CreateElement("si", namespaceURI);
            /*DataTable dataTable2 = SearchRecords.BuildDataSetWithConnectionAdo("select 
             *0 a.product_code,
             *1 b.product_name, 
             *2 replace(replace(a.directions,'<','&lt;'), '>','&gt;') , 
             *3 a.quantity, 
             *4 a.item_no, 
             *5 a.quantity, 
             *6 a.item_from_date, 
             *7 a.item_to_date, 
             *8 a.priority_auth_type, 
             *9 a.priority_auth_reason,  " + "
             *10 a.drug_status_benefit_flag, 
             *11 a.max_days_supply, 
             *12 a.days_quantity_comparison_code, 
             *13 a.fill_type, 
             *14 a.fill_period, 
             *15 a.fill_max_period, " + " 
             *16 a.period_quantity_type, 
             *17 a.period_quantity_days, 
             *18 a.period_quantity_max,
             *19 a.status , 
             *20 a.co_pay_override
             * " + " from member_patient_script_items a,product b where script_no ='" + ScriptNo + "' and a.product_code=b.product_code and (a.status='Authorised' OR a.status='Active/Not yet Authorised')", Connection).Tables[0];
            */
            foreach (var item in ScriptAuthmodel.items) //(int index = 0; index < dataTable2.Rows.Count; ++index)
            {
                XmlElement element11 = xmlDocument.CreateElement("i", namespaceURI);
                element11.SetAttribute("nc", item.nappiCode);
                element11.SetAttribute("pn", item.productName);
                element11.SetAttribute("di", item.directions);
                element11.SetAttribute("do", item.quantity.ToString());
                element11.SetAttribute("pan", item.itemNo.ToString());
                element11.SetAttribute("q", item.toDate.Value.ToString("dd-MMM-yyyy"));
                element11.SetAttribute("coPaymentOverride", "Y");
                if (ScriptAuthmodel.member.MedicalAids[0].medicalAidCode == "SPEC02")
                {
                    try
                    {
                        DateTime dateTime2 = DateTime.Parse(item.fromDate.ToString());
                        DateTime dateTime3 = DateTime.Parse("01-JAN-2011");
                        if (dateTime2 < dateTime3)
                            element11.SetAttribute("fd", "01-JAN-2011");
                        else
                            element11.SetAttribute("fd", dateTime2.ToString("dd-MMM-yyyy"));
                    }
                    catch
                    {
                        element11.SetAttribute("fd", "");
                    }
                }
                else
                {
                    try
                    {
                        DateTime dateTime2 = DateTime.Parse(item.fromDate.ToString());
                        element11.SetAttribute("fd", dateTime2.ToString("dd-MMM-yyyy"));
                    }
                    catch
                    {
                        element11.SetAttribute("fd", "");
                    }
                }
                try
                {
                    DateTime dateTime2 = DateTime.Parse(item.toDate.ToString());
                    element11.SetAttribute("td", dateTime2.ToString("dd-MMM-yyyy"));
                }
                catch
                {
                    element11.SetAttribute("td", "");
                }
                element11.SetAttribute("pat", "");
                element11.SetAttribute("par", "OD");
                if (String.IsNullOrEmpty(item.benefitType))
                {
                    element11.SetAttribute("dsbf", "K");
                }
                else
                {
                    element11.SetAttribute("dsbf", item.benefitType);
                }
                element11.SetAttribute("mds", "30");
                element11.SetAttribute("dcc", "L");
                element11.SetAttribute("ft", "");
                element11.SetAttribute("fp", "");
                element11.SetAttribute("fmp", "");
                element11.SetAttribute("pqt", "");
                element11.SetAttribute("pqd", "");
                element11.SetAttribute("pqm", "");
                element10.AppendChild((XmlNode)element11);
            }
            xmlNode.AppendChild((XmlNode)element10);

            string str6 = ValidationHelpers.ValidateXml("http://uddi.commupoint.com", "http://mediscor.ohis.co.za/schema/onehealthauthschema.xsd", xmlDocument.OuterXml.ToString());
            if (str6 == "The XML is valid according to the specified schema XSD file")
            {
                XmlElement element11 = xmlDocument.CreateElement("m", namespaceURI);
                xmlNode.AppendChild((XmlNode)element11);
                element11.InnerText = "Perfect XML";
            }
            else
            {
                XmlElement element11 = xmlDocument.CreateElement("m", namespaceURI);
                xmlNode.AppendChild((XmlNode)element11);
                element11.InnerText = str6;
            }
            return xmlDocument;
        }

        public XmlDocument GenerateSpectramedNappisAuthXml(ScriptAuthViewModel scriptInfo)
        {
            try
            {
                
                string membership_no = scriptInfo.member.member.membershipNo;
                string dependent_code = scriptInfo.member.dependent.dependentCode;
                string medical_aid = scriptInfo.member.MedicalAids[0].medicalAidCode;
                var m = new MemberRepository(Configuration, context);
                string scheme_code = m.getPlanCode(scriptInfo.member.dependent.DependantID);
                string firstName = scriptInfo.member.dependent.firstName;
                string lastName = scriptInfo.member.dependent.lastName;
                var dateOfBirth = scriptInfo.member.dependent.birthDate.ToString("ddMMyyyy"); ;
                string practiceNo = scriptInfo.member.doctor.practiceNo.Substring(0,7);
                string program_start_date = "";
                if (string.IsNullOrEmpty(program_start_date))
                {
                    program_start_date = DateTime.Now.AddMinutes(-2).ToString();
                }

                string icd_10_code = "";

                XmlDocument document = new XmlDocument();
                document.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><AuthMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"></AuthMessage>");
                XmlNode RootNode = document.DocumentElement;

                //parent item
                XmlElement xmleAuthorisationMessage = document.CreateElement("AuthorisationMessage");

                //main items
                XmlElement xmleTransaction = document.CreateElement("Transaction");
                XmlElement xmleAuthorisation = document.CreateElement("Authorisation");

                int lognumber = LogAuthRequest(3010, scriptInfo.script.scriptID, scriptInfo.member.dependent.DependantID, "", "", "UserName");
                //<transaction>
                XmlElement xmleVersionNumber = document.CreateElement("VersionNumber");
                xmleVersionNumber.InnerText = "2.0";
                XmlElement xmleType = document.CreateElement("Type");
                xmleType.InnerText = "AI";
                XmlElement xmleOriginator = document.CreateElement("Originator");
                xmleOriginator.InnerText = "ZOPHARM";
                XmlElement xmleNumber = document.CreateElement("Number");
                xmleNumber.InnerText = lognumber.ToString();
                XmlElement xmleDateTime = document.CreateElement("DateTime");
                xmleDateTime.InnerText = DateTime.Now.ToString("yyyyMMddHHmmss");
                XmlElement xmleDestinationCode = document.CreateElement("DestinationCode");
                xmleDestinationCode.InnerText = "120AZ";
                XmlElement xmleTestIndicator = document.CreateElement("TestIndicator");
                xmleTestIndicator.InnerText = "Y";

                xmleTransaction.AppendChild(xmleVersionNumber);
                xmleTransaction.AppendChild(xmleType);
                xmleTransaction.AppendChild(xmleOriginator);
                xmleTransaction.AppendChild(xmleNumber);
                xmleTransaction.AppendChild(xmleDateTime);
                xmleTransaction.AppendChild(xmleDestinationCode);
                xmleTransaction.AppendChild(xmleTestIndicator);

                //<authorisation>
                XmlElement xmleStatus = document.CreateElement("Status");
                xmleStatus.InnerText = "A";
                XmlElement xmleAuthorisationType = document.CreateElement("Type");
                xmleAuthorisationType.InnerText = "HIV";
                XmlElement xmleAuthorisationStartDate = document.CreateElement("AuthorisationStartDate");
                xmleAuthorisationStartDate.InnerText = DateTime.Now.ToString("yyyyMMdd");

                XmlElement xmleInHospitalisationIndicator = document.CreateElement("InHospitalisationIndicator");
                xmleInHospitalisationIndicator.InnerText = "N";
                XmlElement xmleDspIndicator = document.CreateElement("DspIndicator");
                xmleDspIndicator.InnerText = "N";

                xmleAuthorisation.AppendChild(xmleStatus);
                xmleAuthorisation.AppendChild(xmleAuthorisationType);
                xmleAuthorisation.AppendChild(xmleAuthorisationStartDate);
                xmleAuthorisation.AppendChild(xmleInHospitalisationIndicator);
                xmleAuthorisation.AppendChild(xmleDspIndicator);

                XmlElement xmleMember = document.CreateElement("Member");
                XmlElement xmleMemberMembershipNo = document.CreateElement("MembershipNumber");
                xmleMemberMembershipNo.InnerText = membership_no.Trim();
                XmlElement xmleMedicalSchemeCode = document.CreateElement("MedicalSchemeCode");
                xmleMedicalSchemeCode.InnerText = scheme_code;
                xmleMember.AppendChild(xmleMemberMembershipNo);
                xmleMember.AppendChild(xmleMedicalSchemeCode);

                XmlElement xmlPatient = document.CreateElement("Patient");
                XmlElement xmlePatientDependantCode = document.CreateElement("DependantCode");
                xmlePatientDependantCode.InnerText = dependent_code.Trim();
                XmlElement xmlePersonDetail = document.CreateElement("PersonalDetail");
                XmlElement xmlePersonDetailFirstName = document.CreateElement("FirstName");
                xmlePersonDetailFirstName.InnerText = firstName.Trim();
                XmlElement xmlePersonDetailDOB = document.CreateElement("DOB");
                string dob = dateOfBirth;
                xmlePersonDetailDOB.InnerText = dob;
                xmlePersonDetail.AppendChild(xmlePersonDetailFirstName);
                xmlePersonDetail.AppendChild(xmlePersonDetailDOB);
                xmlPatient.AppendChild(xmlePatientDependantCode);
                xmlPatient.AppendChild(xmlePersonDetail);

                //add Authorisation child items
                xmleAuthorisation.AppendChild(xmleMember);
                xmleAuthorisation.AppendChild(xmlPatient);

                //provider 
                XmlElement xmlProvider = document.CreateElement("Provider");
                XmlElement xmleRole = document.CreateElement("Role");
                xmleRole.InnerText = "SP";
                XmlElement xmlePracticeNumber = document.CreateElement("PracticeNumber");
                xmlePracticeNumber.InnerText = practiceNo.Trim();
                xmlProvider.AppendChild(xmleRole);
                xmlProvider.AppendChild(xmlePracticeNumber);

                //add Authorisation child items
                xmleAuthorisation.AppendChild(xmlProvider);

                //GenerateDiagnosisXml(ref document, ref xmleAuthorisation, membershipNo.Trim(), dependantCode.Trim(), medicalAidCode.Trim());

                string stage = "P";  //Primary
                string codeSet = "ICD";
                if (string.IsNullOrEmpty(icd_10_code))
                {
                    icd_10_code = "B24";
                }

                XmlElement xmleDiagnosis = document.CreateElement("Diagnosis");

                XmlElement xmleDiagnosisStage = document.CreateElement("Stage");
                xmleDiagnosisStage.InnerText = stage.Trim();
                XmlElement xmleDiagnosisCodeSet = document.CreateElement("CodeSet");
                xmleDiagnosisCodeSet.InnerText = codeSet.Trim();
                XmlElement xmleDiagnosisCode = document.CreateElement("Code");
                xmleDiagnosisCode.InnerText = icd_10_code;

                XmlElement xmleDiagnosisDiagnosisStartDate = document.CreateElement("DiagnosisStartDate");
                xmleDiagnosisDiagnosisStartDate.InnerText = Convert.ToDateTime(program_start_date).ToString("yyyyMMdd");
                XmlElement xmlePMBIndicator = document.CreateElement("PMBIndicator");
                xmlePMBIndicator.InnerText = "N";

                xmleDiagnosis.AppendChild(xmleDiagnosisStage);
                xmleDiagnosis.AppendChild(xmleDiagnosisCodeSet);
                xmleDiagnosis.AppendChild(xmleDiagnosisCode);
                xmleDiagnosis.AppendChild(xmleDiagnosisDiagnosisStartDate);
                xmleDiagnosis.AppendChild(xmlePMBIndicator);

                xmleAuthorisation.AppendChild(xmleDiagnosis);

                XmlElement xmleCollectionOfDrugs = xmleAuthorisation;
                string startDate = string.Empty, endDate = string.Empty;
                foreach (var row in scriptInfo.items)
                {
                    if (!string.IsNullOrEmpty(row.fromDate.ToString()))
                        startDate = Convert.ToDateTime(row.fromDate.ToString()).ToString("yyyyMMdd");

                    if (!string.IsNullOrEmpty(row.toDate.ToString()))
                        endDate = Convert.ToDateTime(row.toDate.ToString()).ToString("yyyyMMdd");
                    else
                        if (!string.IsNullOrEmpty(row.toDate.ToString()))
                    {
                        //calculate end date by adding number of months (cycle length) to start date
                        DateTime datEndDate = Convert.ToDateTime(row.fromDate.ToString());
                        datEndDate = datEndDate.AddMonths(Convert.ToInt32(row.itemRepeats.ToString()));
                        endDate = datEndDate.ToString("yyyyMMdd");
                    }

                    //<Detail>
                    XmlElement xmleDetail = document.CreateElement("Detail");

                    XmlElement xmleDetailCodeSet = document.CreateElement("CodeSet");
                    xmleDetailCodeSet.InnerText = "NP";
                    XmlElement xmleDetailCode = document.CreateElement("Code");
                    xmleDetailCode.InnerText = row.nappiCode.ToString();
                    XmlElement xmleDetailStatus = document.CreateElement("Status");
                    xmleDetailStatus.InnerText = "A";
                    XmlElement xmleDetailQuantity = document.CreateElement("Quantity");
                    xmleDetailQuantity.InnerText = row.quantity.ToString().Trim() + "00";
                    XmlElement xmleDetailInterval = document.CreateElement("Interval");
                    xmleDetailInterval.InnerText = row.itemRepeats.ToString();
                    XmlElement xmleDetailCoPaymentType = document.CreateElement("CoPaymentType");
                    xmleDetailCoPaymentType.InnerText = "R";
                    XmlElement xmleDetailCoPayment = document.CreateElement("CoPayment");
                    xmleDetailCoPayment.InnerText = "0";
                    XmlElement xmleDetailBenefitType = document.CreateElement("BenefitType");
                    xmleDetailBenefitType.InnerText = "HI";
                    XmlElement xmleDetailBenefitDescription = document.CreateElement("BenefitDescription");
                    xmleDetailBenefitDescription.InnerText = "HI";
                    XmlElement xmleDetailDetailNumber = document.CreateElement("DetailNumber");
                    xmleDetailDetailNumber.InnerText = row.itemNo.ToString();

                    XmlElement xmleDetailStartDate = document.CreateElement("StartDate");
                    xmleDetailStartDate.InnerText = startDate.Trim();
                    XmlElement xmleDetailEndDate = document.CreateElement("EndDate");
                    xmleDetailEndDate.InnerText = endDate.Trim();

                    XmlElement xmleBiologicalIndicator = document.CreateElement("BiologicalIndicator");
                    xmleBiologicalIndicator.InnerText = "N";

                    XmlElement xmleCycleLength = document.CreateElement("CycleLength");
                    xmleCycleLength.InnerText = "0";
                    XmlElement xmleRefPriceIndicator = document.CreateElement("RefPriceIndicator");
                    xmleRefPriceIndicator.InnerText = "N";

                    xmleDetail.AppendChild(xmleDetailCodeSet);
                    xmleDetail.AppendChild(xmleDetailCode);
                    xmleDetail.AppendChild(xmleDetailStatus);
                    xmleDetail.AppendChild(xmleDetailQuantity);
                    xmleDetail.AppendChild(xmleDetailInterval);
                    xmleDetail.AppendChild(xmleDetailCoPaymentType);
                    xmleDetail.AppendChild(xmleDetailCoPayment);
                    xmleDetail.AppendChild(xmleDetailBenefitType);
                    xmleDetail.AppendChild(xmleDetailBenefitDescription);
                    xmleDetail.AppendChild(xmleDetailDetailNumber);
                    xmleDetail.AppendChild(xmleDetailStartDate);
                    xmleDetail.AppendChild(xmleDetailEndDate);

                    xmleDetail.AppendChild(xmleBiologicalIndicator);
                    xmleDetail.AppendChild(xmleCycleLength);
                    xmleDetail.AppendChild(xmleRefPriceIndicator);

                    xmleCollectionOfDrugs.AppendChild(xmleDetail);

                }
                //add all main items to parent item
                xmleAuthorisationMessage.AppendChild(xmleTransaction);
                xmleAuthorisationMessage.AppendChild(xmleAuthorisation);

                RootNode.AppendChild(xmleAuthorisationMessage);
                UpdateAuthRequest(lognumber, document.InnerXml.ToString(), "", "System");
                return document;
            }
            catch (Exception ex)
            {
                string filePath = @"d:\Data\Logs\HC\Error.txt";
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Auth Error: " + Environment.NewLine + "Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }

                return null;
            }
        }

        public static XmlDocument GenerateAgilityGeneralChronicAuthXml(DataSet authData, bool dsp)
        {
            string membership_no = authData.Tables[0].Rows[0]["membership_no"].ToString();
            string dependent_code = authData.Tables[0].Rows[0]["member_no"].ToString();
            string medical_aid = authData.Tables[0].Rows[0]["medical_aid_code"].ToString();
            string scheme_code = authData.Tables[0].Rows[0]["plan_code"].ToString();
            string firstName = authData.Tables[0].Rows[0]["first_Name"].ToString();
            string lastName = authData.Tables[0].Rows[0]["Last_name"].ToString();
            string dateOfBirth = authData.Tables[0].Rows[0]["date_Of_Birth"].ToString();
            string practiceNo = authData.Tables[0].Rows[0]["bhf_practice_no"].ToString();
            string program_start_date = authData.Tables[0].Rows[0]["program_start_date"].ToString();
            string icd_10_code = authData.Tables[0].Rows[0]["icd_10_code"].ToString();

            XmlDocument document = new XmlDocument();
            document.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><AuthMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"></AuthMessage>");
            XmlNode RootNode = document.DocumentElement;

            //parent item
            XmlElement xmleAuthorisationMessage = document.CreateElement("AuthorisationMessage");

            //main items
            XmlElement xmleTransaction = document.CreateElement("Transaction");
            XmlElement xmleAuthorisation = document.CreateElement("Authorisation");

            //<transaction>
            XmlElement xmleVersionNumber = document.CreateElement("VersionNumber");
            xmleVersionNumber.InnerText = "2.0";
            XmlElement xmleType = document.CreateElement("Type");
            xmleType.InnerText = "AI";
            XmlElement xmleOriginator = document.CreateElement("Originator");
            xmleOriginator.InnerText = "ZOPHARM";
            XmlElement xmleNumber = document.CreateElement("Number");
            xmleNumber.InnerText = null; //LogAuthRequest(3010, authData.Tables[0].Rows[0]["script_no"].ToString(), DependantID, "", "", "UserName").ToString();
            XmlElement xmleDateTime = document.CreateElement("DateTime");
            xmleDateTime.InnerText = DateTime.Now.ToString("yyyyMMddHHmmss");
            XmlElement xmleDestinationCode = document.CreateElement("DestinationCode");
            xmleDestinationCode.InnerText = "120AZ";
            XmlElement xmleTestIndicator = document.CreateElement("TestIndicator");
            xmleTestIndicator.InnerText = "Y";

            xmleTransaction.AppendChild(xmleVersionNumber);
            xmleTransaction.AppendChild(xmleType);
            xmleTransaction.AppendChild(xmleOriginator);
            xmleTransaction.AppendChild(xmleNumber);
            xmleTransaction.AppendChild(xmleDateTime);
            xmleTransaction.AppendChild(xmleDestinationCode);
            xmleTransaction.AppendChild(xmleTestIndicator);

            //<authorisation>
            XmlElement xmleStatus = document.CreateElement("Status");
            xmleStatus.InnerText = "A";
            XmlElement xmleAuthorisationType = document.CreateElement("Type");
            xmleAuthorisationType.InnerText = "Diabetes";
            XmlElement xmleAuthorisationStartDate = document.CreateElement("AuthorisationStartDate");
            xmleAuthorisationStartDate.InnerText = DateTime.Now.ToString("yyyyMMdd");

            XmlElement xmleInHospitalisationIndicator = document.CreateElement("InHospitalisationIndicator");
            xmleInHospitalisationIndicator.InnerText = "N";
            XmlElement xmleDspIndicator = document.CreateElement("DspIndicator");
            if (dsp)
            {
                xmleDspIndicator.InnerText = "Y";
            }
            else
            {
                xmleDspIndicator.InnerText = "N";
            }

            xmleAuthorisation.AppendChild(xmleStatus);
            xmleAuthorisation.AppendChild(xmleAuthorisationType);
            xmleAuthorisation.AppendChild(xmleAuthorisationStartDate);
            xmleAuthorisation.AppendChild(xmleInHospitalisationIndicator);
            xmleAuthorisation.AppendChild(xmleDspIndicator);

            XmlElement xmleMember = document.CreateElement("Member");
            XmlElement xmleMemberMembershipNo = document.CreateElement("MembershipNumber");
            xmleMemberMembershipNo.InnerText = membership_no.Trim();
            XmlElement xmleMedicalSchemeCode = document.CreateElement("MedicalSchemeCode");
            xmleMedicalSchemeCode.InnerText = scheme_code;
            xmleMember.AppendChild(xmleMemberMembershipNo);
            xmleMember.AppendChild(xmleMedicalSchemeCode);

            XmlElement xmlPatient = document.CreateElement("Patient");
            XmlElement xmlePatientDependantCode = document.CreateElement("DependantCode");
            xmlePatientDependantCode.InnerText = dependent_code.Trim();
            XmlElement xmlePersonDetail = document.CreateElement("PersonalDetail");
            XmlElement xmlePersonDetailFirstName = document.CreateElement("FirstName");
            xmlePersonDetailFirstName.InnerText = firstName.Trim();
            XmlElement xmlePersonDetailDOB = document.CreateElement("DOB");
            string dob = dateOfBirth.Trim().Replace("-", "").Replace("/", "").Substring(0, 8);
            xmlePersonDetailDOB.InnerText = dob;
            xmlePersonDetail.AppendChild(xmlePersonDetailFirstName);
            xmlePersonDetail.AppendChild(xmlePersonDetailDOB);
            xmlPatient.AppendChild(xmlePatientDependantCode);
            xmlPatient.AppendChild(xmlePersonDetail);

            //add Authorisation child items
            xmleAuthorisation.AppendChild(xmleMember);
            xmleAuthorisation.AppendChild(xmlPatient);

            //provider 
            XmlElement xmlProvider = document.CreateElement("Provider");
            XmlElement xmleRole = document.CreateElement("Role");
            xmleRole.InnerText = "SP";
            XmlElement xmlePracticeNumber = document.CreateElement("PracticeNumber");
            xmlePracticeNumber.InnerText = practiceNo.Trim();
            xmlProvider.AppendChild(xmleRole);
            xmlProvider.AppendChild(xmlePracticeNumber);

            //add Authorisation child items
            xmleAuthorisation.AppendChild(xmlProvider);

            //GenerateDiagnosisXml(ref document, ref xmleAuthorisation, membershipNo.Trim(), dependantCode.Trim(), medicalAidCode.Trim());

            string stage = "P";  //Primary
            string codeSet = "ICD";
            string code = string.Empty;

            XmlElement xmleDiagnosis = document.CreateElement("Diagnosis");

            XmlElement xmleDiagnosisStage = document.CreateElement("Stage");
            xmleDiagnosisStage.InnerText = stage.Trim();
            XmlElement xmleDiagnosisCodeSet = document.CreateElement("CodeSet");
            xmleDiagnosisCodeSet.InnerText = codeSet.Trim();
            XmlElement xmleDiagnosisCode = document.CreateElement("Code");
            if (string.IsNullOrEmpty(icd_10_code))
            {
                xmleDiagnosisCode.InnerText = "E11.9";
            }
            else
            {
                xmleDiagnosisCode.InnerText = icd_10_code;
            }
            XmlElement xmleDiagnosisDiagnosisStartDate = document.CreateElement("DiagnosisStartDate");
            xmleDiagnosisDiagnosisStartDate.InnerText = Convert.ToDateTime(program_start_date).ToString("yyyyMMdd");
            XmlElement xmlePMBIndicator = document.CreateElement("PMBIndicator");
            xmlePMBIndicator.InnerText = "N";

            xmleDiagnosis.AppendChild(xmleDiagnosisStage);
            xmleDiagnosis.AppendChild(xmleDiagnosisCodeSet);
            xmleDiagnosis.AppendChild(xmleDiagnosisCode);
            xmleDiagnosis.AppendChild(xmleDiagnosisDiagnosisStartDate);
            xmleDiagnosis.AppendChild(xmlePMBIndicator);

            xmleAuthorisation.AppendChild(xmleDiagnosis);

            XmlElement xmleCollectionOfDrugs = xmleAuthorisation;
            string startDate = string.Empty, endDate = string.Empty;
            foreach (DataRow row in authData.Tables[0].Rows)
            {
                if (row["item_from_date"].ToString() != string.Empty)
                    startDate = Convert.ToDateTime(row["item_from_date"].ToString()).ToString("yyyyMMdd");

                if (row["item_to_date"].ToString() != string.Empty)
                    endDate = Convert.ToDateTime(row["item_to_date"].ToString()).ToString("yyyyMMdd");
                else
                    if (row["item_from_date"].ToString() != string.Empty)
                {
                    //calculate end date by adding number of months (cycle length) to start date
                    DateTime datEndDate = Convert.ToDateTime(row["item_from_date"].ToString());
                    datEndDate = datEndDate.AddMonths(Convert.ToInt32(row["no_of_repeats"].ToString()));
                    endDate = datEndDate.ToString("yyyyMMdd");
                }

                //<Detail>


                XmlElement xmleDetail = document.CreateElement("Detail");

                XmlElement xmleDetailCodeSet = document.CreateElement("CodeSet");
                xmleDetailCodeSet.InnerText = "NP";
                XmlElement xmleDetailCode = document.CreateElement("Code");
                xmleDetailCode.InnerText = row["product_code"].ToString();
                XmlElement xmleDetailStatus = document.CreateElement("Status");
                xmleDetailStatus.InnerText = "A";
                XmlElement xmleDetailQuantity = document.CreateElement("Quantity");
                xmleDetailQuantity.InnerText = row["quantity"].ToString().Trim() + "00";
                XmlElement xmleDetailInterval = document.CreateElement("Interval");
                xmleDetailInterval.InnerText = row["no_of_repeats"].ToString();
                XmlElement xmleDetailCoPaymentType = document.CreateElement("CoPaymentType");
                xmleDetailCoPaymentType.InnerText = "R";
                XmlElement xmleDetailCoPayment = document.CreateElement("CoPayment");
                xmleDetailCoPayment.InnerText = "0";
                XmlElement xmleDetailBenefitType = document.CreateElement("BenefitType");
                XmlElement xmleDetailBenefitDescription = document.CreateElement("BenefitDescription");

                if (!String.IsNullOrEmpty(row["BenefitType"].ToString()))
                {
                    switch (row["BenefitType"].ToString())
                    {
                        case "PMB":
                            {
                                xmleDetailBenefitType.InnerText = "PMB";
                                xmleDetailBenefitDescription.InnerText = "PMB";
                                break;
                            }
                        case "GLU":
                            {
                                xmleDetailBenefitType.InnerText = "GLU";
                                xmleDetailBenefitDescription.InnerText = "Glucometers";
                                break;
                            }
                        case "ACU":
                            {
                                xmleDetailBenefitType.InnerText = "ACU";
                                xmleDetailBenefitDescription.InnerText = "Acute";
                                break;
                            }
                        case "APA":
                            {
                                xmleDetailBenefitType.InnerText = "APA";
                                xmleDetailBenefitDescription.InnerText = "Appeal";
                                break;
                            }
                        case "EXG":
                            {
                                xmleDetailBenefitType.InnerText = "EXG";
                                xmleDetailBenefitDescription.InnerText = "Ex Gratia";
                                break;
                            }
                        case "DIA":
                            {
                                xmleDetailBenefitType.InnerText = "DIA";
                                xmleDetailBenefitDescription.InnerText = "Diabetes Care";
                                break;
                            }

                    }

                }
                else
                {
                    xmleDetailBenefitType.InnerText = "DB";
                    xmleDetailBenefitDescription.InnerText = "DB";
                }

                XmlElement xmleDetailDetailNumber = document.CreateElement("DetailNumber");
                xmleDetailDetailNumber.InnerText = "111" + row["item_no"].ToString();

                XmlElement xmleDetailStartDate = document.CreateElement("StartDate");
                xmleDetailStartDate.InnerText = startDate.Trim();
                XmlElement xmleDetailEndDate = document.CreateElement("EndDate");
                xmleDetailEndDate.InnerText = endDate.Trim();

                XmlElement xmleBiologicalIndicator = document.CreateElement("BiologicalIndicator");
                xmleBiologicalIndicator.InnerText = row["biological"].ToString();

                if (row["max_days_supply"].ToString() == string.Empty)
                {
                    row["max_days_supply"] = "0";
                }

                XmlElement xmleCycleLength = document.CreateElement("CycleLength");
                xmleCycleLength.InnerText = row["max_days_supply"].ToString().Replace("NOTSE", "0");
                XmlElement xmleRefPriceIndicator = document.CreateElement("RefPriceIndicator");
                xmleRefPriceIndicator.InnerText = "N";
                if (row["PriceRefIndicator"].ToString() != string.Empty)
                {
                    if (row["PriceRefIndicator"].ToString() == "True")
                    {
                        xmleRefPriceIndicator.InnerText = "Y";
                    }
                }


                xmleDetail.AppendChild(xmleDetailCodeSet);
                xmleDetail.AppendChild(xmleDetailCode);
                xmleDetail.AppendChild(xmleDetailStatus);
                xmleDetail.AppendChild(xmleDetailQuantity);
                xmleDetail.AppendChild(xmleDetailInterval);
                xmleDetail.AppendChild(xmleDetailCoPaymentType);
                xmleDetail.AppendChild(xmleDetailCoPayment);
                xmleDetail.AppendChild(xmleDetailBenefitType);
                xmleDetail.AppendChild(xmleDetailBenefitDescription);
                xmleDetail.AppendChild(xmleDetailDetailNumber);
                xmleDetail.AppendChild(xmleDetailStartDate);
                xmleDetail.AppendChild(xmleDetailEndDate);

                xmleDetail.AppendChild(xmleBiologicalIndicator);
                xmleDetail.AppendChild(xmleCycleLength);
                xmleDetail.AppendChild(xmleRefPriceIndicator);

                xmleCollectionOfDrugs.AppendChild(xmleDetail);

            }
            //add all main items to parent item
            xmleAuthorisationMessage.AppendChild(xmleTransaction);
            xmleAuthorisationMessage.AppendChild(xmleAuthorisation);

            RootNode.AppendChild(xmleAuthorisationMessage);

            return document;
        }

        public int LogAuthRequest(int clientNo, int scriptNo, Guid DependantID, string reqXml, string resXml, string User)
        {
            var request = new Authorisations()
            {
                dependantID = DependantID,
                scriptID = scriptNo,
                request = reqXml,
                response = resXml,
                source = "CUSTO",
                entryType = "AUTU",
                createdBy = User,
                createdDate = DateTime.Now,
                clientNo = clientNo,
            };
            var c = new ClinicalRepository(context, Configuration);
            var result = c.InsertAuthorizationRequest(request);

            return result;

        }

        public bool UpdateAuthRequest(int lognumber, string reqXml, string resXml, string User)
        { 
            var request = new Authorisations()
            {
                authID = lognumber,
                request = reqXml,
                response = resXml,
                source = "CUSTO",
                entryType = "AUTU",
                modifiedBy = User,
                modifiedDate = DateTime.Now,
            };

            var c = new ClinicalRepository(context, Configuration);
            var result = c.UpdateAuthorizationRequest(request);

            return true;

        }

    }
}