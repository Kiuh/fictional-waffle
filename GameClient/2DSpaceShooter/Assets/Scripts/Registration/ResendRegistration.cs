using General;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Registration
{
    [AddComponentMenu("Registration.ResendRegistration")]
    public class ResendRegistration : MonoBehaviour
    {
        [SerializeField]
        private Button close;

        [SerializeField]
        private TMP_InputField email;

        [SerializeField]
        private Button send;

        [SerializeField]
        private Registration registration;

        [SerializeField]
        private SuccessResend successfully;

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
            registration.SetInteractive(true);
            gameObject.SetActive(false);
        }

        private void Send()
        {
            _ = StartCoroutine(
                ServerProvider.Instance.ResendEmailVerification(
                    new ServerProvider.ResendRegistrationOpenData(email.text),
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
                successfully.gameObject.SetActive(true);
            }
            else
            {
                error.text = webRequest.downloadHandler.text.FromErrorBody();
            }
        }
    }
}
