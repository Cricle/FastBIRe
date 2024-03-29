﻿using FastBIRe.Project.Models;

namespace FastBIRe.Project.Accesstor
{
    public class CreatingProjectEventArgs<TInput, TId> : WithProjectEventArgs<TInput, TId>
           where TInput : IProjectAccesstContext<TId>
    {
        public CreatingProjectEventArgs(TInput input, IProject<TId>? project) : base(input, project)
        {
        }
    }
    public class UpdatingProjectEventArgs<TInput, TId> : WithProjectEventArgs<TInput, TId>
           where TInput : IProjectAccesstContext<TId>
    {
        public UpdatingProjectEventArgs(TInput input, IProject<TId>? project) : base(input, project)
        {
        }
    }
    public class UpdatedProjectEventArgs<TInput, TId> : BoolProjectEventArgs<TInput, TId>
           where TInput : IProjectAccesstContext<TId>
    {
        public UpdatedProjectEventArgs(TInput input, bool succeed) : base(input, succeed)
        {
        }
    }
}
