using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Practice
{
    class Account
    {
        private readonly int _score;
        private readonly List<MoneyOperation> _moneyOperations;

        public Account(List<MoneyOperation> moneyOperations, int score)
        {
            _moneyOperations = moneyOperations;
            _score = score;
        }

        public int Score(DateTime date)
        {
            var sorted = _moneyOperations.Where(x => x.Date <= date);
            int score = _score;
            foreach (var operation in sorted)
            {
                score = operation.Operation.Calculate(score, operation, _moneyOperations);
            }
            return score;
        }

        public int Score()
        {
            int score = _score;
            foreach (var operation in _moneyOperations)
            {
                score = operation.Operation.Calculate(score, operation, _moneyOperations);
            }
            if (score < 0)
                throw new Exception("Расход превысил остаток по карте");
            return score;
        }
    }

    interface IOperation
    {
        public int Calculate(int score, MoneyOperation operation, List<MoneyOperation> operations);
    }

    sealed class In : IOperation
    {
        public int Calculate(int score, MoneyOperation operation, List<MoneyOperation> operations) => score + operation.Money;
    }

    sealed class Out : IOperation
    {
        public int Calculate(int score, MoneyOperation operation, List<MoneyOperation> operations) => score - operation.Money;
    }

    sealed class Revert : IOperation
    {
        public int Calculate(int score, MoneyOperation operation, List<MoneyOperation> operations)
        {
            var findOperation = operations.Find(x => x.Date == operation.Date);
            if (findOperation.Operation is In)
                score -= findOperation.Money;
            else
                score += findOperation.Money;
            return score;
        }
    }
    class MoneyOperation
    {
        public readonly DateTime Date;
        public readonly int Money;
        public readonly IOperation Operation;

        public MoneyOperation(DateTime date, int money, IOperation operation)
        {
            Date = date;
            Money = money;
            Operation = operation;
        }

        public MoneyOperation(DateTime date, IOperation operation)
        {
            Date = date;
            Operation = operation;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<MoneyOperation> operations = ReadFile();
            Account account = new Account(operations, StartScore());
            Console.WriteLine(account.Score());
            Console.WriteLine(account.Score(DateTimeParse("2021-06-01 12:05")));
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
                    moneyOperation = new MoneyOperation(DateTimeParse(lineSplit[0]), int.Parse(lineSplit[1]), SetOperation(lineSplit[2])
                        );
                else
                    moneyOperation = new MoneyOperation(DateTimeParse(lineSplit[0]),SetOperation(lineSplit[1]));
                moneyOperations.Add(moneyOperation);
            }
            return moneyOperations;
        }

        static IOperation SetOperation(string operation)
        {
            switch (operation)
            {
                case "in":
                    return new In();
                case "out":
                    return new Out();
                case "revert":
                    return new Revert();
            }

            return new Revert();
        }

        static DateTime DateTimeParse(string date)
        {
            DateTime dateTime;
            DateTime.TryParseExact(date, "yyyy-MM-dd hh:mm",CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
            return dateTime;
        }
    }
}