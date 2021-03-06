﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Org.InCommon.InCert.Engine.Engines;
using Org.InCommon.InCert.Engine.Extensions;
using Org.InCommon.InCert.Engine.Logging;
using Org.InCommon.InCert.Engine.Results;
using Org.InCommon.InCert.Engine.Results.ControlResults;
using Org.InCommon.InCert.Engine.Results.Errors.General;
using Org.InCommon.InCert.Engine.Results.Errors.UserInterface;
using Org.InCommon.InCert.Engine.UserInterface.ContentWrappers.BannerWrappers;
using Org.InCommon.InCert.Engine.UserInterface.ContentWrappers.ButtonWrappers;
using Org.InCommon.InCert.Engine.UserInterface.Dialogs.ControlActions;
using Org.InCommon.InCert.Engine.UserInterface.Dialogs.Managers;
using Org.InCommon.InCert.Engine.UserInterface.Dialogs.Models.CommandModels;
using Org.InCommon.InCert.Engine.UserInterface.Dialogs.Models.ContainerModels;
using Org.InCommon.InCert.Engine.UserInterface.Dialogs.Properties;
using Org.InCommon.InCert.Engine.Utilities;
using log4net;

namespace Org.InCommon.InCert.Engine.UserInterface.Dialogs.Models.DialogModels
{
    public abstract class AbstractDialogModel : PropertyNotifyBase, IHasControlActions
    {
        private enum ModelKeys
        {
            ContentModel,
            HelpButton,
            BackButton,
            NextButton,
            AdvancedButton
        }

        private readonly Dictionary<ButtonTargets, Action<AbstractButtonWrapper>> _buttonAssigners;
        private readonly Dictionary<ModelKeys, AbstractModel> _childModels = new Dictionary<ModelKeys, AbstractModel>();

        private readonly IDialogsManager _dialogsManager;
        private readonly IBannerManager _bannerManager;

        private readonly IHasEngineFields _engine;
        private readonly Window _dialogInstance;

        private static readonly ILog Log = Logger.Create();

        private bool _showInTaskBar;
        private Brush _background;
        private double _width;
        private double _height;
        private WindowStyle _windowStyle;
        private Cursor _cursor;
        private IResult _result;
        private bool _canClose;
        private bool _enabled;
        private double _left;
        private double _top;
        private string _windowTitle;

        public IResult Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }

        private T GetModelForKey<T>(ModelKeys key) where T : AbstractModel
        {
            if (!_childModels.ContainsKey(key))
                return default(T);

            return _childModels[key] as T;
        }

        private void SetModelForKey(ModelKeys key, AbstractModel value)
        {
            if (value == null)
            {
                RemoveModelForKey(key);
                return;
            }

            value.ControlKey = key.ToString();
            _childModels[key] = value;

        }

        private void RemoveModelForKey(ModelKeys key)
        {
            if (!_childModels.ContainsKey(key))
                return;

            _childModels.Remove(key);
        }

        public AbstractCommandModel NextModel
        {
            get { return GetModelForKey<AbstractCommandModel>(ModelKeys.NextButton); }
            private set
            {
                SetModelForKey(ModelKeys.NextButton, value);
                OnPropertyChanged();
            }
        }

        public AbstractCommandModel BackModel
        {
            get { return GetModelForKey<AbstractCommandModel>(ModelKeys.BackButton); }
            private set
            {
                SetModelForKey(ModelKeys.BackButton, value);
                OnPropertyChanged();
            }
        }

        public AbstractCommandModel AdvancedModel
        {
            get { return GetModelForKey<AbstractCommandModel>(ModelKeys.AdvancedButton); }
            private set
            {
                SetModelForKey(ModelKeys.AdvancedButton, value);
                OnPropertyChanged();
            }
        }

        public AbstractCommandModel HelpModel
        {
            get { return GetModelForKey<AbstractCommandModel>(ModelKeys.HelpButton); }
            private set
            {
                SetModelForKey(ModelKeys.HelpButton, value);
                OnPropertyChanged();
            }
        }

        public AbstractModel ContentModel
        {
            get
            {
                return !_childModels.ContainsKey(ModelKeys.ContentModel) ? null
                    : _childModels[ModelKeys.ContentModel];
            }
            set
            {
                _childModels[ModelKeys.ContentModel] = value;
                OnPropertyChanged();
            }
        }

        public IAppearanceManager AppearanceManager { get; private set; }

        protected AbstractDialogModel(IHasEngineFields engine, Window dialogInstance)
        {
            AppearanceManager = engine.AppearanceManager;
            _dialogsManager = engine.DialogsManager;
            _bannerManager = engine.BannerManager;
            _engine = engine;
            _dialogInstance = dialogInstance;
            dialogInstance.DataContext = this;
            _enabled = true;
            _canClose = true;
            SuppressCloseQuestion = false;

            _buttonAssigners = new Dictionary<ButtonTargets, Action<AbstractButtonWrapper>>
                {
                    {ButtonTargets.AdvancedButton, AssignAdvancedModel},
                    {ButtonTargets.BackButton, AssignBackModel},
                    {ButtonTargets.HelpButton, AssignHelpModel},
                    {ButtonTargets.NextButton, AssignNextModel}
                };

            _dialogInstance.Closing += DialogCloseButtonHandler;
        }

        public Window DialogInstance { get { return _dialogInstance; } }

        public bool SuppressCloseQuestion { get; set; }

        public virtual FontFamily Font
        {
            get { return AppearanceManager.DefaultFontFamily; }
        }

        public virtual Brush TextBrush { get { return AppearanceManager.BodyTextBrush; } }

        public WindowStyle WindowStyle
        {
            get { return _windowStyle; }
            set { _windowStyle = value; OnPropertyChanged(); }
        }

        public string WindowTitle
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_windowTitle))
                {
                    _windowTitle = AppearanceManager.ApplicationTitle;
                }

                return _windowTitle;
            }
            set { _windowTitle = value; OnPropertyChanged(); }
        }

        public double Top
        {
            get { return _top; }
            set { _top = value; OnPropertyChanged(); }
        }

        public double Left
        {
            get { return _left; }
            set { _left = value; OnPropertyChanged(); }
        }

        public WindowStartupLocation StartupLocation
        {
            get { return DialogInstance.WindowStartupLocation; }
            set { DialogInstance.WindowStartupLocation = value; }
        }

        public double Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged(); }
        }

        public double Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged(); }
        }

        public Cursor Cursor
        {
            get { return _cursor; }
            set { _cursor = value; OnPropertyChanged(); }
        }

        public bool CanClose
        {
            get { return _canClose; }
            set
            {
                _canClose = value;
                OnPropertyChanged();
                if (value)
                    UserInterfaceUtilities.EnableCloseButton(_dialogInstance);
                else
                    UserInterfaceUtilities.DisableCloseButton(_dialogInstance);
            }
        }

        public bool IsEnabled
        {
            get { return _enabled; }
            set { _enabled = value; OnPropertyChanged(); }
        }

        public bool ShowInTaskbar
        {
            get { return _showInTaskBar; }
            set { _showInTaskBar = value; OnPropertyChanged(); }
        }

        public Brush Background
        {
            get { return _background; }
            set { _background = value; OnPropertyChanged(); }
        }

        public ImageSource Icon
        {
            get { return AppearanceManager.ApplicationIcon; }
        }

        public IResult ShowBanner(string key)
        {
            var banner = _bannerManager.GetBanner(key);
            return banner == null ? new BannerNotDefined { Banner = key } : ShowBanner(banner);
        }

        private void LoadContent(AbstractBanner banner)
        {
            try
            {
                if (IsBannerCurrent(banner))
                    return;

                SetNavigationModels(banner.GetButtons());

                var content = new ContentPanelModel(this);
                content.LoadContent<DependencyObject>(banner);

                content.Padding = banner.Margin;
                content.Background = AppearanceManager.GetBrushForColor(banner.Background, AppearanceManager.BackgroundBrush);

                CanClose = banner.CanClose;
                SuppressCloseQuestion = banner.SuppressCloseQuestion;

                Height = banner.Height;
                Width = banner.Width;
                Background = AppearanceManager.GetBrushForColor(banner.Background, AppearanceManager.BackgroundBrush);
                Cursor = banner.Cursor;
                ContentModel = content;

                AddActions(banner.GetActions());
                RaiseChangedEvents();

            }
            catch (Exception e)
            {
                Log.WarnFormat("An exception occurred while attempting to load banner content: {0}", e.Message);
            }
        }

        private bool IsBannerCurrent(AbstractBanner banner)
        {
            if (banner.AlwaysRefresh)
                return false;

            var contentModel = ContentModel as AbstractContainerModel;

            return contentModel != null && contentModel.CurrentBanner.Equals(banner);
        }

        private IResult ShowBanner(AbstractBanner banner)
        {
            LoadContent(banner);

            DoActions(true);
            _dialogInstance.Show();

            ActivateInstance(banner);

            return new NextResult();
        }

        private void ActivateInstance(AbstractBanner banner)
        {
            try
            {
                if (banner == null)
                    return;

                if (!banner.Activate)
                    return;

                if (_dialogInstance == null)
                    return;

                if (!_dialogInstance.IsLoaded)
                    return;

                if (!_dialogInstance.IsVisible)
                    return;

                _dialogInstance.Activate();
            }
            catch (Exception e)
            {
                Log.WarnFormat("An exception occurred while attempting to activate a dialog instance: {0}", e.Message);
            }
        }

        public void ResetFocus()
        {
            try
            {
                var scope = FocusManager.GetFocusScope(_dialogInstance);
                if (scope == null)
                    return;

                FocusManager.SetFocusedElement(scope, null);
                Keyboard.ClearFocus();
            }
            catch (Exception e)
            {
                Log.WarnFormat("An exception occurred while attempting to reset the dialog's focus: {0}", e.Message);
            }
        }

        public IResult ShowBannerModal(string key)
        {
            var banner = _bannerManager.GetBanner(key);
            return banner == null ? new BannerNotDefined { Banner = key } : ShowBannerModal(banner);
        }

        private IResult ShowBannerModal(AbstractBanner banner)
        {
            var result = ShowBanner(banner);
            return !result.IsOk() ? result : WaitForResult();
        }

        private void SetNavigationModels(List<AbstractButtonWrapper> buttons)
        {
            var invisibleModel = new InvisibleCommandModel(this)
            {
                FontSize = 12,
                Font = AppearanceManager.DefaultFontFamily
            };
            SetModelForKey(ModelKeys.AdvancedButton,invisibleModel);
            SetModelForKey(ModelKeys.BackButton, invisibleModel);
            SetModelForKey(ModelKeys.HelpButton, invisibleModel);
            SetModelForKey(ModelKeys.NextButton, invisibleModel);

            if (!buttons.Any())
                return;

            foreach (var button in buttons)
            {
                AssignNavigationModel(button);
            }
        }

        private void RaiseChangedEvents()
        {
            FlagPropertyAsChanged("NextModel");
            FlagPropertyAsChanged("HelpModel");
            FlagPropertyAsChanged("AdvancedModel");
            FlagPropertyAsChanged("BackModel");
        }

        protected void AssignNavigationModel(AbstractButtonWrapper button)
        {
            if (button == null)
            {
                Log.Warn("Cannot assign navigation model; wrapper is null or invald.");
                return;
            }

            if (!_buttonAssigners.ContainsKey(button.Target))
            {
                Log.WarnFormat("Cannot assign navigation model for key {0}: key does not exist in collection", button.Target);
                return;
            }

            _buttonAssigners[button.Target].Invoke(button);
        }

        private IResult WaitForResult()
        {
            Result = null;
            do
            {
                Application.Current.DoEvents();
                Thread.Sleep(5);
            } while (Result == null);

            return Result;
        }

        public IResult ShowChildBanner(AbstractDialogModel childDialog, string banner)
        {
            try
            {
                if (childDialog == null)
                    return new DialogInstanceNotFound { Dialog = "[Undefined]" };

                childDialog.DialogInstance.Hide();
                childDialog.DialogInstance.Owner = DialogInstance;
                childDialog.DialogInstance.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                return childDialog.ShowBanner(banner);
            }
            catch (Exception e)
            {
                return new ExceptionOccurred(e);
            }
        }

        public IResult ShowChildBannerModal(AbstractDialogModel childDialog, string banner)
        {
            EnableDisableAllControls(false);
            try
            {
                if (childDialog == null)
                    return new DialogInstanceNotFound { Dialog = "[Undefined]" };

                var window = childDialog.DialogInstance;
                window.Hide();
                window.Owner = DialogInstance;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                var result = childDialog.ShowBannerModal(banner);
                window.Hide();

                return result;
            }
            catch (Exception e)
            {
                return new ExceptionOccurred(e);
            }
            finally
            {
                EnableDisableAllControls(true);
            }
        }

        protected virtual void RaiseCloseQuestion()
        {
            try
            {
                if (SuppressCloseQuestion)
                {
                    Result = new CloseResult();
                    return;
                }

                if (_dialogInstance.Visibility != Visibility.Visible)
                {
                    Result = new CloseResult();
                    return;
                }

                var closeWindow = _dialogsManager.GetDialog<BorderedChildDialogModel>("CloseWindow");
                if (closeWindow == null)
                {
                    Log.Warn("Could not obtain child dialog for the key 'CloseWindow'. Suppressing close dialog");
                    Result = new CloseResult();
                    return;
                }

                _dialogsManager.CancelPending = true;
                var result = ShowChildBannerModal(closeWindow, "Confirm close banner");
                if ((result as CloseResult) == null)
                {
                    Result = null;
                    _dialogsManager.CancelRequested = false;
                    return;
                }

                _dialogsManager.CancelRequested = true;
                Result = result;
            }
            finally
            {
                _dialogsManager.CancelPending = false;
            }
        }

        public void EnableDisableAllControls(List<string> excludeList, bool enabled)
        {
            EnableDisableCloseButton(enabled);

            foreach (var key in _childModels.Keys)
            {
                if (excludeList.Contains(key.ToString()))
                    continue;

                if (_childModels[key] == null)
                    continue;

                _childModels[key].Enabled = enabled;
                _childModels[key].EnableDisableAllControls(excludeList, enabled);
            }

            if (enabled)
            {
                _dialogInstance.Activate();
                DoActions(true);

            }
        }

        public void EnableDisableCloseButton(bool enabled)
        {
            if (!enabled)
            {
                UserInterfaceUtilities.DisableCloseButton(_dialogInstance);
                return;
            }

            if (!CanClose)
                return;

            UserInterfaceUtilities.EnableCloseButton(_dialogInstance);
        }

        public void EnableDisableAllControls(bool enabled)
        {
            EnableDisableAllControls(new List<string>(), enabled);
        }

        public void HideDialog()
        {
            DialogInstance.Hide();
        }

        private void AssignHelpModel(AbstractButtonWrapper info)
        {
            HelpModel = AbstractCommandModel.FromButtonWrapper(_engine, this, info);
        }

        private void AssignAdvancedModel(AbstractButtonWrapper info)
        {
            AdvancedModel = AbstractCommandModel.FromButtonWrapper(_engine, this, info);
        }

        private void AssignBackModel(AbstractButtonWrapper info)
        {
            BackModel = AbstractCommandModel.FromButtonWrapper(_engine, this, info);
        }

        private void AssignNextModel(AbstractButtonWrapper info)
        {
            NextModel = AbstractCommandModel.FromButtonWrapper(_engine, this, info);
        }

        private void DialogCloseButtonHandler(object sender, CancelEventArgs e)
        {
            RaiseCloseQuestion();
            e.Cancel = true;
        }

        public void ClearActions()
        {
            foreach (var model in _childModels.Values.Where(model => model != null))
            {
                model.ClearActions();
            }
        }

        public void AddAction(AbstractControlAction action)
        {
            foreach (var model in _childModels.Values.Where(model => model != null))
            {
                model.AddAction(action);
            }
        }

        public void AddActions(List<AbstractControlAction> actions)
        {
            foreach (var model in _childModels.Values.Where(model => model != null))
            {
                model.AddActions(actions);
            }
        }

        public void DoActions(bool includeOneTime)
        {
            foreach (var model in _childModels.Values.Where(model => model != null))
            {
                model.DoActions(includeOneTime);
            }
        }

        public List<AbstractModel> GetModelsBySettingKey(string key)
        {
            var modelList = new List<AbstractModel>();
            foreach (var model in _childModels.Values)
            {
                if (model == null)
                    continue;

                model.ListKeyedModels(key, ref modelList);
            }

            return modelList;
        }

        public List<AbstractModel> GetModelsByKey(string key)
        {
            var modelList = new List<AbstractModel>();
            foreach (var model in _childModels.Values)
            {
                if (model == null)
                    continue;

                model.ListNamedModels(key, ref modelList);
            }

            return modelList;
        }
    }
}

