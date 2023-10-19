using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Core.ViewModels.Contact;

namespace TMS.Core.Services.Contact
{
    using System.Linq.Expressions;
    using DAL.Entities;

    public interface IContactService
    {
        INFO_CONTACT GetById(int? id);
        IQueryable<INFO_CONTACT> GetAll(Expression<Func<INFO_CONTACT, bool>> query =null );
        IQueryable<INFO_CONTACT> GetContact(Expression<Func<INFO_CONTACT, bool>> query);
        void Insert(INFO_CONTACT entity);
        bool Insert(ContactDetails model);
        void Update(INFO_CONTACT entity);        
        void UpdateContact(ContactDetails model);
        void InsertContact(ContactDetails model);
    }
}
