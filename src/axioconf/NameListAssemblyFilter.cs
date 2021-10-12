#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright PHOENIX CONTACT Software GmbH
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace axioconf
{
    internal class NameListAssemblyFilter : AssemblyFilter
    {
        private readonly HashSet<string> assemblyNames;
        private readonly bool prefixMode;

        internal NameListAssemblyFilter(HashSet<string> assemblyNames, bool prefixMode)
        {
            this.assemblyNames = assemblyNames ?? throw new ArgumentNullException(nameof(assemblyNames));
            this.prefixMode = prefixMode;
        }

        public override bool IsMatch(AssemblyName assemblyName)
        {
            if (assemblyName is null)
            {
                throw new ArgumentNullException(nameof(assemblyName));
            }

            if (prefixMode)
            {
                foreach (string assemblyNamePrefix in assemblyNames)
                {
                    if (assemblyName.Name.StartsWith(assemblyNamePrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return assemblyNames.Contains(assemblyName.Name);
            }
        }
    }
}