using EasyTransition;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    [AddComponentMenu("MainMenu.Main")]
    public class Main : MonoBehaviour
    {
        [SerializeField]
        private Button quit;

        [SerializeField]
        private Button createGeneration;

        [SerializeField]
        private TransitionSettings transitionSettings;

        [SerializeField]
        private string sceneName;

        private void Awake()
        {
            quit.onClick.AddListener(Quit);
            createGeneration.onClick.AddListener(CreateGeneration);
        }

        public void SetSceneString(string sceneName)
        {
            this.sceneName = sceneName;
        }

        private void Quit()
        {
            TransitionManager.Instance().Transition("Login", transitionSettings, 0);
        }

        private void CreateGeneration()
        {
            TransitionManager.Instance().Transition(sceneName, transitionSettings, 0);
        }
    }
}
