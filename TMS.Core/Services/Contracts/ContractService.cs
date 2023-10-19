using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TMS.Core.Services.Contracts
{
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using ViewModels.Contractors;
    using TMS.Core.ViewModels.Contracts;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.ViewModels.UserModels;

    public class ContractService : BaseService, IContractService
    {
        private readonly IRepository<TMS_CONTRACTS> _repoTMS_CONTRACTS;
        private readonly IRepository<CAT_CONTRACTOR> _repoCAT_CONTRACTOR;
        private readonly IRepository<CAT_CONTRACTS_TYPE> _repoCAT_CONTRACTS_TYPE;
        private readonly IRepository<CAT_CONTRACTS_STATUS> _repoCAT_CONTRACTS_STATUS;
        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }

        public ContractService(IUnitOfWork unitOfWork,
            IRepository<TMS_CONTRACTS> repoTMS_CONTRACTS,
            IRepository<CAT_CONTRACTOR> repoCAT_CONTRACTOR,
            IRepository<CAT_CONTRACTS_STATUS> repoCAT_CONTRACTS_STATUS,
            IRepository<CAT_CONTRACTS_TYPE> repoCAT_CONTRACTS_TYPE, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoTMS_CONTRACTS = repoTMS_CONTRACTS;
            _repoCAT_CONTRACTOR = repoCAT_CONTRACTOR;
            _repoCAT_CONTRACTS_TYPE = repoCAT_CONTRACTS_TYPE;
            _repoCAT_CONTRACTS_STATUS = repoCAT_CONTRACTS_STATUS;
        }

        public TMS_CONTRACTS GetById(int? id)
        {
            return !id.HasValue ? null : _repoTMS_CONTRACTS.Get(id.Value);
        }

        public IQueryable<TMS_CONTRACTS> Get()
        {
            return _repoTMS_CONTRACTS.GetAll(a => a.bit_Is_active == true && a.isDelete == false);
        }
        public SelectList GetContractType()
        {
            return new SelectList(_repoCAT_CONTRACTS_TYPE.GetAll(), "id", "name");
        }
        public SelectList GetContractStatus()
        {
            return new SelectList(_repoCAT_CONTRACTS_STATUS.GetAll(), "id", "name");
        }
        public SelectList GetContractList()
        {
            return new SelectList(_repoCAT_CONTRACTOR.GetAll(a => a.bit_Isactive == true).OrderBy(a => a.str_Fullname), "id", "str_Fullname");
        }
        public IQueryable<TMS_CONTRACTS> Get(Expression<Func<TMS_CONTRACTS, bool>> query)
        {
            var entities = _repoTMS_CONTRACTS.GetAll(a => a.isDelete == false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        public IQueryable<TMS_CONTRACTS> GetContract(Expression<Func<TMS_CONTRACTS, bool>> query)
        {
            var entities = _repoTMS_CONTRACTS.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public void Update(TMS_CONTRACTS entity)
        {
            _repoTMS_CONTRACTS.Update(entity);
            Uow.SaveChanges();
        }
        public void Update(ContractModels model, FormCollection form)
        {
            IFormatProvider culture = new System.Globalization.CultureInfo("vi-VN", true);
            var signdate = form["dtm_Signdate"].Trim();
            var expriredate = form["dtm_Expiredate"].Trim();
            var price = form["mon_Price"].Trim();
            var entity = _repoTMS_CONTRACTS.Get(model.Id);
            if (entity == null)
            {
                throw new Exception( "data is not found" );
            }
            entity.int_Id_contractor = model.ContractorID;
            entity.str_Code = form["str_Code"].Trim();
            entity.str_Contractno = form["str_Contractno"].Trim();
            entity.str_Description = form["str_Description"];
            if (!string.IsNullOrEmpty(signdate))
            {
                entity.dtm_Signdate = DateTime.Parse(signdate, culture, System.Globalization.DateTimeStyles.AssumeLocal);
            }
            else
            {
                entity.dtm_Signdate = null;
            }
            if (!string.IsNullOrEmpty(expriredate))
            {
                entity.dtm_Expiredate = DateTime.Parse(expriredate, culture, System.Globalization.DateTimeStyles.AssumeLocal);
            }
            else
            {
                entity.dtm_Expiredate = null;
            }
            entity.str_Note = form["str_Note"].Trim();
            entity.mon_Price = price != "" ? Convert.ToDecimal(price) : 0;
            entity.Currency = model.Currency;
            if (model.StatusID != -1)
            {
                entity.int_Id_status = model.StatusID;
            }
            entity.int_Id_type = model.TypeID;
            entity.dtm_Last_updated_date = DateTime.Now;
            entity.int_Last_updated_by = CurrentUser.USER_ID;
            _repoTMS_CONTRACTS.Update(entity);
            Uow.SaveChanges();

        }
        public void Insert(TMS_CONTRACTS entity)
        {
            _repoTMS_CONTRACTS.Insert(entity);
            Uow.SaveChanges();
        }
        public void Insert(ContractModels model, FormCollection form)
        {
            IFormatProvider culture = new System.Globalization.CultureInfo("vi-VN", true);
            var code = String.IsNullOrEmpty(form["str_Code"].Trim()) ? "" : form["str_Code"].Trim();
            var Contractno = String.IsNullOrEmpty(form["str_Contractno"].Trim()) ? "" : form["str_Contractno"].Trim();
            var signdate = form["dtm_Signdate"].Trim();
            var expriredate = form["dtm_Expiredate"].Trim();
            var price = form["mon_Price"].Trim();


            if (code != "" && Contractno != "")
            {

                var entity = new TMS_CONTRACTS
                {
                    int_Id_contractor = model.ContractorID,
                    str_Code = model.Code,
                    str_Contractno = model.ContractNO,
                    str_Description = form["str_Description"],
                    
                    str_Note = form["str_Note"].Trim(),
                    mon_Price = price != "" ? Convert.ToDecimal(price) : 0,
                    Currency = model.Currency,
                    
                    int_Id_type = model.TypeID,
                    dtm_Created_date = DateTime.Now,
                    int_Created_by = CurrentUser.USER_ID,
                    bit_Is_active = true,
                };
                if(model.StatusID != -1)
                {
                    entity.int_Id_status = model.StatusID;
                }
                if(!string.IsNullOrEmpty(signdate))
                {
                    entity.dtm_Signdate = DateTime.Parse(signdate, culture, System.Globalization.DateTimeStyles.AssumeLocal);
                }
                if(!string.IsNullOrEmpty(expriredate))
                {
                    entity.dtm_Expiredate = DateTime.Parse(expriredate, culture, System.Globalization.DateTimeStyles.AssumeLocal);
                }
                   
                _repoTMS_CONTRACTS.Insert(entity);
                Uow.SaveChanges();
            }
        }
        public CAT_CONTRACTOR GetContractorById(int? id)
        {
            return id.HasValue ? _repoCAT_CONTRACTOR.Get(id) : null;
        }

        public IQueryable<CAT_CONTRACTOR> GetContractor()
        {
            return _repoCAT_CONTRACTOR.GetAll(a => a.isDelete == false && a.bit_Isactive == true);
        }
        public IQueryable<CAT_CONTRACTS_STATUS> GetStatus()
        {
            return _repoCAT_CONTRACTS_STATUS.GetAll();
        }
        public IQueryable<CAT_CONTRACTS_TYPE> GetTypeContract()
        {
            return _repoCAT_CONTRACTS_TYPE.GetAll();
        }
        public IQueryable<CAT_CONTRACTOR> GetContractor(Expression<Func<CAT_CONTRACTOR, bool>> query)
        {
            var entities = _repoCAT_CONTRACTOR.GetAll(a => a.isDelete == false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        public IQueryable<CAT_CONTRACTOR> GetContractorAll(Expression<Func<CAT_CONTRACTOR, bool>> query)
        {
            var entities = _repoCAT_CONTRACTOR.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        public IQueryable<CAT_CONTRACTS_STATUS> GetStatus(Expression<Func<CAT_CONTRACTS_STATUS, bool>> query = null)
        {
            var entities = _repoCAT_CONTRACTS_STATUS.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        public IQueryable<CAT_CONTRACTS_TYPE> GetTypeContract(Expression<Func<CAT_CONTRACTS_TYPE, bool>> query)
        {
            var entities = _repoCAT_CONTRACTS_TYPE.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public void UpdateContractor(CAT_CONTRACTOR entity)
        {
            _repoCAT_CONTRACTOR.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertContractor(CAT_CONTRACTOR entity)
        {
            _repoCAT_CONTRACTOR.Insert(entity);
            Uow.SaveChanges();
        }
        public void InsertContractor(ContractorModels model)
        {
            var entity = new CAT_CONTRACTOR
            {
                str_Code = model.Code,
                str_Sortname = model.SortName,
                str_Fullname = model.FullName,
                str_address = model.Address,
                str_Description = model.Description,
                str_masothue = model.SerialNumberTax,
                dtm_Created_date = DateTime.Now,
                int_Created_by = CurrentUser.USER_ID,
                bit_Isactive = true,
                isDelete = false,
                //IsDelete = false,
            };

            _repoCAT_CONTRACTOR.Insert(entity);
            Uow.SaveChanges();
        }
        public void UpdateContractor(ContractorModels model)
        {
            var entity = _repoCAT_CONTRACTOR.Get(model.Id);
            if (entity == null)
            {
                throw new Exception( Messege.NO_DATA );
            }
            entity.str_Code = model.Code;
            entity.str_Sortname = model.SortName;
            entity.str_Fullname = model.FullName;
            entity.str_address = model.Address;
            entity.str_Description = model.Description;
            entity.str_masothue = model.SerialNumberTax;
            entity.dtm_Updated_date = DateTime.Now;
            entity.int_Updated_by = CurrentUser.USER_ID;

            _repoCAT_CONTRACTOR.Update(entity);
            Uow.SaveChanges();

        }
    }
}
