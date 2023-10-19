using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.TraineeHis
{
    using System.Linq.Expressions;
    using DAL.Entities;

    public interface ITraineeHistoryService : IBaseService
    {
     
        IQueryable<TraineeHistory> Get(Expression<Func<TraineeHistory, bool>> query);

        Trainee GetTraineeByUsername(string username);
        void Insert(TraineeHistory entity);
    }
}
