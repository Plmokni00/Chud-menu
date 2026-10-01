using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine;
namespace Chud.Runtime
{
    public static class GameLists
    {
        private static List<VRRig> FillActiveRigs(IReadOnlyList<VRRig> source)
        {
            List<VRRig> buffer = new List<VRRig>(source == null ? 16 : source.Count);
            if (source == null)
            {
                return buffer;
            }

            for (int i = 0; i < source.Count; i++)
            {
                VRRig rig = source[i];
                if (rig != (Object)null)
                {
                    buffer.Add(rig);
                }
            }

            return buffer;
        }

        private static List<GorillaPlayerScoreboardLine> FillScoreboardLines(List<GorillaPlayerScoreboardLine> source)
        {
            List<GorillaPlayerScoreboardLine> buffer =
                new List<GorillaPlayerScoreboardLine>(source == null ? 16 : source.Count);
            if (source == null)
            {
                return buffer;
            }

            for (int i = 0; i < source.Count; i++)
            {
                GorillaPlayerScoreboardLine line = source[i];
                if (line != (Object)null)
                {
                    buffer.Add(line);
                }
            }

            return buffer;
        }

        public static List<VRRig> ActiveRigs()
        {
            return FillActiveRigs(VRRigCache.ActiveRigs);
        }

        public static List<GorillaPlayerScoreboardLine> ScoreboardLines()
        {
            return FillScoreboardLines(GorillaScoreboardTotalUpdater.allScoreboardLines);
        }
    }
}