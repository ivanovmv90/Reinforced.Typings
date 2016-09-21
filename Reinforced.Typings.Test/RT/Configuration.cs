using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Reinforced.Typings.Fluent;
using Reinforced.Typings.Test.Controllers;
using Reinforced.Typings.Test.Models;
using Reinforced.Typings.Test.RT.Angular;

namespace Reinforced.Typings.Test.RT
{
    public class Configuration
    {
        public static void Configure(ConfigurationBuilder builder)
        {
            // Configuration for Angular.js-firndly controller
            // Setting code generator for methods also performed in attribute - see AngularMethodAttribute
            //builder.ExportAsClass<HomeController>()
            //    .WithCodeGenerator<AngularControllerGenerator>();

            //builder.ExportAsClass<TestModel>().WithPublicProperties();
        }
    }
}