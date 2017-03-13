using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
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
            if (parameters.Length == 0) return new MethodArguments(new Tuple<UrlPart, object>[0]);

            return  MakeMethodArgs(parameters, await BindToJObject(request));
        }

        private static async Task<JObject> BindToJObject(IOwinRequest request)
        {
            switch (request.ContentType.Split(';')[0])
            {
                case "application/json":
                {
                    using (var sr = new StreamReader(request.Body))
                    {
                        var rawBody = await sr.ReadToEndAsync();
                        {
                            return JObject.Parse(rawBody);
                        }
                    }
                }
                case "application/x-www-form-urlencoded":
                    using (var sr = new StreamReader(request.Body))
                    {
                        var rawBody = await sr.ReadToEndAsync();
                        var dict = QueryStringHelper.QueryStringToDict(rawBody);
                        return JObject.Parse(JsonConvert.SerializeObject(dict));
                    }
                case "multipart/form-data":
                    var parser = new MultipartFormDataParser(request.Body);

                    var jo = new JObject();
                    foreach (var parameterPart in parser.Parameters)
                    {
                        jo[parameterPart.Name] = parameterPart.Data;
                    }
                    return jo;
                default:
                    throw new NotImplementedException();
            }
        }

        private static OneOf<ModelBindingFailed, MethodArguments> MakeMethodArgs(MethodParameter[] parameters, JObject jo)
        {
            var tuples = parameters
                .Select(mp => Tuple.Create(mp.UrlPart, FieldTypeToTypeLookup(mp, jo[mp.UrlPart.ToString()]))).ToArray();
            return new MethodArguments(tuples);
        }

        private static object FieldTypeToTypeLookup(MethodParameter methodParameter, JToken token)
        {
            return methodParameter.Type.Match<object>(
                text => token.Value<string>(),
                password => token.Value<string>(),
                select =>
                {
                    var options = select.Options;
                    var selectedHash = token.Value<string>();
                    return options.Single(oah => oah.OptionId == selectedHash).UnderlyingValue;
                }
            );
        }
    }

}