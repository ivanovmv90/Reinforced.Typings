﻿using System;
using System.Linq;
using System.Reflection;
using Reinforced.Typings.Ast;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Xmldoc.Model;

namespace Reinforced.Typings.Generators
{
    /// <summary>
    ///     Default typescript code generator for method
    /// </summary>
    public class MethodCodeGenerator : TsCodeGeneratorBase<MethodInfo, RtFuncion>
    {
        /// <summary>
        ///     Main code generator method. This method should write corresponding TypeScript code for element (1st argument) to
        ///     WriterWrapper (3rd argument) using TypeResolver if necessary
        /// </summary>
        /// <param name="element">Element code to be generated to output</param>
        /// <param name="result">Resulting node</param>
        /// <param name="resolver">Type resolver</param>
        public override RtFuncion GenerateNode(MethodInfo element, RtFuncion result, TypeResolver resolver)
        {
            if (element.IsIgnored()) return null;

            string name;
            RtTypeName type;

            GetFunctionNameAndReturnType(element, resolver, out name, out type);
            result.Identifier = new RtIdentifier(name);
            result.ReturnType = type;

            var doc = Context.Documentation.GetDocumentationMember(element);
            if (doc != null)
            {
                RtJsdocNode jsdoc = new RtJsdocNode { Description = doc.Summary.Text };
                if (doc.Parameters != null)
                {
                    foreach (var documentationParameter in doc.Parameters)
                    {
                        jsdoc.TagToDescription.Add(new Tuple<DocTag, string>(DocTag.Param,
                            documentationParameter.Name + " " + documentationParameter.Description));
                    }
                }

                if (doc.HasReturns())
                {
                    jsdoc.TagToDescription.Add(new Tuple<DocTag, string>(DocTag.Returns, doc.Returns.Text));
                }
                result.Documentation = jsdoc;
            }
            result.Order = element.GetOrder();

            result.AccessModifier = element.GetModifier();
            if (Context.SpecialCase) result.AccessModifier = AccessModifier.Public;
            result.Identifier = new RtIdentifier(name);
            result.IsStatic = element.IsStatic;

            var p = element.GetParameters();
            foreach (var param in p)
            {
                if (param.IsIgnored()) continue;
                var generator = Context.Generators.GeneratorFor(param, Context);
                var argument = generator.Generate(param, resolver);
                result.Arguments.Add((RtArgument)argument);
            }
            var fa = ConfigurationRepository.Instance.ForMember(element);
            if (fa != null && !string.IsNullOrEmpty(fa.Implementation))
            {
                result.Body = new RtRaw(fa.Implementation);
            }
            AddDecorators(result, ConfigurationRepository.Instance.DecoratorsFor(element));
            return result;
        }

        /// <summary>
        ///     Retrieves function name corresponding to method and return type. Fell free to override it.
        /// </summary>
        /// <param name="element">Method info</param>
        /// <param name="resolver">Type resolver</param>
        /// <param name="name">Resulting method name</param>
        /// <param name="type">Resulting return type name</param>
        protected virtual void GetFunctionNameAndReturnType(MethodInfo element, TypeResolver resolver, out string name, out RtTypeName type)
        {
            name = element.Name;
            var fa = ConfigurationRepository.Instance.ForMember(element);

            if (fa != null)
            {
                if (!string.IsNullOrEmpty(fa.Name)) name = fa.Name;

                if (!string.IsNullOrEmpty(fa.Type)) type = new RtSimpleTypeName(fa.Type);
                else if (fa.StrongType != null) type = resolver.ResolveTypeName(fa.StrongType);
                else type = resolver.ResolveTypeName(element.ReturnType);
            }
            else
            {
                type = resolver.ResolveTypeName(element.ReturnType);
            }

            name = Context.ConditionallyConvertMethodNameToCamelCase(name);
            name = element.CamelCaseFromAttribute(name);
            name = element.PascalCaseFromAttribute(name);
            if (element.IsGenericMethod)
            {
                if (!(name.Contains("<") || name.Contains(">")))
                {
                    var args = element.GetGenericArguments();
                    var names = args.Select(resolver.ResolveTypeName);
                    name = string.Concat(name, "<", string.Join(",", names), ">");
                }
            }
        }
    }
}