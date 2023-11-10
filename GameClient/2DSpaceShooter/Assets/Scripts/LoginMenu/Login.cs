using EasyTransition;
using General;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LoginMenu
{
    [AddComponentMenu("LoginMenu.Login")]
    public class Login : MonoBehaviour
    {
        [SerializeField]
        private Button login;

        [SerializeField]
        private Button registration;

        [SerializeField]
        private Button forgotPassword;

        [SerializeField]
        private TMP_Text error;

        [SerializeField]
        private TMP_InputField loginField;

        [SerializeField]
        private TMP_InputField passwordField;

        [SerializeField]
        private TransitionSettings transitionSettings;

        [SerializeField]
        private ForgotPassword forgotPassManager;

        private void Awake()
        {
            login.onClick.AddListener(LogIn);
            registration.onClick.AddListener(Registration);
            forgotPassword.onClick.AddListener(ForgotPassword);
            error.text = string.Empty;
        }

        private void LogIn()
        {
            _ = StartCoroutine(
                ServerProvider.Instance.Login(
                    new ServerProvider.LoginOpenData(loginField.text, passwordField.text),
                    LogInEnd
                )
            );
            LoadingPause.Instance.ShowLoading("Verification");
        }

        private void LogInEnd(UnityWebRequest webRequest)
        {
            LoadingPause.Instance.HideLoading();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                TransitionManager.Instance().Transition("MainMenu", transitionSettings, 0);
            }
            else
            {
                error.text = webRequest.downloadHandler.text.FromErrorBody();
            }
        }

        private void Registration()
        {
            TransitionManager.Instance().Transition("Registration", transitionSettings, 0);
        }

        private void ForgotPassword()
        {
            SetInteractive(false);
            forgotPassManager.gameObject.SetActive(true);
        }

        public void SetInteractive(bool interactable)
        {
            login.interactable = interactable;
            registration.interactable = interactable;
            forgotPassword.interactable = interactable;
            loginField.interactable = interactable;
            passwordField.interactable = interactable;
        }
    }
}
