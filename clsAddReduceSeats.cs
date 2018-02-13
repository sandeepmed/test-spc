using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Net;
using System.Windows.Forms;
using ILMS.ApplicationSettingsManager;

namespace ILMS
{
    class clsAddReduceSeats
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;

        //Construcotr
        public clsAddReduceSeats(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }


        public bool APIAddReduceSeats(object[] lStrvalue)
        {
            string uri = string.Empty;
            string uriApiCredential = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            bool _Flag = false;
            try
            {

                if (htblTestData["orgID"].ToString().Trim() != "")
                {
                    clsAPI.orgID = htblTestData["orgID"].ToString().Trim();
                }
                if (htblTestData["userid"].ToString().Trim() != "")
                {
                    clsAPI.userid = htblTestData["userid"].ToString().Trim();
                }

                APICallReduceSeats oAPICallReduceSeats = new APICallReduceSeats();


                List<APIAddReduceSeats> lstRecords = new List<APIAddReduceSeats>();
                string[] strCourses = htblTestData["OrgCourseID"].ToString().Split(',');/////change name as per xml tag name
                string[] strCourseSeats = htblTestData["NumberofSeats"].ToString().Split(',');/////change name as per xml tag name
                int iCounter = 0;


                foreach (string strCourseID in strCourses)
                {
                    APIAddReduceSeats oAPIAddReduceSeats = new APIAddReduceSeats();
                    oAPIAddReduceSeats.NumberOfSeats = string.Empty;
                    oAPIAddReduceSeats.NumberOfSeats = strCourseSeats[iCounter].ToString();/////change name as per xml tag name
                    oAPIAddReduceSeats.OrgCourseID = string.Empty;
                    oAPIAddReduceSeats.OrgCourseID = strCourseID;
                    lstRecords.Add(oAPIAddReduceSeats);

                    iCounter++;
                }

                oAPICallReduceSeats.OrgCourseRecords = lstRecords;
                oAPICallReduceSeats.OrderID = htblTestData["OrderID"].ToString();
                oAPICallReduceSeats.PurchaseDate = htblTestData["PurchaseDate"].ToString();
                //Application.DoEvents();
                GetAPICredentials oGetAPICredentials = new GetAPICredentials();

                uri = ApplicationSettings.APIURI() + clsAPI.EcomCourseSeats.Replace("$", clsAPI.orgID) + clsAPI.EcommMgrID.Replace("#", clsAPI.userid);
                uriApiCredential = ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", clsAPI.orgID);

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET",
                 uriApiCredential, oAPICallReduceSeats, "H2", _ecomLoginKey, _ecomTransactionKey);

                Thread.Sleep(clsGlobalVariable.iWaitHigh);

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "Get", uri, oAPICallReduceSeats, htblTestData["HeaderType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                Thread.Sleep(clsGlobalVariable.iWaitHigh);

                // APIGetCourseSeats courseSeats = new APIGetCourseSeats();

                var courseSeats = JsonConvert.DeserializeObject<List<APICourseSeats>>(result);


                Dictionary<string, string> dicOldCourseData = new Dictionary<string, string>();

                foreach (string strCourseID in strCourses)
                {
                    for (int i = 0; i < courseSeats.Count; i++)
                    {
                        if (strCourseID.Contains(courseSeats[i].OrgCourseID.ToString()))
                        {
                            dicOldCourseData.Add(strCourseID, courseSeats[i].TotalSeats.ToString());
                        }
                    }
                }


                oGeneric.GetApiResponseCodeData(out statusCode, out result, htblTestData["MethodType"].ToString(), uri, oAPICallReduceSeats, htblTestData["HeaderType"].ToString(), _ecomLoginKey, _ecomTransactionKey);


                //Thread.Sleep(clsGlobalVariable.iWaitSCORMUpload);
                do
                {
                    Thread.Sleep(clsGlobalVariable.iWaitHigh);
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "Get", uri, oAPICallReduceSeats, htblTestData["HeaderType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                    Thread.Sleep(5000);
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "Get", uri, oAPICallReduceSeats, htblTestData["HeaderType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                } while (result.Contains("OrgCourseRecords"));
                Thread.Sleep(clsGlobalVariable.iWaitHigh);

                if (result.ToUpper().Contains("!DOCTYPE"))
                {
                    if (htblTestData["TestCaseType"].ToString() == "Negative")
                    {
                        return true;
                    }
                    else
                    {
                        clsGlobalVariable.strExceptionReport = "Invalid URI";
                        return false;
                    }
                }

                var newCourseSeats = JsonConvert.DeserializeObject<List<APICourseSeats>>(result);


                Dictionary<string, string> dicNewCourseData = new Dictionary<string, string>();

                foreach (string strCourseID in strCourses)
                {
                    for (int i = 0; i < newCourseSeats.Count; i++)
                    {
                        if (strCourseID.Contains(newCourseSeats[i].OrgCourseID.ToString()))
                        {
                            dicNewCourseData.Add(strCourseID, newCourseSeats[i].TotalSeats);
                        }
                    }
                }

                int seatCounter = 0;
                foreach (string strCourseName in strCourses)
                {
                    int oldSeats = 0;
                    int newSeats = 0;
                    try
                    {
                        oldSeats = Convert.ToInt32(dicOldCourseData[strCourseName]);
                        newSeats = Convert.ToInt32(dicNewCourseData[strCourseName]);
                    }
                    catch (Exception ex) { }
                    try
                    {
                        if (Convert.ToInt32(strCourseSeats[seatCounter]) == Convert.ToInt32((newSeats - oldSeats)))
                        {
                            _Flag = true;
                        }
                    }
                    catch (FormatException eFormat)
                    {
                        if (htblTestData["TestCaseType"].ToString() == "Negative")
                        {
                            return true;
                        }
                        else
                        {
                            clsGlobalVariable.strExceptionReport = "Invalid Seats";
                            return false;
                        }
                    }


                    seatCounter++;
                }


                if (_Flag == true)
                { return true; }
                else { return false; }

                return true;
            }
            catch (WebException ex) { return false; }



        }



    }
}
