using UnityEngine;

namespace CarlosReturn
{
    public class CarlosMusicManager : MonoBehaviour
    {
        public AudioManager audioManager;
        public AudioManager chasingManager;

        private SoundObject ambience;
        private SoundObject angry;

        private bool angryDB;
        
        private void Awake()
        {
            audioManager = gameObject.AddComponent<AudioManager>();
            audioManager.audioDevice = gameObject.AddComponent<AudioSource>();
            audioManager.positional = false;
            audioManager.volumeModifier = 0.65f;
            audioManager.maintainLoop = true;
            audioManager.useUnscaledPitch = true;
            audioManager.ignoreListenerPause = true;

            chasingManager = gameObject.AddComponent<AudioManager>();
            chasingManager.audioDevice = gameObject.AddComponent<AudioSource>();
            chasingManager.positional = false;
            chasingManager.volumeModifier = 0.42f;
            chasingManager.maintainLoop = true;
            chasingManager.useUnscaledPitch = true;
            chasingManager.ignoreListenerPause = true;

            ambience = CarlosBasePlugin.assetManager.Get<SoundObject>("car_ambience");
            angry = CarlosBasePlugin.assetManager.Get<SoundObject>("car_angry");
        }

        public void PlaySound(string sound, bool loop = true)
        {
            SoundObject currentSound = CarlosBasePlugin.assetManager.Get<SoundObject>(sound);

            audioManager.Pause(false);
            audioManager.SetLoop(loop);
            audioManager.QueueAudio(currentSound, true);
        }
        public void StopSound()
        {
            if (audioManager)
                audioManager.FlushQueue(true);
        }

        private void Update()
        {
            if (CarlosManager.carlosAngry != angryDB)
                chasingManager.FlushQueue(true);
            angryDB = CarlosManager.carlosAngry;

            if (CarlosManager.carlos && !audioManager.AnyAudioIsPlaying && !chasingManager.QueuedUp)
                chasingManager.QueueAudio(angryDB ? angry : ambience);
            else if (audioManager.AnyAudioIsPlaying)
                chasingManager.FlushQueue(true);
        }
    }
}