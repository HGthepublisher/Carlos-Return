using UnityEngine;

namespace CarlosReturn
{
    public class Carlos_StateBase : NpcState
    {
        public Carlos carlos;
        public Carlos_StateBase(Carlos car) : base(car)
        {
            carlos = car;
            gameManager = GameObject.FindObjectOfType<MainGameManager>();
        }

        public MainGameManager gameManager;

        public int warnings = 0;

        public float CalculateSpeed(float originalSpeed)
        {
            return originalSpeed + Mathf.Clamp((1.4f * CarlosManager.rightAnswers) + (1.8f * CarlosManager.wrongAnswers) - carlos.startingNotebooks, 0, float.PositiveInfinity);
        }
    }

    public class Carlos_Wander : Carlos_StateBase
    {
        public Carlos_Wander(Carlos car, bool unhap = false, bool spawn = false) : base(car)
        {
            unhappy = unhap;
            spawned = spawn;
        }

        private readonly bool unhappy;
        private readonly bool spawned;

        private bool canMove = true;

        public override void Enter()
        {
            base.Enter();
            carlos.ChangeTexture(carlos.carlosHappy);

            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));

            if (unhappy)
                carlos.PlayAudio(carlos.carCantFind);
            else if (spawned)
                carlos.PlayAudio(carlos.carSpawn);

            playerSight = carlos.seeDelay;
        }

        private float playerSight = 1f;
        private bool playerSaw = false;
        private bool noticeDB = false;
        private Vector3 target = new Vector3();
        public override void Update()
        {
            base.Update();
            carlos.SetSpeed(CalculateSpeed(carlos.calmSpeed));
            carlos.SetRoomAvoidance(gameManager.FoundNotebooks <= (gameManager.NotebookTotal - 1));

            if (playerSaw)
            {
                if (playerSight <= 0)
                    carlos.ChangeBehaviourState(new Carlos_Follow(carlos, !seesPlayer, target));
                else
                    playerSight -= Time.deltaTime * npc.ec.EnvironmentTimeScale;

                if (!noticeDB)
                {
                    noticeDB = true;
                    carlos.PlayAudio(carlos.carNotice);
                }
            }
        }

        private bool checkingLocker = false;
        private HideableLocker locker = null;
        public override void Hear(GameObject source, Vector3 position, int value)
        {
            base.Hear(source, position, value);
            locker = source ? source.GetComponent<HideableLocker>() : null;
            checkingLocker = locker;
            ChangeNavigationState(new NavigationState_TargetPosition(npc, 1, position));
        }

        private bool seesPlayer = false;
        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            ChangeNavigationState(new NavigationState_DoNothing(npc, 1));
            canMove = false;
            playerSaw = true;
            seesPlayer = true;
        }
        public override void PlayerLost(PlayerManager player)
        {
            base.PlayerLost(player);
            target = player.transform.position;
            seesPlayer = false;
        }

        public override void OnStateTriggerStay(Entity otherEntity, Collider other, bool validCollision)
        {
            base.OnStateTriggerStay(otherEntity, other, validCollision);
            if (validCollision && other.GetComponent<PlayerManager>())
                carlos.behaviorStateMachine.ChangeState(new Carlos_Warning(carlos));
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            if (checkingLocker)
                carlos.ChangeBehaviourState(new Carlos_CheckLocker(carlos, locker));
            else if (canMove)
                ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }
    }

    public class Carlos_Follow : Carlos_StateBase
    {
        public Carlos_Follow(Carlos car, bool goTo = false, Vector3 target = new Vector3()) : base(car)
        {
            _goTo = goTo;
            _target = target;
        }

        private readonly bool _goTo;
        private Vector3 _target;

        private bool looking = true;

        public override void Enter()
        {
            base.Enter();
            carlos.PlayAudio(carlos.carSighted, true);
            carlos.ChangeTexture(carlos.carlosUnhappy);

            CarlosManager.chasing = true;

            if (_goTo)
                ChangeNavigationState(new NavigationState_TargetPosition(npc, 1, _target));
            else
                ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }

        float timeLeft = 1f;
        public override void Update()
        {
            base.Update();
            carlos.SetSpeed(CalculateSpeed(carlos.speed));
            carlos.SetRoomAvoidance(gameManager.FoundNotebooks <= (gameManager.NotebookTotal - 1));

            carlos.PlayAudio(carlos.carSighted, true);

            if (looking)
                timeLeft = carlos.unnoticeDelay;
            else if (timeLeft <= 0)
                carlos.behaviorStateMachine.ChangeState(new Carlos_Wander(carlos, true));
            else
                timeLeft -= Time.deltaTime * npc.ec.EnvironmentTimeScale;
        }

        private bool checkingLocker = false;
        private HideableLocker locker = null;
        public override void Hear(GameObject source, Vector3 position, int value)
        {
            base.Hear(source, position, value);
            locker = source && seesPlayer ? source.GetComponent<HideableLocker>() : null;
            checkingLocker = locker;

            ChangeNavigationState(new NavigationState_TargetPosition(npc, 1, position));
            looking = true;
        }

        private bool seesPlayer = false;
        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (checkingLocker) return;

            ChangeNavigationState(new NavigationState_TargetPosition(npc, 1, player.transform.position));
            looking = true;
            seesPlayer = true;
        }
        public override void PlayerLost(PlayerManager player)
        {
            base.PlayerLost(player);
            seesPlayer = false;
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            if (checkingLocker)
                carlos.ChangeBehaviourState(new Carlos_CheckLocker(carlos, locker, true));
            else
            {
                looking = false;
                ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
            }
        }

        public override void OnStateTriggerStay(Entity otherEntity, Collider other, bool validCollision)
        {
            base.OnStateTriggerStay(otherEntity, other, validCollision);
            if (validCollision && other.GetComponent<PlayerManager>())
                carlos.behaviorStateMachine.ChangeState(new Carlos_Warning(carlos));
        }

        public override void Exit()
        {
            base.Exit();
            CarlosManager.chasing = false;
        }
    }

    public class Carlos_Warning : Carlos_StateBase
    {
        public Carlos_Warning(Carlos car) : base(car) { }

        private float cooldown;

        private bool inSight = false;

        public override void Enter()
        {
            base.Enter();
            carlos.warnings++;
            warnings = carlos.warnings;

            ChangeNavigationState(new NavigationState_DoNothing(npc, 0));

            if (warnings <= 3)
            {
                carlos.ChangeTexture(carlos.carlosNeutral[warnings - 1]);
                carlos.PlayAudio(carlos.carWarnings[warnings - 1]);
                cooldown = carlos.warningDelay;
            }
            else
            {
                carlos.StopAudio();
                carlos.EnableRenderer(false);
                carlos.SetSpeed(carlos.madSpeed);

                CarlosMusicManager.Instance.StopSound();

                cooldown = carlos.madDelay;
                ChangeNavigationState(new NavigationState_WanderFlee(npc, 0, carlos.player.DijkstraMap));

                foreach (Cell cell in carlos.ec.AllCells())
                    if (cell.hasLight)
                    {
                        cell.SetLight(false);
                        carlos.ec.UpdateLightingAtCell(cell);
                    }
            }
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            inSight = true;
        }
        public override void PlayerLost(PlayerManager player)
        {
            base.PlayerLost(player);
            inSight = false;
        }

        public override void Update()
        {
            base.Update();
            if (!carlos.IsPlayingAudio() || warnings > 3)
                cooldown -= Time.deltaTime * npc.ec.EnvironmentTimeScale;

            if (cooldown <= 0)
            {
                if (warnings > 3)
                    carlos.behaviorStateMachine.ChangeState(new Carlos_Chase(carlos));
                else if (inSight)
                    carlos.behaviorStateMachine.ChangeState(new Carlos_Follow(carlos));
                else
                    carlos.behaviorStateMachine.ChangeState(new Carlos_Wander(carlos));
            }
        }

        public override void Exit()
        {
            base.Exit();
            carlos.EnableRenderer();
            
            if (warnings > 3)
                foreach (Cell cell in carlos.ec.AllCells())
                    if (cell.hasLight)
                    {
                        cell.SetLight(true);
                        cell.lightColor = Color.red;
                        carlos.ec.UpdateLightingAtCell(cell);
                    }
        }
    }

    public class Carlos_Chase : Carlos_StateBase
    {
        public Carlos_Chase(Carlos car) : base(car) { }

        public override void Enter()
        {
            base.Enter();
            carlos.StopAudio();

            CarlosMusicManager.Instance.PlaySound("car_angry", true);

            carlos.ChangeTexture(carlos.carlosMad);
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));

            CoreGameManager.Instance.disablePause = true;
        }

        public override void Update()
        {
            base.Update();

            if (carlos.player && !carlos.player.plm.Entity.Hidden && !carlos.player.plm.Entity.Frozen)
                ChangeNavigationState(new NavigationState_TargetPosition(npc, 0, carlos.player.transform.position));

            carlos.PlayAudio(carlos.carAngry, true);
        }

        public override void OnStateTriggerStay(Entity otherEntity, Collider other, bool validCollision)
        {
            base.OnStateTriggerStay(otherEntity, other, validCollision);
            if (validCollision && other.GetComponent<PlayerManager>())
                CoreGameManager.Instance.ReturnToMenu();
        }

        public override void Hear(GameObject source, Vector3 position, int value)
        {
            base.Hear(source, position, value);
            ChangeNavigationState(new NavigationState_TargetPosition(carlos, 1, position));
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    public class Carlos_CheckLocker : Carlos_StateBase
    {
        public Carlos_CheckLocker(Carlos car, HideableLocker target, bool force = false) : base(car)
        {
            locker = target;
            forceOpen = force;
        }
        private readonly HideableLocker locker;
        private readonly bool forceOpen;

        public override void Enter()
        {
            base.Enter();
            if (forceOpen)
            {
                locker.ForceOpen();
                carlos.ChangeBehaviourState(new Carlos_Warning(carlos));
                return;
            }

            ChangeNavigationState(new NavigationState_DoNothing(npc, 0));
            time = carlos.checkLockerTime;
        }

        private float time;
        public override void Update()
        {
            base.Update();
            time -= Time.deltaTime * npc.ec.EnvironmentTimeScale;
            if (time <= 0)
            {
                if ((Random.Range(gameManager.FoundNotebooks, gameManager.NotebookTotal) >= gameManager.NotebookTotal - 1 || CarlosBasePlugin.hardMode.Value) && locker.playerInside)
                {
                    locker.ForceOpen();
                    carlos.ChangeBehaviourState(new Carlos_Warning(carlos));
                }
                else
                    carlos.ChangeBehaviourState(new Carlos_Wander(carlos));
            }
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
        }
    }
}