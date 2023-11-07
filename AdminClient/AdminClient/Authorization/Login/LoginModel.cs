using System.Net;
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

		[ObservableProperty]
		private bool loginFailed = false;

		partial void OnLoginChanged(string value)
		{
			LoginFailed = false;
		}

		partial void OnPasswordChanged(string value)
		{
			LoginFailed = false;
        }

        [RelayCommand]
		public async void PerformLogin()
		{
			var nonce = Random.Shared.Next().ToString();
			var signature = Cryptography.GetLoginSignature(Login, Password, nonce);

			var login_data = new LoginDto { Nonce = nonce, Signature = signature, };
			var res = await AuthorizationClient.Login(login_data);

			if (res != HttpStatusCode.OK)
			{
				LoginFailed = true;
			}
		}
    }
}
