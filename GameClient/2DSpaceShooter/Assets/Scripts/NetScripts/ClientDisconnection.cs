using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientDisconnection : MonoBehaviour
{
    [SerializeField]
    private Button button;

    private void OnEnable()
    {
        button.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(0);
        });
    }
}
