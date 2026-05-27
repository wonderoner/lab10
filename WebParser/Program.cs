using System;
using DatabaseLibrary;
using System.IO;

namespace WebParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Проверка работы с базой данных...");

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.db");

            using (var db = new DatabaseManager(dbPath))
            {
                Console.WriteLine("1. База данных создана");

                db.Add(1, "TestUser", "Hello World!");
                Console.WriteLine("2. Запись добавлена");

                var record = db.GetByID(1);
                Console.WriteLine($"3. Получена запись: ID={record?.ID}, Name={record?.Name}");

                db.Update(1, "Updated message!");
                Console.WriteLine("4. Запись обновлена");

                var records = db.GetByName("TestUser");
                Console.WriteLine($"5. Найдено записей: {records.Count}");

                db.Delete(1);
                Console.WriteLine("6. Запись удалена");
            }

            if (File.Exists(dbPath))
                File.Delete(dbPath);

            Console.WriteLine("\nВсе работает!");
            Console.ReadKey();
        }
    }
}