﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Reinforced.Typings.Attributes;
using Reinforced.Typings.Fluent;
using Reinforced.Typings.Visitors;
using Reinforced.Typings.Xmldoc;

namespace Reinforced.Typings
{
    /// <summary>
    ///     Facade for final TypeScript export. This class supplies assemblies names or assemblies itself as parameter and
    ///     exports resulting TypeScript file to file or to string
    /// </summary>
    public class TsExporter : MarshalByRefObject
    {
        private readonly FilesOperations _fileOps;
        private readonly StringBuilder _referenceBuilder = new StringBuilder();
        private readonly ExportContext _context;
        private List<Type> _allTypes;
        private HashSet<Type> _allTypesHash;
        private ConfigurationRepository _configurationRepository;
        private bool _isAnalyzed;
        private List<string> _additionalImports = new List<string>();

        #region Constructors

        /// <summary>
        ///     Constructs new instance of TypeScript exporter
        /// </summary>
        /// <param name="context"></param>
        public TsExporter(ExportContext context)
        {
            _context = context;
            _fileOps = new FilesOperations(context);
        }

        #endregion

        private void ExtractReferences()
        {
            if (_isAnalyzed) return;
            _context.Documentation =
                new DocumentationManager(_context.GenerateDocumentation ? _context.DocumentationFilePath : null, _context.Warnings);
            var fluentConfigurationPresents = _context.ConfigurationMethod != null;
            if (fluentConfigurationPresents)
            {
                var configurationBuilder = new ConfigurationBuilder();
                _context.ConfigurationMethod(configurationBuilder);
                _configurationRepository = configurationBuilder.Build();
                ConfigurationRepository.Instance = _configurationRepository;

                foreach (var additionalDocumentationPath in _configurationRepository.AdditionalDocumentationPathes)
                {
                    _context.Documentation.CacheDocumentation(additionalDocumentationPath, _context.Warnings);
                }
            }

            _allTypes = _context.SourceAssemblies
                .SelectMany(c => c.GetTypes().Where(d => d.GetCustomAttribute<TsAttributeBase>(false) != null))
                .Union(ConfigurationRepository.Instance.AttributesForType.Keys).Distinct()
                .ToList();

            _allTypesHash = new HashSet<Type>(_allTypes);

            if (_context.Hierarchical)
            {
                foreach (var type in _allTypesHash)
                {
                    ConfigurationRepository.Instance.AddFileSeparationSettings(type);
                }
            }

            _context.SourceAssemblies.Where(c => c.GetCustomAttributes<TsReferenceAttribute>().Any())
                .SelectMany(c => c.GetCustomAttributes<TsReferenceAttribute>())
                .Select(c => string.Format("/// <reference path=\"{0}\"/>", c.Path))
                .Union(
                    ConfigurationRepository.Instance.References.Select(
                        c => string.Format("/// <reference path=\"{0}\"/>", c)))
                .ToList()
                .ForEach(a => _referenceBuilder.AppendLine(a));

            _context.References = _referenceBuilder.ToString();


            _isAnalyzed = true;
        }

        /// <summary>
        ///     Exports TypeScript source to specified TextWriter according to settings
        /// </summary>
        /// <param name="sw">TextWriter</param>
        /// <param name="tr">TypeResolver object</param>
        /// <param name="types">Types to export</param>
        private void ExportTypes(TextWriter sw, TypeResolver tr, IEnumerable<Type> types = null)
        {
            ExportReferences(sw, types);
            if (types == null) types = _allTypes;
            ExportNamespaces(types, tr, sw);
        }

        private void ExportReferences(TextWriter tw, IEnumerable<Type> types = null)
        {
            WriteWarning(tw);
            tw.WriteLine(_referenceBuilder.ToString());
            if (types != null)
            {
                HashSet<string> pathes = new HashSet<string>();
                foreach (var type in types)
                {
                    var inspected = _fileOps.GenerateInspectedReferences(type, _allTypesHash);
                    if (!string.IsNullOrEmpty(inspected) && !string.IsNullOrWhiteSpace(inspected))
                    {
                        pathes.AddIfNotExists(inspected);
                    }
                }
                foreach (var path in pathes)
                {
                    tw.WriteLine(path);
                }

            }
        }

        /// <summary>
        ///     Exports TypeScript source according to settings
        /// </summary>
        public void Export()
        {
            _fileOps.ClearTempRegistry();
            ExtractReferences();
            var tr = new TypeResolver(_context);
            _context.Lock();

            if (!_context.Hierarchical)
            {
                var file = _fileOps.GetTmpFile(_context.TargetFile);
                using (var fs = File.OpenWrite(file))
                {
                    using (var tw = new StreamWriter(fs))
                    {
                        ExportTypes(tw, tr);
                    }
                }
                if (_additionalImports.Any())
                {
                    AppendAdditionalImports(file);
                }
            }
            else
            {
                var typeFilesMap = _allTypes
                    .GroupBy(c => _fileOps.GetPathForType(c))
                    .ToDictionary(c => c.Key, c => c.AsEnumerable());

                foreach (var kv in typeFilesMap)
                {
                    var path = kv.Key;
                    var tmpFile = _fileOps.GetTmpFile(path);
                    using (var fs = File.OpenWrite(tmpFile))
                    {
                        using (var tw = new StreamWriter(fs))
                        {
                            ExportTypes(tw, tr, kv.Value);
                        }
                    }
                    if (_additionalImports.Any())
                    {
                        AppendAdditionalImports(tmpFile);
                    }
                }
            }

            _context.Unlock();
            _fileOps.DeployTempFiles();
            tr.PrintCacheInfo();
        }

        private void AppendAdditionalImports(string file)
        {
            var sb = new StringBuilder();
            using (var f = File.OpenRead(file))
            {
                using (var sr = new StreamReader(f))
                {
                    string line = sr.ReadLine();
                    while (line != null && !line.StartsWith("export"))
                    {
                        sb.AppendLine(line);
                        line = sr.ReadLine();
                    }
                    if (sr.EndOfStream)
                    {
                        return;
                    }
                    foreach (var import in _additionalImports)
                    {
                        sb.AppendLine(import);
                    }
                    sb.AppendLine()
                        .AppendLine(line)
                        .AppendLine(sr.ReadToEnd());
                }
            }
            using (var f = File.OpenWrite(file))
            {
                using (var sw = new StreamWriter(f))
                {
                    sw.Write(sb.ToString());
                }
            }
            _additionalImports.Clear();
        }

        private void ExportNamespaces(IEnumerable<Type> types, TypeResolver tr, TextWriter tw)
        {
            var gen = tr.GeneratorForNamespace(_context);
            var grp = types.GroupBy(c => c.GetNamespace(true));
            var nsp = grp.Where(g => !string.IsNullOrEmpty(g.Key)) // avoid anonymous types
                .ToDictionary(k => k.Key, v => v.ToList());

            var visitor = _context.ExportPureTypings ? new TypingsExportVisitor(tw, _context) : new TypeScriptExportVisitor(tw, _context);

            foreach (var n in nsp)
            {
                var ns = n.Key;
                if (ns == "-") ns = string.Empty;
                var module = gen.Generate(n.Value, ns, tr);
                visitor.Visit(module);
                _additionalImports = visitor.AdditionalImports;
            }
            tw.Flush();
        }

        private void WriteWarning(TextWriter tw)
        {
            if (_context.WriteWarningComment)
            {
                tw.WriteLine("//     This code was generated by a Reinforced.Typings tool. ");
                tw.WriteLine("//     Changes to this file may cause incorrect behavior and will be lost if");
                tw.WriteLine("//     the code is regenerated.");
                tw.WriteLine();
            }
        }

        /// <summary>
        ///     Exports TypeScript source to string
        /// </summary>
        /// <returns>String containig generated TypeScript source for specified assemblies</returns>
        public string ExportAll()
        {
            _context.Lock();
            ExtractReferences();

            var sb = new StringBuilder();
            var tr = new TypeResolver(_context);
            using (var sw = new StringWriter(sb))
            {
                ExportTypes(sw, tr);
            }
            _context.Unlock();
            return sb.ToString();
        }
    }
}