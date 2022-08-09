using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Controls;

namespace Corners.Controls
{
    public partial class RecordsControl : UserControl
    {
        //Поля
        private readonly MainWindow mainWindow;
        private List<WinInformation> winInformations;

        //Конструкторы
        public RecordsControl(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            winInformations = new List<WinInformation>();
            DeserInformation();
            DataGrid.ItemsSource = winInformations;
        }

        //Методы
        //Достаем из файлов данные
        private void DeserInformation()
        {
            var bf = new BinaryFormatter();
            string fileName = "record";
            if (File.Exists(fileName))
            {
                using (var fs = new FileStream(fileName, FileMode.Open))
                {
                    winInformations = (List<WinInformation>)bf.Deserialize(fs);
                }
            }
        }

        //Возвращение
        private void ReturnButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mainWindow.ChangeContentToMain();
        }
    }
}
