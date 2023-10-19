namespace TMS.Core.ViewModels
{
    using System.Collections.Generic;
    using DAL.Entities;

    public class MultiModels
    {
        public IEnumerable<Department> Departments { get; set; }
        public IEnumerable<JobTitle> JobTiltles { get ; set; }

    }
}