using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Reinforced.Typings.Attributes;

namespace Reinforced.Typings.Test.Models
{
    [TsClass]
    public class Dummy
    {
        public TestModel Test { get; set; }
    }
}