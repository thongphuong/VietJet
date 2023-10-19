using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrainingCenter.Utilities;
using TMS.Core.ViewModels;
using DAL.UnitOfWork;
using DAL.Repositories;
using DAL.Entities;

namespace TrainingCenter.Serveices
{
    public class GetConfig
    {
        public static string ByKey(string key)
        {
            using (var dbContext = new EFDbContext())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbContext);
                IRepository<CONFIG> repoconfig = null;
                repoconfig = unitOfWork.Repository<CONFIG>();
                var firstOrDefault = repoconfig.Get(a => a.KEY == key);
                if (firstOrDefault != null)
                {
                    var value = firstOrDefault.VALUE;
                    return value;
                }
                return "Error";
            }
        }

        public static IEnumerable<CONFIG> ByName(string name)
        {
            using (var dbContext = new EFDbContext())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbContext);
                IRepository<CONFIG> repoconfig = null;
                repoconfig = unitOfWork.Repository<CONFIG>();
                var configs = repoconfig.GetAll(a => a.NAME == name);
                return configs;
            }
        }
    }
}