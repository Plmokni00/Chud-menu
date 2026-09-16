using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	private static GameObject MakeCylinder()
	{
		GameObject go = new GameObject();
		go.AddComponent<MeshFilter>().mesh = CylinderMesh;
		go.AddComponent<MeshRenderer>();
		return go;
	}

	private static GameObject MakeCylinderButton()
	{
		GameObject go = MakeCylinder();
		go.AddComponent<BoxCollider>().isTrigger = true;
		return go;
	}

	private static Mesh _sphereMesh;
	private static Mesh SphereMesh
	{
		get
		{
			if (_sphereMesh == null)
			{
				GameObject temp = GameObject.CreatePrimitive((PrimitiveType)0);
				_sphereMesh = temp.GetComponent<MeshFilter>().sharedMesh;
				Object.DestroyImmediate(temp);
			}
			return _sphereMesh;
		}
	}

	private static GameObject MakeSphereButtonPresser()
	{
		GameObject go = new GameObject();
		go.layer = 2;
		go.AddComponent<MeshFilter>().mesh = SphereMesh;
		go.AddComponent<MeshRenderer>();
		go.AddComponent<SphereCollider>().isTrigger = true;
		go.GetComponent<Renderer>().material = MakePlainMat(ButtonColorEnabled);
		return go;
	}

	public static void InitMenuFont()
	{
		if (!MenuFontInitialized)
		{
			MenuFontInitialized = true;
			MenuFont = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 200);
			if ((Object)(object)MenuFont == (Object)null)
			{
				MenuFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
			}
		}
	}

	private static readonly Dictionary<string, Mesh> _roundedMeshCache = new Dictionary<string, Mesh>();
	internal static Mesh GenerateRoundedRectMesh(float width, float height, float radius, int cornerSegments, float depth)
	{
		string key = width + ":" + height + ":" + radius + ":" + cornerSegments + ":" + depth;
		if (_roundedMeshCache.TryGetValue(key, out var cached) && (Object)(object)cached != (Object)null) return cached;
		Mesh mesh = new Mesh();
		mesh.hideFlags = HideFlags.HideAndDontSave;
		float hw = width * 0.5f;
		float hh = height * 0.5f;
		float hd = depth * 0.5f;
		radius = Mathf.Min(radius, Mathf.Min(hw, hh));
		List<Vector2> outline = new List<Vector2>();
		Vector2[] centers = new Vector2[]
		{
			new Vector2(hw - radius, hh - radius),
			new Vector2(hw - radius, -(hh - radius)),
			new Vector2(-(hw - radius), -(hh - radius)),
			new Vector2(-(hw - radius), hh - radius)
		};
		float[] starts = new float[] { 90f, 0f, -90f, -180f };
		float[] ends = new float[] { 0f, -90f, -180f, -270f };
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
		int outlineCount = outline.Count;
		List<Vector3> verts = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> tris = new List<int>();
		int frontCenter = verts.Count;
		verts.Add(new Vector3(hd, 0, 0));
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
		verts.Add(new Vector3(-hd, 0, 0));
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
		mesh.SetVertices(verts);
		mesh.SetUVs(0, uvs);
		mesh.SetTriangles(tris, 0);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		_roundedMeshCache[key] = mesh;
		return mesh;
	}
}