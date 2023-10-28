using System.Security.Cryptography;
using System.Text;
using CryptoNet;

namespace AuthorizationApi
{
	public static class Cryptography
	{
		private static ICryptoNet rsa;

		public static void SetPubkey(string pubkey)
		{
			rsa = new CryptoNetRsa(pubkey);
        }

		public static string EncryptString(string input)
		{
			byte[] encrypted = rsa.EncryptFromString(input);
			return Convert.ToBase64String(encrypted);
		}

		public static string HashString(string input)
		{
			byte[] hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
			StringBuilder sBuilder = new();
			for (int i = 0; i < hashedBytes.Length; i++)
			{
				_ = sBuilder.Append(hashedBytes[i].ToString("x2"));
			}
			return sBuilder.ToString();
		}

		public static string GetLoginSignature(string login, string password, string nonce)
		{
			return HashString(login + nonce + HashString(password));
		}
	}
}
