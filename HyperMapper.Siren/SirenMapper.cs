using System;
using System.Collections.Generic;
using System.Linq;
using HyperMapper.HyperModel;
using Newtonsoft.Json.Linq;
using OneOf;
using SirenDotNet;
using Action = SirenDotNet.Action;
using Link = HyperMapper.HyperModel.Link;

namespace HyperMapper.Siren
{
    public class SirenMapper 
    {
        public SirenDotNet.Entity BuildFromHypermedia(Resource resource)
        {
            var entity = new SirenDotNet.Entity
            {
                Class = resource.Class,
                Links = new List<SirenDotNet.Link>()
                {
                    new SirenDotNet.Link(resource.Uri, "self")
                }
            };
            List<SubEntity> subEntities = new List<SubEntity>();
            List<SirenDotNet.Action> actions = new List<Action>();
            JObject properties = new JObject();
            //{
            //    {"key", resource.Uri?.ToString()  ?? "root"}
            //};
            foreach (var pair in resource.Children)
            {
                pair.Match(subEntity =>
                {
                    subEntities.Add(BuildSubEntityFromHypermedia(subEntity));
                },
                subEntityRef =>
                {
                    var item = MapSubEntityLink(subEntityRef);
                    subEntities.Add(item);
                }, action =>
                {
                    var item = MapAction(action);
                    actions.Add(item);
                }, link =>
                {
                    
                }, property =>
                {
                    properties[property.Name.ToString()] = property.Value;
                });
                entity.Actions = actions;
                entity.Entities = subEntities;
                entity.Properties = properties;
            }
            return entity;
        }

        private SirenDotNet.SubEntity BuildSubEntityFromHypermedia(Resource subResource)
        {
            var entity = new SirenDotNet.SubEntity.Embedded()
            {
                Class = subResource.Class,
                Links = new List<SirenDotNet.Link>()
                {
                    new SirenDotNet.Link(subResource.Uri, "self")
                }
            };
            List<SubEntity> subEntities = new List<SubEntity>();
            List<SirenDotNet.Action> actions = new List<Action>();
            List<SirenDotNet.Action> actions = new List<SirenDotNet.Link>();

            var properties = new JObject();
            //{
            //    {"key", subResource.Uri?.ToString()  ?? "root"}
            //};
            foreach (var pair in subResource.Children)
            {
                pair.Match(action =>
                {
                    var item = MapAction(action);
                    actions.Add(item);
                }, link =>
                {
                    if (link.Follow != null)
                    {

                    }
                    else
                    {
                        
                    }
                }, property =>
                {
                    properties[property.Name.ToString()] = property.Value;
                });
                entity.Actions = actions.Any() ?actions : null;
                entity.Entities = subEntities.Any() ? subEntities : null;
                entity.Properties = properties.HasValues ? properties : null;
            }
            return entity;
        }

        private SirenDotNet.SubEntity.Linked MapSubEntityLink(Link representation)
        {
            return new SubEntity.Linked()
            {
                Rel = representation.Rels.Select(r => r.ToString()),
                Title = representation.Text,
                Class = representation.Classes,
                Href = representation.Uri
            };
        }

        private static Action MapAction(HyperModel.Operation operation)
        {
            var item = new Action(operation.Key.ToString(), operation.Href)
            {
                Method = (HttpVerbs) Enum.Parse(typeof (HttpVerbs), operation.Method, true),
                Title = operation.Title,
                Type = operation.ContentType,
                Fields = operation.Fields.Select(f => new Field(f.Key.ToString())
                {
                    Value = f.Value,
                    Type = FieldTypes.Text
                }),
                Href = operation.Href,
                Name = operation.Key.ToString(),
            };
            return item;
        }
    }
}