using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for EventLogProvider
/// </summary>
public class EventLogProvider
{
    // Summary:
    //     Error event type.
    public const string EVENT_TYPE_ERROR = "E";
    //
    // Summary:
    //     Info event type.
    public const string EVENT_TYPE_INFORMATION = "I";
    //
    // Summary:
    //     Warning event type.
    public const string EVENT_TYPE_WARNING = "W";

	public EventLogProvider() { }

    public int LogEvent(EventLogInfo eventLogInfo)
    {
        string[] cols = { "LOGCategory", "CreateDay", "LOGCode", "Source", "POID", "POName", "AccountID",
                              "AccountName", "IP", "Description", "PageID", "PageName", "URLRelatively", "URLFull",
                              "ServerName", "ClientInformation" };
        object[] values = { eventLogInfo.LoaiLuuVet, eventLogInfo.NgayTao, eventLogInfo.MaLuuVet, eventLogInfo.Nguon,
                          eventLogInfo.PhongBanID, eventLogInfo.TenPhongBan, eventLogInfo.TaiKhoanID, eventLogInfo.TenTaiKhoan,
                          eventLogInfo.IP, eventLogInfo.MoTa, eventLogInfo.TrangID, eventLogInfo.TenTrang, eventLogInfo.DuongDanTuongDoi,
                          eventLogInfo.DuongDanDayDu, eventLogInfo.TenServer, eventLogInfo.ThongTinClient };

        if (cols != null && cols.Length > 0)
        {
            return CMSUtils.InsertDataSQLNoLog("SYSTEM_LOG", cols, values);
        }
        return 0;
    }

    public int LogEvent(string type, string logCode, string source, string description, string userName)
    {
        string[] cols = { "LOGCategory", "LOGCode", "Source", "POID", "POName",
                              "AccountName", "IP", "Description", "PageID", "PageName", "URLRelatively", "URLFull",
                              "ServerName", "ClientInformation" };
        object[] values = { type, logCode, source,CMSUtils.POID, CMSContext.CurrentDepartment.POName, 
                              userName, CMSContext.IP, description, CMSContext.CurrentPage.PageID,
                              CMSContext.CurrentPage.PageName, CMSContext.CurrentPage.URLRelatively, CMSContext.CurrentPage.URLFull, 
                              System.Environment.MachineName, CMSContext.UserAgent };

        if (cols != null && cols.Length > 0)
        {
            return CMSUtils.InsertDataSQLNoLog("SYSTEM_LOG", cols, values);
        }
        return 0;
    }

    public int LogInformation(string logCode, string source, string description)
    {
        string[] cols = { "LOGCategory", "LOGCode", "Source", "POID", "POName", "AccountID",
                              "AccountName", "IP", "Description", "PageID", "PageName", "URLRelatively", "URLFull",
                              "ServerName", "ClientInformation" };
        object[] values = { EVENT_TYPE_INFORMATION, logCode, source,CMSUtils.POID, CMSContext.CurrentDepartment.POName, 
                              CMSContext.CurrentAccount.AccountID, CMSContext.CurrentAccount.AccountName, CMSContext.IP, 
                              description, CMSContext.CurrentPage.PageID, CMSContext.CurrentPage.PageName, 
                              CMSContext.CurrentPage.URLRelatively, CMSContext.CurrentPage.URLFull, 
                              System.Environment.MachineName, CMSContext.UserAgent };

        if (cols != null && cols.Length > 0)
        {
            return CMSUtils.InsertDataSQLNoLog("SYSTEM_LOG", cols, values);
        }
        return 0;
    }

    public int LogWarning(string source, string logCode, string description)
    {
        string[] cols = { "LOGCategory", "LOGCode", "Source", "POID", "POName", "AccountID",
                              "AccountName", "IP", "Description", "PageID", "PageName", "URLRelatively", "URLFull",
                              "ServerName", "ClientInformation" };
        object[] values = { EVENT_TYPE_WARNING, logCode, source,CMSUtils.POID, CMSContext.CurrentDepartment.POName, 
                              CMSContext.CurrentAccount.AccountID, CMSContext.CurrentAccount.AccountName, CMSContext.IP, 
                              description, CMSContext.CurrentPage.PageID, CMSContext.CurrentPage.PageName, 
                              CMSContext.CurrentPage.URLRelatively, CMSContext.CurrentPage.URLFull, 
                              System.Environment.MachineName, CMSContext.UserAgent };

        if (cols != null && cols.Length > 0)
        {
            return CMSUtils.InsertDataSQLNoLog("SYSTEM_LOG", cols, values);
        }
        return 0;
    }

    public int LogException(string logCode, string source, string description)
    {
        string[] cols = { "LOGCategory", "LOGCode", "Source", "POID", "POName", "AccountID",
                              "AccountName", "IP", "Description", "PageID", "PageName", "URLRelatively", "URLFull",
                              "ServerName", "ClientInformation" };
        object[] values = { EVENT_TYPE_ERROR, logCode, source,CMSUtils.POID, CMSContext.CurrentDepartment.POName, 
                              CMSContext.CurrentAccount.AccountID, CMSContext.CurrentAccount.AccountName, CMSContext.IP, 
                              description, CMSContext.CurrentPage.PageID, CMSContext.CurrentPage.PageName, 
                              CMSContext.CurrentPage.URLRelatively, CMSContext.CurrentPage.URLFull, 
                              System.Environment.MachineName, CMSContext.UserAgent };

        if (cols != null && cols.Length > 0)
        {
            return CMSUtils.InsertDataSQLNoLog("SYSTEM_LOG", cols, values);
        }
        return 0;
    }
}