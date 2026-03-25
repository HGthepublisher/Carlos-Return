using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CarlosReturn
{
    public class CarlosGhostManager : MonoBehaviour
    {
        public static Color carlosGhostColor = new Color32(36, 57, 158, 255);

        public EnvironmentController ec;

        public static bool fuseBlown = false;

        private void Start()
        {
            List<Cell> rooms = ec.AllTilesNoGarbage(false, true);
            Debug.Log(rooms.Count);

            NPC carlosGhost = CarlosBasePlugin.assetManager.Get<NPC>("CarlosGhost");
            int numberOfGhosts = CoreGameManager.Instance.sceneObject.levelTitle == "F1" ? 5 : CoreGameManager.Instance.sceneObject.levelTitle == "F2" ? 7 : 8;
            for (int i = 1; i <= numberOfGhosts; i++)
                ec.SpawnNPC(carlosGhost, rooms[Random.Range(1, rooms.Count)].position);
        }

        private FieldInfo breakerInfo;
        private void Update()
        {
            if (breakerInfo == null)
                breakerInfo = AccessTools.Field(typeof(BreakerController), "fuseBlown");

            fuseBlown = (bool)breakerInfo.GetValue(null);
        }
    }
}