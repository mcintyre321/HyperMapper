using System;
using System.Linq;
using HyperMapper.Examples.TaskList.Domain;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Examples.Web;
using HyperMapper.Mapper;
using HyperMapper.Owin;
using HyperMapper.RepresentationModel;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace HyperMapper.Examples.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            {
                var taskListAppRoot = BuildTaskListAppRoot(new Uri("/tasks", UriKind.Relative));
                RequestHandling.Router taskListRouter = Routing.MakeHypermediaRouterFromRootNode(taskListAppRoot, LocateAdaptors);
                app.RouteWithRepresentors(taskListRouter, new Representor[] {new Siren.SirenRepresentor()}, taskListAppRoot.Uri.ToString());
            }
            {
                var chessAppRoot = new GameFactory("GF", new Uri("/chess", UriKind.Relative),                     TermFactory.From<GameFactory>());
                RequestHandling.Router chessRouter = Routing.MakeHypermediaRouterFromRootNode(chessAppRoot, LocateAdaptors);
                app.RouteWithRepresentors(chessRouter, new Representor[] {new Siren.SirenRepresentor()}, chessAppRoot.Uri.ToString());
            }

        }

        private static TaskListAppRoot BuildTaskListAppRoot(Uri uri)
        {
            var appRoot = new TaskListAppRoot(uri);
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

            return DefaultServiceLocatorDelegate.CreateUsingEmptyCtorAndDisposeIfAvailable(type);
        }
    }
}
