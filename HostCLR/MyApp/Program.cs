
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("Program.Main, we get: {0}", Api.GetInfo());
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct HostApi
{
    public IntPtr test; // host_test_fn
    public IntPtr OutputLog;
    public IntPtr RunCommand;
    public IntPtr RunCommandTimeout;
    public IntPtr FindInPath;
    public IntPtr GetAdbExe;
}

// delegate for native host_test_fn
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostTestDelegation(int a, float b, string c);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public delegate void HostOutputLogDelegation(string c);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public delegate bool HostRunCommandDelegation(string cmd, string args, IntPtr result);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public delegate bool HostRunCommandTimeoutDelegation(string cmd, string args, int timeout, IntPtr result);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public delegate bool HostFindInPathDelegation(string filename, StringBuilder path, ref int path_size);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public delegate bool HostGetAdbExeDelegation(StringBuilder path, ref int path_size);

public static class Api
{
    [MethodImplAttribute(MethodImplOptions.InternalCall)]
    public extern static string GetInfo();
}

namespace DotNetLib
{
    public static class Lib
    {
        [UnmanagedCallersOnly]
        public static int RegisterApi(IntPtr apis)
        {
            HostApi hostApi = Marshal.PtrToStructure<HostApi>(apis);
            HostTestApi = Marshal.GetDelegateForFunctionPointer<HostTestDelegation>(hostApi.test);
            HostOutputLogApi = Marshal.GetDelegateForFunctionPointer<HostOutputLogDelegation>(hostApi.OutputLog);
            HostRunCommandApi = Marshal.GetDelegateForFunctionPointer<HostRunCommandDelegation>(hostApi.RunCommand);
            HostRunCommandTimeoutApi = Marshal.GetDelegateForFunctionPointer<HostRunCommandTimeoutDelegation>(hostApi.RunCommandTimeout);
            HostFindInPathApi = Marshal.GetDelegateForFunctionPointer<HostFindInPathDelegation>(hostApi.FindInPath);
            HostGetAdbExeApi = Marshal.GetDelegateForFunctionPointer<HostGetAdbExeDelegation>(hostApi.GetAdbExe);
            return 0;
        }

        public delegate int GetZipAlignArgsDelegation(string folder, StringBuilder args, ref int argsLength);
        public static int GetZipAlignArgs(string folder, StringBuilder args, ref int argsLength)
        {
            args.Clear();
            args.Append(" -a -b -c");
            argsLength = args.Length;
            return 0;
        }

        public static int HelloMono(int cmd, string arg, ref string refArg, IntPtr addr)
        {
            long v = addr.ToInt64();
            refArg = string.Format("Hello, cmd:{0}, arg:{1}, ref arg:{2}, addr:0x{3:x}", cmd, arg, refArg, v);
            return 0;
        }

        private static int s_CallCount = 1;

        [StructLayout(LayoutKind.Sequential)]
        public struct LibArgs
        {
            public IntPtr Message;
            public int Number;
        }

        public static int Hello(IntPtr arg, int argLength)
        {
            if (argLength < System.Runtime.InteropServices.Marshal.SizeOf(typeof(LibArgs))) {
                return 1;
            }

            LibArgs libArgs = Marshal.PtrToStructure<LibArgs>(arg);
            Console.WriteLine($"Hello, from {nameof(Lib)} [count: {s_CallCount++}]");
            PrintLibArgs(libArgs);
            return 0;
        }

        public delegate void CustomEntryPointDelegate(LibArgs libArgs);
        public static void CustomEntryPoint(LibArgs libArgs)
        {
            Console.WriteLine($"CustomEntryPoint, from {nameof(CustomEntryPoint)} in {nameof(Lib)}");
            PrintLibArgs(libArgs);
        }

        [UnmanagedCallersOnly]
        public static void CustomEntryPointUnmanagedCallersOnly(LibArgs libArgs)
        {
            Console.WriteLine($"CustomEntryPointUnmanagedCallersOnly, from {nameof(CustomEntryPointUnmanagedCallersOnly)} in {nameof(Lib)}");
            PrintLibArgs(libArgs);
        }

        [UnmanagedCallersOnly]
        public static void Hello2(IntPtr message)
        {
            Console.WriteLine($"Hello2, from {nameof(Lib)} [count: {++s_CallCount}]");
            Console.WriteLine($"-- message: {Marshal.PtrToStringUni(message)}");

            int r = HostTestApi(1, 2.0f, "hello2");
            Console.WriteLine($"-- HostTestApi result: {r}");
        }

#nullable enable
        private static void PrintLibArgs(LibArgs libArgs)
        {
            IntPtr msg = libArgs.Message;
            if (msg != IntPtr.Zero) {
                string? message = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Marshal.PtrToStringUni(msg)
                    : Marshal.PtrToStringUTF8(msg);

                Console.WriteLine($"-- message: {message}");
                Console.WriteLine($"-- number: {libArgs.Number}");
            }
        }
#nullable disable

        private static HostTestDelegation HostTestApi;
        private static HostOutputLogDelegation HostOutputLogApi;
        private static HostRunCommandDelegation HostRunCommandApi;
        private static HostRunCommandTimeoutDelegation HostRunCommandTimeoutApi;
        private static HostFindInPathDelegation HostFindInPathApi;
        private static HostGetAdbExeDelegation HostGetAdbExeApi;
    }
}