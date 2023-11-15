using EasyTransition;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ClientDisconnection : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private TransitionSettings transitionSettings;

    private void OnEnable()
    {
        button.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            TransitionManager.Instance().Transition("MainMenu", transitionSettings, 0);
        });
    }
}
