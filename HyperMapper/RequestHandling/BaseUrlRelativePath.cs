using System;
using System.Collections.Generic;
using System.Linq;

namespace HyperMapper.RequestHandling
{
    public class BaseUrlRelativePath
    {
        private readonly string _path;

        public BaseUrlRelativePath(string path)
        {
            _path = path;
        }

        public IEnumerable<UrlPart> GetParts()
        {
            return _path.Split('/')
                .Where(p => !String.IsNullOrEmpty(p))
                .Select(s => new UrlPart(s));
        }
    }
}