using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using Windows.Win32.Security;
using Windows.Win32.System.Console;

namespace PtySharp.Processes
{
    /// <summary>
    /// Support for starting and configuring processes.
    /// </summary>
    /// <remarks>
    /// Possible to replace with managed code? The key is being able to provide the PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE attribute
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416")]
    static class ProcessFactory
    {
        /// <summary>
        /// Start and configure a process. The return value represents the process and should be disposed.
        /// </summary>
        internal static Process Start(string command, HPCON hpcon)
        {
            STARTUPINFOEXW startupInfo = ConfigureProcessThread(hpcon);
            var processInfo = RunProcess(ref startupInfo, command);
            return new Process(startupInfo, processInfo);
        }

        private unsafe static STARTUPINFOEXW ConfigureProcessThread(HPCON hpcon)
        {
            // this method implements the behavior described in https://docs.microsoft.com/en-us/windows/console/creating-a-pseudoconsole-session#preparing-for-creation-of-the-child-process
            nuint lpSize = UIntPtr.Zero;

            BOOL success = PInvoke.InitializeProcThreadAttributeList
            (
                lpAttributeList: new LPPROC_THREAD_ATTRIBUTE_LIST(),
                dwAttributeCount: 1,
                dwFlags: 0,
                lpSize: &lpSize
            );

            if (success || lpSize == UIntPtr.Zero) // we're not expecting `success` here, we just want to get the calculated lpSize
            {
               throw new InvalidOperationException("Could not calculate the number of bytes for the attribute list. " + Marshal.GetLastWin32Error());
            }

            var startupInfo = new STARTUPINFOEXW();
            startupInfo.StartupInfo.cb = (uint)sizeof(STARTUPINFOEXW);
            startupInfo.lpAttributeList = new LPPROC_THREAD_ATTRIBUTE_LIST(NativeMemory.Alloc(lpSize));

            success = PInvoke.InitializeProcThreadAttributeList(
                lpAttributeList: startupInfo.lpAttributeList,
                dwAttributeCount: 1,
                dwFlags: 0,
                lpSize: &lpSize
            );

            if (!success)
            {
               throw new InvalidOperationException("Could not set up attribute list. " + Marshal.GetLastWin32Error());
            }

            success = PInvoke.UpdateProcThreadAttribute(
                lpAttributeList: startupInfo.lpAttributeList,
                dwFlags: 0,
                Attribute: 0x00020016,
                lpValue: hpcon.Value.ToPointer(),
                cbSize: (UIntPtr)IntPtr.Size,
                lpPreviousValue: UIntPtr.Zero.ToPointer(),
                lpReturnSize: UIntPtr.Zero
            );

            // if (!success)
            // {
            //    throw new InvalidOperationException("Could not set pseudoconsole thread attribute. " + Marshal.GetLastWin32Error());
            // }

            return startupInfo;
        }

        private unsafe static PROCESS_INFORMATION RunProcess(ref STARTUPINFOEXW sInfoEx, string commandLine)
        {
            uint securityAttributeSize = (uint)sizeof(SECURITY_ATTRIBUTES);
            var pSec = new SECURITY_ATTRIBUTES { nLength = securityAttributeSize };
            var tSec = new SECURITY_ATTRIBUTES { nLength = securityAttributeSize };

            var lpEnvironment = IntPtr.Zero;
            Span<char> lpCommand = [.. commandLine, '\0'];

            var success = PInvoke.CreateProcess(
                lpApplicationName: null,
                lpCommandLine: ref lpCommand,
                lpProcessAttributes: pSec,
                lpThreadAttributes: tSec,
                bInheritHandles: false,
                dwCreationFlags: PROCESS_CREATION_FLAGS.EXTENDED_STARTUPINFO_PRESENT,
                lpEnvironment: &lpEnvironment,
                lpCurrentDirectory: null,
                lpStartupInfo: sInfoEx.StartupInfo,
                lpProcessInformation: out PROCESS_INFORMATION pInfo
            );

            if (!success)
            {
                throw new InvalidOperationException("Could not create process. " + Marshal.GetLastWin32Error());
            }

            return pInfo;
        }
    }
}
