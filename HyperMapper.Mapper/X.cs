using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HyperMapper.Helpers;
using HyperMapper.Mapping;
using HyperMapper.Model;
using Newtonsoft.Json.Linq;
using OneOf;

namespace HyperMapper.RequestHandling
{
    public class X
    {
        public class MarkedUpProperty
        {
            public PropertyInfo propertyInfo { get; set; }
            public ExposeAttribute att { get; set; }
            public Uri propertyUri { get; set; }
        }

        public static Resource MakeResourceFromNode(INode node, Uri nodeUri, Func<Type, object> serviceLocator)
        {

            var type = node.GetType().GetTypeInfo();

            //var linkedChildResources = GetLinksFromTypeProperties(node, nodeUri, serviceLocator, type);
            var linkedActionResources = X.GetLinksFromTypeMethods(type, nodeUri, node, serviceLocator);


            var childLinks =
                node.ChildKeys.Select(node.GetChild)
                    .Select(
                        c => new Link(c.Key.ToString(), new Rel[] {new Rel("child"),}, new Uri(nodeUri.ToString().TrimEnd('/') + "/" + c.Key, UriKind.Relative))
                        {
                            Follow = () => MakeResourceFromNode(c, new Uri(nodeUri.ToString().TrimEnd('/') + "/" + c.Key, UriKind.Relative), serviceLocator)
                        });

            var links = //linkedChildResources.Concat
                (linkedActionResources).Concat(childLinks);


            var methodHandlers = new[]
            {
                new MethodHandler(new Method.Get(), new MethodParameter[0], arguments =>
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
 
        private static IEnumerable<Link> GetLinksFromTypeProperties(INode node, Uri nodeUri,
            Func<Type, object> serviceLocator,
            TypeInfo type)
        {
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
                            Follow = () => MakeResourceFromNode(value, x.propertyUri, serviceLocator)
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


        static IEnumerable<Link> GetLinksFromTypeMethods(TypeInfo type, Uri nodeUri, object node, Func<Type, object> serviceLocator)
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
                    Follow = () => BuildFromMethodInfo(node, pair.methodUri, pair.methodInfo, serviceLocator),
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

        static Resource BuildFromMethodInfo(object o, Uri uri, MethodInfo methodInfo, Func<Type, object> serviceLocator)
        {

            var methodHandlers = new List<MethodHandler>();

          
            var getHandler = X.BuildGetHandlerForMethodInfo(o, uri, methodInfo, serviceLocator);
            methodHandlers.Add(getHandler);
            var postHandler = MakePostMethodHandler(o, uri, methodInfo, serviceLocator);
            methodHandlers.Add(postHandler);
            var resource = new Resource(uri, new string[0], new List<Link>(), methodHandlers);

            return resource;
        }

        private static MethodHandler BuildGetHandlerForMethodInfo(object o, Uri uri, MethodInfo methodInfo, Func<Type, object> serviceLocator)
        {
            return new MethodHandler(new Method.Get(), new MethodParameter[0], tuples =>
            {
                var oneOfs = new List<OneOf<Link, Property>>();

                var actionFields =
                    methodInfo.GetParameters()
                        .Where(pi => pi.GetCustomAttributes<InjectAttribute>().Any() == false)
                        .Select(pi => new MethodParameter(pi.Name, BuildFromParameterInfo(pi)));

                foreach (var actionField in actionFields)
                {
                    oneOfs.Add(new Property(actionField.Key.ToString(), JObject.FromObject(actionField)));
                }

                var representation = new Representation(new string[] { "operation" }, uri, oneOfs );


                return Task.FromResult<InvokeResult>(new InvokeResult.RepresentationResult(representation));
            });
        }

        private static MethodParameter.MethodParameterType BuildFromParameterInfo(ParameterInfo pi)
        {
            return MethodParameter.MethodParameterType.Text;
        }

        private static MethodHandler MakePostMethodHandler(object o, Uri uri, MethodInfo methodInfo, Func<Type, object> serviceLocator)
        {
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

            var parameterInfo = methodInfo.GetParameters()
                .Where(mi => mi.GetCustomAttribute<InjectAttribute>() == null)
                .Select(pi => new MethodParameter(pi.Name, MethodParameter.MethodParameterType.Text)).ToArray();


            return new MethodHandler(new Method.Post(), parameterInfo, invoke);
        }
    }
}