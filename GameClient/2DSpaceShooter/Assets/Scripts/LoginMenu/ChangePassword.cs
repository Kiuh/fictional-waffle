using Common;
using General;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LoginMenu
{
    [AddComponentMenu("LoginMenu.ChangePassword")]
    public class ChangePassword : MonoBehaviour
    {
        [SerializeField]
        private Button close;

        [SerializeField]
        private TMP_InputField oneTimeKey;

        [SerializeField]
        private TMP_InputField newPassword;

        [SerializeField]
        private TMP_Text error;

        [SerializeField]
        private Button send;

        [SerializeField]
        private SuccessChangePassword successful;

        [SerializeField]
        private Login login;

        private void Awake()
        {
            close.onClick.AddListener(Close);
            send.onClick.AddListener(Send);
            error.text = string.Empty;
        }

        private void Close()
        {
            login.SetInteractive(true);
            gameObject.SetActive(false);
        }

        private void Send()
        {
            Result passwordResult = DataValidator.ValidatePassword(newPassword.text);
            if (passwordResult.Failure)
            {
                error.text = passwordResult.Error;
                return;
            }
            if (!int.TryParse(oneTimeKey.text, out int result))
            {
                error.text = "Invalid Access Code!";
                return;
            }
            _ = StartCoroutine(
                ServerProvider.Instance.RecoverPassword(
                    new ServerProvider.RecoverPasswordOpenData(result, newPassword.text),
                    SendEnd
                )
            );
            LoadingPause.Instance.ShowLoading("Registering");
        }

        private void SendEnd(UnityWebRequest webRequest)
        {
            LoadingPause.Instance.HideLoading();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                successful.gameObject.SetActive(true);
                gameObject.SetActive(false);
            }
            else
            {
                error.text = webRequest.downloadHandler.text.FromErrorBody();
            }
        }
    }
}
