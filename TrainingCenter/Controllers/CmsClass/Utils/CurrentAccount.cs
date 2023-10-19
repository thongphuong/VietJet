using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CurrentUser
/// </summary>
public sealed class CurrentAccount
{
    private string _AccountName;
    private DataTable _dt;
    private int _AccountID;
    private string _Email;
    private string _FullName;
    private string _Gender;
    private string _Phone;
    private string _Fax;
    private string _AddressNow;
    private string _Address;
    private string _IdentityCard;
    private DateTime _LastLoginDay;
    private int _GroupAccountID;
    private int _POID;

    #region [Get Set Value]
    public int POID
    {
        get { return _POID; }
        set { _POID = value; }
    }
    public int GroupAccountID
    {
        get { return _GroupAccountID; }
        set { _GroupAccountID = value; }
    }
    public DateTime LastLoginDay
    {
        get { return _LastLoginDay; }
        set { _LastLoginDay = value; }
    }
    public string IdentityCard
    {
        get { return _IdentityCard; }
        set { _IdentityCard = value; }
    }
    public string Address
    {
        get { return _Address; }
        set { _Address = value; }
    }
    public string AddressNow
    {
        get { return _AddressNow; }
        set { _AddressNow = value; }
    }
    public string Fax
    {
        get { return _Fax; }
        set { _Fax = value; }
    }
    public string Phone
    {
        get { return _Phone; }
        set { _Phone = value; }
    }
    public string Gender
    {
        get { return _Gender; }
        set { _Gender = value; }
    }
    public string FullName
    {
        get { return _FullName; }
        set { _FullName = value; }
    }
    public string Email
    {
        get { return _Email; }
        set { _Email = value; }
    }
    public int AccountID
    {
        get { return _AccountID; }
        set { _AccountID = value; }
    }
    public DataTable Table
    {
        get { return _dt; }
        set { _dt = value; }
    }
    public string AccountName
    {
        get { return _AccountName; }
        set { _AccountName = value; }
    }
    #endregion

    public CurrentAccount()
    {
        _dt = CMSUtils.GetDataSQL("", "SYSTEM_Account", "*", String.Format("AccountName='{0}'", CMSUtils.GetSafeString(HttpContext.Current.User.Identity.Name)), "");

        if (_dt.Rows.Count == 1)
        {
            CMSUtils.IsNumber(_dt.Rows[0]["AccountID"], out _AccountID);
            CMSUtils.IsNumber(_dt.Rows[0]["GroupAccountID"], out _GroupAccountID);
            CMSUtils.IsNumber(_dt.Rows[0]["POID"], out _POID);
            CMSUtils.IsDateTime(_dt.Rows[0]["LastLoginDay"], out _LastLoginDay);
            if (!CMSUtils.IsNull(_dt.Rows[0]["AccountName"])) _AccountName = _dt.Rows[0]["AccountName"].ToString();
            if (!CMSUtils.IsNull(_dt.Rows[0]["Email"])) _Email = _dt.Rows[0]["Email"].ToString();
            if (!CMSUtils.IsNull(_dt.Rows[0]["FullName"])) _FullName = _dt.Rows[0]["FullName"].ToString();
            if (!CMSUtils.IsNull(_dt.Rows[0]["Gender"])) _Gender = _dt.Rows[0]["Gender"].ToString();
            if (!CMSUtils.IsNull(_dt.Rows[0]["Phone"])) _Phone = _dt.Rows[0]["Phone"].ToString();
            if (!CMSUtils.IsNull(_dt.Rows[0]["Fax"])) _Fax = _dt.Rows[0]["Fax"].ToString();
            if (!CMSUtils.IsNull(_dt.Rows[0]["AddressNow"])) _AddressNow = _dt.Rows[0]["AddressNow"].ToString();
            if (!CMSUtils.IsNull(_dt.Rows[0]["Address"])) _Address = _dt.Rows[0]["Address"].ToString();
            if (!CMSUtils.IsNull(_dt.Rows[0]["IdentityCard"])) _IdentityCard = _dt.Rows[0]["IdentityCard"].ToString();
        }
    }

    public object GetValue(string column)
    {
        object result = null;
        result = CMSUtils.GetDataSQL("SYSTEM_Account", column, String.Format("AccountName='{0}'", CMSUtils.GetSafeString(HttpContext.Current.User.Identity.Name)), "");
        return result;
    }
}