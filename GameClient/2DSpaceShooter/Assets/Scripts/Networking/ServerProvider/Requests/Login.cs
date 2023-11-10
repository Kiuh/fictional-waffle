using System;
using System.Collections;
using UnityEngine.Networking;

namespace Networking
{
    public partial class ServerProvider
    {
        [Serializable]
        public class LoginOpenData
        {
            public string Login;
            public string Password;

            public LoginOpenData(string login, string password)
            {
                Login = login;
                Password = password;
            }
        }

        private class LoginData
        {
            public string Signature;
            public string Nonce;
        }

        public IEnumerator Login(LoginOpenData data, Action<UnityWebRequest> action)
        {
            string nonce = Nonce;
            LoginData logInData =
                new()
                {
                    Signature = HashString(data.Login + nonce + HashString(data.Password)),
                    Nonce = nonce
                };

            UnityWebRequest webRequest = requestBuilder.CreateRequest(
                "/Login",
                HttpMethod.Post,
                logInData
            );

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                requestBuilder.SetToken(webRequest.GetResponseHeader("JwtBearerToken"));
            }

            action(webRequest);
            webRequest.Dispose();
        }

        public class ForgotPasswordOpenData
        {
            public string Email;

            public ForgotPasswordOpenData(string email)
            {
                Email = email;
            }
        }

        [Serializable]
        public class ForgotPasswordData
        {
            public string EncryptedNonceWithEmail;
            public string Nonce;
        }

        public IEnumerator ForgotPassword(
            ForgotPasswordOpenData data,
            Action<UnityWebRequest> action
        )
        {
            string nonce = Nonce;
            ForgotPasswordData forgotPasswordData =
                new()
                {
                    Nonce = nonce,
                    EncryptedNonceWithEmail = EncryptString(data.Email + nonce),
                };

            UnityWebRequest webRequest = requestBuilder.CreateRequest(
                "/ForgotPassword",
                HttpMethod.Post,
                forgotPasswordData
            );

            yield return webRequest.SendWebRequest();

            action(webRequest);
            webRequest.Dispose();
        }

        public class RecoverPasswordOpenData
        {
            public int AccessCode;
            public string Password;

            public RecoverPasswordOpenData(int accessCode, string password)
            {
                AccessCode = accessCode;
                Password = password;
            }
        }

        [Serializable]
        public class RecoverPasswordData
        {
            public int AccessCode;
            public string EncryptedHashedPassword;
        }

        public IEnumerator RecoverPassword(
            RecoverPasswordOpenData data,
            Action<UnityWebRequest> action
        )
        {
            RecoverPasswordData recoverPasswordData =
                new()
                {
                    AccessCode = data.AccessCode,
                    EncryptedHashedPassword = EncryptString(HashString(data.Password)),
                };

            UnityWebRequest webRequest = requestBuilder.CreateRequest(
                "/RecoverPassword",
                HttpMethod.Post,
                recoverPasswordData
            );

            yield return webRequest.SendWebRequest();

            action(webRequest);
            webRequest.Dispose();
        }
    }
}
