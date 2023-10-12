using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

public static class EveryThingSDK
{
    public const string DLL = "Everything.dll";

    public const int EVERYTHING_OK = 0;
    public const int EVERYTHING_ERROR_MEMORY = 1;
    public const int EVERYTHING_ERROR_IPC = 2;
    public const int EVERYTHING_ERROR_REGISTERCLASSEX = 3;
    public const int EVERYTHING_ERROR_CREATEWINDOW = 4;
    public const int EVERYTHING_ERROR_CREATETHREAD = 5;
    public const int EVERYTHING_ERROR_INVALIDINDEX = 6;
    public const int EVERYTHING_ERROR_INVALIDCALL = 7;

    public const int EVERYTHING_REQUEST_FILE_NAME = 0x00000001;
    public const int EVERYTHING_REQUEST_PATH = 0x00000002;
    public const int EVERYTHING_REQUEST_FULL_PATH_AND_FILE_NAME = 0x00000004;
    public const int EVERYTHING_REQUEST_EXTENSION = 0x00000008;
    public const int EVERYTHING_REQUEST_SIZE = 0x00000010;
    public const int EVERYTHING_REQUEST_DATE_CREATED = 0x00000020;
    public const int EVERYTHING_REQUEST_DATE_MODIFIED = 0x00000040;
    public const int EVERYTHING_REQUEST_DATE_ACCESSED = 0x00000080;
    public const int EVERYTHING_REQUEST_ATTRIBUTES = 0x00000100;
    public const int EVERYTHING_REQUEST_FILE_LIST_FILE_NAME = 0x00000200;
    public const int EVERYTHING_REQUEST_RUN_COUNT = 0x00000400;
    public const int EVERYTHING_REQUEST_DATE_RUN = 0x00000800;
    public const int EVERYTHING_REQUEST_DATE_RECENTLY_CHANGED = 0x00001000;
    public const int EVERYTHING_REQUEST_HIGHLIGHTED_FILE_NAME = 0x00002000;
    public const int EVERYTHING_REQUEST_HIGHLIGHTED_PATH = 0x00004000;
    public const int EVERYTHING_REQUEST_HIGHLIGHTED_FULL_PATH_AND_FILE_NAME = 0x00008000;

    public const int EVERYTHING_SORT_NAME_ASCENDING = 1;
    public const int EVERYTHING_SORT_NAME_DESCENDING = 2;
    public const int EVERYTHING_SORT_PATH_ASCENDING = 3;
    public const int EVERYTHING_SORT_PATH_DESCENDING = 4;
    public const int EVERYTHING_SORT_SIZE_ASCENDING = 5;
    public const int EVERYTHING_SORT_SIZE_DESCENDING = 6;
    public const int EVERYTHING_SORT_EXTENSION_ASCENDING = 7;
    public const int EVERYTHING_SORT_EXTENSION_DESCENDING = 8;
    public const int EVERYTHING_SORT_TYPE_NAME_ASCENDING = 9;
    public const int EVERYTHING_SORT_TYPE_NAME_DESCENDING = 10;
    public const int EVERYTHING_SORT_DATE_CREATED_ASCENDING = 11;
    public const int EVERYTHING_SORT_DATE_CREATED_DESCENDING = 12;
    public const int EVERYTHING_SORT_DATE_MODIFIED_ASCENDING = 13;
    public const int EVERYTHING_SORT_DATE_MODIFIED_DESCENDING = 14;
    public const int EVERYTHING_SORT_ATTRIBUTES_ASCENDING = 15;
    public const int EVERYTHING_SORT_ATTRIBUTES_DESCENDING = 16;
    public const int EVERYTHING_SORT_FILE_LIST_FILENAME_ASCENDING = 17;
    public const int EVERYTHING_SORT_FILE_LIST_FILENAME_DESCENDING = 18;
    public const int EVERYTHING_SORT_RUN_COUNT_ASCENDING = 19;
    public const int EVERYTHING_SORT_RUN_COUNT_DESCENDING = 20;
    public const int EVERYTHING_SORT_DATE_RECENTLY_CHANGED_ASCENDING = 21;
    public const int EVERYTHING_SORT_DATE_RECENTLY_CHANGED_DESCENDING = 22;
    public const int EVERYTHING_SORT_DATE_ACCESSED_ASCENDING = 23;
    public const int EVERYTHING_SORT_DATE_ACCESSED_DESCENDING = 24;
    public const int EVERYTHING_SORT_DATE_RUN_ASCENDING = 25;
    public const int EVERYTHING_SORT_DATE_RUN_DESCENDING = 26;

    public const int EVERYTHING_TARGET_MACHINE_X86 = 1;
    public const int EVERYTHING_TARGET_MACHINE_X64 = 2;
    public const int EVERYTHING_TARGET_MACHINE_ARM = 3;

    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern uint Everything_SetSearchW(string lpSearchString);
    [DllImport(DLL)]
    public static extern void Everything_SetMatchPath(bool bEnable);
    [DllImport(DLL)]
    public static extern void Everything_SetMatchCase(bool bEnable);
    [DllImport(DLL)]
    public static extern void Everything_SetMatchWholeWord(bool bEnable);
    [DllImport(DLL)]
    public static extern void Everything_SetRegex(bool bEnable);
    [DllImport(DLL)]
    public static extern void Everything_SetMax(uint dwMax);
    [DllImport(DLL)]
    public static extern void Everything_SetOffset(uint dwOffset);
    [DllImport(DLL)]
    public static extern void Everything_SetReplyWindow(IntPtr hWnd);
    [DllImport(DLL)]
    public static extern void Everything_SetReplyID(int nId);


    [DllImport(DLL)]
    public static extern bool Everything_GetMatchPath();
    [DllImport(DLL)]
    public static extern bool Everything_GetMatchCase();
    [DllImport(DLL)]
    public static extern bool Everything_GetMatchWholeWord();
    [DllImport(DLL)]
    public static extern bool Everything_GetRegex();
    [DllImport(DLL)]
    public static extern uint Everything_GetMax();
    [DllImport(DLL)]
    public static extern uint Everything_GetOffset();
    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetSearchW();
    [DllImport(DLL)]
    public static extern uint Everything_GetLastError();
    [DllImport(DLL)]
    public static extern IntPtr Everything_GetReplyWindow();
    [DllImport(DLL)]
    public static extern int Everything_GetReplyID();

    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern bool Everything_QueryW(bool bWait);

    [DllImport(DLL)]
    public static extern bool Everything_IsQueryReply(int message, IntPtr wParam, IntPtr lParam, uint nId);

    [DllImport(DLL)]
    public static extern void Everything_SortResultsByPath();

    [DllImport(DLL)]
    public static extern uint Everything_GetNumFileResults();
    [DllImport(DLL)]
    public static extern uint Everything_GetNumFolderResults();
    [DllImport(DLL)]
    public static extern uint Everything_GetNumResults();
    [DllImport(DLL)]
    public static extern uint Everything_GetTotFileResults();
    [DllImport(DLL)]
    public static extern uint Everything_GetTotFolderResults();
    [DllImport(DLL)]
    public static extern uint Everything_GetTotResults();
    [DllImport(DLL)]
    public static extern bool Everything_IsVolumeResult(uint nIndex);
    [DllImport(DLL)]
    public static extern bool Everything_IsFolderResult(uint nIndex);
    [DllImport(DLL)]
    public static extern bool Everything_IsFileResult(uint nIndex);
    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern void Everything_GetResultFullPathNameW(uint nIndex, StringBuilder lpString, uint nMaxCount);
    [DllImport(DLL)]
    public static extern void Everything_Reset();
    [DllImport(DLL)]
    public static extern void Everything_CleanUp();
    [DllImport(DLL)]
    public static extern uint Everything_GetMajorVersion();
    [DllImport(DLL)]
    public static extern uint Everything_GetMinorVersion();
    [DllImport(DLL)]
    public static extern uint Everything_GetRevision();
    [DllImport(DLL)]
    public static extern uint Everything_GetBuildNumber();
    [DllImport(DLL)]
    public static extern bool Everything_Exit();
    [DllImport(DLL)]
    public static extern bool Everything_IsDBLoaded();
    [DllImport(DLL)]
    public static extern bool Everything_IsAdmin();
    [DllImport(DLL)]
    public static extern bool Everything_IsAppData();
    [DllImport(DLL)]
    public static extern bool Everything_RebuildDB();
    [DllImport(DLL)]
    public static extern bool Everything_UpdateAllFolderIndexes();
    [DllImport(DLL)]
    public static extern bool Everything_SaveDB();
    [DllImport(DLL)]
    public static extern bool Everything_SaveRunHistory();
    [DllImport(DLL)]
    public static extern bool Everything_DeleteRunHistory();
    [DllImport(DLL)]
    public static extern uint Everything_GetTargetMachine();

    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetResultFileNameW(uint nIndex);

    // Everything 1.4
    [DllImport(DLL)]
    public static extern void Everything_SetSort(uint dwSortType);
    [DllImport(DLL)]
    public static extern uint Everything_GetSort();
    [DllImport(DLL)]
    public static extern uint Everything_GetResultListSort();
    [DllImport(DLL)]
    public static extern void Everything_SetRequestFlags(uint dwRequestFlags);
    [DllImport(DLL)]
    public static extern uint Everything_GetRequestFlags();
    [DllImport(DLL)]
    public static extern uint Everything_GetResultListRequestFlags();
    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetResultExtensionW(uint nIndex);
    [DllImport(DLL)]
    public static extern bool Everything_GetResultSize(uint nIndex, out long lpFileSize);
    [DllImport(DLL)]
    public static extern bool Everything_GetResultDateCreated(uint nIndex, out long lpFileTime);
    [DllImport(DLL)]
    public static extern bool Everything_GetResultDateModified(uint nIndex, out long lpFileTime);
    [DllImport(DLL)]
    public static extern bool Everything_GetResultDateAccessed(uint nIndex, out long lpFileTime);
    [DllImport(DLL)]
    public static extern uint Everything_GetResultAttributes(uint nIndex);
    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetResultFileListFileNameW(uint nIndex);
    [DllImport(DLL)]
    public static extern uint Everything_GetResultRunCount(uint nIndex);
    [DllImport(DLL)]
    public static extern bool Everything_GetResultDateRun(uint nIndex, out long lpFileTime);
    [DllImport(DLL)]
    public static extern bool Everything_GetResultDateRecentlyChanged(uint nIndex, out long lpFileTime);
    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetResultHighlightedFileNameW(uint nIndex);
    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetResultHighlightedPathW(uint nIndex);
    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetResultHighlightedFullPathAndFileNameW(uint nIndex);
    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern uint Everything_GetRunCountFromFileNameW(string lpFileName);
    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern bool Everything_SetRunCountFromFileNameW(string lpFileName, uint dwRunCount);
    [DllImport(DLL, CharSet = CharSet.Unicode)]
    public static extern uint Everything_IncRunCountFromFileNameW(string lpFileName);

    public static bool EverythingExists()
    {
        var hwnd = FindWindowW(EVERYTHING_IPC_WNDCLASS, null);
        return hwnd != IntPtr.Zero;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowW(string lpClassName, string lpWindowName);
    private const string EVERYTHING_IPC_WNDCLASS = "EVERYTHING_TASKBAR_NOTIFICATION";
}
