using System;
using System.Collections.Generic;
using System.Linq;
using HyperMapper.HyperModel;
using Newtonsoft.Json.Linq;
using OneOf;
using SirenSharp;
using Action = SirenSharp.Action;
using Entity = HyperMapper.HyperModel.Entity;
using Link = HyperMapper.HyperModel.Link;

namespace HyperMapper.Siren
{
    public class SirenMapper 
    {
        public SirenSharp.Entity BuildFromHypermedia(Entity resource)
        {
            var entity = new SirenSharp.Entity
            {
                Class = resource.Class,
                Links = new List<SirenSharp.Link>()
                {
                    new SirenSharp.Link(resource.Uri, "self")
                }
            };
            List<OneOf<SubEntity, SubEntityLink>> subEntities = new List<OneOf<SubEntity, SubEntityLink>>();
            List<SirenSharp.Action> actions = new List<Action>();
            Dictionary<string, JToken> properties = new Dictionary<string, JToken>()
            {
                {"key", resource.Uri?.ToString()  ?? "root"}
            };
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

        private SirenSharp.SubEntity BuildSubEntityFromHypermedia(Entity subEntity)
        {
            var entity = new SirenSharp.SubEntity
            {
                Class = subEntity.Class,
                Links = new List<SirenSharp.Link>()
                {
                    new SirenSharp.Link(subEntity.Uri, "self")
                }
            };
            List<OneOf<SubEntity, SubEntityLink>> subEntities = new List<OneOf<SubEntity, SubEntityLink>>();
            List<SirenSharp.Action> actions = new List<Action>();
            Dictionary<string, JToken> properties = new Dictionary<string, JToken>()
            {
                {"key", subEntity.Uri?.ToString()  ?? "root"}
            };
            foreach (var pair in subEntity.Children)
            {
                pair.Match(se =>
                {
                    subEntities.Add(BuildSubEntityFromHypermedia(se));
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

        private SirenSharp.SubEntityLink MapSubEntityLink(SubEntityRef representation)
        {
                return new SirenSharp.SubEntityLink()
            {
                Rel = representation.Rels.Select(r => r.ToString()),
                Title = representation.Title,
                Class = representation.Classes,
                Href = representation.Uri
            };
        }

        private static Action MapAction(HyperModel.Action action)
        {
            var item = new Action(action.Key.ToString(), action.Href)
            {
                Method = (HttpVerbs) Enum.Parse(typeof (HttpVerbs), action.Method, true),
                Title = action.Title,
                Type = action.ContentType,
                Fields = action.Fields.Select(f => new Field(f.Key.ToString())
                {
                    Value = f.Value,
                    Type = FieldTypes.Text
                }),
                Href = action.Href,
                Name = action.Key.ToString(),
            };
            return item;
        }
    }
}