using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper.RequestHandling;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using Owin;

namespace HyperMapper.Owin
{
    public static class ModelBinder
    {
        public static async Task<OneOf<ModelBindingFailed, BoundModel>> BindArgsFromRequest(Tuple<Key, Type>[] argumentDefs,
            IOwinRequest request)
        {
            if (argumentDefs.Length == 0) return new BoundModel(new Tuple<Key, object>[0]);

            switch (request.ContentType.Split(';')[0])
            {
                case "application/json":
                {
                    using (var sr = new StreamReader(request.Body))
                    {
                        var rawBody = await sr.ReadToEndAsync();
                        var jObject = JObject.Parse(rawBody);
                        var tuples = argumentDefs
                            .Select(
                                ai =>
                                    Tuple.Create(ai.Item1, jObject[ai.Item1.ToString()]?.ToObject(ai.Item2)))
                            .ToArray();
                        return new BoundModel(tuples);
                    }
                }
                case "application/x-www-form-urlencoded":
                    using (var sr = new StreamReader(request.Body))
                    {
                        var rawBody = await sr.ReadToEndAsync();
                        var dict = QueryStringHelper.QueryStringToDict(rawBody);
                        var jObject = JObject.Parse(JsonConvert.SerializeObject(dict));
                        return new BoundModel(argumentDefs
                            .Select(ai => Tuple.Create(ai.Item1, jObject[ai.Item1.ToString()]?.ToObject(ai.Item2)))
                            .ToArray());
                    }
                    break;
            }
            throw new NotImplementedException();
        }

    }

}