using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CarlosReturn
{
    [HarmonyPatch(typeof(EnvironmentController), "BuildNavMesh")]
    internal static class EnvironmentControllerPatch
    {
        [HarmonyPrefix]
        private static void BuildNavMeshPatch(EnvironmentController __instance)
        {
            GameObject carManagerObject = new GameObject("CarlosManager");
            CarlosManager carManager = carManagerObject.AddComponent<CarlosManager>();
            carManager.ec = __instance;

            GameObject carAmbienceManagerObject = new GameObject("CarlosAmbienceManager");
            CarlosAmbienceManager carAmbienceManager = carAmbienceManagerObject.AddComponent<CarlosAmbienceManager>();
            carAmbienceManager.ec = __instance;
        }

        [HarmonyPatch(typeof(MusicManager))]
        internal static class MusicManagerPatch
        {
            private static CarlosMusicManager audioManager;

            [HarmonyPatch("PlayMidi", new Type[] { typeof(string), typeof(bool) })]
            [HarmonyPrefix]
            private static bool PlayMidiPatch(string song)
            {
                if (audioManager == null)
                {
                    GameObject obj = new GameObject("Carlos Music Manager");
                    GameObject.DontDestroyOnLoad(obj);
                    audioManager = obj.AddComponent<CarlosMusicManager>();
                }

                switch (song)
                {
                    case "titleFixed":
                        EnvironmentController ec = GameObject.FindObjectOfType<EnvironmentController>();
                        if (ec == null)
                            audioManager.PlaySound("car_music_title", false);
                        break;
                    case "Elevator":
                        audioManager.PlaySound("car_music_elevator");
                        break;
                    case "school":
                        if (!CarlosBasePlugin.hardMode.Value)
                            audioManager.PlaySound("car_music_school");
                        break;
                }

                return false;
            }

            [HarmonyPatch("StopMidi")]
            [HarmonyPrefix]
            private static void StopMidiPatch()
            {
                if (audioManager)
                    audioManager.StopSound();
            }
        }

        [HarmonyPatch(typeof(BaldiTV))]
        internal static class BaldiTVPatch
        {
            [HarmonyPatch("QueueEnumerator")]
            [HarmonyPrefix]
            private static bool QueueEnumeratorPatch()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(Activity), "Completed", new Type[] { typeof(int), typeof(bool) })]
        internal static class ActivityPatch
        {
            [HarmonyPrefix]
            private static void CompletedPatch(bool correct)
            {
                if (correct)
                    CarlosManager.rightAnswers++;
                else
                    CarlosManager.wrongAnswers++;
            }
        }

        [HarmonyPatch(typeof(BreakerController), "Initialize")]
        internal static class BreakerControllerPatch
        {
            [HarmonyPostfix]
            private static void InitializePatch(BreakerController __instance)
            {
                FieldInfo info = typeof(BreakerController).GetField("maxPowered", BindingFlags.NonPublic | BindingFlags.Instance);
                info?.SetValue(__instance, CarlosBasePlugin.hardMode.Value ? 2 : 5);
            }
        }
        [HarmonyPatch(typeof(PowerLeverGauge), "Initialize")]
        internal static class PowerLeverGaugePatch
        {
            [HarmonyPostfix]
            private static void InitializePatch(PowerLeverGauge __instance)
            {
                FieldInfo leverInfo = typeof(PowerLeverGauge).GetField("maxLevers", BindingFlags.NonPublic | BindingFlags.Instance);
                leverInfo.SetValue(__instance, CarlosBasePlugin.hardMode.Value ? 2 : 5);
                FieldInfo speedInfo = typeof(PowerLeverGauge).GetField("gaugeSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
                speedInfo?.SetValue(__instance, 4);
            }
        }

        [HarmonyPatch(typeof(SceneTimer), "Start")]
        internal static class SceneTimerPatch
        {
            [HarmonyPostfix]
            private static void StartPatch(SceneTimer __instance)
            {
                string path = Path.Combine(Application.streamingAssetsPath, "Modded/" + CarlosModInfo.modPath + "/Textures/Carlos/car_splash.png");
                byte[] data = File.ReadAllBytes(path);

                RawImage image = __instance.GetComponentInChildren<RawImage>();
                if (!image || data.LongLength <= 0) return;

                Texture2D texture = (Texture2D)image.texture;
                if (texture.name == "BasicallyGames_Logo_Color_2019")
                    texture.LoadImage(data);
            }
        }

        [HarmonyPatch(typeof(AudioManager), "PlaySingle", new Type[] { typeof(SoundObject), typeof(float) })]
        internal class AudioManagerPatch
        {
            [HarmonyPrefix]
            private static void PlaySinglePatch(ref SoundObject file)
            {
                AudioClip clip = file.soundClip;
                if (!clip) return;
                if (clip.name == "NotebookCollect")
                    file = CarlosBasePlugin.audioclips[0];
                else if (clip.name == "Activity_Correct")
                    file = CarlosBasePlugin.audioclips[1];
                else if (clip.name == "Activity_Incorrect")
                    file = CarlosBasePlugin.audioclips[2];
                else if (clip.name == "Doors_StandardOpen")
                    file = CarlosBasePlugin.audioclips[3];
                else if (clip.name == "Doors_StandardShut")
                    file = CarlosBasePlugin.audioclips[4];
                else if (clip.name == "Doors_Swinging")
                    file = CarlosBasePlugin.audioclips[5];
                else if (clip.name == "PowerAlarm_Raw")
                    file = CarlosBasePlugin.audioclips[6];
                else if (clip.name == "PowerAlarm_Reverb")
                    file = CarlosBasePlugin.audioclips[7];
                else if (clip.name == "Elv_Buzz")
                    file = CarlosBasePlugin.audioclips[8];
            }
        }

        [HarmonyPatch(typeof(SubtitleManager), "CreateSub")]
        internal class SubtitleManagerPatch
        {
            [HarmonyPrefix]
            private static bool CreateSubPatch()
            {
                return !CarlosBasePlugin.hardMode.Value;
            }
        }

        [HarmonyPatch(typeof(NameManager), "Awake")]
        internal class NameManagerPatch
        {
            [HarmonyPrefix]
            private static void AwakePatch(NameManager __instance)
            {
                FieldInfo audSource = typeof(NameManager).GetField("audSource", BindingFlags.NonPublic | BindingFlags.Instance);
                if (audSource == null) return;

                AudioSource source = (AudioSource)audSource.GetValue(__instance);
                source.volume = 0;

                GameObject mouth = __instance.transform.parent.GetComponentsInChildren<Animator>().First(i => i.name == "Mouth").gameObject;
                mouth.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(AmbienceRoomFunction))]
        internal class AmbienceRoomFunctionPatch
        {
            [HarmonyPatch("OnPlayerEnter")]
            [HarmonyPrefix]
            private static bool OnPlayerEnterPatch()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(AudioSourceManager), "CopyExistingAudioSource")]
        internal class AudioSourceManagerPatch
        {
            [HarmonyPrefix]
            private static bool CopyExistingAudioSourcePatch(AudioSource source)
            {
                if (!source)
                    return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(CraneController), "EntityTriggerEnter")]
        internal class CraneControllerPatch
        {
            [HarmonyPrefix]
            private static bool EntityTriggerEnterPatch(Entity otherEntity)
            {
                if (otherEntity.GetComponent<CarlosGhost>())
                    return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(TapePlayer), "Cooldown")]
        internal class TapePlayerPatch
        {
            private static IEnumerator Cooldown(TapePlayer instance)
            {
                FieldInfo dijkstraMapInfo = typeof(TapePlayer).GetField("dijkstraMap", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo ecInfo = typeof(TapePlayer).GetField("ec", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo fleeStatesInfo = typeof(TapePlayer).GetField("fleeStates", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo audManInfo = typeof(TapePlayer).GetField("audMan", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo activeInfo = typeof(TapePlayer).GetField("active", BindingFlags.NonPublic | BindingFlags.Instance);
                if (dijkstraMapInfo == null || fleeStatesInfo == null || ecInfo == null || audManInfo == null || activeInfo == null) yield break;

                DijkstraMap dijkstraMap = (DijkstraMap)dijkstraMapInfo.GetValue(instance);
                EnvironmentController ec = (EnvironmentController)ecInfo.GetValue(instance);
                List<NavigationState_WanderFleeOverride> fleeStates = (List<NavigationState_WanderFleeOverride>)fleeStatesInfo.GetValue(instance);
                AudioManager audMan = (AudioManager)audManInfo.GetValue(instance);

                while (dijkstraMap.PendingUpdate)
                    yield return null;

                foreach (NPC npc in ec.Npcs)
                    if (npc.Navigator.enabled && (!npc.Entity.GetComponent<CarlosGhost>()))
                    {
                        NavigationState_WanderFleeOverride navigationState_WanderFleeOverride = new NavigationState_WanderFleeOverride(npc, 31, dijkstraMap);
                        fleeStates.Add(navigationState_WanderFleeOverride);
                        npc.navigationStateMachine.ChangeState(navigationState_WanderFleeOverride);
                    }

                while (audMan.QueuedAudioIsPlaying)
                    yield return null;

                foreach (NavigationState_WanderFleeOverride fleeState in fleeStates)
                    if (fleeState != null && fleeState.npc != null)
                        fleeState.End();

                dijkstraMap.Deactivate();
                fleeStates.Clear();
                activeInfo.SetValue(instance, false);
            }

            [HarmonyPrefix]
            private static bool CooldownPatch(TapePlayer __instance, ref IEnumerator __result)
            {
                __result = Cooldown(__instance);
                return false;
            }
        }
    }
}