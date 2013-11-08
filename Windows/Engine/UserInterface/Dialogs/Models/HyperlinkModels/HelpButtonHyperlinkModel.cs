﻿using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Org.InCommon.InCert.Engine.UserInterface.ContentWrappers.LinkWrappers;

namespace Org.InCommon.InCert.Engine.UserInterface.Dialogs.Models.HyperlinkModels
{
    class HelpButtonHyperlinkModel:AbstractHyperlinkModel
    {
        public HelpButtonHyperlinkModel(AbstractModel parentModel) : base(parentModel)
        {
        }

        public override void LoadContent(AbstractLink wrapper)
        {
            base.LoadContent(wrapper);
            RootDialogModel.HelpModel.PropertyChanged += PropertyChangedHandler;
        }

        public override Brush TextBrush
        {
            get
            {

                if (!Parent.Enabled)
                    return AppearanceManager.MakeBrushTransparent(LinkBrush as SolidColorBrush, 45);

                if (!RootDialogModel.HelpModel.Enabled)
                    return AppearanceManager.MakeBrushTransparent(LinkBrush as SolidColorBrush, 45);

                return LinkBrush;

            }

        }

        protected override void SetEnabledBinding(Hyperlink instance)
        {
            instance.SetBinding(ContentElement.IsEnabledProperty, GetOneWayBinding(RootDialogModel.HelpModel, "Enabled"));
        }

        protected override void SetCommandBinding(Hyperlink instance)
        {
            instance.SetBinding(Hyperlink.CommandProperty, GetOneWayBinding(RootDialogModel.HelpModel, "Command"));
        }
    }
}
