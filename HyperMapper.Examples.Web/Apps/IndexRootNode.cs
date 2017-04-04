using System;
using System.Collections.Generic;
using HyperMapper.Mapper;
using HyperMapper.Mapping;
using HyperMapper.Vocab;

namespace HyperMapper.Examples.Web.Apps
{
    internal class IndexRootNode : RootNode, HyperMapper.Mapping.IHasHyperlinks
    {
        private readonly IList<Tuple<Term, Uri, string>> _hyperlinks = new List<Tuple<Term, Uri, string>>();

        public IndexRootNode(string title, Uri uri) : base(title, uri, TermFactory.From<IndexRootNode>())
        {
        }

        public void AddLink(string text, Uri uri, Term term)
        {
            _hyperlinks.Add(Tuple.Create(term, uri, text));
        }

        public IEnumerable<Tuple<Term, Uri, string>> Hyperlinks => _hyperlinks;



        public sealed class App { private App() { } }
    }
}