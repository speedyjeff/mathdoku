using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Mathdoku
{
    public partial class SliderControl : UserControl
    {
        public UserInput UserInput { get; set; }

        public SolidColorBrush OnBrush;
        public SolidColorBrush OffBrush;
        public SolidColorBrush DisabledBrush;

        public event Action OnUpdate;

        public SliderControl()
        {
            InitializeComponent();

            // init
            MovementTransform = new TranslateTransform();
            LastButtonPressed = -1;
            UserInput = new UserInput(min: 1, max: 6);

            // update component
            Box.RenderTransform = MovementTransform;
        }

        public void Init(int i1, int i2, int i3, int max)
        {
            // set the max
            UserInput.MaxVal = max;
            if (max > UserInput.States.Length) throw new Exception("invalid max");

            // set it back to the middle
            Slider.Value = 50;

            // reset mid range
            MovementTransform.Y = MagicMidResetConst;

            // set text
            textBlock1.Text = i1 + "";
            textBlock2.Text = i2 + "";
            textBlock3.Text = i3 + "";

            // set initial color
            UpdateNumber(i1, rectangle1);
            UpdateNumber(i2, rectangle2);
            UpdateNumber(i3, rectangle3);

            // final init
            LastButtonPressed = -1;
        }

        #region private
        private TranslateTransform MovementTransform;
        private int LastButtonPressed;

        // constants
        private const int MagicTopResetConst = -30;  // this value is based on the size of the box
        private const int MagicBottomResetConst = 30; // this value is based on the size of the box
        private const int MagicMidResetConst = 0;

        private void UpdateNumber(int val, Rectangle rect)
        {
            switch (UserInput.States[val])
            {
                case NumberStates.Disabled:
                    rect.Fill = DisabledBrush;
                    break;
                case NumberStates.On:
                    rect.Fill = OnBrush;
                    break;
                case NumberStates.Off:
                    rect.Fill = OffBrush;
                    break;
            }
        }

        //
        // UI control
        //
        private void ContentPanel_ManipulationDelta(object sender, MouseWheelEventArgs e)
        {
            FlipControl(e.Delta);

            // notify to the parent control that I handled this event
            e.Handled = true;
        }

        private void FlipControl(double delta)
        {
            int val;

            // positive means go down
            // negative means go up

            // trans.Y = -30 - reset for top to down
            // trans.Y = 30 - reset for down to top

            if (delta < 0)
            {
                // moving UP

                while (delta < 0)
                {
                    double localDelta = delta;

                    // ensure to wrap
                    if ((delta + MovementTransform.Y) < MagicTopResetConst) localDelta = MagicTopResetConst - MovementTransform.Y;

                    MovementTransform.Y += localDelta;
                    if (MovementTransform.Y <= MagicTopResetConst)
                    {
                        // this is a transition point
                        //  1. textblock1 == textblock2 (text)
                        //  2. textblock3 == textblock3+1 (text)
                        //  3. textblock2 == textblock3 (text)
                        //  4. Box moves to bottom
                        //  5. set states appropriately

                        // text 1
                        textBlock1.Text = textBlock2.Text;
                        UpdateNumber(Convert.ToInt32(textBlock1.Text), rectangle1);

                        // text 2
                        textBlock2.Text = textBlock3.Text;
                        UpdateNumber(Convert.ToInt32(textBlock2.Text), rectangle2);
                        MovementTransform.Y = MagicBottomResetConst;

                        // text 3
                        val = Convert.ToInt32(textBlock3.Text);
                        if (val < UserInput.MaxVal) textBlock3.Text = (val + 1) + "";
                        else textBlock3.Text = UserInput.MinVal + "";
                        UpdateNumber(Convert.ToInt32(textBlock3.Text), rectangle3);
                    }

                    // shrink the delta
                    delta -= localDelta;
                }
            }
            else
            {
                while (delta > 0)
                {
                    // moving down

                    double localDelta = delta;

                    // wrap
                    if ((MovementTransform.Y + delta) > MagicBottomResetConst) localDelta = MagicBottomResetConst - MovementTransform.Y;

                    MovementTransform.Y += localDelta;
                    if (MovementTransform.Y >= MagicBottomResetConst)
                    {
                        // this is a transition point
                        //  1. textblock2 == textblock1 (text)
                        //  2. textblock3 == textblock2 (text)
                        //  3. textblock1 == textblock1-1 (text)
                        //  4. Box moves to top
                        //  5. update states

                        // text 3
                        textBlock3.Text = textBlock2.Text;
                        UpdateNumber(Convert.ToInt32(textBlock3.Text), rectangle3);

                        // text 2
                        textBlock2.Text = textBlock1.Text;
                        UpdateNumber(Convert.ToInt32(textBlock2.Text), rectangle2);
                        MovementTransform.Y = MagicTopResetConst;

                        // text 1
                        val = Convert.ToInt32(textBlock1.Text);
                        if (val > UserInput.MinVal) textBlock1.Text = (val - 1) + "";
                        else textBlock1.Text = UserInput.MaxVal + "";
                        UpdateNumber(Convert.ToInt32(textBlock1.Text), rectangle1);
                    }

                    delta -= localDelta;
                }
            }
        }

        private void rectangle1_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            FlipState_1();
        }

        private void rectangle1_MouseLeftButtonUp(object sender, MouseButtonEventArgs  e)
        {
            FlipState_1();
        }

        private void FlipState_1()
        {
            if (LastButtonPressed != 1) return;

            int val = Convert.ToInt32(textBlock1.Text);
            if (UserInput.States[val] == NumberStates.Disabled) return;
            if (UserInput.States[val] == NumberStates.On)
            {
                UserInput.States[val] = NumberStates.Off;
                if (OnUpdate != null) OnUpdate();
            }
            else
            {
                UserInput.States[val] = NumberStates.On;
                if (OnUpdate != null) OnUpdate();
            }
            if (UserInput.States[val] == NumberStates.On) rectangle1.Fill = OnBrush;
            else rectangle1.Fill = OffBrush;
        }

        private void rectangle2_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            FlipState_2();
        }

        private void rectangle2_MouseLeftButtonUp(object sender, MouseEventArgs  e)
        {
            FlipState_2();
        }

        private void FlipState_2()
        {
            if (LastButtonPressed != 2) return;

            int val = Convert.ToInt32(textBlock2.Text);
            if (UserInput.States[val] == NumberStates.Disabled) return;
            if (UserInput.States[val] == NumberStates.On)
            {
                UserInput.States[val] = NumberStates.Off;
                if (OnUpdate != null) OnUpdate();
            }
            else
            {
                UserInput.States[val] = NumberStates.On;
                if (OnUpdate != null) OnUpdate();
            }
            if (UserInput.States[val] == NumberStates.On) rectangle2.Fill = OnBrush;
            else rectangle2.Fill = OffBrush;
        }

        private void rectangle3_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            FlipState_3();
        }

        private void rectangle3_MouseLeftButtonUp(object sender, MouseEventArgs  e)
        {
            FlipState_3();
        }

        private void FlipState_3()
        {
            if (LastButtonPressed != 3) return;

            int val = Convert.ToInt32(textBlock3.Text);
            if (UserInput.States[val] == NumberStates.Disabled) return;
            if (UserInput.States[val] == NumberStates.On)
            {
                UserInput.States[val] = NumberStates.Off;
                if (OnUpdate != null) OnUpdate();
            }
            else
            {
                UserInput.States[val] = NumberStates.On;
                if (OnUpdate != null) OnUpdate();
            }
            if (UserInput.States[val] == NumberStates.On) rectangle3.Fill = OnBrush;
            else rectangle3.Fill = OffBrush;
        }

        private void rectangle1_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            LastButtonPressed = 1;
        }

        private void rectangle1_MouseLeftButtonDown(object sender, MouseEventArgs  e)
        {
            LastButtonPressed = 1;
        }

        private void rectangle2_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            LastButtonPressed = 2;
        }

        private void rectangle2_MouseLeftButtonDown(object sender, MouseEventArgs  e)
        {
            LastButtonPressed = 2;
        }

        private void rectangle3_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            LastButtonPressed = 3;
        }

        private void rectangle3_MouseLeftButtonDown(object sender, MouseEventArgs  e)
        {
            LastButtonPressed = 3;
        }

        private void Slider_ValueChanged(double delta)
        {
            FlipControl(delta);
        }
        #endregion
    }
}
