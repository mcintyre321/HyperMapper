using System;

namespace HyperMapper.HyperModel
{
    public class Link  
    {
        public Link(string text, Rel[] rels, Uri uri)
        {
            Text = text;
            Rels = rels;
            Uri = uri;
        }

        public string Text { get; private set; }
        public Rel[] Rels { get; private set; }
        public Uri Uri { get; private set; }
        public string[] Classes { get; set; }

        public Func<Resource> Follow { get; set; }
    }
}