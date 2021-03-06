﻿using Org.InCommon.InCert.Engine.Engines;

namespace Org.InCommon.InCert.Engine.Conditions.Versioning
{
    class NoVersionInstalled:AnyVersionInstalled
    {
        public NoVersionInstalled(IEngine engine):base(engine)
        {
        }

        public override Results.Misc.BooleanReason Evaluate()
        {
            return base.Evaluate().Invert();
        }
    }
}
