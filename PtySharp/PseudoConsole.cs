using Microsoft.Win32.SafeHandles;
using static PtySharp.Native.PseudoConsoleApi;

namespace PtySharp
{
    /// <summary>
    /// Utility functions around the new Pseudo Console APIs
    /// </summary>
    internal sealed class PseudoConsole : IDisposable
    {
        public static readonly IntPtr PseudoConsoleThreadAttribute = (IntPtr)PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE;

        public IntPtr Handle { get; }

        private PseudoConsole(IntPtr handle)
        {
            Handle = handle;
        }

        internal static PseudoConsole Create(SafeFileHandle inputReadSide, SafeFileHandle outputWriteSide, int width, int height)
        {
            var createResult = CreatePseudoConsole(
                size: new COORD { X = (short)width, Y = (short)height },
                hInput: inputReadSide,
                hOutput: outputWriteSide,
                dwFlags: 0,
                phPC: out var phpc
            );

            if (createResult != 0)
            {
                throw new InvalidOperationException("Could not create pseudo console. Error Code " + createResult);
            }

            return new PseudoConsole(phpc);
        }

        public void Dispose()
        {
            ClosePseudoConsole(Handle);
        }
    }
}
