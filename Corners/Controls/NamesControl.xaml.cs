using Corners.Game;
using System.Windows;
using System.Windows.Controls;

namespace Corners.Controls
{
    public partial class NamesControl : UserControl
    {
        #region Поля
        private MainWindow mainWindow;
        private const string WHITE_TEXT = "Первые ходят: White (нажать для изменения)";
        private const string BLACK_TEXT = "Первые ходят: Black (нажать для изменения)";
        private GameColor firstColor;
        private int size;
        private int square;
        #endregion

        #region Конструкторы
        public NamesControl(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            ColorButton.Content = WHITE_TEXT;
            firstColor = GameColor.White;
        }
        #endregion

        #region Обработчики событий
        //Возвращение обратно
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.ChangeContentToMain();
        }

        //Создание новой игры
        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.BeginNewGame(square, size, Player1TextBox.Text, Player2TextBox.Text, firstColor);
            mainWindow.ChangeContentToMain();
        }

        //Контроль изменения текста
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NewGameButton.IsEnabled = int.TryParse(SizeTextBox.Text, out size) && size < 14 && size > 6
                && int.TryParse(SquareTextBox.Text, out square) && square <= size / 2 && square >= 2 &&
                !(string.IsNullOrEmpty(Player1TextBox.Text) || string.IsNullOrEmpty(Player2TextBox.Text));
        }

        //Изменение цвета
        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if(firstColor == GameColor.White)
            {
                ColorButton.Content = BLACK_TEXT;
                firstColor = GameColor.Black;
            }
            else
            {
                ColorButton.Content = WHITE_TEXT;
                firstColor = GameColor.White;
            }
        }
        #endregion
    }
}
