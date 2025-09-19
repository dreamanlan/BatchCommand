
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

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
}

// delegate for native host_test_fn
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostTestDelegate(int a, float b, string c);

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
            HostTestApi = Marshal.GetDelegateForFunctionPointer<HostTestDelegate>(hostApi.test);

            return 1;
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

        private static HostTestDelegate HostTestApi;
    }
}