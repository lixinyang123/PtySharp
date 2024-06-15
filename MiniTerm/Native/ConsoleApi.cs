using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace MiniTerm.Native
{
    /// <summary>
    /// PInvoke signatures for win32 console api
    /// </summary>
    static class ConsoleApi
    {
        internal const int STD_OUTPUT_HANDLE = -11;
        internal const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        internal const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
        internal delegate bool ConsoleEventDelegate(CtrlTypes ctrlType);

        internal enum CtrlTypes : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }
    }
}
