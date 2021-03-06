﻿using System.Windows;
using Org.InCommon.InCert.Engine.Importables;
using Org.InCommon.InCert.Engine.Logging;
using Org.InCommon.InCert.Engine.Results;
using Org.InCommon.InCert.Engine.Results.ControlResults;
using Org.InCommon.InCert.Engine.Engines;
using log4net;

namespace Org.InCommon.InCert.Engine.Tasks.UserInterface
{
    class HideBannerControl : AbstractTask
    {
        private static readonly ILog Log = Logger.Create();

        [PropertyAllowedFromXml]
        public string ControlKey
        {
            get { return GetDynamicValue(); }
            set { SetDynamicValue(value); }
        }

        [PropertyAllowedFromXml]
        public string Dialog
        {
            get { return GetDynamicValue(); }
            set { SetDynamicValue(value); }
        }

        public HideBannerControl(IEngine engine)
            : base(engine)
        {

        }

        public override IResult Execute(IResult previousResults)
        {
            var dialog = DialogsManager.GetExistingDialog(Dialog);
            if (dialog == null)
            {
                Log.WarnFormat("Unable to retrieve dialog for key {0}", Dialog);
                return new NextResult();
            }

            var models = dialog.GetModelsByKey(ControlKey);
            foreach (var model in models)
            {
                model.Visibility = Visibility.Hidden;
            }

            return new NextResult();
        }

        public override string GetFriendlyName()
        {
            return "Hide banner control";
        }
    }
}
