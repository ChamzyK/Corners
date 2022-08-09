using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Corners.Game
{
    [Serializable]
    class Board
    {
        #region Поля
        public readonly int BOARD_SIZE = 8;
        public readonly int CHECKER_SQUARE = 3;
        #endregion

        #region Конструкторы
        public Board(int square, int size, string player1 = "Player1", string player2 = "Player2", GameColor beginColor = GameColor.White)
        {
            if (CHECKER_SQUARE < BOARD_SIZE / 2)
            {
                BOARD_SIZE = size;
                CHECKER_SQUARE = square;
            }

            HistoryCollection = new ObservableCollection<string>(); //Инициализация коллекции
            CheckedCheckerMoves = new List<(int, int)>(); //Инициализация коллекции возможных ходов
            Checkers = new GameColor?[BOARD_SIZE, BOARD_SIZE]; //Инициализация игровой "доски"
            IsChecked = false; //Инициализация isChecked (фишка пока не выбрана)
            TurnColor = beginColor; //Инициализация очередности хода
            InitNewGame(); //Инициализация фишек (заполнение доски фишками)
            WinColor = null;
            Player1 = player1;
            Player2 = player2;
        }
        #endregion

        #region Свойства
        public ObservableCollection<string> HistoryCollection { get; } //История ходов
        public GameColor?[,] Checkers { get; private set; } //Игровая "доска"
        public GameColor TurnColor { get; private set; } //Цвет игрока который может ходить
        public (int, int) CheckedChecker { get; private set; } //Выбранная фигура
        public List<(int, int)> CheckedCheckerMoves { get; set; } // Возможные ходы выбранной фигуры
        public bool IsChecked { get; private set; } //Состояние игры (выбрана ли фигура)
        public GameColor? WinColor { get; private set; } //Цвет победителя
        public string Player1 { get; } //Имя первого игрока
        public string Player2 { get; } //Имя второго игрока
        #endregion

        #region Методы
        //Попытка сделать ход выбранной фишкой
        public bool TryMove(int x, int y)
        {
            //Проверка на конец игры и возможность хода
            if (WinColor == null && IsChecked && CheckedCheckerMoves.Contains((x, y)))
            {
                //Алгоритм смены значений двух переменных
                var temp = Checkers[CheckedChecker.Item1, CheckedChecker.Item2];
                Checkers[CheckedChecker.Item1, CheckedChecker.Item2] = null;
                Checkers[x, y] = temp;

                //Добавление истории
                HistoryCollection.Add(string.Format("{0}) {1}{2} -> {3}{4}",
                                      (HistoryCollection.Count + 1),
                                      (char)(CheckedChecker.Item1 + 'A'),
                                      BOARD_SIZE - CheckedChecker.Item2,
                                      (char)(x + 'A'),
                                      BOARD_SIZE - y));

                CheckWin();

                //Переключение и сброс
                TurnColor = TurnColor == GameColor.Black ? GameColor.White : GameColor.Black;
                IsChecked = false;
                return true;
            }
            IsChecked = false;
            return false;
        }

        //Проверка на победу
        private void CheckWin()
        {
            //Перебираем ячейки где должны стоять фишки
            for (int i = 0; i < CHECKER_SQUARE; i++)
            {
                for (int j = 0; j < CHECKER_SQUARE; j++)
                {
                    //Если хотя бы одна фишка не на месте то выходим из метода
                    bool result = TurnColor == GameColor.White ?
                                                  Checkers[i, j] != GameColor.White :
                                                  Checkers[BOARD_SIZE - i - 1, BOARD_SIZE - j - 1] != GameColor.Black;
                    if (result)
                    {
                        return;
                    }
                }
            }
            WinColor = TurnColor; //Если алгоритм дошел до этого места, то игра закончилась
        }

        //Попытка выбрать фишку
        public bool TryCheck(int x, int y)
        {
            if (WinColor == null) //Если игра еще не закончена
            {
                if (IsChecked = GetChecker(x, y) != null && //В данной ячейке есть Checker
                    TurnColor == Checkers[x, y]) //Проверка цвета на возможность хода
                {
                    CheckedChecker = (x, y);
                    return AddCanMoves();
                }
            }
            return false;
        }

        //Поиск и добавление возможных ходов
        private bool AddCanMoves()
        {
            int x = CheckedChecker.Item1;
            int y = CheckedChecker.Item2;

            CheckedCheckerMoves.Clear(); //Очищаем предыдущие значения

            if (!AddOneStep(x + 1, y)) AddJump(x + 2, y);
            if (!AddOneStep(x - 1, y)) AddJump(x - 2, y);
            if (!AddOneStep(x, y + 1)) AddJump(x, y + 2);
            if (!AddOneStep(x, y - 1)) AddJump(x, y - 2);

            CheckedCheckerMoves = CheckedCheckerMoves.Distinct().ToList(); //Удаление дубликатов

            return CheckedCheckerMoves.Count != 0;
        }

        //Добавление "прыгающих" ходов
        private void AddJump(int x, int y)
        {
            if (InsideBoard(x, y) && Checkers[x, y] == null && !CheckedCheckerMoves.Contains((x, y)))
            {
                CheckedCheckerMoves.Add((x, y));
                if (GetChecker(x + 1, y) != null) AddJump(x + 2, y);
                if (GetChecker(x - 1, y) != null) AddJump(x - 2, y);
                if (GetChecker(x, y + 1) != null) AddJump(x, y + 2);
                if (GetChecker(x, y - 1) != null) AddJump(x, y - 2);
            }
        }

        //Добавление "одношаговых" ходов
        private bool AddOneStep(int x, int y)
        {
            if (InsideBoard(x, y) && Checkers[x, y] == null)
            {
                CheckedCheckerMoves.Add((x, y));
                return true;
            }
            return false;
        }

        //Получение фишки по координатам
        private GameColor? GetChecker(int x, int y)
        {
            if (InsideBoard(x, y))
            {
                return Checkers[x, y];
            }
            return null;
        }

        //Проверка координат (внутри ли поля находится координата)
        public bool InsideBoard(int x, int y)
        {
            return x < BOARD_SIZE && x >= 0 && y < BOARD_SIZE && y >= 0;
        }

        //Добавление фишек в игровое поле
        private void InitNewGame()
        {
            //Строим квадрат из шашек со стороной CHECKER_SQURE
            for (int i = 0; i < CHECKER_SQUARE; i++)
            {
                for (int j = 0; j < CHECKER_SQUARE; j++)
                {
                    Checkers[i, j] = GameColor.Black; //Черных закидываем в левый верхний угол
                    Checkers[BOARD_SIZE - i - 1, BOARD_SIZE - j - 1] = GameColor.White; //Белых в нижний правый
                }
            }
        }
        #endregion
    }
}
