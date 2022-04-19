﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;

namespace WSLIPConf.Helpers
{
    public static class UnelevatedProcessStarter
    {
        public static void PrintIP()
        {
            var cmd = Process.GetCurrentProcess().MainModule.FileName + " /printip";
            Start(cmd);

        }

        public static void Start(string cmdArgs)
        {
            // 1. Get the shell
            var shell = NativeMethods.GetShellWindow();
            if (shell == IntPtr.Zero)
            {
                throw new Exception("Could not find shell window");
            }

            // 2. Copy the access token of the process
            NativeMethods.GetWindowThreadProcessId(shell, out uint shellProcessId);
            var hShellProcess = NativeMethods.OpenProcess(0x00000400 /* QueryInformation */, false, (int)shellProcessId);
            if (!NativeMethods.OpenProcessToken(hShellProcess, 2 /* TOKEN_DUPLICATE */, out IntPtr hShellToken))
            {
                throw new Win32Exception();
            }

            // 3. Dublicate the acess token
            uint tokenAccess = 8 /*TOKEN_QUERY*/ | 1 /*TOKEN_ASSIGN_PRIMARY*/ | 2 /*TOKEN_DUPLICATE*/ | 0x80 /*TOKEN_ADJUST_DEFAULT*/ | 0x100 /*TOKEN_ADJUST_SESSIONID*/;
            var securityAttributes = new SecurityAttributes();

            NativeMethods.DuplicateTokenEx(
                hShellToken,
                tokenAccess,
                ref securityAttributes,
                2 /* SecurityImpersonation */,
                1 /* TokenPrimary */,
                out IntPtr hToken);

            // 4. Create a new process with the copied token
            var si = new Startupinfo();
            si.cb = Marshal.SizeOf(si);
            var proc = new Process();

            if (!NativeMethods.CreateProcessWithTokenW(
                hToken,
                0x00000002 /* LogonNetcredentialsOnly */,
                null,
                cmdArgs,
                0x00000010 /* CreateNewConsole */,
                IntPtr.Zero,
                null,
                ref si,
                out ProcessInformation _))
            {
                throw new Win32Exception();
            }
        }

        public class NativeMethods
        {

            [DllImport("user32.dll")]
            public static extern IntPtr GetShellWindow();
            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr OpenProcess(int processAccess, bool bInheritHandle, int processId);
            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool OpenProcessToken(IntPtr processHandle, UInt32 desiredAccess, out IntPtr tokenHandle);
            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess,
                ref SecurityAttributes lpTokenAttributes,
                int impersonationLevel,
                int tokenType,
                out IntPtr phNewToken);
            [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool CreateProcessWithTokenW(
                IntPtr hToken, int dwLogonFlags,
                string lpApplicationName, string lpCommandLine,
                int dwCreationFlags, IntPtr lpEnvironment,
                string lpCurrentDirectory, [In] ref Startupinfo lpStartupInfo, out ProcessInformation lpProcessInformation);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessInformation
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SecurityAttributes
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct Startupinfo
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
    }
}