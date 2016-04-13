using System;
using System.Collections.Generic;
using HyperMapper.Examples.TaskList.Domain.Ports;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class AppRoot : RootNode
    {
        [Expose]
        public Users Users { get; }

        [Expose]
        public Boards Boards { get; }

        [Expose]
        public string Title => "Task Lists App";

        public AppRoot() 
        {
            this.Users = AddChild(new Users(this, nameof(Users)));
            this.Boards = AddChild(new Boards(this, nameof(Boards)));
        }
    }
}
