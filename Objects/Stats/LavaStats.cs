using System;

namespace Victoria.Objects.Stats
{
    public sealed class LavaStats
    {
        public long RamFree { get; private set; }
        public long RamUsed { get; private set; }
        public int TotalPlayers { get; private set; }
        public int ActivePlayers { get; private set; }
        public int CpuCoreCount { get; private set; }
        public TimeSpan Uptime { get; private set; }
        public long RamAllocated { get; private set; }
        public long RamReservable { get; private set; }
        public double CpuSystemLoad { get; private set; }
        public double CpuLavalinkLoad { get; private set; }
        public int AverageSentFramesPerMinute { get; private set; }
        public int AverageNulledFramesPerMinute { get; private set; }
        public int AverageDeficitFramesPerMinute { get; private set; }

        internal LavaStats()
        {
        }

        internal void Update(Server server)
        {
            ActivePlayers = server.ActivePlayers;
            TotalPlayers = server.TotalPlayers;
            Uptime = server.Uptime;

            CpuCoreCount = server.CPU.Cores;
            CpuSystemLoad = server.CPU.SystemLoad;
            CpuLavalinkLoad = server.CPU.LavalinkLoad;

            RamReservable = server.Memory.Reservable;
            RamUsed = server.Memory.Used;
            RamFree = server.Memory.Free;
            RamAllocated = server.Memory.Allocated;
            RamReservable = server.Memory.Reservable;

            AverageSentFramesPerMinute = server.Frames?.Sent ?? 0;
            AverageNulledFramesPerMinute = server.Frames?.Nulled ?? 0;
            AverageDeficitFramesPerMinute = server.Frames?.Deficit ?? 0;
        }
    }
}