using AuthorizationApi;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AdminClient.Authorization.Login
{
    public partial class LoginModel: ObservableObject
	{
		[ObservableProperty]
		private string login = "";

		[ObservableProperty]
		private string password = "";

		[RelayCommand]
		public async void PerformLogin()
		{
			var nonce = Random.Shared.Next().ToString();
			var signature = Cryptography.GetLoginSignature(Login, Password, nonce);

			var login_data = new LoginDto { Nonce = nonce, Signature = signature, };
			var res = await NetworkClient.Login(login_data);
		}
    }
}
