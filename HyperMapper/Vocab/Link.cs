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

            var properties = new SemanticPropertiesSet();
            properties.AddValue<Href>((JToken)parentUri.ToString());
            properties.AddValue<DisplayText>((JToken)parentTitle);
            properties.AddValue<Rel>(term);
            return properties;
        }
        public class Href { }
        public class Rel { }

    }


}