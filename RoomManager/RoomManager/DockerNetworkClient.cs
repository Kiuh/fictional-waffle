using Docker.DotNet;
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

		private const string DOCKER_HOST = "http://127.0.0.1";
		private const string DOCKER_DAEMON_URI = "tcp://127.0.0.1:2375";
		private const string IMAGE_NAME = "fictional-waffle-authorization:latest";

		private static DockerClient client;

		static DockerNetworkClient()
		{
			client = new DockerClientConfiguration(
					new Uri(DOCKER_DAEMON_URI))
				.CreateClient();
		}

		public static bool TryDeployContainer(string containerName)
		{
			try {
				var id = client.Containers.CreateContainerAsync(
					new CreateContainerParameters() { Image = IMAGE_NAME, Name = containerName, }
				).Result.ID;

				if (id == null)
				{
					return false;
				}

				var started = client.Containers.StartContainerAsync(id, new ContainerStartParameters()).Result;
				return started;
			}
			catch
			{
				return false;
			}
		}

		public static bool TryUndeployContainer(string name)
		{
			var containers = client.Containers.ListContainersAsync(new ContainersListParameters() { All = true }).Result;
			string? id = null;
			foreach (var container in containers)
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

			var exception = client.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters()).Exception;
			return exception == null;
		}

		public static List<ContainerInfo> GetContainersInfo()
		{
			var containers = client.Containers.ListContainersAsync(new ContainersListParameters()).Result;
			var infos = new List<ContainerInfo>();
			foreach (var container in containers)
			{
				var ports = container.Ports;

				var port = ports.First(p => p.PublicPort != 0);
				if (port == null)
				{
					continue;
				}

				var info = new ContainerInfo
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
