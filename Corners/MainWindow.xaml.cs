using Corners.Controls;
using Corners.Game;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Corners
{
    public partial class MainWindow : Window
    {
        #region Поля
        private Board Board; //Игровая доска (не визуальная часть)
        private const int THICKNESS = 1; //Размер кисти для рисования обычных границ
        private const int CHOOSE_THICKNESS = 5; //Размер кисти для обозначения границ выбранного элемента
        private readonly Brush CHOOSE_BRUSH = Brushes.Green; //Цвет кисти для рисования выбранного элемента
        private readonly Brush USSUALY_BRUSH = Brushes.Gray; //Цвет кисти для обозначения обычных границ
        #endregion

        #region Конструкторы
        public MainWindow()
        {
            InitializeComponent();
            BeginNewGame(3,8); //Сразу начинаем новую игру (3 - размер квадрата фишек, 8 размер самого поля)
        }
        #endregion

        #region Методы
        //Подсветка
        private void DrawHighlighting(int x, int y, Brush brush, int thickness)
        {
            foreach (var item in BoardGrid.Children)
            {
                //Находим необходимую ячейку по указаннум координатам
                if (item is Rectangle && Grid.GetColumn((Rectangle)item) == x && Grid.GetRow((Rectangle)item) == y)
                {
                    (item as Rectangle).Stroke = brush; //Меняем цвет обводки
                    (item as Rectangle).StrokeThickness = thickness; //Меняем толщину обводки
                    return;
                }
            }
        }

        //Изменение расположения фишек
        private void ChangeCheckerEllipse(int beginX, int beginY, int endX, int endY)
        {
            foreach (var item in BoardGrid.Children)
            {
                //Находим необходимое изображение фишки (Ellipse)
                if (item is Ellipse && Grid.GetColumn((Ellipse)item) == beginX && Grid.GetRow((Ellipse)item) == beginY)
                {
                    //Меняем координаты
                    Grid.SetColumn((UIElement)item, endX);
                    Grid.SetRow((UIElement)item, endY);
                    return;
                }
            }
        }

        //Показ основного Grid
        public void ChangeContentToMain()
        {
            Content = MainGrid;
        }

        //Создание новой игры
        internal void BeginNewGame(int square, int size, string player1 = "Player1", string player2 = "Player2", GameColor firstTurn = GameColor.White)
        {
            Board = new Board(square, size, player1, player2, firstTurn);
            Refresh();
        }

        //Полное обновление (без обновления доски)
        private void Refresh()
        {
            RefreshBoard();
            RefreshCheckers();
            RefreshHistory();
            RefreshNames();
            RefreshHighlighings();
        }

        //Полное обновление подсвечивания
        private void RefreshHighlighings()
        {
            foreach (var item in BoardGrid.Children) //Удаляем все подсвечивания
            {
                if (item is Rectangle)
                {
                    (item as Rectangle).Stroke = USSUALY_BRUSH;
                    (item as Rectangle).StrokeThickness = THICKNESS;
                }
            }

            if (Board.IsChecked) //Если есть выбранная фишка
            {
                foreach (var item in Board.CheckedCheckerMoves) //Включаем подсвечивание доступных ходов
                {
                    DrawHighlighting(item.Item1, item.Item2, CHOOSE_BRUSH, CHOOSE_THICKNESS);
                }
            }
        }

        //Полное обновление имен игроков
        private void RefreshNames()
        {
            //Обновляем имена игроков
            Player1TextBox.Text = "Черные: " + Board.Player1;
            Player2TextBox.Text = "Белые: " + Board.Player2;
            WhosTurnTextBlock.Text = "Очередь хода: " + (Board.TurnColor == GameColor.Black ? Board.Player1 : Board.Player2);
        }

        //Полное обновление показа истории партии
        private void RefreshHistory()
        {
            HistoryListBox.ItemsSource = Board.HistoryCollection; //Привязываем ListBox к HistoryCollection

            Board.HistoryCollection.CollectionChanged += (o, a) => //При изменении коллекции выполняем следующие действия:
            {
                TurnCountTextBlock.Text = "Ход: " + (Board.HistoryCollection.Count + 1).ToString();
            };

            //Обновление счетчика ходов
            TurnCountTextBlock.Text = "Ход: " + (Board.HistoryCollection.Count + 1).ToString();
        }

        //Полное обновление изображений всех фишек
        private void RefreshCheckers()
        {
            var temp = Board.Checkers; //Для удобства

            var removesEllipse = new List<UIElement>(); //Для того чтобы запомнить ВСЕ ссылки (по другому ссылки теряются)

            for (int i = 0; i < BoardGrid.Children.Count; i++) //Проццесс запоминания ссылок на все изображения фишек
            {
                if (BoardGrid.Children[i] is Ellipse)
                {
                    removesEllipse.Add(BoardGrid.Children[i]);
                }
            }

            foreach (var item in removesEllipse) //Процесс удаления всех изображений фишек с доски
            {
                BoardGrid.Children.Remove(item);
            }

            for (int i = 0; i < Board.BOARD_SIZE; i++) //Добавление изображения фишек по местоположении в классе Board
            {
                for (int j = 0; j < Board.BOARD_SIZE; j++)
                {
                    if (temp[i, j] != null) //Нашли фишку (не пустую ячейку)
                    {
                        var ellipse = new Ellipse() //Создаем для этой фишки изображение
                        {
                            Fill = temp[i, j] == GameColor.White ? Brushes.Silver : Brushes.Black, //Выбираем цвет фишки (Белый, Черный)
                            Stroke = USSUALY_BRUSH, //Цвет границы
                            StrokeThickness = THICKNESS //Ширина обводки границы
                        };

                        //Местоположение в Grid
                        Grid.SetColumn(ellipse, i);
                        Grid.SetRow(ellipse, j);

                        //Добавление в Grid изображения фишки
                        BoardGrid.Children.Add(ellipse);
                    }
                }
            }
        }

        //Обновление игровой доски
        private void RefreshBoard()
        {
            BoardGrid.Children.Clear();
            BoardGrid.ColumnDefinitions.Clear();
            BoardGrid.RowDefinitions.Clear();

            //Добавление Row и Column
            for (int i = 0; i < Board.BOARD_SIZE; i++)
            {
                BoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
                BoardGrid.RowDefinitions.Add(new RowDefinition());
            }

            //Добавление Rectangle (раскраска ячеек)
            for (int i = 0; i < Board.BOARD_SIZE; i++)
            {
                for (int j = 0; j < Board.BOARD_SIZE; j++)
                {
                    //Прямоугольник
                    var rect = new Rectangle
                    {
                        //Задаем цвет в зависимости от положения (чередование цветов)
                        Fill = (i + j) % 2 == 0 ? Brushes.Chocolate : Brushes.AntiqueWhite,
                        Stroke = USSUALY_BRUSH,
                        StrokeThickness = THICKNESS
                    };

                    //Задаем координаты прямоугольника в Grid
                    Grid.SetColumn(rect, i);
                    Grid.SetRow(rect, j);

                    //Добавление в Grid
                    BoardGrid.Children.Add(rect);
                }
            }
        }
        #endregion

        #region Обработчики событий
        //Выход
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //О разработчике
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ВУЗ: Сибирский государственный университет путей сообщения" +
                "\nФакультет: Бизнес-информатика\n" +
                "Группа: БИСТ-211" +
                "\nСтудент: Чамзы Кыргыс Эресович" +
                "\n\nНовосибирск 2019-2020");
        }

        //Справка
        private void Info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Цель игры переставить все свои шашки в дом соперника." +
                "\nИгрок, сделавший это первым, выигрывает.\nКаждый игрок может за один ход переместить одну шашку." +
                "\nШашки можно перемещать в любом направлении на соседнюю пустую клетку, шашки могут перепрыгивать через " +
                "свои и чужие шашки." +
                "Перепрыгивать можно по вертикали и по горизонтали, если за шашкой есть пустая клетка. Прыжки могут " +
                "быть многократными, при этом перепрыгивать шашка может как свои шашки, так и шашки противника. Длина прыжка" +
                " не принудительна, то есть игрок может решить в любое время прекратить многократный ход.",
                "Справка",
                MessageBoxButton.OK,
                MessageBoxImage.Question);
        }

        //Клик по игровой доске
        private void BoardGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Достаем координаты куда нажали
            int x = Grid.GetColumn((UIElement)e.OriginalSource);
            int y = Grid.GetRow((UIElement)e.OriginalSource);

            if (Board.IsChecked) //Если выбрана фишка
            {
                foreach (var item in Board.CheckedCheckerMoves) //Отключение подсвечивания
                {
                    DrawHighlighting(item.Item1, item.Item2, USSUALY_BRUSH, THICKNESS);
                }

                if (Board.TryMove(x, y)) //Попытка движения
                {
                    ChangeCheckerEllipse(Board.CheckedChecker.Item1, Board.CheckedChecker.Item2, x, y);
                    WhosTurnTextBlock.Text = "Очередь хода: " + (Board.TurnColor == GameColor.Black ? Board.Player1 : Board.Player2);
                    if (Board.WinColor != null)
                    {
                        var control = new WinControl(this, Board);
                        Content = control;
                    }
                    return;
                }
            }

            if (Board.TryCheck(x, y)) //Если не получилось передвинуть или фишка еще не выбрана, пробуем выбрать
            {
                foreach (var item in Board.CheckedCheckerMoves) //Если удачно выбрали фишку, то включаем подсвечивание доступных ходов
                {
                    DrawHighlighting(item.Item1, item.Item2, CHOOSE_BRUSH, CHOOSE_THICKNESS);
                }
            }
        }

        //Новая игра
        private void NewGameBtn_Click(object sender, RoutedEventArgs e)
        {
            var control = new NamesControl(this);
            Content = control;
        }

        //Загрузка игры
        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            var bf = new BinaryFormatter();
            var ofd = new OpenFileDialog { Filter = "Файлы игры|*.save|Все файлы|*.*" };

            if (ofd.ShowDialog() == true)
            {
                using (var fs = new FileStream(ofd.FileName, FileMode.OpenOrCreate))
                {
                    Board = (Board)bf.Deserialize(fs);
                }
                Refresh();
            }
        }

        //Сохранение игры
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var bf = new BinaryFormatter();
            var sfd = new SaveFileDialog { Filter = "Файлы игры|*.save|Все файлы|*.*" };

            if (sfd.ShowDialog() == true)
            {
                using (var fs = new FileStream(sfd.FileName, FileMode.OpenOrCreate))
                {
                    bf.Serialize(fs, Board);
                }
            }
        }

        //Показ таблицы рекордов
        private void ShowRecordsBtn_Click(object sender, RoutedEventArgs e)
        {
            var control = new RecordsControl(this);
            Content = control;
        }
        #endregion
    }
}
