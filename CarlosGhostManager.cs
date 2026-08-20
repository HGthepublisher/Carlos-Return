using System.Collections.Generic;
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

            NPC carlosGhost = CarlosBasePlugin.assetManager.Get<NPC>("CarlosGhost");
            int numberOfGhosts = CoreGameManager.Instance.sceneObject.levelTitle == "F1" ? 5 : CoreGameManager.Instance.sceneObject.levelTitle == "F2" ? 7 : 8;
            if (CarlosBasePlugin.impossible.Value)
                numberOfGhosts = (int)(numberOfGhosts * 1.5f);

            for (int i = 1; i <= numberOfGhosts; i++)
                ec.SpawnNPC(carlosGhost, rooms[Random.Range(1, rooms.Count)].position);
        }

        private void Update()
        {
            FieldInfo blown = typeof(BreakerController).GetField("fuseBlown", BindingFlags.NonPublic | BindingFlags.Static);
            if (blown != null)
                fuseBlown = (bool)blown.GetValue(null);
        }
    }
}