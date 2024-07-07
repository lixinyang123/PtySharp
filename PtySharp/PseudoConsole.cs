using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Console;

namespace PtySharp
{
    /// <summary>
    /// Utility functions around the new Pseudo Console APIs
    /// </summary>
    internal sealed class PseudoConsole : IDisposable
    {
        public HPCON Handle { get; }

        private PseudoConsole(HPCON handle)
        {
            Handle = handle;
        }

        internal unsafe static PseudoConsole Create(SafeFileHandle inputReadSide, SafeFileHandle outputWriteSide, int width, int height)
        {
            var hpcon = new HPCON();

            var createResult = PInvoke.CreatePseudoConsole(
                size: new COORD { X = (short)width, Y = (short)height },
                hInput: new HANDLE(inputReadSide.DangerousGetHandle()),
                hOutput: new HANDLE(outputWriteSide.DangerousGetHandle()),
                dwFlags: 0,
                phPC: &hpcon
            );

            if (createResult != 0)
            {
                throw new InvalidOperationException("Could not create pseudo console. Error Code " + createResult);
            }

            return new PseudoConsole(hpcon);
        }

        public void Dispose()
        {
            PInvoke.ClosePseudoConsole(Handle);
        }
    }
}
