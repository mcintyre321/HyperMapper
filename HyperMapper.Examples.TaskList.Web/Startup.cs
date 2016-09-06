using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper.Examples.TaskList.Domain;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Mapping;
using HyperMapper.Owin;
using HyperMapper.RequestHandling;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Owin;

[assembly: OwinStartup(typeof(HyperMapper.Examples.TaskList.Web.Startup))]

namespace HyperMapper.Examples.TaskList.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
          
            var hyperMapperSettings = new HyperMapperSettings()
            {
                BasePath = "/",
                ServiceLocator = type => LocateAdaptors(type)
            };


            var appRoot = BuildAppRoot();
            app.UseHypermedia(() =>
                X.MakeResourceFromNode(Tuple.Create((INode)appRoot, new Uri("/", UriKind.Relative)), null, hyperMapperSettings.ServiceLocator), hyperMapperSettings);

        }

        private static AppRoot BuildAppRoot()
        {
            var appRoot = new AppRoot();
            appRoot.Authentication.Register("testuser", "password");
            appRoot.Boards.AddBoard("My tasks", () => "board1");
            var board = appRoot.Boards.Items.Single(i => i.Key == "board1");
            board.AddCard("Finish HyperMapper", () => "card1");
            return appRoot;
        }

        /// <summary>
        /// Replace this with your own service location/ ioc container call
        /// </summary>
        private static object LocateAdaptors(Type type)
        {
            if (type == typeof(IdGenerator)) return ((IdGenerator)(() => Guid.NewGuid().ToString()));

            throw new Exception($"Could not resolve {type.FullName}");
        }
    }
}
