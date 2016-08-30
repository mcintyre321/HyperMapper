using System;
using System.Diagnostics;
using System.Linq;
using HyperMapper.Examples.TaskList.Domain;
using HyperMapper.Examples.TaskList.Domain.Ports;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using HyperMapper.Owin;
using HyperMapper.RequestHandling;
using Microsoft.Owin.Hosting;

namespace HyperMapper.Examples.TaskList.OwinApp
{
    public class Program
    {
        


        public static void Main(string[] args)
        {
            using (WebApp.Start("http://localhost:12345", Configure))
                Console.ReadLine();
        }

        private static void Configure(IAppBuilder obj)
        {

            var contentFileSystem = new PhysicalFileSystem(@"..\..\Content");
            obj.UseFileServer(new FileServerOptions() { FileSystem = contentFileSystem });

            var hyperMapperSettings = new HyperMapperSettings()
            {
                BasePath = "/",
                ServiceLocator = type => LocateAdaptors(type)
            };


            var appRoot = BuildAppRoot();
            obj.UseHypermedia(() => 
                X.MakeResourceFromNode(appRoot, new Uri("/", UriKind.Relative), hyperMapperSettings.ServiceLocator), hyperMapperSettings);

            Process.Start(@"c:\Program Files (x86)\Google\Chrome\Application\chrome.exe", "http://localhost:12345");
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
