using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Windows.Forms;
using PackRat;

namespace PackRatUI {
    public partial class PackRatUI : Form {
        public List<PRTableNode> DirListing;
        private Guid DirCurrent;
        private DbgWin LogWin;
        private Archive PRX;
        private Action<string, LogType> Log = DbgWin.Log;

        private ImageList _smallImageList = new ImageList();
        private ImageList _largeImageList = new ImageList();

        private SaveState saveState;

        //TODO get system icons for file types and cache them internally
        // ignoring ones that can change.. (ie; executables and images)
        // getting image thumbs may be a whore though :(

        public PackRatUI() {
            InitializeComponent();

            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(PackRat_DragEnter);
            this.DragDrop += new DragEventHandler(PackRat_DragDrop);

            saveState = new SaveState();

            // hook our instance up to the static DbgWin defined in Program
            LogWin = Program.LogWin;
            LogWin.Location = new Point(this.Location.X + 32, this.Location.Y);
            LogWin.Show();
            Log("Hooked up LogWin", LogType.Init);
            Log("Starting application", LogType.GUI);

            if (PWin == null) {
                PWin = Program.ProgWin;
                Log("Hooked up ProgWin", LogType.GUI);
                }

            Log("Init Archive() and registering callbacks", LogType.Init);
            Archive.OnLog = DbgWin.Log;
            PRX = new Archive();
            PRX.OnTaskComplete += Mba_TaskComplete;
            PRX.OnError += Mba_Error;
            PRX.OnRequestPWD += Mba_RequestPWD;
            PRX.OnNameConflict += Mba_OnNameConflict;
            PRX.ProgressReport += Mba_Progress;
            DirCurrent = PRX.RootPath.GUID;
            PRX.RootPath.Flags = NodeFlag.Virtual;

            }

        void PackRat_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            }

        void PackRat_DragDrop(object sender, DragEventArgs e) {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            //foreach (string file in files)
            PRX.AddFiles(files);
            }

        private string Mba_OnNameConflict(PRNode parent, string newname) {
            //throw new NotImplementedException();
            return "nothing";
            }

        private ProgressWin PWin;
        private void Mba_Progress(ProgressTracker pt) {

            //CURRENT show the progress dialog since data's already being routed properly
            }

        private string Mba_RequestPWD(string password) {
            //throw new NotImplementedException();
            return "nothing";
            }

        private void Mba_Error(ErrCode ec) {
            Log("PRX_ERR: " + ec.ToString(), LogType.Critical);
            }

        private void Mba_TaskComplete(bool succeded, ErrCode result) {
            //TODO display results and build the STATUS object in PackRat
            Log($"Result: {succeded.ToString()} [{result}]", succeded ? LogType.Info : LogType.Critical);
            objectListView1.SetObjects(PRX.GetChildren(DirCurrent));
            }

        private void PackRat_Shown(object sender, EventArgs e) {
            }

        private void PackRat_Move(object sender, EventArgs e) {
            LogWin.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            }

        private void PackRat_Resize(object sender, EventArgs e) {
            LogWin.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            }

        private void button1_Click(object sender, EventArgs e) {
            //PRX.AddFiles(new List<string> { @"X:\Battle.Net" });
            DirListing = PRX.GetChildren(DirCurrent);
            objectListView1.SetObjects(DirListing);
            }

        private void button2_Click(object sender, EventArgs e) {
            Log("User initiated BREAK event", LogType.Warning);

            }

        private void objectListView1_DoubleClick(object sender, EventArgs e) {
            PRTableNode selObject = (PRTableNode)objectListView1.SelectedObject;
            if (selObject.Node.Flags.HasFlag(NodeFlag.Directory)) {
                DirListing = PRX.GetChildren(selObject.GUID);

                if (selObject.GUID != PRX.RootPath.GUID)
                    DirListing.Insert(0, PRX.MasterFileTable.GetVirtualUpNode(selObject));

                objectListView1.SetObjects(DirListing);
                }
            }

        private static ErrCode HasPermission(string destDir) {
            return ErrCode.SUCCESS;
            //if (string.IsNullOrEmpty(destDir) || !Directory.Exists(destDir))
            //    return ErrCode.EX_ACCESS_DENIED;
            //try {
            //    DirectorySecurity security = Directory.GetAccessControl(destDir);
            //    SecurityIdentifier users = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            //    foreach (AuthorizationRule rule in security.GetAccessRules(true, true, typeof(SecurityIdentifier))) {
            //        if (rule.IdentityReference == users) {
            //            FileSystemAccessRule rights = ((FileSystemAccessRule)rule);
            //            if (rights.AccessControlType == AccessControlType.Allow) {
            //                if (rights.FileSystemRights == (rights.FileSystemRights | FileSystemRights.Modify))
            //                    return ErrCode.SUCCESS;
            //                }
            //            }
            //        }
            //    return ErrCode.EX_ACCESS_DENIED;
            //    } catch (Exception ex) {
            //    ErrCode ec = Utils.MapExceptionName(ex);
            //    return ec;
            //    }
            }

        //CURRENT Move this to PackRat.cs - Doesn't belong in the UI
        //TODO Why is this in the UI? Needs to be moved to PackRat.cs
        //private void ExtractRecurse(PRTableNode tn, string outPath) {
        //    DirectoryInfo di = Directory.CreateDirectory(outPath + tn.Name);
        //    outPath = di.FullName + @"\";

        //    foreach (PRTableNode prtn in PRX.GetChildren(tn.GUID)) {
        //        if (prtn.Flags.HasFlag(NodeFlag.File)) {
        //            FileInfo fi = new FileInfo(outPath + prtn.Name);
        //            Log($"EXFil: {prtn.GUID} => {fi.Directory}", LogType.IO);
        //            PRX.ExtractFile(prtn.Node, fi.Directory, fi.Name);
        //            }

        //        if (prtn.Flags.HasFlag(NodeFlag.Directory)) {
        //            Log($"EXDir: {prtn.GUID} => {outPath}", LogType.IO);
        //            ExtractRecurse(prtn, outPath);
        //            }

        //        }

        //    }

        private void bExtract_Click(object sender, EventArgs e) {


            if (objectListView1.SelectedObjects.Count == 1) {
                //    Log("Multiple items selected, aborting", LogType.Warning);
                //    return;
                //    }
                PRTableNode selObject = (PRTableNode)objectListView1.SelectedObject;
                if (selObject == null)
                    return;

                ProgressTracker pt = new ProgressTracker();
                Log($"Extract: {selObject.Name}", LogType.Info);
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = selObject.DisplayName;
                sfd.InitialDirectory = saveState.LastDirectory.FullName;

                if (selObject.Node.Flags.HasFlag(NodeFlag.Directory)) {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.ShowNewFolderButton = true;
                    //TODO Store previously selected directory
                    fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    DialogResult fbdres = fbd.ShowDialog();
                    if (fbdres == DialogResult.OK || fbdres == DialogResult.Yes) {
                        Log($"Selected \"{fbd.SelectedPath}\" for output", LogType.IO);
                        //ExtractRecurse(selObject, fbd.SelectedPath);
                        PRX.Extract(selObject.Node, new DirectoryInfo(fbd.SelectedPath), out pt);
                        }
                    return;
                    }


                DialogResult res = sfd.ShowDialog();
                if (res == DialogResult.OK || res == DialogResult.Yes) {
                    FileInfo fi = new FileInfo(sfd.FileName);
                    ErrCode result = HasPermission(fi.Directory.FullName);
                    if (!result.HasFlag(ErrCode.SUCCESS)) {
                        Log($"Extract Failed: {result}", LogType.Error);
                        MessageBox.Show($"Failed to extract {Environment.NewLine}{fi.Directory.FullName} : {result}");
                        return;
                        }
                    // extract file to target dir
                    Log($"EX: {selObject.GUID} => {fi.Directory}", LogType.IO);
                    PRX.Extract(selObject.Node, fi.Directory, out pt, sfd.FileName);
                    }

                }
            else if (objectListView1.SelectedObjects.Count > 1) {

                ProgressTracker pt = new ProgressTracker();
                Log($"Extract: Multiple", LogType.Info);

                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.ShowNewFolderButton = true;
                //TODO Store previously selected directory
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                DialogResult fbdres = fbd.ShowDialog();
                if (fbdres == DialogResult.OK || fbdres == DialogResult.Yes) {
                    Log($"Selected \"{fbd.SelectedPath}\" for output", LogType.IO);
                    foreach (PRTableNode selObject in objectListView1.SelectedObjects) {

                        PRX.Extract(selObject.Node, new DirectoryInfo(fbd.SelectedPath), out pt);
                        }
                    }
                return;

                }

            }


        }
    }