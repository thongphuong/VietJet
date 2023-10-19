using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;

namespace TMS.Core.Utils
{
    public class PartialCacheAttribute : OutputCacheAttribute
    {
        public PartialCacheAttribute(string cacheProfileName)
        {
            var cacheSection = (OutputCacheSettingsSection)WebConfigurationManager
                                .GetSection("system.web/caching/outputCacheSettings");

            var cacheProfile = cacheSection.OutputCacheProfiles[cacheProfileName];

            Duration = cacheProfile.Duration;
            VaryByParam = cacheProfile.VaryByParam;
            Location = cacheProfile.Location;
            SqlDependency = cacheProfile.SqlDependency;
            NoStore = cacheProfile.NoStore;
        }
    }
}
