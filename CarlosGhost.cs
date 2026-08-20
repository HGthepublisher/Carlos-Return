using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CarlosReturn
{
    public class CarlosGhost : NPC
    {
        public AudioManager ghostAudio;

        public SoundObject ghostSoundHigh;
        public SoundObject ghostSoundLow;

        public List<Sprite> ghostSprites;

        public override void Initialize()
        {
            base.Initialize();
            Entity.SetHeight(6.4f);
            if (CarlosBasePlugin.impossible.Value)
                Navigator.speed *= 1.8f;

            SpriteRotator sr = spriteRenderer[0].gameObject.AddComponent<SpriteRotator>();

            FieldInfo fieldInfo1 = AccessTools.Field(typeof(SpriteRotator), "spriteRenderer");
            fieldInfo1.SetValue(sr, spriteRenderer[0]);
            FieldInfo fieldInfo2 = AccessTools.Field(typeof(SpriteRotator), "sprites");
            fieldInfo2.SetValue(sr, ghostSprites.ToArray());

            behaviorStateMachine.ChangeState(new CarlosGhost_Hidden(this));

            FieldInfo fieldInfo3 = AccessTools.Field(typeof(Entity), "maxHideableLightLevel");
            fieldInfo3.SetValue(Entity, -1f);

            bool debug = CarlosBasePlugin.debug.Value;
            bool arrows = CarlosBasePlugin.debugArrows.Value;
            if (debug && arrows)
                ec.map.AddArrow(Entity, CarlosManager.carlosColor - new Color(0.15f, 0.15f, 0.15f, 0.3f));
        }

        private bool db = false;
        protected override void VirtualUpdate()
        {
            base.VirtualUpdate();
            if (CarlosManager.carlos.warnings > 3) Despawn();

            if (db == CarlosGhostManager.fuseBlown) return;
            db = CarlosGhostManager.fuseBlown;

            bool easyMode = CarlosBasePlugin.easy.Value;
            bool debug = CarlosBasePlugin.debug.Value;
            bool arrows = CarlosBasePlugin.debugArrows.Value;
            if (db)
            {
                if (easyMode && !(debug && arrows))
                    ec.map.AddArrow(Entity, CarlosManager.carlosColor - new Color(0.15f, 0.15f, 0.15f, 0.3f));
                behaviorStateMachine.ChangeState(new CarlosGhost_Prep(this));
            }
            else
            {
                if (easyMode && !(debug && arrows))
                    ec.map.arrowTargets[ec.map.arrowTargets.IndexOf(Entity)] = null;
                behaviorStateMachine.ChangeState(new CarlosGhost_Hidden(this));
            }
        }

        public void LookAtPlayer(PlayerManager player) => transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);

        public float GetDistance(Transform from) { return (transform.position - from.position).magnitude; }
    }
}