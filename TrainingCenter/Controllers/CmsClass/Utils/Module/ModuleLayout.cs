using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;


public class ModuleLayout
{
    public static string Nofitication()
    {
        StringBuilder sbHtml = new StringBuilder();
        DataTable db = CMSUtils.GetDataSQL("", "Messages", "*", "", "");
        if (db.Rows.Count > 0)
        {
            foreach (DataRow user in db.Rows)
            {
                sbHtml.AppendFormat("<li> <a href='#'> <div class='pull-left'> <img src='/img/avatar2.png' class='img-circle' alt='user image' /> </div> <h4> {0} <small><i class='fa fa-clock-o'></i>2 hours</small> </h4> <p>Why not buy a new awesome theme?</p> </a> </li>", user["Message"].ToString());
            }
        }

        return sbHtml.ToString();
    }
}
