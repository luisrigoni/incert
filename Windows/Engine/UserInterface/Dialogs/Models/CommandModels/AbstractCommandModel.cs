﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Ninject;
using Ninject.Parameters;
using Org.InCommon.InCert.Engine.Extensions;
using Org.InCommon.InCert.Engine.Logging;
using Org.InCommon.InCert.Engine.UserInterface.ContentWrappers.ButtonWrappers;
using Org.InCommon.InCert.Engine.UserInterface.ContentWrappers.LinkWrappers;
using Org.InCommon.InCert.Engine.UserInterface.Dialogs.Models.DialogModels;
using log4net;

namespace Org.InCommon.InCert.Engine.UserInterface.Dialogs.Models.CommandModels
{
    public abstract class AbstractCommandModel : AbstractModel, ICommandModel
    {
        private static readonly ILog Log = Logger.Create();

        private string _text;
        private bool _defaultButton;
        private bool _cancelButton;

        protected AbstractCommandModel(AbstractDialogModel model)
            : base(model)
        {
            PropertyChanged += PropertyChangedHandler;
        }

        public void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("Enabled", StringComparison.InvariantCulture))
                return;

            FlagPropertyAsChanged("TextBrush");
        }

        public abstract ICommand Command { get; }

        public ButtonTargets Target { get; set; }

        public Brush TextBrush
        {
            get
            {
                return Enabled ?
                    AppearanceManager.NavigationTextBrush :
                    AppearanceManager.MakeBrushTransparent(AppearanceManager.NavigationTextBrush as SolidColorBrush, 45);
            }
        }

        public bool IsDefaultButton
        {
            get { return _defaultButton; }
            set { _defaultButton = value; OnPropertyChanged(); }
        }

        public bool IsCancelButton
        {
            get { return _cancelButton; }
            set { _cancelButton = value; OnPropertyChanged(); }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        public static AbstractCommandModel FromButtonWrapper(AbstractDialogModel dialog, AbstractButtonWrapper wrapper)
        {
            var result = MapButtonWrapperToModel(dialog, wrapper);
            if (result == null)
            {
                Log.WarnFormat("Cannot instatiate model for wrapper of type {0}", wrapper.GetType().Name);
                return null;
            }

            result.Enabled = wrapper.Enabled;
            result.Text = wrapper.Text;
            result.Visibility = wrapper.Visible ? Visibility.Visible : Visibility.Collapsed;
            result.Target = wrapper.Target;
            result.IsDefaultButton = wrapper.IsDefaultButton;
            result.IsCancelButton = wrapper.IsCancelButton;
            return result;
        }

        private static AbstractCommandModel MapButtonWrapperToModel(AbstractDialogModel dialog, AbstractButtonWrapper wrapper)
        {
            var kernal = Application.Current.CurrentKernel();
            var modelParameter = new ConstructorArgument("model", dialog);

            if (wrapper is ResultButton)
                return kernal.Get<ReturnResultModel>(
                    new IParameter[]
                        {
                            modelParameter,
                            new ConstructorArgument("result", (wrapper as ResultButton).TaskResult)
                        });

           
            if (wrapper is UrlButton)
            {
                var link = kernal.Get<UrlLink>();
                link.Target = (wrapper as UrlButton).Value;
                return kernal.Get<OpenLinkModel>(
                    new IParameter[]
                        {
                            modelParameter,
                            new ConstructorArgument("link", link)
                        }
                    );
            }


            if (wrapper is SettingsButton)
            {
                var link = kernal.Get<SettingsLink>();
                link.Target = (wrapper as SettingsButton).Key;
                link.Value = "true";
                return kernal.Get<ModifySettingsModel>(
                    new IParameter[]
                        {
                            modelParameter,
                            new ConstructorArgument("wrapper", link)
                        }
                    );
            }


            if (wrapper is TopicButton)
            {
                var value = (wrapper as TopicButton).Value;
                return kernal.Get<OpenTopicModel>(
                   new IParameter[]
                        {
                            modelParameter,
                            new ConstructorArgument("topic",value)
                        }
                   );
            }


            if (wrapper is StoredResultButton)
            {
                return kernal.Get<ReturnStoredResultModel>(
                    new IParameter[]
                        {
                            modelParameter,
                            new ConstructorArgument("key",(wrapper as StoredResultButton).Key)
                        });
            }

            if (wrapper is OpenAdvancedMenuButton)
            {
                var group = (wrapper as OpenAdvancedMenuButton).Group;
                return kernal.Get<OpenAdvancedButtonModel>(
                    new IParameter[]
                        {
                            modelParameter,
                            new ConstructorArgument("group", group)
                        });

            }

            if (wrapper is DisabledButton)
                return kernal.Get<DisabledCommandModel>(modelParameter);

            if (wrapper is CancelButton)
                return kernal.Get<CancelButtonModel>(modelParameter);

            if (wrapper is DoNothingButton)
                return kernal.Get<DoNothingCommandModel>(modelParameter);

            return null;
        }




    }
}
