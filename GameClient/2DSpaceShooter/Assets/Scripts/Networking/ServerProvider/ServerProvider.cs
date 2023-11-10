using CryptoNet;
using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking
{
    [AddComponentMenu("Networking.ServerProvider")]
    public partial class ServerProvider : MonoBehaviour
    {
        private UnityWebRequestBuilder requestBuilder;
        private ICryptoNet rsa;

        private string Nonce => Convert.ToString(DateTimeOffset.Now.ToUnixTimeMilliseconds());

        public static ServerProvider Instance { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(this);
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            requestBuilder = new("https://localhost:5000");
            _ = StartCoroutine(GetPublicKey());
        }

        private IEnumerator GetPublicKey()
        {
            UnityWebRequest webRequest = requestBuilder.CreateRequest("/PublicKey", HttpMethod.Get);

            yield return webRequest.SendWebRequest();

            rsa = new CryptoNetRsa(webRequest.downloadHandler.text);
            webRequest.Dispose();
        }

        public string HashString(string input)
        {
            using SHA256 sha256Hash = SHA256.Create();
            byte[] hashedBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new();
            for (int i = 0; i < hashedBytes.Length; i++)
            {
                _ = sBuilder.Append(hashedBytes[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public string EncryptString(string value)
        {
            byte[] encrypted = rsa.EncryptFromString(value);
            return Convert.ToBase64String(encrypted);
        }
    }
}
