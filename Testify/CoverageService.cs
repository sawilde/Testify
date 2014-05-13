﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using EnvDTE;
using Leem.Testify.Model;
using System.IO;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Classification;
using log4net;
using Leem.Testify.Poco;

namespace Leem.Testify
{
    
    public  class CoverageService : ICoverageService
    {
        private IList<CodeElement> _classes;
        private IList<CodeElement> _methods;
        private ITextDocument _document;
        private string _solutionDirectory;
        private static CoverageService instance;
        private ILog Log = LogManager.GetLogger(typeof(CoverageService));
        private DTE _dte; 
        private string _solutionName;
        private ITestifyQueries _queries;


        public static CoverageService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CoverageService();

                    instance._classes = new List<CodeElement>();

                    instance._methods = new List<CodeElement>();
                }

                return instance;
            }
        }


        public DTE DTE { set { _dte = value; } }

        public ITestifyQueries Queries { get; set; }

        public string SolutionName
        {
            get { return _solutionName; }
            set
            {
                _solutionName = value;

                _solutionDirectory = System.IO.Path.GetDirectoryName(_solutionName);
            }
        }
        public ITextDocument Document { get { return _document; } set { _document = value; } }

        public List<LineCoverageInfo> CoveredLines { get; set; }

        public IList<LineCoverageInfo> GetCoveredLinesFromCoverageSession(CoverageSession codeCoverage, string projectAssemblyName)
        {

            var coveredLines = new List<LineCoverageInfo>();

            var sessionModules = codeCoverage.Modules;

            Log.DebugFormat("GetCoveredLines for project: {0}", projectAssemblyName);
            Log.DebugFormat("Summary.NumSequencePoints: {0}", codeCoverage.Summary.NumSequencePoints);
            Log.DebugFormat("Summary.SequenceCoverage: {0}", codeCoverage.Summary.SequenceCoverage);
            Log.DebugFormat("Summary.VisitedSequencePoints: {0}", codeCoverage.Summary.VisitedSequencePoints);
            Log.DebugFormat("Summary.SequenceCoverage: {0}", codeCoverage.Summary.SequenceCoverage);
            Log.DebugFormat("Number of Modules: {0}", sessionModules.Count());

            foreach (var sessionModule in sessionModules)
            {
                Log.DebugFormat("Module Name: {0}", sessionModule.ModuleName);
            }

            var module = sessionModules.FirstOrDefault(x => x.ModuleName.Equals(projectAssemblyName));

            var tests = sessionModules.Where(x => x.TrackedMethods.Count() > 0).SelectMany(y => y.TrackedMethods);
            
            if (module != null)
            {
                var classes = module.Classes;

                Log.DebugFormat("First Module Name: {0}", module.ModuleName);
                Log.DebugFormat("Number of Classes: {0}", classes.Count());

                foreach (var codeClass in classes)
                {
                    var methods = codeClass.Methods;

                    foreach (var method in methods)
                    {
                        if (!method.Name.Contains("__"))
                        {
                            ProcessSequencePoints(coveredLines, module, tests, codeClass, method);
                        }
                        //else
                        //{
                        //    Log.DebugFormat("Skipping  Class: {0}   Method {1}: ",codeClass.FullName, method.Name);
                        //}
                        
                    }
                }
            }

            return coveredLines;
        }

        private void ProcessSequencePoints(List<LineCoverageInfo> coveredLines, Module module, IEnumerable<Model.TrackedMethod> tests, Class modelClass, Method method)
        {


            var sequencePoints = method.SequencePoints;
            foreach (var sequencePoint in sequencePoints)
            {
                var coveredLine = new LineCoverageInfo
                {
                    IsCode = true,
                    LineNumber = sequencePoint.StartLine,
                    IsCovered = (sequencePoint.VisitCount > 0),
                    ModuleName = module.ModuleName,
                    ClassName = modelClass.FullName,
                    MethodName = method.Name,
                    UnitTests = new List<UnitTest>()
                };

                if (tests.Any())
                {
                    var coveringTests = new Poco.TrackedMethod();

                    foreach (var trackedMethodRef in sequencePoint.TrackedMethodRefs)
                    {
                        var trackedMethod = tests.FirstOrDefault(y => y.UniqueId.Equals(trackedMethodRef.UniqueId));

                        coveredLine.IsCode = true;

                        coveredLine.IsCovered = (sequencePoint.VisitCount > 0);

                        coveredLine.TrackedMethods.Add(new Poco.TrackedMethod
                        {
                            UniqueId = (int)trackedMethod.UniqueId,
                            UnitTestId = trackedMethod.UnitTestId,
                            Strategy = trackedMethod.Strategy,
                            Name = trackedMethod.Name,
                            MetadataToken = trackedMethod.MetadataToken
                        });

                    }

                }

                coveredLines.Add(coveredLine);
            }
        }



        public List<LineCoverageInfo> GetRetestedLinesFromCoverageSession(CoverageSession coverageSession, string projectAssemblyName, List<int> uniqueIds)
        {

            /// todo need to figure out how to remove a CoveredLine if an Edit has made it uncovered, 
            /// without removing all lines that were not covered by the subset of tests that were run
            /// One option: check all previously covered lines from this group of tests with the current covered lines list from this group.
            /// 
            //todo Handle case where a line that was "Code" and was covered is now not "Code"
            var coveredLines = new List<LineCoverageInfo>();
            
            var sessionModules = coverageSession.Modules;
            var module = sessionModules.FirstOrDefault(x => x.ModuleName.Equals(projectAssemblyName));
            var tests = sessionModules.Where(x => x.TrackedMethods.Count() > 0).SelectMany(y => y.TrackedMethods);

            if (module != null)
            {
                var codeModule = new Poco.CodeModule
                {
                    Name = module.FullName,
                    Summary = new Poco.Summary(module.Summary)
                };

                var classes = module.Classes;

                Log.DebugFormat("First Module Name: {0}", module.ModuleName);
                Log.DebugFormat("Number of Classes: {0}", classes.Count());

                foreach (var moduleClass in classes)
                {
                    if (!moduleClass.FullName.Contains("_"))
                    {
                        var codeClass = new Poco.CodeClass
                        {
                            Name = moduleClass.FullName,
                            Summary = new Poco.Summary(moduleClass.Summary)
                        };

                        var methods = moduleClass.Methods;

                        foreach (var method in methods)
                        {
                            if (!method.Name.Contains("_"))
                            {
                                var codeMethod = new Poco.CodeMethod
                                {
                                    Name = method.Name,
                                    Summary = new Poco.Summary(method.Summary)
                                };

                                var sequencePoints = method.SequencePoints;
                                foreach (var sequencePoint in sequencePoints)
                                {
                                    if (tests.Any())
                                    {
                                        var coveringTests = new List<Poco.TrackedMethod>();

                                        foreach (var trackedMethodRef in sequencePoint.TrackedMethodRefs)
                                        {
                                            var coveredLine = new LineCoverageInfo
                                            {
                                                IsCode = true,
                                                LineNumber = sequencePoint.StartLine,
                                                IsCovered = (sequencePoint.VisitCount > 0),
                                                Module = codeModule,
                                                Class = codeClass,
                                                Method = codeMethod
                                            };

                                            var testsThatCoverLine = tests.Where(y => y.UniqueId.Equals(trackedMethodRef.UniqueId));

                                            foreach (var test in testsThatCoverLine)
                                            {
                                                coveredLine.IsCode = true;

                                                coveredLine.IsCovered = (sequencePoint.VisitCount > 0);

                                                coveredLine.TrackedMethods.Add(new Poco.TrackedMethod
                                                {
                                                    UniqueId = (int)test.UniqueId,
                                                    MetadataToken = method.MetadataToken,
                                                    Strategy = test.Strategy,
                                                    Name = test.Name
                                                });
                                            }

                                            coveredLines.Add(coveredLine);
                                        }

                                    }

                                }
                            }

                        } 
                    }
                }
            }

            return coveredLines;
        }
    }
}
