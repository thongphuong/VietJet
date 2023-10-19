using System.Collections.Generic;
using TMS.Core.ViewModels.Common;

namespace TMS.Core.ViewModels.Jobtitles
{

    public class JobTitleOptionsViewModel
    {
        public Dictionary<int,string> JobLevels { get; set; }
        public IEnumerable<CustomDataListModel> JobHeaders { get; set; }
        public IEnumerable<CustomDataListModel> JobPositions { get; set; }
    }
}
