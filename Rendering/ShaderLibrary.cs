using Chud.Diagnostics;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System;
using UnityEngine;
namespace Chud.Rendering
{
    public static class ShaderLibrary
    {
        public const string UnlitTexture = "Unlit/Texture";
        public const string UnlitColor = "Unlit/Color";
        public const string UrpUnlit = "Universal Render Pipeline/Unlit";
        public const string GorillaUber = "GorillaTag/UberShader";
        public const string GuiText = "GUI/Text Shader";

        public static Shader Textured => Resolve(UnlitTexture, UrpUnlit, GuiText);

        public static Shader Flat => Resolve(UnlitColor, UrpUnlit, GuiText);

        public static Shader Uber => Resolve(GorillaUber, UrpUnlit, GuiText);

        public static Shader Fallback => Resolve(GuiText, UnlitColor, UrpUnlit);

        public static Shader Resolve(params string[] names)
        {
            if (names == null)
            {
                return null;
            }

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                Shader shader = ResolveOne(name);
                if (shader != null)
                {
                    return shader;
                }
            }

            return null;
        }

        private static Shader ResolveOne(string name)
        {
            if (_cache.TryGetValue(name, out Shader cached))
            {
                return cached != (Object)null ? cached : null;
            }

            Shader found = Log.Guard("ShaderLibrary.Resolve(" + name + ")",
                () => Shader.Find(name), null);

            if (found == (Object)null)
            {
                Log.Warn("shader '" + name + "' not found; it may have been stripped from the build");
            }

            _cache[name] = found;
            return found;
        }

        private static readonly Dictionary<string, Shader> _cache =
            new Dictionary<string, Shader>(StringComparer.Ordinal);

        public static void ConfigureTransparent(Material material)
        {
            if (material == null)
            {
                return;
            }

            TrySetFloat(material, "_Surface", 1f);
            TrySetFloat(material, "_Blend", 0f);
            TrySetFloat(material, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            TrySetFloat(material, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            TrySetFloat(material, "_ZWrite", 0f);
            TrySetFloat(material, "_AlphaClip", 0f);

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        public static void ConfigureGhostTransparent(Material material)
        {
            if (material == null)
            {
                return;
            }

            TrySetFloat(material, "_Surface", 1f);
            TrySetFloat(material, "_Blend", 0f);
            TrySetFloat(material, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            TrySetFloat(material, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            TrySetFloat(material, "_ZWrite", 0f);
            material.renderQueue = 3000;
        }

        private static void TrySetFloat(Material material, string property, float value)
        {
            if (!material.HasProperty(property))
            {
                return;
            }

            material.SetFloat(property, value);
        }
    }
}