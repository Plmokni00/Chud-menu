using Chud.Rendering;
using UnityEngine;
namespace Chud.UI
{
    public static class Primitives
    {
        public static GameObject MakeCylinder()
        {
            var go = new GameObject();
            go.AddComponent<MeshFilter>().mesh = PrimitiveMeshes.Cylinder;
            go.AddComponent<MeshRenderer>();
            return go;
        }

        public static GameObject MakeCylinderButton()
        {
            GameObject go = MakeCylinder();
            go.AddComponent<BoxCollider>().isTrigger = true;
            return go;
        }

        public static GameObject MakeConvexRoundedButton(Mesh colliderMesh)
        {
            GameObject go = MakeCylinder();
            var collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = colliderMesh;
            collider.convex = true;
            collider.isTrigger = true;
            return go;
        }

        public static GameObject MakeSphereButtonPresser()
        {
            var go = new GameObject();
            go.layer = 2;
            go.AddComponent<MeshFilter>().mesh = PrimitiveMeshes.Sphere;
            var renderer = go.AddComponent<MeshRenderer>();
            go.AddComponent<SphereCollider>().isTrigger = true;

            Material material = PersistentMaterials.Pointer;
            if (material != null)
            {
                PersistentMaterials.TintPointer(WristMenu.ButtonColorEnabled);
                renderer.material = material;
            }

            return go;
        }
    }
}