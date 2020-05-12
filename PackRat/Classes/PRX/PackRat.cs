using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Threading.Tasks;
using static PackRat.Utils;

namespace PackRat {

    // some VS annoynaces
#pragma warning disable 0169, 0649, 0067, 0168, 0219, 0162, CS0219

    #region "enums"
    [Flags]
    public enum LogType {
        Init = 0x1,
        Parse = 0x2,
        Info = 0x4,
        Warning = 0x8,
        Error = 0x10,
        Critical = 0x20,
        IO = 0x40,
        FTable = 0x80,
        GUI = 0x100,
        Exception = 0x200
        }
    [Flags]
    public enum NodeFlag : byte { // Enc/Comp probably archive-wide rather than per-file, but still.. future proofing
        Directory = 1 << 0,
        File = 1 << 1,
        Encrypted = 1 << 2,
        Compressed = 1 << 3,
        EncryptedName = 1 << 4,
        Virtual = 1 << 5,
        Spanned = 1 << 7, // flag to indicate the file is continued at the immediate beginning of the next file
        }
    [Flags]
    public enum PRXFlag : byte {
        Encrypted = 1 << 0,
        Compressed = 1 << 1,
        Password = 1 << 2,
        EncryptedNodeNames = 1 << 3,
        Multipart = 1 << 7, // will likely never be implemented.
        }
    [Flags]
    public enum Attrib {
        None = 0x00000000,
        ReadOnly = 1 << 0,
        Archive = 1 << 1,
        System = 1 << 2,
        Hidden = 1 << 3,
        Root = 1 << 7,
        }
    [Flags]
    public enum ErrCode {
        NULL = 0,
        SUCCESS = 1 << 0,
        APP_FAIL_TO_READ = 1 << 1,
        APP_FAIL_TO_WRITE = 1 << 2,
        APP_FAIL_TO_ADD = 1 << 3,
        APP_FAIL_TO_PARSE = 1 << 4,
        APP_FAIL_TO_TRUNC = 1 << 5,
        APP_FAIL_HASH = 1 << 6,
        APP_FAIL_PASSWORD = 1 << 7,
        APP_FAIL_CREATE_SWAP_FILE = 1 << 8,
        EX_FILE_NOT_FOUND = 1 << 9,
        EX_EARLY_EOF = 1 << 10,
        APP_SIZE_TOO_LARGE = 1 << 11,
        APP_NAME_TOO_LONG = 1 << 12,
        ERR_GENERIC_OR_UNKNOWN = 1 << 13,
        EX_OUT_OF_MEMORY = 1 << 14,
        APP_NAME_CONFLICT = 1 << 15,
        EX_ACCESS_DENIED = 1 << 16,
        EX_GENERIC_IO = 1 << 17,
        APP_UNHANDLED_EVENT = 1 << 18,
        APP_DUPLICATE_NODE = 1 << 19,
        APP_NONEXISTANT_NODE = 1 << 20,
        APP_THREAD_COMPLETED = 1 << 21,
        APP_THREAD_FAILED = 1 << 22,
        APP_THREAD_HANG = 1 << 23,
        NODE_INVALID_OPERATION = 1 << 24,
        }
    #endregion "enums"

    // for whatever reason, we can't globally disable unreachable code and var assigned but never used bullshit
    //#pragma warning disable 0162, CS0219
    public class Archive : PRBase {
        #region "private members"
        private PRXFlag PRXFlags;
        private FileInfo PRXFile;
        private FileStream PRXSwapFile;
        private Dictionary<PRNode, ErrCode> ProgressTree;
        private byte PRXVersion = 1;
        private short Loc_DataOffset = 0;
        private short Loc_NodeCount = 0;
        private bool PRXBusy = false;
        #endregion "private members"

        #region "public members"
        public PRTable MasterFileTable;
        public PRNode RootPath {
            get {
                return RootNode;
                }
            }

        /// <summary>
        /// Returns a bool indicating whether or not some IO is ongoing.
        /// ie; If TRUE, should WAIT before trying to perform any further actions on the archive
        /// </summary>
        public bool Busy {
            get {
                return PRXBusy;
                }
            }
        #endregion "public members"

        #region "public configs"
        /// <summary>
        /// Disable throwing rename-resolution events and default to programmatically renaming nodes
        /// </summary>
        public bool AutoRenameConflicts = false;

        /// <summary>
        /// Set the archive password (En- and de-crypting)
        /// </summary>
        public string ArchivePassword;
        private Enum clean_temp_files;
        #endregion "public configs"

        #region "events"
        /// <summary>
        /// Error event raised when ANY error is encountered
        /// </summary>
        public event EvError OnError;
        public delegate void EvError(ErrCode ec);
        protected virtual void Error(ErrCode ec) {
            if (OnError != null) {
                OnError(ec);
                }
            }

        /// <summary>
        /// Event raised when a password is needed upon opening an archive or extracting a file (Should return a password to this event)
        /// </summary>
        public event EvPasswordRequest OnRequestPWD;
        public delegate string EvPasswordRequest(string password);
        protected virtual string RequestPWD(string password) {
            if (OnRequestPWD != null) {
                OnRequestPWD(password);
                }
            Error(ErrCode.APP_UNHANDLED_EVENT | ErrCode.APP_FAIL_PASSWORD);
            throw new MissingMethodException("RequestPWD event is not handled.");
            Environment.Exit(0);
            }

        /// <summary>
        /// Event raised when a name conflict is detected (Return a user-modified string or file will be automatically renamed)
        /// </summary>
        public event EvNameConflict OnNameConflict;
        public delegate string EvNameConflict(PRNode parent, string newname);
        protected virtual string NameConflict(PRNode parent, string newname) {
            if (OnNameConflict != null && !AutoRenameConflicts) {
                List<string> kidsByName = FileTable[parent.GUID].ChildrenByName;
                while (kidsByName.Contains(newname, StringComparer.OrdinalIgnoreCase)) {
                    newname = OnNameConflict(parent, newname);
                    }

                return newname;
                }
            else if (OnNameConflict == null & !AutoRenameConflicts) {

                Error(ErrCode.APP_UNHANDLED_EVENT | ErrCode.APP_NAME_CONFLICT);
                throw new MissingMethodException("RequestPWD event is not handled.");
                Environment.Exit(SystemErrorCodes.ERROR_INVALID_EVENT_COUNT); // pragma'd unreachable code because users can 'continue' exceptions

                }
            else {
                string altname = newname;
                string ext = "";
                int fileExtPos = altname.LastIndexOf(".");
                if (fileExtPos >= 0) {
                    altname = altname.Substring(0, fileExtPos);
                    ext = altname.Substring(fileExtPos, altname.Length - fileExtPos - 1);
                    }
                int count = 1;

                List<PRTableNode> nodes;
                nodes = FileTable.GetChildrenOf(parent.GUID);
                List<string> names = new List<string>();
                foreach (PRTableNode n in nodes) {
                    names.Add(n.DisplayName);
                    }

                while (names.Contains(altname, StringComparer.OrdinalIgnoreCase)) {
                    // this should HOPEFULLY append "(incrementing#)" to the filename PRE-extension
                    string tempFileName = string.Format("{0} ({1})", altname, count++);
                    altname = tempFileName + ext;
                    }

                return altname;
                }
            }

        /// <summary>
        /// Event raised when progress has been updated
        /// </summary>
        /// <param name="bytesTotal">total bytes to read</param>
        public event EvProgress ProgressReport;
        public delegate void EvProgress(ProgressTracker progress);

        /// <summary>
        /// Event raised when the current task has completed
        /// </summary>
        public event EvTaskComplete OnTaskComplete;
        public delegate void EvTaskComplete(bool errors, ErrCode result);
        protected virtual void TaskComplete(bool errors, ErrCode result) {
            if (OnTaskComplete != null) {
                OnTaskComplete(errors, result);
                }
            else {
                Log($"{ErrCode.APP_UNHANDLED_EVENT}: OnTaskComplete", LogType.Critical);
                OnError(ErrCode.APP_UNHANDLED_EVENT);
                }
            }


        #endregion "events"

        #region "public subs"
        /// <summary>
        /// A complete log containing per-file ErrCodes
        /// </summary>
        public Dictionary<string, string> Results {
            get {
                Dictionary<string, string> retVal = new Dictionary<string, string>();
                foreach (PRNode node in ProgressTree.Keys) {
                    retVal.Add(FileTable[node.GUID].Name, ProgressTree[node].ToString());
                    }
                return retVal;
                }
            }

        /// <summary>
        /// Primary entry point
        /// </summary>
        public Archive() {
            Log("Initializing PRX manager", LogType.Init);

            Utils.InitDriveInfo();

            ErrCode initErr = ErrCode.NULL;
            initErr = SetTempFile();

            Log("Creating root node", LogType.Info);
            RootNode = PRNode.CreateRootNode();
            FileTable = new PRTable();

            PackRatUI.Program.Archive = this;

            MasterFileTable = FileTable;

            if (initErr > ErrCode.SUCCESS) {
                Error(initErr);
                if (initErr.HasFlag(ErrCode.EX_ACCESS_DENIED)) {
                    System.Windows.Forms.MessageBox.Show("ERR_ACCESS_DENIED" + Environment.NewLine + "Please try running as Administrator.");
                    Environment.Exit(0);
                    }
                }
            }

        /// <summary>
        /// Open an existing PackRatUI archive
        /// </summary>
        /// <param name="fi"><fileinfo>Existing .PRX archive to open</fileinfo></param>
        /// <returns><errcode>Result</errcode></returns>
        public ErrCode OpenFile(FileInfo fi) {
            PRXFile = fi;
            return LoadArchive();
            }

        /// <summary>
        /// Helper function routing data from string[] to List&lt;string&rt; and passed to AddFiles(List, PRNode)
        /// </summary>
        /// <param name="files">string[] of files and/or directories</param>
        /// <param name="parent">PRNode of which these nodes will descend</param>
        public virtual void AddFiles(string[] files, PRNode parent = null) {
            List<string> fileList = new List<string>();
            foreach (string s in files)
                fileList.Add(s);

            this.AddFiles(fileList, parent);
            }

        /// <summary>
        /// Add a list&lt;string&gt; of files to the archive, a parent node may be specified
        /// </summary>
        /// <param name="files">List of files</param>
        /// <param name="parent">[Optional] Parent node (Will use current directory if none specified)</param>
        /// <returns></returns>
        public async virtual void AddFiles(List<string> files, PRNode parent = null) {
            //TODO Parallelize this
            string list = "";
            foreach (string s in files)
                list = list + $"{s} ";

            if (parent == null) {
                parent = RootNode;
                }
            Log($"List of Files to add:" + Environment.NewLine + $"{list}", LogType.IO);
            List<string> filesToAdd = new List<string>();
            foreach (string f in files) {
                if (!File.Exists(f) && !Directory.Exists(f)) {
                    Log($"{ErrCode.EX_FILE_NOT_FOUND}: {f}", LogType.Error);
                    }
                else {
                    filesToAdd.Add(f);
                    }
                }
            files = filesToAdd;

            if (files.Count == 0) {
                Log($"ERR: No files to add!", LogType.Warning);
                return;// ErrCode.APP_FAIL_TO_ADD;
                }

            ErrCode result = ErrCode.SUCCESS;
            PRXBusy = true;
            Task t = Task.Factory.StartNew(() => {
                result = this._AddFiles(files, parent);
                OnTaskComplete(result == ErrCode.SUCCESS ? true : false, result);
            });
            await t;
            if (t.IsCompleted) {
                PRXBusy = false;
                return;// result | ErrCode.APP_THREAD_COMPLETED;
                }
            else {
                PRXBusy = false;
                Error(ErrCode.APP_THREAD_FAILED);
                }
            }

        private ErrCode _AddFiles(List<string> files, PRNode parent = null) {
            ProgressTree = new Dictionary<PRNode, ErrCode>();
            ErrCode results = ErrCode.NULL;

            if (FileTable == null || FileTable?.Count == 0) {
                ErrCode header = ErrCode.NULL;
                try {
                    header = WriteHeader(PRXSwapFile);
                    }
                catch (Exception ex) {
                    results = Utils.MapExceptionName(ex);
                    ErrCode ecex = Utils.MapExceptionName(ex);
                    Log($"{header}, " + $"{ecex}", LogType.Critical);
                    }
                }

            ProgressTracker progress = new ProgressTracker();
            Log("List-gen recursion", LogType.Info);
            Dictionary<string, PRNode> cache = new Dictionary<string, PRNode>();
            foreach (string f in files) {
                if (File.Exists(f)) {
                    progress.TotalBytes += (ulong)f.Length;
                    progress.TotalFiles++;
                    }
                else if (Directory.Exists(f)) {
                    DirectoryInfo di = new DirectoryInfo(f);
                    PRNode nprn = new PRNode(di, parent.GUID);
                    FileTable.Add(new PRTableNode(di, nprn, di.Name, parent.GUID));
                    cache.Add(BaseToString(nprn.TableNode.Name), nprn);
                    RecurseForStatus(progress, di, nprn);
                    }
                else {
                    results &= ErrCode.EX_FILE_NOT_FOUND;
                    Log($"{ErrCode.EX_FILE_NOT_FOUND}: {f}", LogType.Critical);
                    }
                }
            Log("IO cycle start", LogType.IO);

            try {
                foreach (string f in files) {
                    if (File.Exists(f)) {
                        results &= AddFile(f, progress, parent);
                        }
                    if (Directory.Exists(f)) {
                        DirectoryInfo tdi = new DirectoryInfo(f);
                        results &= RecurseTree(tdi, progress, cache[tdi.Name]);
                        }
                    }
                }
            catch (Exception ex) {
                results &= Utils.MapExceptionName(ex);
                Log($"Exception: {Utils.MapExceptionName(ex).ToString()}", LogType.Critical);
                }

            //OnTaskComplete(false);

            if (results == ErrCode.NULL)
                results = ErrCode.SUCCESS;

            return results;
            }

        private ErrCode WriteHeader(FileStream prxStream) {
            Log("Writing PRX header", LogType.IO);

            BinaryWriter bw = new BinaryWriter(PRXSwapFile);
            bw.Seek(0, SeekOrigin.Begin);
            bw.Write("PackRat>"); // magic number (its a bit long but w/e)
            bw.Write(this.PRXVersion); // short file format version
            bw.Write((byte)this.PRXFlags); // PRXFlags

            // save this location in the stream for writing hashes. This ensures we're version-agnostic, no hardcoded offsets
            Loc_DataOffset = (short)bw.BaseStream.Position; // stored as short because this'll always be near the beginning

            bw.Write(new string('X', 64)); // write the HASH spacers

            Loc_NodeCount = (short)bw.BaseStream.Position;

            bw.Write(Int32.MaxValue); // spacer for NodeCount (will be overwritten on every file addition/removal)
            bw.Write(Int32.MaxValue); // spacer for TableOffset (will be overwritten on every file addition/removal)
            bw.Write("PRX>"); // simple PRX> begin tag to indicate data, this will be for recovery features later
            return ErrCode.SUCCESS;
            }

        /// <summary>
        /// Get a list of children of the specified PRNode
        /// </summary>
        /// <param name="parent">Parent Node</param>
        /// <returns><list type="PRTableNode">List of PRTableNodes</list></returns>
        public virtual List<PRTableNode> GetChildren(PRNode n) {
            return this.GetChildren(n.GUID);
            }
        public List<PRTableNode> GetChildren(Guid guid) {
            return FileTable.GetChildrenOf(guid);
            }

        /// <summary>
        /// Get a MemoryStream containing the data of the specified node
        /// </summary>
        /// <param name="node">Requested PRNode</param>
        /// <returns><memorystream></memorystream></returns>
        public MemoryStream GetData(PRNode node) {

            return new MemoryStream();
            }


        public ErrCode Extract(PRNode n, DirectoryInfo di, out ProgressTracker pt, string altName = "") {
            pt = new ProgressTracker();
            if (n.Flags.HasFlag(NodeFlag.Virtual))
                return ErrCode.NODE_INVALID_OPERATION;

            ErrCode result = ErrCode.SUCCESS;
            if (n.Flags.HasFlag(NodeFlag.Directory)) {
                Log($"DIR: {n.TableNode.DisplayName}", LogType.Info);
                if (!Directory.Exists(di.FullName + @"\" + n.TableNode.DisplayName)) {
                    DirectoryInfo tDir = Directory.CreateDirectory(di.FullName + @"\" + n.TableNode.DisplayName);
                    Log($"CreateDir '{tDir.FullName}'", LogType.IO);
                    result &= ExDir(n, tDir, pt);
                    }
                }
            else {
                Log($"FILE: {n.TableNode.DisplayName}", LogType.Info);
                result &= ExFile(n, di, pt, altName);
                }

            if (result != ErrCode.SUCCESS) {
                Log($"ERR: {pt[pt.Count - 1]}", LogType.Error);
                }
            return result;
            }


        private ErrCode ExDir(PRNode n, DirectoryInfo di, ProgressTracker pt) {
            Log($"Sel to Ext: {n.TableNode.DisplayName}", LogType.GUI);
            DirectoryInfo tDir = di;
            ErrCode result = ErrCode.SUCCESS;

            foreach (PRTableNode tn in FileTable.GetChildrenOf(n.GUID)) {
                Log($"Child: {tn.DisplayName}", LogType.Parse);
                if (tn.Flags.HasFlag(NodeFlag.Directory)) {

                    Log($"ExDir {tn.DisplayName}", LogType.IO);
                    if (!Utils.DebugFlags.HasFlag(Debug.NO_RECURSION))
                        result &= ExDir(tn.Node, Directory.CreateDirectory(tDir.FullName + @"\" + Utils.BaseToString(tn.Name)), pt);
                    }
                else if (tn.Flags.HasFlag(NodeFlag.File)) {
                    //Log($"ExFile {tn.DisplayName}", LogType.IO);
                    result &= ExFile(tn.Node, tDir, pt);
                    }
                }
            if (result == ErrCode.SUCCESS) {
                Log("No errors on extraction!", LogType.IO);
                return ErrCode.SUCCESS;
                }
            else {
                return result;
                }
            }

        /// <summary>
        /// Extract specified PRNode to specified file/path
        /// </summary>
        /// <param name="node">Node to extract</param>
        /// <param name="path"></param>
        /// <returns><errcode>Result</errcode></returns>
        private ErrCode ExFile(PRNode node, DirectoryInfo path, ProgressTracker pt, string altName = "") {
            Log($"Creating streams for IO ..", LogType.Init);
            PRTableNode prt = node.TableNode;
            string fName = altName == "" ? prt.DisplayName : altName;
            FileStream output = new FileStream(Path.Combine(path.FullName, fName), FileMode.Create);
            BinaryWriter bw = new BinaryWriter(output);
            PRXSwapFile.Seek(prt.Offset, 0);
            // create a hash algorithm to be used in the while(read) loop
            HashAlgorithm hasher = new MD5CryptoServiceProvider();

            long bytesRead;
            long tbr = 0;
            long bytesToRead = prt.SizeUncompressed;
            var buffer = new byte[Utils.IO_BUFFER_SIZE];

            Log($"Writing data..", LogType.IO);
            long bytesSoFar = 0;
            while (bytesToRead > 0) {
                long bytesLeft = prt.SizeUncompressed - bytesSoFar;
                long bufSize = bytesToRead > Utils.IO_BUFFER_SIZE ? Utils.IO_BUFFER_SIZE : bytesLeft;
                bytesRead = PRXSwapFile.Read(buffer, 0, (int)bufSize);
                bytesSoFar += bytesRead;
                hasher.TransformBlock(buffer, 0, (int)bufSize, null, 0);
                bytesToRead -= bytesRead;
                tbr += bytesRead;
                bw.Write(buffer, 0, (int)bytesRead);
                }

            hasher.TransformFinalBlock(new byte[0], 0, 0);
            byte[] hash = hasher.Hash;
            if (HashToHex(hash) == prt.Hash) {
                Log($"Ex OK [{prt.HashString}]", LogType.Info);
                pt[node] = ErrCode.SUCCESS;
                return ErrCode.SUCCESS;
                }
            else {
                Log($"Fail! [{prt.Hash}]", LogType.Critical);
                pt[node] = ErrCode.APP_FAIL_HASH;
                return ErrCode.APP_FAIL_HASH;
                }
            }
        #endregion "public subs"

        #region "Private subs"
        /// <summary>
        /// Generates, assigns and opens a temporary file
        /// </summary>
        /// <returns><errcode>Result</errcode></returns>
        private ErrCode SetTempFile() {
            ErrCode initErr = ErrCode.NULL;

            if (PRXSwapFile != null) {
                try {
                    PRXFile = null;
                    PRXSwapFile.Dispose();
                    }
                catch (Exception ex) { Error(Utils.MapExceptionName(ex)); }
                }

            string tempFile = Utils.GetTempFilePathWithExtension("prx");
            FileOptions fos = FileOptions.RandomAccess;

            if (!Utils.DebugFlags.HasFlag(Utils.Debug.LEAVE_TEMP_FILES))
                fos |= FileOptions.DeleteOnClose;

            try {
                PRXSwapFile = new FileStream(
                    tempFile,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    Utils.IO_BUFFER_SIZE,
                    fos // hacky way to ensure we don't leave shit behind
                    );

                PRXFile = new FileInfo(tempFile);
                }
            catch (UnauthorizedAccessException ex) {
                Error(Utils.MapExceptionName(ex) | initErr);
                initErr = ErrCode.EX_ACCESS_DENIED;
                }
            catch (Exception gex) {
                Error(Utils.MapExceptionName(gex));
                Environment.Exit(1);
                }
            if (File.Exists(tempFile)) {
                initErr |= ErrCode.SUCCESS;
                }
            Log($"Temp file {PRXFile.Name}", LogType.IO);
            return initErr;
            }

        /// <summary>
        /// Loads/Parses an existing prx archive
        /// </summary>
        /// <returns><errcode>Result</errcode></returns>
        private ErrCode LoadArchive() {
            return ErrCode.SUCCESS;
            }

        /// <summary>
        /// Recurses a directory tree on disk to prime the ProgressTracker
        /// </summary>
        /// <param name="progress">ProgressTracker to update</param>
        /// <param name="directory">Directory to crawl</param>
        /// <param name="parent">Top level node</param>
        /// <returns></returns>
        private ErrCode RecurseForStatus(ProgressTracker progress, DirectoryInfo directory, PRNode parent = null) {
            foreach (FileInfo fi in directory.GetFiles()) {
                progress.TotalBytes += (ulong)fi.Length;
                progress.TotalFiles++;
                }
            foreach (DirectoryInfo di in directory.GetDirectories()) {
                progress.TotalFolders++;
                //PRNode thisDir = new PRNode(di, parent.GUID);
                //FileTable.Add(new PRTableNode(di, thisDir, di.Name, parent.GUID));
                RecurseForStatus(progress, di, parent);
                }
            return ErrCode.SUCCESS;
            }


        private FileInfo tfi = null;
        private DirectoryInfo tdi = null;
        /// <summary>
        /// Recurses a directory structure on disk
        /// </summary>
        /// <param name="path">The root path from which to begin recursion</param>
        /// <returns>ErrCode</returns>
        private ErrCode RecurseTree(DirectoryInfo directory, ProgressTracker progress, PRNode parent) {
            if (parent == null)
                parent = RootNode;

            FileInfo[] files = directory.GetFiles();
            DirectoryInfo[] dirs = directory.GetDirectories();

            try {
                foreach (FileInfo fi in files) {
                    tfi = fi;
                    ErrCode result = AddFile(fi.FullName, progress, parent);
                    }

                //BUG WHAT THE FUCK IS GOING ON-- FIX THIS UNDEF SHIT
                // we get random NullRef exceptions if the files ownership or permissions aren't perfectly set.. can't track it down.
                foreach (DirectoryInfo di in dirs) {
                    tdi = di;

                    //AddDirectory();

                    PRNode thisDir = new PRNode(di, parent.GUID);
                    FileTable.Add(new PRTableNode(di, thisDir, di.Name, parent.GUID));
                    progress.FoldersProcessed++;
                    if (!Utils.DebugFlags.HasFlag(Debug.NO_RECURSION))
                        RecurseTree(di, progress, thisDir);
                    }
                }

            catch (NullReferenceException ex) {
                Log($"NullRef: {Utils.MapExceptionName(ex).ToString()}", LogType.Critical);
                Log($"{ex.Message}", LogType.Exception);
                Log($"{tdi.Name}", LogType.Exception);
                Log($"{tfi.Name}", LogType.Exception);
                }
            catch (UnauthorizedAccessException ex) {
                Log($"AccessEx: {Utils.MapExceptionName(ex).ToString()}", LogType.Critical);
                Log($"{ex.Message}", LogType.Exception);
                Log($"{tdi.Name}", LogType.Exception);
                Log($"{tfi.Name}", LogType.Exception);
                }
            catch (Exception ex) {
                Log($"UnkEX: {Utils.MapExceptionName(ex).ToString()}", LogType.Critical);
                Log($"{ex.Message}", LogType.Exception);
                Log($"{tdi.Name}", LogType.Exception);
                Log($"{tfi.Name}", LogType.Exception);

                }

            return ErrCode.SUCCESS;
            }

        /// <summary>
        /// Adds a directory entry to the archive
        /// </summary>
        /// <param name="dir">Directory to be added</param>
        /// <param name="progress">ProgressTracker</param>
        /// <param name="parent">Parent node of which this node belongs</param>
        /// <returns>ErrCode</returns>
        private PRNode AddDirectory(DirectoryInfo di, ProgressTracker progress, PRNode parent) {
            PRNode retVal = new PRNode(di, parent.GUID);
            PRTableNode prtn = new PRTableNode(di, retVal, di.Name, parent.GUID);
            FileTable.Add(prtn);
            //BUG Directories aren't actually written to the archive, dumbass.

            #region failure
            //long startOfWrite = 0;
            //retVal.AssignTableNode(prtn);

            //try {
            //    BinaryWriter bw = new BinaryWriter(PRXSwapFile);

            //    bw.Seek(0, SeekOrigin.End);
            //    startOfWrite = bw.BaseStream.Position;

            //    bw.Write(retVal.GUID.ToByteArray());
            //    bw.Write(new string('X', 16)); // we write a spacer for the hash that'll be inserted after we actually parse the data
            //    bw.Write((short)retVal.TableNode.DisplayName.Length);
            //    long dataOffset = bw.BaseStream.Position;
            //    retVal.TableNode.Offset = (long)dataOffset;

            //    // create a hash algorithm to be used in the while(read) loop
            //    HashAlgorithm hasher = new MD5CryptoServiceProvider();

            //    int bytesRead;
            //    var buffer = new byte[Utils.IO_BUFFER_SIZE];
            //    progress.BytesProcessedInFile = 0;
            //    progress.TotalBytesInFile = fi.Length;
            //    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0) {
            //        progress.BytesProcessed += bytesRead;
            //        progress.BytesProcessedInFile += bytesRead;

            //        //TODO encrypt/compress data [optional]

            //        bw.Write(buffer, 0, bytesRead);
            //        hasher.TransformBlock(buffer, 0, bytesRead, null, 0);
            //        ProgressReport(progress);
            //        }

            //    hasher.TransformFinalBlock(new byte[0], 0, 0);
            //    byte[] hash = hasher.Hash;
            //    prtn.Hash = HashToHex(hash);
            //    PRXSwapFile.Seek(dataOffset - 24, SeekOrigin.Begin);
            //    //bw.Seek(dataOffset - 24, SeekOrigin.Begin);
            //    PRXSwapFile.Write(hash, 0, hash.Length);
            //    //bw.Write(hash);

            //    //Log($"Wrote {progress.BytesProcessedInFile}b [{prtn.HashString}] at offset: {dataOffset}, EOD: {bw.BaseStream.Position}", LogType.IO);

            //    // move back to the end of the stream just for safety
            //    bw.Seek(0, SeekOrigin.End);
            //    PRXSwapFile.Seek(0, SeekOrigin.End);
            //    //BUG Not properly inserting all files anymore.. WTF
            //    //INFO Have to close the BinaryWriter without colosing the underlying stream.. FUCK.
            //    //bw.Dispose();

            //    // supposedly the binarywriter is closed *WITHOUT* nuking PRXSwapFile..

            //    }
            //catch (Exception ex) {
            //    //TODO remove whatever may have been written in the event of a failure
            //    // this'll be added at a later date when I'm able to produce an error
            //    //Log($"EX: {MapExceptionName(ex)} on {fi.Name} in {fi.DirectoryName}", LogType.Exception);
            //    return MapExceptionName(ex);
            //    }
            //finally {

            //    fs.Dispose();
            //    //GC.Collect(1); //Cardinal sin, I know.. But I can't dispose of the binarywriter, has to be collected so it won't close PRXSwapFile

            //    }
            #endregion failure

            return retVal;
            }

        /// <summary>
        /// Add a file to the prx archive beneath specified node
        /// </summary>
        /// <param name="file">File to be added</param>
        /// <param name="parent">Parent node of added file</param>
        /// <returns><errcode>Result</errcode></returns>
        private ErrCode AddFile(string file, ProgressTracker progress, PRNode parent) {
            ErrCode err = ErrCode.NULL;

            if (!File.Exists(file)) {
                err = ErrCode.APP_FAIL_TO_READ;
                return err;
                }

            // forcefully demand read access to the target file, returning ErrCode.EX_ACCESS_DENIED if this fails
            try {
                new FileIOPermission(FileIOPermissionAccess.Read, file).Demand();
                }
            catch (SecurityException se) {
                err = ErrCode.EX_ACCESS_DENIED;
                return MapExceptionName(se);
                }
            catch (NullReferenceException ex) {
                Log($"Ex: {MapExceptionName(ex)}", LogType.Error);
                }

            long startOfWrite = 0;
            FileInfo fi = new FileInfo(file);
            PRNode node = new PRNode(fi.Name, parent.GUID);
            PRTableNode prtn = new PRTableNode(fi, node, fi.Name, parent.GUID);
            FileStream fs = null;
            node.AssignTableNode(prtn);


            //TODO Refactor this to a BinaryWriter extension: BinaryWriter.Write(PRNode)

            try {
                BinaryWriter bw = new BinaryWriter(PRXSwapFile);

                fs = new FileStream(fi.FullName, FileMode.Open);

                // jump to the end of the swap file
                bw.Seek(0, SeekOrigin.End);
                startOfWrite = bw.BaseStream.Position;

                node.TableNode.SizeUncompressed = fi.Length;

                // write the data header
                bw.Write(node.GUID.ToByteArray());
                bw.Write(new string('X', 16)); // we write a spacer for the hash that'll be inserted after we actually parse the data
                bw.Write((ulong)fi.Length);
                long dataOffset = bw.BaseStream.Position;
                node.TableNode.Offset = (long)dataOffset;

                // create a hash algorithm to be used in the while(read) loop
                HashAlgorithm hasher = new MD5CryptoServiceProvider();

                int bytesRead;
                var buffer = new byte[Utils.IO_BUFFER_SIZE];
                progress.BytesProcessedInFile = 0;
                progress.TotalBytesInFile = fi.Length;
                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0) {
                    progress.BytesProcessed += bytesRead;
                    progress.BytesProcessedInFile += bytesRead;

                    //TODO encrypt/compress data [optional]

                    bw.Write(buffer, 0, bytesRead);
                    hasher.TransformBlock(buffer, 0, bytesRead, null, 0);
                    ProgressReport(progress);
                    }

                hasher.TransformFinalBlock(new byte[0], 0, 0);
                byte[] hash = hasher.Hash;
                prtn.Hash = HashToHex(hash);
                PRXSwapFile.Seek(dataOffset - 24, SeekOrigin.Begin);
                //bw.Seek(dataOffset - 24, SeekOrigin.Begin);
                PRXSwapFile.Write(hash, 0, hash.Length);
                //bw.Write(hash);

                //Log($"Wrote {progress.BytesProcessedInFile}b [{prtn.HashString}] at offset: {dataOffset}, EOD: {bw.BaseStream.Position}", LogType.IO);

                // move back to the end of the stream just for safety
                bw.Seek(0, SeekOrigin.End);
                PRXSwapFile.Seek(0, SeekOrigin.End);
                //BUG Not properly inserting all files anymore.. WTF
                //INFO Have to close the BinaryWriter without colosing the underlying stream.. FUCK.
                //bw.Dispose();

                // supposedly the binarywriter is closed *WITHOUT* nuking PRXSwapFile..

                }
            catch (Exception ex) {
                //TODO remove whatever may have been written in the event of a failure
                // this'll be added at a later date when I'm able to produce an error
                //Log($"EX: {MapExceptionName(ex)} on {fi.Name} in {fi.DirectoryName}", LogType.Exception);
                return MapExceptionName(ex);
                }
            finally {

                fs.Dispose();
                //GC.Collect(1); //Cardinal sin, I know.. But I can't dispose of the binarywriter, has to be collected so it won't close PRXSwapFile

                }

            // increment the number of files processed in our tracker
            progress.FilesProcessed++;

            // finally, if and only if this is ErrCode.SUCCESS- add the node as a TableNode to our FileTable
            if (err == ErrCode.NULL)
                FileTable.Add(prtn);

            return ErrCode.SUCCESS;
            }

        #endregion "Private subs"

        }
    //#pragma warning restore 0162, CS0219

    public class ProgressTracker {
        public int TotalFiles = 0;
        public int FilesProcessed = 0;
        public ulong TotalBytes = 0;
        public long BytesProcessed = 0;
        public long TotalBytesInFile = 0;
        public long BytesProcessedInFile = 0;
        public int FoldersProcessed = 0;
        public int TotalFolders = 0;

        private Dictionary<PRNode, ErrCode> Errors;

        public ErrCode this[PRNode prn] {
            get {
                if (Errors.ContainsKey(prn)) {
                    return Errors[prn];
                    }
                else {
                    return ErrCode.NULL;
                    }
                }
            set {
                if (Errors.ContainsKey(prn)) {
                    Errors[prn] = value;
                    }
                else {
                    Errors.Add(prn, value);
                    }
                }
            }
        public PRNode this[int i] {
            get {
                if (i > Errors.Count - 1)
                    return null;

                return Errors.ElementAt(i).Key;
                }
            }

        public ProgressTracker() {
            Errors = new Dictionary<PRNode, ErrCode>();
            }

        public int Count {
            get {
                return this.Errors.Count;
                }
            }

        internal void Clear() {
            TotalFiles = 0;
            FilesProcessed = 0;
            TotalBytes = 0;
            BytesProcessed = 0;
            TotalBytesInFile = 0;
            BytesProcessedInFile = 0;
            FoldersProcessed = 0;
            TotalFolders = 0;
            }

        // restore the warnings
#pragma warning restore 0169, 0649, 0067, 0168, 0219, 0162, CS0219
        }
    }

/*  PRX FILE LAYOUT # root node is never actually written to the archive, it exists only virtually
    @0   byte[8]   - magic number; PackRat>
    @8   byte      - archive format version
    @9   byte      - FLAGS (bitfield; BIN:????????)
    @10? short     - multi-part--part number (this field won't exist unless FLAGS &= 0x10000000
    @10  byte[32]  - MD5 hash of <TABLE>  // this won't be bad because we can seek and just overwrite specific bytes
    @42  byte[32]  - MD5 hash of <NODES>
    // everything from here on is ENCRYPTED if xxxxxxx1 in FLAGS
    @74  ushort    - notes length
    @76  long      - nodes location
    @84  uint      - node count
    @88  long      - table offset (points to the byte-specific location containing a lookup table for all files in the archive (end of the archive)) // should be INT(offset)char[16](guid)
    @92  char[^]   - archive notes text
         byte[4]   - PRX> // we record <TABLE> offset to be able to remove it in a hurry for appending more files (Delete FROM Offset 'till Size-32)
         byte[??]  - <NODES>
    @84> byte[??]  - <TABLE>
         byte[4]   - <PRX
         // end of encryption
    byte[32]       - HASH OF == EVERYTHING ABOVE ==
    -------------------------
    <NODE> format:
    @0  byte[16] - GUID
    @58 byte[^]  - <DATA>
    -------------------------
    <TABLE> format: 
    byte[16] - GUID
    byte[16] - Parent GUID // moving files to another folder should be EASY this time around, just change the fucking parent-GUID, and we know it's in the small <TABLE> object at EOF
    ulong    - creation date (in epoch time)
    ulong    - modification date (in epoch time)
    byte     - ATTRIB flags
    ushort   - Name length // considered not using this and going with char[64] for names, but it's just the <table> so rewriting it ain't that bad. 10s of KB at the most, instantaineous
             - // we ARE however limited to 65536 ASCII-chars (or 8192 UTF8 chars)
    char[^]  - file name (in ascii-base64)
    ulong    - byte-specific offset to the node in the archive
    */
