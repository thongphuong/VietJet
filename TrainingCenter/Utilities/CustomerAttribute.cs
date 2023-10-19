using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrainingCenter.Utilities
{
    public class CustomAttribute : Attribute
    {
        public string Role { get; set; }
        public CustomAttribute(string role)
        {
            this.Role = role;
        }
    }
}