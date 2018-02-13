using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using OpenQA.Selenium;
using ILMS.ApplicationSettingsManager;
using System;

namespace ILMS
{
    internal class clsCreateOrganization
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;

        public clsCreateOrganization(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }

        /// <summary>
        /// Function to create organization ,Copy super admin course to created org and create ecom manager
        /// </summary>
        /// <param name="lStrvalue">XML Parameter</param>
        /// <returns></returns>
        public bool APICreateOrganizationCopyCourse(object[] lStrvalue)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            bool _Flag = false;
            clsCoursePushDown clData = new clsCoursePushDown();

            Organization organization = new Organization();
            organization.Name = htblTestData["OrganizationName"].ToString();

            User user = new User();
            user.F001 = htblTestData["UserFirstName"].ToString();
            user.F003 = htblTestData["UserLastName"].ToString();
            user.F015 = htblTestData["UserEmail"].ToString();
            user.F023 = htblTestData["TimeZone"].ToString();
            user.F006 = htblTestData["Language"].ToString();

            List<SuperAdminCourseRecords> lstRecords = new List<SuperAdminCourseRecords>();

            string[] strCourses = htblTestData["SuperAdminCourseId"].ToString().Split(',');

            foreach (string strCourseID in strCourses)
            {
                SuperAdminCourseRecords course = new SuperAdminCourseRecords();
                course.NumberOfSeats = htblTestData["Seats"].ToString();
                course.SuperAdminCourseID = string.Empty;
                course.SuperAdminCourseID = strCourseID;
                lstRecords.Add(course);
            }

            clData.Organization = organization;
            clData.User = user;
            clData.SuperAdminCourseRecords = lstRecords;
            clData.OrderID = htblTestData["OrderID"].ToString();
            clData.PurchaseDate = htblTestData["PurchaseDate"].ToString();

            try
            {
                oGeneric.GetApiResponseCodeData(out statusCode, out result, htblTestData["MethodType"].ToString(), ApplicationSettings.APIURI() + htblTestData["URI"].ToString(), clData, htblTestData["HeaderType"].ToString(), "", "");

                if (htblTestData["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    if (result.ToUpper().Contains("!DOCTYPE"))
                    {
                        clsGlobalVariable.strExceptionReport = "Resource not found";
                        return true;
                    }

                    if (statusCode.Contains("201"))
                    {
                        RequestID oRequest = JsonConvert.DeserializeObject<RequestID>(result);
                        URI = ApplicationSettings.APIURI() + htblTestData["URI"].ToString() + "/" + oRequest.requestID.ToString();
                        result = string.Empty;
                        //   oGeneric.GetApiResponseCodeData(out statusCode, out result, "Get", URI, clData, htblTestData["HeaderType"].ToString(), "", "");
                        Thread.Sleep(20000);

                        AccountRequests oRequestData;
                        do
                        {
                            Thread.Sleep(10000);
                            oGeneric.GetApiResponseCodeData(out statusCode, out result, "Get", URI, clData, htblTestData["HeaderType"].ToString(), "", "");
                            Application.DoEvents();
                            oRequestData = JsonConvert.DeserializeObject<AccountRequests>(result);

                        } while (oRequestData.NewlyCreatedOrganizaitonInformation.OrganizationID == "0");

                        int courseCounter = 0;

                        clsAPI.orgID = oRequestData.NewlyCreatedOrganizaitonInformation.OrganizationID;
                        clsAPI.userid = oRequestData.NewlyCreatedeComMgrInformation.UserID;
                        foreach (string strCourseID in strCourses)
                        {
                            if (strCourseID == oRequestData.CourseResults[courseCounter].SuperAdminCourseID.ToString())
                            {
                                courseCounter++;
                                _Flag = true;
                            }
                            else
                            {
                                courseCounter++;
                                _Flag = false;
                            }
                        }

                        if (_Flag == true)
                        {
                            return true;
                        }
                        else { return false; }
                    }
                    else
                    {
                        var oError = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(result);
                        clsGlobalVariable.strExceptionReport = "Invalid URI: Error Code: " + oError[0].ErrorCode + "\n Error Description: " + oError[0].ErrorMessage;
                        return false;
                    }
                }
                else if (htblTestData["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                {
                    if (result.ToUpper().Contains("!DOCTYPE"))
                    {
                        clsGlobalVariable.strExceptionReport = "Resource not found";
                        return true;
                    }

                    if (statusCode.Contains("4") || statusCode.Contains("5"))
                    {
                        if (statusCode.Contains("404") || statusCode.Contains("500") || statusCode.Contains("405"))
                        {
                            clsGlobalVariable.strExceptionReport = "Resource not found";
                            return true;
                        }
                        var oError = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(result);
                        SuperAdminCoursePushDownErrorCode oErrorCode = new SuperAdminCoursePushDownErrorCode();
                        foreach (string strErrorCode in oErrorCode.coursePushDownErrorCode)
                        {
                            if (strErrorCode == oError[0].ErrorCode)
                            {
                                _Flag = true;
                            }
                        }
                        if (_Flag == true)
                        {
                            return true;
                        }
                        else
                        {
                            clsGlobalVariable.strExceptionReport = "Negative Case Failed!";
                            return false;
                        }
                    }
                    else
                    {
                        clsGlobalVariable.strExceptionReport = "Resource or URI Wrong!";
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }
        }

        /// <summary>
        /// Function to pass Account Request ID and get data related to Request
        /// </summary>
        /// <param name="lStrvalue">XML Parameter</param>
        /// <returns></returns>
        public bool APIGetAccountRequestStatus(object[] lStrvalue)
        {
            string strError = string.Empty;
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            string[] strCourseName = htblTestData["CourseName"].ToString().Split(',');

            bool _Flag = false;
            clsCoursePushDown clData = new clsCoursePushDown();
            try
            {
                oGeneric.GetApiResponseCodeData(out statusCode, out result, htblTestData["MethodType"].ToString(), ApplicationSettings.APIURI() + htblTestData["URI"].ToString(), clData, htblTestData["HeaderType"].ToString(), "", "");

                if (htblTestData["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    if (statusCode.Contains("201"))
                    {
                        var oAccountRequest = JsonConvert.DeserializeObject<AccountRequests>(result);
                        for (int courseNameCounter = 0; courseNameCounter < strCourseName.Length; courseNameCounter++)
                        {
                            for (int courseResultCounter = 0; courseResultCounter < oAccountRequest.CourseResults.Count; courseResultCounter++)
                            {
                                if (oAccountRequest.CourseResults[courseResultCounter].SuperAdminCourseID.ToString() == strCourseName[courseNameCounter])
                                {
                                    if (oAccountRequest.CourseResults[courseResultCounter].IsErrorOccurred != "true")
                                    {
                                        _Flag = true;
                                        break;
                                    }
                                    else { strError = oAccountRequest.CourseResults[courseResultCounter].ErrorDescription; }
                                }
                            }
                        }

                        if (_Flag == true)
                        {
                            return true;
                        }
                        else
                        {
                            clsGlobalVariable.strExceptionReport = strError;
                            return false;
                        }
                    }
                }
                else if (htblTestData["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                {
                    if (statusCode.Contains("40") || statusCode.Contains("50"))
                    {
                        return true;
                    }
                    else
                    {
                        clsGlobalVariable.strExceptionReport = "Negative Case Failed!";
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }
        }

        public bool APICopyCourseForExistingOrganization(object[] lStrvalue)
        {
            string header = string.Empty;

            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            bool _Flag = false;

            CopyCourseWithExistingOrganization oCopyCourseWithExistingOrganization = new CopyCourseWithExistingOrganization();
            SuperAdminCourseRecords oSuperAdminCourseRecords = new SuperAdminCourseRecords();

            List<SuperAdminCourseRecords> lstRecords = new List<SuperAdminCourseRecords>();

            string[] strCourses = htblTestData["CourseName"].ToString().Split(',');
            try
            {
                foreach (string strCourseID in strCourses)
                {
                    SuperAdminCourseRecords course = new SuperAdminCourseRecords();
                    course.NumberOfSeats = htblTestData["Seats"].ToString();
                    course.SuperAdminCourseID = string.Empty;
                    course.SuperAdminCourseID = strCourseID;
                    lstRecords.Add(course);
                }

                oCopyCourseWithExistingOrganization.UserID = htblTestData["USERID"].ToString();
                oCopyCourseWithExistingOrganization.SuperAdminCourseRecords = lstRecords;
                oCopyCourseWithExistingOrganization.OrderID = htblTestData["USERID"].ToString();
                oCopyCourseWithExistingOrganization.PurchaseDate = htblTestData["PurchaseDate"].ToString();
                #region
                if ((htblTestData["orgID"].ToString().Trim() == ""))
                {
                    lstRecords.Clear();

                    lstRecords = new List<SuperAdminCourseRecords>();

                    foreach (string strCourseID in strCourses)
                    {
                        SuperAdminCourseRecords course = new SuperAdminCourseRecords();
                        course.NumberOfSeats = htblTestData["Seats"].ToString();
                        course.SuperAdminCourseID = string.Empty;
                        course.SuperAdminCourseID = strCourseID;
                        lstRecords.Add(course);
                    }

                    oCopyCourseWithExistingOrganization.SuperAdminCourseRecords = lstRecords;
                    oCopyCourseWithExistingOrganization.UserID = clsAPI.userid;

                    GetAPICredentials oGetAPICredentials = new GetAPICredentials();

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", clsAPI.orgID), oCopyCourseWithExistingOrganization, "H2", "", "");

                    var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                    Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                    _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                    _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, htblTestData["MethodType"].ToString(), ApplicationSettings.APIURI() + htblTestData["URI"].ToString(), oCopyCourseWithExistingOrganization, htblTestData["HeaderType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                }
                else
                {
                    #endregion
                    _ecomTransactionKey = ApplicationSettings.TransactionKey();
                    _ecomLoginKey = ApplicationSettings.APILoginID();
                    clsAPI.orgID = htblTestData["orgID"].ToString();
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, htblTestData["MethodType"].ToString(), ApplicationSettings.APIURI() + htblTestData["URI"].ToString(), oCopyCourseWithExistingOrganization, htblTestData["HeaderType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                }

                if (htblTestData["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    if (result.ToUpper().Contains("!DOCTYPE"))
                    {
                        clsGlobalVariable.strExceptionReport = "Resource not found";
                        return true;
                    }

                    if (statusCode.Contains("201"))
                    {
                        header = htblTestData["HeaderType"].ToString();

                        RequestID oRequest = JsonConvert.DeserializeObject<RequestID>(result);
                        URI = ApplicationSettings.APIURI() + clsAPI.apiStatus + oRequest.requestID.ToString();
                        result = string.Empty;
                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "Get", URI, oCopyCourseWithExistingOrganization, "H2", "", "");
                        AccountRequests oRequestData = JsonConvert.DeserializeObject<AccountRequests>(result);
                        int courseCounter = 0;

                        foreach (string strCourseID in strCourses)
                        {
                            if (strCourseID == oRequestData.CourseResults[courseCounter].SuperAdminCourseID.ToString())
                            {
                                courseCounter++;
                                _Flag = true;
                            }
                            else
                            {
                                courseCounter++;
                                _Flag = false;
                            }
                        }

                        if (_Flag == true)
                        {
                            return true;
                        }
                        else { return false; }
                    }
                    else
                    {
                        var oError = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(result);
                        clsGlobalVariable.strExceptionReport = "Invalid URI: Error Code: " + oError[0].ErrorCode + "\n Error Description: " + oError[0].ErrorMessage;
                        return false;
                    }
                }
                else if (htblTestData["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                {
                    if (result.ToUpper().Contains("!DOCTYPE"))
                    {
                        clsGlobalVariable.strExceptionReport = "Resource not found";
                        return true;
                    }

                    if (statusCode.Contains("4") || statusCode.Contains("5"))
                    {
                        if (statusCode.Contains("404") || statusCode.Contains("500") || statusCode.Contains("405"))
                        {
                            clsGlobalVariable.strExceptionReport = "Resource not found";
                            return true;
                        }
                        var oError = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(result);
                        SuperAdminCoursePushDownErrorCode oErrorCode = new SuperAdminCoursePushDownErrorCode();
                        foreach (string strErrorCode in oErrorCode.coursePushDownErrorCode)
                        {
                            if (strErrorCode == oError[0].ErrorCode)
                            {
                                _Flag = true;
                            }
                        }
                        if (_Flag == true)
                        {
                            return true;
                        }
                        else
                        {
                            clsGlobalVariable.strExceptionReport = "Negative Case Failed!";
                            return false;
                        }
                    }
                    else
                    {
                        clsGlobalVariable.strExceptionReport = "Resource or URI Wrong!";
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }
        }


        public bool APICreateOrganization(object[] lStrvalue)
        {
            string header = string.Empty;

            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            bool _Flag = false;


            CreateOrganization oCreateOrganization = new CreateOrganization();
            CreateOrganizationUserData oCreateOrganizationUserData = new CreateOrganizationUserData();
            try
            {

                oCreateOrganization.Name = htblTestData["OrganizationName"].ToString().Trim();
                oCreateOrganization.FolderName = htblTestData["FolderName"].ToString().Trim();


                oCreateOrganizationUserData.F001 = htblTestData["UserFirstName"].ToString().Trim();
                oCreateOrganizationUserData.F002 = htblTestData["UserLastName"].ToString().Trim();
                oCreateOrganizationUserData.F003 = htblTestData["UserMiddleName"].ToString().Trim();
                oCreateOrganizationUserData.F008 = htblTestData["Address1"].ToString().Trim();
                oCreateOrganizationUserData.F009 = htblTestData["Address2"].ToString().Trim();
                oCreateOrganizationUserData.F010 = htblTestData["City"].ToString().Trim();
                oCreateOrganizationUserData.F011 = htblTestData["Country"].ToString().Trim();
                oCreateOrganizationUserData.F012 = htblTestData["State"].ToString().Trim();
                oCreateOrganizationUserData.F013 = htblTestData["Region"].ToString().Trim();
                oCreateOrganizationUserData.F014 = htblTestData["ZipCode"].ToString().Trim();
                oCreateOrganizationUserData.F015 = htblTestData["EmailID"].ToString().Trim();
                oCreateOrganizationUserData.F016 = htblTestData["Password"].ToString().Trim();
                oCreateOrganizationUserData.F017 = htblTestData["Phone"].ToString().Trim();
                oCreateOrganizationUserData.F018 = htblTestData["Fax"].ToString().Trim();
                oCreateOrganizationUserData.F023 = htblTestData["TimeZone"].ToString().Trim();

                oCreateOrganization.OrganizationAdmin = oCreateOrganizationUserData;


                oGeneric.GetApiResponseCodeData(out statusCode, out result, htblTestData["MethodType"].ToString().Trim(), clsAPI.apiURI + htblTestData["URI"].ToString().Trim(), oCreateOrganization, htblTestData["HeaderType"].ToString().Trim(), "", "");


                if (htblTestData["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    if (result.ToUpper().Contains("!DOCTYPE"))
                    {
                        clsGlobalVariable.strExceptionReport = "Resource not found";
                        return true;
                    }
                    if (statusCode.Contains("20"))
                    {

                        var oResult = JsonConvert.DeserializeObject<CreateOrganizationUserDataOutput>(result);
                        if (oResult.OrganizationName == htblTestData["OrganizationName"].ToString().Trim())
                        {
                            return true;
                        }
                        else
                        {
                            clsGlobalVariable.strExceptionReport = "Organization Not Found";
                            return false;
                        }
                    }
                }
                else if (htblTestData["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                {
                    if (result.ToUpper().Contains("!DOCTYPE"))
                    {

                        return true;
                    }

                    if (statusCode.Contains("40") || statusCode.Contains("50"))
                    {
                        if (statusCode.Contains("404") || statusCode.Contains("500") || statusCode.Contains("405"))
                        {

                            return true;
                        }
                        var oError = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(result);
                        SuperAdminCoursePushDownErrorCode oErrorCode = new SuperAdminCoursePushDownErrorCode();
                        foreach (string strErrorCode in oErrorCode.coursePushDownErrorCode)
                        {
                            for (int iError = 0; iError < oError.Count; iError++)
                            {
                                if (strErrorCode == oError[iError].ErrorCode)
                                {
                                    _Flag = true;
                                }
                            }
                        }

                        if (_Flag == true)
                        {
                            return true;
                        }
                        else
                        {

                            return false;
                        }
                    }
                    else
                    {
                        clsGlobalVariable.strExceptionReport = "Resource or URI Wrong!";
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }
        }
    }
}