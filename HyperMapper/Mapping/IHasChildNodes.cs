using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper.Vocab;
using ChildNode = System.Tuple<HyperMapper.Vocab.Term, HyperMapper.UrlPart, System.Uri, string, System.Func<System.Threading.Tasks.Task<HyperMapper.Mapping.AbstractNode>>>;

namespace HyperMapper.Mapping
{
    public interface IHasChildNodes
    {
        ChildNodes ChildNodes { get; }
    }

    public class ChildNodes : IEnumerable<ChildNode>
    {
        
        public Dictionary<UrlPart, ChildNode> Items;
        public ChildNodes(IEnumerable<AbstractNode> nodes)
        {
            Items = nodes.Where(n => n != null).Select(node => new ChildNode(node.Term, node.UrlPart, node.Uri, node.Title, () => Task.FromResult(node))).ToDictionary(n => n.Item2, n => n);
        }
        public ChildNodes(IEnumerable<ChildNode> nodes)
        {
            Items = nodes.Where(n => n != null).ToDictionary(n => n.Item2, n => n);
        }


        public ChildNodes Append(params AbstractNode[] abstractNodes)
        {
            return this.Concat(new ChildNodes(abstractNodes));
        }
    

        public static ChildNodes Empty { get; set; }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ChildNode>)this).GetEnumerator();

        IEnumerator<ChildNode> IEnumerable<ChildNode>.GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }

        public bool HasChild(UrlPart urlPart) => this.Items.ContainsKey(urlPart);

        public ChildNode GetChild(UrlPart part)
        {
            ChildNode child = default(ChildNode);
            if (Items.TryGetValue(part, out child)) return child;
            return null;
        }

        public ChildNodes Concat<T>(IEnumerable<T> childNodes) where T : AbstractNode
        {
            return this.Concat(new ChildNodes(childNodes));
        }

        public ChildNodes Concat(ChildNodes childNodes)
        {
            return new ChildNodes(this.Items.Values.Concat(childNodes.Items.Values));
        }

        public static explicit operator ChildNodes(List<AbstractNode> enumerableNodes)
        {
            return new ChildNodes(enumerableNodes);
        }
    }
 
}