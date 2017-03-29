namespace HyperMapper.Vocab
{
    public static class Terms
    { 
        public static Term Title => new Term(nameof(Title));
        public static Term Parent => new Term(nameof(Parent));
        public static Term Child => new Term(nameof(Child));

        public static class Self 
        {
        }
    }
}