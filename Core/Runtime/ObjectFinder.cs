using Chud.Diagnostics;
using Object = UnityEngine.Object;
using UnityEngine;
namespace Chud.Runtime
{
    public static class ObjectFinder
    {
        public static GameObject Find(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return Log.Guard("ObjectFinder.Find(" + path + ")", () => GameObject.Find(path), null);
        }

        public static T FindComponent<T>(string path) where T : Component
        {
            GameObject go = Find(path);
            return go == (Object)null ? null : go.GetComponent<T>();
        }

        public static Camera FindShoulderCamera()
        {
            Camera camera = FindComponent<Camera>("Player Objects/Third Person Camera/Shoulder Camera");
            if (camera != null)
            {
                return camera;
            }

            return FindComponent<Camera>("Shoulder Camera");
        }
    }
}