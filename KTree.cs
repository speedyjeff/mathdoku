using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathdoku
{
    class KTreeNode
    {
        public int Value { get; set; }
        public int Slot { get; set; }
        public KTreeNode Parent { get; set; }
        public List<KTreeNode> children { get; set; }

        public KTreeNode()
        {
            Value = 0;
            Slot = 0;
            Parent = null;
            children = new List<KTreeNode>();
        }
    }

    class KTree
    {

        public KTree()
        {
            Head = new KTreeNode();
        }

        public KTreeNode Head { get; private set; }

        public void AddChild(KTreeNode node, KTreeNode child)
        {
            if (node == null) return;
            if (node.children == null) return;

            node.children.Add(child);
        }

        public void RemoveChild(KTreeNode node, KTreeNode child)
        {
            if (node == null) return;
            if (node.children == null) return;

            node.children.Remove(child);
        }
    }
}
