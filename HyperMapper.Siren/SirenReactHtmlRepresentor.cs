using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperMapper.RepresentationModel;
using Newtonsoft.Json.Linq;

namespace HyperMapper.Siren
{
    public class SirenReactHtmlRepresentor : Representor<SemanticDocument>
    {
        public override async Task<Tuple<string, string>> GetResponse(SemanticDocument hypermediaObject, FindUriForTerm termUriFinder)
        {

            var sirenRep = new SirenRepresentor();
            var response = await sirenRep.GetResponse(hypermediaObject, termUriFinder);
            var index = new HyperMapper.Siren.Index() { Model = JToken.Parse(response.Item2) };
            var transformText = index.TransformText();
            return Tuple.Create("text/html", transformText);
        }

        public override IEnumerable<string> AcceptTypes { get; } = new[] { "text/html" };
    }
}