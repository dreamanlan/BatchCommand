using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ShellApi
{
    internal enum BOOL : int
    {
        FALSE = 0,
        TRUE = 1,
    }

    internal static partial class ComCtl32
    {
        public delegate IntPtr SUBCLASSPROC(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            UIntPtr uIdSubclass,
            UIntPtr dwRefData
        );

        [System.Runtime.InteropServices.DllImport("comctl32.dll", ExactSpelling = true)]
        public static extern BOOL SetWindowSubclass(
            IntPtr hWnd,
            IntPtr pfnSubclass,
            UIntPtr uIdSubclass,
            UIntPtr dwRefData
        );

        [System.Runtime.InteropServices.DllImport("comctl32.dll", ExactSpelling = true)]
        public static extern BOOL RemoveWindowSubclass(
            IntPtr hWnd,
            IntPtr pfnSubclass,
            UIntPtr uIdSubclass
        );

        [System.Runtime.InteropServices.DllImport("comctl32.dll", ExactSpelling = true)]
        public static extern IntPtr DefSubclassProc(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam
        );
    }

    public class SubclassedWindow : MarshalByRefObject, System.Windows.Forms.IWin32Window
    {
        // prevents collection of SubclassedWindow that is still in use
        static private System.Collections.Generic.HashSet<SubclassedWindow> _instancesInUse = new System.Collections.Generic.HashSet<SubclassedWindow>();

        // The number of uses we still have for this instances:
        // - some window attached, or
        // - inside a window procedure
        private int _uses = 0;

        // Our window procedure delegate
        private ComCtl32.SUBCLASSPROC _windowProc;

        // The native handle for our delegate
        private IntPtr _windowProcHandle;

        static SubclassedWindow()
        {
            AppDomain.CurrentDomain.ProcessExit += OnShutdown;
        }

        public SubclassedWindow()
        {
            _windowProc = new ComCtl32.SUBCLASSPROC(Callback);
            _windowProcHandle = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(_windowProc);
        }

        /// <summary>
        ///  Gets the handle for this window.
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        ///  Assigns a handle to this <see cref="NativeWindow"/> instance.
        /// </summary>
        public void AssignHandle(IntPtr handle)
        {
            CheckReleased();
            Debug.Assert(handle != IntPtr.Zero, "handle is 0");

            if (0 == _uses) {
                lock (_instancesInUse) {
                    _instancesInUse.Add(this);
                }
            } // else may happen if handle gets reassigned inside WndProc.
            // This is legal after any call to DefWndProc.

            ++_uses;
            Handle = handle;

            ComCtl32.SetWindowSubclass(handle, _windowProcHandle, UIntPtr.Zero, UIntPtr.Zero);
            OnHandleChange();
        }

        /// <summary>
        ///  Window message callback method. Control arrives here when a window
        ///  message is sent to this Window. This method packages the window message
        ///  in a Message object and invokes the WndProc() method. A WM_NCDESTROY
        ///  message automatically causes the ReleaseHandle() method to be called.
        /// </summary>
        private IntPtr Callback(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            UIntPtr uIdSubclass,
            UIntPtr dwRefData
        )
        {
            Debug.Assert(0 < _uses);
            ++_uses;

            try {
                var m = System.Windows.Forms.Message.Create(hWnd, msg, wParam, lParam);
                WndProc(ref m);
                return m.Result;
            }
            catch (Exception e) {
                OnThreadException(e);
                return IntPtr.Zero;
            }
            finally {
                if (msg == 0x82/*WM_NCDESTROY*/ && Handle != IntPtr.Zero) {
                    InternalReleaseHandle();
                }
                if (0 == --_uses) {
                    lock (_instancesInUse) {
                        _instancesInUse.Remove(this);
                    }
                }
            }
        }

        /// <summary>
        ///  Raises an exception if the window handle is not zero.
        /// </summary>
        private void CheckReleased()
        {
            if (Handle != IntPtr.Zero) {
                throw new InvalidOperationException("Window handle already exists.");
            }
        }

        /// <summary>
        ///  Invokes the default window procedure associated with this Window. It is
        ///  an error to call this method when the Handle property is zero.
        /// </summary>
        public void DefWndProc(ref System.Windows.Forms.Message m)
        {
            Debug.Assert(m.HWnd == Handle, "SubclassedWindow is not attached to the window m is addressed to.");
            m.Result = ComCtl32.DefSubclassProc(m.HWnd, m.Msg, m.WParam, m.LParam);
        }

        /// <summary>
        ///  Specifies a notification method that is called when the handle for a
        ///  window is changed.
        /// </summary>
        protected virtual void OnHandleChange()
        {
        }

        /// <summary>
        ///  On class load, we connect an event to Application to let us know when
        ///  the process or domain terminates.  When this happens, we attempt to
        ///  clear our window class cache.  We cannot destroy windows (because we don't
        ///  have access to their thread), and we cannot unregister window classes
        ///  (because the classes are in use by the windows we can't destroy).  Instead,
        ///  we move the class and window procs to DefWndProc
        /// </summary>
        [System.Runtime.ConstrainedExecution.PrePrepareMethod]
        private static void OnShutdown(object sender, EventArgs e)
        {
            // No lock because access here should be race-free, no concurrent SubclassedWindow.AttachHandle/ReleaseHandle
            // should happen while shutting down.
            Debug.Assert(0 == _instancesInUse.Count);
        }

        /// <summary>
        ///  When overridden in a derived class, manages an unhandled thread exception.
        /// </summary>
        protected virtual void OnThreadException(Exception e)
        {
        }

        private void InternalReleaseHandle()
        {
            Debug.Assert(Handle != IntPtr.Zero);
            ComCtl32.RemoveWindowSubclass(Handle, _windowProcHandle, UIntPtr.Zero);
            Handle = IntPtr.Zero;
            OnHandleChange();
            --_uses;
        }

        /// <summary>
        ///  Releases the handle associated with this window.
        /// </summary>
        public void ReleaseHandle()
        {
            if (Handle != IntPtr.Zero) {
                InternalReleaseHandle();
                if (0 == _uses) {
                    lock (_instancesInUse) {
                        _instancesInUse.Remove(this);
                    }
                }
            }
        }

        /// <summary>
        ///  Invokes the default window procedure associated with this window.
        /// </summary>
        protected virtual void WndProc(ref System.Windows.Forms.Message m)
        {
            DefWndProc(ref m);
        }
    }
}