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

        public static Camera FindDesktopCamera()
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (camera == (Object)null)
                {
                    continue;
                }

                if (camera.name == "Shoulder Camera")
                {
                    return camera;
                }

                Transform parent = camera.transform.parent;
                if (parent != (Object)null && parent.name == "Third Person Camera")
                {
                    return camera;
                }
            }

            return null;
        }

        public static T FindInChildren<T>(Transform root, string relativePath) where T : Component
        {
            if (root == (Object)null)
            {
                return null;
            }

            Transform found = root.Find(relativePath);
            return found == (Object)null ? null : found.GetComponent<T>();
        }
    }
}