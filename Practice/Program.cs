using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Practice
{
    class Account
    {
        private readonly int _startScore;
        private readonly List<MoneyOperation> _moneyOperations;

        public Account(List<MoneyOperation> moneyOperations, int startScore)
        {
            _moneyOperations = moneyOperations;
            _startScore = startScore;
        }

        public int Score(DateTime date)
        {
            IEnumerable<MoneyOperation> sorted = _moneyOperations.Where(x => x.Date <= date);
            int score = _startScore;
            foreach (var operation in sorted)
            {
                score = FindScore(operation, score);
            }
            return score;
        }

        public int Score()
        {
            int score = _startScore;
            foreach (var operation in _moneyOperations)
            {
                score = FindScore(operation, score);
            }
            if (score < 0)
                throw new Exception("Расход превысил остаток по карте");
            return score;
        }

        private int FindScore(MoneyOperation operation, int score)
        {
            switch (operation.OperationName)
            {
                case "in":
                    score += operation.Money;
                    break;
                case "out":
                    score -= operation.Money;
                    break;
                case "revert":
                    var findOperation = _moneyOperations.Find(x => x.Date == operation.Date);
                    if (findOperation.OperationName == "in")
                        score -= findOperation.Money;
                    else
                        score += findOperation.Money;
                    break;
            }

            return score;
        }
    }
    class MoneyOperation
    {
        public DateTime Date { get; }
        public int Money { get; }
        public string OperationName { get; }

        public MoneyOperation(DateTime date, int money, string operationName)
        {
            Date = date;
            Money = money;
            OperationName = operationName;
        }

        public MoneyOperation(DateTime date, string operationName)
        {
            Date = date;
            OperationName = operationName;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<MoneyOperation> operations = ReadFile();
            Account account = new Account(operations, StartScore());
            Console.WriteLine(account.Score());
            Console.WriteLine(account.Score(DateTimeParse("2021-06-01 12:10")));
        }

        static int StartScore() => int.Parse(File.ReadAllLines("File.txt")[0]);

        static List<MoneyOperation> ReadFile()
        {
            string[] file = File.ReadAllLines("File.txt");
            List<MoneyOperation> moneyOperations = new List<MoneyOperation>();
            for(int i = 1;i < file.Length; i++)
            {
                string[] lineSplit = file[i].Split(" | ");
                MoneyOperation moneyOperation;
                if (lineSplit.Length == 3)
                    moneyOperation = new MoneyOperation(DateTimeParse(lineSplit[0]), int.Parse(lineSplit[1]),
                        lineSplit[2]);
                else
                    moneyOperation = new MoneyOperation(DateTimeParse(lineSplit[0]), lineSplit[1]);
                moneyOperations.Add(moneyOperation);
            }
            return moneyOperations;
        }

        static DateTime DateTimeParse(string date)
        {
            DateTime dateTime;
            DateTime.TryParseExact(date, "yyyy-MM-dd hh:mm",CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
            return dateTime;
        }
    }
}