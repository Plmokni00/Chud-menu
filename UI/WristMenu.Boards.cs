using Chud.Diagnostics;
using Chud.Runtime;
using Object = UnityEngine.Object;
using TMPro;
using UnityEngine;
namespace Chud.UI
{
    internal partial class WristMenu
    {
        private static readonly string[] BoardPaths =
        {
            "Environment Objects/LocalObjects_Prefab/TreeRoom/motdHeadingText",
            "Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText",
            "Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData",
            "Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText"
        };

        private static readonly string[] OriginalBoardTexts = new string[BoardPaths.Length];
        private static readonly GameObject[] CachedBoardObjects = new GameObject[BoardPaths.Length];
        private static readonly TMP_Text[] CachedBoardTexts = new TMP_Text[BoardPaths.Length];

        public void ApplyCustomBoardText()
        {
            for (int i = 0; i < BoardPaths.Length && i < CustomBoardTexts.Length; i++)
            {
                TMP_Text label = ResolveBoard(i);
                if (label == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(OriginalBoardTexts[i]))
                {
                    OriginalBoardTexts[i] = label.text;
                }

                label.text = CustomBoardTexts[i];
            }
        }

        public void RestoreOriginalBoardText()
        {
            for (int i = 0; i < BoardPaths.Length; i++)
            {
                if (string.IsNullOrEmpty(OriginalBoardTexts[i]))
                {
                    continue;
                }

                TMP_Text label = ResolveBoard(i);
                if (label != null)
                {
                    label.text = OriginalBoardTexts[i];
                }
            }
        }

        public void ResetBoardCache()
        {
            for (int i = 0; i < BoardPaths.Length; i++)
            {
                CachedBoardObjects[i] = null;
                CachedBoardTexts[i] = null;
                OriginalBoardTexts[i] = null;
            }
        }

        private static TMP_Text ResolveBoard(int index)
        {
            if (CachedBoardTexts[index] != (Object)null)
            {
                return CachedBoardTexts[index];
            }

            GameObject go = Chud.Runtime.ObjectFinder.Find(BoardPaths[index]);
            if (go == (Object)null)
            {
                return null;
            }

            CachedBoardObjects[index] = go;
            CachedBoardTexts[index] = go.GetComponent<TMP_Text>();
            return CachedBoardTexts[index];
        }
    }
}