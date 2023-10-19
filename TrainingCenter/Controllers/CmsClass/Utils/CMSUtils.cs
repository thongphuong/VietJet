
using Microsoft.Web.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for CMSUtils
/// </summary>
public class CMSUtils
{
    #region Init

    #endregion
    public static int POID { get { return 1; } }
    // có xài,có repo
    //typelog = 1 = course
    //typelog = 2 = assigntrainee
    //typelog = 3 = subject result
    //typelog = 4 = course result
    // có xài,ko repo
    #region [public static DataTable GetDocumentsByStore]
    public static DataTable GetDocumentsByStore(string[] parameterName, object[] parameterValue, object storeName)
    {
        var pars = new SqlParameter[parameterName.Length];
        for (int i = 0; i < pars.Length; i++)
        {
            pars[i] = new SqlParameter(parameterName[i], parameterValue[i]);
        }
        var tb = MBMSqlHelper.ExecuteDataTable(storeName.ToString(), pars);
        return tb;
    }
    #endregion
    #region [public static DataTable GetQuery]
    public static DataTable GetQuery(string Query)
    {
        string sampleQuery = " ##Query## ";

        if (!string.IsNullOrEmpty(Query))
            sampleQuery = sampleQuery.Replace("##Query##", Query);
        else
            sampleQuery = sampleQuery.Replace("##Query##", "");

        return MBMSqlHelper.ExecuteDataTable(sampleQuery);
    }
    #endregion
    public static string alert(string type, string content)
    {
        StringBuilder html = new StringBuilder();
        switch (type)
        {
            case "success":
                html.AppendFormat("<div class='alert alert-success'><a href='javascript:void(0)' class='close' data-dismiss='alert' aria-label='close'>&times;</a>{0}</div>", content);
                break;
            case "info":
                html.AppendFormat("<div class='alert alert-info'><a href='javascript:void(0)' class='close' data-dismiss='alert' aria-label='close'>&times;</a>{0}</div>", content);
                break;
            case "warning":
                html.AppendFormat("<div class='alert alert-warning'><a href='javascript:void(0)' class='close' data-dismiss='alert' aria-label='close'>&times;</a>{0}</div>", content);
                break;
            case "danger":
                html.AppendFormat("<div class='alert alert-danger'><a href='javascript:void(0)' class='close' data-dismiss='alert' aria-label='close'>&times;</a>{0}</div>", content);
                break;
        }
        return html.ToString();
    }
    public static Boolean checkInjection(string userInput)
    {
        bool isSQLInjection = false;
        string[] sqlCheckList = { "--",
                                       ";--",
                                       ";",
                                       "/*",
                                       "*/",
                                        "@@",
                                        "@",
                                        "char",
                                       "nchar",
                                       "varchar",
                                       "nvarchar",
                                       "alter",
                                       "begin",
                                       "cast",
                                       "create",
                                       "cursor",
                                       "declare",
                                       "delete",
                                       "drop",
                                       "end",
                                       "exec",
                                       "execute",
                                       "fetch",
                                            "insert",
                                          "kill",
                                             "select",
                                           "sys",
                                            "sysobjects",
                                            "syscolumns",
                                           "table",
                                           "update"
                                       };
        string CheckString = userInput.Replace("'", "''");
        for (int i = 0; i <= sqlCheckList.Length - 1; i++)
        {
            if ((CheckString.IndexOf(sqlCheckList[i],
StringComparison.OrdinalIgnoreCase) >= 0))
            { isSQLInjection = true; }
        }
        return isSQLInjection;
    }
    public static object[] SetDBNullobject(object[] input)
    {
        object[] values = new object[] { };

        for (int i = 0; i < input.Length; i++)
        {
            object temp = DBNull.Value;
            if (!CMSUtils.IsNull(input[i]))
            {
                temp = input[i];
            }
            input[i] = temp;
        }
        return input;
    }
    public static object[] SetObjectNull(object[] arr_coursefrom)
    {
        if (!CMSUtils.IsNull(arr_coursefrom))
        {
            for (int i = 0; i < arr_coursefrom.Length; i++)
            {
                if (CMSUtils.IsNull(arr_coursefrom[i]))
                {
                    arr_coursefrom[i] = null;
                }
                else
                {
                    if (arr_coursefrom[i].ToString() == "-1")
                    {
                        arr_coursefrom[i] = null;
                    }
                }
            }
        }
        return arr_coursefrom;
    }
    //============================ Check Null =====================================================================//
    #region [public static bool IsNull(object value)]
    public static bool IsNull(object value)
    {
        bool result = true;

        if (value != DBNull.Value && value != null && !String.IsNullOrEmpty(value.ToString()))
        {
            result = false;
        }

        return result;
    }
    #endregion
    #region [public static bool IsNull(string value)]
    public static bool IsNull(string value)
    {
        bool result = true;

        if (value != null && !String.IsNullOrEmpty(value.ToString()))
        {
            result = false;
        }

        return result;
    }
    #endregion
    // không xài
    #region[Vietjet]

    //public static string Compare_Two_string(string After = "", string Before = "")
    //{
    //    HtmlDiff diffHelper = new HtmlDiff(Before, After);
    //    return diffHelper.Build().ToString();
    //}


    #endregion































    //============================ Account =======================================================================//
    #region [public static void CheckUserAuthenticate(object sender)]
    public static void CheckUserAuthenticate(object sender)
    {
        if (!HttpContext.Current.User.Identity.IsAuthenticated)
        {
            HttpContext.Current.Response.Redirect("/login.aspx");
        }
        else
        {
            DataTable dt = CMSUtils.GetDataSQL("", "SYSTEM_Account", "AccountID,IsActivate",
                String.Format("AccountName='{0}'", CMSUtils.GetSafeString(HttpContext.Current.User.Identity.Name)), "");
            if (dt.Rows.Count <= 0)
            {
                HttpContext.Current.Response.Redirect("/login.aspx");
            }
            else
            {
                if (!CMSUtils.IsNull(dt.Rows[0]["IsActivate"]))
                {
                    if (!bool.Parse(dt.Rows[0]["IsActivate"].ToString()))
                    {
                        HttpContext.Current.Response.Redirect("/403.aspx");
                    }
                }
                else
                {
                    HttpContext.Current.Response.Redirect("/403.aspx");
                }
            }
        }
    }
    #endregion
    #region [public static String HashPassword(String password, String salt)]
    public static String HashPassword(String password, String salt)
    {
        var combinedPassword = String.Concat(password, salt);
        var sha256 = new SHA256Managed();
        var bytes = UTF8Encoding.UTF8.GetBytes(combinedPassword);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    #endregion
    #region [public static Boolean AuthenticateUser(string userName, string passWord)]
    public static Boolean AuthenticateUser(string userName, string passWord)
    {
        string userId = "-1";
        SqlParameter[] pars = new SqlParameter[2];
        SqlParameter singleParU = new SqlParameter("@Username", userName);
        SqlParameter singleParP = new SqlParameter("@Password", HashPassword(passWord, "hethongsoS2T"));
        pars[0] = singleParU;
        pars[1] = singleParP;
        userId = MBMSqlHelper.ExecuteScalar("Validate_User", pars, true).ToString();
        switch (userId)
        {
            case "-1":
                return false;
            default:
                return true;
        }
    }
    #endregion
    #region [public static Boolean CheckRoles(string pageName, string checkRole)]
    public static Boolean CheckRoles(string pageName, string checkRole)
    {
        DataTable tbAccount = CMSUtils.GetDataSQL("", "SYSTEM_Account", "GroupAccountID", "AccountName = '" + HttpContext.Current.User.Identity.Name + "'", "");

        if (tbAccount.Rows.Count > 0)
        {
            object pageID = CMSUtils.GetDataSQL("", "SYSTEM_Page", "PageID", String.Format("PageName like N'{0}'", GetSafeString(pageName)), "");

            if (pageID != null)
            {
                DataTable tbGroupAccountPage = CMSUtils.GetDataSQL("", "SYSTEM_GroupAccount_Page", "IsView,IsCreate,IsEdit,IsDelete", "GroupAccountID = " + tbAccount.Rows[0]["GroupAccountID"].ToString() + " AND PageID = N'" + pageID + "'", "");
                if (tbGroupAccountPage.Rows.Count > 0)
                {
                    if (tbGroupAccountPage.Rows[0][checkRole] != null)
                    {
                        if (tbGroupAccountPage.Rows[0][checkRole].ToString() == "True")
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    #endregion
    //============================ Check Role Account ============================================================//
    #region [public static bool IsGlobalAdmin()]
    public static bool IsGlobalAdmin()
    {
        if (CMSContext.CurrentAccount.GroupAccountID == 1)
        {
            return true;
        }
        return false;
    }
    #endregion
    #region [public static bool IsView()]
    public static bool IsView()
    {
        if (IsGlobalAdmin()) return true;
        if (CMSContext.CurrentAccount.POID != CMSUtils.POID) return false;
        object xem = CMSUtils.GetDataSQL("View_SYSTEM_GroupAccount_Page_Joined", "IsView",
            String.Format("GroupAccountID={0} AND PageID={1}", CMSContext.CurrentAccount.GroupAccountID,
               CMSContext.CurrentPage.PageID),
            "");
        if (CMSUtils.IsBoolean(xem))
        {
            if (bool.Parse(xem.ToString())) return true;
        }
        return false;
    }
    #endregion
    #region [public static bool IsView(object trangID)]
    public static bool IsView(object trangID)
    {
        if (IsGlobalAdmin()) return true;
        if (CMSContext.CurrentAccount.POID != CMSUtils.POID) return false;
        object xem = CMSUtils.GetDataSQL("View_SYSTEM_GroupAccount_Page_Joined", "IsView",
            String.Format("GroupAccountID={0} AND PageID={1}", CMSContext.CurrentAccount.GroupAccountID,
                trangID),
            "");
        if (CMSUtils.IsBoolean(xem))
        {
            if (bool.Parse(xem.ToString())) return true;
        }
        return false;
    }
    #endregion
    #region [public static bool IsAdd()]
    public static bool IsAdd()
    {
        if (IsGlobalAdmin()) return true;
        if (CMSContext.CurrentAccount.POID != CMSUtils.POID) return false;
        object them = CMSUtils.GetDataSQL("View_SYSTEM_GroupAccount_Page_Joined", "Them",
            String.Format("GroupAccountID={0} AND PageID={1}", CMSContext.CurrentAccount.GroupAccountID,
                CMSContext.CurrentPage.PageID),
            "");
        if (CMSUtils.IsBoolean(them))
        {
            if (bool.Parse(them.ToString())) return true;
        }
        return false;
    }
    #endregion
    #region [public static bool IsEdit()]
    public static bool IsEdit()
    {
        if (IsGlobalAdmin()) return true;
        if (CMSContext.CurrentAccount.POID != CMSUtils.POID) return false;
        object sua = CMSUtils.GetDataSQL("View_SYSTEM_GroupAccount_Page_Joined", "IsEdit",
            String.Format("GroupAccountID={0} AND PageID={1}", CMSContext.CurrentAccount.GroupAccountID,
                CMSContext.CurrentPage.PageID),
            "");
        if (CMSUtils.IsBoolean(sua))
        {
            if (bool.Parse(sua.ToString())) return true;
        }
        return false;
    }
    #endregion
    #region [public static bool IsDelete()]
    public static bool IsDelete()
    {
        if (IsGlobalAdmin()) return true;
        if (CMSContext.CurrentAccount.POID != CMSUtils.POID) return false;
        object xoa = CMSUtils.GetDataSQL("View_SYSTEM_GroupAccount_Page_Joined", "IsDelete",
            String.Format("GroupAccountID={0} AND PageID={1}", CMSContext.CurrentAccount.GroupAccountID,
                CMSContext.CurrentPage.PageID),
            "");
        if (CMSUtils.IsBoolean(xoa))
        {
            if (bool.Parse(xoa.ToString())) return true;
        }
        return false;
    }
    #endregion

    //============================ Insert =========================================================================//
    #region [public static object InsertDataAndReturnValueSQL(string tableName, string[] column, object[] data)]
    public static object InsertDataAndReturnValueSQL(string tableName, string[] column, object[] data)
    {
        string content = "";
        string sampleQuery = "INSERT INTO ##TABLE## (##COLUMNS##) VALUES (##DATA##); SELECT scope_identity();";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Xóa tất cả dữ liệu rỗng insert vào database
        CMSUtils.GetAndRemoveNullValue(ref column, ref data);

        //Nối các column
        string columns = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                columns += column[i];
            }
            else
            {
                columns += column[i] + ",";
            }
        }

        #region [Insert column CreateDay, CreateAccount, EditDay, EditAccount]
        columns += ",CreateDay,CreateAccount,EditDay,EditAccount";
        #endregion

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");

        //Khai báo parameter
        string param = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                param += "@" + column[i];
            }
            else
            {
                param += "@" + column[i] + ",";
            }
        }

        #region [Insert params CreateDay, CreateAccount, EditDay, EditAccount]
        param += ",@" + "CreateDay" + ",";
        param += "@" + "CreateAccount" + ",";
        param += "@" + "EditDay" + ",";
        param += "@" + "EditAccount";
        #endregion

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##DATA##", param);
        else
            sampleQuery = sampleQuery.Replace("##DATA##", "");


        SqlParameter[] par = new SqlParameter[data.Length + 4];
        for (int i = 0; i < data.Length; i++)
        {
            SqlParameter singlePar = new SqlParameter("@" + column[i], data[i]);
            par[i] = singlePar;
        }

        #region [Insert CreateDay, CreateAccount, EditDay, EditAccount]
        par[data.Length] = new SqlParameter("@" + "CreateDay", DateTime.Now);
        par[data.Length + 1] = new SqlParameter("@" + "CreateAccount", CMSContext.CurrentAccount.AccountID);
        par[data.Length + 2] = new SqlParameter("@" + "EditDay", DateTime.Now);
        par[data.Length + 3] = new SqlParameter("@" + "EditAccount", CMSContext.CurrentAccount.AccountID);
        #endregion

        object result = MBMSqlHelper.ExecuteScalar(sampleQuery, par, false);
        if (!CMSUtils.IsNull(result))
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (column[i].ToLower().Contains("Password"))
                {
                    content += String.Format("Cột {0}: Đã tạo mật khẩu mới <br/>", column[i]);
                    continue;
                }
                content += String.Format("Cột {0}: {1} <br/>", column[i], data[i]);
            }
            new EventLogProvider().LogInformation("Insert", CMSContext.CurrentPage.PageName, content);
        }

        return result;
    }
    #endregion
    #region [public static object InsertDataAndReturnValueSQLToNewConnectionStrings(string NewConnectionStrings,string tableName, string[] column, object[] data)]
    public static object InsertDataAndReturnValueSQLToNewConnectionStrings(string NewConnectionStrings, string tableName, string[] column, object[] data)
    {
        string content = "";
        string sampleQuery = "INSERT INTO ##TABLE## (##COLUMNS##) VALUES (##DATA##); SELECT scope_identity();";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Xóa tất cả dữ liệu rỗng insert vào database
        CMSUtils.GetAndRemoveNullValue(ref column, ref data);

        //Nối các column
        string columns = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                columns += column[i];
            }
            else
            {
                columns += column[i] + ",";
            }
        }

        #region [Insert column CreateDay, CreateAccount, EditDay, EditAccount]
        columns += ",CreateDay,CreateAccount,EditDay,EditAccount";
        #endregion

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");

        //Khai báo parameter
        string param = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                param += "@" + column[i];
            }
            else
            {
                param += "@" + column[i] + ",";
            }
        }

        #region [Insert params CreateDay, CreateAccount, EditDay, EditAccount]
        param += ",@" + "CreateDay" + ",";
        param += "@" + "CreateAccount" + ",";
        param += "@" + "EditDay" + ",";
        param += "@" + "EditAccount";
        #endregion

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##DATA##", param);
        else
            sampleQuery = sampleQuery.Replace("##DATA##", "");


        SqlParameter[] par = new SqlParameter[data.Length + 4];
        for (int i = 0; i < data.Length; i++)
        {
            SqlParameter singlePar = new SqlParameter("@" + column[i], data[i]);
            par[i] = singlePar;
        }

        #region [Insert CreateDay, CreateAccount, EditDay, EditAccount]
        par[data.Length] = new SqlParameter("@" + "CreateDay", DateTime.Now);
        par[data.Length + 1] = new SqlParameter("@" + "CreateAccount", CMSContext.CurrentAccount.AccountID);
        par[data.Length + 2] = new SqlParameter("@" + "EditDay", DateTime.Now);
        par[data.Length + 3] = new SqlParameter("@" + "EditAccount", CMSContext.CurrentAccount.AccountID);
        #endregion

        object result = MBMSqlHelperNewConnectionStrings.ExecuteScalar(GetConnetion(NewConnectionStrings), sampleQuery, par, false);
        if (!CMSUtils.IsNull(result))
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (column[i].ToLower().Contains("Password"))
                {
                    content += String.Format("Cột {0}: Đã tạo mật khẩu mới <br/>", column[i]);
                    continue;
                }
                content += String.Format("Cột {0}: {1} <br/>", column[i], data[i]);
            }
            new EventLogProviderToNewConnectionStrings().LogInformation(GetConnetion(NewConnectionStrings), "Insert", CMSContext.CurrentPage.PageName, content);
        }

        return result;
    }
    #endregion
    //----
    #region [public static int InsertDataSQLNoLog(string tableName, string[] column, object[] data)]
    public static int InsertDataSQLNoLog(string tableName, string[] column, object[] data)
    {
        string sampleQuery = "INSERT INTO ##TABLE## (##COLUMNS##) VALUES (##DATA##)";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Xóa tất cả dữ liệu rỗng insert vào database
        CMSUtils.GetAndRemoveNullValue(ref column, ref data);

        //Nối các column
        string columns = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                columns += column[i];
            }
            else
            {
                columns += column[i] + ",";
            }
        }

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");

        //Khai báo parameter
        string param = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                param += "@" + column[i];
            }
            else
            {
                param += "@" + column[i] + ",";
            }
        }

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##DATA##", param);
        else
            sampleQuery = sampleQuery.Replace("##DATA##", "");


        SqlParameter[] par = new SqlParameter[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            SqlParameter singlePar = new SqlParameter("@" + column[i], data[i]);
            par[i] = singlePar;
        }
        var result = MBMSqlHelper.ExecuteNonQuery(sampleQuery, par, false);
        return result;
    }


    public static object InsertDataSQLNoLogAndReturnID(string tableName, string[] column, object[] data, string id)
    {
        string sampleQuery = "INSERT INTO ##TABLE## (##COLUMNS##)  OUTPUT INSERTED.##id##  VALUES (##DATA##) ";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Xóa tất cả dữ liệu rỗng insert vào database
        CMSUtils.GetAndRemoveNullValue(ref column, ref data);

        //Nối các column
        string columns = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                columns += column[i];
            }
            else
            {
                columns += column[i] + ",";
            }
        }

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");
        if (!string.IsNullOrEmpty(id))
            sampleQuery = sampleQuery.Replace("##id##", id);
        else
            sampleQuery = sampleQuery.Replace("##id##", "");
        //Khai báo parameter
        string param = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                param += "@" + column[i];
            }
            else
            {
                param += "@" + column[i] + ",";
            }
        }

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##DATA##", param);
        else
            sampleQuery = sampleQuery.Replace("##DATA##", "");


        SqlParameter[] par = new SqlParameter[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            SqlParameter singlePar = new SqlParameter("@" + column[i], data[i]);
            par[i] = singlePar;
        }

        return MBMSqlHelper.ExecuteScalar(sampleQuery, par, false);
    }
    #endregion
    #region [public static int InsertDataSQLNoLogToNewConnectionStrings(string NewConnectionStrings, string tableName, string[] column, object[] data)]
    public static int InsertDataSQLNoLogToNewConnectionStrings(string NewConnectionStrings, string tableName, string[] column, object[] data)
    {
        string sampleQuery = "INSERT INTO ##TABLE## (##COLUMNS##) VALUES (##DATA##)";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Xóa tất cả dữ liệu rỗng insert vào database
        CMSUtils.GetAndRemoveNullValue(ref column, ref data);

        //Nối các column
        string columns = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                columns += column[i];
            }
            else
            {
                columns += column[i] + ",";
            }
        }

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");

        //Khai báo parameter
        string param = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                param += "@" + column[i];
            }
            else
            {
                param += "@" + column[i] + ",";
            }
        }

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##DATA##", param);
        else
            sampleQuery = sampleQuery.Replace("##DATA##", "");


        SqlParameter[] par = new SqlParameter[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            SqlParameter singlePar = new SqlParameter("@" + column[i], data[i]);
            par[i] = singlePar;
        }

        return MBMSqlHelperNewConnectionStrings.ExecuteNonQuery(GetConnetion(NewConnectionStrings), sampleQuery, par, false);
    }
    #endregion
    //----
    #region [public static int InsertDataSQL(string tableName, string[] column, object[] data)]
    public static int InsertDataSQL(string tableName, string[] column, object[] data)
    {
        string content = "";
        string sampleQuery = "INSERT INTO ##TABLE## (##COLUMNS##) VALUES (##DATA##)";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Xóa tất cả dữ liệu rỗng insert vào database
        CMSUtils.GetAndRemoveNullValue(ref column, ref data);

        //Nối các column
        string columns = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                columns += column[i];
            }
            else
            {
                columns += column[i] + ",";
            }
        }

        #region [Insert column CreateDay, CreateAccount, EditDay, EditAccount]
        columns += ",CreateDay,CreateAccount,EditDay,EditAccount";
        #endregion

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");

        //Khai báo parameter
        string param = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                param += "@" + column[i];
            }
            else
            {
                param += "@" + column[i] + ",";
            }
        }

        #region [Insert params CreateDay, CreateAccount, EditDay, EditAccount]
        param += ",@" + "CreateDay" + ",";
        param += "@" + "CreateAccount" + ",";
        param += "@" + "EditDay" + ",";
        param += "@" + "EditAccount";
        #endregion

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##DATA##", param);
        else
            sampleQuery = sampleQuery.Replace("##DATA##", "");


        SqlParameter[] par = new SqlParameter[data.Length + 4];
        for (int i = 0; i < data.Length; i++)
        {
            SqlParameter singlePar = new SqlParameter("@" + column[i], data[i]);
            par[i] = singlePar;
        }

        #region [Insert values CreateDay, CreateAccount, EditDay, EditAccount]
        par[data.Length] = new SqlParameter("@" + "CreateDay", DateTime.Now);
        par[data.Length + 1] = new SqlParameter("@" + "CreateAccount", CMSContext.CurrentAccount.AccountID);
        par[data.Length + 2] = new SqlParameter("@" + "EditDay", DateTime.Now);
        par[data.Length + 3] = new SqlParameter("@" + "EditAccount", CMSContext.CurrentAccount.AccountID);
        #endregion

        int result = MBMSqlHelper.ExecuteNonQuery(sampleQuery, par, false);

        if (result > 0)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (column[i].ToLower().Contains("Password"))
                {
                    content += String.Format("Cột {0}: Đã tạo mật khẩu mới <br/>", column[i]);
                    continue;
                }
                content += String.Format("Cột {0}: {1} <br/>", column[i], data[i]);
            }
            new EventLogProvider().LogInformation("Insert", CMSContext.CurrentPage.PageName, content);
        }

        return result;
    }
    #endregion
    #region [public static int InsertDataSQLToNewConnectionStrings(string NewConnectionStrings,string tableName, string[] column, object[] data)]
    public static int InsertDataSQLToNewConnectionStrings(string NewConnectionStrings, string tableName, string[] column, object[] data)
    {
        string content = "";
        string sampleQuery = "INSERT INTO ##TABLE## (##COLUMNS##) VALUES (##DATA##)";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Xóa tất cả dữ liệu rỗng insert vào database
        CMSUtils.GetAndRemoveNullValue(ref column, ref data);

        //Nối các column
        string columns = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                columns += column[i];
            }
            else
            {
                columns += column[i] + ",";
            }
        }

        #region [Insert column CreateDay, CreateAccount, EditDay, EditAccount]
        columns += ",CreateDay,CreateAccount,EditDay,EditAccount";
        #endregion

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");

        //Khai báo parameter
        string param = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                param += "@" + column[i];
            }
            else
            {
                param += "@" + column[i] + ",";
            }
        }

        #region [Insert params CreateDay, CreateAccount, EditDay, EditAccount]
        param += ",@" + "CreateDay" + ",";
        param += "@" + "CreateAccount" + ",";
        param += "@" + "EditDay" + ",";
        param += "@" + "EditAccount";
        #endregion

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##DATA##", param);
        else
            sampleQuery = sampleQuery.Replace("##DATA##", "");


        SqlParameter[] par = new SqlParameter[data.Length + 4];
        for (int i = 0; i < data.Length; i++)
        {
            SqlParameter singlePar = new SqlParameter("@" + column[i], data[i]);
            par[i] = singlePar;
        }

        #region [Insert values CreateDay, CreateAccount, EditDay, EditAccount]
        par[data.Length] = new SqlParameter("@" + "CreateDay", DateTime.Now);
        par[data.Length + 1] = new SqlParameter("@" + "CreateAccount", CMSContext.CurrentAccount.AccountID);
        par[data.Length + 2] = new SqlParameter("@" + "EditDay", DateTime.Now);
        par[data.Length + 3] = new SqlParameter("@" + "EditAccount", CMSContext.CurrentAccount.AccountID);
        #endregion

        int result = MBMSqlHelperNewConnectionStrings.ExecuteNonQuery(GetConnetion(NewConnectionStrings), sampleQuery, par, false);

        if (result > 0)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (column[i].ToLower().Contains("Password"))
                {
                    content += String.Format("Cột {0}: Đã tạo mật khẩu mới <br/>", column[i]);
                    continue;
                }
                content += String.Format("Cột {0}: {1} <br/>", column[i], data[i]);
            }
            new EventLogProviderToNewConnectionStrings().LogInformation(GetConnetion(NewConnectionStrings), "Insert", CMSContext.CurrentPage.PageName, content);
        }

        return result;
    }
    #endregion
    //============================ Update =========================================================================//
    #region [public static int UpdateDataSQL(string MainColumn, string columnID, string tableName, string[] column, object[] data)]
    public static int UpdateDataSQL(string MainColumn, string columnID, string tableName, string[] column, object[] data)
    {
        string content = "";
        string sampleQuery = "UPDATE ##TABLE## SET ##COLUMNS## WHERE ##MAINCOLUMN##";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Xóa cột có dữ liệu không thay đổi
        string cols = String.Join(",", column);
        DataTable dt = CMSUtils.GetDataSQL("", tableName, cols, String.Format("{0}='{1}'", MainColumn, columnID), "");
        if (dt.Rows.Count <= 0) return 1;
        CMSUtils.GetAndRemoveTheSameValue(ref column, ref data, ref dt);
        if (column.Length <= 0) return 1;

        //Nối các column
        string columns = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                columns += column[i] + "= @" + column[i];
            }
            else
            {
                columns += column[i] + "= @" + column[i] + ",";
            }
        }

        #region [Insert params EditDay, EditAccount]
        columns += ",EditDay= @EditDay,";
        columns += "EditAccount= @EditAccount";
        #endregion

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");


        if (!string.IsNullOrEmpty(MainColumn))
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", MainColumn + "='" + columnID + "'");
        else
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", "");

        SqlParameter[] par = new SqlParameter[data.Length + 2];
        for (int i = 0; i < data.Length; i++)
        {
            SqlParameter singlePar = new SqlParameter("@" + column[i], data[i]);
            par[i] = singlePar;
        }

        #region [Insert EditDay, EditAccount]
        par[data.Length] = new SqlParameter("@" + "EditDay", DateTime.Now);
        par[data.Length + 1] = new SqlParameter("@" + "EditAccount", CMSContext.CurrentAccount.AccountID);
        #endregion

        int result = MBMSqlHelper.ExecuteNonQuery(sampleQuery, par, false);

        if (result > 0)
        {
            for (int i = 0; i < column.Length; i++)
            {
                if (column[i].ToLower().Contains("Password"))
                {
                    content += String.Format("Cột {0}: Đã cập nhật mật khẩu mới <br/>", column[i]);
                    continue;
                }
                content += String.Format("Cột {0}: {1} => {2} <br/>", column[i], dt.Rows[0][i], data[i]);
            }
            new EventLogProvider().LogInformation("Update", CMSContext.CurrentPage.PageName, content);
        }

        return result;
    }
    #endregion
    #region [public static int UpdateDataSQLToNewConnectionStrings(string NewConnectionStrings,string MainColumn, string columnID, string tableName, string[] column, object[] data)]
    public static int UpdateDataSQLToNewConnectionStrings(string NewConnectionStrings, string MainColumn, string columnID, string tableName, string[] column, object[] data)
    {
        string content = "";
        string sampleQuery = "UPDATE ##TABLE## SET ##COLUMNS## WHERE ##MAINCOLUMN##";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Xóa cột có dữ liệu không thay đổi
        string cols = String.Join(",", column);
        DataTable dt = CMSUtils.GetDataSQLToNewConnectionStrings(GetConnetion(NewConnectionStrings), "", tableName, cols, String.Format("{0}='{1}'", MainColumn, columnID), "");
        if (dt.Rows.Count <= 0) return 1;
        CMSUtils.GetAndRemoveTheSameValue(ref column, ref data, ref dt);
        if (column.Length <= 0) return 1;

        //Nối các column
        string columns = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                columns += column[i] + "= @" + column[i];
            }
            else
            {
                columns += column[i] + "= @" + column[i] + ",";
            }
        }

        #region [Insert params EditDay, EditAccount]
        columns += ",EditDay= @EditDay,";
        columns += "EditAccount= @EditAccount";
        #endregion

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");


        if (!string.IsNullOrEmpty(MainColumn))
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", MainColumn + "='" + columnID + "'");
        else
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", "");

        SqlParameter[] par = new SqlParameter[data.Length + 2];
        for (int i = 0; i < data.Length; i++)
        {
            SqlParameter singlePar = new SqlParameter("@" + column[i], data[i]);
            par[i] = singlePar;
        }

        #region [Insert EditDay, EditAccount]
        par[data.Length] = new SqlParameter("@" + "EditDay", DateTime.Now);
        par[data.Length + 1] = new SqlParameter("@" + "EditAccount", CMSContext.CurrentAccount.AccountID);
        #endregion

        int result = MBMSqlHelperNewConnectionStrings.ExecuteNonQuery(GetConnetion(NewConnectionStrings), sampleQuery, par, false);

        if (result > 0)
        {
            for (int i = 0; i < column.Length; i++)
            {
                if (column[i].ToLower().Contains("Password"))
                {
                    content += String.Format("Cột {0}: Đã cập nhật mật khẩu mới <br/>", column[i]);
                    continue;
                }
                content += String.Format("Cột {0}: {1} => {2} <br/>", column[i], dt.Rows[0][i], data[i]);
            }
            new EventLogProviderToNewConnectionStrings().LogInformation(GetConnetion(NewConnectionStrings), "Update", CMSContext.CurrentPage.PageName, content);
        }

        return result;
    }
    #endregion
    //----
    #region [public static int UpdateDataSQLNoLog(string MainColumn, string columnID, string tableName, string[] column, object[] data)]
    public static int UpdateDataSQLNoLog(string MainColumn, string columnID, string tableName, string[] column, object[] data)
    {
        string sampleQuery = "UPDATE ##TABLE## SET ##COLUMNS## WHERE ##MAINCOLUMN##";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Nối các column
        string columns = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                columns += column[i] + "= @" + column[i];
            }
            else
            {
                columns += column[i] + "= @" + column[i] + ",";
            }
        }

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");


        if (!string.IsNullOrEmpty(MainColumn))
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", MainColumn + "='" + columnID + "'");
        else
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", "");

        SqlParameter[] par = new SqlParameter[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            SqlParameter singlePar = new SqlParameter("@" + column[i], data[i]);
            par[i] = singlePar;
        }
        return MBMSqlHelper.ExecuteNonQuery(sampleQuery, par, false);
    }
    #endregion
    #region [public static int UpdateDataSQLNoLogToNewConnectionStrings(string NewConnectionStrings,string MainColumn, string columnID, string tableName, string[] column, object[] data)]
    public static int UpdateDataSQLNoLogToNewConnectionStrings(string NewConnectionStrings, string MainColumn, string columnID, string tableName, string[] column, object[] data)
    {
        string sampleQuery = "UPDATE ##TABLE## SET ##COLUMNS## WHERE ##MAINCOLUMN##";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Nối các column
        string columns = "";
        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
            {
                columns += column[i] + "= @" + column[i];
            }
            else
            {
                columns += column[i] + "= @" + column[i] + ",";
            }
        }

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");


        if (!string.IsNullOrEmpty(MainColumn))
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", MainColumn + "='" + columnID + "'");
        else
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", "");

        SqlParameter[] par = new SqlParameter[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            SqlParameter singlePar = new SqlParameter("@" + column[i], data[i]);
            par[i] = singlePar;
        }
        return MBMSqlHelperNewConnectionStrings.ExecuteNonQuery(GetConnetion(NewConnectionStrings), sampleQuery, par, false);
    }
    #endregion
    //============================ Delete =========================================================================//
    #region [public static int DeleteDataSQL(string tableName)]
    public static int DeleteDataSQL(string tableName)
    {
        string sampleQuery = "DELETE FROM ##TABLE##";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        int result = MBMSqlHelper.ExecuteNonQuery(sampleQuery);

        new EventLogProvider().LogInformation("DELETE", CMSContext.CurrentPage.PageName, "Đã xóa toàn bộ dữ liệu!");

        return result;
    }
    #endregion
    #region [public static int DeleteDataSQLToNewConnectionStrings(string NewConnectionStrings,string tableName)]
    public static int DeleteDataSQLToNewConnectionStrings(string NewConnectionStrings, string tableName)
    {
        string sampleQuery = "DELETE FROM ##TABLE##";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        int result = MBMSqlHelperNewConnectionStrings.ExecuteNonQuery(GetConnetion(NewConnectionStrings), sampleQuery);

        new EventLogProviderToNewConnectionStrings().LogInformation(GetConnetion(NewConnectionStrings), "DELETE", CMSContext.CurrentPage.PageName, "Đã xóa toàn bộ dữ liệu!");

        return result;
    }
    #endregion
    //------
    #region [public static int DeleteDataSQLNoLog(string MainColumn, string columnID, string tableName)]
    public static int DeleteDataSQLNoLog(string MainColumn, string columnID, string tableName)
    {
        string sampleQuery = "DELETE FROM ##TABLE## WHERE ##MAINCOLUMN##";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Nối các column
        if (!string.IsNullOrEmpty(MainColumn))
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", MainColumn + "='" + columnID + "'");
        else
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", "");

        int result = MBMSqlHelper.ExecuteNonQuery(sampleQuery);

        return result;
    }
    #endregion
    #region [public static int DeleteDataSQLNoLogToNewConnectionStrings(string NewConnectionStrings,string MainColumn, string columnID, string tableName)]
    public static int DeleteDataSQLNoLogToNewConnectionStrings(string NewConnectionStrings, string MainColumn, string columnID, string tableName)
    {
        string sampleQuery = "DELETE FROM ##TABLE## WHERE ##MAINCOLUMN##";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        //Nối các column
        if (!string.IsNullOrEmpty(MainColumn))
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", MainColumn + "='" + columnID + "'");
        else
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", "");

        int result = MBMSqlHelperNewConnectionStrings.ExecuteNonQuery(GetConnetion(NewConnectionStrings), sampleQuery);

        return result;
    }
    #endregion
    //------
    #region [public static int DeleteDataSQL(string MainColumn, string columnID, string tableName)]
    public static int DeleteDataSQL(string MainColumn, string columnID, string tableName)
    {
        string content = "";
        string sampleQuery = "DELETE FROM ##TABLE## WHERE ##MAINCOLUMN##";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        if (!String.IsNullOrEmpty(MainColumn))
        {
            DataTable dt = CMSUtils.GetDataSQL("", tableName, "*", String.Format("{0}='{1}'", MainColumn, columnID), "");
            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    content += String.Format("Cột {0}: {1},", column.ColumnName, row[column.ColumnName]);
                }
                content += "<br/>";
            }
        }

        //Nối các column
        if (!string.IsNullOrEmpty(MainColumn))
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", MainColumn + "='" + columnID + "'");
        else
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", "");

        int result = MBMSqlHelper.ExecuteNonQuery(sampleQuery);

        if (result > 0)
        {
            new EventLogProvider().LogInformation("Delete", CMSContext.CurrentPage.PageName, content);
        }

        return result;
    }
    #endregion
    #region [public static int DeleteDataSQLToNewConnectionStrings(string NewConnectionStrings,string MainColumn, string columnID, string tableName)]
    public static int DeleteDataSQLToNewConnectionStrings(string NewConnectionStrings, string MainColumn, string columnID, string tableName)
    {
        string content = "";
        string sampleQuery = "DELETE FROM ##TABLE## WHERE ##MAINCOLUMN##";

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        if (!String.IsNullOrEmpty(MainColumn))
        {
            DataTable dt = CMSUtils.GetDataSQLToNewConnectionStrings(GetConnetion(NewConnectionStrings), "", tableName, "*", String.Format("{0}='{1}'", MainColumn, columnID), "");
            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    content += String.Format("Cột {0}: {1},", column.ColumnName, row[column.ColumnName]);
                }
                content += "<br/>";
            }
        }

        //Nối các column
        if (!string.IsNullOrEmpty(MainColumn))
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", MainColumn + "='" + columnID + "'");
        else
            sampleQuery = sampleQuery.Replace("##MAINCOLUMN##", "");

        int result = MBMSqlHelperNewConnectionStrings.ExecuteNonQuery(GetConnetion(NewConnectionStrings), sampleQuery);

        if (result > 0)
        {
            new EventLogProviderToNewConnectionStrings().LogInformation(GetConnetion(NewConnectionStrings), "Delete", CMSContext.CurrentPage.PageName, content);
        }

        return result;
    }
    #endregion
    //============================ GetConnetion ========================================================================//
    #region [private static string GetConnetion(string code)]
    private static string GetConnetion(string code)
    {
        string str = "";
        DataTable dt = CMSUtils.GetDataSQL("", "SYSTEM_Connetion", "*", "Code = N'" + code + "'", "");
        if (dt.Rows.Count > 0)
        {
            str = dt.Rows[0]["Content"].ToString();
        }
        return str;
    }
    #endregion
    //============================ GetData ========================================================================//
    #region [public static DataTable GetDataSQL(string top, string tableName, string columns, string where, string orderBy)]
    public static DataTable GetDataSQL(string top, string tableName, string columns, string where, string orderBy)
    {

        string sampleQuery = "SELECT ##TOPN## ##COLUMNS## FROM ##TABLE## ##WHERE## ##ORDERBY## ";

        if (!string.IsNullOrEmpty(top.ToString()))
            sampleQuery = sampleQuery.Replace("##TOPN##", "TOP(" + top + ")");
        else
            sampleQuery = sampleQuery.Replace("##TOPN##", "");

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        if (!string.IsNullOrEmpty(where))
            sampleQuery = sampleQuery.Replace("##WHERE##", " WHERE " + where + "");
        else
            sampleQuery = sampleQuery.Replace("##WHERE##", "");

        if (!string.IsNullOrEmpty(orderBy))
            sampleQuery = sampleQuery.Replace("##ORDERBY##", "ORDER BY " + orderBy);
        else
            sampleQuery = sampleQuery.Replace("##ORDERBY##", "");

        return MBMSqlHelper.ExecuteDataTable(sampleQuery);
    }
    #endregion
    #region [public static DataTable GetDataSQLToNewConnectionStrings(string NewConnectionStrings,string top, string tableName, string columns, string where, string orderBy)]
    public static DataTable GetDataSQLToNewConnectionStrings(string NewConnectionStrings, string top, string tableName, string columns, string where, string orderBy)
    {
        string sampleQuery = "SELECT ##TOPN## ##COLUMNS## FROM ##TABLE## ##WHERE## ##ORDERBY## ";

        if (!string.IsNullOrEmpty(top.ToString()))
            sampleQuery = sampleQuery.Replace("##TOPN##", "TOP(" + top + ")");
        else
            sampleQuery = sampleQuery.Replace("##TOPN##", "");

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        if (!string.IsNullOrEmpty(where))
            sampleQuery = sampleQuery.Replace("##WHERE##", " WHERE " + where + "");
        else
            sampleQuery = sampleQuery.Replace("##WHERE##", "");

        if (!string.IsNullOrEmpty(orderBy))
            sampleQuery = sampleQuery.Replace("##ORDERBY##", "ORDER BY " + orderBy);
        else
            sampleQuery = sampleQuery.Replace("##ORDERBY##", "");

        return MBMSqlHelperNewConnectionStrings.ExecuteDataTable(GetConnetion(NewConnectionStrings), sampleQuery);
    }
    #endregion
    //-----

    #region [public static DataTable GetDocumentsByStoreToNewConnectionStrings]
    public static DataTable GetDocumentsByStoreToNewConnectionStrings(string NewConnectionStrings, string[] parameterName, object[] parameterValue, object storeName)
    {
        var pars = new SqlParameter[parameterName.Length];
        for (int i = 0; i < pars.Length; i++)
        {
            pars[i] = new SqlParameter(parameterName[i], parameterValue[i]);
        }
        var tb = MBMSqlHelperNewConnectionStrings.ExecuteDataTable(GetConnetion(NewConnectionStrings), storeName.ToString(), pars);
        return tb;
    }
    #endregion
    //-----
    #region [public static object GetDataSQL(string tableName, string columns, string where, string orderBy)]
    public static object GetDataSQL(string tableName, string columns, string where, string orderBy)
    {
        string sampleQuery = "SELECT TOP(1) ##COLUMNS## FROM ##TABLE## ##WHERE## ##ORDERBY## ";

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        if (!string.IsNullOrEmpty(where))
            sampleQuery = sampleQuery.Replace("##WHERE##", " WHERE " + where + "");
        else
            sampleQuery = sampleQuery.Replace("##WHERE##", "");

        if (!string.IsNullOrEmpty(orderBy))
            sampleQuery = sampleQuery.Replace("##ORDERBY##", " ORDER BY " + orderBy);
        else
            sampleQuery = sampleQuery.Replace("##ORDERBY##", "");

        return MBMSqlHelper.ExecuteScalar(sampleQuery);
    }
    #endregion
    #region [public static object GetDataSQLToNewConnectionStrings(string NewConnectionStrings,string tableName, string columns, string where, string orderBy)]
    public static object GetDataSQLToNewConnectionStrings(string NewConnectionStrings, string tableName, string columns, string where, string orderBy)
    {
        string sampleQuery = "SELECT TOP(1) ##COLUMNS## FROM ##TABLE## ##WHERE## ##ORDERBY## ";

        if (!string.IsNullOrEmpty(columns))
            sampleQuery = sampleQuery.Replace("##COLUMNS##", columns);
        else
            sampleQuery = sampleQuery.Replace("##COLUMNS##", "");

        sampleQuery = sampleQuery.Replace("##TABLE##", tableName);

        if (!string.IsNullOrEmpty(where))
            sampleQuery = sampleQuery.Replace("##WHERE##", " WHERE " + where + "");
        else
            sampleQuery = sampleQuery.Replace("##WHERE##", "");

        if (!string.IsNullOrEmpty(orderBy))
            sampleQuery = sampleQuery.Replace("##ORDERBY##", " ORDER BY " + orderBy);
        else
            sampleQuery = sampleQuery.Replace("##ORDERBY##", "");

        return MBMSqlHelperNewConnectionStrings.ExecuteScalar(GetConnetion(NewConnectionStrings), sampleQuery);
    }
    #endregion
    //-----

    #region [public static DataTable GetQueryToNewConnectionStrings]
    public static DataTable GetQueryToNewConnectionStrings(string NewConnectionStrings, string Query)
    {
        string sampleQuery = " ##Query## ";

        if (!string.IsNullOrEmpty(Query))
            sampleQuery = sampleQuery.Replace("##Query##", Query);
        else
            sampleQuery = sampleQuery.Replace("##Query##", "");

        //return MBMSqlHelperNewConnectionStrings.ExecuteDataTable(GetConnetion(NewConnectionStrings), sampleQuery);
        return MBMSqlHelperNewConnectionStrings.ExecuteDataTable(NewConnectionStrings, sampleQuery);
    }
    #endregion
    //-----
    #region [public static string GetSafeString(string value)]
    public static string GetSafeString(string value)
    {
        if (!string.IsNullOrEmpty(value))
            return value.Replace("'", "")
                .Replace(";", "")
                .Replace("/", "")
                .Replace("\"", "")
                .Replace("%", "")
                .Replace("#", "")
                .Replace("?", "")
                .Replace("%", "")
                .Replace("$", "");
        else
            return "";
    }
    #endregion
    #region [public static string GetPlainText(string s)]
    public static string GetPlainText(string s)
    {
        s = s.Replace("&nbsp;", " ");
        s = s.Replace("&lt;", "<");
        s = s.Replace("&gt;", ">");
        s = s.Replace("&amp;", "&");
        s = s.Replace("&quot;", "\"");
        s = Regex.Replace(s, @"\x20{2,}", " ");
        s = Regex.Replace(s, "<[^>]*>", "");
        return s.Trim();
    }
    #endregion




    #region [public static void GetValuesList(ref string value, ListItemCollection item)]
    public static void GetValuesList(ref string value, ListItemCollection item)
    {
        if (item.Count != 0)
        {
            for (int i = 0; i < item.Count; i++)
            {
                if (item[i].Selected && value != string.Empty)
                {
                    value += "|" + item[i].Value;
                }
                else if (item[i].Selected && value == string.Empty)
                {
                    value += item[i].Value;
                }
            }
        }
    }
    #endregion
    #region [public static void GetAndRemoveNullValue(ref string[] cols, ref object[] values)]
    public static void GetAndRemoveNullValue(ref string[] cols, ref object[] values)
    {
        string index = "";

        //Kiểm tra nếu null thì lưu index lại
        for (int i = 0; i < cols.Length; i++)
        {
            if (CMSUtils.IsNull(values[i]))
            {
                index += i + ",";
            }
        }

        if (!String.IsNullOrEmpty(index))
        {
            index = index.Trim(',');
            string[] arrIndex = index.Split(new char[] { ',' });
            //Xóa những cột không cần thiết ra khỏi array
            for (int i = arrIndex.Length - 1; i >= 0; i--)
            {
                cols = cols.Where((val, idx) => idx != int.Parse(arrIndex[i])).ToArray();
                values = values.Where((val, idx) => idx != int.Parse(arrIndex[i])).ToArray();
            }
        }
    }
    #endregion
    #region [public static void GetAndRemoveTheSameValue(ref string[] cols, ref object[] values, ref object[] oldValues)]
    public static void GetAndRemoveTheSameValue(ref string[] cols, ref object[] values, ref object[] oldValues)
    {
        string index = "";

        //Kiểm tra nếu giống dữ liệu cũ thì xóa đi
        for (int i = 0; i < cols.Length; i++)
        {
            if (oldValues[i] == values[i])
            {
                index += i + ",";
            }
        }

        if (!String.IsNullOrEmpty(index))
        {
            index = index.Trim(',');
            string[] arrIndex = index.Split(new char[] { ',' });
            //Xóa những cột không cần thiết ra khỏi array
            for (int i = arrIndex.Length - 1; i >= 0; i--)
            {
                oldValues = oldValues.Where((val, idx) => idx != int.Parse(arrIndex[i])).ToArray();
                cols = cols.Where((val, idx) => idx != int.Parse(arrIndex[i])).ToArray();
                values = values.Where((val, idx) => idx != int.Parse(arrIndex[i])).ToArray();
            }
        }
    }
    #endregion
    #region [public static void GetAndRemoveTheSameValue(ref string[] cols, ref object[] values, ref DataTable oldDt)]
    public static void GetAndRemoveTheSameValue(ref string[] cols, ref object[] values, ref DataTable oldDt)
    {
        string index = "";

        //Kiểm tra nếu giống dữ liệu cũ thì xóa đi
        for (int i = 0; i < cols.Length; i++)
        {
            if (oldDt.Rows[0][i].Equals(values[i]))
            {
                index += i + ",";
            }
        }

        if (!String.IsNullOrEmpty(index))
        {
            index = index.Trim(',');
            string[] arrIndex = index.Split(new char[] { ',' });
            //Xóa những cột không cần thiết ra khỏi array
            for (int i = arrIndex.Length - 1; i >= 0; i--)
            {
                oldDt.Columns.Remove(cols[int.Parse(arrIndex[i])]);
                cols = cols.Where((val, idx) => idx != int.Parse(arrIndex[i])).ToArray();
                values = values.Where((val, idx) => idx != int.Parse(arrIndex[i])).ToArray();
            }
        }
    }
    #endregion
    //============================ Cut String =====================================================================//
    #region [public static string[] SplitString(string value, char split)]
    public static string[] SplitString(string value, char split)
    {
        if (value != string.Empty)
        {
            string[] getValue = new string[3];
            if (value.Contains(split.ToString()))
            {
                getValue = value.Split(new Char[] { split });
            }
            else
            {
                getValue = new string[1];
                getValue.SetValue(value, 0);
            }
            return getValue;
        }
        return null;
    }
    #endregion
    //============================ Validation Number ===============================================================//
    #region [public static bool IsNumber(object value, Type type)]
    public static bool IsNumber(object value, Type type)
    {
        bool result = false;

        if (!CMSUtils.IsNull(value))
        {
            switch (type.Name)
            {
                case "SByte":
                    SByte sSByte = 0;
                    if (SByte.TryParse(value.ToString(), out sSByte)) result = true;
                    break;
                case "Byte":
                    Byte bByte = 0;
                    if (Byte.TryParse(value.ToString(), out bByte)) result = true;
                    break;
                case "Int16":
                    Int16 iInt16 = 0;
                    if (Int16.TryParse(value.ToString(), out iInt16)) result = true;
                    break;
                case "Int32":
                    int iInt32 = 0;
                    if (int.TryParse(value.ToString(), out iInt32)) result = true;
                    break;
                case "Int64":
                    Int64 iInt64 = 0;
                    if (Int64.TryParse(value.ToString(), out iInt64)) result = true;
                    break;
                case "UInt16":
                    UInt16 uUint16 = 0;
                    if (UInt16.TryParse(value.ToString(), out uUint16)) result = true;
                    break;
                case "UInt32":
                    uint uInt = 0;
                    if (uint.TryParse(value.ToString(), out uInt)) result = true;
                    break;
                case "UInt64":
                    UInt64 uUInt64 = 0;
                    if (UInt64.TryParse(value.ToString(), out uUInt64)) result = true;
                    break;
                case "Double":
                    Double dDouble = 0;
                    if (Double.TryParse(value.ToString(), out dDouble)) result = true;
                    break;
                case "Single":
                    float fFloat = 0;
                    if (float.TryParse(value.ToString(), out fFloat)) result = true;
                    break;
                case "Decimal":
                    decimal dDecimal = 0;
                    if (decimal.TryParse(value.ToString(), out dDecimal)) result = true;
                    break;
            }
        }

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out int number)]
    public static bool IsNumber(object value, out int number)
    {
        bool result = false;
        number = 0;
        object oNumber = 0;
        result = IsNumber(value, typeof(int), out oNumber);
        number = int.Parse(oNumber.ToString());

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out Int16 number)]
    public static bool IsNumber(object value, out Int16 number)
    {
        bool result = false;
        number = 0;
        object oNumber = 0;
        result = IsNumber(value, typeof(Int16), out oNumber);
        number = (Int16)oNumber;

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out sbyte number)]
    public static bool IsNumber(object value, out sbyte number)
    {
        bool result = false;
        number = 0;
        object oNumber = 0;
        result = IsNumber(value, typeof(sbyte), out oNumber);
        number = (sbyte)oNumber;

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out byte number)]
    public static bool IsNumber(object value, out byte number)
    {
        bool result = false;
        number = 0;
        object oNumber = 0;
        result = IsNumber(value, typeof(byte), out oNumber);
        number = (byte)oNumber;

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out long number)]
    public static bool IsNumber(object value, out long number)
    {
        bool result = false;
        number = 0;
        object oNumber = 0;
        result = IsNumber(value, typeof(long), out oNumber);
        number = (long)oNumber;

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out ulong number)]
    public static bool IsNumber(object value, out ulong number)
    {
        bool result = false;
        number = 0;
        object oNumber = 0;
        result = IsNumber(value, typeof(ulong), out oNumber);
        number = (ulong)oNumber;

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out uint number)]
    public static bool IsNumber(object value, out uint number)
    {
        bool result = false;
        number = 0;
        object oNumber = 0;
        result = IsNumber(value, typeof(uint), out oNumber);
        number = (uint)oNumber;

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out UInt16 number)]
    public static bool IsNumber(object value, out UInt16 number)
    {
        bool result = false;
        number = 0;
        object oNumber = 0;
        result = IsNumber(value, typeof(UInt16), out oNumber);
        number = (UInt16)oNumber;

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out float number)]
    public static bool IsNumber(object value, out float number)
    {
        bool result = false;
        number = 0;
        object oNumber = 0;
        result = IsNumber(value, typeof(float), out oNumber);
        number = (float)oNumber;

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out double number)]
    public static bool IsNumber(object value, out double number)
    {
        bool result = false;
        number = 0;
        object oNumber = 0;
        result = IsNumber(value, typeof(double), out oNumber);
        number = (double)oNumber;

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out decimal number)]
    public static bool IsNumber(object value, out decimal number)
    {
        bool result = false;
        number = 0;
        object oNumber = 0;
        result = IsNumber(value, typeof(decimal), out oNumber);
        number = (decimal)oNumber;

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out sbyte? returnValue)]
    public static bool IsNumber(object value, out sbyte? returnValue)
    {
        bool result = false;
        returnValue = null;

        if (!CMSUtils.IsNull(value))
        {
            sbyte temp = 0;
            if (sbyte.TryParse(value.ToString(), out temp)) result = true;
            returnValue = temp;
        }

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out int? returnValue)]
    public static bool IsNumber(object value, out int? returnValue)
    {
        bool result = false;
        returnValue = null;

        if (!CMSUtils.IsNull(value))
        {
            int temp = 0;
            if (int.TryParse(value.ToString(), out temp)) result = true;
            returnValue = temp;
        }

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out Int16? returnValue)]
    public static bool IsNumber(object value, out Int16? returnValue)
    {
        bool result = false;
        returnValue = null;

        if (!CMSUtils.IsNull(value))
        {
            Int16 temp = 0;
            if (Int16.TryParse(value.ToString(), out temp)) result = true;
            returnValue = temp;
        }

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out long? returnValue)]
    public static bool IsNumber(object value, out long? returnValue)
    {
        bool result = false;
        returnValue = null;

        if (!CMSUtils.IsNull(value))
        {
            long temp = 0;
            if (long.TryParse(value.ToString(), out temp)) result = true;
            returnValue = temp;
        }

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out uint? returnValue)]
    public static bool IsNumber(object value, out uint? returnValue)
    {
        bool result = false;
        returnValue = null;

        if (!CMSUtils.IsNull(value))
        {
            uint temp = 0;
            if (uint.TryParse(value.ToString(), out temp)) result = true;
            returnValue = temp;
        }

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out UInt16? returnValue)]
    public static bool IsNumber(object value, out UInt16? returnValue)
    {
        bool result = false;
        returnValue = null;

        if (!CMSUtils.IsNull(value))
        {
            UInt16 temp = 0;
            if (UInt16.TryParse(value.ToString(), out temp)) result = true;
            returnValue = temp;
        }

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out ulong? returnValue)]
    public static bool IsNumber(object value, out ulong? returnValue)
    {
        bool result = false;
        returnValue = null;

        if (!CMSUtils.IsNull(value))
        {
            ulong temp = 0;
            if (ulong.TryParse(value.ToString(), out temp)) result = true;
            returnValue = temp;
        }

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out decimal? returnValue)]
    public static bool IsNumber(object value, out decimal? returnValue)
    {
        bool result = false;
        returnValue = null;

        if (!CMSUtils.IsNull(value))
        {
            decimal temp = 0;
            if (decimal.TryParse(value.ToString(), out temp)) result = true;
            returnValue = temp;
        }

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out float? returnValue)]
    public static bool IsNumber(object value, out float? returnValue)
    {
        bool result = false;
        returnValue = null;

        if (!CMSUtils.IsNull(value))
        {
            float temp = 0;
            if (float.TryParse(value.ToString(), out temp)) result = true;
            returnValue = temp;
        }

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, out double? returnValue)]
    public static bool IsNumber(object value, out double? returnValue)
    {
        bool result = false;
        returnValue = null;

        if (!CMSUtils.IsNull(value))
        {
            double temp = 0;
            if (double.TryParse(value.ToString(), out temp)) result = true;
            returnValue = temp;
        }

        return result;
    }
    #endregion
    #region [public static bool IsNumber(object value, Type type, out object number)]
    public static bool IsNumber(object value, Type type, out object number)
    {
        bool result = false;
        number = 0;

        if (!CMSUtils.IsNull(value))
        {
            switch (type.Name)
            {
                case "SByte":
                    SByte sSByte = 0;
                    if (SByte.TryParse(value.ToString(), out sSByte)) result = true;
                    number = sSByte;
                    break;
                case "Byte":
                    Byte bByte = 0;
                    if (Byte.TryParse(value.ToString(), out bByte)) result = true;
                    number = bByte;
                    break;
                case "Int16":
                    Int16 iInt16 = 0;
                    if (Int16.TryParse(value.ToString(), out iInt16)) result = true;
                    number = iInt16;
                    break;
                case "Int32":
                    int iInt32 = 0;
                    if (int.TryParse(value.ToString(), out iInt32)) result = true;
                    number = iInt32;
                    break;
                case "Int64":
                    Int64 iInt64 = 0;
                    if (Int64.TryParse(value.ToString(), out iInt64)) result = true;
                    number = iInt64;
                    break;
                case "UInt16":
                    UInt16 uUint16 = 0;
                    if (UInt16.TryParse(value.ToString(), out uUint16)) result = true;
                    number = uUint16;
                    break;
                case "UInt32":
                    uint uInt = 0;
                    if (uint.TryParse(value.ToString(), out uInt)) result = true;
                    number = uInt;
                    break;
                case "UInt64":
                    UInt64 uUInt64 = 0;
                    if (UInt64.TryParse(value.ToString(), out uUInt64)) result = true;
                    number = uUInt64;
                    break;
                case "Double":
                    Double dDouble = 0;
                    if (Double.TryParse(value.ToString(), out dDouble)) result = true;
                    number = dDouble;
                    break;
                case "Single":
                    float fFloat = 0;
                    if (float.TryParse(value.ToString(), out fFloat)) result = true;
                    number = fFloat;
                    break;
                case "Decimal":
                    decimal dDecimal = 0;
                    if (decimal.TryParse(value.ToString(), out dDecimal)) result = true;
                    number = dDecimal;
                    break;
            }
        }

        return result;
    }
    #endregion
    //============================ Validation Boolean =============================================================//
    #region [public static bool IsBoolean(object value)]
    public static bool IsBoolean(object value)
    {
        bool result = false;

        if (!CMSUtils.IsNull(value))
        {
            bool bBoolean = false;
            if (bool.TryParse(value.ToString(), out bBoolean)) result = true;
        }

        return result;
    }
    #endregion
    #region [public static bool IsBoolean(object value, out bool boolean)]
    public static bool IsBoolean(object value, out bool boolean)
    {
        bool result = false;
        boolean = false;

        if (!CMSUtils.IsNull(value))
        {
            bool bBoolean = false;
            if (bool.TryParse(value.ToString(), out bBoolean)) result = true;
            boolean = bBoolean;
        }

        return result;
    }
    #endregion
    #region [public static bool IsBoolean(object value, out bool? boolean)]
    public static bool IsBoolean(object value, out bool? boolean)
    {
        bool result = false;
        boolean = null;

        if (!CMSUtils.IsNull(value))
        {
            bool bBoolean = false;
            if (bool.TryParse(value.ToString(), out bBoolean)) result = true;
            boolean = bBoolean;
        }

        return result;
    }
    #endregion
    //============================ Validation DateTime ============================================================//
    #region [public static bool IsDateTime(object value)]
    public static bool IsDateTime(object value)
    {
        bool result = false;

        if (!CMSUtils.IsNull(value))
        {
            DateTime dDateTime = DateTime.Now;
            if (DateTime.TryParse(value.ToString(), out dDateTime)) result = true;
        }

        return result;
    }
    #endregion
    #region [public static bool IsDateTime(object value, out DateTime dateTime)]
    public static bool IsDateTime(object value, out DateTime dateTime)
    {
        bool result = false;
        dateTime = DateTime.Now;

        if (!CMSUtils.IsNull(value))
        {
            DateTime dDateTime = DateTime.Now;
            if (DateTime.TryParse(value.ToString(), out dDateTime)) result = true;
            dateTime = dDateTime;
        }

        return result;
    }
    #endregion
    #region [public static bool IsDateTime(object value, out DateTime? dateTime)]
    public static bool IsDateTime(object value, out DateTime? dateTime)
    {
        bool result = false;
        dateTime = null;

        if (!CMSUtils.IsNull(value))
        {
            DateTime dDateTime = DateTime.Now;
            if (DateTime.TryParse(value.ToString(), out dDateTime)) result = true;
            dateTime = dDateTime;
        }

        return result;
    }
    #endregion
    //============================ Convert Text To Date  ==========================================================//
    #region[private static string ConvertToText(object number)]
    private static string ConvertToText(object number)
    {
        string s = number.ToString();
        string[] so = new string[] { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
        string[] hang = new string[] { "", "ngàn", "triệu", "tỷ" };
        int i, j, donvi, chuc, tram;
        string str = " ";
        bool booAm = false;
        decimal decS = 0;
        //Tung addnew
        try
        {
            decS = Convert.ToDecimal(s.ToString());
        }
        catch (Exception ex)
        {
            ex.ToString();
        }
        if (decS < 0)
        {
            decS = -decS;
            s = decS.ToString();
            booAm = true;
        }
        i = s.Length;
        if (i == 0)
            str = so[0] + str;
        else
        {
            j = 0;
            while (i > 0)
            {
                donvi = Convert.ToInt32(s.Substring(i - 1, 1));
                i--;
                if (i > 0)
                    chuc = Convert.ToInt32(s.Substring(i - 1, 1));
                else
                    chuc = -1;
                i--;
                if (i > 0)
                    tram = Convert.ToInt32(s.Substring(i - 1, 1));
                else
                    tram = -1;
                i--;
                if ((donvi > 0) || (chuc > 0) || (tram > 0) || (j == 3))
                    str = hang[j] + str;
                j++;
                if (j > 3) j = 1;
                if ((donvi == 1) && (chuc > 1))
                {
                    if (chuc == 10)
                        str = "một " + str;
                    else
                        str = "mốt " + str;
                }
                else
                {
                    if ((donvi == 5) && (chuc > 0))
                        str = "lăm " + str;
                    else if (donvi > 0)
                        str = so[donvi] + " " + str;
                }
                if (chuc < 0)
                    break;
                else
                {
                    if ((chuc == 0) && (donvi > 0)) str = "linh " + str;
                    if (chuc == 1) str = "mười " + str;
                    if (chuc > 1) str = so[chuc] + " mươi " + str;
                }
                if (tram < 0) break;
                else
                {
                    if ((tram > 0) || (chuc > 0) || (donvi > 0)) str = so[tram] + " trăm " + str;
                }
                str = " " + str;
            }
        }
        if (booAm) str = "Âm " + str;
        return str;
    }
    #endregion
    #region[public static string ConvertDateToText(int year, int month, int day)]
    public static string ConvertDateToText(int year, int month, int day)
    {
        string str = "";
        string sYear = ConvertToText(year.ToString("#"));
        string sMonth = ConvertToText(month.ToString("#"));
        string sDay = ConvertToText(day.ToString("#"));

        str = String.Format("Ngày {0} tháng {1} năm {2}", sDay, sMonth, sYear);

        return str;
    }
    #endregion
    #region[public static string ConvertNumberDecimalToText(decimal number)]
    public static string ConvertNumberDecimalToText(decimal number)
    {
        string str = ConvertToText(number.ToString("#"));
        return str;
    }
    #endregion
    #region[public static string ConvertNumberIntToText(int number)]
    public static string ConvertNumberIntToText(int number)
    {
        string str = ConvertToText(number.ToString("#"));
        return str;
    }
    #endregion
    #region[public static string ConvertNumberDoubleToText(double number)]
    public static string ConvertNumberDoubleToText(double number)
    {
        string str = ConvertToText(number.ToString("#"));
        return str;
    }
    #endregion

    //============================ Create Code  ==========================================================//
    #region[private static string CreateCode(object number)]
    private static string CreateCode(object number)
    {
        string s = number.ToString();
        string[] so = new string[] { "Gm", "2L", "5Y", "z6", "rL", "4h", "A9", "kq", "s5", "hP" };
        string[] hang = new string[] { "45", "r4", "4v", "3b" };
        int i, j, donvi, chuc, tram;
        string str = "";
        bool booAm = false;
        decimal decS = 0;
        //Tung addnew
        try
        {
            decS = Convert.ToDecimal(s.ToString());
        }
        catch (Exception ex)
        {
            ex.ToString();
        }
        if (decS < 0)
        {
            decS = -decS;
            s = decS.ToString();
            booAm = true;
        }
        i = s.Length;
        if (i == 0)
            str = so[0] + str;
        else
        {
            j = 0;
            while (i > 0)
            {
                donvi = Convert.ToInt32(s.Substring(i - 1, 1));
                i--;
                if (i > 0)
                    chuc = Convert.ToInt32(s.Substring(i - 1, 1));
                else
                    chuc = -1;
                i--;
                if (i > 0)
                    tram = Convert.ToInt32(s.Substring(i - 1, 1));
                else
                    tram = -1;
                i--;
                if ((donvi > 0) || (chuc > 0) || (tram > 0) || (j == 3))
                    str = hang[j] + str;
                j++;
                if (j > 3) j = 1;
                if ((donvi == 1) && (chuc > 1))
                {
                    if (chuc == 10)
                        str = "5t" + str;
                    else
                        str = "k2" + str;
                }
                else
                {
                    if ((donvi == 5) && (chuc > 0))
                        str = "1f" + str;
                    else if (donvi > 0)
                        str = so[donvi] + "" + str;
                }
                if (chuc < 0)
                    break;
                else
                {
                    if ((chuc == 0) && (donvi > 0)) str = "kF" + str;
                    if (chuc == 1) str = "5K" + str;
                    if (chuc > 1) str = so[chuc] + "b3" + str;
                }
                if (tram < 0) break;
                else
                {
                    if ((tram > 0) || (chuc > 0) || (donvi > 0)) str = so[tram] + "F1" + str;
                }
                str = "" + str;
            }
        }
        if (booAm) str = "V0" + str;
        return str;
    }
    #endregion
    #region[public static string CreateStringCode]
    public static string CreateStringCode(int day, int hour, int month, int Minute, int year, int sec)
    {
        string str = "";
        string sYear = CreateCode(year.ToString("#"));
        string sMonth = CreateCode(month.ToString("#"));
        string sDay = CreateCode(day.ToString("#"));
        string sHour = CreateCode(hour.ToString("#"));
        string sMu = CreateCode(Minute.ToString("#"));
        string sSec = CreateCode(sec.ToString("#"));

        str = String.Format("{0}{1}{2}{3}{4}{5}", sDay, sHour, sMonth, sMu, sYear, sSec);

        return str;
    }
    #endregion
    //============================ Check Site  ==========================================================//
    #region [public static void CheckSite(string url)]
    //    Response.Write("<br/> " + HttpContext.Current.Request.Url.Host);
    //Response.Write("<br/> " + HttpContext.Current.Request.Url.Authority);
    //Response.Write("<br/> " + HttpContext.Current.Request.Url.AbsolutePath);
    //Response.Write("<br/> " + HttpContext.Current.Request.ApplicationPath);
    //Response.Write("<br/> " + HttpContext.Current.Request.Url.AbsoluteUri);
    //Response.Write("<br/> " + HttpContext.Current.Request.Url.PathAndQuery);
    public static void CheckSite(string url)
    {
        if (url == "/Coming_Soon.aspx")
        {

        }
        else
        {
            if (url == "/Module/Affiliate/Default.aspx?key=111")
            {

            }
            else
            {
                HttpContext.Current.Response.Redirect("/404.aspx");
            }
        }
    }
    #endregion



    public static object SetDBNull(object input, string ob)
    {
        object ob_ = ob;
        ob_ = DBNull.Value;
        if (!CMSUtils.IsNull(input))
        {
            ob_ = input;
        }
        return ob_;
    }


}
