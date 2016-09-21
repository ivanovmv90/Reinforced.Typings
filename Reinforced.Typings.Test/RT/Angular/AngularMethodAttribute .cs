using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Reinforced.Typings.Attributes;

namespace Reinforced.Typings.Test.RT.Angular
{
    public class AngularMethodAttribute : TsFunctionAttribute
    {
        public AngularMethodAttribute(Type returnType)
        {
            // Here we override method return type for TypeScript export
            StrongType = returnType;

            // Here we are specifying code generator for particular method
            CodeGeneratorType = typeof(AngularActionCallGenerator);
        }
    }
}