using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackRat {
    public class PRTable : PRBase {

        public static object theLock = new object();

        private Dictionary<Guid, PRTableNode> Nodes;

        #region "Accessors"
        public int Count {
            get {
                return Nodes.Count;
                }
            }

        public PRTableNode this[Guid index] {
            get {
                return Nodes[index];
                }
            }

        public List<PRTableNode> this[int index] {
            get {
                return Nodes.Values.ToList();
                }
            set {
                Nodes.Clear();
                foreach (PRTableNode prtn in value) {
                    Nodes.Add(prtn.GUID, prtn);
                    }
                }
            }

        /// <summary>
        /// Returns true if the specified Guid exists in the FileTable
        /// </summary>
        /// <param name="guid">Guid to find</param>
        /// <returns><Boolean>True if Guid exists in the FileTable</Boolean></returns>
        public bool Contains(Guid guid) {
            if (Nodes.ContainsKey(guid))
                return true;
            else
                return false;
            }

        //public IEnumerator GetEnumerator() {
        //    foreach (object o in Nodes.Values.ToList()) {
        //        if (o == null) {
        //            break;
        //            }
        //        yield return o;
        //        }
        //    }
        #endregion "Accessors"

        /// <summary>
        /// Creates a virtual node to display, linking 'up' a level in the tree
        /// </summary>
        /// <param name="current">Current <PRTableNode>PRTableNode</PRTableNode> from which to generate the virtual 'up' node</param>
        /// <returns><PRTableNode>PRTableNode</PRTableNode></returns>
        public PRTableNode GetVirtualUpNode(PRTableNode current) {
            PRTableNode virtualUP = PRTableNode.CreateUp(new PRNode("Up", Guid.Empty));
            virtualUP.GUID = GetPPGuid(current.GUID);
            virtualUP.Name = Utils.StringToBase("Up");
            virtualUP.Node.Flags = NodeFlag.Directory;
            return virtualUP;
            }

        /// <summary>
        /// Returns the specified Guid's parent Guid
        /// </summary>
        /// <param name="child">The child whose parent is to be returned</param>
        /// <returns><Guid>Guid</Guid></returns>
        private Guid GetPPGuid(Guid child) {
            if (Nodes.ContainsKey(Nodes[child].Parent))
                return Nodes[Nodes[child].Parent].GUID;
            else
                return Nodes[child].Parent;
            }

        /// <summary>
        /// Initializes the MasterFileTable and instantiates a virtual 'root' from which all nodes are descended
        /// </summary>
        public PRTable() {
            Log("MasterFileTable initialize and set virtual toplevel node", LogType.FTable);
            Nodes = new Dictionary<Guid, PRTableNode>();

            PRTableNode prt = PRTableNode.CreateUp(RootNode);
            Encoding enc = System.Text.Encoding.UTF8;
            prt.GUID = RootNode.GUID;
            prt.Parent = RootNode.GUID;
            prt.DateCreated = 0;
            prt.DateModified = 0;
            prt.Attribs = Attrib.Root;
            prt.NameLength = Convert.ToBase64String(enc.GetBytes("/")).Length;
            prt.Name = Convert.ToBase64String(enc.GetBytes("/"));
            
            Nodes.Add(prt.GUID, prt);
            }

        /// <summary>
        /// Adds an orphan <PRTableNode>&lt;PRTableNode&gt;</PRTableNode> to the FileTable
        /// </summary>
        /// <param name="node">PRTableNode to insert</param>
        /// <returns><ErrCode>ErrCode</ErrCode></returns>
        public ErrCode Add(PRTableNode node) {
            //Log($"Adding PRTNode {node.Name}", LogType.Info);
            if (Nodes.Values.Contains(node)) {
                return ErrCode.APP_DUPLICATE_NODE;
                }
            else {
                Nodes.Add(node.GUID, node);
                return ErrCode.SUCCESS;
                }
            }

        /// <summary>
        /// Adds one node to another. (eg; Parent adopts Child)
        /// </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="child">Child to be adopted</param>
        /// <returns><ErrCode>ErrCode</ErrCode></returns>
        public ErrCode AddChildTo(PRNode parent, PRNode child) {
            Log($"Setting {Nodes[child.GUID].Name}'s parent to {Nodes[parent.GUID].Name}", LogType.Info);
            if (Nodes.ContainsKey(child.GUID)) {
                Nodes[child.GUID].Parent = parent.GUID;
                return ErrCode.SUCCESS;
                }
            return ErrCode.APP_NONEXISTANT_NODE;
            }

        /// <summary>
        /// Returns a List<PRTableNode>&lt;PRTableNode&gt;</PRTableNode> containing the 'children' of the specified GUID
        /// </summary>
        /// <param name="guid">Guid from which to iterate children</param>
        /// <returns>List&lt;PRTableNode&gt;</returns>
        public List<PRTableNode> GetChildrenOf(Guid guid) {
            List<PRTableNode> retVal = new List<PRTableNode>();
            Log($"List children of {guid}", LogType.FTable);
            foreach (PRTableNode prt in Nodes.Values) {
                if (prt.Flags.HasFlag(NodeFlag.Directory)) {
                    }
                
                if (prt.Parent == guid && prt.GUID != Guid.Empty)
                    if (!prt.Flags.HasFlag(NodeFlag.Virtual) && prt.GUID != Guid.Empty)
                        retVal.Add(prt);
                }
            return retVal;
            }

        }
    }
