using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.ViewModel
{
    public class LogViewModel
    {
        public Dictionary<string,string> dic_Content { get; set; }
        public Dictionary<string, object> obj_Content { get; set; }
        public Dictionary<string, object> depart_Content { get; set; }
        public Dictionary<string, object> subject_Content { get; set; }
        public Dictionary<string, object> instructor_Content { get; set; }
        public string _department { get; set; }
    }
    public class RetrieveMultipleResponse
    {
        public List<Attribute> Attributes { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
    }
    public class Value
    {
        [JsonProperty("Value")]
        public string value { get; set; }
        public List<string> Values { get; set; }
    }

    public class Attribute
    {
        public string Key { get; set; }
        public Value Value { get; set; }
    }
}
