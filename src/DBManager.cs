using SQLite;
using UniSchedule.Models;

namespace UniSchedule
{
    public class DBManager
    {
        private SQLiteConnection _db;
        private readonly string _dbPath;

        public DBManager(IConfiguration configuration)
        {
            _dbPath = configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Database path not found!");
            bool exist = File.Exists(_dbPath);
            _db = new SQLiteConnection(_dbPath);
            if (!exist)
            {
                Console.WriteLine($"Database {_dbPath} is not exist!\nCreate new DB with new table 'Users'");
                _db.CreateTable<User>();
                string g_pass = UniSchedule.Crypto.GeneratePassword(12);
                if (this.TryAddUser("operator@mauniver.ru", g_pass, "operator"))
                {
                    Console.WriteLine($"New user generated: email: operator@mauniver.ru password: {g_pass}");
                }
            }
            Console.WriteLine($"Database connection is active: {_dbPath}");
            Console.WriteLine($"Registered users: {this.GetAllUsers().Count}");
        }

        public bool TryAddUser(string mail, string password, string role = "user")
        {
            if (_db.Table<User>().Any(x => x.Mail == mail))
                return false;

            _db.Insert(new User
            {
                Mail = mail,
                Password = UniSchedule.Crypto.MD5HashCreate(password),
                Role = role
            });

            return true;
        }

        public List<User> GetAllUsers() => _db.Table<User>().ToList();

        public User? FindUser(string mail) =>
            _db.Table<User>().FirstOrDefault(x => x.Mail == mail);

        public User? FindUserById(int id) =>
            _db.Table<User>().FirstOrDefault(x => x.Id == id);

        public User? TryLogin(string mail, string password)
        {
            User? user = FindUser(mail);
            return (user != null && user.Password == UniSchedule.Crypto.MD5HashCreate(password)) ? user : null;
        }

        public bool TryDeleteUser(string mail)
        {
            var user = FindUser(mail);
            if (user == null) return false;
            _db.Delete(user);
            return true;
        }

        public bool TryDeleteUserById(int id)
        {
            var user = FindUserById(id);
            if (user == null) return false;
            _db.Delete(user);
            return true;
        }

        public bool TryUpdateUser(int id, string? newMail, string? newRole, string? newPassword)
        {
            try
            {
                var user = FindUserById(id);
                if (user == null) return false;

                // Нельзя менять оператора
                if (user.Role == "operator" && (newRole != null && newRole != "operator"))
                {
                    Console.WriteLine("Cannot modify operator role");
                    return false;
                }

                // Обновляем email
                if (!string.IsNullOrEmpty(newMail) && newMail != user.Mail)
                {
                    // Проверяем, не занят ли email другим пользователем
                    var existingUser = FindUser(newMail);
                    if (existingUser != null && existingUser.Id != id)
                    {
                        Console.WriteLine($"Email {newMail} already used by another user");
                        return false;
                    }
                    user.Mail = newMail;
                }

                // Обновляем роль
                if (!string.IsNullOrEmpty(newRole))
                {
                    user.Role = newRole;
                }

                // Обновляем пароль
                if (!string.IsNullOrEmpty(newPassword))
                {
                    user.Password = UniSchedule.Crypto.MD5HashCreate(newPassword);
                }

                _db.Update(user);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return false;
            }
        }

        public void Close() => _db?.Close();
    }
}