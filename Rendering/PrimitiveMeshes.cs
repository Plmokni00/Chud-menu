using Chud.Diagnostics;
using Object = UnityEngine.Object;
using UnityEngine;
namespace Chud.Rendering
{
    public static class PrimitiveMeshes
    {
        private static Mesh _cylinder;
        private static Mesh _sphere;
        private static Mesh _capsule;

        public static Mesh Cylinder => _cylinder != (Object)null ? _cylinder : (_cylinder = Harvest(PrimitiveType.Cylinder));

        public static Mesh Sphere => _sphere != (Object)null ? _sphere : (_sphere = Harvest(PrimitiveType.Sphere));

        public static Mesh Capsule => _capsule != (Object)null ? _capsule : (_capsule = Harvest(PrimitiveType.Capsule));

        public static Mesh Harvest(PrimitiveType type)
        {
            return Log.Guard("PrimitiveMeshes.Harvest(" + type + ")", () =>
            {
                GameObject temp = GameObject.CreatePrimitive(type);
                try
                {
                    MeshFilter filter = temp.GetComponent<MeshFilter>();
                    if (filter == null || filter.sharedMesh == null)
                    {
                        Log.Warn("built-in primitive '" + type + "' has no mesh");
                        return null;
                    }

                    return filter.sharedMesh;
                }
                finally
                {
                    Object.DestroyImmediate(temp);
                }
            }, null);
        }
    }
}