/* 
* Copyright (c) 2012 Jaco Adriaansen
* This code is distributed under the MIT (for details please see license.txt)
*/


using System;
namespace Raptorious.SharpMt940Lib.Mt940Format
{
    /// <summary>
    /// IMt940Format implementation for MigrosBank.
    /// </summary>
    /// <see cref="IMt940Format"></see>
    public class MigrosBank : IMt940Format
    {
        /// <summary>
        /// Header property
        /// </summary>
        /// <value>Miros Bank defines it's header as such:
        /// [newline]
        /// </value>
        public Separator Header
        {
            get;
            private set;
        }

        /// <summary>
        /// Trailer
        /// </summary>
        /// <value>
        /// Migros Bank defines it's trailer as [newline]
        /// </value>
        /// 
        public Separator Trailer
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public MigrosBank()
        {
            Header = new Separator("");
            Trailer = null;
        }
    }
}