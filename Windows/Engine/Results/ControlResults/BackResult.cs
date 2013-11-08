﻿using Org.InCommon.InCert.Engine.TaskBranches.BranchStrategies;

namespace Org.InCommon.InCert.Engine.Results.ControlResults
{
    class BackResult:AbstractTaskResult
    {
        public override IBranchStrategy GetBranchStrategy()
        {
            return new MoveToPreviousTask();
        }

        public override bool IsOk()
        {
            return false;
        }
    }
}
