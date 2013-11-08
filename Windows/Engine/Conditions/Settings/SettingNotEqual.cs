﻿using Org.InCommon.InCert.Engine.Engines;
using Org.InCommon.InCert.Engine.Results.Misc;

namespace Org.InCommon.InCert.Engine.Conditions.Settings
{
    public class SettingNotEqual:SettingEquals
    {
        public SettingNotEqual(IEngine engine) : base(engine)
        {
        }

        public override BooleanReason Evaluate()
        {
            return base.Evaluate().Invert();
        }

        
    }
}
