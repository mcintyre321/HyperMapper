using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CsQuery;
using FormFactory;
using FormFactory.Attributes;
using FormFactory.RazorEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SirenDotNet;
using Action = System.Action;
using Formatting = System.Xml.Formatting;

namespace HyperMapper.Siren
{
    public class SirenToHtmlConverter 
    {




        public static string ReadSirenAndConvertToForm(SirenDotNet.Entity entity)
        {
            var html = BuildComponentsForEntity(entity, 1);
            var json = "<pre>" + string.Join(Environment.NewLine, JObject.FromObject(entity).ToString(Newtonsoft.Json.Formatting.Indented)) + "</pre>";
            html = SplitContainer(html, json);
            return html;
        }

        private static string BuildComponentsForEntity(Entity entity, int depth)
        {


            var list = new List<string>();

            if (entity.Title != null)
            {
                list.Add($"<h{depth}>{entity.Title}</h{depth}>");
            }

            list.AddRange(entity.Links?.Select(x => BuildPropertyVmFromLink(x).Render(new RazorTemplateHtmlHelper()).ToString()) ?? new List<string>());
            depth++;
            list.AddRange(entity.Entities?.Select(e => BuildPropertyVmFromSubEntity(e, depth))
                .Select(entityPropertiesHtml => string.Join(Environment.NewLine, entityPropertiesHtml))
                .Select(entityHtml => CsQuery.CQ.Create("<div>").Html(entityHtml).AddClass("subentity").AddClass("depth" + depth).Render())
                .ToList() ?? new List<string>());
            depth--;
            ;
            entity.Properties?.Properties().Select(PropertyVmFromJToken).ToList().ForEach(p => list.Add(p.Render(new RazorTemplateHtmlHelper()).ToString()));
            entity.Actions?.Select(BuildFormVmFromAction).ToList().ForEach(x => list.Add(x.Render(new RazorTemplateHtmlHelper()).ToString()));
            return CQ.Create("<div>").Html(string.Join(Environment.NewLine, list)).AddClass("entity").AddClass("depth" + depth).Render();

        }

        private static IEnumerable<string> BuildPropertyVmFromSubEntity(SubEntity e, int depth)
        {
            var list = new List<string>();

            var embedded = e as SubEntity.Embedded;
            if (embedded != null)
            {
                if (embedded.Title != null)
                {
                    list.Add($"<h{depth}>{embedded.Title}</h{depth}>");
                }

                embedded.Links?.Select(BuildPropertyVmFromLink).ToList().ForEach(x => list.Add(x.Render(new RazorTemplateHtmlHelper()).ToString()));
                embedded.Properties?.Properties()
                    .Select(PropertyVmFromJToken)
                    .ToList()
                    .ForEach(x => list.Add(x.Render(new RazorTemplateHtmlHelper()).ToString()));
                embedded.Actions?.Select(BuildFormVmFromAction).ToList().ForEach(x => list.Add(x.Render(new RazorTemplateHtmlHelper()).ToString()));


                depth++;
                list.AddRange(embedded.Entities?.Select(e1 => BuildPropertyVmFromSubEntity(e1, depth))
                    .Select(entityPropertiesHtml => string.Join(Environment.NewLine, entityPropertiesHtml))
                    .Select(
                        entityHtml =>
                            CsQuery.CQ.Create("<div>")
                                .Html(entityHtml)
                                .AddClass("entity")
                                .AddClass("depth" + depth)
                                .Render())
                    .ToList() ?? new List<string>());
                depth--;
                return list.AsEnumerable();
            }
            var linked = (SubEntity.Linked)e;
            if (linked != null)
            {
                return new[]
                {
                    $"<a href='{linked.Href}'>{linked.Title ?? linked.Href.ToString()}</a>"
                };
            }
            //not implemented
            return list;
        }

        private static PropertyVm PropertyVmFromJToken(JProperty property)
        {
            var propertyVm = new PropertyVm(typeof(string), property.Name)
            {
                Value = property.Value.ToString(),
                Readonly = true,
                DisplayName = property.Name,

            };
            //propertyVm.GetCustomAttributes = () => new object[] {new DataTypeAttribute(DataType.MultilineText)};
            return propertyVm;
        }

        private static PropertyVm BuildPropertyVmFromLink(Link link)
        {
            var element = new XElement("a", new XAttribute("href", link.Href));

            var rels = string.Join(", ", link.Rel);
            element.SetAttributeValue("title", rels);
            element.Value = link.Title ?? rels;
            var propertyVm = new PropertyVm(typeof(XElement), rels)
            {
                Value = element,
                Readonly = true,
                DisplayName = rels,
                Name = rels,
                GetCustomAttributes = () => new object[] { new NoLabelAttribute() }
            };
            return propertyVm;
        }

         

        private static FormVm BuildFormVmFromAction(SirenDotNet.Action action)
        {
            var form = new FormVm
            {
                ActionUrl = action.Href.ToString(),
                DisplayName = action.Title ?? action.Name ?? "link",
                Method = action?.Method.ToString().ToLower(),
                Inputs = action.Fields?.Select(field =>
                {
                    var propertyVm = new PropertyVm(typeof(string), field.Name) { DisplayName = field.Name };
                    if (field.Type.ToString() == "select")
                    {
                        propertyVm.Choices = ((IEnumerable<JObject>) field.ExtensionData["options"])
                            .Select(jo => Tuple.Create(jo["name"].Value<string>(), jo["value"].Value<string>()));
                    }
                    return propertyVm;
                })?.ToArray() ?? Enumerable.Empty<PropertyVm>().ToArray()

            };




            if (form.Method != "get" && form.Method != "post")
            {
                form.ActionUrl += form.ActionUrl.Contains("?") ? "&" : "?";
                form.ActionUrl += "_method=" + form.Method.ToString().ToUpper();
                form.Method = "post";
            }
            return form;
        }

        static string SplitContainer(string left, string right)
        {
            return
                $@"
<div class=""split-container"">
  <div class=""split-item split-left"">
     {left}
  </div>
  <div class=""split-item split-right"">
     {right}
  </div>
</div>
<style>

.split-container {{
  -webkit-box-orient: horizontal;
  -webkit-box-direction: normal;
  -webkit-flex-direction: row;
  -ms-flex-direction: row;
  flex-direction: row;
  display: -webkit-box;
  display: -webkit-flex;
  display: -ms-flexbox;
  display: flex;
}}

.split-item {{
  
  
  display: -webkit-box;
  display: -webkit-flex;
  display: -ms-flexbox;
  display: flex;
  -webkit-box-orient: vertical;
  -webkit-box-direction: normal;
  -webkit-flex-direction: column;
  -ms-flex-direction: column;
  flex-direction: column;
  
  
  width: 50%;
  padding: 3em 5em 6em 5em;
}}
 
.request{{
  background-color: aliceblue;  
}}

.response{{
  background-color: lightgrey;
}}

.response.status200{{
  background-color: f5fff0
}}

.entity, .subentity {{
    background-color: rgba(220, 220, 220, 0.1);
    border: solid 1px rgba(220, 220, 220, 0.2);
    padding: 10px 10px;
    -webkit-border-radius: 5px;
    -moz-border-radius: 5px;
    border-radius: 5px;
}}

</style>
";
        }


    }

}
