using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Lấy được giá trị như tên, id của huyện, đơn vi đổ vào trong dropdownlist
/// </summary>
public class CLDropDownListLog
{
    private string dataTextField;

    public string DataTextField
    {
        get { return dataTextField; }
        set { dataTextField = value; }
    }
    private string dataValueField;

    public string DataValueField
    {
        get { return dataValueField; }
        set { dataValueField = value; }
    }

	public CLDropDownListLog()
	{
	
	}

    public CLDropDownListLog(string dataTextField, string dataValueField)
    {
        this.dataTextField = dataTextField;
        this.dataValueField = dataValueField;
    }
}