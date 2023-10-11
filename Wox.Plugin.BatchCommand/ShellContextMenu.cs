using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;

namespace ShellApi
{
    /// <example>
    ///    ShellContextMenu scm = new ShellContextMenu();
    ///    FileInfo[] files = new FileInfo[1];
    ///    files[0] = new FileInfo(@"c:\windows\notepad.exe");
    ///    scm.ShowContextMenu(files, ctrl, shift, x, y);
    /// </example>
    public class ShellContextMenu
    {
        #region Constructor
        /// <summary>Default constructor</summary>
        public ShellContextMenu()
        {
        }
        #endregion

        #region Destructor
        /// <summary>Ensure all resources get released</summary>
        ~ShellContextMenu()
        {
            ReleaseAll();
        }
        #endregion

        #region ShowContextMenu()
        public void SetHwnd(IntPtr hwnd)
        {
            m_Hwnd = hwnd;
        }
        public string ShowContextMenu(FileInfo[] files, bool ctrl, bool shift)
        {
            // Release all resources first.
            ReleaseAll();
            m_PIDLs = GetPIDLs(files);
            return this.ShowContextMenu(ctrl, shift);
        }
        public string ShowContextMenu(DirectoryInfo[] dirs, bool ctrl, bool shift)
        {
            // Release all resources first.
            ReleaseAll();
            m_PIDLs = GetPIDLs(dirs);
            return this.ShowContextMenu(ctrl, shift);
        }
        private string ShowContextMenu(bool ctrl, bool shift)
        {
            string errMsg = string.Empty;
            IntPtr pMenu = IntPtr.Zero,
                iContextMenuPtr = IntPtr.Zero,
                iContextMenuPtr2 = IntPtr.Zero,
                iContextMenuPtr3 = IntPtr.Zero;

            try {
                if (null == m_PIDLs) {
                    ReleaseAll();
                    return "PIDLs is null";
                }

                if (false == GetContextMenuInterfaces(m_ParentFolder, m_PIDLs, out iContextMenuPtr)) {
                    ReleaseAll();
                    return "GetContextMenuInterfaces failed";
                }

                pMenu = CreatePopupMenu();

                int nResult = m_ContextMenu.QueryContextMenu(
                    pMenu,
                    0,
                    CMD_FIRST,
                    CMD_LAST,
                    CMF.EXPLORE |
                    CMF.NORMAL |
                    (shift ? CMF.EXTENDEDVERBS : 0));

                Marshal.QueryInterface(iContextMenuPtr, ref IID_IContextMenu2, out iContextMenuPtr2);
                Marshal.QueryInterface(iContextMenuPtr, ref IID_IContextMenu3, out iContextMenuPtr3);

                m_ContextMenu2 = (IContextMenu2)Marshal.GetTypedObjectForIUnknown(iContextMenuPtr2, typeof(IContextMenu2));
                m_ContextMenu3 = (IContextMenu3)Marshal.GetTypedObjectForIUnknown(iContextMenuPtr3, typeof(IContextMenu3));

                if (m_Hwnd == IntPtr.Zero || !IsWindow(m_Hwnd)) {
                    var p = Process.GetCurrentProcess();
                    //p.MainWindowHandle这个句柄好像不太稳定
                    //m_Hwnd = p.MainWindowHandle;
                    if (m_Hwnd == IntPtr.Zero || !IsWindow(m_Hwnd)) {
                        var hwnds = EnumToplevelWindows();
                        foreach (var hwnd in hwnds) {
                            var tid = GetWindowThreadProcessId(hwnd, out var pid);
                            if (pid == p.Id) {
                                m_Hwnd = hwnd;
                                break;
                            }
                        }
                    }
                    if (m_Hwnd == IntPtr.Zero || !IsWindow(m_Hwnd)) {
                        var hwnds = EnumProcessWindows(p.Id);
                        foreach(var hwnd in hwnds) {
                            if (IsWindow(hwnd)) {
                                m_Hwnd = hwnd;
                                break;
                            }
                        }
                    }
                }
                POINT pt;
                GetCursorPos(out pt);
                uint nSelected = TrackPopupMenuEx(
                    pMenu,
                    TPM.RETURNCMD,
                    pt.x,
                    pt.y,
                    m_Hwnd,
                    IntPtr.Zero);
                int lastError = GetLastError();
                if (lastError != 0) {
                    errMsg = new Win32Exception(lastError).Message;
                }

                DestroyMenu(pMenu);
                pMenu = IntPtr.Zero;

                if (nSelected != 0) {
                    InvokeCommand(m_ContextMenu, nSelected, m_ParentFolderPath, ctrl, shift, pt.x, pt.y);
                }
            }
            catch {
                throw;
            }
            finally {
                if (pMenu != IntPtr.Zero) {
                    DestroyMenu(pMenu);
                }

                if (iContextMenuPtr != IntPtr.Zero)
                    Marshal.Release(iContextMenuPtr);

                if (iContextMenuPtr2 != IntPtr.Zero)
                    Marshal.Release(iContextMenuPtr2);

                if (iContextMenuPtr3 != IntPtr.Zero)
                    Marshal.Release(iContextMenuPtr3);

                ReleaseAll();
            }
            return errMsg;
        }
        #endregion

        public static IList<IntPtr> EnumToplevelWindows()
        {
            var handles = new List<IntPtr>();
            EnumWindows((hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
            return handles;
        }
        public static IList<IntPtr> EnumProcessWindows(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                EnumThreadWindows(thread.Id,
                    (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

            return handles;
        }

        #region GetContextMenuInterfaces()
        /// <summary>Gets the interfaces to the context menu</summary>
        /// <param name="oParentFolder">Parent folder</param>
        /// <param name="arrPIDLs">PIDLs</param>
        /// <returns>true if it got the interfaces, otherwise false</returns>
        private bool GetContextMenuInterfaces(IShellFolder oParentFolder, IntPtr[] arrPIDLs, out IntPtr ctxMenuPtr)
        {
            int nResult = oParentFolder.GetUIObjectOf(
                IntPtr.Zero,
                (uint)arrPIDLs.Length,
                arrPIDLs,
                ref IID_IContextMenu,
                IntPtr.Zero,
                out ctxMenuPtr);

            if (S_OK == nResult) {
                m_ContextMenu = (IContextMenu)Marshal.GetTypedObjectForIUnknown(ctxMenuPtr, typeof(IContextMenu));

                return true;
            }
            else {
                ctxMenuPtr = IntPtr.Zero;
                m_ContextMenu = null;
                return false;
            }
        }
        #endregion

        #region InvokeCommand
        private void InvokeCommand(IContextMenu oContextMenu, uint nCmd, string strFolder, bool ctrl, bool shift, int x, int y)
        {
            CMINVOKECOMMANDINFOEX invoke = new CMINVOKECOMMANDINFOEX();
            invoke.cbSize = s_InvokeCommand;
            invoke.lpVerb = (IntPtr)(nCmd - CMD_FIRST);
            invoke.lpDirectory = strFolder;
            invoke.lpVerbW = (IntPtr)(nCmd - CMD_FIRST);
            invoke.lpDirectoryW = strFolder;
            invoke.fMask = CMIC.UNICODE | CMIC.PTINVOKE |
                (ctrl ? CMIC.CONTROL_DOWN : 0) |
                (shift ? CMIC.SHIFT_DOWN : 0);
            invoke.ptInvoke = new POINT(x, y);
            invoke.nShow = SW.SHOWNORMAL;

            oContextMenu.InvokeCommand(ref invoke);
        }
        #endregion

        #region ReleaseAll()
        /// <summary>
        /// Release all allocated interfaces, PIDLs 
        /// </summary>
        private void ReleaseAll()
        {
            if (null != m_ContextMenu) {
                Marshal.ReleaseComObject(m_ContextMenu);
                m_ContextMenu = null;
            }
            if (null != m_ContextMenu2) {
                Marshal.ReleaseComObject(m_ContextMenu2);
                m_ContextMenu2 = null;
            }
            if (null != m_ContextMenu3) {
                Marshal.ReleaseComObject(m_ContextMenu3);
                m_ContextMenu3 = null;
            }
            if (null != m_DesktopFolder) {
                Marshal.ReleaseComObject(m_DesktopFolder);
                m_DesktopFolder = null;
            }
            if (null != m_ParentFolder) {
                Marshal.ReleaseComObject(m_ParentFolder);
                m_ParentFolder = null;
            }
            if (null != m_PIDLs) {
                FreePIDLs(m_PIDLs);
                m_PIDLs = null;
            }
        }
        #endregion

        #region GetDesktopFolder()
        /// <summary>
        /// Gets the desktop folder
        /// </summary>
        /// <returns>IShellFolder for desktop folder</returns>
        private IShellFolder GetDesktopFolder()
        {
            IntPtr pUnkownDesktopFolder = IntPtr.Zero;

            if (null == m_DesktopFolder) {
                // Get desktop IShellFolder
                int nResult = SHGetDesktopFolder(out pUnkownDesktopFolder);
                if (S_OK != nResult) {
                    throw new ShellContextMenuException("Failed to get the desktop shell folder");
                }
                m_DesktopFolder = (IShellFolder)Marshal.GetTypedObjectForIUnknown(pUnkownDesktopFolder, typeof(IShellFolder));
            }

            return m_DesktopFolder;
        }
        #endregion

        #region GetParentFolder()
        /// <summary>
        /// Gets the parent folder
        /// </summary>
        /// <param name="folderName">Folder path</param>
        /// <returns>IShellFolder for the folder (relative from the desktop)</returns>
        private IShellFolder GetParentFolder(string folderName)
        {
            if (null == m_ParentFolder) {
                IShellFolder oDesktopFolder = GetDesktopFolder();
                if (null == oDesktopFolder) {
                    return null;
                }

                // Get the PIDL for the folder file is in
                IntPtr pPIDL = IntPtr.Zero;
                uint pchEaten = 0;
                SFGAO pdwAttributes = 0;
                int nResult = oDesktopFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, folderName, ref pchEaten, out pPIDL, ref pdwAttributes);
                if (S_OK != nResult) {
                    return null;
                }

                IntPtr pStrRet = Marshal.AllocCoTaskMem(MAX_PATH * 2 + 4);
                Marshal.WriteInt32(pStrRet, 0, 0);
                nResult = m_DesktopFolder.GetDisplayNameOf(pPIDL, SHGNO.FORPARSING, pStrRet);
                StringBuilder strFolder = new StringBuilder(MAX_PATH);
                StrRetToBuf(pStrRet, pPIDL, strFolder, MAX_PATH);
                Marshal.FreeCoTaskMem(pStrRet);
                pStrRet = IntPtr.Zero;
                m_ParentFolderPath = strFolder.ToString();

                // Get the IShellFolder for folder
                IntPtr pUnknownParentFolder = IntPtr.Zero;
                nResult = oDesktopFolder.BindToObject(pPIDL, IntPtr.Zero, ref IID_IShellFolder, out pUnknownParentFolder);
                // Free the PIDL first
                Marshal.FreeCoTaskMem(pPIDL);
                if (S_OK != nResult) {
                    return null;
                }
                m_ParentFolder = (IShellFolder)Marshal.GetTypedObjectForIUnknown(pUnknownParentFolder, typeof(IShellFolder));
            }

            return m_ParentFolder;
        }
        #endregion

        #region GetPIDLs()
        /// <summary>
        /// Get the PIDLs
        /// </summary>
        /// <param name="arrFI">Array of FileInfo</param>
        /// <returns>Array of PIDLs</returns>
        protected IntPtr[] GetPIDLs(FileInfo[] arrFI)
        {
            if (null == arrFI || 0 == arrFI.Length) {
                return null;
            }

            IShellFolder oParentFolder = GetParentFolder(arrFI[0].DirectoryName);
            if (null == oParentFolder) {
                return null;
            }

            IntPtr[] arrPIDLs = new IntPtr[arrFI.Length];
            int n = 0;
            foreach (FileInfo fi in arrFI) {
                // Get the file relative to folder
                uint pchEaten = 0;
                SFGAO pdwAttributes = 0;
                IntPtr pPIDL = IntPtr.Zero;
                int nResult = oParentFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, fi.Name, ref pchEaten, out pPIDL, ref pdwAttributes);
                if (S_OK != nResult) {
                    FreePIDLs(arrPIDLs);
                    return null;
                }
                arrPIDLs[n] = pPIDL;
                n++;
            }

            return arrPIDLs;
        }

        /// <summary>
        /// Get the PIDLs
        /// </summary>
        /// <param name="arrFI">Array of DirectoryInfo</param>
        /// <returns>Array of PIDLs</returns>
        protected IntPtr[] GetPIDLs(DirectoryInfo[] arrFI)
        {
            if (null == arrFI || 0 == arrFI.Length) {
                return null;
            }

            IShellFolder oParentFolder = GetParentFolder(arrFI[0].Parent.FullName);
            if (null == oParentFolder) {
                return null;
            }

            IntPtr[] arrPIDLs = new IntPtr[arrFI.Length];
            int n = 0;
            foreach (DirectoryInfo fi in arrFI) {
                // Get the file relative to folder
                uint pchEaten = 0;
                SFGAO pdwAttributes = 0;
                IntPtr pPIDL = IntPtr.Zero;
                int nResult = oParentFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, fi.Name, ref pchEaten, out pPIDL, ref pdwAttributes);
                if (S_OK != nResult) {
                    FreePIDLs(arrPIDLs);
                    return null;
                }
                arrPIDLs[n] = pPIDL;
                n++;
            }

            return arrPIDLs;
        }
        #endregion

        #region FreePIDLs()
        /// <summary>
        /// Free the PIDLs
        /// </summary>
        /// <param name="arrPIDLs">Array of PIDLs (IntPtr)</param>
        protected void FreePIDLs(IntPtr[] arrPIDLs)
        {
            if (null != arrPIDLs) {
                for (int n = 0; n < arrPIDLs.Length; n++) {
                    if (arrPIDLs[n] != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(arrPIDLs[n]);
                        arrPIDLs[n] = IntPtr.Zero;
                    }
                }
            }
        }
        #endregion

        #region Local variabled
        private IntPtr m_Hwnd;
        private IContextMenu m_ContextMenu;
        private IContextMenu2 m_ContextMenu2;
        private IContextMenu3 m_ContextMenu3;
        private IShellFolder m_DesktopFolder;
        private IShellFolder m_ParentFolder;
        private IntPtr[] m_PIDLs;
        private string m_ParentFolderPath;
        #endregion

        #region Variables and Constants

        private const int MAX_PATH = 260;
        private const uint CMD_FIRST = 1;
        private const uint CMD_LAST = 30000;

        private const int S_OK = 0;
        private const int S_FALSE = 1;

        private static int s_MenuItemInfo = Marshal.SizeOf(typeof(MENUITEMINFO));
        private static int s_InvokeCommand = Marshal.SizeOf(typeof(CMINVOKECOMMANDINFOEX));

        #endregion

        #region DLL Import

        // Retrieves the IShellFolder interface for the desktop folder, which is the root of the Shell's namespace.
        [DllImport("shell32.dll")]
        private static extern Int32 SHGetDesktopFolder(out IntPtr ppshf);

        // Takes a STRRET structure returned by IShellFolder::GetDisplayNameOf, converts it to a string, and places the result in a buffer. 
        [DllImport("shlwapi.dll", EntryPoint = "StrRetToBuf", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Int32 StrRetToBuf(IntPtr pstr, IntPtr pidl, StringBuilder pszBuf, int cchBuf);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern int GetCursorPos(out POINT pPoint);

        // The TrackPopupMenuEx function displays a shortcut menu at the specified location and tracks the selection of items on the shortcut menu. The shortcut menu can appear anywhere on the screen.
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern uint TrackPopupMenuEx(IntPtr hmenu, TPM flags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        // The CreatePopupMenu function creates a drop-down menu, submenu, or shortcut menu. The menu is initially empty. You can insert or append menu items by using the InsertMenuItem function. You can also use the InsertMenu function to insert menu items and the AppendMenu function to append menu items.
        [DllImport("user32", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreatePopupMenu();

        // The DestroyMenu function destroys the specified menu and frees any memory that the menu occupies.
        [DllImport("user32", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DestroyMenu(IntPtr hMenu);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetLastError();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private delegate bool EnumThreadCallback(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadCallback lpfn, IntPtr lParam);

        private delegate bool EnumWindowsCallback(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int EnumWindows(EnumWindowsCallback callPtr, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        #endregion

        #region Shell GUIDs

        private static Guid IID_IShellFolder = new Guid("{000214E6-0000-0000-C000-000000000046}");
        private static Guid IID_IContextMenu = new Guid("{000214e4-0000-0000-c000-000000000046}");
        private static Guid IID_IContextMenu2 = new Guid("{000214f4-0000-0000-c000-000000000046}");
        private static Guid IID_IContextMenu3 = new Guid("{bcfce0a0-ec17-11d0-8d10-00a0c90f2719}");

        #endregion

        #region Structs

        // Contains extended information about a shortcut menu command
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CMINVOKECOMMANDINFOEX
        {
            public int cbSize;
            public CMIC fMask;
            public IntPtr hwnd;
            public IntPtr lpVerb;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpDirectory;
            public SW nShow;
            public int dwHotKey;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpTitle;
            public IntPtr lpVerbW;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpParametersW;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpDirectoryW;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpTitleW;
            public POINT ptInvoke;
        }

        // Contains information about a menu item
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MENUITEMINFO
        {
            public MENUITEMINFO(string text)
            {
                cbSize = s_MenuItemInfo;
                dwTypeData = text;
                cch = text.Length;
                fMask = 0;
                fType = 0;
                fState = 0;
                wID = 0;
                hSubMenu = IntPtr.Zero;
                hbmpChecked = IntPtr.Zero;
                hbmpUnchecked = IntPtr.Zero;
                dwItemData = IntPtr.Zero;
                hbmpItem = IntPtr.Zero;
            }

            public int cbSize;
            public MIIM fMask;
            public MFT fType;
            public MFS fState;
            public uint wID;
            public IntPtr hSubMenu;
            public IntPtr hbmpChecked;
            public IntPtr hbmpUnchecked;
            public IntPtr dwItemData;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string dwTypeData;
            public int cch;
            public IntPtr hbmpItem;
        }

        // A generalized global memory handle used for data transfer operations by the 
        // IAdviseSink, IDataObject, and IOleCache interfaces
        [StructLayout(LayoutKind.Sequential)]
        private struct STGMEDIUM
        {
            public TYMED tymed;
            public IntPtr hBitmap;
            public IntPtr hMetaFilePict;
            public IntPtr hEnhMetaFile;
            public IntPtr hGlobal;
            public IntPtr lpszFileName;
            public IntPtr pstm;
            public IntPtr pstg;
            public IntPtr pUnkForRelease;
        }

        // Defines the x- and y-coordinates of a point
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct POINT
        {
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public int x;
            public int y;
        }

        #endregion

        #region Enums

        // Defines the values used with the IShellFolder::GetDisplayNameOf and IShellFolder::SetNameOf 
        // methods to specify the type of file or folder names used by those methods
        [Flags]
        private enum SHGNO
        {
            NORMAL = 0x0000,
            INFOLDER = 0x0001,
            FOREDITING = 0x1000,
            FORADDRESSBAR = 0x4000,
            FORPARSING = 0x8000
        }

        // The attributes that the caller is requesting, when calling IShellFolder::GetAttributesOf
        [Flags]
        private enum SFGAO : uint
        {
            BROWSABLE = 0x8000000,
            CANCOPY = 1,
            CANDELETE = 0x20,
            CANLINK = 4,
            CANMONIKER = 0x400000,
            CANMOVE = 2,
            CANRENAME = 0x10,
            CAPABILITYMASK = 0x177,
            COMPRESSED = 0x4000000,
            CONTENTSMASK = 0x80000000,
            DISPLAYATTRMASK = 0xfc000,
            DROPTARGET = 0x100,
            ENCRYPTED = 0x2000,
            FILESYSANCESTOR = 0x10000000,
            FILESYSTEM = 0x40000000,
            FOLDER = 0x20000000,
            GHOSTED = 0x8000,
            HASPROPSHEET = 0x40,
            HASSTORAGE = 0x400000,
            HASSUBFOLDER = 0x80000000,
            HIDDEN = 0x80000,
            ISSLOW = 0x4000,
            LINK = 0x10000,
            NEWCONTENT = 0x200000,
            NONENUMERATED = 0x100000,
            READONLY = 0x40000,
            REMOVABLE = 0x2000000,
            SHARE = 0x20000,
            STORAGE = 8,
            STORAGEANCESTOR = 0x800000,
            STORAGECAPMASK = 0x70c50008,
            STREAM = 0x400000,
            VALIDATE = 0x1000000
        }

        // Determines the type of items included in an enumeration. 
        // These values are used with the IShellFolder::EnumObjects method
        [Flags]
        private enum SHCONTF
        {
            FOLDERS = 0x0020,
            NONFOLDERS = 0x0040,
            INCLUDEHIDDEN = 0x0080,
            INIT_ON_FIRST_NEXT = 0x0100,
            NETPRINTERSRCH = 0x0200,
            SHAREABLE = 0x0400,
            STORAGE = 0x0800,
        }

        // Specifies how the shortcut menu can be changed when calling IContextMenu::QueryContextMenu
        [Flags]
        private enum CMF : uint
        {
            NORMAL = 0x00000000,
            DEFAULTONLY = 0x00000001,
            VERBSONLY = 0x00000002,
            EXPLORE = 0x00000004,
            NOVERBS = 0x00000008,
            CANRENAME = 0x00000010,
            NODEFAULT = 0x00000020,
            INCLUDESTATIC = 0x00000040,
            EXTENDEDVERBS = 0x00000100,
            RESERVED = 0xffff0000
        }

        // Flags specifying the information to return when calling IContextMenu::GetCommandString
        [Flags]
        private enum GCS : uint
        {
            VERBA = 0,
            HELPTEXTA = 1,
            VALIDATEA = 2,
            VERBW = 4,
            HELPTEXTW = 5,
            VALIDATEW = 6
        }

        // Specifies how TrackPopupMenuEx positions the shortcut menu horizontally
        [Flags]
        private enum TPM : uint
        {
            LEFTBUTTON = 0x0000,
            RIGHTBUTTON = 0x0002,
            LEFTALIGN = 0x0000,
            CENTERALIGN = 0x0004,
            RIGHTALIGN = 0x0008,
            TOPALIGN = 0x0000,
            VCENTERALIGN = 0x0010,
            BOTTOMALIGN = 0x0020,
            HORIZONTAL = 0x0000,
            VERTICAL = 0x0040,
            NONOTIFY = 0x0080,
            RETURNCMD = 0x0100,
            RECURSE = 0x0001,
            HORPOSANIMATION = 0x0400,
            HORNEGANIMATION = 0x0800,
            VERPOSANIMATION = 0x1000,
            VERNEGANIMATION = 0x2000,
            NOANIMATION = 0x4000,
            LAYOUTRTL = 0x8000
        }

        // The cmd for a custom added menu item
        private enum CMD_CUSTOM
        {
            ExpandCollapse = (int)CMD_LAST + 1
        }

        // Flags used with the CMINVOKECOMMANDINFOEX structure
        [Flags]
        private enum CMIC : uint
        {
            HOTKEY = 0x00000020,
            ICON = 0x00000010,
            FLAG_NO_UI = 0x00000400,
            UNICODE = 0x00004000,
            NO_CONSOLE = 0x00008000,
            ASYNCOK = 0x00100000,
            NOZONECHECKS = 0x00800000,
            SHIFT_DOWN = 0x10000000,
            CONTROL_DOWN = 0x40000000,
            FLAG_LOG_USAGE = 0x04000000,
            PTINVOKE = 0x20000000
        }

        // Specifies how the window is to be shown
        [Flags]
        private enum SW
        {
            HIDE = 0,
            SHOWNORMAL = 1,
            NORMAL = 1,
            SHOWMINIMIZED = 2,
            SHOWMAXIMIZED = 3,
            MAXIMIZE = 3,
            SHOWNOACTIVATE = 4,
            SHOW = 5,
            MINIMIZE = 6,
            SHOWMINNOACTIVE = 7,
            SHOWNA = 8,
            RESTORE = 9,
            SHOWDEFAULT = 10,
        }

        // Specifies the content of the new menu item
        [Flags]
        private enum MFT : uint
        {
            GRAYED = 0x00000003,
            DISABLED = 0x00000003,
            CHECKED = 0x00000008,
            SEPARATOR = 0x00000800,
            RADIOCHECK = 0x00000200,
            BITMAP = 0x00000004,
            OWNERDRAW = 0x00000100,
            MENUBARBREAK = 0x00000020,
            MENUBREAK = 0x00000040,
            RIGHTORDER = 0x00002000,
            BYCOMMAND = 0x00000000,
            BYPOSITION = 0x00000400,
            POPUP = 0x00000010
        }

        // Specifies the state of the new menu item
        [Flags]
        private enum MFS : uint
        {
            GRAYED = 0x00000003,
            DISABLED = 0x00000003,
            CHECKED = 0x00000008,
            HILITE = 0x00000080,
            ENABLED = 0x00000000,
            UNCHECKED = 0x00000000,
            UNHILITE = 0x00000000,
            DEFAULT = 0x00001000
        }

        // Specifies the content of the new menu item
        [Flags]
        private enum MIIM : uint
        {
            BITMAP = 0x80,
            CHECKMARKS = 0x08,
            DATA = 0x20,
            FTYPE = 0x100,
            ID = 0x02,
            STATE = 0x01,
            STRING = 0x40,
            SUBMENU = 0x04,
            TYPE = 0x10
        }

        // Indicates the type of storage medium being used in a data transfer
        [Flags]
        private enum TYMED
        {
            ENHMF = 0x40,
            FILE = 2,
            GDI = 0x10,
            HGLOBAL = 1,
            ISTORAGE = 8,
            ISTREAM = 4,
            MFPICT = 0x20,
            NULL = 0
        }

        #endregion

        #region IShellFolder
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214E6-0000-0000-C000-000000000046")]
        private interface IShellFolder
        {
            // Translates a file object's or folder's display name into an item identifier list.
            // Return value: error code, if any
            [PreserveSig]
            Int32 ParseDisplayName(
                IntPtr hwnd,
                IntPtr pbc,
                [MarshalAs(UnmanagedType.LPWStr)]
                string pszDisplayName,
                ref uint pchEaten,
                out IntPtr ppidl,
                ref SFGAO pdwAttributes);

            // Allows a client to determine the contents of a folder by creating an item
            // identifier enumeration object and returning its IEnumIDList interface.
            // Return value: error code, if any
            [PreserveSig]
            Int32 EnumObjects(
                IntPtr hwnd,
                SHCONTF grfFlags,
                out IntPtr enumIDList);

            // Retrieves an IShellFolder object for a subfolder.
            // Return value: error code, if any
            [PreserveSig]
            Int32 BindToObject(
                IntPtr pidl,
                IntPtr pbc,
                ref Guid riid,
                out IntPtr ppv);

            // Requests a pointer to an object's storage interface. 
            // Return value: error code, if any
            [PreserveSig]
            Int32 BindToStorage(
                IntPtr pidl,
                IntPtr pbc,
                ref Guid riid,
                out IntPtr ppv);

            // Determines the relative order of two file objects or folders, given their
            // item identifier lists. Return value: If this method is successful, the
            // CODE field of the HRESULT contains one of the following values (the code
            // can be retrived using the helper function GetHResultCode): Negative A
            // negative return value indicates that the first item should precede
            // the second (pidl1 < pidl2). 

            // Positive A positive return value indicates that the first item should
            // follow the second (pidl1 > pidl2).  Zero A return value of zero
            // indicates that the two items are the same (pidl1 = pidl2). 
            [PreserveSig]
            Int32 CompareIDs(
                IntPtr lParam,
                IntPtr pidl1,
                IntPtr pidl2);

            // Requests an object that can be used to obtain information from or interact
            // with a folder object.
            // Return value: error code, if any
            [PreserveSig]
            Int32 CreateViewObject(
                IntPtr hwndOwner,
                Guid riid,
                out IntPtr ppv);

            // Retrieves the attributes of one or more file objects or subfolders. 
            // Return value: error code, if any
            [PreserveSig]
            Int32 GetAttributesOf(
                uint cidl,
                [MarshalAs(UnmanagedType.LPArray)]
                IntPtr[] apidl,
                ref SFGAO rgfInOut);

            // Retrieves an OLE interface that can be used to carry out actions on the
            // specified file objects or folders.
            // Return value: error code, if any
            [PreserveSig]
            Int32 GetUIObjectOf(
                IntPtr hwndOwner,
                uint cidl,
                [MarshalAs(UnmanagedType.LPArray)]
                IntPtr[] apidl,
                ref Guid riid,
                IntPtr rgfReserved,
                out IntPtr ppv);

            // Retrieves the display name for the specified file object or subfolder. 
            // Return value: error code, if any
            [PreserveSig()]
            Int32 GetDisplayNameOf(
                IntPtr pidl,
                SHGNO uFlags,
                IntPtr lpName);

            // Sets the display name of a file object or subfolder, changing the item
            // identifier in the process.
            // Return value: error code, if any
            [PreserveSig]
            Int32 SetNameOf(
                IntPtr hwnd,
                IntPtr pidl,
                [MarshalAs(UnmanagedType.LPWStr)]
                string pszName,
                SHGNO uFlags,
                out IntPtr ppidlOut);
        }
        #endregion

        #region IContextMenu
        [ComImport()]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [GuidAttribute("000214e4-0000-0000-c000-000000000046")]
        private interface IContextMenu
        {
            // Adds commands to a shortcut menu
            [PreserveSig()]
            Int32 QueryContextMenu(
                IntPtr hmenu,
                uint iMenu,
                uint idCmdFirst,
                uint idCmdLast,
                CMF uFlags);

            // Carries out the command associated with a shortcut menu item
            [PreserveSig()]
            Int32 InvokeCommand(
                ref CMINVOKECOMMANDINFOEX info);

            // Retrieves information about a shortcut menu command, 
            // including the help string and the language-independent, 
            // or canonical, name for the command
            [PreserveSig()]
            Int32 GetCommandString(
                uint idcmd,
                GCS uflags,
                uint reserved,
                [MarshalAs(UnmanagedType.LPArray)]
                byte[] commandstring,
                int cch);
        }

        [ComImport, Guid("000214f4-0000-0000-c000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IContextMenu2
        {
            // Adds commands to a shortcut menu
            [PreserveSig()]
            Int32 QueryContextMenu(
                IntPtr hmenu,
                uint iMenu,
                uint idCmdFirst,
                uint idCmdLast,
                CMF uFlags);

            // Carries out the command associated with a shortcut menu item
            [PreserveSig()]
            Int32 InvokeCommand(
                ref CMINVOKECOMMANDINFOEX info);

            // Retrieves information about a shortcut menu command, 
            // including the help string and the language-independent, 
            // or canonical, name for the command
            [PreserveSig()]
            Int32 GetCommandString(
                uint idcmd,
                GCS uflags,
                uint reserved,
                [MarshalAs(UnmanagedType.LPWStr)]
                StringBuilder commandstring,
                int cch);

            // Allows client objects of the IContextMenu interface to 
            // handle messages associated with owner-drawn menu items
            [PreserveSig]
            Int32 HandleMenuMsg(
                uint uMsg,
                IntPtr wParam,
                IntPtr lParam);
        }

        [ComImport, Guid("bcfce0a0-ec17-11d0-8d10-00a0c90f2719")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IContextMenu3
        {
            // Adds commands to a shortcut menu
            [PreserveSig()]
            Int32 QueryContextMenu(
                IntPtr hmenu,
                uint iMenu,
                uint idCmdFirst,
                uint idCmdLast,
                CMF uFlags);

            // Carries out the command associated with a shortcut menu item
            [PreserveSig()]
            Int32 InvokeCommand(
                ref CMINVOKECOMMANDINFOEX info);

            // Retrieves information about a shortcut menu command, 
            // including the help string and the language-independent, 
            // or canonical, name for the command
            [PreserveSig()]
            Int32 GetCommandString(
                uint idcmd,
                GCS uflags,
                uint reserved,
                [MarshalAs(UnmanagedType.LPWStr)]
                StringBuilder commandstring,
                int cch);

            // Allows client objects of the IContextMenu interface to 
            // handle messages associated with owner-drawn menu items
            [PreserveSig]
            Int32 HandleMenuMsg(
                uint uMsg,
                IntPtr wParam,
                IntPtr lParam);

            // Allows client objects of the IContextMenu3 interface to 
            // handle messages associated with owner-drawn menu items
            [PreserveSig]
            Int32 HandleMenuMsg2(
                uint uMsg,
                IntPtr wParam,
                IntPtr lParam,
                IntPtr plResult);
        }
        #endregion
    }

    #region ShellContextMenuException
    public class ShellContextMenuException : Exception
    {
        /// <summary>Default contructor</summary>
        public ShellContextMenuException()
        {
        }

        /// <summary>Constructor with message</summary>
        /// <param name="message">Message</param>
        public ShellContextMenuException(string message)
            : base(message)
        {
        }
    }
    #endregion
}
