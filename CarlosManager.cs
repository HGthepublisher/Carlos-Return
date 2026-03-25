using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarlosReturn
{
    public class CarlosManager : MonoBehaviour
    {
        public static NPC carlos;
        public static bool chasing = false;
        public static Color carlosColor = new Color32(32, 153, 244, 255);

        public static bool carlosAngry = false;

        public EnvironmentController ec;

        private bool baldiDestroyed = false;
        private bool spawned = false;

        private readonly float poweroutageMinutes = 4.5f;

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

        private BreakerController breaker;

        private void Start()
        {
            rightAnswers = 0;
            wrongAnswers = 0;

            if (CoreGameManager.Instance.sceneObject.levelTitle == "PIT")
            {
                PropagatedAudioManagerAnimator animator = FindObjectsOfType<PropagatedAudioManagerAnimator>().First(manager => manager.name == "JohnnyBase");
                animator.enabled = false;
                Animator mouthAnim = animator.GetComponentInChildren<Animator>();
                mouthAnim.enabled = false;
            }
            if (!floors.Any(floor => CoreGameManager.Instance.sceneObject.levelTitle == floor))
                Destroy(gameObject);

            poweroutageTime = 60 * poweroutageMinutes;
            breaker = BreakerController.allBreakers[0];
        }

        private float poweroutageTime;
        private void Update()
        {
            if (!ec.Players[0] || !FindObjectOfType<MainGameManager>()) return;

            if (!baldiDestroyed)
            {
                HappyBaldi baldi = FindObjectOfType<HappyBaldi>();
                if (baldi)
                {
                    baldi.gameObject.SetActive(false);
                    baldiDestroyed = true;
                }
            }

            int neededNotebooks = CoreGameManager.Instance.sceneObject.levelTitle == "F1" ? 3 : CoreGameManager.Instance.sceneObject.levelTitle == "F2" ? 2 : 1;
            if (!spawned && (MainGameManager.Instance.FoundNotebooks >= neededNotebooks || wrongAnswers > 0))
            {
                spawned = true;

                List<RoomController> rooms = new List<RoomController>();
                foreach (RoomController room in ec.rooms)
                    if (room.category == RoomCategory.Class)
                        rooms.Add(room);

                NPC newCarlos = CarlosBasePlugin.assetManager.Get<NPC>("Carlos");
                NPC carlosInstance = ec.SpawnNPC(newCarlos, rooms[Random.Range(1, rooms.Count)].RandomEntitySafeCellNoGarbage().position);

                carlos = carlosInstance;

                MusicManager.Instance.StopMidi();

                GameObject carlosGhostManagerObject = new GameObject("CarlosGhostManager");
                CarlosGhostManager carlosGhostManager = carlosGhostManagerObject.AddComponent<CarlosGhostManager>();
                carlosGhostManager.ec = ec;
            }

            if (CoreGameManager.Instance.sceneObject.levelObject == null) return;

            if (breaker && poweroutageTime <= 0 && !chasing)
            {
                poweroutageTime = 60 * poweroutageMinutes;
                breaker.Invoke("BlowFuse", 0);
            }
            else
                poweroutageTime -= Time.deltaTime * ec.EnvironmentTimeScale;
        }
    }
}