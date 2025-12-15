using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Minesweeper
{
    public partial class MainWindow : Window
    {
        const int Rows = 10;
        const int Cols = 10;
        const int MinesCount = 15;
        Cell[,] cells;
        bool gameOver;
        int openedSafeCells;
        int flaggedCells;
        DispatcherTimer timer;
        int secondsElapsed;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            StartGame();
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            secondsElapsed++;
            TimerText.Text = secondsElapsed.ToString();
        }

        void StartGame()
        {
            BoardGrid.Children.Clear();
            cells = new Cell[Rows, Cols];
            gameOver = false;
            openedSafeCells = 0;
            flaggedCells = 0;
            secondsElapsed = 0;
            StatusText.Text = "Игра идёт";
            StatusText.Foreground = new SolidColorBrush(Color.FromRgb(206, 145, 120));
            MinesCountText.Text = MinesCount.ToString();
            TimerText.Text = "0";
            timer.Stop();
            timer.Start();
            
            CreateCells();
            PlaceMines();
            CalculateNumbers();
        }

        void CreateCells()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    var button = new Button
                    {
                        Style = (Style)FindResource("CellButtonStyle"),
                        Tag = new Position(row, col)
                    };
                    button.Click += CellButton_Click;
                    button.MouseRightButtonUp += CellButton_RightClick;

                    var cell = new Cell
                    {
                        Button = button,
                        IsMine = false,
                        IsOpened = false,
                        IsFlagged = false,
                        NeighborMines = 0
                    };

                    cells[row, col] = cell;
                    BoardGrid.Children.Add(button);
                }
            }
        }

        void PlaceMines()
        {
            var random = new Random();
            int placed = 0;
            while (placed < MinesCount)
            {
                int row = random.Next(Rows);
                int col = random.Next(Cols);
                if (cells[row, col].IsMine)
                    continue;
                cells[row, col].IsMine = true;
                placed++;
            }
        }

        void CalculateNumbers()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    if (cells[row, col].IsMine)
                        continue;
                    int count = 0;
                    ForEachNeighbor(row, col, (r, c) =>
                    {
                        if (cells[r, c].IsMine)
                            count++;
                    });
                    cells[row, col].NeighborMines = count;
                }
            }
        }

        void CellButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameOver)
                return;
            var button = (Button)sender;
            var pos = (Position)button.Tag;
            var cell = cells[pos.Row, pos.Col];
            if (cell.IsOpened || cell.IsFlagged)
                return;

            if (cell.IsMine)
            {
                OpenMine(cell);
                EndGame(false);
                return;
            }

            OpenSafeCell(pos.Row, pos.Col);
            CheckWinCondition();
        }

        void CellButton_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (gameOver)
                return;
            var button = (Button)sender;
            var pos = (Position)button.Tag;
            var cell = cells[pos.Row, pos.Col];
            if (cell.IsOpened)
                return;

            if (cell.IsFlagged)
            {
                cell.IsFlagged = false;
                cell.Button.Content = "";
                flaggedCells--;
            }
            else
            {
                cell.IsFlagged = true;
                cell.Button.Content = "🚩";
                cell.Button.Foreground = new SolidColorBrush(Color.FromRgb(244, 71, 71));
                flaggedCells++;
            }

            MinesCountText.Text = (MinesCount - flaggedCells).ToString();
            CheckWinCondition();
        }

        void OpenSafeCell(int row, int col)
        {
            var cell = cells[row, col];
            if (cell.IsOpened || cell.IsFlagged)
                return;
            if (cell.IsMine)
                return;

            cell.IsOpened = true;
            openedSafeCells++;
            cell.Button.IsEnabled = false;
            cell.Button.Background = new SolidColorBrush(Color.FromRgb(37, 37, 38));

            if (cell.NeighborMines > 0)
            {
                cell.Button.Content = cell.NeighborMines.ToString();
                cell.Button.Foreground = GetColorForNumber(cell.NeighborMines);
            }
            else
            {
                cell.Button.Content = "";
                ForEachNeighbor(row, col, (r, c) =>
                {
                    if (!cells[r, c].IsOpened && !cells[r, c].IsMine)
                        OpenSafeCell(r, c);
                });
            }
        }

        void OpenMine(Cell cell)
        {
            cell.Button.Content = "💣";
            cell.Button.Background = new SolidColorBrush(Color.FromRgb(244, 71, 71));
            cell.Button.Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        }

        void RevealAllMines()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    var cell = cells[row, col];
                    if (!cell.IsMine)
                        continue;
                    if (cell.IsFlagged)
                    {
                        cell.Button.Background = new SolidColorBrush(Color.FromRgb(78, 201, 176));
                    }
                    else
                    {
                        cell.Button.Content = "💣";
                        cell.Button.Background = new SolidColorBrush(Color.FromRgb(62, 62, 66));
                    }
                    cell.Button.IsEnabled = false;
                }
            }
        }

        void CheckWinCondition()
        {
            bool allSafeCellsOpened = (openedSafeCells == Rows * Cols - MinesCount);
            bool allMinesFlagged = (flaggedCells == MinesCount);
            if (allMinesFlagged)
            {
                bool allFlagsCorrect = true;
                for (int row = 0; row < Rows; row++)
                {
                    for (int col = 0; col < Cols; col++)
                    {
                        var cell = cells[row, col];
                        if (cell.IsFlagged && !cell.IsMine)
                        {
                            allFlagsCorrect = false;
                            break;
                        }
                    }
                    if (!allFlagsCorrect)
                        break;
                }
                if (allFlagsCorrect)
                    EndGame(true);
            }
            else if (allSafeCellsOpened)
            {
                EndGame(true);
            }
        }

        void EndGame(bool win)
        {
            gameOver = true;
            timer.Stop();
            if (win)
            {
                StatusText.Text = "🎉 Победа! 😊";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(78, 201, 176));
                RevealAllMines();
            }
            else
            {
                StatusText.Text = "💥 Поражение";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(244, 71, 71));
                RevealAllMines();
            }

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    cells[row, col].Button.IsEnabled = false;
                }
            }
        }

        void ForEachNeighbor(int row, int col, Action<int, int> action)
        {
            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    if (r < 0 || c < 0 || r >= Rows || c >= Cols)
                        continue;
                    if (r == row && c == col)
                        continue;
                    action(r, c);
                }
            }
        }

        Brush GetColorForNumber(int n)
        {
            switch (n)
            {
                case 1: return new SolidColorBrush(Color.FromRgb(78, 201, 176));
                case 2: return new SolidColorBrush(Color.FromRgb(156, 220, 254));
                case 3: return new SolidColorBrush(Color.FromRgb(206, 145, 120));
                case 4: return new SolidColorBrush(Color.FromRgb(197, 134, 192));
                case 5: return new SolidColorBrush(Color.FromRgb(220, 220, 170));
                case 6: return new SolidColorBrush(Color.FromRgb(86, 156, 214));
                case 7: return new SolidColorBrush(Color.FromRgb(181, 206, 168));
                case 8: return new SolidColorBrush(Color.FromRgb(212, 212, 212));
                default: return new SolidColorBrush(Color.FromRgb(212, 212, 212));
            }
        }

        void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        struct Position
        {
            public int Row { get; }
            public int Col { get; }
            public Position(int row, int col)
            {
                Row = row;
                Col = col;
            }
        }

        class Cell
        {
            public Button Button { get; set; }
            public bool IsMine { get; set; }
            public bool IsOpened { get; set; }
            public bool IsFlagged { get; set; }
            public int NeighborMines { get; set; }
        }
    }
}
