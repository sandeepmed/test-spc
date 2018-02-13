using ILMS.ApplicationSettingsManager;

namespace ILMS
{
    public static class clsAPI
    {
        public static string apiFlag = "QA";
        public static string apiStatus = "/administration/accountrequests/";
        public static string orgCredential = "/administration/organizations/$/apicredentials";
        public static string orgID = string.Empty;
        public static string userid = string.Empty;
        public static string ilmsconnectFormat = "customerid={OrgID}&sharedkey={Key}&userid={UserID}";

        public static string apiURI = ApplicationSettings.APIURI();
        public static string apiEnrollment = "/organizations/&/ecommgr/$/courses/#/enrollments";
        public static string apiExtendLicense = "/organizations/$/ecommgr/#/courses/seats/licensesperiod";
        public static string apiILMSConnect = "/organizations/$/ilmsconnect";
        public static string apiResetPasswordURL = "/organizations/$/users/#/password/reseturl";
        public static string EcomCourseSeats = "/organizations/$";
        public static string EcommMgrID = "/ecommgr/#/courses/seats";
        public static string EcommEnrolUser = "/organizations/{orgid}/ecommgr/{userid}/courses/{courseid}/enrollments";
        public static string apiOrg = "/organizations/$";
        public static string CourseEnrollment = "/organizations/{orgid}/Courses/{CourseID}/Enrollments";
        public static string user = "/organizations/{orgid}/Users";
        public static string CurriculumEnrollment = "/organizations/{orgid}/Curricula/{CourseID}/Enrollments";
        public static string SessionEnrollment = "/organizations/{orgid}/Courses/{CourseID}/Sessions/{SessionID}/Enrollments";
        public static string GetSessionID = "/organizations/{orgid}/Courses/{CourseID}/Sessions";
        public static string GetTranscript = "/organizations/{orgid}/Users/{UserID}/Transcripts";
        public static string GetAllGroups = "/organizations/{orgid}/Groups";
        public static string GetGroupByID = "/organizations/{orgid}/Groups/{GroupID}";
        public static string GetGroupMembers = "/organizations/{orgid}/Groups/{GroupID}/members";
        public static string GetGroupExplicitInclUsers = "/organizations/{orgid}/Groups/{GroupID}/ExplicitInclusionListUsers";
        public static string GetGroupExplicitExclUsers = "/organizations/{orgid}/Groups/{GroupID}/ExplicitExclusionListUsers";
        public static string AddUserToGroup = "/organizations/{orgID}/groups/{groupID}/members";
        public static string RemoveUserToGroup = "/organizations/{orgID}/groups/{groupID}/members/{userID}";
        public static string GetGroupCourses = "/organizations/{orgid}/Groups/{GroupID}/groupcourses";
        public static string DeleteCourseGroupCourses = "/organizations/{orgid}/Groups/{GroupID}/groupcourses/{CourseID}";
        public static string GetSingleUser = "/organizations/{orgID}/users/{UserID}";
        public static string GetUserBasedonEmail = "/organizations/{orgID}/users?F015={EmailID}";
        public static string UpdateUserProfile = "/organizations/{orgID}/users/{UserID}";
        public static string Region = "/organizations/{orgid}/regions";
        public static string updateRegion = "/organizations/{orgid}/regions/{regionID}";
        public static string GetAllDepartments = "/organizations/{orgid}/regions/{regionID}/Divisions/{divisionID}/departments";
        public static string GetSingleDepartment = "/organizations/{orgid}/regions/{regionID}/Divisions/{divisionID}/departments/{deptID}";
        public static string GetAllCourses = "/organizations/{orgid}/Courses";
        public static string GetSingleCourse = "/organizations/{orgid}/Courses/{CourseID}";
        public static string GetCourseSummaryReport = "/organizations/{orgid}/reports/enrollmentsummary?courseid={CourseID}";
        public static string GetSessionSummaryReport = "/organizations/{orgid}/reports/enrollmentsummary?courseid={CourseID}&sessionid={SessionID}";
        public static string GetCourseSummaryReportIncludeInactiveUsers = "/organizations/{orgid}/reports/enrollmentsummary?courseid={CourseID}&includeinactiveusers=true";
        public static string CourseDuedateSetting = "/organizations/{orgid}/Courses/{courseID}/duedate";



        ////////////////////////////transcript api
        public static string GetUserTranscript = "/organizations/{orgID}/users/{userid}/transcripts";
        public static string GetSpecificCourseTranscript = "/organizations/{orgID}/users/{userID}/courses/{courseID}/transcripts";
        public static string GetSpecificCurriculumTranscript = "/organizations/{orgID}/users/{userID}/curricula/{courseID}/transcripts";
        public static string GetSpecificSessionTranscript = "/organizations/{orgID}/users/{userID}/courses/{courseID}/sessions/{sessionID}/transcripts";
        public static string EnrollRecertification = "/organizations/{orgid}/Courses/{CourseID}/enrollments?InstanceEnrollment=true";

        ////////////////////////////curicula

        public static string GetCuriculla = "/organizations/{orgID}/curricula";
    }

    public class GeneralAPIErrCodes
    {
        public string[] allAPIErrorCodes = new string[188] 
        {"ER4001",
"ERCOU101",
"ERCOU102",
"ERCOU103",
"ERDEP103",
"ERDEP104",
"ERDEP106",
"ERDEP301",
"ERDEP310",
"ERDIV103",
"ERDIV104",
"ERDIV106",
"ERDIV201",
"ERDIV301",
"ERDIV310",
"ERDUE001",
"ERDUE002",
"ERDUE003",
"ERENR001",
"ERENR002",
"ERENR003",
"ERENR004",
"ERENR005",
"ERENR006",
"ERENR007",
"ERENR008",
"ERENR009",
"ERENR101",
"ERENR201",
"ERENR202",
"ERGRP101",
"ERORG201",
"ERPAS101",
"ERPAS102",
"ERPAS103",
"ERPAS104",
"ERPAS105",
"ERPAS106",
"ERREG101",
"ERREG102",
"ERREG103",
"ERREG104",
"ERREG106",
"ERREG201",
"ERREG202",
"ERREG301",
"ERTRN001",
"ERTRN002",
"ERTRN003",
"ERTRN004",
"ERTRN005",
"ERUSR100",
"ERUSR111",
"ERUSR112",
"ERUSR113",
"ERUSR114",
"ERUSR116",
"ERUSR117",
"ERUSR118",
"ERUSR123",
"ERUSR124",
"ERUSR125",
"ERUSR126",
"ERUSR127",
"ERUSR128",
"ERUSR129",
"ERUSR130",
"ERUSR133",
"ERUSR134",
"ERUSR138",
"ERUSR139",
"ERUSR140",
"ERUSR141",
"ERUSR142",
"ERUSR143",
"ERUSR144",
"ERUSR145",
"ERUSR146",
"ERUSR201",
"ERUSR2011",
"ERUSR2021",
"ERUSR203",
"ERUSR204",
"ERUSR205",
"ERUSR206",
"ERUSR207",
"ERUSR208",
"ERUSR209",
"ERUSR210",
"ERUSR211",
"ERUSR212",
"ERUSR213",
"ERUSR214",
"ERUSR215",
"ERUSR216",
"ERUSR217",
"ERUSR218",
"ERUSR219",
"ERUSR220",
"ERUSR221",
"ERUSR222",
"ERUSR223",
"ERUSR224",
"ERUSR225",
"ERUSR226",
"ERUSR227",
"ERUSR228",
"ERUSR229",
"ERUSR230",
"ERUSR232",
"ERUSR233",
"ERUSR234",
"ERUSR238",
"ERUSR312",
"ERUSR315",
"ERUSR401",
"ERUSR402",
"ERUSR501",
"ERUSR502",
"ERUSR503",
"ERUSR504",
"ERUSR505",
"ERUSR506",
"ERUSR507",
"ERUSR508",
"ERUSR509",
"ERUSR510",
"ERUSR511",
"ERUSR512",
"ERUSR513",
"ERUSR514",
"ERUSR515",
"ERUSR516",
"ERUSR517",
"ERUSR518",
"ERUSR519",
"ERUSR520",
"ERUSR521",
"ERUSR522",
"ERUSR523",
"ERUSR524",
"ERUSR525",
"ERUSR526",
"ERUSR527",
"ERUSR528",
"ERUSR529",
"ERUSR530",
"ERUSR532",
"ERUSR533",
"ERUSR534",
"ERUSR535",
"ERUSR536",
"ERUSR537",
"ERUSR538",
"ERUSR539",
"ERUSR601",
"ERUSR602",
"ERUSR603",
"ERUSR604",
"ERUSR605",
"ERUSR606",
"ERGRC102",
"ERGRC101",
"ERDUE101",
"ERGRP104",
"ERUSL101",
"ERGRP105",
"EREXI101",
"EREXI102",
"EREXE101",
"EREXE102",
"ERGRP506",
"ERDTF001",
"ERDTT002",
"ERDTT003",
"ERDTT004",
"ERDTT005",
"ERUSR540",
"ERUSR541",
"ERTRN006",
"ERTRN008",
"ERTRN009",
"ERTRN010",
"ERSES001",
"ERSES002",
"ERPAG001",
"ERPAG002",
"ERPAG003"
        };
        }


}
