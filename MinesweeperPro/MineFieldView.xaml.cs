using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MinesweeperPro.Mines;

namespace MinesweeperPro
{
    /// <summary>
    /// Interaction logic for MineFieldView.xaml
    /// </summary>
    public partial class MineFieldView : UserControl
    {
        public MineFieldView()
        {
            InitializeComponent();
        }

        private void OnCellClicked(object sender, MouseButtonEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            var field = (CellState)fe.DataContext;
            field.Open();
        }

        private void OnCellFlagged(object sender, MouseButtonEventArgs e)
        {
            var fe = (FrameworkElement) sender;
            var field = (CellState)fe.DataContext;
            field.Flag();
        }
    }
}
