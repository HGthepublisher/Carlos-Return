using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarlosReturn
{
    [BepInPlugin(CarlosModInfo.modPath, CarlosModInfo.modName, CarlosModInfo.modVersion)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    public class CarlosBasePlugin : BaseUnityPlugin
    {
        public static CarlosBasePlugin instance;

        public static AssetManager assetManager = new AssetManager();

        private readonly ModAsset[] assets = new ModAsset[]
        {
            new ModAsset() {assetName = "carlos_happy", assetPath = "Textures/Carlos/"},
            new ModAsset() {assetName = "carlos_neutral", assetPath = "Textures/Carlos/", assetAmount = 3},
            new ModAsset() {assetName = "carlos_unhappy", assetPath = "Textures/Carlos/"},
            new ModAsset() {assetName = "carlos_mad", assetPath = "Textures/Carlos/"},
            new ModAsset() {assetName = "carlosghost_", assetPath = "Textures/CarlosGhost/", assetAmount = 8, imgPPU = 37},

            new ModAsset() {assetName = "pri_carlos", assetPath = "Textures/Carlos/", assetType = AssetType.Texture},
            new ModAsset() {assetName = "car_poster", assetPath = "Textures/Posters/", assetType = AssetType.Texture, assetAmount = 15},
            new ModAsset() {assetName = "car_wall", assetPath = "Textures/Halls/", assetType = AssetType.Texture, assetAmount = 3},
            new ModAsset() {assetName = "car_floor", assetPath = "Textures/Halls/", assetType = AssetType.Texture, assetAmount = 2},
            new ModAsset() {assetName = "car_ceiling", assetPath = "Textures/Halls/", assetType = AssetType.Texture, assetAmount = 2},

            new ModAsset() {assetName = "car_music_title", assetPath = "Sounds/Music/", assetType = AssetType.Audio, soundType = SoundType.Music},
            new ModAsset() {assetName = "car_music_elevator", assetPath = "Sounds/Music/", assetType = AssetType.Audio, soundType = SoundType.Music},
            new ModAsset() {assetName = "car_music_school", assetPath = "Sounds/Music/", assetType = AssetType.Audio, soundType = SoundType.Music},
            new ModAsset() {assetName = "car_ambience", assetPath = "Sounds/Music/", assetType = AssetType.Audio, soundType = SoundType.Music},
            new ModAsset() {assetName = "car_angry", assetPath = "Sounds/Music/", assetType = AssetType.Audio, soundType = SoundType.Music},

            new ModAsset() {assetName = "carlos_spawn", assetPath = "Sounds/Carlos/", assetType = AssetType.Audio, subtitle = "[BANG]"},
            new ModAsset() {assetName = "carlos_see", assetPath = "Sounds/Carlos/", assetType = AssetType.Audio, soundType = SoundType.Voice, subtitle = "!!!!!"},
            new ModAsset() {assetName = "carlos_lost", assetPath = "Sounds/Carlos/", assetType = AssetType.Audio, soundType = SoundType.Voice, subtitle = "..... ???"},
            new ModAsset() {assetName = "carlos_notice", assetPath = "Sounds/Carlos/", assetType = AssetType.Audio, soundType = SoundType.Voice, subtitle = ". . ."},
            new ModAsset() {assetName = "carlos_warning", assetPath = "Sounds/Carlos/", assetType = AssetType.Audio, soundType = SoundType.Voice, subtitles = new string[] {"?", "...", "!!!"}, assetAmount = 3},
            new ModAsset() {assetName = "carlos_angry", assetPath = "Sounds/Carlos/", assetType = AssetType.Audio, soundType = SoundType.Voice, subtitle = "*ANGRY SCREAMING*"},
            new ModAsset() {assetName = "carlosghost_ambience_low", assetPath = "Sounds/Carlos/", assetType = AssetType.Audio, soundType = SoundType.Voice, subtitle = "*Humming*"},
            new ModAsset() {assetName = "carlosghost_ambience", assetPath = "Sounds/Carlos/", assetType = AssetType.Audio, soundType = SoundType.Voice, subtitle = "*HUMMING*"},

            new ModAsset() {assetName = "car_ambient", assetPath = "Sounds/Ambience/", assetType = AssetType.Audio, assetAmount = 8},

            new ModAsset() {assetName = "car_notebook_collect", assetPath = "Sounds/Effects/", assetType = AssetType.Audio},
            new ModAsset() {assetName = "car_act_correct", assetPath = "Sounds/Effects/", assetType = AssetType.Audio, subtitle = ":)", color = Color.white},
            new ModAsset() {assetName = "car_act_incorrect", assetPath = "Sounds/Effects/", assetType = AssetType.Audio, subtitle = ">:(", color = new Color32(216, 12, 14, 255)},
            new ModAsset() {assetName = "car_door_open", assetPath = "Sounds/Effects/", assetType = AssetType.Audio, subtitle = "[Creak]", color = Color.white},
            new ModAsset() {assetName = "car_door_shut", assetPath = "Sounds/Effects/", assetType = AssetType.Audio, subtitle = "[Slam]", color = Color.white},
            new ModAsset() {assetName = "car_door_swing", assetPath = "Sounds/Effects/", assetType = AssetType.Audio, subtitle = "[SSwwiinngg]", color = Color.white},
            new ModAsset() {assetName = "car_alarm", assetPath = "Sounds/Effects/", assetType = AssetType.Audio, subtitle = "[!!!]", color = Color.red},
            new ModAsset() {assetName = "car_alarm_reverb", assetPath = "Sounds/Effects/", assetType = AssetType.Audio, subtitle = "[!!!]", color = Color.red},
            new ModAsset() {assetName = "car_buzz", assetPath = "Sounds/Effects/", assetType = AssetType.Audio, subtitle = ":)", color = Color.grey},
        };

        public static List<SoundObject> audioclips = new List<SoundObject>();

        public static ConfigEntry<bool> debug;
        public static ConfigEntry<bool> hardMode;

        public void Awake()
        {
            Console.Title = "Carlos' Return - " + CarlosModInfo.modVersion + " | " + Console.Title;

            instance = this;
            Harmony carHarmony = new Harmony(CarlosModInfo.modPath);
            carHarmony.PatchAll();
            LoadAssets();

            LoadingEvents.RegisterOnAssetsLoaded(Info, RegisterAssets(), LoadingEventOrder.Start);
            GeneratorManagement.Register(this, GenerationModType.Preparation, ChangeFloorTypes);
            GeneratorManagement.Register(this, GenerationModType.Base, EditFloor);

            MTM101BaldiDevAPI.AddWarningScreen("This mod is NOT affiliated with Joseph.\nThanks for getting my mod! :D\n- HGThePublisher", false);
            MTM101BaldiDevAPI.AddWarningScreen("Credits to ConfusedSeagull for code inspiration, this is my first mod.\nThis mod was entirely developed from scratch in inspiration from GraysLand's videos.\n", false);
            MTM101BaldiDevAPI.AddWarningScreen("Audio is required and essential of this mod, either that or turn on captions.\n\nGood luck...\n- Carlos", false);

            debug = Config.Bind("Dev", "Debug", false, "Adds stuff for debug, unless you want to cheat.");
            hardMode = Config.Bind("Settings", "Impossible", false, "If this mod wasn't a challenge enough, try beating this.");
        }

        internal enum AssetType
        {
            Sprite,
            Audio,
            Texture,
        }
        internal class ModAsset
        {
            public string assetName = "asset";
            public AssetType assetType = AssetType.Sprite;
            public string assetPath = "";

            public int assetAmount = 1;

            public SoundType soundType = SoundType.Effect;
            public string subtitle = "";
            public string[] subtitles = null;
            public Color color = CarlosManager.carlosColor;

            public float imgPPU = 32;
        }

        private void LoadAssets()
        {
            void LoadAnAsset(ModAsset asset, int assetID = -1)
            {
                string assetIndex = assetID >= 0 ? assetID.ToString() : "";
                string assetSubtitle = asset.subtitles != null ? asset.subtitles[int.Parse(assetIndex) - 1] : asset.subtitle;

                switch (asset.assetType)
                {
                    default:
                        Sprite sprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, asset.assetPath + asset.assetName + assetIndex + ".png"), asset.imgPPU);
                        assetManager.Add(asset.assetName + assetIndex, sprite);
                        return;
                    case AssetType.Audio:
                        SoundObject soundObject = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, asset.assetPath + asset.assetName + assetIndex + ".wav"), assetSubtitle, asset.soundType, asset.color, -1);
                        soundObject.subtitle = assetSubtitle != "";
                        assetManager.Add(asset.assetName + assetIndex, soundObject);
                        return;
                    case AssetType.Texture:
                        Texture2D texture = AssetLoader.TextureFromMod(this, asset.assetPath + asset.assetName + assetIndex + ".png");
                        assetManager.Add(asset.assetName + assetIndex, texture);
                        return;
                }
            }

            foreach (ModAsset asset in assets)
            {
                if (asset.assetAmount > 1)
                    for (int i = 1; i <= asset.assetAmount; i++)
                        LoadAnAsset(asset, i);
                else
                    LoadAnAsset(asset);
            }   
        }

        private IEnumerator RegisterAssets()
        {
            yield return 1;
            yield return "Carlos' Return";

            Carlos carlos = new NPCBuilder<Carlos>(Info)
                .SetName("Carlos")
                .SetEnum("carlos")
                .SetMinMaxAudioDistance(12, 620)
                .AddMetaFlag(NPCFlags.StandardAndHear)
                .AddSpawnableRoomCategories(RoomCategory.Class)
                .AddLooker()
                .AddTrigger()
                .SetPoster(ObjectCreators.CreateCharacterPoster(assetManager.Get<Texture2D>("pri_carlos"), "Carlos", "He's just a sub for Baldi, but there's a few issues.\nDon't make him too mad.\nHe doesn't like visitors."))
                .Build();

            carlos.carAudio = carlos.GetComponent<AudioManager>();
            carlos.carlosHappy = assetManager.Get<Sprite>("carlos_happy");
            carlos.carlosNeutral = new Sprite[] 
            {
                assetManager.Get<Sprite>("carlos_neutral1"),
                assetManager.Get<Sprite>("carlos_neutral2"),
                assetManager.Get<Sprite>("carlos_neutral3"),
            };
            carlos.carlosUnhappy = assetManager.Get<Sprite>("carlos_unhappy");
            carlos.carlosMad = assetManager.Get<Sprite>("carlos_mad");

            carlos.carSpawn = assetManager.Get<SoundObject>("carlos_spawn");
            carlos.carSighted = assetManager.Get<SoundObject>("carlos_see");
            carlos.carCantFind = assetManager.Get<SoundObject>("carlos_lost");
            carlos.carNotice = assetManager.Get<SoundObject>("carlos_notice");
            carlos.carAngry = assetManager.Get<SoundObject>("carlos_angry");

            for (int i = 1; i <= assets.First(asset => asset.assetName == "carlos_warning").assetAmount; i++)
                carlos.carWarnings.Add(assetManager.Get<SoundObject>("carlos_warning" + i));

            carlos.spriteRenderer[0].sprite = carlos.carlosHappy;

            assetManager.Add("Carlos", carlos);

            CarlosGhost carlosGhost = new NPCBuilder<CarlosGhost>(Info)
                .SetName("CarlosGhost")
                .SetEnum("carlosghost")
                .SetWanderEnterRooms()
                .SetMinMaxAudioDistance(10, 200)
                .AddMetaFlag(NPCFlags.CanMove | NPCFlags.CanSee | NPCFlags.MakeNoise | NPCFlags.HasTrigger | NPCFlags.HasSprite)
                .AddSpawnableRoomCategories(RoomCategory.Hall)
                .SetFOV(60)
                .IgnorePlayerVisibility()
                .AddLooker()
                .AddTrigger()
                .Build();

            carlosGhost.ghostAudio = carlosGhost.GetComponent<AudioManager>();
            carlosGhost.ghostSoundLow = assetManager.Get<SoundObject>("carlosghost_ambience_low");
            carlosGhost.ghostSoundHigh = assetManager.Get<SoundObject>("carlosghost_ambience");

            List<Sprite> spriteRotations = new List<Sprite>()
            {
                assetManager.Get<Sprite>("carlosghost_7"),
                assetManager.Get<Sprite>("carlosghost_8"),
                assetManager.Get<Sprite>("carlosghost_1"),
                assetManager.Get<Sprite>("carlosghost_2"),
                assetManager.Get<Sprite>("carlosghost_3"),
                assetManager.Get<Sprite>("carlosghost_4"),
                assetManager.Get<Sprite>("carlosghost_5"),
                assetManager.Get<Sprite>("carlosghost_6"),
            };
            carlosGhost.ghostSprites = spriteRotations;

            assetManager.Add("CarlosGhost", carlosGhost);

            for (int i = 1; i <= assets.First(asset => asset.assetName == "car_ambient").assetAmount; i++)
                CarlosAmbienceManager.ambience.Add(assetManager.Get<SoundObject>("car_ambient" + i));

            audioclips.Add(assetManager.Get<SoundObject>("car_notebook_collect"));
            audioclips.Add(assetManager.Get<SoundObject>("car_act_correct"));
            audioclips.Add(assetManager.Get<SoundObject>("car_act_incorrect"));
            audioclips.Add(assetManager.Get<SoundObject>("car_door_open"));
            audioclips.Add(assetManager.Get<SoundObject>("car_door_shut"));
            audioclips.Add(assetManager.Get<SoundObject>("car_door_swing"));
            audioclips.Add(assetManager.Get<SoundObject>("car_alarm"));
            audioclips.Add(assetManager.Get<SoundObject>("car_alarm_reverb"));
            audioclips.Add(assetManager.Get<SoundObject>("car_buzz"));
        }

        private bool posterOnce = false;
        private void EditFloor(string floorName, int floorNumber, SceneObject scene)
        {
            if (!floorName.StartsWith("F")) return;

            if (floorNumber >= 3)
                return;

            scene.skyboxColor = Color.black;
            scene.levelObject.mapPrice = 0;

            List<PosterObject> posterObjects = new List<PosterObject>();
            List<WeightedPosterObject> posters = new List<WeightedPosterObject>();
            for (int i = 1; i <= assets.First(asset => asset.assetName == "car_poster").assetAmount; i++)
                posterObjects.Add(new PosterObject()
                {
                    name = "Carlos Poster " + i,
                    baseTexture = assetManager.Get<Texture2D>("car_poster" + i)
                });
        
            foreach (PosterObject poster in posterObjects)
                posters.Add(new WeightedPosterObject()
                {
                    selection = poster,
                    weight = 1000,
                });

            scene.levelObject.posterChance = 5;
            scene.levelObject.posters = scene.levelObject.posters.AddRangeToArray(posters.ToArray());

            if (!posterOnce)
                scene.forcedNpcs = scene.forcedNpcs.AddRangeToArray(new NPC[] { assetManager.Get<NPC>("Carlos") });
            posterOnce = true;
            scene.potentialNPCs = new List<WeightedNPC>();
            scene.additionalNPCs = 0;

            WeightedTexture2D wallTexture1 = new WeightedTexture2D() { selection = assetManager.Get<Texture2D>("car_wall1"), weight = 0 };
            WeightedTexture2D wallTexture2 = new WeightedTexture2D() { selection = assetManager.Get<Texture2D>("car_wall2"), weight = 0 };
            WeightedTexture2D wallTexture3 = new WeightedTexture2D() { selection = assetManager.Get<Texture2D>("car_wall3"), weight = 0 };
            WeightedTexture2D floorTexture1 = new WeightedTexture2D() { selection = assetManager.Get<Texture2D>("car_floor1"), weight = 0 };
            WeightedTexture2D floorTexture2 = new WeightedTexture2D() { selection = assetManager.Get<Texture2D>("car_floor2"), weight = 0 };
            WeightedTexture2D ceilingTexture1 = new WeightedTexture2D() { selection = assetManager.Get<Texture2D>("car_ceiling1"), weight = 0 };
            WeightedTexture2D ceilingTexture2 = new WeightedTexture2D() { selection = assetManager.Get<Texture2D>("car_ceiling2"), weight = 0 };

            scene.levelObject.hallWallTexs = new WeightedTexture2D[] { floorNumber >= 2 ? wallTexture3 : floorNumber == 1 ? wallTexture2 : wallTexture1 };
            scene.levelObject.hallFloorTexs = new WeightedTexture2D[] { floorNumber >= 2 ? floorTexture1 : floorNumber == 1 ? floorTexture2 : floorTexture2 };
            scene.levelObject.hallCeilingTexs = new WeightedTexture2D[] { floorNumber >= 2 ? ceilingTexture2 : floorNumber == 1 ? ceilingTexture2 : ceilingTexture1 };

            if (hardMode.Value)
                scene.mapPrice = 999999;
            else
                scene.mapPrice = 1;

            if (floorNumber >= 2)
            {
                SceneObject endingScene = Resources.FindObjectsOfTypeAll<SceneObject>().First(scene => scene.name == "PlaceholderEnding");
                scene.nextLevel = endingScene;
                scene.levelObject.finalLevel = true;
            }

            scene.levelObject.potentialStructures = new WeightedStructureWithParameters[] { };

            StructureWithParameters[] structures = new StructureWithParameters[0];
            if (!hardMode.Value)
            {
                structures = new StructureWithParameters[]
                {
                    Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Maintenance_Lvl4").forcedStructures.First(item => item.prefab.name == "PowerLeverConstructor"),
                    Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Schoolhouse_Lvl2").forcedStructures.First(item => item.prefab.name == "Structure_Lockers"),
                    Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Schoolhouse_Lvl2").forcedStructures.First(item => item.prefab.name == "Structure_EnvironmentObjectBuilder_Weighted"),
                };
                if (floorNumber == 1)
                    structures = structures.AddRangeToArray(new StructureWithParameters[]
                    {
                        Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Factory_Lvl4").forcedStructures.First(item => item.prefab.name == "ConveyorBeltConstructor"),
                        Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Factory_Lvl4").forcedStructures.First(item => item.prefab.name == "Rotohall_Structure"),
                        Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Factory_Lvl4").forcedStructures.First(item => item.prefab.name == "FactoryBoxConstructor"),
                        Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Factory_Lvl4").forcedStructures.First(item => item.prefab.name == "Structure_EnvironmentObjectBuilder_Weighted"),
                    });
                else if (floorNumber >= 2)
                    structures = structures.AddRangeToArray(new StructureWithParameters[]
                    {
                        Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Maintenance_Lvl5").forcedStructures.First(item => item.prefab.name == "Structure_Vent"),
                        Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Maintenance_Lvl5").forcedStructures.First(item => item.prefab.name == "Structure_EnvironmentObjectBuilder_Weighted"),
                    });
            }
            else
            {
                structures = new StructureWithParameters[]
                {
                    Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Maintenance_Lvl4").forcedStructures.First(item => item.prefab.name == "PowerLeverConstructor"),
                    Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Schoolhouse_Lvl2").forcedStructures.First(item => item.prefab.name == "Structure_Lockers"),
                };
                if (floorNumber == 1)
                    structures = structures.AddRangeToArray(new StructureWithParameters[]
                    {
                        Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Factory_Lvl4").forcedStructures.First(item => item.prefab.name == "ConveyorBeltConstructor"),
                        Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Factory_Lvl4").forcedStructures.First(item => item.prefab.name == "Rotohall_Structure"),
                        Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Factory_Lvl4").forcedStructures.First(item => item.prefab.name == "FactoryBoxConstructor"),
                    });
                else if (floorNumber >= 2)
                    structures = structures.AddRangeToArray(new StructureWithParameters[]
                    {
                        Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Maintenance_Lvl5").forcedStructures.First(item => item.prefab.name == "Structure_Vent"),
                    });
            }

            scene.levelObject.forcedStructures = structures;
        }

        private void ChangeFloorTypes(string floorName, int floorNumber, SceneObject scene)
        {
            if (!floorName.StartsWith("F")) return;

            if (floorNumber == 0)
            {
                scene.levelObject = Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Schoolhouse_Lvl2");
                scene.levelObject.standardLightStrength = 9;
                scene.levelObject.exitCount = 2;
            }
            else if (floorNumber == 1)
            {
                scene.levelObject = Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Factory_Lvl4");
                scene.levelObject.standardLightStrength = 8;
                scene.levelObject.exitCount = 3;
            }
            else if (floorNumber >= 2)
            {
                scene.levelObject = Resources.FindObjectsOfTypeAll<LevelObject>().First(level => level.name == "Maintenance_Lvl5");
                scene.levelObject.standardLightStrength = 7;
                scene.levelObject.exitCount = 4;
            }
        }
    }
}