﻿using System;
using System.Diagnostics;
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
        private static readonly AppRoot AppRoot = new AppRoot();

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


            
            obj.UseHypermedia(() => 
                X.MakeResourceFromNode(AppRoot, new Uri("/", UriKind.Relative), hyperMapperSettings.ServiceLocator), hyperMapperSettings);

            Process.Start(@"c:\Program Files (x86)\Google\Chrome\Application\chrome.exe", "http://localhost:12345");
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
