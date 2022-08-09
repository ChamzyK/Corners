using System;

namespace Corners
{
    [Serializable]
    public class WinInformation
    {
        public string WinPlayer { get; set; }
        public string LosePlayer { get; set; }
        public int TurnCount { get; set; }
        public DateTime EndTime { get; set; }
        public int Size { get; set; }
        public int Square { get; set; }

        public override string ToString()
        {
            return string.Format("\nПобедитель: {0}.  Проигравший: {1}.\n" +
                                                 "Количество ходов: {2}. Время окончания партии: {3}.\n" +
                                                 "Размерность поля: {4}. Количество фишек: {5}.\n",
                                                 WinPlayer, //0
                                                 LosePlayer, //1
                                                 TurnCount, //2
                                                 EndTime, //3
                                                 Size, //5
                                                 Square); //5
        }
    }
}
