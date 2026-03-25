using System.Collections.Generic;
using UnityEngine;

namespace CarlosReturn
{
    public class CarlosAmbienceManager : MonoBehaviour
    {
        public EnvironmentController ec;

        public AudioManager audioManager;

        public static List<SoundObject> ambience = new List<SoundObject>();

        private float delayTime = 0;

        private void Awake()
        {
            audioManager = gameObject.AddComponent<AudioManager>();
            audioManager.audioDevice = gameObject.AddComponent<AudioSource>();
            audioManager.positional = true;
            audioManager.volumeModifier = 0.8f;
            audioManager.maintainLoop = false;

            SetDelay();
        }

        private void Update()
        {
            if (!audioManager || ambience.Count <= 0) return;

            delayTime -= Time.deltaTime * ec.EnvironmentTimeScale;

            if (delayTime <= 0)
            {
                SetDelay();

                transform.position = CoreGameManager.Instance.GetPlayer(0).transform.position + new Vector3(Random.Range(-20, 20), 0, Random.Range(-20, 20));
                SoundObject ambient = ambience[Random.Range(1, ambience.Count)];
                ambient.subtitle = false;
                audioManager.PlaySingle(ambient);
            }
        }

        private void SetDelay() => delayTime = 60 * (Random.Range(8, 22) / 10);
    }
}