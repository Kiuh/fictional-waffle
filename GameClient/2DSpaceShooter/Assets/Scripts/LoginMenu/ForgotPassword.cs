using General;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LoginMenu
{
    [AddComponentMenu("LoginMenu.ForgotPassManager")]
    public class ForgotPassword : MonoBehaviour
    {
        [SerializeField]
        private Button close;

        [SerializeField]
        private TMP_InputField email;

        [SerializeField]
        private Button send;

        [SerializeField]
        private Login login;

        [SerializeField]
        private ChangePassword successForgotPassManager;

        [SerializeField]
        private TMP_Text error;

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
            _ = StartCoroutine(
                ServerProvider.Instance.ForgotPassword(
                    new ServerProvider.ForgotPasswordOpenData(email.text),
                    SendEnd
                )
            );
            LoadingPause.Instance.ShowLoading("Verification");
        }

        private void SendEnd(UnityWebRequest webRequest)
        {
            LoadingPause.Instance.HideLoading();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                gameObject.SetActive(false);
                successForgotPassManager.gameObject.SetActive(true);
            }
            else
            {
                error.text = webRequest.downloadHandler.text.FromErrorBody();
            }
        }
    }
}
