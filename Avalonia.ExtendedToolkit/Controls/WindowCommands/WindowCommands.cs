﻿using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Templates;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Collections;
using Avalonia.Controls;
using System;
using Avalonia.Controls.Presenters;
using Avalonia.ExtendedToolkit.Extensions;
using Avalonia.Controls.Generators;

namespace Avalonia.ExtendedToolkit.Controls
{
    /// <summary>
    /// An Enum representing different themes for window commands.
    /// </summary>
    public enum WindowCommandTheme
    {
        Light,
        Dark
    }

    //should be toolbar but avalonia have no right now
    public class WindowCommands : HeaderedItemsControl, INotifyPropertyChanged
    {


        public WindowCommandTheme Theme
        {
            get { return (WindowCommandTheme)GetValue(ThemeProperty); }
            set { SetValue(ThemeProperty, value); }
        }


        public static readonly AvaloniaProperty ThemeProperty =
            AvaloniaProperty.Register<WindowCommands, WindowCommandTheme>(nameof(Theme));



        public ControlTemplate LightTemplate
        {
            get { return (ControlTemplate)GetValue(LightTemplateProperty); }
            set { SetValue(LightTemplateProperty, value); }
        }


        public static readonly AvaloniaProperty LightTemplateProperty =
            AvaloniaProperty.Register<WindowCommands, ControlTemplate>(nameof(LightTemplate));



        public ControlTemplate DarkTemplate
        {
            get { return (ControlTemplate)GetValue(DarkTemplateProperty); }
            set { SetValue(DarkTemplateProperty, value); }
        }


        public static readonly AvaloniaProperty DarkTemplateProperty =
            AvaloniaProperty.Register<WindowCommands, ControlTemplate>(nameof(DarkTemplate));



        public bool ShowSeparators
        {
            get { return (bool)GetValue(ShowSeparatorsProperty); }
            set { SetValue(ShowSeparatorsProperty, value); }
        }


        public static readonly AvaloniaProperty ShowSeparatorsProperty =
            AvaloniaProperty.Register<WindowCommands, bool>(nameof(ShowSeparators));



        public bool ShowLastSeparator
        {
            get { return (bool)GetValue(ShowLastSeparatorProperty); }
            set { SetValue(ShowLastSeparatorProperty, value); }
        }


        public static readonly AvaloniaProperty ShowLastSeparatorProperty =
            AvaloniaProperty.Register<WindowCommands, bool>(nameof(ShowLastSeparator));



        public int SeparatorHeight
        {
            get { return (int)GetValue(SeparatorHeightProperty); }
            set { SetValue(SeparatorHeightProperty, value); }
        }


        public static readonly AvaloniaProperty SeparatorHeightProperty =
            AvaloniaProperty.Register<WindowCommands, int>(nameof(SeparatorHeight), defaultValue: 15);





        public WindowCommands()
        {
            ThemeProperty.Changed.AddClassHandler<WindowCommands>((o, e) => OnThemeChanged(o, e));
            ShowSeparatorsProperty.Changed.AddClassHandler<WindowCommands>((o, e) => OnShowSeparatorsChanged(o, e));
            ShowLastSeparatorProperty.Changed.AddClassHandler<WindowCommands>((o, e) => OnShowLastSeparatorChanged(o, e));
            this.Initialized += WindowCommands_Initialized;
            
        }

        
        

        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);
            ResetSeparators();
        }



        private void WindowCommands_Initialized(object sender, EventArgs e)
        {
            var contentPresenter = this.TryFindParent<ContentPresenter>();
            if (contentPresenter != null)
            {
                this.SetValue(DockPanel.DockProperty, contentPresenter.GetValue(DockPanel.DockProperty));
            }

            var parentWindow = this.ParentWindow;
            if (null == parentWindow)
            {
                this.ParentWindow = this.TryFindParent<Window>();
            }
        }

        private void OnShowLastSeparatorChanged(WindowCommands o, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
            {
                return;
            }
            ResetSeparators(false);
        }

        private void OnShowSeparatorsChanged(WindowCommands o, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
            {
                return;
            }
            ResetSeparators();
        }

        private void OnThemeChanged(WindowCommands o, AvaloniaPropertyChangedEventArgs e)
        {
            // only apply theme if value is changed
            if (e.NewValue == e.OldValue)
            {
                return;
            }

            if ((WindowCommandTheme)e.NewValue == WindowCommandTheme.Light)
            {
                if (o.LightTemplate != null)
                {
                    o.SetValue(TemplateProperty, o.LightTemplate);
                }
            }
            else if ((WindowCommandTheme)e.NewValue == WindowCommandTheme.Dark)
            {
                if (o.DarkTemplate != null)
                {
                    o.SetValue(TemplateProperty, o.DarkTemplate);
                }
            }

        }

        private void ResetSeparators(bool reset = true)
        {
            if (Items.OfType<object>().Count() == 0)
            {
                return;
            }

            var windowCommandsItems = this.GetWindowCommandsItems().ToList();

            if (reset)
            {
                foreach (var windowCommandsItem in windowCommandsItems)
                {
                    windowCommandsItem.IsSeparatorVisible = ShowSeparators;
                }
            }

            var lastContainer = windowCommandsItems.LastOrDefault(i => i.IsVisible);
            if (lastContainer != null)
            {
                lastContainer.IsSeparatorVisible = ShowSeparators && ShowLastSeparator;
            }
        }

        private WindowCommandsItem GetWindowCommandsItem(object item)
        {
            var windowCommandsItem = item as WindowCommandsItem;
            if (windowCommandsItem != null)
            {
                return windowCommandsItem;
            }

            var index= ItemContainerGenerator.IndexFromContainer(item as IControl);
            return (WindowCommandsItem)ItemContainerGenerator.ContainerFromIndex(index);
            //return (WindowCommandsItem)this.ItemContainerGenerator.ContainerFromItem(item);
        }

        private IEnumerable<WindowCommandsItem> GetWindowCommandsItems()
        {
            return (from object item in (IEnumerable)this.Items select this.GetWindowCommandsItem(item)).Where(i => i != null);
        }

        private Window _parentWindow;

        public Window ParentWindow
        {
            get { return _parentWindow; }
            set
            {
                if (Equals(_parentWindow, value))
                {
                    return;
                }
                _parentWindow = value;
                //this.RaisePropertyChanged("ParentWindow");
            }
        }
    }
}