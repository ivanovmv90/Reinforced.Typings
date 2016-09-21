using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Reinforced.Typings.Attributes;
using Reinforced.Typings.Test.Models;
using Reinforced.Typings.Test.Models.New;
using Reinforced.Typings.Test.RT.Angular;

namespace Reinforced.Typings.Test.Controllers
{
    [TsClass(CodeGeneratorType = typeof(AngularControllerGenerator))]
    public class HomeController : Controller
    {
        [AngularMethod(typeof(int))]
        public ActionResult Index()
        {
            return Json(0);
        }

        [AngularMethod(typeof(TestModel))]
        public TestModel Test()
        {
            return new TestModel();
        }

        [AngularMethod(typeof(Dummy))]
        public Dummy GetStr()
        {
            return new Dummy();
        }

        [AngularMethod(typeof(FancyModel))]
        public FancyModel FancyMethod(int id)
        {
            return new FancyModel();
        }
    }
}
