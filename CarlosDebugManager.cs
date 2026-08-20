using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace CarlosReturn
{
    public class CarlosDebugManager : MonoBehaviour
    {
        public CarlosManager carlosManager;

        private bool keybinds = false;

        private void Start()
        {
            if (!carlosManager || !CarlosBasePlugin.debug.Value) Destroy(gameObject);

            keybinds = CarlosBasePlugin.debugKeybinds.Value;
        }

        private void Update()
        {
            if (keybinds)
            {
                if (Input.GetKeyDown(KeyCode.P) && carlosManager.breaker)
                {
                    FieldInfo fuseBlown = typeof(BreakerController).GetField("fuseBlown", BindingFlags.NonPublic | BindingFlags.Static);
                    if (fuseBlown != null)
                    {
                        if ((bool)fuseBlown.GetValue(null))
                        {
                            carlosManager.breaker.Invoke("Reboot", 0);
                            carlosManager.breaker.Invoke("ResetFuse", 0);
                        }
                        else
                            carlosManager.breaker.Invoke("BlowFuse", 0);
                    }
                }
                if (Input.GetKeyDown(KeyCode.O))
                    carlosManager.forceSpawn = true;
                if (Input.GetKeyDown(KeyCode.L) && CarlosManager.carlos)
                    CarlosManager.carlos.Entity.SetBlinded(!CarlosManager.carlos.Blinded);
                if (Input.GetKeyDown(KeyCode.K) && CarlosManager.carlos)
                    CarlosManager.carlos.Entity.SetFrozen(!CarlosManager.carlos.Entity.Frozen);
                if (Input.GetKeyDown(KeyCode.Semicolon))
                    CarlosAmbienceManager.delayTime = 0;
                if (Input.GetKeyDown(KeyCode.Slash) && MainGameManager.Instance)
                    if (MainGameManager.Instance && MainGameManager.Instance.GameMode == GameMode.HideAndSeek && MainGameManager.Instance.GameReady)
                        MainGameManager.Instance.LoadNextLevel();
                if (Input.GetKeyDown(KeyCode.Period))
                    CoreGameManager.Instance.AddPoints(500, 0, true);
            }
        }
    }
}