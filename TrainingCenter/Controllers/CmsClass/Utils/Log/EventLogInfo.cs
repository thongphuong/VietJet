using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for EventLogInfo
/// </summary>
public class EventLogInfo
{
    private string loaiLuuVet;
    private DateTime? ngayTao;
    private string maLuuVet;
    private string nguon;
    private int? phongBanID;
    private string tenPhongBan;
    private int? taiKhoanID;
    private string tenTaiKhoan;
    private string ip;
    private string moTa;
    private int? trangID;
    private string tenTrang;
    private string duongDanTuongDoi;
    private string duongDanDayDu;
    private string tenServer;
    private string thongTinClient;

    #region [Get Set]
    public string LoaiLuuVet { get; set; }
    public DateTime? NgayTao { get; set; }
    public string MaLuuVet { get; set; }
    public string Nguon { get; set; }
    public int? PhongBanID { get; set; }
    public string TenPhongBan { get; set; }
    public int? TaiKhoanID { get; set; }
    public string TenTaiKhoan { get; set; }
    public string IP { get; set; }
    public string MoTa { get; set; }
    public int? TrangID { get; set; }
    public string TenTrang { get; set; }
    public string DuongDanTuongDoi { get; set; }
    public string DuongDanDayDu { get; set; }
    public string TenServer { get; set; }
    public string ThongTinClient { get; set; }
    #endregion

    public EventLogInfo()
	{
		
	}
}