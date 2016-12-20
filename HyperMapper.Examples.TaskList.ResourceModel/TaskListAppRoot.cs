using System;
using System.Collections.Generic;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Mapper;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class TaskListAppRoot : RootNode
    {
        [Expose]
        public Authentication Authentication { get; }

        [Expose]
        public Boards Boards { get; }

        public TaskListAppRoot(Uri uri) :base("Task Lists App", uri, TermFactory.From<TaskListAppRoot>())
        {
            this.Authentication = AddChild(new Authentication(this, nameof(Authentication)));
            this.Boards = AddChild(new Boards(this, nameof(Boards)));
        }
    }
}
