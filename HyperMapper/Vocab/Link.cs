using System;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using Newtonsoft.Json.Linq;

namespace HyperMapper.Vocab
{
    public class Links
    {
        public static SemanticPropertiesSet CreateLink(string parentTitle, Uri parentUri, Term term)
        {

            var properties = new SemanticPropertiesSet
            {
                SemanticProperty.CreateValue<Href>((JToken) parentUri.ToString()),
                SemanticProperty.CreateValue<DisplayText>((JToken) parentTitle),
                SemanticProperty.CreateTerm<Rel>(term)
            };
            return properties;
        }
        public class Href { }
        public class Rel { }

    }


}