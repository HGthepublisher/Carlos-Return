using MTM101BaldAPI;
using System.Collections.Generic;
using UnityEngine;

namespace CarlosReturn
{
    public class CarlosAmbienceManager : MonoBehaviour
    {
        public EnvironmentController ec;

        public AudioManager audioManager;

        public static List<SoundObject> ambience = new List<SoundObject>();

        private float delayTime = 60;
        private float RadomDelay() { return 60 * (Random.Range(8, 22) / 10); }

        private void Awake()
        {
            if (!audioManager)
            {
                audioManager = gameObject.AddComponent<AudioManager>();
                audioManager.positional = true;
                audioManager.volumeModifier = 0.8f;
                audioManager.maintainLoop = false;
            }
        }

        private bool db = false;
        private void Update()
        {
            if (!audioManager || ambience.Count <= 0 || db) return;
            db = true;
             
            delayTime -= Time.deltaTime * ec.EnvironmentTimeScale;

            if (delayTime <= 0)
            {
                delayTime = RadomDelay();

                SoundObject ambient = ambience[Random.Range(1, ambience.Count)];
                ambient.subtitle = false;
                audioManager.PlaySingle(ambient);
            }

            db = false;
        }
    }
}