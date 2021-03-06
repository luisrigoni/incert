﻿using Org.InCommon.InCert.Engine.Engines;

namespace Org.InCommon.InCert.Engine.Conditions.Wireless
{
    class NotConnectedToNetwork:ConnectedToNetwork
    {
        public NotConnectedToNetwork(IEngine engine):base(engine)
        {
        }

        public override Results.Misc.BooleanReason Evaluate()
        {
            return base.Evaluate().Invert();
        }
    }
}
