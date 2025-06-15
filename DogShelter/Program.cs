using DogShelter.Entities;
using Dapper;
using Microsoft.Data.Sqlite;
using Z.Dapper.Plus;

namespace DogShelter
{
    internal class Program
    {
        static void Main()
        {
            DapperPlusManager.Entity<Dog>().Table("Dogs").Key(d => d.Id);
            string connectionString = "Data Source=dogshelter.db";
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();
            InitDatabase(connection);
            var dogs = new List<Dog>
            {
                new Dog { Name = "Барон", Age = 5, Breed = "Німецька вівчарка" },
                new Dog { Name = "Джессі", Age = 3, Breed = "Лабрадор" },
                new Dog { Name = "Рекс", Age = 4, Breed = "Хаскі" },
                new Dog { Name = "Бім", Age = 2, Breed = "Такса" },
                new Dog { Name = "Лорд", Age = 6, Breed = "Доберман" },
                new Dog { Name = "Лакі", Age = 1, Breed = "Мопс" },
                new Dog { Name = "Джек", Age = 7, Breed = "Ретрівер" },
                new Dog { Name = "Макс", Age = 5, Breed = "Алабай" },
                new Dog { Name = "Шарік", Age = 2, Breed = "Безпородний" },
                new Dog { Name = "Соня", Age = 4, Breed = "Кокер-спанієль" },
                new Dog { Name = "Тузик", Age = 3, Breed = "Чау-чау" },
                new Dog { Name = "Леді", Age = 6, Breed = "Бультер'єр" },
                new Dog { Name = "Зевс", Age = 2, Breed = "Пітбуль" },
                new Dog { Name = "Грейс", Age = 5, Breed = "Хаскі" },
                new Dog { Name = "Арчі", Age = 1, Breed = "Лабрадор" },
                new Dog { Name = "Рокі", Age = 7, Breed = "Доберман" },
                new Dog { Name = "Белла", Age = 4, Breed = "Шпіц" },
                new Dog { Name = "Лаккі", Age = 3, Breed = "Безпородний" },
                new Dog { Name = "Оскар", Age = 6, Breed = "Алабай" },
                new Dog { Name = "Нора", Age = 2, Breed = "Кавалер Кінг Чарльз" }
            };

            connection.BulkInsert(dogs);
        }

        static void InitDatabase(SqliteConnection connection)
        {
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Adopters (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    PhoneNumber TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Dogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Age INTEGER NOT NULL,
                    Breed TEXT NOT NULL,
                    IsAdopted BOOLEAN NOT NULL,
                    AdopterId INTEGER,
                    FOREIGN KEY (AdopterId) REFERENCES Adopters(Id)
                );

            ");
        }

    }
}
