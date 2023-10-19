using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

/// <summary>
/// Summary description for Context
/// </summary>
public class CMSContext
{
    private static string _ip;
    private static string _userAgent;
    private static CurrentAccount _currentAccount;
    private static CurrentPage _currentPage;
    private static CurrentGroupAccount _currentGroupAccount;
    private static CurrentDepartment _currentDepartment;

    #region [Get, set value]
    public static string IP
    {
        get
        {
            _ip = HttpContext.Current.Request.UserHostAddress;
            return _ip;
        }
    }
    public static string UserAgent
    {
        get
        {
            _userAgent = HttpContext.Current.Request.UserAgent;
            return _userAgent;
        }
    }
    public static CurrentGroupAccount CurrentGroupAccount
    {
        get
        {
            _currentGroupAccount = new CurrentGroupAccount();
            return _currentGroupAccount;
        }
    }
    public static CurrentAccount CurrentAccount
    {
        get
        {
            _currentAccount = new CurrentAccount();
            return _currentAccount;
        }
    }
    public static CurrentPage CurrentPage
    {
        get
        {
            _currentPage = new CurrentPage();
            return _currentPage;
        }
    }
    public static CurrentDepartment CurrentDepartment
    {
        get
        {
            _currentDepartment = new CurrentDepartment();
            return _currentDepartment;
        }
    }
    #endregion
}