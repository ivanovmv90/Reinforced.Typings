using System;
using System.Collections.Generic;
using System.Text;
using Reinforced.Typings.Ast;
using Reinforced.Typings.Generators;

namespace Reinforced.Typings.Test.RT.Angular
{
    /// <summary>
    /// We have to add some fields and constructor to Angular service
    /// </summary>
    public class AngularControllerGenerator : ClassCodeGenerator
    {
        public override RtClass GenerateNode(Type element, RtClass result, TypeResolver resolver)
        {
            result = base.GenerateNode(element, result, resolver);
            if (result == null) return null;

            // Here we just create ng.IHttpService type name because it is used several times
            var httpServiceType = new RtSimpleTypeName("Http") {};

            // Here we are declaring constructor for our angular service using $http as parameter
            // It is quite simple so no more details
            RtConstructor constructor = new RtConstructor();
            constructor.Arguments.Add(new RtArgument(){ Type = httpServiceType, Identifier = new RtIdentifier("private http") });

            // Here we add import declarations
            var importCode = new StringBuilder();
            importCode.AppendLine("import {A, B} from \"test\";");
            Context.Location.CurrentModule.CompilationUnits.Add(new RtRaw(importCode.ToString()));

            result.AngularDecorator = "@Injectable()";

            // Here we are adding our constructor to resulting class
            result.Members.Add(constructor);

            // That's all. here we return node that will be written to target file.
            // Check result in /Scripts/ReinforcedTypings/GeneratedTypings.ts
            return result;
        }
    }
}