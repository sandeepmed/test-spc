using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System.Collections;
using System.Threading;
using ILMS.ApplicationSettingsManager;

namespace ILMS
{
    class clsAPIDivision
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _orgID = string.Empty;

        public clsAPIDivision(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }


        public bool AddDeleteDivision(object[] oValue)
        {
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable hTable = new Hashtable();
            hTable = oGeneric.GetTestData(oValue);

            switch (hTable["Function"].ToString().ToUpper().Trim())
            {
                case "ADD":
                    return AddDivision(hTable);
                //break;
                case "REMOVE":
                    return RemoveDivision(hTable);
                case "UPDATE":
                    return UpdateDivision(hTable);
                    //break;
            }


            return true;
        }

        public bool AddDivision(Hashtable hTable)
        {

            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string GroupResult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string strGroupID = string.Empty;

            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);
                clsRegionApi oRegion = new clsRegionApi();
                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;
                oRegion.name = hTable["RegionName"].ToString();

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oGetAPICredentials, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);


                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.Region.Replace("{orgid}", _orgID), oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                var RegionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);
                List<ILMS.GetRegion> t = new List<GetRegion>();
                var Regionid = RegionData.Where(cus => cus.Name == hTable["RegionName"].ToString());
                if (Regionid != null)
                {
                    oRegion.name = hTable["RenameDivision"].ToString();
                    t = Regionid.ToList();
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "Post", ApplicationSettings.APIURI() + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", t[0].ID.ToString()) + "/Divisions", oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);
                }




                Thread.Sleep(clsGlobalVariable.iWaitMedium);

                if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", t[0].ID.ToString()) + "/Divisions", oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                    var DivisionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                    _Flag = DivisionData.Any(cus => cus.Name == hTable["RenameDivision"].ToString());
                    List<ILMS.GetRegion> GetDivision = new List<GetRegion>();

                    if (_Flag == true)
                    {
                        GetDivision = DivisionData.Where(cus => cus.Name == hTable["RenameDivision"].ToString()).ToList();
                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", t[0].ID.ToString()) + "/Divisions/" + GetDivision[0].ID, oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                        var SingleDivisionData = JsonConvert.DeserializeObject<GetRegion>(result);
                        _Flag = true;

                    }


                    return _Flag;
                }
                else
                {
                    if (result.Contains("ErrorCode") || result.Contains("Invalid URI."))
                    {
                        if (result.Contains("Invalid URI."))
                        {
                            return true;
                        }


                        SuperAdminCoursePushDownErrorCode oError = new SuperAdminCoursePushDownErrorCode();
                        var error = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(result);

                        foreach (string errorcode in oError.coursePushDownErrorCode)
                        {
                            for (int i = 0; i < error.Count; i++)
                            {
                                if (error[i].ErrorCode == errorcode) { _Flag = true; }
                                else
                                {

                                    _Flag = false;
                                }
                            }
                            if (_Flag == true) break;
                        }
                        return _Flag;
                    }
                }
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }


            return true;
        }
        public bool UpdateDivision(Hashtable hTable)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string RegionResult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string strGroupID = string.Empty;

            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);
                clsRegionApi oRegion = new clsRegionApi();
                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;
                //  oRegion.name = hTable["Name"].ToString();

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oGetAPICredentials, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);


                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.Region.Replace("{orgid}", _orgID), oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                var RegionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                var Regionid = RegionData.Where(cus => cus.Name == hTable["RegionName"].ToString());
                List<ILMS.GetRegion> t = new List<GetRegion>();
                t = Regionid.ToList();

                if (t.Count > 0)
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", t[0].ID.ToString()) + "/Divisions", oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                    var DivisionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                    _Flag = DivisionData.Any(cus => cus.Name == hTable["DivisionName"].ToString());
                    List<ILMS.GetRegion> GetDivision = new List<GetRegion>();

                    if (_Flag == true)
                    {
                        GetDivision = DivisionData.Where(cus => cus.Name == hTable["DivisionName"].ToString()).ToList();
                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", t[0].ID.ToString()) + "/Divisions/" + GetDivision[0].ID, oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                        var SingleDivisionData = JsonConvert.DeserializeObject<GetRegion>(result);
                        _Flag = true;

                        oRegion.name = hTable["RenameDivision"].ToString();
                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "PUT", clsAPI.apiURI + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", t[0].ID.ToString()) + "/Divisions/" + GetDivision[0].ID, oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                    }


                }
                else { return false; }




                //   oGeneric.GetApiResponseCodeData(out statusCode, out result, "DELETE", ApplicationSettings.APIURI() + clsAPI.updateRegion.Replace("{orgid}", _orgID), oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);


                Thread.Sleep(clsGlobalVariable.iWaitMedium);

                if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", t[0].ID.ToString()) + "/Divisions", oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                    RegionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);
                    if (hTable["RenameDivision"].ToString() == "")
                    {
                        _Flag = RegionData.Any(cus => cus.Name == hTable["DivisionName"].ToString());
                    }
                    else { _Flag = RegionData.Any(cus => cus.Name == hTable["RenameDivision"].ToString()); }
                    return _Flag;
                }
                else
                {
                    if (result.Contains("ErrorCode") || result.Contains("Invalid URI."))
                    {
                        if (result.Contains("Invalid URI."))
                        {
                            return true;
                        }


                        SuperAdminCoursePushDownErrorCode oError = new SuperAdminCoursePushDownErrorCode();
                        var error = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(result);

                        foreach (string errorcode in oError.coursePushDownErrorCode)
                        {
                            for (int i = 0; i < error.Count; i++)
                            {
                                if (error[i].ErrorCode == errorcode) { _Flag = true; }
                                else
                                {

                                    _Flag = false;
                                }
                            }
                            if (_Flag == true) break;
                        }
                        return _Flag;
                    }
                }
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }


            return true;


        }
        public bool RemoveDivision(Hashtable hTable)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string RegionResult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string strGroupID = string.Empty;

            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);
                clsRegionApi oRegion = new clsRegionApi();
                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;
                //  oRegion.name = hTable["Name"].ToString();

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oGetAPICredentials, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);


                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.Region.Replace("{orgid}", _orgID), oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                var RegionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                var Regionid = RegionData.Where(cus => cus.Name == hTable["RegionName"].ToString());
                List<ILMS.GetRegion> t = new List<GetRegion>();
                t = Regionid.ToList();

                if (t.Count > 0)
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", t[0].ID.ToString()) + "/Divisions", oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                    var DivisionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                    _Flag = DivisionData.Any(cus => cus.Name == hTable["DivisionName"].ToString());
                    List<ILMS.GetRegion> GetDivision = new List<GetRegion>();

                    if (_Flag == true)
                    {
                        GetDivision = DivisionData.Where(cus => cus.Name == hTable["DivisionName"].ToString()).ToList();
                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", t[0].ID.ToString()) + "/Divisions/" + GetDivision[0].ID, oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                        var SingleDivisionData = JsonConvert.DeserializeObject<GetRegion>(result);
                        _Flag = true;

                        oRegion.name = hTable["DivisionName"].ToString();
                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "DELETE", clsAPI.apiURI + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", t[0].ID.ToString()) + "/Divisions/" + GetDivision[0].ID, oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                    }


                }
                else { return false; }




                //   oGeneric.GetApiResponseCodeData(out statusCode, out result, "DELETE", ApplicationSettings.APIURI() + clsAPI.updateRegion.Replace("{orgid}", _orgID), oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);


                Thread.Sleep(clsGlobalVariable.iWaitMedium);

                if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", t[0].ID.ToString()) + "/Divisions", oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                    RegionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                    _Flag = RegionData.Any(cus => cus.Name == hTable["DivisionName"].ToString());

                    if (_Flag == false) { return true; }

                    return _Flag;
                }
                else
                {
                    if (result.Contains("ErrorCode") || result.Contains("Invalid URI."))
                    {
                        if (result.Contains("Invalid URI."))
                        {
                            return true;
                        }


                        SuperAdminCoursePushDownErrorCode oError = new SuperAdminCoursePushDownErrorCode();
                        var error = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(result);

                        foreach (string errorcode in oError.coursePushDownErrorCode)
                        {
                            for (int i = 0; i < error.Count; i++)
                            {
                                if (error[i].ErrorCode == errorcode) { _Flag = true; }
                                else
                                {

                                    _Flag = false;
                                }
                            }
                            if (_Flag == true) break;
                        }
                        return _Flag;
                    }
                }
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }


            return true;

        }


    }
}
