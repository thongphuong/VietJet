namespace TMS.Core.Services.Orientation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DAL.Entities;
    using TMS.Core.ViewModels.Courses;
    using TMS.Core.ViewModels.UserModels;
    using ViewModels.Room;
    using TMS.Core.ViewModels.Orientation;

    public interface IOrientationService : IBaseService
    {
        IQueryable<Orientation> Get();
        IQueryable<Orientation> Get(Expression<Func<Orientation, bool>> query = null);
        IQueryable<PotentialSuccessor> Get(Expression<Func<PotentialSuccessor, bool>> query = null);
        IQueryable<Orientation_Item> GetItem();
        IQueryable<Orientation_Item> GetItem(Expression<Func<Orientation_Item, bool>> query = null);
        IQueryable<Orientation_Kind_Of_Successor> GetKind();
        IQueryable<Orientation_Kind_Of_Successor> GetKind(Expression<Func<Orientation_Kind_Of_Successor, bool>> query);
        PotentialSuccessor GetbyId(int? id);
        IQueryable<PotentialSuccessors_Item> GetItembyId(int? id);
        int ModifyOrientation(OrientationViewModel model, int? idtrainee, int? idjobtitle, DateTime? expectDate, int? idjob);

        int ModifyOrientationPDP(OrientationPDPViewModel model, int? idtrainee, int? idjobtitle,
            DateTime? expectDate, int? idjob);
        void Insert(Orientation entity);
        void ModifySuccessor(List<OrientationModify> model, int? id, int? idjobfuture);
        void ApproveySuccessor(List<OrientationModify> model, int? id, int typeApprove);

    }
}
