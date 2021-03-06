﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Org.InCommon.InCert.Engine.Extensions;
using Org.InCommon.InCert.Engine.Logging;
using Org.InCommon.InCert.Engine.Settings;
using Org.InCommon.InCert.Engine.UserInterface.ContentWrappers.ContentControlWrappers;
using Org.InCommon.InCert.Engine.UserInterface.Dialogs.Converters;
using Org.InCommon.InCert.Engine.UserInterface.Dialogs.Instances.CustomControls;
using log4net;

namespace Org.InCommon.InCert.Engine.UserInterface.Dialogs.Models.ContentModels
{
    class InputContentModel : AbstractContentModel
    {
        private readonly ISettingsManager _manager;
        private static readonly ILog Log = Logger.Create();
        private int _maxLines;
        private int _minLines;
        private int _maxLength;
        private bool _readOnly;
        private ScrollBarVisibility _verticalScrollbarVisibility;
        private ScrollBarVisibility _horizontalScrollbarVisibility;
        private string _watermark;
        private Brush _waterMarkBrush;

        public string Watermark
        {
            get { return _watermark; }
            set { _watermark = value; OnPropertyChanged(); }
        }

        public Brush WatermarkBrush
        {
            get { return _waterMarkBrush; }
            set { _waterMarkBrush = value; OnPropertyChanged(); }
        }

        public bool IsReadOnly
        {
            get { return _readOnly; }
            set { _readOnly = value; OnPropertyChanged(); }
        }

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return _horizontalScrollbarVisibility; }
            set { _horizontalScrollbarVisibility = value; OnPropertyChanged(); }
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return _verticalScrollbarVisibility; }
            set { _verticalScrollbarVisibility = value; OnPropertyChanged(); }
        }

        public int MaxLines
        {
            get { return _maxLines; }
            set { _maxLines = value; OnPropertyChanged(); }
        }

        public int MinLines
        {
            get { return _minLines; }
            set { _minLines = value; OnPropertyChanged(); }
        }

        public int MaxLength
        {
            get { return _maxLength; }
            set { _maxLength = value; OnPropertyChanged(); }
        }

        public InputContentModel(AbstractModel parentModel, ISettingsManager manager)
            : base(parentModel)
        {
            _manager = manager;
            _minLines = 1;
        }

        public override T LoadContent<T>(AbstractContentWrapper wrapper)
        {
            try
            {
                var result = new TextBoxField { DataContext = this };
                InitializeSettingBinding(result, wrapper as SimpleInputField);
                InitializeValues(wrapper);
                SetValues(wrapper as SimpleInputField, result);
                Padding = wrapper.Padding.GetValueOrDefault(new Thickness(4));

                Content = result;
                return result as T;
            }
            catch (Exception e)
            {
                Log.Warn(e);
                return default(T);
            }
        }

        protected override void InitializeValues(AbstractContentWrapper wrapper)
        {
            base.InitializeValues(wrapper);
            TextBrush = AppearanceManager.GetBrushForColor(wrapper.Color, AppearanceManager.InputFieldTextBrush);
        }

        private void SetValues(SimpleInputField wrapper, TextBoxField target)
        {
            if (wrapper == null)
                return;

            MaxLines = wrapper.MaxLines;
            MinLines = wrapper.MinLines;
            IsReadOnly = wrapper.ReadOnly;
            VerticalScrollBarVisibility = wrapper.CanScroll ?
                ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;

            if (wrapper.AlwaysScrollToEnd)
                target.TextChanged += ScrollToEndOnTextChangedHandler;

            target.Text = _manager.GetTemporarySettingString(wrapper.SettingKey);
            Watermark = wrapper.Watermark;
            WatermarkBrush = AppearanceManager.InputFieldTextBrush.FadeBrush();
        }

        private void InitializeSettingBinding(FrameworkElement instance, SimpleInputField wrapper)
        {
            if (instance == null)
                return;

            if (wrapper == null)
                return;

            SettingKey = wrapper.SettingKey;

            var binding = new Binding
            {
                Converter = new SettingsConverter(this, _manager),
                ConverterParameter = wrapper.SettingKey,
                Mode = BindingMode.OneWayToSource,
                Path = new PropertyPath("SettingProperty"),
                Source = _manager.BindingProxy,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };

            // readonly field is assumed to be driven by other setting, so set two-way binding
            if (wrapper.ReadOnly)
                binding.Mode = BindingMode.TwoWay;

            instance.SetBinding(TextBox.TextProperty,
                binding);
        }



        protected override void InitializeBindings(DependencyObject target)
        {
            base.InitializeBindings(target);
            BindingOperations.SetBinding(target, TextBox.MaxLinesProperty, GetOneWayBinding(this, "MaxLines"));
            BindingOperations.SetBinding(target, TextBox.MinLinesProperty, GetOneWayBinding(this, "MinLines"));
            BindingOperations.SetBinding(target, TextBoxBase.IsReadOnlyProperty, GetOneWayBinding(this, "IsReadOnly"));
            BindingOperations.SetBinding(target, TextBoxBase.VerticalScrollBarVisibilityProperty, GetOneWayBinding(this, "VerticalScrollBarVisibility"));
        }

        private static void ScrollToEndOnTextChangedHandler(object sender, TextChangedEventArgs e)
        {
            var target = sender as TextBox;
            if (target == null)
                return;

            target.ScrollToEnd();
        }

        public void SetText(string value)
        {
            var instance = Content as TextBoxField;
            if (instance == null)
                return;

            instance.Text = value;
        }
    }
}
