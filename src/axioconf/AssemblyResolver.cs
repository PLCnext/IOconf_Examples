#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright PHOENIX CONTACT Software GmbH
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace axioconf
{
    internal class AssemblyResolver
    {
        private readonly AssemblyResolverOptions options;
        private bool isRegistered = false;

        public AssemblyResolver(AssemblyResolverOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Register()
        {
            if (!isRegistered)
            {
                if (options.LookupDirectories?.Length > 0)
                {
                    AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                    isRegistered = true;
                }
            }
        }
        public void Unregister()
        {
            if (isRegistered)
            {
                AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
                isRegistered = false;
            }
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Check if the requested assembly is part of the loaded assemblies
            Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }

            // This resolver is called when an loaded control tries to load a generated XmlSerializer - We need to discard it.
            // http://connect.microsoft.com/VisualStudio/feedback/details/88566/bindingfailure-an-assembly-failed-to-load-while-using-xmlserialization

            AssemblyName assemblyName = new AssemblyName(args.Name);

            if (assemblyName.Name.EndsWith(".xmlserializers", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // http://stackoverflow.com/questions/4368201/appdomain-currentdomain-assemblyresolve-asking-for-a-appname-resources-assembl

            if (assemblyName.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string assembly = null;

            if (options.Filters != null && options.Filters.Length > 0)
            {
                if (!options.Filters.Any(f => f.IsMatch(assemblyName)))
                {
                    return null;
                }
            }

            // Find the corresponding assembly file
            foreach (string lookupDirectory in options.LookupDirectories)
            {
                assembly = (new[] { "*.dll", "*.exe" }).SelectMany(g => Directory.EnumerateFiles(lookupDirectory, g))
                    .FirstOrDefault(f =>
                    {
                        try
                        {
                            AssemblyName fileAssemblyName = AssemblyName.GetAssemblyName(f);

                            return assemblyName.Name.Equals(fileAssemblyName.Name, StringComparison.OrdinalIgnoreCase);
                        }
                        catch (BadImageFormatException) { return false; /* Bypass assembly is not a .net exe */ }
                        catch (Exception ex) { throw new ApplicationException("Error loading assembly " + f, ex); }
                    });

                if (assembly != null)
                {
                    return Assembly.LoadFrom(assembly);
                }
            }

            return null;
        }
    }
}