using System;
using System.Linq;
using HyperMapper.Examples.TaskList.Domain;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Examples.Web;
using HyperMapper.Examples.Web.Apps;
using HyperMapper.Mapper;
using HyperMapper.Owin;
using HyperMapper.RepresentationModel;
using HyperMapper.Siren;
using HyperMapper.Vocab;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace HyperMapper.Examples.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var representors = new Representor<SemanticDocument>[] { new Siren.SirenRepresentor(), new SirenHtmlRepresentor() };

            var taskListAppRoot = BuildTaskListAppRoot(new Uri("/tasks", UriKind.Relative));
            app.ExposeRootNodeAsHypermediaApi(taskListAppRoot, LocateAdaptors, representors);


            var chessAppRoot = new ChessEngineApp("ChessEngine", new Uri("/chess", UriKind.Relative), TermFactory.From<ChessEngineApp>());
            app.ExposeRootNodeAsHypermediaApi(chessAppRoot, LocateAdaptors, representors);

            var indexAppRoot = new IndexRootNode("Index", new Uri("/", UriKind.Relative));
            indexAppRoot.AddLink("chess engine app", chessAppRoot.Uri, TermFactory.From<ChessEngineApp>());
            indexAppRoot.AddLink("task list app", taskListAppRoot.Uri, TermFactory.From<TaskListAppRoot>());
            app.ExposeRootNodeAsHypermediaApi(indexAppRoot, LocateAdaptors, representors);

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
