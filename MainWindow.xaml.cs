
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace shashki
{
    public partial class MainWindow : Window
    {
        private const int BoardSize = 8;
        private readonly double CellSize = 600 / BoardSize;
        private Ellipse selectedPiece = null;
        private Point selectedPosition;
        private bool isWhiteTurn = true;
        private Dictionary<Point, Ellipse> pieces = new Dictionary<Point, Ellipse>();
        private List<Point> highlightedMoves = new List<Point>();
        private Dictionary<Point, List<Point>> jumpPaths = new Dictionary<Point, List<Point>>();

        public MainWindow()
        {
            InitializeComponent();
        }

        

        private void FindJumpsRecursive(Point currentPos, Ellipse piece, int[] direction, List<Point> capturedSoFar)
        {
            int newX = (int)currentPos.X + direction[0];
            int newY = (int)currentPos.Y + direction[1];

            if (!IsInside(newX, newY)) return;

            if (!pieces.ContainsKey(new Point(newX, newY))) return;

            var enemy = pieces[new Point(newX, newY)];
            if (enemy.Fill == piece.Fill) return;

            int jumpX = newX + direction[0];
            int jumpY = newY + direction[1];

            if (!IsInside(jumpX, jumpY) || pieces.ContainsKey(new Point(jumpX, jumpY))) return;

            var newCaptured = new List<Point>(capturedSoFar) { new Point(newX, newY) };

            var nextPos = new Point(jumpX, jumpY);
            var newPath = new List<Point>(newCaptured);

            jumpPaths[nextPos] = newPath;

            int[][] continueDirs = (piece.Tag as string) == "Queen"
                ? new[] { new[] { -1, -1 }, new[] { 1, -1 }, new[] { -1, 1 }, new[] { 1, 1 } }
                : new[] { new[] { -1, (piece.Fill == Brushes.White ? -1 : 1) }, new[] { 1, (piece.Fill == Brushes.White ? -1 : 1) } };

            foreach (var d in continueDirs)
            {
                FindJumpsRecursive(nextPos, piece, d, newPath);
            }
        }

       

        

        private void Highlight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var highlight = sender as FrameworkElement;
            if (highlight?.Tag is Point target)
            {
                MovePiece(target);
            }
        }

        private void MovePiece(Point target)
        {
            if (selectedPiece == null || !highlightedMoves.Contains(target)) return;

            if (jumpPaths.TryGetValue(target, out var captured))
            {
                foreach (var point in captured)
                {
                    if (pieces.ContainsKey(point))
                    {
                        BoardCanvas.Children.Remove(pieces[point]);
                        pieces.Remove(point);
                    }
                }
            }

            Canvas.SetLeft(selectedPiece, target.X * CellSize + CellSize * 0.1);
            Canvas.SetTop(selectedPiece, target.Y * CellSize + CellSize * 0.1);

            pieces.Remove(selectedPosition);
            pieces[target] = selectedPiece;

            if ((selectedPiece.Fill == Brushes.White && target.Y == 0) ||
                (selectedPiece.Fill == Brushes.Black && target.Y == BoardSize - 1))
            {
                selectedPiece.Stroke = Brushes.Gold;
                selectedPiece.StrokeThickness = 3;
                selectedPiece.Tag = "Queen";
            }

            ClearHighlights();
            selectedPiece = null;

            isWhiteTurn = !isWhiteTurn;
        }

        private bool IsInside(int x, int y)
        {
            return x >= 0 && y >= 0 && x < BoardSize && y < BoardSize;
        }

        private void ClearHighlights()
        {
            for (int i = BoardCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (BoardCanvas.Children[i] is Ellipse el && el.Fill == Brushes.LightBlue)
                {
                    el.MouseDown -= Highlight_MouseDown;
                    BoardCanvas.Children.RemoveAt(i);
                }
            }
            highlightedMoves.Clear();
        }
    }
}
