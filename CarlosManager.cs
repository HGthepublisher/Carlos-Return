using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CarlosReturn
{
    public class CarlosManager : MonoBehaviour
    {
        public static Carlos carlos;
        public static bool chasing = false;
        public static Color carlosColor = new Color32(32, 153, 244, 255);

        public static bool carlosAngry = false;

        public EnvironmentController ec;

        private bool baldiDestroyed = false;
        private bool spawned = false;

        public float poweroutageMinutes = 4.5f;

        public bool forceSpawn = false;

        private readonly string[] floors = new string[]
        {
                "F1",
                "F2",
                "F3",
                "F4",
                "F5",
        };

        public static int rightAnswers = 0;
        public static int wrongAnswers = 0;

        public BreakerController breaker;

        private void Start()
        {
            if (CarlosBasePlugin.impossible.Value)
                poweroutageMinutes = 1.5f;

            rightAnswers = 0;
            wrongAnswers = 0;

            if (BaseGameManager.Instance is PitstopGameManager)
            {
                PropagatedAudioManagerAnimator animator = FindObjectsOfType<PropagatedAudioManagerAnimator>().First(manager => manager.name == "JohnnyBase");
                animator.enabled = false;
                Animator mouthAnim = animator.GetComponentInChildren<Animator>();
                mouthAnim.enabled = false;
            }

            if (CoreGameManager.Instance.currentMode != Mode.Main)
                Destroy(gameObject);

            GameObject debugger = new GameObject("CarlosDebugManager");
            CarlosDebugManager debugManager = debugger.AddComponent<CarlosDebugManager>();
            debugManager.carlosManager = this;

            poweroutageTime = 60 * poweroutageMinutes;
            if (BreakerController.allBreakers.Count > 0)
                breaker = BreakerController.allBreakers[0];
        }

        public float poweroutageTime;
        private void Update()
        {

            if (!ec.Players[0] || !MainGameManager.Instance && !forceSpawn) return;

            if (!baldiDestroyed && !forceSpawn)
            {
                HappyBaldi baldi = FindObjectOfType<HappyBaldi>();
                if (baldi)
                {
                    baldi.gameObject.SetActive(false);
                    baldiDestroyed = true;
                }
            }

            bool explorerMode = CarlosBasePlugin.explorer.Value;
            bool easyMode = CarlosBasePlugin.easy.Value;
            int neededNotebooks = CoreGameManager.Instance.sceneObject.levelTitle == "F1" ? 3 : CoreGameManager.Instance.sceneObject.levelTitle == "F2" ? 2 : 1;
            if (CarlosBasePlugin.impossible.Value)
                neededNotebooks = 0;
            else if (CarlosBasePlugin.easy.Value)
                neededNotebooks += 1;
            if (!spawned && (rightAnswers >= neededNotebooks || (wrongAnswers > 0 && !easyMode)) && !explorerMode || (forceSpawn && !spawned))
            {
                spawned = true;

                List<RoomController> rooms = new List<RoomController>();
                foreach (RoomController room in ec.rooms)
                    if (room.category == RoomCategory.Class)
                        rooms.Add(room);
                if (rooms.Count == 0)
                    foreach (RoomController room in ec.rooms)
                        rooms.Add(room);

                NPC newCarlos = CarlosBasePlugin.assetManager.Get<NPC>("Carlos");
                Carlos carlosInstance = (Carlos)ec.SpawnNPC(newCarlos, rooms[Random.Range(1, rooms.Count)].RandomEntitySafeCellNoGarbage().position);
                if (CarlosBasePlugin.impossible.Value)
                    carlosInstance.warnings = 2;

                carlos = carlosInstance;

                MusicManager.Instance.StopMidi();

                GameObject carlosGhostManagerObject = new GameObject("CarlosGhostManager");
                CarlosGhostManager carlosGhostManager = carlosGhostManagerObject.AddComponent<CarlosGhostManager>();
                carlosGhostManager.ec = ec;

                CarlosMusicManager.Instance.PlaySound("car_ambience");
            }

            if (CoreGameManager.Instance.sceneObject.levelObject == null) return;

            if (breaker && poweroutageTime <= 0 && !chasing)
            {
                poweroutageTime = 60 * poweroutageMinutes;
                FieldInfo fuseBlown = typeof(BreakerController).GetField("fuseBlown", BindingFlags.NonPublic | BindingFlags.Static);
                if (fuseBlown != null && !(bool)fuseBlown.GetValue(null))
                    breaker.Invoke("BlowFuse", 0);
            }
            else
                poweroutageTime -= Time.deltaTime * ec.EnvironmentTimeScale;
        }
    }
}