using EasyTransition;
using UnityEngine;
using UnityEngine.UI;

namespace Registration
{
    [AddComponentMenu("Registration.Successfully")]
    public class SuccessResend : MonoBehaviour
    {
        [SerializeField]
        private Button close;

        [SerializeField]
        private TransitionSettings transitionSettings;

        private void Awake()
        {
            close.onClick.AddListener(Close);
        }

        private void Close()
        {
            TransitionManager.Instance().Transition("Login", transitionSettings, 0);
        }
    }
}
