using Microsoft.Win32.SafeHandles;
using Windows.Win32;

namespace MiniTerm
{
    /// <summary>
    /// Utility functions around the new Pseudo Console APIs
    /// </summary>
    internal sealed class PseudoConsole : IDisposable
    {
        internal const uint PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE = 0x00020016;

        public static readonly IntPtr PseudoConsoleThreadAttribute = (IntPtr)PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE;

        public ClosePseudoConsoleSafeHandle Handle { get; }

        private PseudoConsole(ClosePseudoConsoleSafeHandle handle)
        {
            Handle = handle;
        }

        internal static PseudoConsole Create(SafeFileHandle inputReadSide, SafeFileHandle outputWriteSide, int width, int height)
        {
            var createResult = PInvoke.CreatePseudoConsole(
                new Windows.Win32.System.Console.COORD { X = (short)width, Y = (short)height },
                inputReadSide, outputWriteSide,
                0, out ClosePseudoConsoleSafeHandle hPC);
            if(createResult != 0)
            {
                throw new InvalidOperationException("Could not create pseudo console. Error Code " + createResult);
            }
            return new PseudoConsole(hPC);
        }

        public void Dispose()
        {
            Handle.Close();
        }
    }
}
