﻿using System;
using System.Xml.Serialization;

namespace Leem.Testify.Model
{
    /// <summary>
    ///     A reference to a tracked method
    /// </summary>
    public class TrackedMethodRef
    {
        /// <summary>
        ///     unique id assigned
        /// </summary>
        [XmlAttribute("uid")]
        public UInt32 UniqueId { get; set; }

        // visit count
        [XmlAttribute("vc")]
        public int VisitCount { get; set; }
    }
}