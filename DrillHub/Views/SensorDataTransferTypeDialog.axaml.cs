using Avalonia.Controls;
using DrillHub.Vidgets;
using FluentAvalonia.UI.Media.Animation;
using System;

namespace DrillHub.Views
{
    public partial class SensorDataTransferTypeDialog : UserControl
    {
        public SensorDataTransferTypeDialog()
        {
            InitializeComponent();

            TabHost.SelectionChanged += TabHost_SelectionChanged;
            TabHost_SelectionChanged(null, null);

        }

        private void TabHost_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var idx = TabHost.SelectedIndex;

            ColorsPageFrame.Navigate(idx switch
            {
                0 => typeof(TransportTypeTCP),
                1 => typeof(TransportTypeOPC),
                2 => typeof(TransportTypeETP),
                3 => typeof(TransportTypeUDP),
                4 => typeof(TransportTypeHTTP),
                5 => typeof(TransportTypeMODBUS),
                _ => throw new ArgumentOutOfRangeException()
            }, null, GetTransitionInfo(_oldIndex, idx));

            _oldIndex = idx;
        }

        private SlideNavigationTransitionEffect GetEffect(int oldIndex, int index)
        {
            if (oldIndex < 0)
                return SlideNavigationTransitionEffect.FromBottom;

            if (oldIndex > index)
                return SlideNavigationTransitionEffect.FromRight;
            else
                return SlideNavigationTransitionEffect.FromLeft;
        }

        private NavigationTransitionInfo GetTransitionInfo(int oldIndex, int index)
        {
            if (oldIndex == -1)
            {
                return new SuppressNavigationTransitionInfo();
            }
            else
            {
                return new SlideNavigationTransitionInfo
                {
                    Effect = GetEffect(oldIndex, index)
                };
            }
        }

        private int _oldIndex = -1;

    }
}
