using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper.Examples.TaskList.Domain;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Mapper;
using HyperMapper.Owin;
using HyperMapper.RequestHandling;
using HyperMapper.ResourceModel;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using OneOf.Types;
using Owin;

[assembly: OwinStartup(typeof(HyperMapper.Examples.TaskList.Web.Startup))]

namespace HyperMapper.Examples.TaskList.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            ServiceLocatorDelegate serviceLocator = type =>
            {
                var instance = Activator.CreateInstance(type);

                return Tuple.Create(instance, new Action(() =>
                {
                    if (instance is IDisposable)
                    {
                        ((IDisposable) instance).Dispose();
                    }
                }));
            };

            var basePath = "/";
            var appRoot = BuildAppRoot(new Uri(basePath, UriKind.Relative));
            HyperMapper.RequestHandling.Router makeHypermediaRouter = Routing.MakeHypermediaRouterFromRootNode(appRoot, serviceLocator);

            app.RouteWithRepresentors(makeHypermediaRouter, new Representor[] {new Siren.SirenRepresentor()},  basePath);

        }

        private static AppRoot BuildAppRoot(Uri uri)
        {
            var appRoot = new AppRoot(uri);
            appRoot.Authentication.Register("testuser", "password");
            appRoot.Boards.AddBoard("My tasks", "Things I need to do", () => "board1");
            var board = appRoot.Boards.Items.Single(i => i.UrlPart == "board1");
            board.AddCard("Finish HyperMapper", "Coding n stuff", () => "card1");
            return appRoot;
        }

        /// <summary>
        /// Replace this with your own service location/ ioc container call
        /// </summary>
        private static Tuple<object, Action> LocateAdaptors(Type type)
        {
            if (type == typeof(IdGenerator)) return Tuple.Create((object)(IdGenerator)(() => Guid.NewGuid().ToString()), null as Action);

            throw new Exception($"Could not resolve {type.FullName}");
        }
    }
}
