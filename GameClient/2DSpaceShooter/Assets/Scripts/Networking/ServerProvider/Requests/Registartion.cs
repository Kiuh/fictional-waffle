using System;
using System.Collections;
using UnityEngine.Networking;

namespace Networking
{
    public partial class ServerProvider
    {
        public class RegistrationOpenData
        {
            public string Login;
            public string Email;
            public string Password;

            public RegistrationOpenData(string login, string email, string password)
            {
                Login = login;
                Email = email;
                Password = password;
            }
        }

        [Serializable]
        public class RegistrationData
        {
            public string Login;
            public string EncryptedNonceWithEmail;
            public string Nonce;
            public string EncryptedHashedPassword;
        }

        public IEnumerator Registration(RegistrationOpenData data, Action<UnityWebRequest> action)
        {
            string nonce = Nonce;
            RegistrationData registrationData =
                new()
                {
                    Login = data.Login,
                    Nonce = nonce,
                    EncryptedNonceWithEmail = EncryptString(data.Email + nonce),
                    EncryptedHashedPassword = EncryptString(HashString(data.Password))
                };

            UnityWebRequest webRequest = requestBuilder.CreateRequest(
                "/Registration",
                HttpMethod.Put,
                registrationData
            );

            yield return webRequest.SendWebRequest();

            action(webRequest);
            webRequest.Dispose();
        }

        public class ResendRegistrationOpenData
        {
            public string Email;

            public ResendRegistrationOpenData(string email)
            {
                Email = email;
            }
        }

        [Serializable]
        public class ResendRegistrationData
        {
            public string EncryptedNonceWithEmail;
            public string Nonce;
        }

        public IEnumerator ResendEmailVerification(
            ResendRegistrationOpenData data,
            Action<UnityWebRequest> action
        )
        {
            string nonce = Nonce;
            ResendRegistrationData resendRegistrationData =
                new()
                {
                    Nonce = nonce,
                    EncryptedNonceWithEmail = EncryptString(data.Email + nonce),
                };

            UnityWebRequest webRequest = requestBuilder.CreateRequest(
                "/ResendRegistration",
                HttpMethod.Post,
                resendRegistrationData
            );

            yield return webRequest.SendWebRequest();

            action(webRequest);
            webRequest.Dispose();
        }
    }
}
