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

        

        public override Task<Tuple<string, string>> GetResponse(SemanticDocument hypermediaObject, FindUriForTerm termUriFinder)
        {
            var responseEntity = this.BuildFromSemanticDocument(hypermediaObject, termUriFinder);
            var serializerSettings = this.JsonSerializerSettings;

            var serializer = JsonSerializer.Create(serializerSettings);
            var objectAsJson = JToken.FromObject(responseEntity, serializer);

            return Task.FromResult(Tuple.Create("application/vnd.siren+json", objectAsJson.ToString(Formatting.Indented)));
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
                            var fieldSet = mp.Value.AsT0;
                            var fieldKind = fieldSet[TermFactory.From<Operation.FieldKind>()].Value.AsT2;

                            var field = new Field()
                            {
                                Name = fieldSet[TermFactory.From<FieldName>()].Value.AsT1.Value<string>(),
                            };
                            if (fieldKind == TermFactory.From<Operation.TextField>())
                            {
                                field.Type = SirenDotNet.FieldTypes.Text;
                            }
                            else if (fieldKind == TermFactory.From<Operation.PasswordField>())
                            {
                                field.Type = SirenDotNet.FieldTypes.Password;
                            }
                            else if (fieldKind == TermFactory.From<Operation.SelectField>())
                            {
                                field.Type = new FieldTypes("select");
                                var options = fieldSet[TermFactory.From<Operation.Options>()].Value.AsT3;

                                field.ExtensionData["options"] = options.Select(o => o.AsT0)
                                    .Select(
                                        o => new
                                        {
                                            name = o[TermFactory.From<DisplayText>()].Value.AsT1.Value<string>(),
                                            value = o[TermFactory.From<Operation.FieldValue>()].Value.AsT1.Value<string>()
                                        })
                                        .Select(JObject.FromObject).ToArray();
                            }


                            return field;
                        };
                        ;
                        var href = new Uri(set[TermFactory.From<Operation.ActionUrl>()].Value.AsT1.Value<string>(), UriKind.Relative);
                        var action = new SirenDotNet.Action(uriTermFinder(property.Term).ToString(), href)
                        {
                            Method = HttpVerbs.POST,
                            Title = set[TermFactory.From<DisplayText>()].Value.AsT1.Value<string>(),
                            Fields = set[TermFactory.From<Operation.Fields>()].Value.AsT0.Select(x => buildField(x)).ToArray()
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
                        jo[uriTermFinder(property.Term).ToString()] = value.ToString();
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