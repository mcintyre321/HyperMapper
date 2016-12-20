using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HyperMapper.Helpers;
using HyperMapper.RepresentationModel;
using HyperMapper.RepresentationModel.Vocab;
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

        public static Resource MakeResourceFromNode(INode node, ServiceLocatorDelegate serviceLocator)
        {
            var methodNode = node as MethodInfoNode;

            if (methodNode != null)
            {
                var resource = new Resource(new[]
                {
                    BuildGetHandlerForMethodInfo(methodNode),
                    BuildPostHandlerForMethodInfo(methodNode, serviceLocator)
                });
                return resource;
            }
            else
            {
                var type = node.GetType().GetTypeInfo();
                var nodeUri = node.Uri;
                //var linkedChildResources = GetLinksFromTypeProperties(node, nodeUri, serviceLocator, type);
                //var linkedActionResources = Functions.GetLinksFromTypeMethods(type, nodeUri, node, serviceLocator,
                //    node);

                var childLinks =
                    node.ChildKeys.Select(key => node.GetChild(key))
                        .Select(
                            c =>
                            {
                                var uri = UriHelper.Combine(nodeUri, c.UrlPart.ToString());
                                var term = Term.Child;
                                return new Link(c.Title.ToString(), uri, term)
                                {
                                    Follow = () => MakeResourceFromNode(c, serviceLocator)
                                };
                            });

                var links = childLinks;

                if (node.Parent != null)
                {
                    links = links.Concat(new[]
                    {
                        new Link(node.Parent.Title, node.Parent.Uri, Term.Parent)
                    });
                }


                var methodHandlers = new[]
                {
                    new MethodHandler(new Method.Get(), new MethodParameter[0], arguments =>
                    {
                        var oneOfs = new HashSet<Property> ();
                        foreach (var link in links)
                        {
                            oneOfs.Add(link);
                        }

                        var properties = MarkedUpProperties(nodeUri, type)
                            .Where(IsSimpleProperty)
                            .Select(
                                x =>
                                    new ValueProperty(x.propertyInfo.Name,
                                        JToken.FromObject(x.propertyInfo.GetValue(node)),
                                        TermFactory.From(x.propertyInfo)));

                        foreach (var property in properties)
                        {
                            oneOfs.Add(property);
                        }
                        oneOfs.Add(new ValueProperty("title", JToken.FromObject(node.Title), Term.Title));

                        var representation = new Representation(nodeUri, oneOfs);

                        return Task.FromResult<InvokeResult>(new InvokeResult.RepresentationResult(representation));
                    })
                };



                var entity = new Resource(methodHandlers.ToArray());

                return entity;
            }
        }

        private static IEnumerable<Link> GetLinksFromTypeProperties(INode nodeAndUri,
            ServiceLocatorDelegate serviceLocator,
            TypeInfo type)
        {
            var nodeUri = nodeAndUri.Uri;
            var node = nodeAndUri;


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
                        //var rels = x.propertyInfo.GetCustomAttributes<RelAttribute>()
                        //    .Select(ra => Term(ra.RelString));

                        //if (IsChildUri(nodeUri, x.propertyUri))
                        //{
                        //    rels = rels.Append(new Term("child"));
                        //}

                        return new Link(x.propertyInfo.Name, x.propertyUri, TermFactory.From(x.propertyInfo))
                        {
                            Follow = () => MakeResourceFromNode(value, serviceLocator)
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
                    propertyUri = UriHelper.Combine(nodeUri, propertyInfo.Name)
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


        //static IEnumerable<Link> GetLinksFromTypeMethods(TypeInfo type, Uri nodeUri, object node, Func<Type, object> serviceLocator, INode parentNodeAndUri)
        //{
        //    return type.DeclaredMethods.Where(x => x.GetCustomAttributes<ExposeAttribute>().Any())
        //        .Select(methodInfo => new
        //        {
        //            methodInfo,
        //            methodUri = new Uri((nodeUri.ToString() + "/" + methodInfo.Name), UriKind.Relative)
        //        })
        //        .Select(pair => new Link(
        //            pair.methodInfo.Name,

        //            new Rel[] {new Rel("operation"), new Rel(pair.methodInfo.Name)},
        //            pair.methodUri)
        //        {
        //            Follow = () => BuildFromMethodInfo(node, pair.methodUri, pair.methodInfo, serviceLocator, parentNodeAndUri),
        //            Classes = new [] { "operation"}
        //        });
        //}

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

     

        private static MethodHandler BuildGetHandlerForMethodInfo(MethodInfoNode methodInfoNode)
        {
            return new MethodHandler(new Method.Get(), new MethodParameter[0], tuples =>
            {
                HashSet<Property>  oneOfs = BuildResourceElementsFromMethodInfo(methodInfoNode);
                oneOfs.Add(new ValueProperty("title", JToken.FromObject(methodInfoNode.Title), Term.Title));
                var representation = new Representation(methodInfoNode.Uri, oneOfs);
                var representationResult = new InvokeResult.RepresentationResult(representation);
                return Task.FromResult<InvokeResult>(representationResult);
            });
        }

        private static HashSet<Property>  BuildResourceElementsFromMethodInfo(MethodInfoNode methodInfoNode)
        {
            var oneOfs = new HashSet<Property>  ();
            oneOfs.Add(new Link(methodInfoNode.Parent.Title, methodInfoNode.Parent.Uri, Term.Parent));
            var methodParameters =
                methodInfoNode.MethodInfo.GetParameters()
                    .Where(pi => pi.GetCustomAttributes<InjectAttribute>().Any() == false)
                    .Select(pi => new MethodParameter(pi.Name, BuildFromParameterInfo(pi)));
            var term = TermFactory.From(methodInfoNode.MethodInfo);
            var operation = new Operation(methodInfoNode.Title, methodParameters, methodInfoNode.Uri, term);
            oneOfs.Add(operation);
            return oneOfs;
        }

        private static MethodParameter.MethodParameterType BuildFromParameterInfo(ParameterInfo pi)
        {
            return MethodParameter.MethodParameterType.Text;
        }

        private static MethodHandler BuildPostHandlerForMethodInfo(MethodInfoNode methodNode, ServiceLocatorDelegate serviceLocator)
        {
            MethodHandler.InvokeMethodDelegate invoke = async (submittedArgs) =>
            {
                var argsEnumerator = submittedArgs.GetEnumerator();
                var argsList = methodNode.MethodInfo.GetParameters()
                    .Select(pi =>
                    {
                        if (pi.GetCustomAttribute<InjectAttribute>() != null)
                        {
                            if (serviceLocator == null)
                            {
                                throw new InvalidOperationException($"Cannot [Inject] parameter {pi.Name} for {methodNode.MethodInfo.DeclaringType.Name}.{methodNode.MethodInfo.DeclaringType.Name} Please set ServiceLocator at startup");
                            }
                            return serviceLocator(pi.ParameterType);
                        }
                        else
                        {
                            argsEnumerator.MoveNext();
                            var current = argsEnumerator.Current;
                            if (current.Item1 != new UrlPart(pi.Name))
                                throw new ArgumentException("Mismatch: expected " + pi.Name + ", received" +
                                                            current.Item1.ToString());
                            return Tuple.Create(current.Item2, null as Action);
                        }
                    }).ToList();

                var parameters = argsList.Select(a => a.Item1).ToArray();
                var result = methodNode.MethodInfo.Invoke(methodNode.Parent, parameters);
                foreach (var tuple in argsList)
                {
                    tuple.Item2?.Invoke();
                }

                if (methodNode.MethodInfo.ReturnType == typeof(void))
                {
                }
                var task = result as Task;
                if (task != null)
                {
                    await task;
                    result = task.GetType().GetRuntimeProperty("Result")?.GetValue(task) ?? result;
                }

                var resourceElements = BuildResourceElementsFromMethodInfo(methodNode);
                var node = result as INode;
                if (node != null)
                {
                    resourceElements.Add(new Link($"Created \'{node.Title}\'", node.Uri, node.Term));
                }
                resourceElements.Add(new ValueProperty("title", JToken.FromObject(methodNode.Title), Term.Title));
                var representation = new Representation(methodNode.Uri, resourceElements);
                return new InvokeResult.RepresentationResult(representation);
            };

            var parameterInfo = methodNode.MethodInfo.GetParameters()
                .Where(mi => mi.GetCustomAttribute<InjectAttribute>() == null)
                .Select(pi => new MethodParameter(pi.Name, MethodParameter.MethodParameterType.Text)).ToArray();


            return new MethodHandler(new Method.Post(), parameterInfo, invoke);
        }
    }
}