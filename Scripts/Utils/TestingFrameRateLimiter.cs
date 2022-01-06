using UnityEngine;

namespace Assets.Scripts
{
    //Eri frameratejen testausta varten
    public class TestingFrameRateLimiter : MonoBehaviour
    {
        [SerializeField] int frameRate;

        public int FrameRate
        {
            get { return frameRate; }
            set
            {
                frameRate = value;
                ChangeTargetFramerate();
            }
        }

        void Awake()
        {
#if UNITY_EDITOR
            QualitySettings.vSyncCount = 0;
            ChangeTargetFramerate();
#endif
        }

        public void ChangeTargetFramerate()
        {
            Application.targetFrameRate = FrameRate;
        }

    }
}