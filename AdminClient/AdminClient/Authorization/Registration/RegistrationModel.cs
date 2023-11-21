using AuthorizationApi;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AdminClient.Authorization.Registration
{
    public partial class RegistrationModel : ObservableObject
    {
        [ObservableProperty]
        private string login = "";

        [ObservableProperty]
        private string password = "";

        [ObservableProperty]
        private string email = "";

        [RelayCommand]
        public async Task Register()
        {
            string pubkey = await AuthorizationClient.GetPubkey();
            Cryptography.SetPubkey(pubkey);

            string nonce = Random.Shared.Next().ToString();

            string encrypted_nonce_with_email = Cryptography.EncryptString(nonce + Email);

            string encrypted_hashed_password = Cryptography.HashString(Password);
            encrypted_hashed_password = Cryptography.EncryptString(encrypted_hashed_password);

            RegistrationDto registration_data =
                new()
                {
                    Login = Login,
                    Nonce = nonce,
                    EncryptedHashedPassword = encrypted_hashed_password,
                    EncryptedNonceWithEmail = encrypted_nonce_with_email,
                };
            _ = await AuthorizationClient.Register(registration_data);
        }
    }
}
