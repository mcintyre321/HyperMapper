using System;

namespace HyperMapper.HyperModel
{
    public class Link  
    {
        public Link(string text, string rel, Uri uri)
        {
            Text = text;
            Rel = rel;
            Uri = uri;
        }

        public string Text { get; private set; }
        public string Rel { get; private set; }
        public Uri Uri { get; private set; }
    }
}