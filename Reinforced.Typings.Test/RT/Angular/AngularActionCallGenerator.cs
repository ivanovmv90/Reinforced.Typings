using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Reinforced.Typings.Ast;
using Reinforced.Typings.Generators;

namespace Reinforced.Typings.Test.RT.Angular
{
    /// <summary>
    /// Action call generator for controller method inside angularjs glue-class is quite similar
    /// to jQuery's one.
    /// </summary>
    public class AngularActionCallGenerator : MethodCodeGenerator
    {
        public override RtFuncion GenerateNode(MethodInfo element, RtFuncion result, TypeResolver resolver)
        {
            result = base.GenerateNode(element, result, resolver);
            if (result == null) return null;

            // here we are overriding return type to corresponding promise
            var retType = result.ReturnType;
            bool isVoid = (retType is RtSimpleTypeName) && (((RtSimpleTypeName)retType).TypeName == "void");

            // we use TypeResolver to get "any" type to avoid redundant type name construction
            // (or because I'm too lazy to manually construct "any" type)
            if (isVoid) retType = resolver.ResolveTypeName(typeof(object));

            var genericReturnType = retType;
            var isArrayType = retType is RtArrayType;
            if (retType is RtSimpleTypeName || isArrayType)
            {
                //TODO: if array of other than RtSimpleType
                var retTypeSimple = isArrayType? (retType as RtArrayType).ElementType as RtSimpleTypeName : retType as RtSimpleTypeName;
                string ns = retTypeSimple.Namespace;
                if (!string.IsNullOrWhiteSpace(ns))
                {
                    var moduleName = Context.Location.CurrentModule.ModuleName;
                    var importSource = string.Join("/", moduleName.Split('.').Select(x => "..")) 
                        + "/" 
                        + ns.Replace(".", "/") 
                        + "/" 
                        + retTypeSimple.TypeName;
                    Context.AdditionalImports.Add($"import {{{retTypeSimple.TypeName}}} from \"{importSource}\";");
                    var rtSimpleTypeName = new RtSimpleTypeName(retTypeSimple.TypeName);
                    rtSimpleTypeName.Namespace = string.Empty;
                    genericReturnType = isArrayType ? new RtArrayType(rtSimpleTypeName) as RtTypeName : rtSimpleTypeName;
                }
            }

            // Here we override TS method return type to make it angular.IPromise
            // We are using RtSimpleType with generig parameter of existing method type
            result.ReturnType = new RtSimpleTypeName("Observable", new[] { genericReturnType });

            // Here we retrieve method parameters
            // We are using .GetName() extension method to retrieve parameter name
            // It is supplied within Reinforced.Typings and retrieves parameter name 
            // including possible name override with Fluent configuration or 
            // [TsParameter] attribute
            var p = element.GetParameters().Select(c => string.Format("'{0}': {0}", c.GetName()));

            // Joining parameters for method body code
            var dataParameters = string.Join(", ", p);

            // Here we get path to controller
            // It is quite simple solution requiring /{controller}/{action} route
            string controller = element.DeclaringType.Name.Replace("Controller", String.Empty);
            string path = $"/{controller}/{element.Name}";

            var code = new StringBuilder();
            code.AppendLine($"var params = {{ {dataParameters} }};");
            code.AppendLine($"return this.http.post('{path}', params)");
            code.AppendLine($"    .map((r: Response) => r.json().data) as {result.ReturnType};");
            RtRaw body = new RtRaw(code.ToString());
            result.Body = body;

            // That's all. here we return node that will be written to target file.
            // Check result in /Scripts/ReinforcedTypings/GeneratedTypings.ts
            return result;
        }
    }
}