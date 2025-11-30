using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

/////////////////////////////////////////////////////////////
// РОЗДІЛ 1 — КОРИСТУВАЦЬКІ ВИНЯТКИ
/////////////////////////////////////////////////////////////
public class InvalidInputException : Exception
{
    public InvalidInputException(string message) : base(message) { }
}

/////////////////////////////////////////////////////////////
// РОЗДІЛ 2 — БАЗОВИЙ КЛАС "Телефонний номер" (старий варіант)
/////////////////////////////////////////////////////////////
public class PhoneNumberOld
{
    public string Number { get; set; }
    public string Operator { get; set; }

    public PhoneNumberOld() { }
    public PhoneNumberOld(string number, string oper)
    {
        Number = number;
        Operator = oper;
    }

    public virtual void Print()
    {
        Console.WriteLine($"Номер: {Number}, Оператор: {Operator}");
    }
}

/////////////////////////////////////////////////////////////
// РОЗДІЛ 3 — ПОХІДНИЙ КЛАС "Дзвінки"
/////////////////////////////////////////////////////////////
public class CallInfo : PhoneNumberOld
{
    public DateTime Date { get; set; }
    public int Minutes { get; set; }
    public decimal Cost { get; set; }

    public CallInfo() { }

    public CallInfo(string number, string oper, DateTime date, int minutes, decimal cost)
        : base(number, oper)
    {
        Date = date;
        Minutes = minutes;
        Cost = cost;
    }

    public override void Print()
    {
        Console.WriteLine(
            $"Номер: {Number}, Оператор: {Operator}, Дата: {Date:dd.MM.yyyy}, Хвилин: {Minutes}, Вартість: {Cost} грн");
    }
}

/////////////////////////////////////////////////////////////
// РОЗДІЛ 4 — КЛАС РОБОТИ З ФАЙЛОВОЮ "БАЗОЮ ДАНИХ"
/////////////////////////////////////////////////////////////
public class CallDatabase
{
    private readonly string filePath;

    public CallDatabase(string path)
    {
        filePath = path;

        if (!File.Exists(filePath))
        {
            var seed = new List<CallInfo>()
            {
                new CallInfo("0671234567", "Kyivstar", DateTime.Now.AddDays(-5), 10, 25),
                new CallInfo("0501112223", "Vodafone", DateTime.Now.AddDays(-4), 20, 40),
                new CallInfo("0935556677", "Lifecell", DateTime.Now.AddDays(-3), 15, 30),
                new CallInfo("0679998877", "Kyivstar", DateTime.Now.AddDays(-2), 8, 20),
                new CallInfo("0503344556", "Vodafone", DateTime.Now.AddDays(-1), 12, 28)
            };

            SaveAll(seed);
        }
    }

    public List<CallInfo> LoadAll()
    {
        var list = new List<CallInfo>();

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split('|');
            list.Add(new CallInfo(
                parts[0],
                parts[1],
                DateTime.ParseExact(parts[2], "dd.MM.yyyy", CultureInfo.InvariantCulture),
                int.Parse(parts[3]),
                decimal.Parse(parts[4])
            ));
        }

        return list;
    }

    public void SaveAll(List<CallInfo> list)
    {
        var lines = list.Select(c =>
            $"{c.Number}|{c.Operator}|{c.Date:dd.MM.yyyy}|{c.Minutes}|{c.Cost}");
        File.WriteAllLines(filePath, lines);
    }

    public void Add(CallInfo call)
    {
        var list = LoadAll();
        list.Add(call);
        SaveAll(list);
    }

    public void DeleteByIndex(int index)
    {
        var list = LoadAll();
        if (index < 0 || index >= list.Count)
            throw new InvalidInputException("Невірний індекс для видалення.");

        list.RemoveAt(index);
        SaveAll(list);
    }

    public void Edit(int index, CallInfo newData)
    {
        var list = LoadAll();
        if (index < 0 || index >= list.Count)
            throw new InvalidInputException("Невірний індекс для редагування.");

        list[index] = newData;
        SaveAll(list);
    }
}

/////////////////////////////////////////////////////////////
// РОЗДІЛ 5 — ДОПОМІЖНИЙ КЛАС ДЛЯ БЕЗПЕЧНОГО ВВОДУ
/////////////////////////////////////////////////////////////
public static class InputHelper
{
    public static int ReadInt(string msg)
    {
        while (true)
        {
            Console.Write(msg);
            var s = Console.ReadLine();
            if (int.TryParse(s, out int value))
                return value;
            Console.WriteLine("Помилка: введіть ціле число!");
        }
    }

    public static decimal ReadDecimal(string msg)
    {
        while (true)
        {
            Console.Write(msg);
            var s = Console.ReadLine();
            if (decimal.TryParse(s, out decimal value))
                return value;
            Console.WriteLine("Помилка: введіть число!");
        }
    }

    public static DateTime ReadDate(string msg)
    {
        while (true)
        {
            Console.Write(msg);
            var s = Console.ReadLine();
            if (DateTime.TryParseExact(s, "dd.MM.yyyy", null,
                DateTimeStyles.None, out DateTime d))
                return d;
            Console.WriteLine("Помилка: формат дати dd.MM.yyyy !");
        }
    }
}

/////////////////////////////////////////////////////////////
// РОЗДІЛ 6 — ОСНОВНА ПРОГРАМА + МЕНЮ
/////////////////////////////////////////////////////////////
class Program
{
    static void Pause()
    {
        Console.WriteLine("\nНатисніть будь-яку клавішу, щоб повернутися в меню...");
        Console.ReadKey();
        Console.Clear();
    }

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Виберіть версію програми:");
        Console.WriteLine("1 – Стара версія");
        Console.WriteLine("2 – Нова версія");
        Console.Write("Ваш вибір: ");

        var choice = Console.ReadKey().Key;
        Console.Clear();

        var db = new CallDatabase("calls.txt");

        if (choice == ConsoleKey.D1)
            OldMenu(db);
        else if (choice == ConsoleKey.D2)
            NewMenu(db);
        else
            Console.WriteLine("Невірний вибір. Вихід...");
    }

    ///////////////////////////////////////////////////////////////
    // СТАРЕ МЕНЮ
    ///////////////////////////////////////////////////////////////
    static void OldMenu(CallDatabase db)
    {
        while (true)
        {
            Console.WriteLine("===== СТАРЕ МЕНЮ =====");
            Console.WriteLine("v – вивести всі записи");
            Console.WriteLine("a – додати запис");
            Console.WriteLine("e – редагувати");
            Console.WriteLine("d – видалити");
            Console.WriteLine("s – статистика");
            Console.WriteLine("x – видалити файл БД");
            Console.WriteLine("Enter – вихід");
            Console.Write("Ваш вибір: ");

            var key = Console.ReadKey().Key;
            Console.WriteLine();

            switch (key)
            {
                case ConsoleKey.V: ShowOld(db); Pause(); break;
                case ConsoleKey.A: AddOld(db); Pause(); break;
                case ConsoleKey.E: EditOld(db); Pause(); break;
                case ConsoleKey.D: DeleteOld(db); Pause(); break;
                case ConsoleKey.S: StatsOld(db); Pause(); break;
                case ConsoleKey.X: DeleteFileOld(); Pause(); break;
                case ConsoleKey.Enter: return;
                default: Console.Clear(); break;
            }
        }
    }

    static void ShowOld(CallDatabase db)
    {
        var list = db.LoadAll();
        Console.WriteLine("\n===== ВСІ ЗАПИСИ (OLD) =====");

        for (int i = 0; i < list.Count; i++)
        {
            Console.Write(i + ") ");
            list[i].Print(); // Тепер показує хвилини, дату і вартість
        }
    }

    static void AddOld(CallDatabase db)
    {
        Console.WriteLine("\n--- Додавання ---");

        Console.Write("Номер: ");
        string number = Console.ReadLine();

        Console.Write("Оператор: ");
        string oper = Console.ReadLine();

        DateTime date = InputHelper.ReadDate("Дата (dd.MM.yyyy): ");
        int min = InputHelper.ReadInt("Хвилин: ");
        decimal cost = InputHelper.ReadDecimal("Вартість: ");

        db.Add(new CallInfo(number, oper, date, min, cost));
        Console.WriteLine("Додано!");
    }

    static void EditOld(CallDatabase db)
    {
        Console.WriteLine("\n--- Редагування ---");
        ShowOld(db);

        int index = InputHelper.ReadInt("Введіть індекс: ");

        Console.Write("Номер: ");
        string number = Console.ReadLine();

        Console.Write("Оператор: ");
        string oper = Console.ReadLine();

        DateTime date = InputHelper.ReadDate("Дата (dd.MM.yyyy): ");
        int min = InputHelper.ReadInt("Хвилин: ");
        decimal cost = InputHelper.ReadDecimal("Вартість: ");

        db.Edit(index, new CallInfo(number, oper, date, min, cost));
        Console.WriteLine("Змінено!");
    }

    static void DeleteOld(CallDatabase db)
    {
        Console.WriteLine("\n--- Видалення ---");
        ShowOld(db);
        int index = InputHelper.ReadInt("Індекс для видалення: ");

        try
        {
            db.DeleteByIndex(index);
            Console.WriteLine("Видалено!");
        }
        catch (InvalidInputException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void StatsOld(CallDatabase db)
    {
        var list = db.LoadAll();
        Console.WriteLine("\n===== СТАТИСТИКА (OLD) =====");

        var total = list.Sum(c => c.Cost);
        var days = list.Select(c => c.Date.Date).Distinct().Count();
        Console.WriteLine($"Середня платня в день: {total / days:0.00} грн");

        decimal limit = InputHelper.ReadDecimal("\nПоріг вартості хвилини: ");
        var countHigh = list
            .Where(c => (c.Cost / c.Minutes) > limit)
            .Select(c => c.Date.Date)
            .Distinct()
            .Count();
        Console.WriteLine($"Днів з вартістю хвилини > {limit}: {countHigh}");

        var evenDays = list
            .Where(c => c.Minutes % 2 == 0)
            .Select(c => c.Date.ToString("dd.MM.yyyy"))
            .Distinct();
        Console.WriteLine("\nДні з парною кількістю хвилин:");
        foreach (var d in evenDays)
            Console.WriteLine(d);
    }

    static void DeleteFileOld()
    {
        if (File.Exists("calls.txt"))
        {
            File.Delete("calls.txt");
            Console.WriteLine("Файл БД видалено!");
        }
        else Console.WriteLine("Файл не існує.");
    }

    ///////////////////////////////////////////////////////////////
    // НОВЕ МЕНЮ
    ///////////////////////////////////////////////////////////////
    static void NewMenu(CallDatabase db)
    {
        while (true)
        {
            Console.WriteLine("===== НОВЕ МЕНЮ =====");
            Console.WriteLine("v – вивести всі записи");
            Console.WriteLine("a – додати запис");
            Console.WriteLine("e – редагувати");
            Console.WriteLine("d – видалити");
            Console.WriteLine("s – статистика");
            Console.WriteLine("x – видалити файл БД");
            Console.WriteLine("Enter – вихід");
            Console.Write("Ваш вибір: ");

            var key = Console.ReadKey().Key;
            Console.WriteLine();

            switch (key)
            {
                case ConsoleKey.V: ShowNew(db); Pause(); break;
                case ConsoleKey.A: AddNew(db); Pause(); break;
                case ConsoleKey.E: EditNew(db); Pause(); break;
                case ConsoleKey.D: DeleteNew(db); Pause(); break;
                case ConsoleKey.S: StatsNew(db); Pause(); break;
                case ConsoleKey.X: DeleteFileNew(); Pause(); break;
                case ConsoleKey.Enter: return;
                default: Console.Clear(); break;
            }
        }
    }

    static void ShowNew(CallDatabase db)
    {
        var list = db.LoadAll();
        Console.WriteLine("\n===== ВСІ ЗАПИСИ (NEW) =====");
        for (int i = 0; i < list.Count; i++)
        {
            Console.Write(i + ") ");
            list[i].Print();
        }
    }

    static void AddNew(CallDatabase db)
    {
        Console.WriteLine("\n--- Додавання ---");

        Console.Write("Номер: ");
        string number = Console.ReadLine();

        Console.Write("Оператор: ");
        string oper = Console.ReadLine();

        DateTime date = InputHelper.ReadDate("Дата (dd.MM.yyyy): ");
        int min = InputHelper.ReadInt("Хвилин: ");
        decimal cost = InputHelper.ReadDecimal("Вартість: ");

        db.Add(new CallInfo(number, oper, date, min, cost));
        Console.WriteLine("Додано!");
    }

    static void EditNew(CallDatabase db)
    {
        Console.WriteLine("\n--- Редагування ---");
        ShowNew(db);

        int index = InputHelper.ReadInt("Введіть індекс: ");

        Console.Write("Номер: ");
        string number = Console.ReadLine();

        Console.Write("Оператор: ");
        string oper = Console.ReadLine();

        DateTime date = InputHelper.ReadDate("Дата (dd.MM.yyyy): ");
        int min = InputHelper.ReadInt("Хвилин: ");
        decimal cost = InputHelper.ReadDecimal("Вартість: ");

        db.Edit(index, new CallInfo(number, oper, date, min, cost));
        Console.WriteLine("Змінено!");
    }

    static void DeleteNew(CallDatabase db)
    {
        Console.WriteLine("\n--- Видалення ---");
        ShowNew(db);
        int index = InputHelper.ReadInt("Індекс для видалення: ");

        try
        {
            db.DeleteByIndex(index);
            Console.WriteLine("Видалено!");
        }
        catch (InvalidInputException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void StatsNew(CallDatabase db)
    {
        var list = db.LoadAll();
        Console.WriteLine("\n===== СТАТИСТИКА (NEW) =====");

        var total = list.Sum(c => c.Cost);
        var days = list.Select(c => c.Date.Date).Distinct().Count();
        Console.WriteLine($"Середня платня в день: {total / days:0.00} грн");

        decimal limit = InputHelper.ReadDecimal("\nПоріг вартості хвилини: ");
        var countHigh = list
            .Where(c => (c.Cost / c.Minutes) > limit)
            .Select(c => c.Date.Date)
            .Distinct()
            .Count();
        Console.WriteLine($"Днів з вартістю хвилини > {limit}: {countHigh}");

        var evenDays = list
            .Where(c => c.Minutes % 2 == 0)
            .Select(c => c.Date.ToString("dd.MM.yyyy"))
            .Distinct();
        Console.WriteLine("\nДні з парною кількістю хвилин:");
        foreach (var d in evenDays)
            Console.WriteLine(d);
    }

    static void DeleteFileNew()
    {
        if (File.Exists("calls.txt"))
        {
            File.Delete("calls.txt");
            Console.WriteLine("Файл БД видалено!");
        }
        else Console.WriteLine("Файл не існує.");
    }
}
