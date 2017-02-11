using System;
using System.Collections.Generic;
using HyperMapper.RepresentationModel;
using System.Linq;

namespace HyperMapper.Mapper
{
    public class GlossaryNode : INode
    { 
        Dictionary<Term, INode> terms = new Dictionary<Term, INode>();
        INode parent;
        public GlossaryNode(INode parent)
        {
            this.parent = parent;
            this.terms.Add(TermFactory.From<GlossaryNode>(), new TermNode(this, TermFactory.From<GlossaryNode>()));
        }

        public override IEnumerable<UrlPart> ChildKeys
        {
            get
            {
                return terms.Select(t => new UrlPart(t.Key.UrlPart));
            }
        }

        public override INode Parent
        {
            get
            {
                return parent;
            }
        }

        public override Term Term
        {
            get
            {
                return TermFactory.From<GlossaryNode>();
            }
        }

        public Uri GetUriForTerm(Term term)
        {
            INode node = null;
            if (!terms.TryGetValue(term, out node))
            {
                terms.Add(term, new TermNode(this, term));
            }
            return node.Uri;
        }

        public override string Title
        {
            get
            {
                return "Glossary";
            }
        }

        public override Uri Uri
        {
            get
            {
                return UriHelper.Combine(parent.Uri, UrlPart.ToString());
            }
        }

        public override UrlPart UrlPart
        {
            get
            {
                return new UrlPart("_glossary");
            }
        }

        public override INode GetChild(UrlPart key)
        {
            return terms.Where(k => k.Key.UrlPart == key).Select(pair => pair.Value).SingleOrDefault();
        }

        public override bool HasChild(UrlPart urlPart)
        {
            return ChildKeys.Any(ck => ck == urlPart);
        }
    }
     

    internal class TermNode : INode
    {
        private GlossaryNode glossaryNode;
        private Term term;

        public TermNode(GlossaryNode glossaryNode, Term term)
        {
            this.glossaryNode = glossaryNode;
            this.term = term;
        }

        public override IEnumerable<UrlPart> ChildKeys => Enumerable.Empty<UrlPart>();

        public override INode Parent => glossaryNode;

        public override Term Term => TermFactory.From<TermNode>();

        public override string Title => term.Title;

        public override Uri Uri => UriHelper.Combine(glossaryNode.Uri, UrlPart.ToString());

        public override UrlPart UrlPart => term.UrlPart;

        public override INode GetChild(UrlPart key)
        {
            return null;
        }

        public override bool HasChild(UrlPart urlPart)
        {
            return false;
        }
    }
}