using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper.HyperModel;
using HyperMapper.RequestHandling;
using Newtonsoft.Json.Linq;
using OneOf;
using SirenDotNet;
using Action = SirenDotNet.Action;
using Link = HyperMapper.HyperModel.Link;

namespace HyperMapper.Siren
{
    public class SirenMapper 
    {
        public SirenDotNet.Entity BuildFromHypermedia(Representation representation)
        {
            var links = new List<SirenDotNet.Link>()
            {
                new SirenDotNet.Link(representation.Uri, "self")
            };

            var actions = new List<Action>();

            var subEntities = new List<SubEntity>();

            var properties = new JObject();

            
            foreach (var pair in representation.Children)
            {
                pair.Switch(
                //    subEntity =>
                //{
                //    subEntities.Add(BuildSubEntityFromHypermedia(subEntity));
                //},
                //subEntityRef =>
                //{
                //    var item = MapSubEntityLink(subEntityRef);
                //    subEntities.Add(item);
                //}, 
                //action =>
                //{
                //    var item = MapAction(action);
                //    actions.Add(item);
                //}, 
                    link =>
                    {
                        links.Add(new SirenDotNet.Link(link.Uri, link.Rels.Select(r => r.ToString()).ToArray())
                        {
                            Title = link.Text,
                        });
                    },
                property =>
                {
                    properties[property.Name.ToString()] = property.Value;
                });
            }

            var entity = new SirenDotNet.Entity
            {
                Class = representation.Class.Any() ? representation.Class : null,
                Links = links.Any() ? links : null,
                Actions = actions.Any() ? actions:null,
                Entities = subEntities.Any() ? subEntities:null,
                Properties = properties.HasValues ? properties : null
            };
            return entity;
        }
        

        //private static Action MapAction(HyperModel.Operation operation)
        //{
        //    var item = new Action(operation.Key.ToString(), operation.Href)
        //    {
        //        Method = (HttpVerbs) Enum.Parse(typeof (HttpVerbs), operation.Method, true),
        //        Title = operation.Title,
        //        Type = operation.ContentType,
        //        Fields = operation.Fields.Select(f => new Field(f.Key.ToString())
        //        {
        //            Value = f.Value,
        //            Type = FieldTypes.Text
        //        }),
        //        Href = operation.Href,
        //        Name = operation.Key.ToString(),
        //    };
        //    return item;
        //}
    }
}