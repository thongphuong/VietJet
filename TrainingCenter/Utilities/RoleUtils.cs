using System;
using System.Web.Security;
using DAL.Entities;

namespace Utilities
{
    public static class RoleUtils
    {
        //public static void setRole(UserProfile user)
        //{
        //    try
        //    {
        //        Roles.RemoveUserFromRoles(user.UserName, Roles.GetRolesForUser(user.UserName));
        //    }
        //    catch (Exception) { }

        //    if (user.UserName.Trim().ToUpper() == "ADMIN")
        //    {
        //        Roles.AddUserToRoles(user.UserName, new string[] { "UserProfile", "UserGroup", "Department", "Configuration", "Subject", "JobTitle", "Course", "Trainee", "Room", "Instructor", "Report","RegisterTrainee","Approve", "Recurrent" });
        //    }
        //    else if (user.Group_Roles != null)
        //    {
        //        if (user.Group_Roles.bit_UserProfile)
        //            Roles.AddUserToRole(user.UserName, "UserProfile");
        //        if (user.Group_Roles.bit_Group)
        //            Roles.AddUserToRole(user.UserName, "UserGroup");
        //        if (user.Group_Roles.bit_Department)
        //            Roles.AddUserToRole(user.UserName, "Department");
        //        if (user.Group_Roles.bit_Configuration)
        //            Roles.AddUserToRole(user.UserName, "Configuration");
        //        if (user.Group_Roles.bit_Subject)
        //            Roles.AddUserToRole(user.UserName, "Subject");
        //        if (user.Group_Roles.bit_JobTitle)
        //            Roles.AddUserToRole(user.UserName, "JobTitle");
        //        if (user.Group_Roles.bit_Course)
        //            Roles.AddUserToRole(user.UserName, "Course");
        //        if (user.Group_Roles.bit_Trainee)
        //            Roles.AddUserToRole(user.UserName, "Trainee");
        //        if (user.Group_Roles.bit_Room)
        //            Roles.AddUserToRole(user.UserName, "Room");
        //        if (user.Group_Roles.bit_Approve)
        //            Roles.AddUserToRole(user.UserName, "Approve");
        //        if (user.Group_Roles.bit_Report)
        //            Roles.AddUserToRole(user.UserName, "Report");
        //        if (user.Group_Roles.bit_RegisterTrainee)
        //            Roles.AddUserToRole(user.UserName, "RegisterTrainee");
        //        if (user.Group_Roles.bit_Instructor)
        //            Roles.AddUserToRole(user.UserName, "Instructor");
        //        if (user.Group_Roles.bit_Recurrent)
        //            Roles.AddUserToRole(user.UserName, "Recurrent");
        //    }
        //}
    }
}