using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HttpMultipartParser;
using HyperMapper.RequestHandling;
using HyperMapper.ResourceModel;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using Owin;

namespace HyperMapper.Owin
{
    public static class ModelBinder
    {
        public static async Task<OneOf<ModelBindingFailed, MethodArguments>> BindArgsFromRequest(MethodParameter[] parameters,
            IOwinRequest request)
        {
            if (parameters.Length == 0) return new MethodArguments(new Tuple<Key, object>[0]);

            switch (request.ContentType.Split(';')[0])
            {
                case "application/json":
                {
                    using (var sr = new StreamReader(request.Body))
                    {
                        var rawBody = await sr.ReadToEndAsync();
                        var jObject = JObject.Parse(rawBody);
                        var tuples = parameters
                            .Select(
                                methodParameter =>
                                    Tuple.Create(methodParameter.Key, jObject[methodParameter.Key.ToString()]?.ToObject(FieldTypeToTypeLookup(methodParameter.Type))))
                            .ToArray();
                        return new MethodArguments(tuples);
                    }
                }
                case "application/x-www-form-urlencoded":
                    using (var sr = new StreamReader(request.Body))
                    {
                        var rawBody = await sr.ReadToEndAsync();
                        var dict = QueryStringHelper.QueryStringToDict(rawBody);
                        var jObject = JObject.Parse(JsonConvert.SerializeObject(dict));
                        return new MethodArguments(parameters
                            .Select(ai => Tuple.Create(ai.Key, jObject[ai.Key.ToString()]?.ToObject(FieldTypeToTypeLookup(ai.Type))))
                            .ToArray());
                    }
                    break;
                case "multipart/form-data":
                    var parser = new MultipartFormDataParser(request.Body);

                    var multiPartJObject = new JObject();
                    foreach (var parameterPart in parser.Parameters)
                    {
                        multiPartJObject[parameterPart.Name] = parameterPart.Data;
                    }
                    return new MethodArguments(parameters
                        .Select(ai => Tuple.Create(ai.Key, multiPartJObject[ai.Key.ToString()]?.ToObject(FieldTypeToTypeLookup(ai.Type))))
                        .ToArray());

                    break;
            }
            throw new NotImplementedException();
        }

        private static Type FieldTypeToTypeLookup(MethodParameter.MethodParameterType methodParameter)
        {
            switch (methodParameter)
            {
                case MethodParameter.MethodParameterType.Text:
                    return typeof(string);
                case MethodParameter.MethodParameterType.Password:
                    return typeof(string);
                default:
                    throw new NotImplementedException();
            }
        }
    }

}