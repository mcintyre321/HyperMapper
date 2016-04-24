using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Owin;

namespace HyperMapper.Owin
{
    public static class ModelBinder
    {
        public static async Task<Tuple<Key, object>[]> BindArgsFromRequest(Tuple<Key, Type>[] argumentDefs,
            IOwinRequest request)
        {
            switch (request.ContentType.Split(';')[0])
            {
                case "application/json":
                {
                    using (var sr = new StreamReader(request.Body))
                    {
                        var rawBody = await sr.ReadToEndAsync();
                        var jObject = JObject.Parse(rawBody);
                        return argumentDefs
                            .Select(
                                ai =>
                                    Tuple.Create<Key, object>(ai.Item1, jObject[ai.Item1.ToString()]?.ToObject(ai.Item2)))
                            .ToArray();
                    }
                }
                case "application/x-www-form-urlencoded":
                    using (var sr = new StreamReader(request.Body))
                    {
                        var rawBody = await sr.ReadToEndAsync();
                        var dict = QueryStringHelper.QueryStringToDict(rawBody);
                        var jObject = JObject.Parse(JsonConvert.SerializeObject(dict));
                        return argumentDefs
                            .Select(ai => Tuple.Create(ai.Item1, jObject[ai.Item1.ToString()]?.ToObject(ai.Item2)))
                            .ToArray();
                    }
                    break;
            }
            throw new NotImplementedException();
        }

    }

}