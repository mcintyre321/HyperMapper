using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HyperMapper.Helpers;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.ResourceModel;
using HyperMapper.Vocab;
using Newtonsoft.Json.Linq;

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

        public static Resource<SemanticDocument> MakeResourceFromNode(AbstractNode node,
            ServiceLocatorDelegate serviceLocator)
        {
            var methodNode = node as MethodInfoNode;
            if (methodNode != null)
                return new Resource<SemanticDocument>(new[]
                {
                    BuildGetHandlerForMethodInfo(methodNode),
                    BuildPostHandlerForMethodInfo(methodNode, serviceLocator)
                });

            var abstractNode = node;
            {
                var type = abstractNode.GetType().GetTypeInfo();
                var nodeUri = abstractNode.Uri;

               


                var methodHandlers = new[]
                {
                    new MethodHandler<SemanticDocument>(new Method.Get(), new MethodParameter[0], arguments =>
                    {
                        var doc = new SemanticDocument();

                        {
                            var childLinks =
                                node.ChildKeys.Select(node.GetChild)
                                    .Select(
                                        c =>
                                        {
                                            var uri = UriHelper.Combine(nodeUri, c.UrlPart.ToString());
                                            var term = Terms.Child;
                                            return Links.CreateLink(c.Title.ToString(), uri, term);
                                        });

                            var links2 = childLinks;

                            if (node.Parent != null)
                            {
                                doc.AddLink(Links.CreateLink(node.Parent.Title, node.Parent.Uri, Terms.Parent));
                            }
                            foreach (var link in links2)
                            {
                                doc.AddLink(link);
                            }
                        }
                        var operations = Functions.GetOperations(type, abstractNode, serviceLocator);
                        foreach (var linkedActionResource in operations)
                        {
                            doc.Value.Add(linkedActionResource);
                        }
                        var markedUpProps = MarkedUpProperties(nodeUri, type)
                            .Where(IsSimpleProperty)
                            .Select(x => SemanticProperty.CreateValue(TermFactory.From(x.propertyInfo), JToken.FromObject(x.propertyInfo.GetValue(abstractNode))));

                        foreach (var property in markedUpProps)
                        {
                            doc.Value.Add(property);
                        }

                        var hyperlinks = (abstractNode as IHasHyperlinks)?.Hyperlinks ?? Enumerable.Empty<Tuple<Term, Uri, string>>();
                        foreach (var hyperlink in hyperlinks)
                        {
                            doc.AddLink(Links.CreateLink(hyperlink.Item3, hyperlink.Item2, hyperlink.Item1));
                        }

                        doc.Value.Add(SemanticProperty.CreateValue(Terms.Title, JToken.FromObject(abstractNode.Title)));


                        return
                            Task.FromResult<InvokeResult<SemanticDocument>>(
                                new InvokeResult<SemanticDocument>.RepresentationResult(doc));
                    })
                };
                return new Resource<SemanticDocument>(methodHandlers.ToArray());
            }
        }

        //private static IEnumerable<SemanticProperty> GetLinksFromTypeProperties(AbstractNode nodeAndUri,
        //    ServiceLocatorDelegate serviceLocator,
        //    TypeInfo type)
        //{
        //    var nodeUri = nodeAndUri.Uri;
        //    var node = nodeAndUri;


        //    var linkedChildResources = MarkedUpProperties(nodeUri, type)
        //        .Where(IsLinkedResource)
        //        .Select(x =>
        //        {
        //            //if (x.propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        //            //{
        //            //    var itemsNode = new Node(node, x.propertyInfo.Name);
        //            //    var items = x.propertyInfo.GetValue(node) as IEnumerable<object>;
        //            //    var nodes = items.Select
        //            //    foreach (var item in nodes)
        //            //    {
        //            //        itemsNode.AddChild(item);
        //            //    }
        //            //    return itemsNode;
        //            //}
        //            //else
        //            {
        //                var value = (AbstractNode) x.propertyInfo.GetValue(node);
        //                //var rels = x.propertyInfo.GetCustomAttributes<RelAttribute>()
        //                //    .Select(ra => Term(ra.RelString));

        //                //if (IsChildUri(nodeUri, x.propertyUri))
        //                //{
        //                //    rels = rels.Append(new Term("child"));
        //                //}

        //                return Link.Create(x.propertyInfo.Name, x.propertyUri, TermFactory.From(x.propertyInfo));
        //                {
        //                    //Follow = () => MakeResourceFromNode(value, serviceLocator)
        //                };
        //            }
        //        });
        //    return linkedChildResources;
        //}

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
                
                var doc = new SemanticDocument();
                doc.Value.Add(SemanticProperty.CreateValue(Terms.Title, JToken.FromObject(methodInfoNode.Title)));
                AddSemanticItemsFromMethodInfo(methodInfoNode, doc);
                var representationResult = new InvokeResult<SemanticDocument>.RepresentationResult(doc);
                return Task.FromResult<InvokeResult<SemanticDocument>>(representationResult);
            });
        }


        private static void AddSemanticItemsFromMethodInfo(MethodInfoNode methodInfoNode, SemanticDocument representation)
        {

            var term = TermFactory.From(methodInfoNode.MethodInfo);
            var methodParameters = methodInfoNode.GetParameters();
            var operation = Operation.Create(methodInfoNode.Title, methodParameters, methodInfoNode.Uri, term);
            representation.Value.Add(operation);

            representation.AddLink(Links.CreateLink(methodInfoNode.Parent.Title, methodInfoNode.Parent.Uri, Terms.Parent));
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
                AddSemanticItemsFromMethodInfo(methodNode, representation);
                var node = result as AbstractNode;
                if (node != null)
                {
                    representation.AddLink(Links.CreateLink($"Created \'{node.Title}\'", node.Uri, node.Term));
                }
                representation.Value.Add(SemanticProperty.CreateValue(Terms.Title, JToken.FromObject(methodNode.Title)));
                return new InvokeResult<SemanticDocument>.RepresentationResult(representation);
            };

            var parameterInfo = methodNode.GetParameters();

            return new MethodHandler<SemanticDocument>(new Method.Post(), parameterInfo, invoke2);
        }

        static IEnumerable<SemanticProperty> GetOperations(TypeInfo type, AbstractNode node,
            ServiceLocatorDelegate serviceLocator)
        {
            return type.DeclaredMethods.Where(x => x.GetCustomAttributes<ExposeAttribute>().Any())
                .Select(mi => new MethodInfoNode(node, mi))
                .Select(min => Operation.Create(min.Title, min.GetParameters(), min.Uri, min.Term));
        }
    }

    internal static class FunctionHelpers
    {

        internal static SemanticProperty AddLink(this SemanticDocument doc, SemanticPropertiesSet link)
        {
            var linksRel = TermFactory.From<Links>();
            var links = doc.Value[linksRel] ?? (SemanticProperty.CreateList(linksRel, new SemanticPropertiesList())).Then(sp => doc.Value.Add(sp));
            var semanticPropertiesList = links.Value.AsT3;
            semanticPropertiesList.Add(link);
            return links;
        }
    }
}