using System;
using System.Collections.Generic;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Mapping;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class AppRoot : RootNode
    {
        [Expose]
        public Authentication Authentication { get; }

        [Expose]
        public Boards Boards { get; }

        public AppRoot() :base("Task Lists App")
        {
            this.Authentication = AddChild(new Authentication(this, nameof(Authentication)));
            this.Boards = AddChild(new Boards(this, nameof(Boards)));
        }
    }
}
