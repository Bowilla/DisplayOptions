using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BBModMenu;
using MelonLoader;
using UnityEngine;
using UnityEngine.UIElements;

namespace DisplayOptions
{
    public class DisplayOptionsMod : MelonMod
    {
        private int[] resolution = new int[2];
        private int[] custom_resolution = new int[2];
        private bool fullscreen;
        private string snapKey;

        public override void OnLateInitializeMelon()
        {
            MelonLogger.Msg("DisplayOptions starting to load.");

            GameObject gameUI = GameObject.Find("GameUI");
            GameUI _gameUI = gameUI.GetComponent<GameUI>();
            List<UIScreen> screens = typeof(GameUI)?.GetField("screens", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(_gameUI) as List<UIScreen>;

            ModMenu _modMenu = screens?.FirstOrDefault(screen => screen is ModMenu) as ModMenu;
            if (_modMenu is null)
            {
                MelonLogger.Msg("ModMenu not found");
                return;
            }

            string categoryName = "Display";
            var displayOptions = _modMenu.AddSetting(categoryName);

            var vsyncToggle = _modMenu.CreateToggle(categoryName, "VSync", false);
            vsyncToggle.RegisterValueChangedCallback(delegate (ChangeEvent<bool> b)
            {
                QualitySettings.vSyncCount = (b.newValue) ? 1 : 0;
            });
            var fullscreenToggle = _modMenu.CreateToggle(categoryName, "Fullscreen", Screen.fullScreen);
            fullscreenToggle.RegisterValueChangedCallback(delegate (ChangeEvent<bool> b)
            {
                fullscreen = b.newValue;
            });


            // Resolution sliders
            var widthSlider = _modMenu.CreateSlider(categoryName, "Width", 160, 3840, Screen.width, true);
            widthSlider.RegisterValueChangedCallback(delegate (ChangeEvent<float> f)
            {
                custom_resolution[0] = (int)f.newValue;
            });
            var heightSlider = _modMenu.CreateSlider(categoryName, "Height", 120, 2160, Screen.height, true);
            heightSlider.RegisterValueChangedCallback(delegate (ChangeEvent<float> f)
            {
                custom_resolution[1] = (int)f.newValue;
            });


            var settingsGroup = _modMenu.CreateGroup("Settings");

            var widthWrapper = _modMenu.CreateWrapper();
            widthWrapper.Add(_modMenu.CreateLabel("Width"));
            widthWrapper.Add(widthSlider);

            var heightWrapper = _modMenu.CreateWrapper();
            heightWrapper.Add(_modMenu.CreateLabel("Height"));
            heightWrapper.Add(heightSlider);

            List<string> resolutionPresets = new List<string>()
            {
                "[8K UHD 16:9] 7680x4320",
                "[4K UHD 16:9] 3840x2160",
                "[WQXGA 16:10] 2560x1600",
                "[QHD 16:9] 2560x1440",
                "[QXGA 4:3] 2048x1536",
                "[WUXGA 16:10] 1920x1200",
                "[FHD 16:9] 1920x1080",
                "[WSXGA+ 16:10] 1680x1050",
                "[UXGA 4:3] 1600x1200",
                "[HD+ 16:9] 1600x900",
                "[WXGA+ 16:10] 1440x900",
                "[HD 16:9] 1366x768",
                "[WXGA 16:10] 1280x800",
                "[WXGA 16:9] 1280x720",
                "[XGA 4:3] 1024x768",
                "[SVGA 4:3] 800x600",
                "[VGA 4:3] 640x480",
                "[nHD 16:9] 640x360",
                "Custom Resolution",
            };
            Dictionary<string, int[]> resolutionValues = new Dictionary<string, int[]>
            {
                { "[8K UHD 16:9] 7680x4320", new int[2]{ 7680, 4320 } },
                { "[4K UHD 16:9] 3840x2160", new int[2]{ 3840, 2160 } },
                { "[WQXGA 16:10] 2560x1600", new int[2]{ 2560, 1600 } },
                { "[QHD 16:9] 2560x1440", new int[2]{ 2560, 1440 } },
                { "[QXGA 4:3] 2048x1536", new int[2]{ 2048, 1536 } },
                { "[WUXGA 16:10] 1920x1200", new int[2]{ 1920, 1200 } },
                { "[FHD 16:9] 1920x1080", new int[2]{ 1920, 1080 } },
                { "[WSXGA+ 16:10] 1680x1050", new int[2]{ 1680, 1050 } },
                { "[UXGA 4:3] 1600x1200", new int[2]{ 1600, 1200 } },
                { "[HD+ 16:9] 1600x900", new int[2]{ 1600, 900 } },
                { "[WXGA+ 16:10] 1440x900", new int[2]{ 1440, 900 } },
                { "[HD 16:9] 1366x768", new int[2]{ 1366, 768 } },
                { "[WXGA 16:10] 1280x800", new int[2]{ 1280, 800 } },
                { "[WXGA 16:9] 1280x720", new int[2]{ 1280, 720 } },
                { "[XGA 4:3] 1024x768", new int[2]{ 1024, 768 } },
                { "[SVGA 4:3] 800x600", new int[2]{ 800, 600 } },
                { "[VGA 4:3] 640x480", new int[2]{ 640, 480 } },
                { "[nHD 16:9] 640x360", new int[2]{ 640, 360 } },
                { "Custom Resolution", new int[2]{ (int)widthSlider.value, (int)heightSlider.value } },
            };
            var resolutionPreset = _modMenu.CreateCarousel(categoryName, "ResolutionPresets", resolutionPresets, (_key) =>
            {
                MelonLogger.Msg("Resolution preset changed to " + _key);
                resolutionValues.TryGetValue(_key, out int[] val);
                resolution[0] = val[0];
                resolution[1] = val[1];

                if (_key == "Custom Resolution")
                {
                    resolution = custom_resolution = val;
                    settingsGroup.Add(widthWrapper);
                    settingsGroup.Add(heightWrapper);
                }
                else
                {
                    settingsGroup.Remove(widthWrapper);
                    settingsGroup.Remove(heightWrapper);
                }

            }, "[FHD 16:9] 1920x1080");

            // Resolution snapping hotkey
            var key = _modMenu.CreateHotKey(categoryName, "ResolutionSnapHotkey", KeyCode.F11);
            snapKey = key.Value;
            key.OnChanged += newKey =>
            {
                MelonLogger.Msg($"Resolution Snap Hotkey : {newKey}");
                snapKey = newKey;
            };

            // Apply Settings Button
            var applyBtn = _modMenu.CreateButton("Apply");
            applyBtn.clicked += Apply;



            var vsyncWrapper = _modMenu.CreateWrapper();
            vsyncWrapper.Add(_modMenu.CreateLabel("Enable VSync"));
            vsyncWrapper.Add(vsyncToggle);

            var fullscreenToggleWrapper = _modMenu.CreateWrapper();
            fullscreenToggleWrapper.Add(_modMenu.CreateLabel("Enable Fullscreen"));
            fullscreenToggleWrapper.Add(fullscreenToggle);

            var resolutionPresetWrapper = _modMenu.CreateWrapper();
            resolutionPresetWrapper.Add(_modMenu.CreateLabel("Resolution Presets"));
            resolutionPresetWrapper.Add(resolutionPreset.Root);


            var applyGroup = _modMenu.CreateGroup("Apply");

            var applyWrapper = _modMenu.CreateWrapper();
            applyWrapper.Add(_modMenu.CreateLabel("Apply Resolution Settings"));
            applyWrapper.Add(applyBtn);

            var keyWrapper = _modMenu.CreateWrapper();
            keyWrapper.Add(_modMenu.CreateLabel("Resolution Snap Hotkey"));
            keyWrapper.Add(key.Root);

            settingsGroup.Add(vsyncWrapper);
            settingsGroup.Add(fullscreenToggleWrapper);
            settingsGroup.Add(resolutionPresetWrapper);

            applyGroup.Add(applyWrapper);
            applyGroup.Add(keyWrapper);

            displayOptions.Add(settingsGroup);
            displayOptions.Add(applyGroup);


            resolutionValues.TryGetValue(resolutionPreset.Value, out int[] _val);
            resolution[0] = _val[0];
            resolution[1] = _val[1];

            if (resolutionPreset.Value == "Custom Resolution")
            {
                custom_resolution = _val;
                settingsGroup.Add(widthWrapper);
                settingsGroup.Add(heightWrapper);
            }

            fullscreen = fullscreenToggle.value;

            Apply();
            QualitySettings.vSyncCount = (vsyncToggle.value) ? 1 : 0;
        }

        private void Apply()
        {
            string f = fullscreen ? "fullscreen" : "windowed";
            MelonLogger.Msg($"Resolution: {resolution[0]}x{resolution[1]} {f}");
            Screen.SetResolution(resolution[0], resolution[1], fullscreen);
        }

        public override void OnUpdate()
        {
            if (Utils.IsHotkeyPressed(snapKey))
            {
                Apply();
            }
        }

    }
}
