using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for EventLogProviderToNewConnectionStrings
/// </summary>
public class EventLogProviderToNewConnectionStrings
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

	public EventLogProviderToNewConnectionStrings() { }

    public int LogEvent(string NewConnectionStrings, EventLogInfo eventLogInfo)
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
            return CMSUtils.InsertDataSQLNoLogToNewConnectionStrings(NewConnectionStrings, "SYSTEM_LOG", cols, values);
        }
        return 0;
    }

    public int LogEvent(string NewConnectionStrings, string type, string logCode, string source, string description, string userName)
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
            return CMSUtils.InsertDataSQLNoLogToNewConnectionStrings(NewConnectionStrings, "SYSTEM_LOG", cols, values);
        }
        return 0;
    }

    public int LogInformation(string NewConnectionStrings, string logCode, string source, string description)
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
            return CMSUtils.InsertDataSQLNoLogToNewConnectionStrings(NewConnectionStrings, "SYSTEM_LOG", cols, values);
        }
        return 0;
    }

    public int LogWarning(string NewConnectionStrings, string source, string logCode, string description)
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
            return CMSUtils.InsertDataSQLNoLogToNewConnectionStrings(NewConnectionStrings,"SYSTEM_LOG", cols, values);
        }
        return 0;
    }

    public int LogException(string NewConnectionStrings, string logCode, string source, string description)
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
            return CMSUtils.InsertDataSQLNoLogToNewConnectionStrings(NewConnectionStrings,"SYSTEM_LOG", cols, values);
        }
        return 0;
    }
}