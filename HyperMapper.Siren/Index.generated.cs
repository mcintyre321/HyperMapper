﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HyperMapper.Siren
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    public partial class Index : HyperMapper.Siren.TemplateBase<Newtonsoft.Json.Linq.JToken>
    {
#line hidden

        public override void Execute()
        {


WriteLiteral("\r\n");


WriteLiteral(@"<!DOCTYPE html>

<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title>React OWIN Sample</title>
    <script src=""//cdnjs.cloudflare.com/ajax/libs/jquery/3.0.0-beta1/jquery.min.js"" type=""text/javascript""></script>
    <script src=""//cdnjs.cloudflare.com/ajax/libs/react/15.0.1/react-with-addons.js"" type=""text/javascript""></script>
    <script src=""//cdnjs.cloudflare.com/ajax/libs/react/15.0.1/react-dom.js"" type=""text/javascript""></script>
    <script src=""//cdn.jsdelivr.net/form2js/2.0/form2js.min.js"" type=""text/javascript""></script>
    <script src=""/Content/Siren.js""></script>
</head>
<body>

<h1>React demo app</h1>
<div id=""container""></div>
<script type=""text/javascript"">
    var model = ");


            
            #line 20 "..\..\Index.cshtml"
           Write(Model.ToString(Newtonsoft.Json.Formatting.Indented));

            
            #line default
            #line hidden
WriteLiteral(@";
    var component = ReactDOM.render(
        React.createElement(Siren, model),
        document.getElementById('container')
    ); 

    //$(document).on('submit', 'form', function (e) {
    //    e.preventDefault();
    //    var $form = $(this).closest(""form"");
    //    $.ajax({
    //        url: $form.attr(""action""),
    //        cache: false,
    //        type: 'POST',
    //        data: (form2js($form[0])),
    //        dataType: 'json',
    //        accepts: ""application/vnd.siren+json; charset=utf-8"",
    //        success: function(data) {
    //            component.setState(data);
    //        }, 
    //    });
    //});
</script>
</body>
</html>");


        }
    }
}
#pragma warning restore 1591
