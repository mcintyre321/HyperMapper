using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HyperMapper.Mapper.Rules;
using HyperMapper.Mapping;
using HyperMapper.Mapping.ResultTypes;
using HyperMapper.RepresentationModel;
using HyperMapper.ResourceModel;
using HyperMapper.Vocab;
using Newtonsoft.Json.Linq;

namespace HyperMapper.Mapper
{
    public class Functions
    {
        public interface IAbstractNodeToSemanticDocumentMappingRule
        {
            void Apply(AbstractNode abstractNode, SemanticDocument doc);
        }

        public static Resource<SemanticDocument> MakeResourceFromNode(AbstractNode node, ServiceLocatorDelegate serviceLocator)
        {
            var rules = new List<IAbstractNodeToSemanticDocumentMappingRule>()
            {
                new TheNodeTitleWillBeAddedAsAProperty(),
                new ThereWillBeALinkToTheParentNodeInTheDocument(),
                new ImplementingIHasChildNodesWillExposeTheItemsAsNodeResources(),
                new ImplementingIHasHyperlinksWillAddALinkPropertyToDocument(),
                new MarkingAPropertyWithExposeAttributeWillAddAValuePropertyToDocument(),
                new MarkingAMethodWithExposeAttributeWillAddTheMethodAsAnActionResource(),
            };

            var methodNode = node as MethodInfoNode;
            if (methodNode != null)
            {
                return new Resource<SemanticDocument>(new[]
                {
                    BuildGetHandlerForMethodInfo(methodNode),
                    BuildPostHandlerForMethodInfo(methodNode, serviceLocator)
                });
            }

            var methodHandlers = new[]
            {
                new MethodHandler<SemanticDocument>(new Method.Get(), new MethodParameter[0], arguments =>
                {
                    var doc = new SemanticDocument();
                    foreach (var mappingRule in rules)
                    {
                        mappingRule.Apply(node, doc);
                    }
                    return Task.FromResult<InvokeResult<SemanticDocument>>(new InvokeResult<SemanticDocument>.RepresentationResult(doc));
                })
            };
            return new Resource<SemanticDocument>(methodHandlers.ToArray());
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


        private static void AddSemanticItemsFromMethodInfo(MethodInfoNode methodInfoNode,
            SemanticDocument representation)
        {

            var term = TermFactory.From(methodInfoNode.MethodInfo);
            var methodParameters = methodInfoNode.GetParameters();
            var operation = Operation.Create(methodInfoNode.Title, methodParameters, methodInfoNode.Uri, term);
            representation.Value.Add(operation);

            representation.AddLink(Links.CreateLink(methodInfoNode.Parent.Title, methodInfoNode.Parent.Uri, Terms.Parent));
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
                var invokeResult = methodNode.MethodInfo.Invoke(methodNode.Parent, parameters);
                foreach (var tuple in argsList)
                {
                    tuple.Item2?.Invoke();
                }

                if (methodNode.MethodInfo.ReturnType == typeof(void))
                {
                }
                var task = invokeResult as Task;
                if (task != null)
                {
                    await task;
                    invokeResult = task.GetType().GetRuntimeProperty("Result")?.GetValue(task) ?? invokeResult;
                }
                var representation = new SemanticDocument();


                //Add a link back to the thing that the action happened to 
                {
                    OneOf.OneOf<Modified, Created> result = default(OneOf.OneOf<Modified, Created>);
                    if (invokeResult is Created) result = (Created) invokeResult;
                    if (invokeResult is Modified) result = (Modified) invokeResult;
                    if (invokeResult is AbstractNode) result = new Created((AbstractNode) invokeResult);
                    result = new Modified(methodNode.Parent);

                    result.Switch(
                        modified => representation.AddLink(Links.CreateLink($"Modified \'{modified.Node.Title}\'",
                            modified.Node.Uri, modified.Node.Term)),
                        created => representation.AddLink(Links.CreateLink($"Created \'{created.Node.Title}\'",
                            created.Node.Uri, created.Node.Term)));
                }

                representation.Value.Add(SemanticProperty.CreateValue(Terms.Title, JToken.FromObject(methodNode.Title)));
                return new InvokeResult<SemanticDocument>.RepresentationResult(representation);
            };

            var parameterInfo = methodNode.GetParameters();

            return new MethodHandler<SemanticDocument>(new Method.Post(), parameterInfo, invoke2);
        }
    }
}