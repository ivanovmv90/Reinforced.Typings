using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Reinforced.Typings.Attributes;

namespace Reinforced.Typings.Test.Models.New
{
    [TsClass]
    public class ExtendedFancyModel: FancyModel
    {
        public string NewFancyProperty { get; set; }
    }
}