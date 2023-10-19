using System;
using System.Data.Entity;
using DAL.Entities;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Reflection;
using System.Web;

namespace DAL.UnitOfWork
{
    using DAL.Repositories;

    public class UnitOfWork : IUnitOfWork
    {
        private readonly EFDbContext _dbContext;
        private bool _disposed = false;
        public UnitOfWork(EFDbContext dbcontext)
        {
            _dbContext = dbcontext;
            _disposed = false;
            //LogWrite("Opening Connection");
        }

        //private readonly EFDbContext _dbContext;
        //public UnitOfWork()
        //{
        //    _dbContext =new EFDbContext();
        //}


        public ObjectResult<sp_GetTrainingHeaderTV_Result> sp_GetTrainingHeader_Result(string listId, string departmentCode, DateTime? fromDateStart, DateTime? fromDateEnd, DateTime? toDateStart, DateTime? toDateOfEnd, string status)
        {
            return _dbContext.sp_GetTrainingHeaderTV(listId, departmentCode, fromDateStart, fromDateEnd, toDateStart,
                toDateOfEnd, status);
        }
        public ObjectResult<sp_GetCostHeader_Result> sp_GetCostHeader_Result(string listId, DateTime? fromDateStart, DateTime? fromDateEnd, DateTime? toDateStart, DateTime? toDateOfEnd)
        {
            return _dbContext.sp_GetCostHeader(listId, fromDateStart, fromDateEnd, toDateStart,
                toDateOfEnd);
        }
        public ObjectResult<sp_GetCostDetail_Result> sp_GetCostDetail_Result(string listId, DateTime? fromDateStart, DateTime? fromDateEnd, DateTime? toDateStart, DateTime? toDateOfEnd)
        {
            return _dbContext.sp_GetCostDetail(listId, fromDateStart, fromDateEnd, toDateStart,
                toDateOfEnd);
        }
        public ObjectResult<sp_GetTrainingDetail_Result> sp_GetTrainingDetail_Result(string listId, string departmentCode, DateTime? fromDateStart,
            DateTime? fromDateEnd, DateTime? toDateStart, DateTime? toDateOfEnd, string status)
        {
            return _dbContext.sp_GetTrainingDetail(listId, departmentCode, fromDateStart, fromDateEnd, toDateStart, toDateOfEnd, status);
        }

        public IRepository<T> Repository<T>() where T : class
        {
            return new Repository<T>(_dbContext);
        }
        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                    //LogWrite("Closed Connection");
                }
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private string m_exePath = string.Empty;

        private void LogWrite(string logMessage)
        {
            var name = "LogConnection.txt";
            m_exePath = Path.Combine(HttpRuntime.AppDomainAppPath + "/LogConnection/");
            CheckFileDefault(m_exePath);
            try
            {
                var fullPath = m_exePath + name;
                var fileInfo = new FileInfo(fullPath);
                if (fileInfo.Exists)
                {
                    var bytes = fileInfo.Length;
                    var kilobytes = (double)bytes / 1024;
                    var megabytes = kilobytes / 1024;
                    //var gibabytes = megabytes / 1024;

                    if (megabytes > 5)
                    {
                        System.IO.File.WriteAllText(fullPath, string.Empty);
                    }
                }
                using (var w = File.AppendText(m_exePath + "\\" + name))
                {
                    Log(logMessage + fileInfo.Length, w);
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }
        private void CheckFileDefault(string path)
        {
            var isExists = System.IO.Directory.Exists(path);
            if (!isExists)
            {
                System.IO.Directory.CreateDirectory(path);
            }


        }
        private void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  : ");
                txtWriter.WriteLine("  :{0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
                txtWriter.Flush();
                txtWriter.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }



        #endregion
        public ObjectResult<sp_Reminder_TV_Result> sp_GetReminder_Result(string departlist, string listJobtitle, string subjectName, string subjectCode, DateTime dateFrom, int nom)
        {
            return _dbContext.sp_Reminder_TV(departlist, listJobtitle, subjectName, subjectCode, dateFrom, nom);
        }

        public ObjectResult<sp_GetSubjectResult_TV_Result> sp_GetSubjectResult_TV_Result(int courseDetailId)
        {
            return _dbContext.sp_GetSubjectResult_TV(courseDetailId);
        }

        public ObjectResult<sp_GetInstructorReport_TV_Result> sp_GetInstructorReport_TV_Result(string subjectName, string subjectCode, string traineeName, string traineeCode, int departmentID, int jobtitleID, string sortDirection, string orderingFunction)
        {
            return _dbContext.sp_GetInstructorReport_TV(subjectName, subjectCode, traineeName, traineeCode, departmentID, jobtitleID, sortDirection, orderingFunction);
        }
        public ObjectResult<sp_GetListEmployee_TV_Result> sp_GetEmployeeList_TV_Result(string filterCodeOrName)
        {
            return _dbContext.sp_GetListEmployee_TV(filterCodeOrName);
        }

        public ObjectResult<sp_GetDetail_TV_Result> sp_GetDetail_Result(string venue, string courseCode, string courseName, int? departmentid, DateTime? dateFrom, DateTime? dateTo, string courids, int? CourseID)
        {
            return _dbContext.sp_GetDetail_TV(venue, courseCode, courseName, departmentid, dateFrom, dateTo, courids, CourseID);
        }
    }
}
