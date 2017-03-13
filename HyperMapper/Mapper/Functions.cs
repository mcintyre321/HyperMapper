using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.ResourceModel;
using HyperMapper.Vocab;
using Newtonsoft.Json.Linq;
using Link = HyperMapper.Vocab.Link;

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

        public static Resource<SemanticDocument> MakeResourceFromNode(AbstractNode node, ServiceLocatorDelegate serviceLocator)
        {
            var methodNode = node as MethodInfoNode;

            if (methodNode != null)
            {
                var resource = new Resource<SemanticDocument>(new[]
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
                //var linkedActionResources = Functions.GetLinksFromTypeMethods(type, nodeUri, node, serviceLocator,
                //    node);

                var childLinks =
                    node.ChildKeys.Select(node.GetChild)
                        .Select(
                            c =>
                            {
                                var uri = UriHelper.Combine(nodeUri, c.UrlPart.ToString());
                                var term = Terms.Child;
                                return Link.Create(c.Title.ToString(), uri, term);
                                {
                                    //Follow = () => MakeResourceFromNode(c, serviceLocator)
                                };
                            });

                var links = childLinks;

                if (node.Parent != null)
                {
                    links = links.Concat(new[]
                    {
                        Link.Create(node.Parent.Title, node.Parent.Uri, Terms.Parent)
                    });
                }


                var methodHandlers = new[]
                {
                    new MethodHandler<SemanticDocument>(new Method.Get(), new MethodParameter[0], arguments =>
                    {
                        var doc = new SemanticDocument();

                        
                        foreach (var link in links)
                        {
                            doc.Value.Add(link);
                        }

                        var markedUpProps = MarkedUpProperties(nodeUri, type)
                            .Where(IsSimpleProperty)
                            .Select(x => new SemanticProperty(TermFactory.From(x.propertyInfo), JToken.FromObject(x.propertyInfo.GetValue(node))));

                        foreach (var property in markedUpProps)
                        {
                            doc.Value.Add(property);
                        }

                        foreach (var hyperlink in (node as IHasHyperlinks)?.Hyperlinks ?? Enumerable.Empty<Tuple<Term, Uri, string>>())
                        {
                            var semanticProperty = Link.Create(hyperlink.Item3, hyperlink.Item2, hyperlink.Item1);
                            doc.Value.Add(semanticProperty);
                        }

                        doc.Value.Add(new SemanticProperty(Terms.Title, JToken.FromObject(node.Title)));


                        return Task.FromResult<InvokeResult<SemanticDocument>>(new InvokeResult<SemanticDocument>.RepresentationResult(doc));
                    })
                };



                var entity = new Resource<SemanticDocument>(methodHandlers.ToArray());

                return entity;
            }
        }

        private static IEnumerable<SemanticProperty> GetLinksFromTypeProperties(AbstractNode nodeAndUri,
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
                        var value = (AbstractNode) x.propertyInfo.GetValue(node);
                        //var rels = x.propertyInfo.GetCustomAttributes<RelAttribute>()
                        //    .Select(ra => Term(ra.RelString));

                        //if (IsChildUri(nodeUri, x.propertyUri))
                        //{
                        //    rels = rels.Append(new Term("child"));
                        //}

                        return Link.Create(x.propertyInfo.Name, x.propertyUri, TermFactory.From(x.propertyInfo));
                        {
                            //Follow = () => MakeResourceFromNode(value, serviceLocator)
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

        private static bool IsLinkedResource(MarkedUpProperty x)
        {
            return x.propertyInfo.PropertyType != typeof(string);
        }


        private static MethodHandler<SemanticDocument> BuildGetHandlerForMethodInfo(MethodInfoNode methodInfoNode)
        {
            return new MethodHandler<SemanticDocument>(new Method.Get(), new MethodParameter[0], tuples =>
            {
                var semanticProperties = BuildResourceElementsFromMethodInfo(methodInfoNode).ToList();
                semanticProperties.Add(new SemanticProperty(Terms.Title, JToken.FromObject(methodInfoNode.Title)));
                var objectProperty = new SemanticDocument();
                foreach (var semanticProperty in semanticProperties)
                {
                    objectProperty.Value.Add(semanticProperty);
                }

                var representationResult = new InvokeResult<SemanticDocument>.RepresentationResult(objectProperty);
                return Task.FromResult<InvokeResult<SemanticDocument>>(representationResult);
            });
        }

        private static IEnumerable<SemanticProperty> BuildResourceElementsFromMethodInfo(MethodInfoNode methodInfoNode)
        {
            yield return Link.Create(methodInfoNode.Parent.Title, methodInfoNode.Parent.Uri, Terms.Parent);
            var methodParameters = methodInfoNode.GetParameters();
            var term = TermFactory.From(methodInfoNode.MethodInfo);
            var operation = Operation.Create(methodInfoNode.Title, methodParameters, methodInfoNode.Uri, term);
            yield return operation;
        }

        private static MethodParameter.MethodParameterType BuildFromParameterInfo(ParameterInfo pi)
        {
            return new MethodParameter.MethodParameterType.Text();
        }

        private static MethodHandler<SemanticDocument> BuildPostHandlerForMethodInfo(MethodInfoNode methodNode,
            ServiceLocatorDelegate serviceLocator)
        {
            MethodHandler<SemanticDocument>.InvokeMethodDelegate invoke2 = async (submittedArgs) =>
            {
                var argsEnumerator = submittedArgs.GetEnumerator();
                var argsList = methodNode.MethodInfo.GetParameters()
                    .Select(pi =>
                    {
                        if (pi.GetCustomAttribute<InjectAttribute>() != null)
                        {
                            if (serviceLocator == null)
                            {
                                throw new InvalidOperationException(
                                    $"Cannot [Inject] parameter {pi.Name} for {methodNode.MethodInfo.DeclaringType.Name}.{methodNode.MethodInfo.DeclaringType.Name} Please set ServiceLocator at startup");
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
                var representation = new SemanticDocument();
                foreach (var semanticProperty in BuildResourceElementsFromMethodInfo(methodNode).ToList())
                {
                    representation.Value.Add(semanticProperty);
                }
                var node = result as AbstractNode;
                if (node != null)
                {
                    representation.Value.Add(Link.Create($"Created \'{node.Title}\'", node.Uri, node.Term));
                }
                representation.Value.Add(new SemanticProperty(Terms.Title, JToken.FromObject(methodNode.Title)));
                return new InvokeResult<SemanticDocument>.RepresentationResult(representation);
            };

            var parameterInfo = methodNode.GetParameters();

            return new MethodHandler<SemanticDocument>(new Method.Post(), parameterInfo, invoke2);
        }
    }
}