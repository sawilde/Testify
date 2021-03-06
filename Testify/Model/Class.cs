﻿//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using System.Collections.Generic;

namespace Leem.Testify.Model
{
    /// <summary>
    ///     An entity that contains methods
    /// </summary>
    public class Class : SkippedEntity
    {
        public Class()
        {
            Methods = new List<Method>();
            Summary = new Summary();
        }

        /// <summary>
        ///     A Summary of results for a class
        /// </summary>
        public Summary Summary { get; set; }

        /// <summary>
        ///     The full name of the class
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        ///     A reference to a file in the file collection (used to help visualisation)
        /// </summary>
        public FileRef FileRef { get; set; }

        public List<File> Files { get; set; }

        /// <summary>
        ///     A list of methods that make up the class
        /// </summary>
        public List<Method> Methods { get; set; }

        /// <summary>
        ///     Control serialization of the Summary  object
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeSummary()
        {
            return !ShouldSerializeSkippedDueTo();
        }

        public override void MarkAsSkipped(SkippedMethod reason)
        {
            SkippedDueTo = reason;
        }
    }
}