using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Mathdoku
{
    public sealed partial class MathdokuGame : UserControl
    {
        public MathdokuGame()
        {
            InitializeComponent();

            // great a gradient of colors for the regions (index by the GroupID)
            GroupColors = new SolidColorBrush[10];
            var theme = (themefill.Fill as SolidColorBrush);
            var R = theme.Color.R;
            var G = theme.Color.G;
            var B = theme.Color.B;
            if (!UseDarkMode)
            {
                var diff = LightThemeConst; // use light diff

                for (int i = 0; i < GroupColors.Length; i++)
                {
                    GroupColors[i] = new SolidColorBrush(Color.FromArgb(255, R, G, B));
                    if (R == 0) R = 10;
                    if (G == 0) G = 10;
                    if (B == 0) B = 10;

                    if (R < G && R < B) R = (byte)(R * diff);
                    else if (G < B) G = (byte)(G * diff);
                    else B = (byte)(B * diff);
                }
            }
            else
            {
                var diff = DarkThemeConst; // use dark diff

                for (int i = 0; i < GroupColors.Length; i++)
                {
                    GroupColors[i] = new SolidColorBrush(Color.FromArgb(255, R, G, B));
                    R = (byte)(R * diff);
                    G = (byte)(G * diff);
                    B = (byte)(B * diff);
                }
            }

            // initialize
            History = new BoardHistory();
            SmallTextStyle = smallText.Style;
            LargeTextStyle = largeText.Style;
            NormalBrush = (SolidColorBrush)normalstroke.Stroke;
            ErrorBrush = (SolidColorBrush)errorstroke.Stroke;
            Rand = new Random();

            // initialize getting user input
            if (UseSliderInput)
            {
                InputTransform = new TranslateTransform();
                sliderInput.RenderTransform = InputTransform;
                sliderInput.OffBrush = (onfill.Fill as SolidColorBrush);
                sliderInput.OnBrush = (themefill.Fill as SolidColorBrush);
                sliderInput.DisabledBrush = (disabledfill.Fill as SolidColorBrush);

                sliderInput.OnUpdate += AcceptInput;
            }
            else
            {
                // number panel input
                numPanelInput.Visibility = Visibility.Visible;
                numPanelInput.OffBrush = (onfill.Fill as SolidColorBrush);
                numPanelInput.OnBrush = (themefill.Fill as SolidColorBrush);
                numPanelInput.DisabledBrush = (disabledfill.Fill as SolidColorBrush);

                numPanelInput.OnUpdate += AcceptInput;
            }

            // setup the callback for new game
            newgamepanel.NewGameCallbackEvent += CreateNewBoard;

            // init a new board
            NewBoard(new KenKenOptions()
            {
                Seed = Rand.Next(),
                PuzzleName = "",
                IsAssociative = true,
                Dimension = 6,
                StartingHint = 0.1f
            });
        }

        public bool UseDarkMode { get; set; } = true;
        public bool UseSliderInput { get; set; } = false;

        #region private
        private KenKenBoard Board;
        private Random Rand;
        private List<int>[][] UserInputBoard;
        private TranslateTransform InputTransform;
        private BoardHistory History;

        private const string TitlePrefix = "title";
        private const string ValuePrefix = "value";
        private const string RectanglePrefix = "rectangle";

        private const double LightThemeConst = 6;
        private const double DarkThemeConst = 0.75;
        private const int MaxDimension = 6;

        private SolidColorBrush[] GroupColors;
        private Style SmallTextStyle;
        private Style LargeTextStyle;
        private SolidColorBrush NormalBrush;
        private SolidColorBrush ErrorBrush;

        //
        // Primary logic
        //
        private void NewBoard(KenKenOptions options)
        {
            Board = new KenKenBoard(options);

            // capture user input
            UserInputBoard = new List<int>[options.Dimension][];
            for (int y = 0; y < UserInputBoard.Length; y++)
            {
                UserInputBoard[y] = new List<int>[options.Dimension];
                for (int x = 0; x < UserInputBoard[y].Length; x++)
                {
                    UserInputBoard[y][x] = new List<int>();
                }
            }

            // set puzzle seed
            if (options.PuzzleName != null && options.PuzzleName != "")
                seedtext.Text = Board.Options.Seed + " (" + Board.Options.PuzzleName + ")";
            else
                seedtext.Text = Board.Options.Seed + "";

            // init board
            if (Board.Options.Dimension == 6)
            {
                var visited = new HashSet<int>();
                for (int y = 0; y < options.Dimension; y++)
                {
                    for (int x = 0; x < options.Dimension; x++)
                    {
                        // cell title
                        if (!visited.Contains(Board.BoardData[y][x].RegionID))
                        {
                            UILookup<TextBlock>(TitlePrefix, y, x).Text = Board.BoardData[y][x].Total + " " + Board.BoardData[y][x].Operator;
                            visited.Add(Board.BoardData[y][x].RegionID);
                        }
                        else
                        {
                            UILookup<TextBlock>(TitlePrefix, y, x).Text = "";
                        }

                        // value
                        DisplayCellValues(y, x, UserInputBoard[y][x]);

                        // rectangle background color
                        UILookup<Rectangle>(RectanglePrefix, y, x).Fill = GroupColors[Board.BoardData[y][x].GroupID];
                        UILookup<Rectangle>(RectanglePrefix, y, x).IsHitTestVisible = true;
                    }
                }
            }
            else if (Board.Options.Dimension == 4)
            {
                var visited = new HashSet<int>();
                for (int y = 0; y < options.Dimension; y++)
                {
                    for (int x = 0; x < options.Dimension; x++)
                    {
                        // cell title
                        if (!visited.Contains(Board.BoardData[y][x].RegionID))
                        {
                            UILookup<TextBlock>(TitlePrefix, y + 1, x + 1).Text = Board.BoardData[y][x].Total + " " + Board.BoardData[y][x].Operator;
                            visited.Add(Board.BoardData[y][x].RegionID);
                        }
                        else
                        {
                            UILookup<TextBlock>(TitlePrefix, y + 1, x + 1).Text = "";
                        }

                        // value
                        DisplayCellValues(y + 1, x + 1, UserInputBoard[y][x]);

                        // rectangle background color
                        UILookup<Rectangle>(RectanglePrefix, y + 1, x + 1).Fill = GroupColors[Board.BoardData[y][x].GroupID];
                        UILookup<Rectangle>(RectanglePrefix, y + 1, x + 1).IsHitTestVisible = true;
                    }
                }

                // clear the boarders
                for (int y = 0; y < MaxDimension; y++)
                {
                    for (int x = 0; x < MaxDimension; x++)
                    {
                        if (y == 0 || x == 0 || y > Board.Options.Dimension || x > Board.Options.Dimension)
                        {
                            UILookup<TextBlock>(ValuePrefix, y, x).Text = "";
                            UILookup<TextBlock>(TitlePrefix, y, x).Text = "";
                            UILookup<Rectangle>(RectanglePrefix, y, x).Fill = disabledfill.Fill;
                            UILookup<Rectangle>(RectanglePrefix, y, x).IsHitTestVisible = false;
                            HighlightCell(y, x, true);
                        }
                    }
                }
            }
            else
            {
                throw new Exception("unknown board size");
            }

            // clear history
            History.Clear();

            // add a series of hints to the board to get it started
            if (options.StartingHint > 0 && options.StartingHint < 1f)
            {
                var numStartingHints = (int)Math.Ceiling((options.Dimension * options.Dimension) * options.StartingHint);
                while (numStartingHints-- > 0) GiveHint();
            }
        }

        private void SetCellValues(int y, int x, int value)
        {
            UserInputBoard[y][x].Clear();
            UserInputBoard[y][x].Add(value);
            if (Board.Options.Dimension == 6)
                DisplayCellValues(y, x, UserInputBoard[y][x]);
            else
                DisplayCellValues(y + 1, x + 1, UserInputBoard[y][x]);
        }

        private void DisplayCellValues(int y, int x, List<int> values)
        {
            if (values.Count == 0)
            {
                UILookup<TextBlock>(ValuePrefix, y, x).Text = "";
            }
            else if (values.Count == 1)
            {
                UILookup<TextBlock>(ValuePrefix, y, x).Text = values[0] + "";
                UILookup<TextBlock>(ValuePrefix, y, x).Style = LargeTextStyle;
            }
            else
            {
                var value = "";
                for (int i = 0; i < values.Count; i++)
                {
                    value += values[i];
                    if (i < values.Count - 1)
                    {
                        value += ",";
                        if (i == 2) value += "\n";
                    }
                }
                UILookup<TextBlock>(ValuePrefix, y, x).Text = value;
                UILookup<TextBlock>(ValuePrefix, y, x).Style = SmallTextStyle;
            }
            UILookup<Rectangle>(RectanglePrefix, y, x).Stroke = NormalBrush;
        }

        private void GiveHint()
        {
            var cells = new List<Tuple<int,int>>();
            var cellsWithGuesses = new List<Tuple<int, int>>();

            // identify cells that have no guesses, and those that have multiple
            for (int y = 0; y < Board.Options.Dimension; y++)
            {
                for (int x = 0; x < Board.Options.Dimension; x++)
                {
                    // this cell is appropriate for a hint
                    if (UserInputBoard[y][x].Count == 0) cells.Add(new Tuple<int,int>(x, y));
                    if (UserInputBoard[y][x].Count > 1) cellsWithGuesses.Add(new Tuple<int, int>(x, y));
                }
            }

            // find cells that are obvious for a hint (eg. no guess)
            if (cells.Count > 0)
            {
                // pick a cell and provide a hint
                int index = Rand.Next() % cells.Count;
                var x = cells[index].Item1;
                var y = cells[index].Item2;

                // save for undo/redo (empty)
                var values = new List<int>() { Board.BoardData[y][x].Value };
                if (Board.Options.Dimension == 6)
                    History.Add(y, x, UserInputBoard[y][x], values);
                else
                    History.Add(y+1, x+1, UserInputBoard[y][x], values);

                // set value
                SetCellValues(y, x, Board.BoardData[y][x].Value);

                return;
            }

            if (cellsWithGuesses.Count > 0)
            {
                // pick a cell and provide a hint
                int index = Rand.Next() % cellsWithGuesses.Count;
                var x = cellsWithGuesses[index].Item1;
                var y = cellsWithGuesses[index].Item2;

                // save for undo/redo
                var values = new List<int>() { Board.BoardData[y][x].Value };
                if (Board.Options.Dimension == 6)
                    History.Add(y, x, UserInputBoard[y][x], values);
                else
                    History.Add(y+1, x+1, UserInputBoard[y][x], values);

                // set new value
                SetCellValues(y, x, Board.BoardData[y][x].Value);

                return;
            }

            // no option for a hint
        }

        private void HighlightCell(int y, int x, bool valid)
        {
            if (valid)
                UILookup<Rectangle>(RectanglePrefix, y, x).Stroke = NormalBrush;
            else
                UILookup<Rectangle>(RectanglePrefix, y, x).Stroke = ErrorBrush;
        }

        private void ValidateBoard()
        {
            if (Board.Options.Dimension == 6)
            {
                for (int y = 0; y < Board.Options.Dimension; y++)
                {
                    for (int x = 0; x < Board.Options.Dimension; x++)
                    {
                        if (UserInputBoard[y][x].Count == 0 || UserInputBoard[y][x].Count > 1)
                            HighlightCell(y, x, valid: false);
                        else if (UserInputBoard[y][x][0] == Board.BoardData[y][x].Value)
                            HighlightCell(y, x, valid: true);
                        else
                            HighlightCell(y, x, valid: false);
                    }
                }
            }
            else
            {
                for (int y = 0; y <  Board.Options.Dimension; y++)
                {
                    for (int x = 0; x < Board.Options.Dimension; x++)
                    {
                        if (UserInputBoard[y][x].Count == 0 || UserInputBoard[y][x].Count > 1)
                            HighlightCell(y+1, x+1, valid: false);
                        else if (UserInputBoard[y][x][0] == Board.BoardData[y][x].Value)
                            HighlightCell(y+1, x+1, valid: true);
                        else
                            HighlightCell(y+1, x+1, valid: false);
                    }
                }
            }
        }

        private void AcceptInput()
        {
            // accept any input that may be ready
            if (UseSliderInput)
            {
                if (sliderInput.Visibility == Visibility.Visible) AcceptInput(sliderInput.UserInput);
            }
            else
            {
                AcceptInput(numPanelInput.UserInput);
            }
        }

        private void AcceptInput(UserInput user)
        {
            List<int> values;
            int lY, lX;

            // convert to local coordinates for 4x4 (as the whole board is shifted to the middle)
            lY = user.Y;
            lX = user.X;
            if (Board.Options.Dimension == 4)
            {
                lY--;
                lX--;
            }

            // store old values
            values = new List<int>();
            foreach (int val in UserInputBoard[lY][lX]) values.Add(val);

            // update user input
            UserInputBoard[lY][lX].Clear();
            for (int i = 1; i < user.States.Length; i++)
            {
                if (user.States[i] == NumberStates.On)
                    UserInputBoard[lY][lX].Add(i);
            }

            // save for undo/redo
            History.Add(user.Y, user.X, values, UserInputBoard[lY][lX]);

            // make UI updates
            DisplayCellValues(user.Y, user.X, UserInputBoard[lY][lX]);
        }

        private void ClickStarted(int gridX, int gridY)
        {
            var lY = gridY;
            var lX = gridX;

            // convert to local coordinates for 4x4 (as the whole board is shifted to the middle)
            if (Board.Options.Dimension == 4)
            {
                lY--;
                lX--;
            }

            UserInput user = null;
            if (UseSliderInput) user = sliderInput.UserInput;
            else user = numPanelInput.UserInput;

            // gather data
            for (int i = 0; i < user.States.Length; i++)
            {
                if (i > user.MaxVal) user.States[i] = NumberStates.Disabled;
                else user.States[i] = NumberStates.Off;
            }
            foreach (int i in UserInputBoard[lY][lX]) user.States[i] = NumberStates.On;

            // prepare the input device
            user.X = gridX;
            user.Y = gridY;

            if (UseSliderInput)
            {
                // flip the visibility
                if (sliderInput.Visibility == Visibility.Collapsed)
                {
                    sliderInput.OnBrush = GroupColors[Board.BoardData[lY][lX].GroupID];
                    sliderInput.Init(Board.Options.Dimension, 1, 2, Board.Options.Dimension);

                    // move the input device and make visible
                    InputTransform.Y = (gridY % MaxDimension) * 75;
                    InputTransform.X = (gridX - 3) * 75 + (75 / 2);

                    sliderInput.Visibility = Visibility.Visible;
                }
                else
                {
                    sliderInput.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // todo
                numPanelInput.OnBrush = GroupColors[Board.BoardData[lY][lX].GroupID];
                numPanelInput.Init(Board.Options.Dimension);
            }
        }

        private void Undo()
        {
            int y = -1, x = -1;
            int lY, lX;
            List<int> values = new List<int>();

            if (History.Undo(ref y, ref x, ref values))
            {
                // convert to local for 4x4
                lY = y;
                lX = x;
                if (Board.Options.Dimension == 4)
                {
                    lY--;
                    lX--;
                }

                // update user input
                UserInputBoard[lY][lX].Clear();
                foreach (int val in values) UserInputBoard[lY][lX].Add(val);

                // make UI updates
                DisplayCellValues(y, x, UserInputBoard[lY][lX]);
            }
        }

        private void Redo()
        {
            int y = -1, x = -1;
            int lY, lX;
            List<int> values = new List<int>();

            if (History.Redo(ref y, ref x, ref values))
            {
                // convert to local for 4x4
                lY = y;
                lX = x;
                if (Board.Options.Dimension == 4)
                {
                    lY--;
                    lX--;
                }

                // update user input
                UserInputBoard[lY][lX].Clear();
                foreach (int val in values) UserInputBoard[lY][lX].Add(val);

                // make UI updates
                DisplayCellValues(y, x, UserInputBoard[lY][lX]);
            }
        }

        //
        // UI Utilities
        //
        private T UILookup<T>(string name, int y, int x)
        {
            return (T)this.FindName(name + y + "x" + x);
        }

        private void ParseName(Rectangle rect, out int y, out int x)
        {
            string[] parts;
            parts = rect.Name.Replace(RectanglePrefix, "").Split(new char[] {'x'});

            y = Convert.ToInt32(parts[0]);
            x = Convert.ToInt32(parts[1]);
        }

        private void CreateNewBoard()
        {
            // grab the input from the user
            var boardOptions = new KenKenOptions()
            {
                PuzzleName = newgamepanel.PuzzleName,
                Seed = !string.IsNullOrWhiteSpace(newgamepanel.PuzzleName) ? KenKenOptions.PuzzleNameToSeed(newgamepanel.PuzzleName) : Rand.Next(),
                IsAssociative = newgamepanel.IsAssociative,
                Dimension = newgamepanel.Dimension,
                StartingHint = 0.1f
            };

            // clean up the input
            var user = UseSliderInput ? sliderInput.UserInput : numPanelInput.UserInput;
            for(int i =0; i<user.States.Length; i++)
            {
                if (i > boardOptions.Dimension) user.States[i] = NumberStates.Disabled;
                else user.States[i] = NumberStates.Off;
            }
            if (UseSliderInput) sliderInput.Init(boardOptions.Dimension, 1, 2, boardOptions.Dimension);
            else numPanelInput.Init(boardOptions.Dimension);

            // create new board
            NewBoard(boardOptions);
        }

        //
        // UI Handlers
        //
        public void newBoard_Click(object sender, RoutedEventArgs e)
        {
            // open the new board dialog
            newgamepanel.Open();
        }

        private void hintBoard_Click(object sender, RoutedEventArgs e)
        {
            GiveHint();
        }

        private void validateBoard_Click(object sender, RoutedEventArgs e)
        {
            ValidateBoard();
        }

        private void help_Click(object sender, RoutedEventArgs e)
        {
            // show help
            helppanel.IsHitTestVisible = true;
            helppanel.Visibility = Visibility.Visible;
        }

        private void undo_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void redo_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        private void generic_PointerPressed(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                ParseName(rect, out int y, out int x);
                ClickStarted(x, y);
            }
        }
        #endregion
    }
}
