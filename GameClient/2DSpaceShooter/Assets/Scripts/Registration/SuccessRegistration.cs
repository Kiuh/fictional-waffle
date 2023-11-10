using EasyTransition;
using UnityEngine;
using UnityEngine.UI;

namespace Registration
{
    [AddComponentMenu("Registration.SuccessRegistration")]
    public class SuccessRegistration : MonoBehaviour
    {
        [SerializeField]
        private Button ok;

        [SerializeField]
        private TransitionSettings transitionSettings;

        private void Awake()
        {
            ok.onClick.AddListener(Ok);
        }

        private void Ok()
        {
            TransitionManager.Instance().Transition("Login", transitionSettings, 0);
        }
    }
}
