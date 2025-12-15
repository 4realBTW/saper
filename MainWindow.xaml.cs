using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Minesweeper
{
    public partial class MainWindow : Window
    {
        const int Rows = 10;
        const int Cols = 10;
        const int MinesCount = 15;
        Cell[,] cells;

        public MainWindow()
        {
            InitializeComponent();
            StartGame();
        }

        void StartGame()
        {
            BoardGrid.Children.Clear();
            cells = new Cell[Rows, Cols];
            StatusText.Text = "Игра идёт";
            StatusText.Foreground = new SolidColorBrush(Color.FromRgb(206, 145, 120));

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
