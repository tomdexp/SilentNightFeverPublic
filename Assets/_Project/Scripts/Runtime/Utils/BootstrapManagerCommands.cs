using _Project.Scripts.Runtime.Networking;
using QFSW.QC;

namespace _Project.Scripts.Runtime.Utils
{
    public static class BootstrapManagerCommands
    {
        [Command("/online.leave", "Leaves the online session.")]
        public static void TryLeaveOnline()
        {
            if (BootstrapManager.HasInstance) BootstrapManager.Instance.TryLeaveOnline();
        }
    }
}