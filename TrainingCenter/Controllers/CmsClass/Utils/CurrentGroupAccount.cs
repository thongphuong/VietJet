using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CurrentUser
/// </summary>
public sealed class CurrentGroupAccount
{
    private DataTable dtGroupAccount;
    private int _GroupAccountID;
    private string _GroupAccountName;
    private string _GroupAccountCode;

    #region [Get Set Value]
    public int GroupAccountID
    {
        get
        {
            _GroupAccountID = CMSContext.CurrentAccount.GroupAccountID;
            return _GroupAccountID;
        }
    }
    public string GroupAccountCode
    {
        get
        {
            if (dtGroupAccount.Rows[0]["GroupAccountCode"] != DBNull.Value && dtGroupAccount.Rows[0]["GroupAccountCode"] != null)
            {
                _GroupAccountName = dtGroupAccount.Rows[0]["GroupAccountCode"].ToString();
            }
            return _GroupAccountCode;
        }
    }
    public string GroupAccountName
    {
        get
        {
            if (dtGroupAccount.Rows[0]["GroupAccountName"] != DBNull.Value && dtGroupAccount.Rows[0]["GroupAccountName"] != null)
            {
                _GroupAccountName = dtGroupAccount.Rows[0]["GroupAccountName"].ToString();
            }
            return _GroupAccountName;
        }
    }
    #endregion

    public CurrentGroupAccount()
    {
        dtGroupAccount = CMSUtils.GetDataSQL("", "SYSTEM_GroupAccount", "*", String.Format("GroupAccountID={0}", CMSContext.CurrentAccount.GroupAccountID), "");
        //
        // TODO: Add constructor logic here
        //
    }

    public object GetValue(string column)
    {
        object result = null;
        dtGroupAccount = CMSUtils.GetDataSQL("", "SYSTEM_GroupAccount", column, String.Format("GroupAccountID={0}", CMSContext.CurrentAccount.GroupAccountID), "");
        return result;
    }
}