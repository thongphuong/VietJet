using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CurrentUser
/// </summary>
public sealed class CurrentPage
{
    private int _pageID;
    private string _duongDan;
    private string _uRLRelatively;
    private string _uRLFull;
    private string _pageName;
    private string _pageCode;
    private DataTable dtPage;

    #region [Get, set Value]
    public int PageID
    {
        get
        {
            if (dtPage.Rows.Count > 0 && dtPage.Rows[0]["PageID"] != DBNull.Value && dtPage.Rows[0]["PageID"] != null)
            {
                int.TryParse(dtPage.Rows[0]["PageID"].ToString(), out _pageID);
            }
            return _pageID;
        }
    }
    public string URL
    {
        get
        {
            _duongDan = HttpContext.Current.Request.RawUrl.ToLower();
            if (_duongDan.Contains("?"))
            {
                _duongDan = _duongDan.Substring(0, _duongDan.IndexOf("?"));
            }
            return _duongDan;
        }
    }
    public string URLRelatively
    {
        get { return HttpContext.Current.Request.RawUrl; }
    }
    public string URLFull
    {
        get { return HttpContext.Current.Request.Url.ToString(); }
    }
    public string PageName
    {
        get
        {
            if (dtPage.Rows.Count > 0 && dtPage.Rows[0]["PageName"] != DBNull.Value && dtPage.Rows[0]["PageName"] != null)
            {
                _pageName = dtPage.Rows[0]["PageName"].ToString();
            }
            return _pageName;
        }
        set { _pageName = value; }
    }
    public string PageCode
    {
        get
        {
            if (dtPage.Rows.Count > 0 && dtPage.Rows[0]["PageCode"] != DBNull.Value && dtPage.Rows[0]["PageCode"] != null)
            {
                _pageCode = dtPage.Rows[0]["PageCode"].ToString();
            }
            return _pageCode;
        }
        set { _pageName = value; }
    }
    #endregion

    public CurrentPage()
    {
        string url = HttpContext.Current.Request.RawUrl.ToLower();
        if (url.Contains("?"))
        {
            url = url.Substring(0, url.IndexOf("?"));
        }
        dtPage = CMSUtils.GetDataSQL("", "SYSTEM_Page", "*", String.Format("URL LIKE '{0}%' AND POID={1}", url, CMSUtils.POID), "");

        if (dtPage.Rows.Count <= 0)
        {
            HttpContext.Current.Response.Redirect("/");
        }
        //
        // TODO: Add constructor logic here
        //
    }

    public object GetValue(string column)
    {
        object result = null;
        string url = HttpContext.Current.Request.RawUrl.ToLower();
        if (url.Contains("?"))
        {
            url = url.Substring(0, url.IndexOf("?"));
        }
        result = CMSUtils.GetDataSQL("SYSTEM_Page", column, String.Format("URL LIKE '{0}%' AND POID={1}", url, CMSUtils.POID), "");
        return result;
    }

    public bool isView()
    {
        return true;
    }

    public bool isAdd()
    {
        return true;
    }

    public bool isEdit()
    {
        return true;
    }

    public bool isDelete()
    {
        return true;
    }
}