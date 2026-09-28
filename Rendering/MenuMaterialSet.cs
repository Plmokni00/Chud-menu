using Chud.Diagnostics;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System;
using UnityEngine;
namespace Chud.Rendering
{
    public sealed class MenuMaterialSet : IDisposable
    {
        private const int GradientHeight = 16;

        private const float HighlightBlend = 0.075f;

        private readonly Dictionary<string, Material> _gradients = new Dictionary<string, Material>(16, StringComparer.Ordinal);
        private readonly Dictionary<Color32, Material> _plain = new Dictionary<Color32, Material>(16);
        private readonly Dictionary<Color32, Material> _outlines = new Dictionary<Color32, Material>(8);
        private readonly List<Material> _owned = new List<Material>(32);
        private readonly List<Texture> _ownedTextures = new List<Texture>(8);

        private bool _disposed;

        public int MaterialCount => _owned.Count;

        public Material Gradient(Color top, Color bottom)
        {
            string key = ColorKey(top) + "|" + ColorKey(bottom);
            if (_gradients.TryGetValue(key, out Material cached) && cached != (Object)null)
            {
                return cached;
            }

            return Log.Guard("MenuMaterialSet.Gradient", () =>
            {
                var texture = new Texture2D(2, GradientHeight, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    wrapMode = TextureWrapMode.Repeat
                };

                Color highlight = Color.Lerp(bottom, Color.white, HighlightBlend);
                for (int y = 0; y < GradientHeight; y++)
                {
                    float t = 1f - Mathf.Abs((float)y / (GradientHeight - 1) * 2f - 1f);
                    Color row = Color.Lerp(bottom, highlight, t);
                    texture.SetPixel(0, y, row);
                    texture.SetPixel(1, y, row);
                }

                texture.Apply();
                _ownedTextures.Add(texture);

                var material = new Material(ShaderLibrary.Textured)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    color = Color.white,
                    mainTexture = texture,
                    mainTextureScale = new Vector2(1f, 0.5f)
                };

                _owned.Add(material);
                _gradients[key] = material;
                return material;
            }, null);
        }

        public Material Plain(Color color)
        {
            Color32 key = color;
            if (_plain.TryGetValue(key, out Material cached) && cached != (Object)null)
            {
                return cached;
            }

            return Log.Guard("MenuMaterialSet.Plain", () =>
            {
                var material = new Material(ShaderLibrary.Flat)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    color = color
                };

                _owned.Add(material);
                _plain[key] = material;
                return material;
            }, null);
        }

        public Material Outline(Color color)
        {
            Color32 key = color;
            if (_outlines.TryGetValue(key, out Material cached) && cached != (Object)null)
            {
                return cached;
            }

            return Log.Guard("MenuMaterialSet.Outline", () =>
            {
                var material = new Material(ShaderLibrary.Flat)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    color = color
                };

                ShaderLibrary.ConfigureTransparent(material);
                _owned.Add(material);
                _outlines[key] = material;
                return material;
            }, null);
        }

        public void TickGradients(float time)
        {
            if (_disposed)
            {
                return;
            }

            var offset = new Vector2(0f, time * 0.2f);
            foreach (Material material in _gradients.Values)
            {
                if (material == (Object)null || !material.HasProperty("_MainTex"))
                {
                    continue;
                }

                material.mainTextureOffset = offset;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            for (int i = 0; i < _owned.Count; i++)
            {
                DestroySafely(_owned[i]);
            }

            for (int i = 0; i < _ownedTextures.Count; i++)
            {
                if (_ownedTextures[i] != (Object)null)
                {
                    Object.Destroy(_ownedTextures[i]);
                }
            }

            _owned.Clear();
            _ownedTextures.Clear();
            _gradients.Clear();
            _plain.Clear();
            _outlines.Clear();
        }

        private static void DestroySafely(Material material)
        {
            if (material == (Object)null)
            {
                return;
            }

            Object.Destroy(material);
        }

        private static string ColorKey(Color c)
        {
            return string.Concat(
                c.r.ToString("F3", System.Globalization.CultureInfo.InvariantCulture), ",",
                c.g.ToString("F3", System.Globalization.CultureInfo.InvariantCulture), ",",
                c.b.ToString("F3", System.Globalization.CultureInfo.InvariantCulture), ",",
                c.a.ToString("F3", System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    public static class PersistentMaterials
    {
        private static Material _pointer;

        public static Material Pointer
        {
            get
            {
                if (_pointer == (Object)null)
                {
                    _pointer = Log.Guard("PersistentMaterials.Pointer", () =>
                        new Material(ShaderLibrary.Flat)
                        {
                            hideFlags = HideFlags.HideAndDontSave
                        }, null);
                }

                return _pointer;
            }
        }

        public static void TintPointer(Color color)
        {
            Material material = _pointer;
            if (material == (Object)null)
            {
                return;
            }

            material.color = color;
        }
    }
}