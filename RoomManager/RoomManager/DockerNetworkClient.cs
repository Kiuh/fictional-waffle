﻿using Docker.DotNet;
using Docker.DotNet.Models;

namespace RoomManager
{
    internal static class DockerNetworkClient
    {
        public struct ContainerInfo
        {
            public string Name;
            public DateTime DeployedAt;
            public Uri Uri;
        }

        private static string DOCKER_HOST = "http://127.0.0.1";
        private static string DOCKER_DAEMON_URI = "tcp://127.0.0.1:2375";
        private static string IMAGE_NAME = "fictional-waffle-authorization:latest";

        private static readonly DockerClient client;

        static DockerNetworkClient()
        {
            if(Environment.GetEnvironmentVariable("DOCKER_HOST") != null)
            {
                DOCKER_HOST = Environment.GetEnvironmentVariable("DOCKER_HOST");
            }

            if (Environment.GetEnvironmentVariable("DOCKER_DAEMON_URI") != null)
            {
                DOCKER_DAEMON_URI = Environment.GetEnvironmentVariable("DOCKER_DAEMON_URI");
            }

            if (Environment.GetEnvironmentVariable("IMAGE_NAME") != null)
            {
                IMAGE_NAME = Environment.GetEnvironmentVariable("IMAGE_NAME");
            }

            client = new DockerClientConfiguration(new Uri(DOCKER_DAEMON_URI)).CreateClient();
        }

        public static bool TryDeployContainer(string containerName)
        {
            try
            {
                string id = client.Containers
                    .CreateContainerAsync(
                        new CreateContainerParameters()
                        {
                            Image = IMAGE_NAME,
                            Name = containerName,
                        }
                    )
                    .Result.ID;

                if (id == null)
                {
                    return false;
                }

                bool started = client.Containers
                    .StartContainerAsync(id, new ContainerStartParameters())
                    .Result;
                return started;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryUndeployContainer(string name)
        {
            IList<ContainerListResponse> containers = client.Containers
                .ListContainersAsync(new ContainersListParameters() { All = true })
                .Result;
            string? id = null;
            foreach (ContainerListResponse? container in containers)
            {
                if (container.Names[0] == "/" + name)
                {
                    id = container.ID;
                    break;
                }
            }

            if (id == null)
            {
                return false;
            }

            AggregateException? exception = client.Containers
                .RemoveContainerAsync(id, new ContainerRemoveParameters())
                .Exception;
            return exception == null;
        }

        public static List<ContainerInfo> GetContainersInfo()
        {
            IList<ContainerListResponse> containers = client.Containers
                .ListContainersAsync(new ContainersListParameters())
                .Result;
            List<ContainerInfo> infos = new();
            foreach (ContainerListResponse? container in containers)
            {
                IList<Port> ports = container.Ports;

                Port? port = ports.FirstOrDefault(p => p.PublicPort != 0 && p.PublicPort > 50000);
                if (port == null)
                {
                    continue;
                }

                ContainerInfo info =
                    new()
                    {
                        DeployedAt = container.Created,
                        Name = container.Names[0],
                        Uri = new Uri(DOCKER_HOST + ":" + port.PublicPort),
                    };

                infos.Add(info);
            }

            return infos;
        }
    }
}
