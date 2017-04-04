using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperMapper.RepresentationModel;
using HyperMapper.Siren;

namespace HyperMapper.SirenServerSideRepresentor
{
    public class SirenServerSideHtmlRepresentor : Representor<SemanticDocument>
    {
        public override Task<Tuple<string, string>> GetResponse(SemanticDocument hypermediaObject, FindUriForTerm termUriFinder)
        {
            var sirenRepresentor = new SirenRepresentor();
            var sirenDoc = sirenRepresentor.BuildFromSemanticDocument(hypermediaObject, termUriFinder);
            var html = SirenToHtmlConverter.ReadSirenAndConvertToForm(sirenDoc);
            return Task.FromResult(Tuple.Create("text/html", html));

        }

        public override IEnumerable<string> AcceptTypes => new[] {"text/html"};
    }
}
