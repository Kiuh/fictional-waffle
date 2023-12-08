using AuthorizationApi;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net;

namespace AdminClient.Authorization.Login
{
    public partial class LoginModel : ObservableObject
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
        public async Task PerformLogin()
        {
            string nonce = Random.Shared.Next().ToString();
            string signature = Cryptography.GetLoginSignature(Login, Password, nonce);

            LoginDto login_data = new() { Nonce = nonce, Signature = signature, };
            HttpStatusCode res = await AuthorizationClient.Login(login_data, Login, Password);

            if (res != HttpStatusCode.OK)
            {
                LoginFailed = true;
                return;
            }

            Application.Current.MainPage = new Management.ManagementView();
        }
    }
}
