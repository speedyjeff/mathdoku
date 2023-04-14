using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mathdoku
{
    /// <summary>
    /// Interaction logic for NumberPanelControl.xaml
    /// </summary>
    public partial class NumberPanelControl : UserControl
    {
        public NumberPanelControl()
        {
            InitializeComponent();

            UserInput = new UserInput(min: 1, max: 6);
        }

        public UserInput UserInput { get; set; }
        public SolidColorBrush OnBrush;
        public SolidColorBrush OffBrush;
        public SolidColorBrush DisabledBrush;

        public event Action OnUpdate;

        public void Init(int max)
        {
            // set the max
            UserInput.MaxVal = max;
            if (max > UserInput.States.Length) throw new Exception("invalid max");

            // set the initial value highlights
            UpdateHighlight(1, Number1);
            UpdateHighlight(2, Number2);
            UpdateHighlight(3, Number3);
            UpdateHighlight(4, Number4);
            UpdateHighlight(5, Number5);
            UpdateHighlight(6, Number6);
        }

        #region private
        private void UpdateHighlight(int val, Button btn)
        {
            switch (UserInput.States[val])
            {
                case NumberStates.Disabled:
                    btn.Background = DisabledBrush;
                    break;
                case NumberStates.On:
                    btn.Background = OnBrush;
                    break;
                case NumberStates.Off:
                    btn.Background = OffBrush;
                    break;
            }
        }

        private void ToggleButtonState(int val, Button btn)
        {
            if (UserInput.States[val] == NumberStates.Disabled) return;
            // toggle the state
            if (UserInput.States[val] == NumberStates.Off) UserInput.States[val] = NumberStates.On;
            else UserInput.States[val] = NumberStates.Off;
            // update
            UpdateHighlight(val, btn);
            // fire update
            if (OnUpdate != null) OnUpdate();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ToggleButtonState(1, Number1);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ToggleButtonState(2, Number2);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ToggleButtonState(3, Number3);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ToggleButtonState(4, Number4);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ToggleButtonState(5, Number5);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            ToggleButtonState(6, Number6);
        }
        #endregion
    }
}
