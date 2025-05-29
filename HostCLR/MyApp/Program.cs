
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

Console.WriteLine("Hello, World! reply:{0}", DotNetLib.Lib.GetInfo());

namespace DotNetLib
{
    public static class Lib
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static string GetInfo();

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
            Console.WriteLine($"Hello, world! from {nameof(Lib)} [count: {s_CallCount++}]");
            PrintLibArgs(libArgs);
            return 0;
        }

        public delegate void CustomEntryPointDelegate(LibArgs libArgs);
        public static void CustomEntryPoint(LibArgs libArgs)
        {
            Console.WriteLine($"Hello, world! from {nameof(CustomEntryPoint)} in {nameof(Lib)}");
            PrintLibArgs(libArgs);
        }

        [UnmanagedCallersOnly]
        public static void CustomEntryPointUnmanagedCallersOnly(LibArgs libArgs)
        {
            Console.WriteLine($"Hello, world! from {nameof(CustomEntryPointUnmanagedCallersOnly)} in {nameof(Lib)}");
            PrintLibArgs(libArgs);
        }

        [UnmanagedCallersOnly]
        public static void Hello2(IntPtr message)
        {
            Console.WriteLine($"Hello, world! from {nameof(Lib)} [count: {++s_CallCount}]");
            Console.WriteLine($"-- message: {Marshal.PtrToStringUni(message)}");
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
    }
}