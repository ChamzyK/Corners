using Corners.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Controls;

namespace Corners.Controls
{
    public partial class WinControl : UserControl
    {
        //Поля
        private readonly MainWindow mainWindow;
        private readonly WinInformation winInformation;

        //Конструторы
        internal WinControl(MainWindow mainWindow, Board board)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;

            winInformation = new WinInformation
            {
                WinPlayer = board.WinColor == GameColor.Black ? board.Player1 : board.Player2,
                LosePlayer = board.WinColor == GameColor.Black ? board.Player2 : board.Player1,
                TurnCount = board.HistoryCollection.Count,
                EndTime = DateTime.Now,
                Size = board.BOARD_SIZE,
                Square = board.CHECKER_SQUARE
            };

            ResultTextBlock.Text = winInformation.ToString();

            SaveResult();
        }

        //Методы
        //Сохранение результата в файл
        private void SaveResult()
        {
            string fileName = "record";
            var bf = new BinaryFormatter();
            if (File.Exists(fileName)) //Если файл существует
            {
                List<WinInformation> tempCollection;
                using (var fs = new FileStream(fileName, FileMode.Open)) //Достаем список и записываем повверх новые данные
                {
                    tempCollection = (List<WinInformation>)bf.Deserialize(fs);
                    tempCollection.Add(winInformation);
                }
                using (var fs = new FileStream(fileName, FileMode.Open)) //Возвращаем обратно список в файл
                {
                    bf.Serialize(fs, tempCollection);
                }
            }
            else //Если файл не существует
            {
                var tempCollection = new List<WinInformation>();
                tempCollection.Add(winInformation);
                using (var fs = new FileStream(fileName, FileMode.Create)) //Создаем новый файл и заносим туда первые данные
                {
                    bf.Serialize(fs, tempCollection);
                }
            }
        }

        //Возврат
        private void ReturnButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mainWindow.ChangeContentToMain();
        }
    }
}
