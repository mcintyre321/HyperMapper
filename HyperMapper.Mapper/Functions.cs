using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HyperMapper.Helpers;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.RequestHandling;
using HyperMapper.ResourceModel;
using Newtonsoft.Json.Linq;
using OneOf;

namespace HyperMapper.Mapper
{
    public class Functions
    {
        public class MarkedUpProperty
        {
            public PropertyInfo propertyInfo { get; set; }
            public ExposeAttribute att { get; set; }
            public Uri propertyUri { get; set; }
        }

        public static Resource MakeResourceFromNode(Tuple<INode,Uri> nodeAndUri, Tuple<INode, Uri> parentNodeAndUri, Func<Type, object> serviceLocator)
        {

            var type = nodeAndUri.Item1.GetType().GetTypeInfo();
            var nodeUri = nodeAndUri.Item2;
            var node = nodeAndUri.Item1;
            //var linkedChildResources = GetLinksFromTypeProperties(node, nodeUri, serviceLocator, type);
            var linkedActionResources = Functions.GetLinksFromTypeMethods(type, nodeUri, node, serviceLocator, nodeAndUri);



            var childLinks =
                node.ChildKeys.Select(node.GetChild)
                    .Select(
                        c =>
                        {
                            var uri = new Uri(nodeUri.ToString().TrimEnd('/') + "/" + c.Key, UriKind.Relative);
                            return new Link(c.Title.ToString(), new Rel[] {new Rel("child"),}, uri)
                            {
                                Follow = () => MakeResourceFromNode(Tuple.Create(c, uri),nodeAndUri, serviceLocator)
                            };
                        });

            var links = //linkedChildResources.Concat
                (linkedActionResources).Concat(childLinks);

            if (node.Parent != null)
            {
                links = links.Concat(new[] {new Link("parent", new Rel[] {new Rel("parent"),}, parentNodeAndUri.Item2)
                });
            }


            var methodHandlers = new[]
            {
                new MethodHandler(new Method.Get(), new MethodParameter[0], arguments =>
                {
                    var oneOfs = new List<OneOf<Link, Property, Operation>>();
                    foreach (var link in links)
                    {
                        oneOfs.Add(link);
                    }

                    var properties = MarkedUpProperties(nodeUri, type)
                        .Where(IsSimpleProperty)
                        .Select(x => new Property(x.propertyInfo.Name, JToken.FromObject(x.propertyInfo.GetValue(node))));

                    foreach (var property in properties)
                    {
                        oneOfs.Add(property);
                    }

                    var representation = new Representation(new Class[0], node.Title, nodeUri, oneOfs);

                    return Task.FromResult<InvokeResult>(new InvokeResult.RepresentationResult(representation));
                })
            };



            var entity = new Resource(node.Title, nodeUri, GetClasses(type).ToArray(), links.ToArray(), methodHandlers.ToArray());

            return entity;
        }
 
        private static IEnumerable<Link> GetLinksFromTypeProperties(Tuple<INode, Uri> nodeAndUri,
            Func<Type, object> serviceLocator,
            TypeInfo type)
        {
            var nodeUri = nodeAndUri.Item2;
            var node = nodeAndUri.Item1;


            var linkedChildResources = MarkedUpProperties(nodeUri, type)
                .Where(IsLinkedResource)
                .Select(x =>
                {
                    //if (x.propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    //{
                    //    var itemsNode = new Node(node, x.propertyInfo.Name);
                    //    var items = x.propertyInfo.GetValue(node) as IEnumerable<object>;
                    //    var nodes = items.Select
                    //    foreach (var item in nodes)
                    //    {
                    //        itemsNode.AddChild(item);
                    //    }
                    //    return itemsNode;
                    //}
                    //else
                    {
                        var value = (INode) x.propertyInfo.GetValue(node);
                        var rels = x.propertyInfo.GetCustomAttributes<RelAttribute>()
                            .Select(ra => new Rel(ra.RelString));

                        if (IsChildUri(nodeUri, x.propertyUri))
                        {
                            rels = rels.Append(new Rel("child"));
                        }

                        return new Link(x.propertyInfo.Name, rels.ToArray(), x.propertyUri)
                        {
                            Follow = () => MakeResourceFromNode(Tuple.Create(value, x.propertyUri), nodeAndUri , serviceLocator)
                        };
                    }
                });
            return linkedChildResources;
        }

        private static IEnumerable<MarkedUpProperty> MarkedUpProperties(Uri nodeUri, TypeInfo type)
        {
            var markedUp = type.DeclaredProperties
                .Select(propertyInfo => new MarkedUpProperty
                {
                    propertyInfo = propertyInfo,
                    att = propertyInfo.GetCustomAttribute<ExposeAttribute>(),
                    propertyUri = new Uri((nodeUri.ToString().TrimEnd('/') + "/" + propertyInfo.Name), UriKind.Relative)
                }).Where(x => x.att != null);
            return markedUp;
        }

        private static bool IsSimpleProperty(MarkedUpProperty arg)
        {
            return arg.propertyInfo.PropertyType == typeof(string);
        }

        private static bool IsChildUri(Uri nodeUri, Uri childUri)
        {
            return childUri.ToString().StartsWith(nodeUri.ToString());
        }

        private static bool IsLinkedResource(MarkedUpProperty x)
        {
            return x.propertyInfo.PropertyType != typeof(string);
        }


        static IEnumerable<Link> GetLinksFromTypeMethods(TypeInfo type, Uri nodeUri, object node, Func<Type, object> serviceLocator, Tuple<INode, Uri> parentNodeAndUri)
        {
            return type.DeclaredMethods.Where(x => x.GetCustomAttributes<ExposeAttribute>().Any())
                .Select(methodInfo => new
                {
                    methodInfo,
                    methodUri = new Uri((nodeUri.ToString() + "/" + methodInfo.Name), UriKind.Relative)
                })
                .Select(pair => new Link(
                    pair.methodInfo.Name,

                    new Rel[] {new Rel("operation"), new Rel(pair.methodInfo.Name)},
                    pair.methodUri)
                {
                    Follow = () => BuildFromMethodInfo(node, pair.methodUri, pair.methodInfo, serviceLocator, parentNodeAndUri),
                    Classes = new [] { "operation"}
                });
        }

        private static string[] GetClasses(TypeInfo type)
        {
            return type
                .Recurse(t => IntrospectionExtensions.GetTypeInfo(t.BaseType))
                .TakeWhile(t => t.BaseType != null)
                .Where(
                    t =>
                        t.AsType() != typeof(object) &&
                        (t.GetCustomAttribute<HyperMapperAttribute>(false)?.UseTypeNameAsClassNameForEntity ?? true))
                .Select(t => t.Name).ToArray();
        }

        static Resource BuildFromMethodInfo(object o, Uri uri, MethodInfo methodInfo, Func<Type, object> serviceLocator, Tuple<INode, Uri> parentNodeAndUri)
        {
            var resource = new Resource(methodInfo.Name, uri, new string[0], new List<Link>(), new[]
            {
                BuildGetHandlerForMethodInfo(o, uri, methodInfo, parentNodeAndUri, serviceLocator),
                BuildPostHandlerForMethodInfo(o, uri, methodInfo, serviceLocator, parentNodeAndUri)
            });

            return resource;
        }

        private static MethodHandler BuildGetHandlerForMethodInfo(object o, Uri uri, MethodInfo methodInfo, Tuple<INode, Uri> parentNodeAndUri, Func<Type, object> serviceLocator)
        {
            return new MethodHandler(new Method.Get(), new MethodParameter[0], tuples =>
            {
                var oneOfs = BuildResourceElementsFromMethodInfo(methodInfo, uri, parentNodeAndUri);
                var representation = new Representation(new Class[0], methodInfo.Name, uri, oneOfs );
                var representationResult = new InvokeResult.RepresentationResult(representation);
                return Task.FromResult<InvokeResult>(representationResult);
            });
        }

        private static List<OneOf<Link, Property, Operation>> BuildResourceElementsFromMethodInfo(MethodInfo methodInfo, Uri uri, Tuple<INode, Uri> parentNodeAndUri)
        {
            var oneOfs = new List<OneOf<Link, Property, Operation>>();
            oneOfs.Add(new Link(parentNodeAndUri.Item1.Title, new[] { new Rel("parent") }, parentNodeAndUri.Item2));
            var methodParameters =
                methodInfo.GetParameters()
                    .Where(pi => pi.GetCustomAttributes<InjectAttribute>().Any() == false)
                    .Select(pi => new MethodParameter(pi.Name, BuildFromParameterInfo(pi)));
            var operation = new Operation(methodInfo.Name, methodParameters, uri);
            oneOfs.Add(operation);
            return oneOfs;
        }

        private static MethodParameter.MethodParameterType BuildFromParameterInfo(ParameterInfo pi)
        {
            return MethodParameter.MethodParameterType.Text;
        }

        private static MethodHandler BuildPostHandlerForMethodInfo(object o, Uri uri, MethodInfo methodInfo, Func<Type, object> serviceLocator, Tuple<INode, Uri> parentNodeAndUri)
        {
            MethodHandler.InvokeMethodDelegate invoke = async (submittedArgs) =>
            {
                var argsEnumerator = submittedArgs.GetEnumerator();
                var argsList = methodInfo.GetParameters()
                    .Select(pi =>
                    {
                        if (pi.GetCustomAttribute<InjectAttribute>() != null)
                        {
                            if (serviceLocator == null)
                            {
                                throw new InvalidOperationException($"Cannot [Inject] parameter {pi.Name} for {methodInfo.DeclaringType.Name}.{methodInfo.Name} Please set ServiceLocator at startup");
                            }
                            return serviceLocator(pi.ParameterType);
                        }
                        else
                        {
                            argsEnumerator.MoveNext();
                            var current = argsEnumerator.Current;
                            if (current.Item1 != new Key(pi.Name))
                                throw new ArgumentException("Mismatch: expected " + pi.Name + ", received" +
                                                            current.Item1.ToString());
                            return current.Item2;
                        }
                    });

                var result = methodInfo.Invoke(o, argsList.ToArray());
                if (methodInfo.ReturnType == typeof(void))
                {
                }
                var task = result as Task;
                if (task != null)
                {
                    await task;
                    result = task.GetType().GetRuntimeProperty("Result")?.GetValue(task) ?? result;
                }

                var resourceElements = BuildResourceElementsFromMethodInfo(methodInfo, uri, parentNodeAndUri);
                var node = result as INode;
                if (node != null)
                {
                    resourceElements.Add(new Link("Created " + node.Title, new Rel[0],  new Uri(parentNodeAndUri.Item2, "./" + node.Key)));
                }
                var representation = new Representation(new Class[0], methodInfo.Name, uri, resourceElements);
                return new InvokeResult.RepresentationResult(representation);
            };

            var parameterInfo = methodInfo.GetParameters()
                .Where(mi => mi.GetCustomAttribute<InjectAttribute>() == null)
                .Select(pi => new MethodParameter(pi.Name, MethodParameter.MethodParameterType.Text)).ToArray();


            return new MethodHandler(new Method.Post(), parameterInfo, invoke);
        }
    }
}