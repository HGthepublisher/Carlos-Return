using MTM101BaldAPI;
using UnityEngine;
using UnityEngine.Rendering;

namespace CarlosReturn
{
    public class CarlosMusicManager : MonoBehaviour
    {
        public static CarlosMusicManager Instance { get; private set; }

        public AudioManager audioManager;
        
        private void Awake()
        {
            if (Instance)
                Destroy(gameObject);
            Instance = this;

            if (!audioManager)
            {
                audioManager = gameObject.AddComponent<AudioManager>();
                audioManager.positional = false;
                audioManager.volumeModifier = 0.65f;
                audioManager.maintainLoop = true;
                audioManager.useUnscaledPitch = true;
                audioManager.ignoreListenerPause = true;
            }
        }

        public void PlaySound(string sound, bool loop = true)
        {
            SoundObject currentSound = CarlosBasePlugin.assetManager.Get<SoundObject>(sound);

            audioManager.volumeModifier = sound == "car_ambience" ? 0.1f : 0.65f;
            audioManager.Pause(false);
            audioManager.SetLoop(loop);
            audioManager.QueueAudio(currentSound, true);
        }
        public void StopSound()
        {
            if (audioManager)
                audioManager.FlushQueue(true);
        }
    }
}