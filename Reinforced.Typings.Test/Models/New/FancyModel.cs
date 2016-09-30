using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Reinforced.Typings.Attributes;

namespace Reinforced.Typings.Test.Models.New
{
    [TsClass]
    public class FancyModel
    {
        public int One { get; set; }
        public List<string> Two { get; set; }
        public bool Zero { get; set; }
        public Dummy[] Dummy { get; set; }
    }
}