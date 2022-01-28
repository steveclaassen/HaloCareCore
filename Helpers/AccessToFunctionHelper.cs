namespace HaloCareCore.Helpers
{
    public static class AccessToFunctionHelper
    {
        public static  bool CanView { set; get; }
        public static bool CanEdit { set; get; }
        public static bool CanAdd { set; get; }

        public static readonly string Settings = "Settings";
        public static readonly string PatientClinical = "PatientClinical";
        public static readonly string PatientSummary = "PatientSummary";
        public static readonly string PatientDashboard = "PatientDashboard";
        public static readonly string PatientAssignment = "PatientAssignment";
        public static readonly string AssignmentReport = "AssignmentReport";
        public static readonly string Search = "Search";
        public static readonly string PatientCommunication = "PatientCommunication";
    }
}