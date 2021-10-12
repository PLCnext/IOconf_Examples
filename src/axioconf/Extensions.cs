#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright PHOENIX CONTACT Software GmbH
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Ade.Network.Configuration;
using Ade.Network.Configuration.Settings;
using Ade.Network.Configuration.Topologies;
using Path = System.IO.Path;

namespace axioconf
{
    internal static class Extensions
    {
        public static IEnumerable<IDevice> GetConnectedDevices(this IDevice device)
        {
            if (device is null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            IEnumerable<ICommunicationLayer> applicationLayers = GetApplicationLayers(device);
            foreach (ICommunicationLayer communicationLayer in applicationLayers)
            {
                ITopology communicationTopology = communicationLayer.GetTopology(Specification.IsTopology(Layers.Application));

                if (communicationTopology != null)
                {
                    foreach (IDevice connectedDevice in GetConnectedDevices(communicationTopology))
                    {
                        yield return connectedDevice;
                    }
                }
            }
        }

        public static IEnumerable<IDevice> GetConnectedDevices(this ITopology topology)
        {
            if (topology is null)
            {
                throw new ArgumentNullException(nameof(topology));
            }

            return topology.GetConnectables().OfType<IDevice>();
        }

        private static IEnumerable<ICommunicationLayer> GetApplicationLayers(IDevice device)
        {
            return device.Layers.SelectMany(l => GetApplicationLayers(l));
        }

        private static IEnumerable<ICommunicationLayer> GetApplicationLayers(ICommunicationLayer communicationLayer)
        {
            if (communicationLayer.OsiLayer == Layers.Application)
            {
                yield return communicationLayer;
            }
            else
            {
                foreach (ICommunicationLayer layer in communicationLayer.Layers.SelectMany(l => GetApplicationLayers(l)))
                {
                    yield return layer;
                }
            }
        }

        public static IEnumerable<IParameter> Flatten(this IEnumerable<IParameter> e)
        {
            return e.SelectMany(c => c.Parameters.Flatten()).Concat(e);
        }
        public static Uri ToImportUri(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            else
            {
                string fullPath = Path.GetFullPath(path);
                return new Uri($"{fullPath}", UriKind.Absolute);
            }
        }

        public static Uri ToProjectUri(this string projectPath)
        {
            string projectFullpath = projectPath;
            if (!string.IsNullOrEmpty(projectFullpath) && !System.IO.Path.IsPathRooted(projectFullpath))
            {
                projectFullpath = System.IO.Path.GetFullPath(projectFullpath);
            }

            Uri projectUri = new Uri($"file:///{projectFullpath}");

            return projectUri;
        }
    }
}