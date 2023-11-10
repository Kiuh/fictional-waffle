using TMPro;
using UnityEngine;

namespace Common
{
    public class TimeScaleChanger : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text timeScaleText;
        public void ChangeTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            timeScaleText.text = $"Time scale: {timeScale}";
        }
    }
}
