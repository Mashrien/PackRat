using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PackRat {
    public class PRNode : PRBase {
        public Guid GUID { get; } = Guid.Empty;

        // These two properties are virtual only, they're not to be written to the .PRX
        [NonSerialized]
        public Guid Parent;

        [NonSerialized]
        public NodeFlag Flags;

        [NonSerialized]
        private PRTableNode _TableNode;
        /// <summary>
        /// The PRTableNode associated with this data node
        /// </summary>
        public PRTableNode TableNode {
            get {
                if (FileTable.Contains(this.GUID))
                    return FileTable[this.GUID];
                else
                    return _TableNode;
                }
            }

        private PRNode(string name, Guid guid, Attrib attrib) {
            this.GUID = guid;
            this.Flags = NodeFlag.Directory;
            }

        /// <summary>
        /// Only to be called to instantiate a ROOT node
        /// </summary>
        public static PRNode CreateRootNode() {
            PRNode rootNode = new PRNode(Utils.StringToBase("/"), Guid.Empty, Attrib.Root);
            rootNode.Parent = rootNode.GUID;
            return rootNode;
            }

        /// <summary>
        /// Create a new file-based PRNode
        /// </summary>
        /// <param name="_name">File Name</param>
        /// <param name="_parent">Parent GUID</param>
        /// <param name="_creation">Creation date as epoch</param>
        /// <param name="_access">Access date as epoch</param>
        /// <param name="_size">Size of the file</param>
        /// <param name="_attribs">RASH / OS file attributes</param>
        public PRNode(string _name, Guid _parent) {
            this.Parent = _parent;
            this.GUID = NewGUID();
            this.Flags = NodeFlag.File;
            }

        /// <summary>
        /// Create a new file-based PRNode
        /// </summary>
        /// <param name="fi">File from which the PRNode is generated</param>
        /// <param name="parent">File's parent GUID</param>
        public PRNode(FileInfo fi, Guid parent) {
            //Log($"New File {fi.Name}", LogType.IO);
            this.Parent = parent;
            this.GUID = Guid.NewGuid();
            this.Flags = NodeFlag.File;
            }

        /// <summary>
        /// Create a new directory-based PRNode
        /// </summary>
        /// <param name="di">Directory</param>
        /// <param name="parent">Parent GUID</param>
        public PRNode(DirectoryInfo di, Guid parent) {
            //Log($"New Dir {di.Name}", LogType.IO);
            this.Parent = parent;
            this.GUID = Guid.NewGuid();
            this.Flags = NodeFlag.Directory;
            //this.DataULength = -1;
            }

        public ErrCode AssignTableNode(PRTableNode prtn) {
            this._TableNode = prtn;
            return ErrCode.SUCCESS;
            }
        }
    }
