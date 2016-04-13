namespace HyperMapper.Siren
{
    public class TemplateBase<TModel> : RazorGenerator.Templating.RazorTemplateBase
    {
        public TModel Model { get; set; }
    }
}