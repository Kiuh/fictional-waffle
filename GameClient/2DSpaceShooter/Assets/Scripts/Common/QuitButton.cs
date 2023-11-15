using EasyTransition;
using System.Collections;
using UnityEngine;

namespace Common
{
    [AddComponentMenu("Common.QuitButton")]
    public class QuitButton : MonoBehaviour
    {
        [SerializeField]
        private TransitionSettings transitionSettings;

        public void Quit(string targetScene)
        {
            TransitionManager.Instance().Transition(targetScene, transitionSettings, 0);
        }

        public void CloseApplication()
        {
            TransitionManager.Instance().Transition(transitionSettings, 0);
            _ = StartCoroutine(WaitToQuit());
        }

        private IEnumerator WaitToQuit()
        {
            yield return new WaitForSecondsRealtime(1f);
            Application.Quit();
        }
    }
}
