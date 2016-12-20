using System;
using System.Linq;
using System.Reflection;
using HyperMapper.RepresentationModel.Vocab;

namespace HyperMapper.RepresentationModel
{
    public abstract class Property
    {
        public string Name { get; }
        public Term Term { get; set; }

        public Property(string name, Term term)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (term == null) throw new ArgumentNullException(nameof(term));
            Name = name;
            Term = term;
        }
        
    }
}
