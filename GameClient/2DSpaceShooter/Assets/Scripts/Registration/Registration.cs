using Common;
using EasyTransition;
using General;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Registration
{
    [AddComponentMenu("Registration.Registration")]
    public class Registration : MonoBehaviour
    {
        [SerializeField]
        private Button back;

        [SerializeField]
        private Button register;

        [SerializeField]
        private Button lostVerificationButton;

        [SerializeField]
        private TMP_InputField loginField;

        [SerializeField]
        private TMP_InputField emailField;

        [SerializeField]
        private TMP_InputField passwordField;

        [SerializeField]
        private TMP_InputField repeatPasswordField;

        [SerializeField]
        private TMP_Text error;

        [SerializeField]
        private SuccessRegistration successRegistration;

        [SerializeField]
        private ResendRegistration lostVerification;

        [SerializeField]
        private TransitionSettings transitionSettings;

        private void Awake()
        {
            back.onClick.AddListener(Back);
            register.onClick.AddListener(Register);
            lostVerificationButton.onClick.AddListener(LostVerification);
            error.text = string.Empty;
        }

        private void Register()
        {
            Result loginResult = DataValidator.ValidateLogin(loginField.text);
            if (loginResult.Failure)
            {
                error.text = loginResult.Error;
                return;
            }
            Result emailResult = DataValidator.ValidateEmail(emailField.text);
            if (emailResult.Failure)
            {
                error.text = emailResult.Error;
                return;
            }
            Result passwordResult = DataValidator.ValidatePassword(passwordField.text);
            if (passwordResult.Failure)
            {
                error.text = passwordResult.Error;
                return;
            }
            if (passwordField.text != repeatPasswordField.text)
            {
                error.text = "Passwords not match.";
                return;
            }

            _ = StartCoroutine(
                ServerProvider.Instance.Registration(
                    new ServerProvider.RegistrationOpenData(
                        loginField.text,
                        emailField.text,
                        passwordField.text
                    ),
                    RegisterEnd
                )
            );
            LoadingPause.Instance.ShowLoading("Registering");
        }

        private void RegisterEnd(UnityWebRequest webRequest)
        {
            LoadingPause.Instance.HideLoading();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                successRegistration.gameObject.SetActive(true);
            }
            else
            {
                error.text = webRequest.downloadHandler.text.FromErrorBody();
            }
        }

        private void LostVerification()
        {
            SetInteractive(false);
            lostVerification.gameObject.SetActive(true);
        }

        private void Back()
        {
            TransitionManager.Instance().Transition("Login", transitionSettings, 0);
        }

        public void SetInteractive(bool value)
        {
            back.interactable = value;
            register.interactable = value;
            lostVerificationButton.interactable = value;
        }
    }
}
