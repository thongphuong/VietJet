using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Core.ViewModels.Contact;

namespace TMS.Core.Services.Contact
{
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.App_GlobalResources;

    public class ContactService : BaseService, IContactService
    {
        private readonly IRepository<INFO_CONTACT> _repoContact;
        public ContactService(IUnitOfWork unitOfWork, IRepository<INFO_CONTACT> repoContact, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoContact = repoContact;
        }

        public INFO_CONTACT GetById(int? id)
        {
            return !id.HasValue ? null : _repoContact.Get(id.Value);
        }


        public IQueryable<INFO_CONTACT> GetAll(Expression<Func<INFO_CONTACT, bool>> query = null)
        {
            var entities = _repoContact.GetAll(a => a.IsDeleted == false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public IQueryable<INFO_CONTACT> GetContact(Expression<Func<INFO_CONTACT, bool>> query)
        {
            var entities = _repoContact.GetAll(a => a.IsDeleted == false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public void Insert(INFO_CONTACT entity)
        {
            _repoContact.Insert(entity);
            Uow.SaveChanges();
        }

        public bool Insert(ContactDetails model)
        {
            var entity = _repoContact.Get(model.Id);
            if (entity != null)
            {
                entity.INFO_REPLY_CONTACT.Add(new INFO_REPLY_CONTACT()
                {
                    Contact_Id = model.Id,
                    Content = model.FContent,
                    Subject = model.FSubject,
                    CreationDate = DateTime.Now,
                    Email = model.Email
                });
                Uow.SaveChanges();
                return true;
            }
            return false;


        }
        public void Update(INFO_CONTACT entity)
        {
            _repoContact.Update(entity);
            Uow.SaveChanges();
        }
               

        public void UpdateContact(ContactDetails model)
        {
            var entity = _repoContact.Get(model.Id);
            if (entity == null)
            {
                throw new Exception(Messege.NO_DATA);
            }
            entity.FullName = model.FullName;
            entity.Company = model.Company;
            entity.Email = model.Email;
            entity.Phone = model.Phone;
            entity.IsDeleted = model.isDeleted;
            _repoContact.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertContact(ContactDetails model)
        {
            var entity = new INFO_CONTACT
            {
                FullName = model.FullName,
                Company = model.Company,
                Email=model.Email,
                Phone=model.Phone,          
                IsDeleted=model.isDeleted,
                bit_Is_active=true,
               
            };
            _repoContact.Insert(entity);
            Uow.SaveChanges();
        }



    }
}
