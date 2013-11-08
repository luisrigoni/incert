﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Org.InCommon.InCert.Engine.UserInterface.Dialogs.Models.Mockup.ContentModels
{
    public class MockCheckboxModel
    {
        public Visibility Visibility { get { return Visibility.Visible; } }
        public Brush TextBrush { get { return new SolidColorBrush(Colors.Black); } }
        public Brush CheckBrush { get { return new SolidColorBrush(Colors.Red); } }
        public double FontSize { get { return 12; } }
        public Thickness Margin { get { return new Thickness(8); } }
        public Dock Dock { get { return Dock.Top; } }
        public FontFamily FontFamily { get { return new FontFamily("Verdana"); } }
        public string GroupName { get; set; }
        public Thickness Padding { get { return new Thickness(4, 0, 0, 0); } }
        public ICommand Command { get { return null; } }
        public string Text { get { return "Checkbox sample text"; } }
        public bool Enabled { get; set; }
    }
}
