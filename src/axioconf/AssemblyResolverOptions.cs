#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright PHOENIX CONTACT Software GmbH
//
///////////////////////////////////////////////////////////////////////////////
#endregion

namespace axioconf
{
    internal class AssemblyResolverOptions
    {
        public string[] LookupDirectories { get; set; }

        public AssemblyFilter[] Filters { get; set; }
    }
}