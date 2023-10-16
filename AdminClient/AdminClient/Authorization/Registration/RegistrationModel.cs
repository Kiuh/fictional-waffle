using AuthorizationApi;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AdminClient.Authorization.Registration
{
    public partial class RegistrationModel: ObservableObject
    {
		[ObservableProperty]
		private string login = "";

		[ObservableProperty]
		private string password = "";

		[ObservableProperty]
		private string email = "";

		[RelayCommand]
		public async void Register()
		{
			var pubkey = await NetworkClient.GetPubkey();
			Cryptography.SetPubkey(pubkey);

			var nonce = Random.Shared.Next().ToString();

			var encrypted_nonce_with_email = Cryptography.EncryptString(nonce + Email);

			var encrypted_hashed_password = Cryptography.HashString(Password);
			encrypted_hashed_password = Cryptography.EncryptString(encrypted_hashed_password);

			var registration_data = new RegistrationDto()
			{
				Login = Login,
				Nonce = nonce,
				EncryptedHashedPassword = encrypted_hashed_password,
				EncryptedNonceWithEmail = encrypted_nonce_with_email,
			};

			var res = await NetworkClient.Register(registration_data);
		}
	}
}
