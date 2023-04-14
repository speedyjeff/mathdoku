using System.Windows;
using System.Windows.Controls;

namespace Mathdoku
{
    public sealed partial class NewGame : UserControl
    {
        public NewGame()
        {
            InitializeComponent();
        }

        public event Action NewGameCallbackEvent;
        public string PuzzleName { get { return puzzleName.Text; } }
        public int Dimension { get { return (puzzleSize.IsChecked.Value ? 6 : 4); } }
        public bool IsAssociative { get { return (associativity.IsChecked != null && associativity.IsChecked.Value) ? true : false; } }

        public void Open()
        {
            puzzleName.Text = "";
            this.Visibility = System.Windows.Visibility.Visible;
            this.IsHitTestVisible = true;
            innerpanel.Height = HiddenHeightConst;
        }

        public void Close()
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
            this.IsHitTestVisible = false;
        }

        #region private
        private const int HiddenHeightConst = 250;
        private const int OpenHeightConst = 400;

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewGameCallbackEvent != null)
            {
                // call call back
                NewGameCallbackEvent();
                Close();

                return;
            }

            // this is a developer error
            throw new Exception("Failed to set NewGameCallbackEvent delegate");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void puzzleName_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
            // ensure the puzzle name can only be 10 chars long
            if (puzzleName.Text.Length >= 10) puzzleName.Text = puzzleName.Text.Substring(0, 10);
        }
        #endregion
    }
}
