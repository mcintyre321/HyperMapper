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
using Action = HyperMapper.HyperModel.Action;

namespace HyperMapper.RequestHandling
{
    public class ModelBindingFailed { }
    public class BoundModel {
        public BoundModel(Tuple<Key, object>[] args)
        {
            Args = args;
        }

        public Tuple<Key, object>[] Args { get; }
    }
    public delegate Task<OneOf<ModelBindingFailed, BoundModel>> ModelBinder(Tuple<Key, Type>[] keys);
    public delegate Task<Entity> RequestHandler(Uri uri, bool isPost, ModelBinder readBody);

    
    public class RequestHandlerBuilder
    {
        public RequestHandler MakeRequestHandler(Uri baseUri, Func<INode> getRootNode, Func<Type, object> serviceLocator)
        {
            return async (requestAbsUri, isInvoke, modelBinder) =>
            {
                var rootNode = getRootNode();
                var requestUri = new Uri(requestAbsUri.PathAndQuery.ToString(), UriKind.Relative);
                var root = GetResource(rootNode, baseUri, requestUri, serviceLocator);
                
                    
                if (isInvoke)
                {
                    OneOf<Entity, Action, Entity.ChildNotFound> target = root.AsT0;
                    var parts = requestUri.ToString().Substring(baseUri.ToString().Length).Split('/');

                    foreach (var part in parts.Where(p => !String.IsNullOrEmpty(p) ))
                    {
                        target = target.Match(
                            childEntity => childEntity.GetChildByUriSegment(part),
                            childAction => {throw new NotImplementedException("Actions don't have children");},
                            childNotFound => { throw new NotImplementedException("Actions don't have children"); });
                    }
                    if (target.IsT0) throw new Exception("SHould have walked to an action..");

                    var targetAction = target.AsT1;
                    var modelBindResult = await modelBinder(targetAction.ArgumentInfo);
                    var args = modelBindResult.Match(
                        failed => { throw new NotImplementedException(); },
                        model => model);
                    var result = await targetAction.Invoke(args.Args);

                    return MakeActionEntity(targetAction);
                }

                return root.Match(entity => entity, MakeActionEntity);
            };
        }

        private Entity MakeActionEntity(Action action)
        {
            return new Entity(action.Href, new string[0], new List<OneOf<Entity, SubEntityRef, Action, Link, Property>>()
            {
                action
            });
        }


        private OneOf<Entity, Action> GetResource(INode node, Uri nodeUri, Uri requestUri,
            Func<Type, object> serviceLocator)
        {

            var properties = new List<OneOf<Entity, SubEntityRef, Action, Link, Property>>();
            var type = node.GetType().GetTypeInfo();

            var markedUpProperties = type.DeclaredProperties.Select(propertyInfo => new
            {
                propertyInfo,
                att = propertyInfo.GetCustomAttribute<ExposeAttribute>(),
                propertyUri = new Uri((nodeUri.ToString().TrimEnd('/') + "/" + propertyInfo.Name), UriKind.Relative)

            }).Where(x => x.att != null)
            .ToArray();

            markedUpProperties.Where(x => x.propertyInfo.PropertyType == typeof (string))
                .Select(x => new Property(x.propertyInfo.Name, JToken.FromObject(x.propertyInfo.GetValue(node))))
                .ToArray().ForEach(p => properties.Add(p));

            markedUpProperties
                .Where(x => x.propertyInfo.PropertyType != typeof (string))
                .Where(x => requestUri.ToString().StartsWith(x.propertyUri.ToString()))
                .Select(x =>
                {
                    var value = (INode) x.propertyInfo.GetValue(node);
                    return GetResource(value, x.propertyUri, requestUri, serviceLocator).AsT0;
                }).ToArray().ForEach(ent => properties.Add(ent));
 

                markedUpProperties
                .Where(x => x.propertyInfo.PropertyType != typeof (string))
                .Where(x => !requestUri.ToString().StartsWith(x.propertyUri.ToString()))
                    .Select(x =>
                    {
                        return new SubEntityRef()
                        {
                            Name = x.propertyInfo.Name,
                            Rels =
                                x.propertyInfo.GetCustomAttributes<RelAttribute>()
                                    .Select(ra => new Rel(ra.RelString))
                                    .Append(new Rel("down")),
                            Uri = (x.propertyUri),
                            Title = x.propertyInfo.Name,
                            Classes = GetClasses(x.propertyInfo.PropertyType.GetTypeInfo()),
                            FetchEntity = () =>
                            {
                                var value = (INode) x.propertyInfo.GetValue(node);
                                return GetResource(value, x.propertyUri, requestUri, serviceLocator).AsT0;
                            }
                        };
                    }).ToArray().ForEach( p  => properties.Add(p));
            

            type.DeclaredMethods.Where(x => x.GetCustomAttributes<ExposeAttribute>().Any())
                .Select(methodInfo => new
                {
                    methodInfo,
                    methodUri = new Uri((nodeUri.ToString() + "/" + methodInfo.Name), UriKind.Relative)
                })
                .Select(pair => this.BuildFromMethodInfo(node, pair.methodUri, pair.methodInfo, serviceLocator))
                .ToArray().ForEach(a => properties.Add(a));


            var entity = new Entity(nodeUri, GetClasses(type).ToArray(), properties.ToArray());

            return entity;
        }

        private static List<string> GetClasses(TypeInfo type)
        {
            return type
                .Recurse(t => t.BaseType.GetTypeInfo())
                .TakeWhile(t => t.BaseType != null)
                .Where(t => t.AsType() != typeof (object) && (t.GetCustomAttribute<HyperMapperAttribute>(false)?.UseTypeNameAsClassNameForEntity ?? true))
                .Select(t => t.Name).ToList();
        }

        private Action BuildFromMethodInfo(object o, Uri uri, MethodInfo methodInfo, Func<Type, object> serviceLocator)
        {
            var actionFields =
                methodInfo.GetParameters()
                    .Where(pi => pi.GetCustomAttributes<InjectAttribute>().Any() == false)
                    .Select(pi => new ActionField(pi.Name, BuildFromParameterInfo(pi)));

            Func<IEnumerable<Tuple<Key, object>>, Task<object>> invoke = async (submittedArgs) =>
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
                                throw new ArgumentException("Mismatch: expected " + pi.Name + ", received" + current.Item1.ToString());
                            return current.Item2;
                        }
                    });

                var result = methodInfo.Invoke(o, argsList.ToArray());
                if (methodInfo.ReturnType == typeof (void))
                {
                    return Task.FromResult<object>(0);
                }
                if (result is Task)
                {
                    await ((Task) result);
                }
                return Task.FromResult(result);
            };

            Tuple<Key, Type>[] argumentInfo = methodInfo.GetParameters()
                .Where(mi => mi.GetCustomAttribute<InjectAttribute>() == null)
                .Select(pi => Tuple.Create((Key) pi.Name, pi.ParameterType)).ToArray();

            

            return new Action(methodInfo.Name, methodInfo.Name, "POST", uri, "application/json", actionFields.ToArray(), invoke, argumentInfo);
        }

        private ActionField.FieldType BuildFromParameterInfo(ParameterInfo pi)
        {
            return ActionField.FieldType.Text;
        }
    }
}