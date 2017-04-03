using System;
using System.Collections.Generic;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Mapper;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.Vocab;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class TaskListAppRoot : RootNode
    {
        public Authentication Authentication { get; }

        public Boards Boards { get; }

        public TaskListAppRoot(Uri uri) :base("Task Lists App", uri, TermFactory.From<TaskListAppRoot>())
        {
            this.Authentication = (new Authentication(this, nameof(Authentication)));
            this.Boards = (new Boards(this, nameof(Boards)));
        }

        public override ChildNodes ChildNodes => base.ChildNodes.Append(this.Authentication, this.Boards);
    }
}
