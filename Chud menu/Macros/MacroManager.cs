using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Chud.Classes;
using Chud.UI;
using GorillaLocomotion;
using GTAG_NotificationLib;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Chud.Backend
{
    public static class MacroManager
    {
        public const float MacroStep = 0.05f;

        public static bool RecordingMacro = false;

        private enum ActiveRecorder { None, Self, Gun }
        private static ActiveRecorder _activeRecorder = ActiveRecorder.None;

        public static bool DeleteMode = false;

        public static bool LoopMacros = false;

        private static bool _isPlayingMacro = false;

        private static bool _playInputWasHeld = false;

        private static readonly Dictionary<string, ChudMacro> _macros =
            new Dictionary<string, ChudMacro>(StringComparer.OrdinalIgnoreCase);

        private class PlaybackState
        {
            public ChudMacro Macro;
            public MacroGhostPreview PreviewRig;
        }

        private static readonly Dictionary<string, PlaybackState> _enabledMacros =
            new Dictionary<string, PlaybackState>(StringComparer.OrdinalIgnoreCase);

        private static readonly List<ChudRigTransform> _selfRecordingData = new List<ChudRigTransform>();
        private static MacroFakeRig _selfFakeRig;
        private static float _selfLastTime;

        private static readonly List<ChudRigTransform> _gunRecordingData = new List<ChudRigTransform>();
        private static MacroFakeRig _gunFakeRig;
        private static float _gunLastTime;
        private static VRRig _gunLockedTarget;

        public static string MacrosFolder
        {
            get
            {
                try { return Path.Combine(WristMenu.FolderName, "Macros"); }
                catch { return "Chud Menu/Macros"; }
            }
        }

        private static bool IsGKeyHeld()
        {
            try
            {
                if (Keyboard.current != null && Keyboard.current.gKey != null)
                    return Keyboard.current.gKey.isPressed;
            }
            catch { }
            return false;
        }

        private static bool IsRecordInputHeld()
        {
            try
            {
                if (WristMenu.triggerDownL)
                    return true;
            }
            catch { }
            return IsGKeyHeld();
        }

        private static bool IsMacroInputHeld()
        {
            try
            {
                if (WristMenu.triggerDownR)
                    return true;
            }
            catch { }
            return IsGKeyHeld();
        }

        private static string FormatMacroName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            StringBuilder sb = new StringBuilder(input.Length);
            foreach (char c in input)
                sb.Append(char.IsLetterOrDigit(c) ? c : '-');
            return sb.ToString();
        }

        private static int MacroSortNumber(string name)
        {
            if (string.IsNullOrEmpty(name))
                return int.MaxValue;
            string t = name.Trim();
            if (t.Equals("macro", StringComparison.OrdinalIgnoreCase))
                return 1;
            if (t.StartsWith("macro ", StringComparison.OrdinalIgnoreCase))
            {
                int n;
                if (int.TryParse(t.Substring(6).Trim(), out n))
                    return n;
            }
            return int.MaxValue;
        }

        private static int CompareMacroNames(string a, string b)
        {
            int na = MacroSortNumber(a);
            int nb = MacroSortNumber(b);
            bool sa = na != int.MaxValue;
            bool sb = nb != int.MaxValue;
            if (sa && sb)
            {
                int c = na.CompareTo(nb);
                if (c != 0)
                    return c;
                return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
            }
            if (sa)
                return -1;
            if (sb)
                return 1;
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static string GenerateMacroName()
        {
            try
            {
                string folder = MacrosFolder;
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                if (!File.Exists(Path.Combine(folder, FormatMacroName("macro") + ".macro")))
                    return "macro";
                for (int n = 2; n <= 9999; n++)
                {
                    string candidate = "macro " + n;
                    if (!File.Exists(Path.Combine(folder, FormatMacroName(candidate) + ".macro")))
                        return candidate;
                }
                return "macro " + (System.DateTime.Now.Ticks % 100000);
            }
            catch { }
            return "macro";
        }

        private static byte[] Compress(byte[] data)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
                    gzip.Write(data, 0, data.Length);
                return output.ToArray();
            }
        }

        private static string Decompress(byte[] data)
        {
            using (MemoryStream input = new MemoryStream(data))
            using (GZipStream gzip = new GZipStream(input, System.IO.Compression.CompressionMode.Decompress))
            using (StreamReader reader = new StreamReader(gzip, Encoding.UTF8))
                return reader.ReadToEnd();
        }

        public static void FinishRecordingMacro(List<ChudRigTransform> recordingData)
        {
            try
            {
                if (recordingData == null || recordingData.Count == 0)
                {
                    NotifiLib.SendNotification("Macros: recording discarded (no data)", 1);
                    return;
                }

                string name = GenerateMacroName();
                ChudMacro macro = new ChudMacro
                {
                    Name = name,
                    Positions = new List<ChudRigTransform>(recordingData),
                };
                SaveMacro(macro);
                NotifiLib.SendNotification("Macros: saved " + name, 2);
            }
            catch (Exception e)
            {
                try { NotifiLib.SendNotification("Macros: save failed", 1); } catch { }
                Debug.LogError("[Chud][Macros] FinishRecordingMacro failed: " + e);
            }
        }

        public static void SaveMacro(ChudMacro macro)
        {
            try
            {
                string folder = MacrosFolder;
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                string filePath = Path.Combine(folder, FormatMacroName(macro.Name) + ".macro");
                File.WriteAllBytes(filePath, Compress(Encoding.UTF8.GetBytes(macro.DumpJson())));
            }
            catch (Exception e)
            {
                Debug.LogError("[Chud][Macros] SaveMacro failed: " + e);
            }
            LoadAllMacros();
        }

        public static void DeleteMacro(string fileKey)
        {
            string display = fileKey;
            try
            {
                if (_macros.TryGetValue(fileKey, out ChudMacro existing) && !string.IsNullOrEmpty(existing.Name))
                    display = existing.Name;
            }
            catch { }

            try
            {
                if (_enabledMacros.TryGetValue(fileKey, out PlaybackState st))
                {
                    try { st.PreviewRig?.Destroy(); } catch { }
                    _enabledMacros.Remove(fileKey);
                }
            }
            catch { }

            _macros.Remove(fileKey);

            try
            {
                string path = Path.Combine(MacrosFolder, fileKey + ".macro");
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogError("[Chud][Macros] DeleteMacro file failed: " + e);
            }

            try { NotifiLib.SendNotification("Macros: deleted " + display, 1); } catch { }
            LoadAllMacros();
        }

        public static void LoadAllMacros()
        {
            try
            {
                string folder = MacrosFolder;
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            catch { }

            try
            {
                foreach (var kvp in _enabledMacros)
                {
                    try { kvp.Value.PreviewRig?.Destroy(); } catch { }
                }
            }
            catch { }
            _enabledMacros.Clear();
            _macros.Clear();

            MenuCategory cat = null;
            try { cat = MenuManager.Categories.Find(c => c.Name == "Macros"); } catch { }
            if (cat == null)
                return;
            if (cat.Buttons == null)
                cat.Buttons = new List<ButtonInfo>();
            try
            {
                if (cat.Buttons.Count > 5)
                    cat.Buttons.RemoveRange(5, cat.Buttons.Count - 5);
            }
            catch { }

            string[] files = new string[0];
            try { files = Directory.GetFiles(MacrosFolder); } catch { }

            foreach (string file in files)
            {
                try
                {
                    if (!file.EndsWith(".macro", StringComparison.OrdinalIgnoreCase))
                        continue;
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (string.IsNullOrEmpty(fileName))
                        continue;
                    string json = Decompress(File.ReadAllBytes(file));
                    ChudMacro macro = ChudMacro.LoadJson(json);
                    if (string.IsNullOrEmpty(macro.Name))
                        macro.Name = fileName;
                    if (macro.Positions == null)
                        macro.Positions = new List<ChudRigTransform>();
                    _macros[fileName] = macro;
                }
                catch { continue; }
            }

            List<KeyValuePair<string, ChudMacro>> ordered = new List<KeyValuePair<string, ChudMacro>>(_macros);
            ordered.Sort((a, b) => CompareMacroNames(
                string.IsNullOrEmpty(a.Value.Name) ? a.Key : a.Value.Name,
                string.IsNullOrEmpty(b.Value.Name) ? b.Key : b.Value.Name));

            foreach (var kvp in ordered)
            {
                try
                {
                    string fileKey = kvp.Key;
                    ChudMacro macro = kvp.Value;
                    string displayName = string.IsNullOrEmpty(macro.Name) ? fileKey : macro.Name;

                    string btnText = displayName;
                    int dup = 2;
                    while (cat.Buttons.Exists(b => b.buttonText == btnText))
                    {
                        btnText = displayName + " (" + dup + ")";
                        dup++;
                        if (dup > 999)
                            break;
                    }

                    string capturedKey = fileKey;
                    cat.Buttons.Add(new ButtonInfo
                    {
                        buttonText = btnText,
                        toolTip = "Hold right trigger near macro start to play " + displayName,
                        enabled = false,
                        isTogglable = true,
                        type = ButtonType.FrameToggle,
                        enableMethod = () => EnableMacro(capturedKey),
                        method = () => TickMacro(capturedKey),
                        disableMethod = () => DisableMacro(capturedKey),
                    });
                }
                catch { continue; }
            }

            try { Mods.InvalidateActiveButtonsCache(); } catch { }

            try
            {
                if (MenuManager.CurrentCategoryName == "Macros" && WristMenu.instance != null)
                {
                    WristMenu.pageNumber = 0;
                    if (WristMenu.toggleMenu)
                        WristMenu.RefreshMenu();
                    else
                    {
                        WristMenu.DestroyMenu();
                        WristMenu.instance.Draw();
                    }
                }
            }
            catch { }
        }

        public static void ReloadMacros()
        {
            LoadAllMacros();
            try { NotifiLib.SendNotification("Macros reloaded", 1); } catch { }
        }

        public static void EnableRecordMacro()
        {
            try { NotifiLib.SendNotification("Macros: hold LEFT trigger to record, release to save", 2); } catch { }
        }

        public static void RecordMacroTick()
        {
            try
            {
                if (_isPlayingMacro)
                    return;
                if (VRRig.LocalRig == null)
                    return;

                bool held = IsRecordInputHeld();
                if (held)
                {
                    if (RecordingMacro && _activeRecorder != ActiveRecorder.Self)
                        return;

                    if (!RecordingMacro)
                    {
                        _selfRecordingData.Clear();
                        RecordingMacro = true;
                        _activeRecorder = ActiveRecorder.Self;

                        ChudRigTransform start = ChudRigTransform.GetRigPosition(VRRig.LocalRig);
                        try
                        {
                            if (_selfFakeRig != null)
                            {
                                try { _selfFakeRig.Destroy(); } catch { }
                                _selfFakeRig = null;
                            }
                            _selfFakeRig = new MacroFakeRig(Color.yellow,
                                start.HeadPosition, start.HeadRotation,
                                start.LeftHandPosition, start.LeftHandRotation,
                                start.RightHandPosition, start.RightHandRotation,
                                "recording", false);
                        }
                        catch { }

                        try { NotifiLib.SendNotification("Macros: Recording macro...", 2); } catch { }
                        _selfLastTime = Time.time - MacroStep;
                    }

                    try { _selfFakeRig?.Tick(); } catch { }
                    try
                    {
                        if (ControllerInputPoller.instance != null)
                            ControllerInputPoller.instance.leftControllerIndexFloat = 0f;
                    }
                    catch { }

                    if (Time.time - _selfLastTime > MacroStep)
                    {
                        _selfLastTime = Time.time;
                        _selfRecordingData.Add(ChudRigTransform.GetRigPosition(VRRig.LocalRig));
                    }
                }
                else if (RecordingMacro && _activeRecorder == ActiveRecorder.Self)
                {
                    List<ChudRigTransform> copy = new List<ChudRigTransform>(_selfRecordingData);
                    try
                    {
                        if (_selfFakeRig != null)
                        {
                            try { _selfFakeRig.Destroy(); } catch { }
                            _selfFakeRig = null;
                        }
                    }
                    catch { }
                    _selfRecordingData.Clear();
                    RecordingMacro = false;
                    _activeRecorder = ActiveRecorder.None;
                    FinishRecordingMacro(copy);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[Chud][Macros] RecordMacroTick failed: " + e);
            }
        }

        public static void DisableRecordMacro()
        {
            try
            {
                if (RecordingMacro && _activeRecorder == ActiveRecorder.Self)
                {
                    List<ChudRigTransform> copy = new List<ChudRigTransform>(_selfRecordingData);
                    try
                    {
                        if (_selfFakeRig != null)
                        {
                            try { _selfFakeRig.Destroy(); } catch { }
                            _selfFakeRig = null;
                        }
                    }
                    catch { }
                    _selfRecordingData.Clear();
                    RecordingMacro = false;
                    _activeRecorder = ActiveRecorder.None;
                    FinishRecordingMacro(copy);
                }
                else
                {
                    try
                    {
                        if (_selfFakeRig != null)
                        {
                            try { _selfFakeRig.Destroy(); } catch { }
                            _selfFakeRig = null;
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static void MacroGunTick()
        {
            try
            {
                if (_isPlayingMacro)
                    return;
                try
                {
                    Mods.MakeRightHandGun(() =>
                    {
                        try
                        {
                            VRRig hit = Mods.GetGunTargetPlayer();
                            if (hit != null && !hit.isLocal)
                                _gunLockedTarget = hit;
                        }
                        catch { }
                    }, () => { });
                }
                catch { }

                bool grip = Mods.isRightHanded ? WristMenu.gripDownL : WristMenu.gripDownR;
                bool trigger = Mods.isRightHanded ? WristMenu.triggerDownL : WristMenu.triggerDownR;

                if (!grip)
                    _gunLockedTarget = null;

                try
                {
                    if (_gunLockedTarget != null && _gunLockedTarget.isLocal)
                        _gunLockedTarget = null;
                }
                catch { _gunLockedTarget = null; }

                bool shooting = grip && trigger && _gunLockedTarget != null;

                if (shooting)
                {
                    if (RecordingMacro && _activeRecorder != ActiveRecorder.Gun)
                        return;

                    if (!RecordingMacro)
                    {
                        _gunRecordingData.Clear();
                        RecordingMacro = true;
                        _activeRecorder = ActiveRecorder.Gun;

                        ChudRigTransform start = ChudRigTransform.GetRigPosition(_gunLockedTarget);
                        try
                        {
                            if (_gunFakeRig != null)
                            {
                                try { _gunFakeRig.Destroy(); } catch { }
                                _gunFakeRig = null;
                            }
                            _gunFakeRig = new MacroFakeRig(Color.yellow,
                                start.HeadPosition, start.HeadRotation,
                                start.LeftHandPosition, start.LeftHandRotation,
                                start.RightHandPosition, start.RightHandRotation,
                                "recording", false);
                        }
                        catch { }

                        try { NotifiLib.SendNotification("Macros: Recording macro...", 2); } catch { }
                        _gunLastTime = Time.time - MacroStep;
                    }

                    try { _gunFakeRig?.Tick(); } catch { }

                    if (Time.time - _gunLastTime > MacroStep)
                    {
                        _gunLastTime = Time.time;
                        try
                        {
                            if (_gunLockedTarget != null)
                                _gunRecordingData.Add(ChudRigTransform.GetRigPosition(_gunLockedTarget));
                        }
                        catch { }
                    }

                    try
                    {
                        if (Mods.pointer != null && Mods.Line != null && _gunLockedTarget != null)
                        {
                            Mods.pointer.transform.position = _gunLockedTarget.transform.position;
                            Mods.Line.SetPosition(1, _gunLockedTarget.transform.position);
                        }
                    }
                    catch { }
                }
                else if (RecordingMacro && _activeRecorder == ActiveRecorder.Gun)
                {
                    List<ChudRigTransform> copy = new List<ChudRigTransform>(_gunRecordingData);
                    try
                    {
                        if (_gunFakeRig != null)
                        {
                            try { _gunFakeRig.Destroy(); } catch { }
                            _gunFakeRig = null;
                        }
                    }
                    catch { }
                    _gunRecordingData.Clear();
                    RecordingMacro = false;
                    _activeRecorder = ActiveRecorder.None;
                    FinishRecordingMacro(copy);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[Chud][Macros] MacroGunTick failed: " + e);
            }
        }

        public static void MacroGunCleanup()
        {
            try
            {
                if (RecordingMacro && _activeRecorder == ActiveRecorder.Gun)
                {
                    List<ChudRigTransform> copy = new List<ChudRigTransform>(_gunRecordingData);
                    try
                    {
                        if (_gunFakeRig != null)
                        {
                            try { _gunFakeRig.Destroy(); } catch { }
                            _gunFakeRig = null;
                        }
                    }
                    catch { }
                    _gunRecordingData.Clear();
                    RecordingMacro = false;
                    _activeRecorder = ActiveRecorder.None;
                    FinishRecordingMacro(copy);
                }
                else
                {
                    try
                    {
                        if (_gunFakeRig != null)
                        {
                            try { _gunFakeRig.Destroy(); } catch { }
                            _gunFakeRig = null;
                        }
                    }
                    catch { }
                }
            }
            catch { }
            _gunLockedTarget = null;
            try { Mods.CleanupGun(); } catch { }
        }

        public static void EnableDeleteMode()
        {
            DeleteMode = true;
            try { NotifiLib.SendNotification("Macros: delete mode ON — click a saved macro to delete it", 2); } catch { }
        }

        public static void DisableDeleteMode()
        {
            DeleteMode = false;
            try { NotifiLib.SendNotification("Macros: delete mode OFF", 1); } catch { }
        }

        public static void EnableMacro(string fileKey)
        {
            try
            {
                if (DeleteMode)
                {
                    DeleteMacro(fileKey);
                    return;
                }

                if (!_macros.TryGetValue(fileKey, out ChudMacro macro))
                {
                    try { NotifiLib.SendNotification("Macros: not found", 1); } catch { }
                    ForceDisableButton(fileKey);
                    return;
                }

                if (macro.Positions == null || macro.Positions.Count == 0)
                {
                    try { NotifiLib.SendNotification("Macros: empty, cannot preview", 1); } catch { }
                    ForceDisableButton(fileKey);
                    return;
                }

                if (_enabledMacros.ContainsKey(fileKey))
                    return;

                ChudRigTransform start = macro.Positions[0];
                MacroGhostPreview preview = null;
                try
                {
                    Color c = WristMenu.ButtonColorEnabled;
                    preview = new MacroGhostPreview(start, macro.Name, c);
                }
                catch (Exception e)
                {
                    Debug.LogError("[Chud][Macros] EnableMacro preview failed: " + e);
                }

                _enabledMacros[fileKey] = new PlaybackState { Macro = macro, PreviewRig = preview };
                try { NotifiLib.SendNotification("Macros: preview on — " + macro.Name, 1); } catch { }
            }
            catch (Exception e)
            {
                Debug.LogError("[Chud][Macros] EnableMacro failed: " + e);
            }
        }

        public static void TickMacro(string fileKey)
        {
            try
            {
                if (!_enabledMacros.TryGetValue(fileKey, out PlaybackState state))
                    return;

                bool held = IsMacroInputHeld();
                bool freshPress = held && !_playInputWasHeld;
                _playInputWasHeld = held;

                try { state.PreviewRig?.Tick(); } catch { }

                if (_isPlayingMacro)
                    return;
                if (!freshPress)
                    return;
                if (state.Macro.Positions == null || state.Macro.Positions.Count == 0)
                    return;
                if (RecordingMacro)
                    return;
                if (VRRig.LocalRig == null)
                    return;

                try
                {
                    float dist = Vector3.Distance(VRRig.LocalRig.transform.position, state.Macro.Positions[0].RigPosition);
                    if (dist >= 1f)
                        return;
                }
                catch { return; }

                if (Mods.instance != null)
                    Mods.instance.StartCoroutine(PlayMacro(fileKey));
            }
            catch (Exception e)
            {
                Debug.LogError("[Chud][Macros] TickMacro failed: " + e);
            }
        }

        public static void DisableMacro(string fileKey)
        {
            try
            {
                if (_enabledMacros.TryGetValue(fileKey, out PlaybackState state))
                {
                    try { state.PreviewRig?.Destroy(); } catch { }
                    _enabledMacros.Remove(fileKey);
                }
            }
            catch { }
        }

        private static void ForceDisableButton(string fileKey)
        {
            try
            {
                string want = fileKey;
                if (_macros.TryGetValue(fileKey, out ChudMacro m) && !string.IsNullOrEmpty(m.Name))
                    want = m.Name;

                MenuCategory cat = MenuManager.Categories.Find(c => c.Name == "Macros");
                if (cat == null || cat.Buttons == null)
                    return;
                foreach (ButtonInfo b in cat.Buttons)
                {
                    if (b == null || b.enabled != true)
                        continue;
                    if (b.buttonText == want || b.buttonText.StartsWith(want + " (", StringComparison.Ordinal))
                    {
                        b.enabled = false;
                        try { Mods.InvalidateActiveButtonsCache(); } catch { }
                        try { WristMenu.UpdateButtonVisual(b.buttonText, false); } catch { }
                        break;
                    }
                }
            }
            catch { }
        }

        private static Vector3 FormatTeleportPosition(Vector3 teleportPosition)
        {
            try { return Chud.Backend.Console.World2Player(teleportPosition); }
            catch
            {
                try
                {
                    return teleportPosition
                        - GorillaTagger.Instance.bodyCollider.transform.position
                        + GorillaTagger.Instance.transform.position;
                }
                catch { return teleportPosition; }
            }
        }

        private static IEnumerator PlayMacro(string fileKey)
        {
            ChudMacro macro;
            PlaybackState state = null;
            _enabledMacros.TryGetValue(fileKey, out state);

            if (state != null)
                macro = state.Macro;
            else if (!_macros.TryGetValue(fileKey, out macro))
                yield break;

            if (_isPlayingMacro || RecordingMacro)
                yield break;
            if (macro.Positions == null || macro.Positions.Count == 0)
                yield break;

            _isPlayingMacro = true;

            List<MeshCollider> disabledColliders = new List<MeshCollider>();
            try
            {
                MeshCollider[] all = Resources.FindObjectsOfTypeAll<MeshCollider>();
                foreach (MeshCollider c in all)
                {
                    try
                    {
                        if (c != null && c.enabled)
                        {
                            disabledColliders.Add(c);
                            c.enabled = false;
                        }
                    }
                    catch { }
                }
            }
            catch { }

            ChudRigTransform original = new ChudRigTransform();
            try { original = ChudRigTransform.GetRigPosition(VRRig.LocalRig); } catch { }

            bool rigWasEnabled = true;
            Vector3 rigPos = original.RigPosition;
            Quaternion rigRot = original.RigRotation;
            try
            {
                if (VRRig.LocalRig != null)
                {
                    rigWasEnabled = VRRig.LocalRig.enabled;
                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = rigPos;
                    VRRig.LocalRig.transform.rotation = rigRot;
                }
            }
            catch { }

            try { state?.PreviewRig?.SetVisible(false); } catch { }
            try { Mods.SubscribeMacroGhost(); } catch { }

            try
            {
                List<ChudRigTransform> positions = macro.Positions;
                if (positions != null && positions.Count != 0)
                    yield return PlayMacroIteration(macro, positions, original, state);
            }
            finally
            {
                try { Mods.UnsubscribeMacroGhost(); } catch { }
                try { state?.PreviewRig?.SetVisible(true); } catch { }
                try { ResetPreviewRig(state, macro); } catch { }
                try
                {
                    if (VRRig.LocalRig != null)
                        VRRig.LocalRig.enabled = rigWasEnabled;
                }
                catch { }
                try
                {
                    foreach (MeshCollider c in disabledColliders)
                    {
                        try { if (c != null) c.enabled = true; } catch { }
                    }
                }
                catch { }
                _isPlayingMacro = false;
            }
        }

        private static IEnumerator PlayMacroIteration(ChudMacro macro, List<ChudRigTransform> positions,
            ChudRigTransform startPosition, PlaybackState state)
        {
            float macroStartTime = Time.time;
            float macroEndTime = positions.Count * MacroStep;
            int lastFuture = -1;

            try
            {
                if (state != null && state.PreviewRig != null)
                {
                    state.PreviewRig.LastUpdateDelay = MacroStep;
                    state.PreviewRig.LastUpdateTime = Time.time - MacroStep;
                }
            }
            catch { }

            while (Time.time < macroStartTime + macroEndTime && IsMacroInputHeld())
            {
                float elapsed = Time.time - macroStartTime;
                float stepElapsed = elapsed % MacroStep;

                int idx = Mathf.FloorToInt(elapsed / MacroStep);
                idx = Mathf.Clamp(idx, 0, positions.Count - 1);

                ChudRigTransform last = idx == 0 ? startPosition : positions[idx - 1];
                ChudRigTransform cur = positions[idx];
                float t = stepElapsed / MacroStep;

                try { ApplyRigPosition(last, cur, t); } catch { }
                try { UpdatePreviewTarget(state, positions, idx, ref lastFuture); } catch { }

                yield return null;
            }
        }

        private static void ApplyRigPosition(ChudRigTransform last, ChudRigTransform cur, float t)
        {
            Vector3 rigPos = Vector3.Lerp(last.RigPosition, cur.RigPosition, t);
            Quaternion rigRot = Quaternion.Lerp(last.RigRotation, cur.RigRotation, t);

            if (VRRig.LocalRig != null)
            {
                try
                {
                    VRRig.LocalRig.transform.position = rigPos;
                    VRRig.LocalRig.transform.rotation = rigRot;
                }
                catch { }
            }

            try
            {
                if (GorillaTagger.Instance != null && GorillaTagger.Instance.rigidbody != null)
                {
                    GorillaTagger.Instance.rigidbody.transform.position = FormatTeleportPosition(rigPos);
                    GorillaTagger.Instance.rigidbody.linearVelocity =
                        Vector3.Lerp(last.Velocity, cur.Velocity, t);
                }
            }
            catch { }

            try
            {
                if (VRRig.LocalRig != null)
                {
                    if (VRRig.LocalRig.leftHand != null && VRRig.LocalRig.leftHand.rigTarget != null)
                    {
                        VRRig.LocalRig.leftHand.rigTarget.position =
                            Vector3.Lerp(last.LeftHandPosition, cur.LeftHandPosition, t);
                        VRRig.LocalRig.leftHand.rigTarget.rotation =
                            Quaternion.Lerp(last.LeftHandRotation, cur.LeftHandRotation, t);
                    }
                    if (VRRig.LocalRig.rightHand != null && VRRig.LocalRig.rightHand.rigTarget != null)
                    {
                        VRRig.LocalRig.rightHand.rigTarget.position =
                            Vector3.Lerp(last.RightHandPosition, cur.RightHandPosition, t);
                        VRRig.LocalRig.rightHand.rigTarget.rotation =
                            Quaternion.Lerp(last.RightHandRotation, cur.RightHandRotation, t);
                    }
                    if (VRRig.LocalRig.head != null && VRRig.LocalRig.head.rigTarget != null)
                    {
                        VRRig.LocalRig.head.rigTarget.rotation =
                            Quaternion.Lerp(last.HeadRotation, cur.HeadRotation, t);
                    }
                }
            }
            catch { }
        }

        private static void UpdatePreviewTarget(PlaybackState state, List<ChudRigTransform> positions,
            int current, ref int lastFuture)
        {
            if (state == null || state.PreviewRig == null || positions == null || positions.Count == 0)
                return;
            int lookAhead = Mathf.Max(1, Mathf.RoundToInt(1f / MacroStep));
            int future = Mathf.Min(current + lookAhead, positions.Count - 1);
            if (future == lastFuture)
                return;
            lastFuture = future;
            ChudRigTransform target = positions[future];
            state.PreviewRig.UpdateTargets(target.HeadPosition, target.HeadRotation,
                target.LeftHandPosition, target.LeftHandRotation,
                target.RightHandPosition, target.RightHandRotation);
        }

        private static void ResetPreviewRig(PlaybackState state, ChudMacro macro)
        {
            if (state == null || state.PreviewRig == null)
                return;
            if (macro.Positions == null || macro.Positions.Count == 0)
                return;
            ChudRigTransform first = macro.Positions[0];
            try
            {
                state.PreviewRig.UpdateTargets(first.HeadPosition, first.HeadRotation,
                    first.LeftHandPosition, first.LeftHandRotation,
                    first.RightHandPosition, first.RightHandRotation);
                state.PreviewRig.LastUpdateDelay = MacroStep;
                state.PreviewRig.LastUpdateTime = Time.time - MacroStep;
            }
            catch { }
        }
    }
}
