using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.Security;

namespace PtySharp
{
    /// <summary>
    /// A pipe used to talk to the pseudoconsole, as described in:
    /// https://docs.microsoft.com/en-us/windows/console/creating-a-pseudoconsole-session
    /// </summary>
    /// <remarks>
    /// We'll have two instances of this class, one for input and one for output.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416")]
    internal sealed class PseudoConsolePipe : IDisposable
    {
        public readonly SafeFileHandle ReadSide;
        public readonly SafeFileHandle WriteSide;

        public PseudoConsolePipe()
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("OperatingSystem is not support");
            }

            if (!PInvoke.CreatePipe(out ReadSide, out WriteSide, new SECURITY_ATTRIBUTES(), 0))
            {
                throw new InvalidOperationException("failed to create pipe");
            }
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReadSide?.Dispose();
                WriteSide?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
