using System;
using System.Collections.Generic;
using System.Linq;
using HyperMapper.Models;
using Newtonsoft.Json.Linq;
using OneOf;
using SirenSharp;
using Action = SirenSharp.Action;
using Entity = HyperMapper.Models.Entity;
using Link = HyperMapper.Models.Link;

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
                {"key", resource.Key?.ToString() ?? "root"}
            };
            foreach (var pair in resource.Children)
            {
                pair.Value.Match(representation =>
                {
                    var item = MapSubEntityLink(representation);
                    subEntities.Add(item);
                }, action =>
                {
                    var item = MapAction(pair, action);
                    actions.Add(item);
                }, link =>
                {
                    
                }, jToken =>
                {
                    properties[pair.Key.ToString()] = jToken;
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

        private static Action MapAction(KeyValuePair<Key, OneOf<SubEntityRef, Models.Action, Link, JToken>> pair, HyperMapper.Models.Action action)
        {
            var item = new Action(pair.Key.ToString(), action.Href)
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
                Name = pair.Key.ToString(),
            };
            return item;
        }
    }
}