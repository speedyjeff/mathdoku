using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Mathdoku
{
    public sealed partial class Help : UserControl
    {
        public Help()
        {
            InitializeComponent();
        }

        public void Close()
        {
            IsHitTestVisible = false;
            Visibility = System.Windows.Visibility.Collapsed;
        }

        #region private
        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
    }
}
