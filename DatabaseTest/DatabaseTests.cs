using NUnit.Framework;
using DatabaseLibrary;
using System.IO;
using System;
using Microsoft.Data.Sqlite;

namespace DatabaseTests
{
    [TestFixture]
    public class DatabaseTests
    {
        private DatabaseManager _db;
        private string _dbPath;

        [SetUp]
        public void Setup()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.sqlite");
            _db = new DatabaseManager(_dbPath);
        }

        [TearDown]
        public void TearDown()
        {
            _db?.Dispose();
            _db = null;

            // Очищаем пул соединений SQLite
            SqliteConnection.ClearAllPools();

            // Ждем немного перед удалением файла
            System.Threading.Thread.Sleep(100);

            try
            {
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                }
            }
            catch (IOException)
            {
                // Если не удалось удалить, пробуем еще раз через секунду
                System.Threading.Thread.Sleep(1000);
                try
                {
                    if (File.Exists(_dbPath))
                    {
                        File.Delete(_dbPath);
                    }
                }
                catch
                {
                    // Игнорируем ошибку удаления в тестах
                }
            }
        }

        [Test]
        public void Add_ValidData_ShouldAddRecord()
        {
            _db.Add(1, "TestUser", "Test message");
            var result = _db.GetByID(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ID, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("TestUser"));
            Assert.That(result.Message, Is.EqualTo("Test message"));
        }

        [Test]
        public void Add_DuplicateID_ShouldReplaceRecord()
        {
            _db.Add(1, "User1", "Message1");
            _db.Add(1, "User2", "Message2");
            var result = _db.GetByID(1);

            Assert.That(result!.Name, Is.EqualTo("User2"));
            Assert.That(result.Message, Is.EqualTo("Message2"));
        }

        [Test]
        public void GetByID_ExistingID_ShouldReturnRecord()
        {
            _db.Add(1, "User", "Message");
            var result = _db.GetByID(1);

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void GetByID_NonExistingID_ShouldReturnNull()
        {
            var result = _db.GetByID(999);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetByName_ExistingName_ShouldReturnRecords()
        {
            _db.Add(1, "User1", "Message1");
            _db.Add(2, "User1", "Message2");
            var results = _db.GetByName("User1");

            Assert.That(results.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetByName_NonExistingName_ShouldReturnEmptyList()
        {
            var results = _db.GetByName("NonExisting");
            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Update_ExistingID_ShouldUpdateMessage()
        {
            _db.Add(1, "User", "Original");
            _db.Update(1, "Updated");
            var result = _db.GetByID(1);

            Assert.That(result!.Message, Is.EqualTo("Updated"));
        }

        [Test]
        public void Update_NonExistingID_ShouldNotThrowException()
        {
            Assert.DoesNotThrow(() => _db.Update(999, "New message"));
        }

        [Test]
        public void Delete_ExistingID_ShouldRemoveRecord()
        {
            _db.Add(1, "User", "Message");
            _db.Delete(1);
            var result = _db.GetByID(1);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Delete_NonExistingID_ShouldNotThrowException()
        {
            Assert.DoesNotThrow(() => _db.Delete(999));
        }
    }
}