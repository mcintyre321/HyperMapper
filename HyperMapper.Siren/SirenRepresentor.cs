using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper.RepresentationModel;
using HyperMapper.RepresentationModel.Vocab;
using HyperMapper.RequestHandling;
using HyperMapper.ResourceModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OneOf;
using SirenDotNet;
using Action = SirenDotNet.Action;
using Link = HyperMapper.RepresentationModel.Vocab.Link;

namespace HyperMapper.Siren
{

    public class SirenRepresentor : Representor
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

        

        public override Tuple<string, string> GetResponse(Representation hypermediaObject)
        {
            var responseEntity = this.BuildFromHypermedia(hypermediaObject);
            var serializerSettings = this.JsonSerializerSettings;

            var serializer = JsonSerializer.Create(serializerSettings);
            var objectAsJson = JToken.FromObject(responseEntity, serializer);

            return Tuple.Create("application/vnd.siren+json", objectAsJson.ToString(Formatting.Indented));
        }

        public override IEnumerable<string> AcceptTypes { get; } = new[] {"application/vnd.siren+json"};

        public SirenDotNet.Entity BuildFromHypermedia(Representation representation)
        {
            var links = new List<SirenDotNet.Link>()
            {
                new SirenDotNet.Link(representation.Uri, "self")
            };

            var actions = new List<Action>();

            var subEntities = new List<SubEntity>();

            var properties = new JObject();

            string sirenTitle = null;

            {

                foreach (var pair in representation.Children)
                {
                    if (pair is Link)
                    {
                        var link = (Link) pair;
                        //if (link.Classes?.Contains("operation") ?? false)
                        //{
                        //    var operationResource = link.Follow();

                        //    var posttHandler = operationResource.GetMethodHandler(new Method.Post()).AsT0;

                        //    var action = new SirenDotNet.Action(link.Text, link.Uri)
                        //    {
                        //        Class = link.Classes,
                        //        Method = HttpVerbs.POST,
                        //        Fields = posttHandler.Parameters.Select(mp => new Field()
                        //        {
                        //            Name = mp.UrlPart.ToString(),
                        //            Type = LookupFieldType(mp.Type)
                        //        })
                        //    };
                        //    actions.Add(action);
                        //}
                        //else
                        {
                            links.Add(new SirenDotNet.Link(link.Uri, RelsFromTerm(link.Terms))
                            {
                                Title = link.Name,
                            });
                        }
                    }
                    else if (pair is Operation)
                    {
                        var operation = (Operation) pair;
                        {
                            var action = new SirenDotNet.Action(operation.Name, operation.Uri)
                            {
                                Method = HttpVerbs.POST,
                                Fields = operation.Parameters.Select(mp => new Field()
                                {
                                    Name = mp.UrlPart.ToString(),
                                    Type = LookupFieldType(mp.Type)
                                })
                            };
                            actions.Add(action);
                        }
                    }
                    else
                    {
                        var property = (ValueProperty) pair;
                        var isTitle = property.Terms.Contains(Term.Title);
                        if (isTitle)
                        {
                            sirenTitle = property.Value.ToObject<string>();
                        }
                        else
                        {
                            properties[property.Name.ToString()] = property.Value;
                        }
                    }
                }
            }


            if (sirenTitle == null)
            {
                throw new Exception("Title cannot be null for siren, attach a Term.Title property");
            }

            var entity = new SirenDotNet.Entity
            {
                Title = sirenTitle,
                //Class = representation.Class?.Select(c => c.ToString()),
                Links = links.Any() ? links : null,
                Actions = actions.Any() ? actions : null,
                Entities = subEntities.Any() ? subEntities : null,
                Properties = properties.HasValues ? properties : null
            };
            return entity;
        }

        private string[] RelsFromTerm(Term[] terms)
        {
            return terms.Select(t => t.ToString()).ToArray();
        }

        private FieldTypes LookupFieldType(MethodParameter.MethodParameterType type)
        {
            switch (type)
            {
                case MethodParameter.MethodParameterType.Text:
                    return FieldTypes.Text;
                case MethodParameter.MethodParameterType.Password:
                    return FieldTypes.Password;
                default:
                    throw new NotImplementedException();
            }
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
        //            Value = f.Value,
        //            Type = FieldTypes.Text
        //        }),
        //        Href = operation.Href,
        //        Name = operation.UrlPart.ToString(),
        //    };
        //    return item;
    }
}