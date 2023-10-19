using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
//======================
#region Assembly System.Data.dll, v2.0.0.0
// C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.Data.dll
#endregion

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
//================
using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

public class MBMSqlHelperNewConnectionStrings : System.Web.UI.Page
{
    /// <summary>
    /// Get Sql Connection in Web.config
    /// </summary>

    /// <summary>
    /// Execute an update store
    /// </summary>
    /// <param name="storeName">Store procedure name</param>
    /// <param name="pars">Sql Parameter command</param>
    /// <returns></returns>
    public static int ExecuteNonQuery(string ConnectionStrings, string storeName, SqlParameter[] pars, Boolean check)
    {
        if (string.IsNullOrEmpty(storeName))
            return 0;
        else if (check)
            return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteNonQuery(ConnectionStrings, CommandType.StoredProcedure, storeName, pars);
        else
            return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteNonQuery(ConnectionStrings, CommandType.Text, storeName, pars);
    }
    /// <summary>
    /// Execute an command text
    /// </summary>
    /// <param name="commandText"></param>
    /// <returns></returns>
    public static int ExecuteNonQuery(string ConnectionStrings, string commandText)
    {
        if (string.IsNullOrEmpty(commandText))
            return 0;
        else
            return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteNonQuery(ConnectionStrings, CommandType.Text, commandText);
    }

    /// <summary>
    /// Get DataTable by Store Procedure
    /// </summary>
    /// <param name="storeName"></param>
    /// <param name="pars"></param>
    /// <returns></returns>
    public static DataTable ExecuteDataTable(string ConnectionStrings, string storeName, SqlParameter[] pars)
    {
        return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteDataset(ConnectionStrings, CommandType.StoredProcedure, storeName, pars).Tables[0];
    }

    /// <summary>
    /// Get DataTable by command text
    /// </summary>
    /// <param name="commandLine"></param>
    /// <returns></returns>
    public static DataTable ExecuteDataTable(string ConnectionStrings, string commandText)
    {
        return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteDataset(ConnectionStrings, CommandType.Text, commandText).Tables[0];
    }

    /// <summary>
    /// Get as SqlDataReader
    /// </summary>
    /// <param name="commandText">command text</param>
    /// <returns></returns>
    public static SqlDataReader ExecuteDataReader(string ConnectionStrings, string commandText)
    {
        return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteReader(ConnectionStrings, CommandType.Text, commandText);
    }

    /// <summary>
    /// Get as SqlDataReader
    /// </summary>
    /// <param name="storeName">store name</param>
    /// <param name="pars"></param>
    /// <returns></returns>
    public static SqlDataReader ExecuteDataReader(string ConnectionStrings, string storeName, SqlParameter[] pars)
    {
        return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteReader(ConnectionStrings, CommandType.StoredProcedure, storeName, pars);
    }

    /// <summary>
    /// Get as Object
    /// </summary>
    /// <param name="storeName"></param>
    /// <param name="pars"></param>
    /// <returns></returns>
    public static object ExecuteScalar(string ConnectionStrings, string storeName, SqlParameter[] pars, Boolean check)
    {
        if (check)
            return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteScalar(ConnectionStrings, CommandType.StoredProcedure, storeName, pars);
        else
            return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteScalar(ConnectionStrings, CommandType.Text, storeName, pars);

    }

    /// <summary>
    /// Get as object
    /// </summary>
    /// <param name="commandText"></param>
    /// <returns></returns>
    public static object ExecuteScalar(string ConnectionStrings, string commandText)
    {
        return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteScalar(ConnectionStrings, CommandType.Text, commandText);
    }
}

