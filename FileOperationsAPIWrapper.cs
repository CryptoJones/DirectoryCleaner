using System;
using System.Runtime.InteropServices;

namespace DirectoryCleaner
{
    public class FileOperationApiWrapper
    {
        /// <summary>
        /// Possible flags for the SHFileOperation method.
        /// </summary>
        [Flags]
        public enum FileOperationFlags : ushort
        {
            /// <summary>
            /// Do not show a dialog during the process
            /// </summary>
            FofSilent = 0x0004,
            /// <summary>
            /// Do not ask the user to confirm selection
            /// </summary>
            FofNoconfirmation = 0x0010,
            /// <summary>
            /// Delete the file to the recycle bin.  (Required flag to send a file to the bin
            /// </summary>
            FofAllowundo = 0x0040,
            /// <summary>
            /// Do not show the names of the files or folders that are being recycled.
            /// </summary>
            FofSimpleprogress = 0x0100,
            /// <summary>
            /// Surpress errors, if any occur during the process.
            /// </summary>
            FofNoerrorui = 0x0400,
            /// <summary>
            /// Warn if files are too big to fit in the recycle bin and will need
            /// to be deleted completely.
            /// </summary>
            FofWantnukewarning = 0x4000,
        }

        /// <summary>
        /// File Operation Function Type for SHFileOperation
        /// </summary>
        public enum FileOperationType : uint
        {
            /// <summary>
            /// Move the objects
            /// </summary>
            FoMove = 0x0001,
            /// <summary>
            /// Copy the objects
            /// </summary>
            FoCopy = 0x0002,
            /// <summary>
            /// Delete (or recycle) the objects
            /// </summary>
            FoDelete = 0x0003,
            /// <summary>
            /// Rename the object(s)
            /// </summary>
            FoRename = 0x0004,
        }



        /// <summary>
        /// SHFILEOPSTRUCT for SHFileOperation from COM
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct Shfileopstruct
        {

            private readonly IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public FileOperationType wFunc;
            public string pFrom;
            private readonly string pTo;
            public FileOperationFlags fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            private readonly bool fAnyOperationsAborted;
            private readonly IntPtr hNameMappings;
            private readonly string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHFileOperation(ref Shfileopstruct fileOp);

        /// <summary>
        /// Send file to recycle bin
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        /// <param name="flags">FileOperationFlags to add in addition to FOF_ALLOWUNDO</param>
        public static bool Send(string path, FileOperationFlags flags)
        {
            try
            {
                var fs = new Shfileopstruct
                {
                    wFunc = FileOperationType.FoDelete,
                    pFrom = path + '\0' + '\0',
                    fFlags = FileOperationFlags.FofAllowundo | flags
                };
                SHFileOperation(ref fs);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Send file to recycle bin.  Display dialog, display warning if files are too big to fit (FOF_WANTNUKEWARNING)
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        public static bool Send(string path)
        {
            return Send(path, FileOperationFlags.FofNoconfirmation | FileOperationFlags.FofWantnukewarning);
        }

        /// <summary>
        /// Send file silently to recycle bin.  Surpress dialog, surpress errors, delete if too large.
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        public static bool MoveToRecycleBin(string path)
        {
            return Send(path, FileOperationFlags.FofNoconfirmation | FileOperationFlags.FofNoerrorui | FileOperationFlags.FofSilent);

        }

        private static bool DeleteFile(string path, FileOperationFlags flags)
        {
            try
            {
                var fs = new Shfileopstruct
                {
                    wFunc = FileOperationType.FoDelete,
                    pFrom = path + '\0' + '\0',
                    fFlags = flags
                };
                SHFileOperation(ref fs);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DeleteCompletelySilent(string path)
        {
            return DeleteFile(path,
                FileOperationFlags.FofNoconfirmation | FileOperationFlags.FofNoerrorui |
                FileOperationFlags.FofSilent);
        }
    }
}