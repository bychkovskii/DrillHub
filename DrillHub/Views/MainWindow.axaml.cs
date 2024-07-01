using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using DrillHub.ViewModels;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using System;
using System.Collections.Generic;
using Tmds.DBus.Protocol;

namespace DrillHub.Views
{
    public partial class MainWindow : Window
    {
        private Frame _frameContent;
        private StackPanel _titleBar;

        ListBox lstBxbNavMenu;

        public MainWindow()
        {
            InitializeComponent();

            _frameContent = this.FindControl<Frame>("MainContent");
            _frameContent?.Navigate(typeof(StatusMonitorView), null, new SuppressNavigationTransitionInfo());

            lstBxbNavMenu = this.FindControl<ListBox>("NavigationMenu");
            lstBxbNavMenu.SelectionChanged += OnListBoxSelectionChanged;

            lstBxbNavMenu.SelectedIndex = 0;
        }

        public void btnShowLog_OnClick(object sender, RoutedEventArgs args)
        {
            SideBarMenu.IsPaneOpen = !SideBarMenu.IsPaneOpen;
        }


        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _titleBar = this.FindControl<StackPanel>("TopPanelTitleBar");
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (Equals(e.Source, _titleBar))
            {
                BeginMoveDrag(e);
            }
        }

        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as ListBox).SelectedIndex)
            {
                case 0:
                    _frameContent.Navigate(typeof(StatusMonitorView));
                    Item1.Opacity = 10;
                    Item2.Opacity = 90;
                    Item3.Opacity = 90;
                    break;

                case 1:
                    _frameContent.Navigate(typeof(SensorsDataView));
                    Item1.Opacity = 90;
                    Item2.Opacity = 10;
                    Item3.Opacity = 90;
                    break;

                case 2:
                    _frameContent.Navigate(typeof(WitsmlObjectsView));
                    Item1.Opacity = 90;
                    Item2.Opacity = 90;
                    Item3.Opacity = 10;

                    break;
            }
        }

    }

}