using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.TraineeHis

{
    using System.Linq.Expressions;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.Utils;

    public class TraineeHistoryService : BaseService, ITraineeHistoryService
    {
        private readonly IRepository<TraineeHistory> _repoTraineeHistory;
        private readonly IRepository<Trainee> _repoTrainee;

        public TraineeHistoryService(IUnitOfWork unitOfWork, IRepository<TraineeHistory> repoTraineeHistory,
            IRepository<Trainee> repoTrainee,
            IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoTraineeHistory = repoTraineeHistory;
            _repoTrainee = repoTrainee;


        }


        public IQueryable<TraineeHistory> Get(Expression<Func<TraineeHistory, bool>> query)
        {
            return _repoTraineeHistory.GetAll(query);
        }

        public Trainee GetTraineeByUsername(string username)
        {
            var query = _repoTrainee.GetAll().Where(c => c.str_Staff_Id.Equals(username)).FirstOrDefault();
            return query;
        }

        public void Insert(TraineeHistory entity)
        {
            _repoTraineeHistory.Insert(entity);
            Uow.SaveChanges();
        }
    }
}
