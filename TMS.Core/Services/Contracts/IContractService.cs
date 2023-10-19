using System;
using System.Linq;
using System.Linq.Expressions;
using DAL.Entities;
using System.Web.Mvc;

namespace TMS.Core.Services.Contracts
{
    using TMS.Core.ViewModels.Contracts;
    using ViewModels.Contractors;
    public interface IContractService : IBaseService
    {
        TMS_CONTRACTS GetById(int? id);
        IQueryable<TMS_CONTRACTS> Get();
        SelectList GetContractType();
        SelectList GetContractStatus();
        SelectList GetContractList();
        IQueryable<TMS_CONTRACTS> Get(Expression<Func<TMS_CONTRACTS, bool>> query);
        IQueryable<TMS_CONTRACTS> GetContract(Expression<Func<TMS_CONTRACTS, bool>> query);

        void Update(TMS_CONTRACTS entity);
        void Insert(TMS_CONTRACTS entity);
        void Insert(ContractModels model, FormCollection form);
        void Update(ContractModels model, FormCollection form);
        CAT_CONTRACTOR GetContractorById(int? id);
        IQueryable<CAT_CONTRACTOR> GetContractor();
        IQueryable<CAT_CONTRACTS_STATUS> GetStatus();
        IQueryable<CAT_CONTRACTS_TYPE> GetTypeContract();
        IQueryable<CAT_CONTRACTOR> GetContractor(Expression<Func<CAT_CONTRACTOR, bool>> query);
        IQueryable<CAT_CONTRACTOR> GetContractorAll(Expression<Func<CAT_CONTRACTOR, bool>> query);
        IQueryable<CAT_CONTRACTS_STATUS> GetStatus(Expression<Func<CAT_CONTRACTS_STATUS, bool>> query = null);
        IQueryable<CAT_CONTRACTS_TYPE> GetTypeContract(Expression<Func<CAT_CONTRACTS_TYPE, bool>> query = null);

        void UpdateContractor(CAT_CONTRACTOR entity);
        void InsertContractor(CAT_CONTRACTOR entity);
        void UpdateContractor(ContractorModels model);
        void InsertContractor(ContractorModels model);
    }
}
