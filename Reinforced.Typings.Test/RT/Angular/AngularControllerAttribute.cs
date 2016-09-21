using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Reinforced.Typings.Attributes;

namespace Reinforced.Typings.Test.RT.Angular
{
    public class AngularControllerAttribute : TsClassAttribute
    {
        public AngularControllerAttribute()
        {
            CodeGeneratorType = typeof (AngularControllerGenerator);
            AutoExportConstructors = false;
            AutoExportMethods = false;
        }
    }
}