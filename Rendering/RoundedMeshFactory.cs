using Chud.Diagnostics;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine;
namespace Chud.Rendering
{
    public static class RoundedMeshFactory
    {
        private static readonly Dictionary<string, Mesh> RectCache = new Dictionary<string, Mesh>(32, System.StringComparer.Ordinal);
        private static readonly Dictionary<string, Mesh> FrameCache = new Dictionary<string, Mesh>(16, System.StringComparer.Ordinal);

        public static Mesh RoundedRect(float width, float height, float radius, int cornerSegments, float depth)
        {
            string key = string.Concat(
                width.ToString("R", System.Globalization.CultureInfo.InvariantCulture), ":",
                height.ToString("R", System.Globalization.CultureInfo.InvariantCulture), ":",
                radius.ToString("R", System.Globalization.CultureInfo.InvariantCulture), ":",
                cornerSegments.ToString(System.Globalization.CultureInfo.InvariantCulture), ":",
                depth.ToString("R", System.Globalization.CultureInfo.InvariantCulture));

            if (RectCache.TryGetValue(key, out Mesh cached) && cached != (Object)null)
            {
                return cached;
            }

            Mesh mesh = Log.Guard("RoundedMeshFactory.RoundedRect", () => BuildRoundedRect(width, height, radius, cornerSegments, depth), null);
            if (mesh != null)
            {
                RectCache[key] = mesh;
            }

            return mesh;
        }

        public static Mesh RoundedFrame(float radius, float border, int cornerSegments)
        {
            string key = string.Concat(
                radius.ToString("F4", System.Globalization.CultureInfo.InvariantCulture), ":",
                border.ToString("F4", System.Globalization.CultureInfo.InvariantCulture), ":",
                cornerSegments.ToString(System.Globalization.CultureInfo.InvariantCulture));

            if (FrameCache.TryGetValue(key, out Mesh cached) && cached != (Object)null)
            {
                return cached;
            }

            Mesh mesh = Log.Guard("RoundedMeshFactory.RoundedFrame", () => BuildRoundedFrame(radius, border, cornerSegments), null);
            if (mesh != null)
            {
                FrameCache[key] = mesh;
            }

            return mesh;
        }

        private static Mesh BuildRoundedRect(float width, float height, float radius, int cornerSegments, float depth)
        {
            float hw = width * 0.5f;
            float hh = height * 0.5f;
            float hd = depth * 0.5f;

            radius = Mathf.Min(radius, Mathf.Min(hw, hh));
            if (cornerSegments < 1)
            {
                cornerSegments = 1;
            }

            List<Vector2> outline = BuildRectOutline(hw, hh, radius, cornerSegments);
            int outlineCount = outline.Count;

            var verts = new List<Vector3>(outlineCount * 2 + 2);
            var uvs = new List<Vector2>(outlineCount * 2 + 2);
            var tris = new List<int>(outlineCount * 6);

            int frontCenter = verts.Count;
            verts.Add(new Vector3(hd, 0f, 0f));
            uvs.Add(new Vector2(0.5f, 0.5f));

            int frontStart = verts.Count;
            for (int i = 0; i < outlineCount; i++)
            {
                verts.Add(new Vector3(hd, outline[i].x, outline[i].y));
                uvs.Add(new Vector2((outline[i].y + hw) / width, (outline[i].x + hh) / height));
            }

            for (int i = 0; i < outlineCount; i++)
            {
                int next = (i + 1) % outlineCount;
                tris.Add(frontCenter);
                tris.Add(frontStart + i);
                tris.Add(frontStart + next);
            }

            int backCenter = verts.Count;
            verts.Add(new Vector3(-hd, 0f, 0f));
            uvs.Add(new Vector2(0.5f, 0.5f));

            int backStart = verts.Count;
            for (int i = 0; i < outlineCount; i++)
            {
                verts.Add(new Vector3(-hd, outline[i].x, outline[i].y));
                uvs.Add(new Vector2((outline[i].y + hw) / width, (outline[i].x + hh) / height));
            }

            for (int i = 0; i < outlineCount; i++)
            {
                int next = (i + 1) % outlineCount;
                tris.Add(backCenter);
                tris.Add(backStart + next);
                tris.Add(backStart + i);
            }

            for (int i = 0; i < outlineCount; i++)
            {
                int next = (i + 1) % outlineCount;
                int f0 = frontStart + i;
                int f1 = frontStart + next;
                int b0 = backStart + i;
                int b1 = backStart + next;
                tris.Add(f0);
                tris.Add(b0);
                tris.Add(f1);
                tris.Add(f1);
                tris.Add(b0);
                tris.Add(b1);
            }

            var mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static List<Vector2> BuildRectOutline(float hw, float hh, float radius, int cornerSegments)
        {
            Vector2[] centers =
            {
                new Vector2(hw - radius, hh - radius),
                new Vector2(hw - radius, -(hh - radius)),
                new Vector2(-(hw - radius), -(hh - radius)),
                new Vector2(-(hw - radius), hh - radius)
            };

            float[] starts = { 90f, 0f, -90f, -180f };
            float[] ends = { 0f, -90f, -180f, -270f };

            var outline = new List<Vector2>((cornerSegments + 1) * 4);
            for (int c = 0; c < 4; c++)
            {
                for (int i = 0; i <= cornerSegments; i++)
                {
                    float angle = Mathf.Lerp(starts[c], ends[c], (float)i / cornerSegments) * Mathf.Deg2Rad;
                    float y = centers[c].y + Mathf.Sin(angle) * radius;
                    float z = centers[c].x + Mathf.Cos(angle) * radius;
                    outline.Add(new Vector2(y, z));
                }
            }

            return outline;
        }

        private static Mesh BuildRoundedFrame(float radius, float border, int cornerSegments)
        {
            if (cornerSegments < 4)
            {
                cornerSegments = 4;
            }

            int outerSegments = (cornerSegments + 1) * 4;
            List<Vector2> outer = BuildFrameOutline(radius, outerSegments);
            List<Vector2> inner = BuildFrameOutline(Mathf.Max(0.001f, radius - border), outerSegments);

            var verts = new List<Vector3>(outerSegments * 2);
            var uvs = new List<Vector2>(outerSegments * 2);
            var tris = new List<int>(outerSegments * 9);

            for (int i = 0; i < outerSegments; i++)
            {
                verts.Add(new Vector3(0f, outer[i].x, outer[i].y));
                uvs.Add(new Vector2(outer[i].x * 0.5f + 0.5f, outer[i].y * 0.5f + 0.5f));
            }

            for (int i = 0; i < outerSegments; i++)
            {
                verts.Add(new Vector3(0f, inner[i].x, inner[i].y));
                uvs.Add(new Vector2(inner[i].x * 0.5f + 0.5f, inner[i].y * 0.5f + 0.5f));
            }

            for (int i = 0; i < outerSegments; i++)
            {
                int next = (i + 1) % outerSegments;
                int o0 = i;
                int o1 = next;
                int i0 = outerSegments + i;
                int i1 = outerSegments + next;

                tris.Add(o0);
                tris.Add(i0);
                tris.Add(o1);

                tris.Add(o1);
                tris.Add(i0);
                tris.Add(i1);

                tris.Add(o0);
                tris.Add(i1);
                tris.Add(i0);
            }

            var mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static List<Vector2> BuildFrameOutline(float radius, int segments)
        {
            const float half = 0.5f;
            radius = Mathf.Clamp(radius, 0f, half - 0.0001f);

            Vector2[] centers =
            {
                new Vector2(half - radius, half - radius),
                new Vector2(half - radius, -(half - radius)),
                new Vector2(-(half - radius), -(half - radius)),
                new Vector2(-(half - radius), half - radius)
            };

            float[] starts = { 90f, 0f, -90f, -180f };
            float[] ends = { 0f, -90f, -180f, -270f };

            int perCorner = segments / 4;
            if (perCorner < 1)
            {
                perCorner = 1;
            }

            var outline = new List<Vector2>(perCorner * 4);
            for (int c = 0; c < 4; c++)
            {
                for (int i = 0; i < perCorner; i++)
                {
                    float t = (float)i / perCorner;
                    float angle = Mathf.Lerp(starts[c], ends[c], t) * Mathf.Deg2Rad;
                    outline.Add(new Vector2(
                        centers[c].x + Mathf.Cos(angle) * radius,
                        centers[c].y + Mathf.Sin(angle) * radius));
                }
            }

            return outline;
        }
    }
}