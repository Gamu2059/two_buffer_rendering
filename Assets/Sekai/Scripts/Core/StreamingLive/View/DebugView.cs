using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Sekai.Scripts.Core.StreamingLive.View
{
    public class DebugView : MonoBehaviour
    {
        [SerializeField]
        private Text text;

#if UNITY_ANDROID && !UNITY_EDITOR

        private const float recordInterval = 30f;
        private const int recordCount = 60;
        
        private AndroidJavaClass thermal;
        private float time;

        private int currentCount;

        private void Start()
        {
            thermal = new AndroidJavaClass("com.sho_yamagami.thermal.Thermal");
            thermal.CallStatic("setup");
            text.text = "";
            time = Time.unscaledTime;
            currentCount = 0;
        }

        private void OnDestroy()
        {
            thermal?.CallStatic("cleanup");
        }

        private void Update()
        {
            if(Time.unscaledTime - time >= recordInterval)
            {
                time = Time.unscaledTime;
                currentCount++;
                Record();

                if(currentCount >= recordCount)
                {
                    Destroy(this);
                }
            }
        }

        private void Record()
        {
            var temperature = thermal.CallStatic<float>("getBatteryTemperature");
            text.text = $"{text.text}, {temperature.ToString()}";
        }
#else
        private void Start()
        {
            Destroy(gameObject);
        }
#endif
    }
}