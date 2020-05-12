using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedWinapi;


namespace PackRat {
    public class PRTableNode : PRBase {
        public Guid GUID;
        public Guid Parent;
        public double DateCreated, DateModified;
        public long SizeUncompressed, SizeCompressed;
        public Attrib Attribs;
        public NodeFlag Flags;
        public string Hash;
        public int NameLength;
        public string Name;
        public long Offset;
        public PRNode Node { get; } = null;

        #region "properties"
        public string Size {
            get {
                if (Node != null) {
                    if (!Flags.HasFlag(NodeFlag.Directory))
                        return SizeUncompressed.ToString();
                    else
                        return "";
                    }
                else {
                    return "";
                    }
                }
            }

        public List<string> ChildrenByName {
            get {
                if (!this.Node.Flags.HasFlag(NodeFlag.Directory))
                    return null;
                List<string> kids = new List<string>();
                foreach (PRTableNode n in FileTable.GetChildrenOf(this.Node.GUID)) {
                    kids.Add(Utils.BaseToString(FileTable[n.GUID].Name));
                    }
                return kids;
                }
            }

        public ErrCode AddChild(PRNode n) {
            if (this.Node == n) {
                Log("Can't set ourself as our own parent!", LogType.Error);
                return ErrCode.NODE_INVALID_OPERATION;
                }
            n.TableNode.Parent = this.GUID;
            return ErrCode.SUCCESS;
            }

        public string HashString {
            get {
                if (Hash != null)
                    return Encoding.ASCII.GetString((Encoding.ASCII.GetBytes(Hash)));
                else
                    return string.Empty;
                }
            }

        public string DisplayName {
            get {
                return Utils.BaseToString(Name);
                }
            }

        public string SizeUncompressedString {
            get {
                if (this.Flags.HasFlag(NodeFlag.Directory))
                    return "";
                else
                    return Utils.SizeSuffix(SizeUncompressed);
                }
            }
        #endregion "properties"

        /// <summary>
        /// Creates a Master FileTable node, shouldn't be called by outsiders
        /// </summary>
        private PRTableNode(PRNode node, string name, Guid parent) {
            Encoding enc = System.Text.Encoding.UTF8;
            Parent = parent;
            this.GUID = node.GUID;
            this.Parent = node.Parent;
            this.NameLength = Convert.ToBase64String(enc.GetBytes(name)).Length;
            this.Name = Convert.ToBase64String(enc.GetBytes(name));
            this.Node = node;
            this.Flags = node.Flags;
            }

        private PRTableNode(PRNode n) {
            this.Node = n;
            }

        /// <summary>
        /// Creates a virtual UP table node for navigation purposes
        /// </summary>
        /// <param name="n">Virual PRNode from which to base this</param>
        /// <returns>Virtual PRTableNode (that doesn't exist in the file table)</returns>
        public static PRTableNode CreateUp(PRNode n) {
            PRTableNode prtn = new PRTableNode(n);
            return prtn;
            }

        /// <summary>
        /// Create a new PRTableNode
        /// </summary>
        /// <param name="directoryInfo" type="DirectoryInfo">DirectoryInfo object</param>
        /// <param name="node">PRNode representing the directory</param>
        /// <param name="name">Name of the directory</param>
        public PRTableNode(DirectoryInfo dir, PRNode node, string name, Guid parent) : this(node, name, parent) {
            this.Attribs = Utils.GetAttribs(dir);
            this.SizeUncompressed = 0;
            this.SizeCompressed = 0;
            this.DateCreated = Utils.DateTimeToUnixTimestamp(dir.CreationTime);
            this.DateModified = Utils.DateTimeToUnixTimestamp(dir.LastWriteTime);
            //Log($"New {node.Flags} {name} in {Utils.BaseToString(FileTable?[node.Parent]?.Name)}", LogType.FTable);
            }

        /// <summary>
        /// Create a new FILE PRTableNode
        /// </summary>
        /// <param name="fileInfo" type="FileInfo">FileInfo object</param>
        /// <param name="node">PRNode representing the file</param>
        /// <param name="name">Name of the file</param>
        public PRTableNode(FileInfo file, PRNode node, string name, Guid parent) : this(node, name, parent) {
            try {
                this.Attribs = Utils.GetAttribs(file);
                this.SizeCompressed = file.Length;
                this.SizeUncompressed = 0;
                this.DateCreated = Utils.DateTimeToUnixTimestamp(file.CreationTime);
                this.DateModified = Utils.DateTimeToUnixTimestamp(file.LastWriteTime);
                //Log($"New {node.Flags} {name} in {Utils.BaseToString(FileTable?[node.Parent]?.Name)}", LogType.FTable);
                }
            catch (Exception ex) {
                Log($"Exception: {Utils.MapExceptionName(ex).ToString()}", LogType.Critical);
                }
            }


        }
    }
