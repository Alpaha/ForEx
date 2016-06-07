using System;
using System.Collections.Generic;
using System.Linq;

////////////////////////////
// Реализация игры HANABI //
////////////////////////////

//------------------------//
//                        //
// Автор: Альпашко Андрей //
//                        //
//------------------------//

namespace Alpashko_Andrew
{
    #region class Card
    class Card
    {
        // fields
        private char color;
        private int rank;
        //      Переменная которая показывает, является ли данная карта рискованной.
        //      Основывается на двух переменных(knowColor, knowRank)
        //      Только если обе переменные - "False", то, соответственно карта рискованная, и наоборот.
        private bool riskyCard = true;
        private bool knowColor = false;
        private bool knowRank = false;
        // Ctors
        public Card(string input)
        {
            this.color = input[0];
            this.rank = (int)char.GetNumericValue(input[1]);
        }

        // Props
        public char Color
        { get { return color; } }
        public int Rank
        { get { return rank; } }

        public bool RiskyCard
        { get { return riskyCard; } set { riskyCard = value; } }
        public bool KnowColor
        { get { return knowColor; } set { knowColor = value; } }
        public bool KnowRank
        { get { return knowRank; } set { knowRank = value; } }

        // Свойства для определения карт методом исключения
        public List<char> DefinedColor
        { get; set; }
        public List<int> DefinedRank
        { get; set; }
    }
    #endregion

    #region class Game
    class Game
    {
        #region fields
        private int turn; // Номер хода
        private int score; // Очки
        private int index; // Индекс поиска по стартовой строке (раздача карт, и определение колоды)
        private bool finished; // Флаг окончания игрового цикла (True - закончена, False - активна)
        private int riskyTurns; // Рисковый ход
        private string[] commandString = new string[] { "Tell color ", "Tell rank ", "Play card ", "Drop card " }; // Строки команд в игре
        
        private List<Card> player1 = new List<Card>(); // Список карт первого игрока
        private List<Card> player2 = new List<Card>(); // Список карт второго игрока
        private List<Card> table = new List<Card>(); // Карты на столе
        private List<Card> deck = new List<Card>();  // Список карт для колоды

        private System.IO.StreamReader reader;
        #endregion

        // Ctor.
        public Game(string input)
        {
            this.turn = 0;
            this.score = 0;
            this.finished = false;
            this.riskyTurns = 0;
            this.index = 0;
            this.reader = reader;
        }

        #region Methods
        
        public string StartGame(string startString)
        {
            if (!startString.Contains("Start new game with deck") || startString.Length < 56) // Условие при котором игра не сможет начаться
                return string.Empty;

            GetStartCards(GetListOfCard(startString)); // Получить стартовые карты.
            InitializeTableCards(table); // Карты на столе.
            string outputString = DoSomeAction(finished, commandString, reader); // Здесь вводим команды.
            if (outputString.Contains("Start"))
            {
                if (finished)
                {
                    Console.WriteLine("Turn: {0}, cards {1}, with risk: {2}", turn, score, riskyTurns);
                }
                return outputString;
            }
            else
                finished = true;
            Console.WriteLine("Turn: {0}, cards {1}, with risk: {2}", turn, score, riskyTurns);
            //WatchTable(player1, player2, table, turn);
            return string.Empty;
        }

        // Стартовые значения карт на столе
        private void InitializeTableCards(List<Card> table)
        {
            table.Add(new Card("R0"));
            table.Add(new Card("G0"));
            table.Add(new Card("B0"));
            table.Add(new Card("W0"));
            table.Add(new Card("Y0"));
        }

        // Получаем стартовые карты из стартовой строки
        public void GetStartCards(string[] input)
        {
            string[] startCardsString = input;
            
            // Раздаём карты игрокам
            PlayerGetsCards(startCardsString, player1, ref index);
            PlayerGetsCards(startCardsString, player2, ref index);

            // Заполняем колоду
            GetDeckCards(deck, ref index, startCardsString);

            // Заполняем свойства определяемости карт методом исключения
            GetDefinedCards(player1);
            GetDefinedCards(player2);
        }
        // Раздаём карты игрокам.
        public void PlayerGetsCards(string[] input, List<Card> player, ref int index)
        {
            for (int count = 0; count < 5; count++)
            {
                player.Add(new Card(input[index]));
                index++;
            }
        }
        private void GetDeckCards(List<Card> deck, ref int index, string[] cardsForDeck)
        {
            while(index < cardsForDeck.Length)
            {
                deck.Add(new Card(cardsForDeck[index]));
                index++;
            }
        }
        // Получаем весь список карт.
        public string[] GetListOfCard(string input)
        {
            return input.Replace("Start new game with deck ", string.Empty).Split(' ');
        }

        // Инициализируем свойства для метода исключения!!!
        public void GetDefinedCards(List<Card> player)
        {
            foreach (Card item in player)
            {
                if (item.DefinedColor == null && item.DefinedRank == null)
                {
                    item.DefinedColor = new List<char> { 'R', 'G', 'B', 'W', 'Y' };
                    item.DefinedRank = new List<int> { 1, 2, 3, 4, 5 };
                }
            }
        }

        // Просмотр положения на столе
        public void WatchTable(List<Card> player1, List<Card> player2, List<Card> table, int turn)
        {
            Console.WriteLine("Turn: {0}, Score: {1}, Finished: {2}", turn, score, finished); // Статистика по ходам, очкам.
            Console.WriteLine("Turn: {0}, cards {1}, with risk: {2}", turn, score, riskyTurns);
            if (turn % 2 == 0)                          // Проверка игрока, который ходит, а затем вывод инфо
            {                                           // о картах на руках у игроков
                GetStatisticPlayer1IsFirst(player1, player2);
                Console.Write("\n         Table: ");
                foreach (var card in table)
                {
                    Console.Write("{0}{1} ", card.Color, card.Rank);
                }
                Console.WriteLine();
            }
            else
            {
                GetStatisticPlayer2IsFirst(player1, player2);
                Console.Write("\n         Table: ");
                foreach (var card in table)
                {
                    Console.Write("{0}{1} ", card.Color, card.Rank);
                }
                Console.WriteLine();
            }
        }
        public void GetStatisticPlayer1IsFirst(List<Card> player1, List<Card> player2)
        {
            Console.Write("Current player: ");
            foreach (var card in player1)
            {
                Console.Write("{0}{1} ", card.Color, card.Rank);
            }
            Console.Write("\n   Next player: ");
            foreach (var card in player2)
            {
                Console.Write("{0}{1} ", card.Color, card.Rank);
            }
        }
        public void GetStatisticPlayer2IsFirst(List<Card> player1, List<Card> player2)
        {
            Console.Write("Current player: ");
            foreach (var item in player2)
            {
                Console.Write("{0}{1} ", item.Color, item.Rank);
            }
            Console.Write("\n   Next player: ");
            foreach (var item in player1)
            {
                Console.Write("{0}{1} ", item.Color, item.Rank);
            }
        }

        #region Обработка основных команд в игре
        // Метод отвечающий за цикловую обработку входящих команд
        // (Tell, Drop, Play)
        public string DoSomeAction(bool finished, string[] commands, System.IO.StreamReader reader)
        {
            string commandString = string.Empty;

            // Пока игра/колода не закончена или на столе не выложены все 25 карт
            while (finished != true && score != 25 && deck.Count > 0) 
            {
                //WatchTable(player1, player2, table, turn); // - раскомментить, если нужно проверить ход игры
                commandString = reader.ReadLine();
                if (turn % 2 == 0)// Проверяем какой игрок делает свой ход.
                {
                    if (commandString.Contains(commands[0])) finished = TellColor(commandString, player2);
                    else if (commandString.Contains(commands[1])) finished = TellRank(commandString, player2);
                    else if (commandString.Contains(commands[2])) finished = PlayCard(commandString, player1);
                    else if (commandString.Contains(commands[3])) DropCard(commandString, player1);
                    else finished = true;
                }
                else
                {
                    if (commandString.Contains(commands[0])) finished = TellColor(commandString, player1);
                    else if (commandString.Contains(commands[1])) finished = TellRank(commandString, player1);
                    else if (commandString.Contains(commands[2])) finished = PlayCard(commandString, player2);
                    else if (commandString.Contains(commands[3])) DropCard(commandString, player2);
                    else finished = true;
                }
            }
            return commandString;
        }
        // Функция Сказать Цвет
        public bool TellColor( string inputCommand, List<Card> playerCards )
        {
            inputCommand = inputCommand.Replace( "Tell color ", string.Empty );
            char inputColor = inputCommand[ 0 ]; // Цвет который ищем в руке
            int foundAllCards = 0;
            // Определяем количество карт заданного цвета в руке игрока
            int numberOfColorInHand = DetectNumberOfColorInHand( inputColor, playerCards );
            // Определяем количество карт во входной строке
            int numberOfColorInString = DetectNumberOfColorInCommand( inputCommand );
            // Проверка количества карт в руке и во входной строке
            if( numberOfColorInHand == numberOfColorInString )
            {
                // Выбираем номера карт из строки
                string[ ] inputValuesOfCards = inputCommand.Remove( 0, inputCommand.IndexOfAny( new[ ] { '0', '1', '2', '3', '4' } ) ).Split( ' ' );
                // Путём нахождения вхождения числового символа
                foreach( Card card in playerCards )
                {
                    if( card.Color == inputColor )
                    {
                        foreach( var dignit in inputValuesOfCards )
                        {
                            if( int.Parse( dignit ) == playerCards.IndexOf( card ) )
                                foundAllCards++;
                        }
                        card.KnowColor = true;
                        if( card.KnowColor && card.KnowRank )
                        {
                            card.RiskyCard = false;
                        }
                    }
                    //else if( card.DefinedColor.Count > 1 )
                    //{
                    //    card.DefinedRank.Remove( inputColor );
                    //}
                    //else
                    //{
                    //    card.KnowRank = true;
                    //    if( card.KnowRank && card.KnowColor )
                    //        card.RiskyCard = false;
                    //}
                    else
                    {
                        if( card.DefinedColor.Count > 1 )
                        {
                            card.DefinedColor.Remove( inputColor );
                            int sovpad = table.Where( tableCard => tableCard.Rank == card.Rank - 1 ).Count( );
                            if( sovpad == card.DefinedColor.Count )
                            {
                                card.KnowColor = true;
                                card.RiskyCard = false;
                            }
                        }
                        else
                        {
                            card.KnowColor = true;
                            if( card.KnowColor && card.KnowRank )
                                card.RiskyCard = false;
                        }
                    }
                }
            }
                if( foundAllCards == numberOfColorInString )
                {
                    turn++;
                    return false;
                }
                else
                    return true;
        }
        public int DetectNumberOfColorInHand(char color, List<Card> playerCards)
        {
            return playerCards.Where( card => card.Color == color ).Select( x => x ).Count();
        }
        public int DetectNumberOfColorInCommand(string inputCommand)
        {
            return inputCommand.Remove( 0, inputCommand.IndexOfAny( new[ ] { '0', '1', '2', '3', '4' } ) ).Split( ).Length;
        }

        // Функция Сказать Ранг
        public bool TellRank(string inputCommand, List<Card> playerCards)
        {
            inputCommand = inputCommand.Replace("Tell rank ", string.Empty);
            int inputRank = (int)char.GetNumericValue(inputCommand[0]); // Значение карты, которое ищем в руке
            int foundAllCards = 0;
            int numberOfRankInHand = DetectNumberOfRankInHand(inputRank, playerCards); // Определяем количество карт заданного значения в руке игрока
            int numberOfRankInString = DetectNumberOfRankInCommand(inputCommand); // Определяем количество карт во входной строке
            if( numberOfRankInHand == numberOfRankInString )// Проверка количества карт в руке и во входной строке
            {
                string[ ] inputValuesOfCards = inputCommand.Remove( 0, 12 ).Split( ' ' ); // Выбираем номера карт из строки
                foreach( Card card in playerCards )
                {
                    if( card.Rank == inputRank )                                     // Данная конструкция нужная
                    {                                                                // для того, чтобы, если карты
                        foreach( string dignit in inputValuesOfCards )                // на вход подаются в разном порядке
                        {                                                            // то определение всё равно произошло
                            if( int.Parse( dignit ) == playerCards.IndexOf( card ) ) // к прим. (Tell rank 1 for cards 1 4 2 5 3)
                                foundAllCards++;                                     // хотя наверное можно через LINQ
                        }

                        card.KnowRank = true;
                        if( card.KnowColor && card.KnowRank )
                            card.RiskyCard = false;
                    }

                    else
                    {
                        if( card.DefinedRank.Count > 1 )
                        {
                            card.DefinedRank.Remove( inputRank );
                        }
                        else
                        {
                            card.KnowRank = true;
                            if( card.KnowColor && card.KnowRank )
                                card.RiskyCard = false;
                        }

                        int sovpad = table.Where( tableCard => tableCard.Rank == card.Rank - 1 ).Count( );
                        if( sovpad == card.DefinedColor.Count )
                        {
                            card.KnowColor = true;
                            card.RiskyCard = false;
                        }
                    }
                }
            }
            if (foundAllCards == numberOfRankInString)
            {
                turn++;
                return false;
            }
            else return true;
        }
        public int DetectNumberOfRankInHand(int rank, List<Card> playerCards)
        {
            int NumberOfCards = 0; // Счётчик количества карт с этим цветом в руке
            foreach (var item in playerCards)
            {
                if (item.Rank == rank)
                    NumberOfCards++;
            }
            return NumberOfCards;
        }
        public int DetectNumberOfRankInCommand(string inputCommand)
        {
            string[] inputString = inputCommand.Remove(0, 12).Split(' ');
            return inputString.Length;
        }

        // Функции Скинуть Карту/Сыграть Карту
        public void DropCard(string inputCommand, List<Card> playerCards)
        {
            playerCards.RemoveAt(int.Parse(inputCommand.Remove(0, inputCommand.Length - 1))); // Выбрасываем указанную карту
            playerCards.Add(deck[0]); // Берём новую карту из колоды
            GetDefinedCards(playerCards);
            deck.RemoveAt(0); // Сдвигаем колоду
            turn++;
        }
        public bool PlayCard(string inputCommand, List<Card> playerCards)
        {
            int index = int.Parse(inputCommand.Remove(0, inputCommand.Length - 1)); // Берём индекс карты
            Card playCard = null;// Вносим временную карту, чтобы записать в неё значение сыгранной карты
            foreach (var card in table)
            {
                // Карта должна быть того же цвета и больше на единицу, либо другого цвета 
                if(card.Color == playerCards[index].Color && (playerCards[index].Rank - card.Rank == 1))
                {
                    playCard = playerCards[index];
                    playerCards.RemoveAt(index);
                    playerCards.Add(deck[0]);
                    GetDefinedCards(playerCards);
                    deck.RemoveAt(0);
                    turn++;
                    score++;
                    if(playCard.RiskyCard)
                    {
                        riskyTurns++;
                    }
                    break;
                }
            }
            if(playCard != null)
            {
                for(int i = 0; i < table.Count; i++)
                {
                    if(table[i].Color == playCard.Color)
                    {
                        table[i] = playCard;
                        break;
                    }
                }
                return false;
            }
            else
            {
                playerCards.RemoveAt(index);
                playerCards.Add(deck[0]);
                deck.RemoveAt(0);
                turn++;
                return true;
            }
        }
        #endregion
    }
    #endregion
    #endregion


    class Alpashko_Andrew
    {
        static void Main(string[] args)
        {
            // Строка отвечающая за ввод новой игры, из ещё не завершённой
            string outputData = null;
            string startGameString = null;
            // Запуск новой игры, где будет считываться входная строка.
            while ((startGameString = Console.ReadLine()) != null)
            {   // Проверяем стартовую строка
                
                if (!string.IsNullOrEmpty(outputData)) // Если предыдущая игра не завершилась
                    startGameString = outputData; // Стартовой строке передаём строку из предыдущей игры
                
                Game game = new Game(startGameString);
                outputData = game.StartGame(startGameString);
                
                game = null;
            }
        }
    }
}
