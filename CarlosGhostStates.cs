using UnityEngine;

namespace CarlosReturn
{
    public class CarlosGhost_StateBase : NpcState
    {
        public CarlosGhost carlosGhost;
        public CarlosGhost_StateBase(CarlosGhost carGhost) : base(carGhost) => carlosGhost = carGhost;
    }

    public class CarlosGhost_Hidden : CarlosGhost_StateBase
    {
        public CarlosGhost_Hidden(CarlosGhost carGhost) : base(carGhost) { }

        public override void Enter()
        {
            base.Enter();
            carlosGhost.navigationStateMachine.ChangeState(new NavigationState_DoNothing(carlosGhost, 1));
            carlosGhost.spriteRenderer[0].enabled = false;
            carlosGhost.ghostAudio.FlushQueue(true);
        }

        public override void Update()
        {
            base.Update();
            carlosGhost.LookAtPlayer(carlosGhost.ec.Players[0]);
        }

        public override void DestinationEmpty() => base.DestinationEmpty();
    }
    public class CarlosGhost_Prep : CarlosGhost_StateBase
    {
        public CarlosGhost_Prep(CarlosGhost carGhost) : base(carGhost) { }

        public override void Exit()
        {
            base.Exit();
            carlosGhost.spriteRenderer[0].enabled = true;
        }

        public override void Update()
        {
            base.Update();
            carlosGhost.LookAtPlayer(carlosGhost.ec.Players[0]);
            if (!carlosGhost.looker.PlayerInSight() && carlosGhost.GetDistance(carlosGhost.ec.Players[0].transform) >= 75)
                carlosGhost.behaviorStateMachine.ChangeState(new CarlosGhost_Wander(carlosGhost));
        }

        public override void DestinationEmpty() => base.DestinationEmpty();
    }

    public class CarlosGhost_Wander : CarlosGhost_StateBase
    {
        public CarlosGhost_Wander(CarlosGhost carGhost) : base(carGhost) { }

        public override void Enter()
        {
            base.Enter();
            carlosGhost.navigationStateMachine.ChangeState(new NavigationState_WanderRandom(carlosGhost, 1));
            carlosGhost.ghostAudio.FlushQueue(true);
            carlosGhost.ghostAudio.QueueAudio(carlosGhost.ghostSoundLow, true);
            carlosGhost.ghostAudio.volumeModifier = 0.85f;
        }

        public override void Update()
        {
            base.Update();
            if (carlosGhost.ghostAudio.filesQueued <= 1)
                carlosGhost.ghostAudio.QueueAudio(carlosGhost.ghostSoundLow);
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (carlosGhost.GetDistance(player.transform) <= 25)
                carlosGhost.behaviorStateMachine.ChangeState(new CarlosGhost_Watch(carlosGhost));
            else
                carlosGhost.navigationStateMachine.ChangeState(new NavigationState_TargetPosition(carlosGhost, 1, player.transform.position));
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            carlosGhost.navigationStateMachine.ChangeState(new NavigationState_WanderRandom(carlosGhost, 1));
        }
    }

    public class CarlosGhost_Watch : CarlosGhost_StateBase
    {
        public CarlosGhost_Watch(CarlosGhost carGhost) : base(carGhost) { }

        public override void Enter()
        {
            base.Enter();
            carlosGhost.navigationStateMachine.ChangeState(new NavigationState_DoNothing(carlosGhost, 1));
            carlosGhost.ghostAudio.FlushQueue(true);
            carlosGhost.ghostAudio.QueueAudio(carlosGhost.ghostSoundHigh, true);
            carlosGhost.ghostAudio.volumeModifier = 1.95f;
        }

        private float watchTimer = 3;
        public override void Update()
        {
            base.Update();
            carlosGhost.LookAtPlayer(carlosGhost.ec.Players[0]);

            watchTimer -= Time.deltaTime * carlosGhost.ec.EnvironmentTimeScale;
            if (watchTimer <= 0)
                carlosGhost.behaviorStateMachine.ChangeState(new CarlosGhost_Wander(carlosGhost));
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (carlosGhost.GetDistance(player.transform) > 35) 
            watchTimer = 1.5f;
            carlosGhost.ec.MakeNoise(player.transform.position, 1);
        }

        public override void DestinationEmpty() => base.DestinationEmpty();
    }
}