using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CurrentDepartment
/// </summary>
public class CurrentDepartment
{
    private int _POID = CMSUtils.POID;
    private string _POName;
    private string _POCode;
    private bool? _IsActivate;
    private DataTable dt;

    #region [get set value]
    public static int POID { get; set; }
    public string POName
    {
        get
        {
            if (dt.Rows.Count > 0 && dt.Rows[0]["POName"] != DBNull.Value && dt.Rows[0]["POName"] != null)
            {
                _POName = dt.Rows[0]["POName"].ToString();
            }
            return _POName;
        }
        set { _POName = value; }
    }
    public string POCode
    {
        get
        {
            if (dt.Rows.Count > 0 && dt.Rows[0]["POCode"] != DBNull.Value && dt.Rows[0]["POCode"] != null)
            {
                _POCode = dt.Rows[0]["POCode"].ToString();
            }
            return _POCode;
        }
        set { _POCode = value; }
    }
    public bool? IsActivate
    {
        get
        {
            if (dt.Rows.Count > 0 && !CMSUtils.IsNull(dt.Rows[0]["IsActivate"]))
            {
                _IsActivate = bool.Parse(dt.Rows[0]["IsActivate"].ToString());
            }
            return _IsActivate;
        }
        set { _IsActivate = value; }
    }
    #endregion

    public CurrentDepartment()
	{
        dt = CMSUtils.GetDataSQL("", "SYSTEM_PO", "*", "POID=" + CMSUtils.POID, "");
	}
}