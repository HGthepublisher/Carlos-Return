using System.Collections.Generic;
using UnityEngine;

namespace CarlosReturn
{
    public class Carlos : NPC
    {
        public Sprite carlosHappy;
        public Sprite[] carlosNeutral;
        public Sprite carlosUnhappy;
        public Sprite carlosMad;

        public AudioManager carAudio;

        public List<SoundObject> carWarnings = new List<SoundObject>();

        public SoundObject carSpawn;
        public SoundObject carCantFind;
        public SoundObject carSighted;
        public SoundObject carNotice;
        public SoundObject carAngry;

        public PlayerManager player;

        public int warnings = 0;

        public readonly float calmSpeed = 10;
        public readonly float speed = 19;
        public readonly float madSpeed = 64;

        public readonly float seeDelay = 1.2f;
        public readonly float hearDelay = 0.6f;
        public readonly float madDelay = 6.6f;
        public readonly float warningDelay = 2.8f;

        public readonly float unnoticeDelay = 3.2f;

        public readonly float fleeTime = 8.5f;

        public readonly float checkLockerTime = 0.58f;

        public int startingNotebooks;

        public override void Initialize()
        {
            base.Initialize();
            if (ec)
                player = ec.Players[0];

            Navigator.Entity.SetHeight(6.2f);
            Navigator.SetSpeed(speed);

            carAudio.volumeModifier = 1.85f;

            startingNotebooks = BaseGameManager.Instance.FoundNotebooks;

            behaviorStateMachine.ChangeState(new Carlos_Wander(this, false, true));
        }

        private bool looping = false;
        private SoundObject loopSound = null;
        protected override void VirtualUpdate()
        {
            base.VirtualUpdate();
            if (looping && loopSound && !carAudio.QueuedUp)
                carAudio.QueueAudio(loopSound);
        }

        public void EnableRenderer(bool enabled = true) => spriteRenderer[0].enabled = enabled;
        public void ChangeTexture(Sprite sprite) => spriteRenderer[0].sprite = sprite;
        public void StopAudio()
        {
            carAudio.FlushQueue(true);
            looping = false;
            loopSound = null;
        }
        public void PlayAudio(SoundObject sound, bool loop = false)
        {
            if (loop && loopSound == sound) return;

            StopAudio();
            carAudio.QueueAudio(sound, true);

            looping = loop;
            loopSound = loop ? sound : null;
        }
        public bool IsPlayingAudio()
        {
            return carAudio.AnyAudioIsPlaying;
        }
        public void SetSpeed(float speed) => navigator.SetSpeed(speed);
        public void SetRoomAvoidance(bool avoid) => navigator.SetRoomAvoidance(avoid);
        public void ChangeBehaviourState(Carlos_StateBase state) => behaviorStateMachine.ChangeState(state);
    }
}