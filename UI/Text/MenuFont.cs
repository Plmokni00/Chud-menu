using Chud.Diagnostics;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System;
using UnityEngine;
namespace Chud.UI
{
    public sealed class MenuFont
    {
        public const float PixelsPerUnit = 2000f;
        public const float CapRatio = 0.7f;
        public const float LineRatio = 1.16f;
        public const int MinSize = 6;
        public const int MaxSize = 200;

        public static readonly int[] Ladder = { 24, 34, 44, 54, 64, 74, 86, 100, 115, 132, 150, 175, 205, 230, 260 };

        private static readonly string[] PreferredFamilies =
        {
            "Comic Sans MS",
            "Comic Sans"
        };

        private readonly Dictionary<int, Font> _bySize = new Dictionary<int, Font>(24);
        private string _family;
        private Font _measure;

        public Font Notification => Get(MaxSize / 6);

        public Font Measure
        {
            get
            {
                if (_measure == null)
                {
                    _measure = Get(104);
                }

                return _measure;
            }
        }

        public Font Get(int size, bool snap = true)
        {
            int wanted = snap ? Snap(size) : Mathf.Clamp(size, MinSize, MaxSize);
            if (_bySize.TryGetValue(wanted, out Font font) && font != (Object)null)
            {
                return font;
            }

            font = Create(wanted);
            _bySize[wanted] = font;
            return font;
        }

        public static int Snap(int size)
        {
            int snapped = Ladder[0];
            for (int i = 0; i < Ladder.Length; i++)
            {
                if (Ladder[i] <= size)
                {
                    snapped = Ladder[i];
                }
            }

            return snapped;
        }

        public float MeasureEmWidth(string content)
        {
            Font font = Measure;
            if (font == (Object)null || string.IsNullOrEmpty(content))
            {
                return 0f;
            }

            const int measureSize = 104;

            try
            {
                font.RequestCharactersInTexture(content, measureSize, FontStyle.Bold);
            }
            catch (Exception)
            {
                return 0f;
            }

            float advance = 0f;
            var info = new CharacterInfo();
            for (int i = 0; i < content.Length; i++)
            {
                try
                {
                    if (font.GetCharacterInfo(content[i], out info, measureSize, FontStyle.Bold))
                    {
                        advance += info.advance;
                    }
                }
                catch (Exception)
                {
                }
            }

            return advance / measureSize;
        }

        public int Fit(string content, float maxWidth, float maxHeight)
        {
            int limit = Mathf.FloorToInt(maxHeight * PixelsPerUnit / CapRatio);
            float emWidth = MeasureEmWidth(content);
            if (emWidth > 0.0001f && maxWidth > 0f)
            {
                limit = Mathf.Min(limit, Mathf.FloorToInt(maxWidth * PixelsPerUnit / emWidth));
            }

            return Snap(Mathf.Clamp(limit, Ladder[0], Ladder[Ladder.Length - 1]));
        }

        public float FillStretch(string content, int fontSize, float maxWidth, float maxStretch)
        {
            float emWidth = MeasureEmWidth(content);
            if (emWidth <= 0.0001f || maxWidth <= 0f || fontSize <= 0)
            {
                return 1f;
            }

            float natural = emWidth * fontSize / PixelsPerUnit;
            if (natural <= 0.0001f)
            {
                return 1f;
            }

            return Mathf.Clamp(maxWidth / natural, 1f, maxStretch);
        }

        private Font Create(int size)
        {
            if (_family == null)
            {
                _family = ResolveFamily();
            }

            return CreateForFamily(_family, size);
        }

        private Font CreateForFamily(string family, int size)
        {
            if (!string.IsNullOrEmpty(family))
            {
                Font font = Try(() => Font.CreateDynamicFontFromOSFont(family, size));
                if (font != (Object)null)
                {
                    return font;
                }
            }

            Font builtin = Try(() => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
            if (builtin != (Object)null)
            {
                return builtin;
            }

            return Try(() => Resources.GetBuiltinResource<Font>("Arial.ttf"));
        }

        private static Font Try(Func<Font> create)
        {
            try
            {
                return create();
            }
            catch (Exception ex)
            {
                Log.Warn("font creation failed", ex);
                return null;
            }
        }

        private static string ResolveFamily()
        {
            if (!string.IsNullOrEmpty(_resolvedFamily))
            {
                return _resolvedFamily;
            }

            _resolvedFamily = "Arial";

            string[] installed = null;
            try
            {
                installed = Font.GetOSInstalledFontNames();
            }
            catch (Exception)
            {
            }

            if (installed == null)
            {
                return _resolvedFamily;
            }

            for (int p = 0; p < PreferredFamilies.Length; p++)
            {
                for (int i = 0; i < installed.Length; i++)
                {
                    if (string.Equals(installed[i], PreferredFamilies[p], StringComparison.OrdinalIgnoreCase))
                    {
                        _resolvedFamily = installed[i];
                        return _resolvedFamily;
                    }
                }
            }

            Log.Warn(
                "Comic Sans is not installed; the menu will render with a builtin fallback font. " +
                "Install Comic Sans MS to get the intended look.");

            return _resolvedFamily;
        }

        private static string _resolvedFamily;
    }
}