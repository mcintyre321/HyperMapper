using System;
using System.Collections.Generic;
using HyperMapper.RepresentationModel;
using System.Linq;
using HyperMapper.Mapping;
using HyperMapper.ResourceModel;
using HyperMapper.Vocab;

namespace HyperMapper.Mapper
{
    public class GlossaryNode : AbstractNode
    { 
        Dictionary<Term, AbstractNode> terms = new Dictionary<Term, AbstractNode>();
        AbstractNode parent;
        public GlossaryNode(AbstractNode parent)
        {
            this.parent = parent;
            this.terms.Add(TermFactory.From<GlossaryNode>(), new TermNode(this, TermFactory.From<GlossaryNode>()));
        }

     

        public override AbstractNode Parent => parent;

        public override Term Term => TermFactory.From<GlossaryNode>();

        public Uri GetUriForTerm(Term term)
        {
            AbstractNode node = null;
            if (!terms.TryGetValue(term, out node))
            {
                var termNode = new TermNode(this, term);
                terms.Add(term, termNode);
                node = termNode;
            }
            return node.Uri;
        }

        public override string Title => "Glossary";

        public override Uri Uri => UriHelper.Combine(parent.Uri, UrlPart.ToString());

        public override UrlPart UrlPart => new UrlPart("_glossary");

    }
}