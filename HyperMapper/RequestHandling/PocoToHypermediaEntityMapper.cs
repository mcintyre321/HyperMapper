using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HyperMapper.Models;
using Newtonsoft.Json.Linq;
using OneOf;
using Action = HyperMapper.Models.Action;

namespace HyperMapper.RequestHandling
{
    public class PocoToHypermediaEntityMapper
    {
        public Entity Map(Uri uri, INode node, Func<Type, object> serviceLocator)
        {
            var type = node.GetType().GetTypeInfo();

            var classes = GetClasses(type);

            Dictionary<Key, OneOf<SubEntityRef, Action, Link, JToken>> properties = new Dictionary<Key, OneOf<SubEntityRef, Action, Link, JToken>>();
            var markedUpProperties = type.DeclaredProperties.Select(propertyInfo => new
            {
                propertyInfo,
                att = propertyInfo.GetCustomAttribute<ExposeAttribute>()
            })
            .Where(x => x.att != null);
            foreach (var x in markedUpProperties)
            {
                var propertyInfo = x.propertyInfo;
                var propertyUri = new Uri(uri.ToString() + "/" + propertyInfo.Name, UriKind.Relative);

                if (propertyInfo.PropertyType == typeof (string))
                {
                    properties.Add(propertyInfo.Name, JToken.FromObject(propertyInfo.GetValue(node)));
                }
                else
                {
                    properties.Add(propertyInfo.Name, new SubEntityRef()
                    {
                        Rels = x.propertyInfo.GetCustomAttributes<RelAttribute>().Select(ra => new Rel(ra.RelString)).Append(new Rel("down")),
                        Uri = new Uri(uri.ToString() +  "/" + x.propertyInfo.Name, UriKind.Relative),
                        Title = x.propertyInfo.Name,
                        Classes = GetClasses(propertyInfo.PropertyType.GetTypeInfo()),
                        FetchEntity = () =>
                        {
                            var value = (INode) propertyInfo.GetValue(node);
                            return this.Map(propertyUri, value, serviceLocator);
                        }
                    });
                }
            }
            foreach (var methodInfo in type.DeclaredMethods.Where(x => x.GetCustomAttributes<ExposeAttribute>().Any()))
            {
                var methodUri = new Uri(uri.ToString() + "/" + methodInfo.Name, UriKind.Relative);
                var methodAction = this.BuildFromMethodInfo(node, methodUri, methodInfo, serviceLocator);
                properties.Add(methodInfo.Name, methodAction);
            }
            
            var entity = new Entity(node.Key, uri, classes.ToArray(), properties);

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