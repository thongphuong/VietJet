using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Courses
{
    public class PartialIngredientsViewModify
    {
        public int Id { get; set; }
        public string IsCreate { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public Dictionary<int, string> DictionaryIngredients { get; set; }
    }
    public class PartialIngredient
    {
        public int Row { get; set; }
        public int IsOnline { get; set; }
        public Dictionary<int, string> DictionaryIngredients { get; set; }
    }
}
