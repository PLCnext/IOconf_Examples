#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright PHOENIX CONTACT Software GmbH
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace axioconf
{
    internal abstract class AssemblyFilter
    {
        public abstract bool IsMatch(AssemblyName assemblyName);

        public static AssemblyFilter FromConfig(string configurationFile)
        {
            if (configurationFile is null)
            {
                throw new ArgumentNullException(nameof(configurationFile));
            }

            HashSet<string> assemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (File.Exists(configurationFile))
            {
                XDocument document = XDocument.Load(configurationFile);

                foreach (XElement composablePartSource in document.Descendants("ComposablePartSource"))
                {
                    string assemblyName = composablePartSource.Attribute("AssemblyName")?.Value;

                    if (!string.IsNullOrEmpty(assemblyName))
                    {
                        assemblyNames.Add(assemblyName);
                    }
                }
            }

            return new NameListAssemblyFilter(assemblyNames, false);
        }

        public static AssemblyFilter FromListOfPrefixes(params string[] assemblyPrefixes)
        {
            HashSet<string> assemblyNames = new HashSet<string>(assemblyPrefixes, StringComparer.OrdinalIgnoreCase);

            return new NameListAssemblyFilter(assemblyNames, true);
        }
    }
}