using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HyperMapper.Helpers;
using HyperMapper.HyperModel;
using HyperMapper.Mapping;
using Newtonsoft.Json.Linq;
using OneOf;

namespace HyperMapper.RequestHandling
{
    public class RequestHandlerBuilder
    {
        public RequestHandler MakeRequestHandler(Uri baseUri, Func<INode> getRootNode, Func<Type, object> serviceLocator)
        {
            return async (request, modelBinder) =>
            {
                var rootNode = getRootNode();
                var requestUri = new Uri(request.Uri.PathAndQuery.ToString(), UriKind.Relative);
                var root = X.MakeResourceFromNode(rootNode, baseUri, requestUri, serviceLocator);

                var parts = requestUri.ToString().Substring(baseUri.ToString().Length).Split('/')
                    .Where(p => !String.IsNullOrEmpty(p));

                var target = parts
                    .Aggregate((OneOf<Resource, None>) root,
                        (x, part) => x.Match(
                            childEntity => childEntity.GetChildByUriSegment(part),
                            none => none)
                    );

                return await target.Match(
                    resource => resource.GetMethodHandler(request.Method)
                        .Match(
                            async handler =>
                            {

                                var bindResult = await modelBinder(handler.ArgumentInfo);
                                var response = await bindResult.Match<Task<Response>>(
                                    failed => Task.FromResult<Response>(new Response.ModelBindingFailedResponse()),
                                    async boundModel => (await handler.Invoke(boundModel)).Match(
                                        representation =>
                                            new Response.RepresentationResponse(representation.Representation)
                                        ));
                                return response;
                            },
                            none => Task.FromResult((Response) new Response.MethodNotAllowed())),
                    none => Task.FromResult((Response) new Response.NotFoundResponse()));
            };
        }
    }
    public class X
        {

   
        public class MarkedUpProperty
        {
            public PropertyInfo propertyInfo { get; set; }
            public ExposeAttribute att { get; set; }
            public Uri propertyUri { get; set; }
        }

        public static Resource MakeResourceFromNode(INode node, Uri nodeUri, Uri requestUri,
            Func<Type, object> serviceLocator)
        {

            var type = node.GetType().GetTypeInfo();

            var linkedChildResources = LinkedChildResources(node, nodeUri, requestUri, serviceLocator, type);
            var linkedOtherResources = LinkedOtherResources(nodeUri, requestUri, type);
            var linkedActionResources = X.GetActions(type, nodeUri, node, serviceLocator);

            var links = linkedChildResources
                .Concat(linkedOtherResources)
                .Concat(linkedActionResources);


            var methodHandlers = new[]
            {
                new MethodHandler("GET", new Tuple<Key, Type>[0], arguments =>
                {
                    var oneOfs = new List<OneOf<Link, Property>>();
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

                    var representation = new Representation(new string[0], nodeUri, oneOfs);
                          
                    return Task.FromResult<InvokeResult>(new InvokeResult.RepresentationResult(representation));
                })
            };



            var entity = new Resource(nodeUri, GetClasses(type).ToArray(), links.ToArray(), methodHandlers.ToArray());

            return entity;
        }

        private static IEnumerable<Link> LinkedOtherResources(Uri nodeUri, Uri requestUri, TypeInfo type)
        {
            var linkedOtherResources = MarkedUpProperties(nodeUri, type)
                .Where(IsLinkedResource)
                .Where(x => IsChildUri(requestUri, x))
                .Select(x =>
                {
                    return new Link(x.propertyInfo.Name, x.propertyInfo.GetCustomAttributes<RelAttribute>()
                        .Select(ra => new Rel(ra.RelString)).ToArray(), x.propertyUri
                        )
                    {
                        //Classes = GetClasses(x.propertyInfo.PropertyType.GetTypeInfo()),
                    };
                });
            return linkedOtherResources;
        }

        private static IEnumerable<Link> LinkedChildResources(INode node, Uri nodeUri, Uri requestUri, Func<Type, object> serviceLocator,
            TypeInfo type)
        {
            var linkedChildResources = MarkedUpProperties(nodeUri, type)
                .Where(IsLinkedResource)
                .Where(x => !IsChildUri(requestUri, x))
                .Select(x =>
                {
                    var value = (INode) x.propertyInfo.GetValue(node);
                    return new Link(x.propertyInfo.Name, x.propertyInfo.GetCustomAttributes<RelAttribute>()
                        .Select(ra => new Rel(ra.RelString))
                        .Append(new Rel("down")).ToArray(), x.propertyUri)
                    {
                        Follow = () => MakeResourceFromNode(value, x.propertyUri, requestUri, serviceLocator)
                    };
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

        private static bool IsChildUri(Uri requestUri, MarkedUpProperty x)
        {
            return x.propertyUri.ToString().StartsWith(requestUri.ToString());
        }

        private static bool IsLinkedResource(MarkedUpProperty x)
        {
            return x.propertyInfo.PropertyType != typeof(string);
        }

         
        static IEnumerable<Link> GetActions(TypeInfo type, Uri nodeUri, object node, Func<Type, object> serviceLocator)
        {
            return type.DeclaredMethods.Where(x => x.GetCustomAttributes<ExposeAttribute>().Any())
                .Select(methodInfo => new
                {
                    methodInfo,
                    methodUri = new Uri((nodeUri.ToString() + "/" + methodInfo.Name), UriKind.Relative)
                })
                .Select(pair => new Link(
                    pair.methodInfo.Name, 
                    new Rel[]{ new Rel("action"), new Rel(pair.methodInfo.Name)}, 
                    pair.methodUri)
                {
                    Follow = () => BuildFromMethodInfo(node, pair.methodUri, pair.methodInfo, serviceLocator)
                });
        }
        private static string[] GetClasses(TypeInfo type)
        {
            return type
                .Recurse(t => t.BaseType.GetTypeInfo())
                .TakeWhile(t => t.BaseType != null)
                .Where(
                    t =>
                        t.AsType() != typeof(object) &&
                        (t.GetCustomAttribute<HyperMapperAttribute>(false)?.UseTypeNameAsClassNameForEntity ?? true))
                .Select(t => t.Name).ToArray();
        }

        static Resource BuildFromMethodInfo(object o, Uri uri, MethodInfo methodInfo, Func<Type, object> serviceLocator)
        {
            //var actionFields =
            //    methodInfo.GetParameters()
            //        .Where(pi => pi.GetCustomAttributes<InjectAttribute>().Any() == false)
            //        .Select(pi => new ActionField(pi.Name, BuildFromParameterInfo(pi)));

            Func<IEnumerable<Tuple<Key, object>>, Task<InvokeResult>> invoke = async (submittedArgs) =>
            {
                var argsEnumerator = submittedArgs.GetEnumerator();
                var argsList = methodInfo.GetParameters()
                    .Select(pi =>
                    {
                        if (pi.GetCustomAttribute<InjectAttribute>() != null)
                        {
                            if (serviceLocator == null)
                            {
                                throw new InvalidOperationException(
                                    $"Cannot [Inject] parameter {pi.Name} for {methodInfo.DeclaringType.Name}.{methodInfo.Name} Please set ServiceLocator at startup");
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
                if (result is Task)
                {
                    await ((Task) result);
                }
                var representation = new Representation(new string[0], uri, new List<OneOf<Link, Property>>());
                return new InvokeResult.RepresentationResult(representation);
            };

            Tuple<Key, Type>[] argumentInfo = methodInfo.GetParameters()
                .Where(mi => mi.GetCustomAttribute<InjectAttribute>() == null)
                .Select(pi => Tuple.Create((Key) pi.Name, pi.ParameterType)).ToArray();


            var methodHandlers = new List<MethodHandler>();

            var postHandler = new MethodHandler("POST", argumentInfo, invoke);
            methodHandlers.Add(postHandler);
            var resource = new Resource(uri, new string[0], new List<Link>(), methodHandlers);

            return resource;
        }
        }
}