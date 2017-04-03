using System;
using System.Collections.Generic;
using System.Linq;
using HyperMapper.Mapping;
using HyperMapper.Vocab;

namespace HyperMapper.Mapper
{
    internal class TermNode : AbstractNode
    {
        private GlossaryNode glossaryNode;
        private Term term;

        public TermNode(GlossaryNode glossaryNode, Term term)
        {
            this.glossaryNode = glossaryNode;
            this.term = term;
        }


        public override AbstractNode Parent => glossaryNode;

        public override Term Term => TermFactory.From<TermNode>();

        public override string Title => term.Title;

        public override Uri Uri => UriHelper.Combine(glossaryNode.Uri, UrlPart.ToString());

        public override UrlPart UrlPart => term.UrlPart;


    }
}