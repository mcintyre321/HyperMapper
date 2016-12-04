using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HyperMapper.RepresentationModel
{
    public abstract class Property
    {
        public string Name { get; }
        public Term[] Terms { get; set; }

        public Property(string name, Term[] terms)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (terms == null) throw new ArgumentNullException(nameof(terms));
            Name = name;
            Terms = terms;
        }
        
    }
}
