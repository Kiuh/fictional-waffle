namespace RoomManagerApi
{
	public class RoomPartialInfoDto
	{
		public required string Name { get; set; }
		public required int ActiveUsers { get; set; }
		public required int Capacity { get; set; }
	}

    public class RoomInfoDto
	{
		public required string Name { get; set; }
		public required string ContainerName { get; set; }
		public required DateTime DeployedAt { get; set; }
		public required int ActiveUsers { get; set; }
		public required int Capacity { get; set; }
	}

	public class RoomDeployRequestDto
	{
		public required string Name { get; set; }
		public required string ContainerName { get; set; }
		public required int Capacity { get; set; }
    }

	public class RoomDropRequestDto
	{
		public required string Name { get; set; }
	}
}
