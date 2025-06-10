using DogShelter.Entities;
using Dapper;
using Microsoft.Data.Sqlite;

namespace DogShelter
{
    internal class Program
    {
        static string connectionString = "Data Source=dogshelter.db";

        static void Main()
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();
            InitDatabase(connection);
            int option;
            do
            {
                Console.WriteLine("\nDog Shelter Menu:");
                Console.WriteLine("1. Add Dog");
                Console.WriteLine("2. View All Dogs");
                Console.WriteLine("3. View Dogs In Shelter");
                Console.WriteLine("4. View Adopted Dogs");
                Console.WriteLine("5. Search Dog");
                Console.WriteLine("6. Update Dog By Id");
                Console.WriteLine("7. Adopt Dog");
                Console.WriteLine("8. Exit");
                Console.Write("Select option: ");
                option = Int32.Parse(Console.ReadLine());

                switch (option)
                {
                    case 1:
                        AddDog(connection);
                        break;
                    case 2:
                        ViewAllDogs(connection);
                        break;
                    case 3:
                        ViewDogsByAdoptionStatus(connection, false);
                        break;
                    case 4:
                        ViewDogsByAdoptionStatus(connection, true);
                        break;
                    case 5:
                        SearchDog(connection);
                        break;
                    case 6:
                        UpdateDogById(connection);
                        break;
                    case 7:
                        AdoptDog(connection);
                        break;
                    default:
                        break;
                }
            } while (option != 8);
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
        static void AddDog(SqliteConnection connection)
        {
            Console.Write("Name: ");
            string name = Console.ReadLine()!;
            Console.Write("Age: ");
            int age = int.Parse(Console.ReadLine()!);
            Console.Write("Breed: ");
            string breed = Console.ReadLine()!;

            connection.Execute("INSERT INTO Dogs (Name, Age, Breed, IsAdopted, AdopterId) VALUES (@Name, @Age, @Breed, 0, NULL)",
                new { Name = name, Age = age, Breed = breed });

        }
        static void ViewAllDogs(SqliteConnection connection)
        {
            var dogs = connection.Query<Dog>("SELECT * FROM Dogs").ToList();
            DisplayDogs(dogs);
        }
        static void ViewDogsByAdoptionStatus(SqliteConnection connection, bool isAdopted)
        {
            var dogs = connection.Query<Dog>("SELECT * FROM Dogs WHERE IsAdopted = @isAdopted", new { isAdopted }).ToList();
            DisplayDogs(dogs);
        }
        static void SearchDog(SqliteConnection connection)
        {
            Console.WriteLine("Search by: 1. Id  2. Name  3. Breed");
            string option = Console.ReadLine()!;
            string query = "";
            object param = new();

            switch (option)
            {
                case "1":
                    Console.Write("Enter Id: ");
                    int id = int.Parse(Console.ReadLine()!);
                    query = "SELECT * FROM Dogs WHERE Id = @value";
                    param = new { value = id };
                    break;
                case "2":
                    Console.Write("Enter Name: ");
                    string name = Console.ReadLine()!;
                    query = "SELECT * FROM Dogs WHERE Name = @value";
                    param = new { value = name };
                    break;
                case "3":
                    Console.Write("Enter Breed: ");
                    string breed = Console.ReadLine()!;
                    query = "SELECT * FROM Dogs WHERE Breed = @value";
                    param = new { value = breed };
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    return;
            }

            var dogs = connection.Query<Dog>(query, param).ToList();
            DisplayDogs(dogs);
        }
        static void UpdateDogById(SqliteConnection connection)
        {
            Console.Write("Enter Dog Id to update: ");
            int id = int.Parse(Console.ReadLine()!);

            var dog = connection.QueryFirstOrDefault<Dog>("SELECT * FROM Dogs WHERE Id = @Id", new { Id = id });
            if (dog == null)
            {
                Console.WriteLine("Dog not found.");
                return;
            }

            Console.Write("New Name: ");
            string name = Console.ReadLine()!;
            Console.Write("New Age: ");
            string ageInput = Console.ReadLine()!;
            Console.Write("New Breed: ");
            string breed = Console.ReadLine()!;
            Console.Write("Is Adopted (true/false): ");
            string adoptedInput = Console.ReadLine()!;

            connection.Execute(@"
                UPDATE Dogs 
                SET Name = @Name, Age = @Age, Breed = @Breed, IsAdopted = @IsAdopted 
                WHERE Id = @Id",
                new
                {
                    Name = string.IsNullOrWhiteSpace(name) ? dog.Name : name,
                    Age = string.IsNullOrWhiteSpace(ageInput) ? dog.Age : int.Parse(ageInput),
                    Breed = string.IsNullOrWhiteSpace(breed) ? dog.Breed : breed,
                    IsAdopted = string.IsNullOrWhiteSpace(adoptedInput) ? dog.IsAdopted : bool.Parse(adoptedInput),
                    Id = id
                });

            Console.WriteLine("Dog updated.");
        }
        static void AdoptDog(SqliteConnection connection)
        {
            Console.Write("Enter Dog Id to adopt: ");
            int dogId = int.Parse(Console.ReadLine()!);

            var dog = connection.QueryFirstOrDefault<Dog>("SELECT * FROM Dogs WHERE Id = @Id", new { Id = dogId });

            if (dog == null)
            {
                Console.WriteLine("Dog not found.");
                return;
            }

            if (dog.IsAdopted)
            {
                Console.WriteLine("This dog is already adopted.");
                return;
            }

            Console.Write("Adopter's Name: ");
            string adopterName = Console.ReadLine()!;
            Console.Write("Phone Number: ");
            string phone = Console.ReadLine()!;

            var existingAdopter = connection.QueryFirstOrDefault<Adopter>(
                "SELECT * FROM Adopters WHERE Name = @Name AND PhoneNumber = @Phone",
                new { Name = adopterName, Phone = phone });

            int adopterId;

            if (existingAdopter == null)
            {
                connection.Execute("INSERT INTO Adopters (Name, PhoneNumber) VALUES (@Name, @Phone)",
                    new { Name = adopterName, Phone = phone });

                adopterId = connection.QuerySingle<int>("SELECT last_insert_rowid()");
            }
            else
            {
                adopterId = existingAdopter.Id;
            }

            connection.Execute("UPDATE Dogs SET IsAdopted = 1, AdopterId = @AdopterId WHERE Id = @Id",
                new { AdopterId = adopterId, Id = dogId });
        }
        static void DisplayDogs(List<Dog> dogs)
        {
            if (dogs.Count == 0)
            {
                Console.WriteLine("No dogs found.");
                return;
            }

            foreach (var dog in dogs)
            {
                string adoptedInfo = dog.IsAdopted ? $"Yes (AdopterId: {dog.AdopterId})" : "No";
                Console.WriteLine($"Id: {dog.Id}, Name: {dog.Name}, Age: {dog.Age}, Breed: {dog.Breed}, Adopted: {adoptedInfo}");
            }
        }

    }
}
