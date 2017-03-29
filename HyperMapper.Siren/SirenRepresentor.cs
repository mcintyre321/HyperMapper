using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HyperMapper.RepresentationModel;
using HyperMapper.RequestHandling;
using HyperMapper.ResourceModel;
using HyperMapper.Vocab;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OneOf;
using SirenDotNet;
using Action = SirenDotNet.Action;

namespace HyperMapper.Siren
{
    public class SirenHtmlRepresentor : Representor<SemanticDocument>
    {
        public override Tuple<string, string> GetResponse(SemanticDocument hypermediaObject, FindUriForTerm termUriFinder)
        {

            var sirenRep = new SirenRepresentor();
            var response = sirenRep.GetResponse(hypermediaObject, termUriFinder);
            var index = new HyperMapper.Siren.Index() { Model = JToken.Parse(response.Item2) };
            var transformText = index.TransformText();
            return Tuple.Create("text/html", transformText);
        }

        public override IEnumerable<string> AcceptTypes { get; } = new[] { "text/html" };
    }

    public class SirenRepresentor : Representor<SemanticDocument>
    {

        public SirenRepresentor()
        {
            JsonSerializerSettings = GetJsonSerializerSettings();
        }

        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            };
            settings.Converters.Add(new StringEnumConverter {CamelCaseText = true});
            return settings;
        }

        

        public override Tuple<string, string> GetResponse(SemanticDocument hypermediaObject, FindUriForTerm termUriFinder)
        {
            var responseEntity = this.BuildFromSemanticDocument(hypermediaObject, termUriFinder);
            var serializerSettings = this.JsonSerializerSettings;

            var serializer = JsonSerializer.Create(serializerSettings);
            var objectAsJson = JToken.FromObject(responseEntity, serializer);

            return Tuple.Create("application/vnd.siren+json", objectAsJson.ToString(Formatting.Indented));
        }

        public override IEnumerable<string> AcceptTypes { get; } = new[] {"application/vnd.siren+json"};

        public SirenDotNet.Entity BuildFromSemanticDocument(SemanticDocument semanticDocument, FindUriForTerm uriTermFinder)
        {
            var links = new List<SirenDotNet.Link>();
            var actions = new List<Action>();

            var subEntities = new List<SubEntity>();

            var jo = new JObject();

            string sirenTitle = null;
            foreach (var property in semanticDocument.Value)
            {
                property.Value.Switch(set =>
                {
                    if (property.Term.Means(TermFactory.From<Operation>()))
                    {
                        Func<SemanticProperty, Field> buildField = mp =>
                        {
                            var fieldKind = mp.Value.AsT0[TermFactory.From<FieldKind>()].Value.AsT1.Value<string>();

                            var sirenFieldType = new SirenDotNet.FieldTypes(fieldKind);

                            //if (mp.Type.IsT2)
                            //{
                            //    field.ExtensionData["options"] =
                            //        mp.Type.AsT2.Options.Select(o => new { name = o.Description, value = o.OptionId });
                            //}


                            var field = new Field()
                            {
                                Name = mp.Value.AsT0[TermFactory.From<FieldName>()].Value.AsT1.Value<string>(),
                                Type = sirenFieldType
                            };

                            return field;
                        };
                        ;
                        var href = new Uri(set[TermFactory.From<Operation.ActionUrl>()].Value.AsT1.Value<string>(), UriKind.Relative);
                        var action = new SirenDotNet.Action(uriTermFinder(property.Term).ToString(), href)
                        {
                            Method = HttpVerbs.POST,
                            Title = set[TermFactory.From<DisplayText>()].Value.AsT1.Value<string>(),
                            //TODO Fields = property.Parameters.Select(buildField),
                        };
                        actions.Add(action);
                    }
                   

                },
                value =>
                {
                    var isTitle = property.Term == Terms.Title;
                    if (isTitle)
                    {
                        sirenTitle = value.ToObject<string>();
                    }
                    else
                    {
                        jo[uriTermFinder(property.Term)] = value;
                    }
                },
                term => { },
                list =>
                {
                    if (property.Term == TermFactory.From<Vocab.Links>())
                    {
                        foreach (var value in list)
                        {
                            var set = value.AsT0;
                            var displayName = set[TermFactory.From<DisplayText>()].Value.AsT1.Value<string>();
                            Term rel = set[TermFactory.From<Vocab.Links.Rel>()].Value.AsT2;
                            var href = new Uri(set[TermFactory.From<Vocab.Links.Href>()].Value.AsT1.Value<string>(), UriKind.Relative);
                            var action = new SirenDotNet.Link(href, uriTermFinder(rel).ToString())
                            {
                                Title = displayName
                            };
                            links.Add(action);
                        }
                    }
                });
            }



            if (sirenTitle == null)
            {
                throw new Exception("Title cannot be null for siren, attach a Term.Title property");
            }

            var entity = new SirenDotNet.Entity
            {
                Title = sirenTitle,
                //Class = semanticDocument.Class?.Select(c => c.ToString()),
                Links = links.Any() ? links : null,
                Actions = actions.Any() ? actions : null,
                Entities = subEntities.Any() ? subEntities : null,
                Properties = jo.HasValues ? jo : null
            };
            return entity;
        }


        //private static Action MapAction(HyperModel.Operation operation)
        //{
        //    var item = new Action(operation.UrlPart.ToString(), operation.Href)
        //    {
        //        Method = (HttpVerbs) Enum.Parse(typeof (HttpVerbs), operation.Method, true),
        //        Title = operation.Title,
        //        Type = operation.ContentType,
        //        Fields = operation.Fields.Select(f => new Field(f.UrlPart.ToString())
        //        {
        //            _items = f._items,
        //            Type = FieldTypes.Text
        //        }),
        //        Href = operation.Href,
        //        Name = operation.UrlPart.ToString(),
        //    };
        //    return item;
    }
}