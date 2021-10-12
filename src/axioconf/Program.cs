#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright PHOENIX CONTACT Software GmbH
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Ade.Components;
using Ade.Fdcml.Core;
using Ade.Network.Configuration;
using Ade.Network.Configuration.Compiler;
using Ade.Network.Configuration.Project;
using Ade.Network.Configuration.Topologies;
using Ade.Network.Configuration.TypeSystem;
using static System.Console;
using System.IO;


namespace axioconf
{
    class Program : IDisposable
    {

        private const string binariesDirectory = @"";
        private const string ApplicationName = "axioconf.exe";

        public struct Information
        {
            public string Number;
            public string DeviceName;
            public string Version;
        }

        ///<summary>Saves command-line arguments. The data can then be parsed by the ActivityLineParser class.</summary>
        private readonly string[] arguments;

        ///<summary>Saves ComponentFramework (Main class) for managing PLCnext IOconf Interface components.</summary>
        private readonly ComponentFramework componentFramework = new ComponentFramework($@"{binariesDirectory}{ApplicationName}.config", $@"{binariesDirectory}{ApplicationName}.merged.config", $@"{binariesDirectory}{ApplicationName}.user.config");

        /// <summary>
        /// Initializes a new instance of a PLCnext IOconf Interface program class.
        /// </summary>
        public Program(string[] arguments)
        {
            this.arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        /// <summary> Gets or sets a module which can execute operations of the PLCnext IOconf Interface application startup and shutdown.</summary>
        [ImportMany]
#pragma warning disable CA1819 // Properties should not return arrays
        public IModule[] StartupModules { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary> Gets or sets the main network configuration interface.</summary>
        [Import]
        [CLSCompliant(false)]
        public INetCI NetCIRoot { get; set; }

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine();
                Console.WriteLine("Error: The application requires as parameter a configuration file path e.g. 'axioconf.exe C:/IOConf/axioconf/TestRelease.txt' ");
                return 0;
            }
           AssemblyResolverOptions assemblyResolverOptions =
           new AssemblyResolverOptions
             {
                Filters = new AssemblyFilter[] {
                    AssemblyFilter.FromConfig($@"{binariesDirectory}{ApplicationName}.config"),
                    AssemblyFilter.FromListOfPrefixes("Microsoft", "System.", "Ade.", "CommandLine")
                },
                LookupDirectories = new string[] {
                    binariesDirectory,
                }
           };

            AssemblyResolver assemblyResolver = new AssemblyResolver(assemblyResolverOptions);
            try
            {
                //assemblyResolver.Register();

                using (Program program = new Program(args))
                {
                    return program.Run();
                }
            }
            finally
            {
                assemblyResolver.Unregister();
            }
        }

        /// <summary>
        ///  Runs an asynchronous operation that can return a value
        /// </summary>
        /// <returns>Zero if the specified operation is successfully executed, otherwise, ErrorCode None zero numeric value</returns>
        public int Run()
        {
            this.Startup();

            // Start CommandLine Activities
            var result = CreateConfiguration(this.arguments);
            // End CommandLine Activities

            GC.Collect();

            //ReadLine();

            this.Shutdown();

            return result;
        }


        private int CreateConfiguration(string[] args)
        {
            
            INetCI netCI = this.NetCIRoot;

            Uri projectUri = new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "fsu.ioconfx"), UriKind.RelativeOrAbsolute);

            netCI.CreateNew(projectUri);

            Uri controllerLibUri = new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "Axiocontrol.pcwlx"), UriKind.RelativeOrAbsolute);
            IImport controllerLib = netCI.Import(controllerLibUri);



            ///////////////////////////////
            //long memoryBefore = GC.GetTotalMemory(true);

            List<string> fileContents = new List<string>();
            List<string> deviceList = new List<string>();
            List<Information> infos = new List<Information>();
            string axioConfigPath = args[0];

            //string axioConfigPath = "C:/IOConf/axioconf/TestRelease.txt";

            if (File.Exists(axioConfigPath))
            {
                Console.WriteLine();
                Console.WriteLine("The displayed data are read out from the configuration file: " + axioConfigPath);

                StreamReader streamReader = new StreamReader(axioConfigPath);

                while (!streamReader.EndOfStream)
                {
                    fileContents.Add(streamReader.ReadLine());
                }

                streamReader.Close();

                for (int i = 0; i < fileContents.Count; i++)
                {
                    Information information = new Information();
                    string[] str = new string[3];
                    str = fileContents[i].Split(';');

                    information.Number = str[0];
                    information.DeviceName = str[1];
                    information.Version = str[2];

                    infos.Add(information);

                    Console.WriteLine();
                    Console.WriteLine("Number: " + information.Number + " | DeviceName: " + information.DeviceName + " | Version: " + information.Version);
                }

                Console.WriteLine();
                Console.WriteLine("Press any Key to start the Axioline Configuration ... ");
                Console.ReadLine();

                #region Controller stuff

                string ControllerName = infos[0].DeviceName;
                string ControllerVersion = infos[0].Version;

                IDevice controller = netCI
                    .GetConnectables(Specification.IsIdentifiedBy(_ => _.ProductName == ControllerName && _.Version == ControllerVersion))
                    .Single();
                Console.WriteLine("Connect: " + "Number: " + 0 + " | DeviceName: " + ControllerName + " | Version: " + ControllerVersion);
                ISpecification<ITopology> topologySpec = Specification.IsTopology(Layers.Application);
                IEnumerable<ITopology> layer7Topologies = netCI.GetConnectables(topologySpec);
                ITopology layerSevenTopology = layer7Topologies.Single();
                layerSevenTopology.Connect(controller);

                #endregion Controller stuff

                #region Axioline stuff

                for (int i = 1; i < infos.Count; i++)
                {
                    string DeviceName = infos[i].DeviceName;
                    string DeviceVersion = infos[i].Version;

                    IDevice device = netCI.GetConnectables(Specification.IsIdentifiedBy(_ => _.ProductName == DeviceName))
                                   .First();
                    Console.WriteLine("Connect: " + "Number: " + i + " | DeviceName: " + DeviceName + " | Version: " + DeviceVersion);
                    controller.Connect(device);
                }

                #endregion Axioline stuff

                CompileOptions options =
                    new CompileOptions
                    {
                        OutputPath = new Uri(Environment.CurrentDirectory)
                    };

                IResult[] results = netCI.Compile(options).ToArray();

                if (results.Length != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine(results[0].Severity + ": " + results[0].Id + ": " + results[0].Description);
                    //Console.WriteLine("Result: " + results[0]);
                }

                netCI.Update();
                netCI.Close();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("The file Path: " + axioConfigPath + " is not exists -> Create a new configuration file..");
                Console.WriteLine();
                Console.WriteLine("Press any Key to save the device information of Axiocontrol.pcwlx into: " + axioConfigPath);
                Console.ReadLine();

                IDevice[] deviceContent = netCI.GetConnectables(Specification.IsIdentifiedBy(_ => _.Vendor == "Phoenix Contact")).ToArray();

                if (deviceContent.Count() > 0)
                {
                    Console.WriteLine("Number of founded devices: " + deviceContent.Count());

                    StreamWriter streamWriter = null;
                    try
                    {
                        streamWriter = new StreamWriter(axioConfigPath);

                        for (int i = 0; i < deviceContent.Count(); i++)
                        {
                            deviceList.Add(deviceContent.GetValue(i).ToString());
                            //Console.WriteLine("Nr: " + i +" " + deviceContent.GetValue(i));

                            deviceList[i] = deviceList[i].Replace("Lazy ", "").Replace(" v ", ";");
                            Console.WriteLine(i + ";" + deviceList[i]);
                            streamWriter.WriteLine(i + ";" + deviceList[i]);
                        }

                        streamWriter.Close();

                        Console.WriteLine("Please edit the axioline configuration file: " + axioConfigPath + " and restart this application!");
                    }

                    catch (IOException e)
                    {
                        Console.WriteLine("IOException");
                        Console.WriteLine(
                           "{0}: {1}",
                            e.GetType().Name, e.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception");
                        Console.WriteLine(
                            "{0}: {1}",
                            ex.GetType().Name, ex.Message);
                    }
                    finally
                    {
                        if (streamWriter != null)
                            streamWriter.Dispose();
                    }
                }
                else
                {
                    Console.WriteLine("Error: Number of founded devices: " + deviceContent.Count() + " ,please verify the Axiocontrol.pcwlx library!");
                }
            }

            //long memoryAfter = GC.GetTotalMemory(false);

            //Console.WriteLine();
            //Console.WriteLine("Memory Used for reading Axioline Configuration File = \t" +
            //string.Format(((memoryAfter - memoryBefore) / 1000).ToString(), "n") + "kb" + "\n" + "\n");
            
            return 0;
        }

        /// <summary>
        /// Initializes ComponentFramework and module Events.
        /// </summary>
        private void Startup()
        {
            this.componentFramework.UnexpectedException += this.OnReportUnexpectedException;
            this.componentFramework.StartupCompleted += this.OnComponentFrameworkStartupCompleted;
            this.componentFramework.Init();

            ICompositionManager compositionManager = this.componentFramework.GetService<ICompositionManager>();
            try
            {
                compositionManager.ComposePart(this);
            }
            catch (CompositionException compositionException)
            {
                foreach (Exception rootCause in compositionException.RootCauses)
                {
                    Console.WriteLine("***");
                    Console.WriteLine(rootCause);
                }
            }

            if (this.StartupModules != null)
            {
                foreach (IModule startupModule in this.StartupModules)
                {
                    startupModule.OnStartup(StartupPhase.Initialize);
                    startupModule.OnStartup(StartupPhase.Startup);
                }
            }
        }

        /// <summary>
        /// Prepare Module Shutdown Events.
        /// </summary>
        private void Shutdown()
        {
            if (this.StartupModules != null)
            {
                foreach (IModule startupModule in this.StartupModules)
                {
                    startupModule.OnShutdown(ShutdownPhase.Shutdown);
                }
            }
        }
        
        /// <summary>
        /// Occurs after all components are initialized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentFrameworkStartupCompleted(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Occurs after an unexpected component Framework exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReportUnexpectedException(object sender, UnexpectedExceptionEventArgs e)
        {
            WriteLine(e.Message);
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                this.componentFramework?.Dispose();
            }

            this.disposed = true;
        }
    }
}
