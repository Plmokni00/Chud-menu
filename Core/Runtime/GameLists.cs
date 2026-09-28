using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine;
namespace Chud.Runtime
{
    public static class GameLists
    {
        [System.ThreadStatic]
        private static List<VRRig> _rigBuffer;

        [System.ThreadStatic]
        private static List<GorillaPlayerScoreboardLine> _scoreboardBuffer;

        [System.ThreadStatic]
        private static List<GorillaGuardianZoneManager> _zoneBuffer;

        public static List<VRRig> ActiveRigs()
        {
            List<VRRig> buffer = _rigBuffer ?? (_rigBuffer = new List<VRRig>(16));
            buffer.Clear();

            IReadOnlyList<VRRig> source = VRRigCache.ActiveRigs;
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

        public static List<GorillaPlayerScoreboardLine> ScoreboardLines()
        {
            List<GorillaPlayerScoreboardLine> buffer =
                _scoreboardBuffer ?? (_scoreboardBuffer = new List<GorillaPlayerScoreboardLine>(16));
            buffer.Clear();

            List<GorillaPlayerScoreboardLine> source = GorillaScoreboardTotalUpdater.allScoreboardLines;
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

        public static List<GorillaGuardianZoneManager> GuardianZones()
        {
            List<GorillaGuardianZoneManager> buffer =
                _zoneBuffer ?? (_zoneBuffer = new List<GorillaGuardianZoneManager>(8));
            buffer.Clear();

            List<GorillaGuardianZoneManager> source = GorillaGuardianZoneManager.zoneManagers;
            if (source == null)
            {
                return buffer;
            }

            for (int i = 0; i < source.Count; i++)
            {
                GorillaGuardianZoneManager zone = source[i];
                if (zone != (Object)null)
                {
                    buffer.Add(zone);
                }
            }

            return buffer;
        }

    }
}