﻿//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Leem.Testify.Model
{
    /// <summary>
    /// The details of a module
    /// </summary>
    public class Module : SkippedEntity
    {
        /// <summary>
        /// simple constructor
        /// </summary>
        public Module()
        {
            Aliases = new List<string>();
            Summary = new Summary();
        }

        /// <summary>
        /// A Summary of results for a module
        /// </summary>
        public Summary Summary { get; set; }

        /// <summary>
        /// The full path name to the module
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// A list of aliases
        /// </summary>
        [XmlIgnore]
        public IList<string> Aliases { get; private set; }

        /// <summary>
        /// The name of the module
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// The files that make up the module
        /// </summary>
        public List<File> Files { get; set; }

        /// <summary>
        /// The classes that make up the module
        /// </summary>
        public List<Class> Classes { get; set; }

        /// <summary>
        /// Methods that are being tracked i.e. test methods
        /// </summary>
        public List<TrackedMethod> TrackedMethods { get; set; }

        /// <summary>
        /// A hash of the file used to group them together (especially when running against mstest)
        /// </summary>
        [XmlAttribute("hash")]
        public string ModuleHash { get; set; }

        public override void MarkAsSkipped(SkippedMethod reason)
        {
            SkippedDueTo = reason;
        }
    }
}