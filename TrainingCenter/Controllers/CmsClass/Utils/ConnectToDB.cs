using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ConnectToDB
/// </summary>
public class ConnectToDB
{
    public ConnectToDB()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    public static string connectionString
    {
        get { return System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString; }
    }
    public static SqlConnection getConnection()
    {
        SqlConnection con = new SqlConnection(connectionString);
        try
        {
            con.Open();
        }
        catch
        {
            con = null;
        }
        return con;
    }
    public static void CloseConnection(SqlConnection con)
    {
        try
        {
            con.Close();
        }
        catch
        {
            con = null;
        }
    }
}