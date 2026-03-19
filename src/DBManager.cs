using SQLite;
using System.Security.Cryptography;
using System.Text;
using uni_schedule.Models;

namespace uni_schedule.src
{
    public class DBManager
    {
        private SQLiteConnection _db;

        public DBManager()
        {
            string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "schedule.db");
            _db = new SQLiteConnection(dbPath);
            _db.CreateTable<User>();
            Console.WriteLine($"БД: {dbPath}");
        }

        private string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        public bool AddUser(string mail, string password, string role = "user")
        {
            if (_db.Table<User>().Any(x => x.Mail == mail))
                return false;

            _db.Insert(new User { Mail = mail, Password = Hash(password), Role = role });
            return true;
        }

        public List<User> GetAllUsers() => _db.Table<User>().ToList();

        public User? FindUser(string mail) =>
            _db.Table<User>().FirstOrDefault(x => x.Mail == mail);

        public bool CheckPassword(string mail, string password)
        {
            var user = FindUser(mail);
            return user != null && user.Password == Hash(password);
        }

        public bool DeleteUser(string mail)
        {
            var user = FindUser(mail);
            if (user == null) return false;
            _db.Delete(user);
            return true;
        }
        public void AddTestUsers()
        {
            if (!_db.Table<User>().Any())
            {
                AddUser("admin@mauniver.ru", "admin123", "admin");
                AddUser("user@mauniver.ru", "user123", "user");
                Console.WriteLine("Тестовые пользователи");
            }
        }

        public void Close() => _db?.Close();
    }
}