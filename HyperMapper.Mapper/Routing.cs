using System;
using System.Linq;
using HyperMapper.Mapper;
using HyperMapper.RequestHandling;
using HyperMapper.ResourceModel;
using OneOf;
using OneOf.Types;

namespace HyperMapper.Mapping
{
    public class Routing
    {
        public static Func<string, OneOf<INode, None>> RouteByWalkingNode(INode root)
        {
            return path =>
            {
                var parts = path.Split('/')
                    .Where(p => !String.IsNullOrEmpty(p));

                return parts
                    .Aggregate((OneOf<INode, None>) root,
                        (node, part) => node.Match<OneOf<INode, None>>(
                            childNode =>
                            {
                                var child = childNode.GetChild(part);
                                if (child == null) return new None();
                                return child;
                            },
                            none => none)
                    );
                
            };
        }



        //public OneOf<Resource, OneOf.Types.None> GetChildByUriSegment(string part)
        //{
        //    foreach (var child in Children)
        //    {
        //        if (child.Follow != null && child.Uri.ToString() == this.Uri.ToString().TrimEnd('/') + "/" + part)
        //            return child.Follow();
        //    }

        //    return new OneOf.Types.None();
        //}

        public static Router RouteFromRootNode(RootNode root, Func<Type, object> serviceLocator)
        {
            return url =>
            {
                var nodeRouter = Routing.RouteByWalkingNode(root);
                return nodeRouter(url).Match<OneOf.OneOf<Resource, None>>(
                    node => Functions.MakeResourceFromNode(node, serviceLocator), none => none);
            };
        }
    }
}