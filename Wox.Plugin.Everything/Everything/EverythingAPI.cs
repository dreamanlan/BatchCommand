using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Wox.Plugin.Everything.Everything.Exceptions;

namespace Wox.Plugin.Everything.Everything
{
    public sealed class EverythingAPI
    {
        #region DllImport
        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern int Everything_SetSearchW(string lpSearchString);

        [DllImport(Main.DLL)]
        public static extern void Everything_SetMatchPath(bool bEnable);

        [DllImport(Main.DLL)]
        public static extern void Everything_SetMatchCase(bool bEnable);

        [DllImport(Main.DLL)]
        public static extern void Everything_SetMatchWholeWord(bool bEnable);

        [DllImport(Main.DLL)]
        public static extern void Everything_SetRegex(bool bEnable);

        [DllImport(Main.DLL)]
        public static extern void Everything_SetMax(int dwMax);

        [DllImport(Main.DLL)]
        public static extern void Everything_SetOffset(int dwOffset);
        [DllImport(Main.DLL)]
        public static extern void Everything_SetReplyWindow(IntPtr hWnd);
        [DllImport(Main.DLL)]
        public static extern void Everything_SetReplyID(int nId);

        [DllImport(Main.DLL)]
        public static extern bool Everything_GetMatchPath();

        [DllImport(Main.DLL)]
        public static extern bool Everything_GetMatchCase();

        [DllImport(Main.DLL)]
        public static extern bool Everything_GetMatchWholeWord();

        [DllImport(Main.DLL)]
        public static extern bool Everything_GetRegex();

        [DllImport(Main.DLL)]
        public static extern uint Everything_GetMax();

        [DllImport(Main.DLL)]
        public static extern uint Everything_GetOffset();

        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetSearchW();

        [DllImport(Main.DLL)]
        public static extern EverythingAPI.StateCode Everything_GetLastError();
        [DllImport(Main.DLL)]
        public static extern IntPtr Everything_GetReplyWindow();
        [DllImport(Main.DLL)]
        public static extern int Everything_GetReplyID();

        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern bool Everything_QueryW(bool bWait);

        [DllImport(Main.DLL)]
        public static extern bool Everything_IsQueryReply(int message, IntPtr wParam, IntPtr lParam, uint nId);


        [DllImport(Main.DLL)]
        public static extern void Everything_SortResultsByPath();

        [DllImport(Main.DLL)]
        public static extern int Everything_GetNumFileResults();

        [DllImport(Main.DLL)]
        public static extern int Everything_GetNumFolderResults();

        [DllImport(Main.DLL)]
        public static extern int Everything_GetNumResults();

        [DllImport(Main.DLL)]
        public static extern int Everything_GetTotFileResults();

        [DllImport(Main.DLL)]
        public static extern int Everything_GetTotFolderResults();

        [DllImport(Main.DLL)]
        public static extern int Everything_GetTotResults();

        [DllImport(Main.DLL)]
        public static extern bool Everything_IsVolumeResult(int nIndex);

        [DllImport(Main.DLL)]
        public static extern bool Everything_IsFolderResult(int nIndex);

        [DllImport(Main.DLL)]
        public static extern bool Everything_IsFileResult(int nIndex);

        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern void Everything_GetResultFullPathNameW(int nIndex, StringBuilder lpString, int nMaxCount);
        [DllImport(Main.DLL)]
        public static extern void Everything_Reset();
        [DllImport(Main.DLL)]
        public static extern void Everything_CleanUp();
        [DllImport(Main.DLL)]
        public static extern int Everything_GetMajorVersion();
        [DllImport(Main.DLL)]
        public static extern int Everything_GetMinorVersion();
        [DllImport(Main.DLL)]
        public static extern int Everything_GetRevision();
        [DllImport(Main.DLL)]
        public static extern uint Everything_GetBuildNumber();
        [DllImport(Main.DLL)]
        public static extern bool Everything_Exit();
        [DllImport(Main.DLL)]
        public static extern bool Everything_IsDBLoaded();
        [DllImport(Main.DLL)]
        public static extern bool Everything_IsAdmin();
        [DllImport(Main.DLL)]
        public static extern bool Everything_IsAppData();
        [DllImport(Main.DLL)]
        public static extern bool Everything_RebuildDB();
        [DllImport(Main.DLL)]
        public static extern bool Everything_UpdateAllFolderIndexes();
        [DllImport(Main.DLL)]
        public static extern bool Everything_SaveDB();
        [DllImport(Main.DLL)]
        public static extern bool Everything_SaveRunHistory();
        [DllImport(Main.DLL)]
        public static extern bool Everything_DeleteRunHistory();
        [DllImport(Main.DLL)]
        public static extern uint Everything_GetTargetMachine();
        // https://www.voidtools.com/forum/viewtopic.php?t=8169
        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultFileNameW(int nIndex);

        // Everything 1.4
        [DllImport(Main.DLL)]
        public static extern void Everything_SetSort(uint dwSortType);
        [DllImport(Main.DLL)]
        public static extern uint Everything_GetSort();
        [DllImport(Main.DLL)]
        public static extern uint Everything_GetResultListSort();
        [DllImport(Main.DLL)]
        public static extern void Everything_SetRequestFlags(uint flag);
        [DllImport(Main.DLL)]
        public static extern uint Everything_GetRequestFlags();
        [DllImport(Main.DLL)]
        public static extern uint Everything_GetResultListRequestFlags();
        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultExtensionW(uint nIndex);
        [DllImport(Main.DLL)]
        public static extern bool Everything_GetResultSize(uint nIndex, out long lpFileSize);
        [DllImport(Main.DLL)]
        public static extern bool Everything_GetResultDateCreated(uint nIndex, out long lpFileTime);
        [DllImport(Main.DLL)]
        public static extern bool Everything_GetResultDateModified(uint nIndex, out long lpFileTime);
        [DllImport(Main.DLL)]
        public static extern bool Everything_GetResultDateAccessed(uint nIndex, out long lpFileTime);
        [DllImport(Main.DLL)]
        public static extern uint Everything_GetResultAttributes(uint nIndex);
        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultFileListFileNameW(uint nIndex);
        [DllImport(Main.DLL)]
        public static extern uint Everything_GetResultRunCount(uint nIndex);
        [DllImport(Main.DLL)]
        public static extern bool Everything_GetResultDateRun(uint nIndex, out long lpFileTime);
        [DllImport(Main.DLL)]
        public static extern bool Everything_GetResultDateRecentlyChanged(uint nIndex, out long lpFileTime);
        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedFileNameW(int nIndex);
        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedPathW(int nIndex);
        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedFullPathAndFileNameW(int nIndex);
        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern uint Everything_GetRunCountFromFileNameW(string lpFileName);
        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern bool Everything_SetRunCountFromFileNameW(string lpFileName, uint dwRunCount);
        [DllImport(Main.DLL, CharSet = CharSet.Unicode)]
        public static extern uint Everything_IncRunCountFromFileNameW(string lpFileName);

        public static bool EverythingExists()
        {
            var hwnd = FindWindowW(EVERYTHING_IPC_WNDCLASS, null);
            return hwnd != IntPtr.Zero;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowW(string lpClassName, string lpWindowName);
        private const string EVERYTHING_IPC_WNDCLASS = "EVERYTHING_TASKBAR_NOTIFICATION";
        #endregion

        public enum StateCode
        {
            OK,
            MemoryError,
            IPCError,
            RegisterClassExError,
            CreateWindowError,
            CreateThreadError,
            InvalidIndexError,
            InvalidCallError
        }

        /// <summary>
        /// Gets or sets a value indicating whether [match path].
        /// </summary>
        /// <value><c>true</c> if [match path]; otherwise, <c>false</c>.</value>
        public Boolean MatchPath
        {
            get
            {
                return Everything_GetMatchPath();
            }
            set
            {
                Everything_SetMatchPath(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [match case].
        /// </summary>
        /// <value><c>true</c> if [match case]; otherwise, <c>false</c>.</value>
        public Boolean MatchCase
        {
            get
            {
                return Everything_GetMatchCase();
            }
            set
            {
                Everything_SetMatchCase(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [match whole word].
        /// </summary>
        /// <value><c>true</c> if [match whole word]; otherwise, <c>false</c>.</value>
        public Boolean MatchWholeWord
        {
            get
            {
                return Everything_GetMatchWholeWord();
            }
            set
            {
                Everything_SetMatchWholeWord(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable regex].
        /// </summary>
        /// <value><c>true</c> if [enable regex]; otherwise, <c>false</c>.</value>
        public Boolean EnableRegex
        {
            get
            {
                return Everything_GetRegex();
            }
            set
            {
                Everything_SetRegex(value);
            }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            Everything_Reset();
        }

        /// <summary>
        /// Searches the specified key word.
        /// </summary>
        /// <param name="keyWord">The key word.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="maxCount">The max count.</param>
        /// <returns></returns>
        public IEnumerable<SearchResult> Search(string keyWord, int offset = 0, int maxCount = 100)
        {
            if (string.IsNullOrEmpty(keyWord))
                throw new ArgumentNullException("keyWord");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");

            if (maxCount < 0)
                throw new ArgumentOutOfRangeException("maxCount");

            if (keyWord.StartsWith("@"))
            {
                Everything_SetRegex(true);
                keyWord = keyWord.Substring(1);
            }
            Everything_SetSearchW(keyWord);
            Everything_SetOffset(offset);
            Everything_SetMax(maxCount);


            if (!Everything_QueryW(true))
            {
                switch (Everything_GetLastError())
                {
                    case StateCode.CreateThreadError:
                        throw new CreateThreadException();
                    case StateCode.CreateWindowError:
                        throw new CreateWindowException();
                    case StateCode.InvalidCallError:
                        throw new InvalidCallException();
                    case StateCode.InvalidIndexError:
                        throw new InvalidIndexException();
                    case StateCode.IPCError:
                        throw new IPCErrorException();
                    case StateCode.MemoryError:
                        throw new MemoryErrorException();
                    case StateCode.RegisterClassExError:
                        throw new RegisterClassExException();
                }
                yield break;
            }

            const int bufferSize = 4096;
            StringBuilder buffer = new StringBuilder(bufferSize);
            for (int idx = 0; idx < Everything_GetNumResults(); ++idx)
            {
                Everything_GetResultFullPathNameW(idx, buffer, bufferSize);

                var result = new SearchResult { FullPath = buffer.ToString() };
                if (Everything_IsFolderResult(idx))
                    result.Type = ResultType.Folder;
                else if (Everything_IsFileResult(idx))
                    result.Type = ResultType.File;

                yield return result;
            }
        }
    }
}
