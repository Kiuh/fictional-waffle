namespace AuthorizationApi
{
	public class ErrorPageDto
	{
		public string StatusCode { get; set; } = "500";
		public string Title { get; set; } = "Internal Server Error";
		public List<string> Labels { get; set; } = new();

		public ErrorPageDto(string label)
		{
			Labels.Add(label);
		}
	}

	public sealed class ForgotPasswordDto
	{
		public required string EncryptedNonceWithEmail { get; set; }
		public required string Nonce { get; set; }
	}

	public sealed class LoginDto
	{
		public required string Signature { get; set; }
		public required string Nonce { get; set; }
	}

	public sealed class RecoverPasswordDto
	{
		public required int AccessCode { get; set; }
		public required string EncryptedHashedPassword { get; set; }
	}

	public sealed class RegistrationDto
	{
		public required string Login { get; set; }
		public required string EncryptedNonceWithEmail { get; set; }
		public required string Nonce { get; set; }
		public required string EncryptedHashedPassword { get; set; }
	}

	public sealed class ResendVerificationDto
	{
		public required string EncryptedNonceWithEmail { get; set; }
		public required string Nonce { get; set; }
	}
}