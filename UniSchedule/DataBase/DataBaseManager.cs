using SQLite;
using UniSchedule.DataBase.Models;

namespace UniSchedule.DataBase{
    public class DataBaseManager{
        private SQLiteConnection _db;
        private readonly string _dbPath;

        public DataBaseManager(IConfiguration configuration){
            _dbPath = configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Database path not found!");
            bool exist = File.Exists(_dbPath);
            _db = new SQLiteConnection(_dbPath);
            if (!exist)
            {
                Console.WriteLine($"Database {_dbPath} is not exist!\nCreate new DB with new tables 'Users' and 'Departments'");
                _db.CreateTable<User>();
                _db.CreateTable<Department>();
                createOpeartor();
            }
            Console.WriteLine($"Database connection is active: {_dbPath}");
            Console.WriteLine($"Registered users: {this.GetAllUsers().Count}");
        }

        public List<User> GetAllUsers() => _db.Table<User>().ToList();
        public async Task<List<User>> GetAllUsersAsync() => _db.Table<User>().ToList();

        public async Task<User?> AuthenticateUserAsync(string email, string password){
            var user = await GetUserByEmailAsync(email);
            if(user == null) return null;
            if (!VerifyPassword(password, user.Password))
                return null;
            return user;
        }

        public async Task<bool> TryRemoveUserAsync(string email){
            var user = await GetUserByEmailAsync(email);
            if(user==null) return false;

            try{
                _db.Delete(user);
                return true;
            }
            catch(Exception){
                return false;
            }
        }

        public async Task<bool> TryCreateUserAsync(
                string email,
                string password,
                string name,
                string engName,
                string role){
            try{
                if(GetUserByEmail(email)!=null) return false;
                _db.Insert(new User(){
                    Mail = email,
                    Password = Crypto.MD5HashCreate(password),
                    Name = name,
                    EngName = engName,
                    Role = role,
                    DepartmentId = null
                });
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Creating new user throw exception: " + e.Message);
                return false;
            }
        }

        public async Task<bool> TryUpdateUserAsync(
                string email,
                string? new_email,
                string? new_password,
                string? new_name,
                string? new_engName,
                string? new_role,
                int? department){
            try{
                var user = await GetUserByEmailAsync(email);
                if(user==null) return false;
                if(new_email!=null && GetUserByEmail(new_email)!=null)
                    return false;

                if(new_email!=null) user.Mail = new_email;
                if(new_password!=null) user.Password = Crypto.MD5HashCreate(new_password);
                if(new_name!=null) user.Name = new_name;
                if(new_engName!=null) user.EngName = new_engName;
                if(new_role!=null) user.Role = new_role;
                if(department!=null) user.DepartmentId = department;

                _db.Update(user);

                return true;
            }
            catch(Exception e){
                Console.WriteLine("Updating user throw exception: " + e.Message);
                return false;
            }
        }

        public bool VerifyPassword(string password, string passwordHash) =>
            Crypto.MD5HashCreate(password) == passwordHash;

        public User? GetUserByEmail(string email) =>
            _db.Table<User>().FirstOrDefault(x => x.Mail == email);

        public async Task<User?> GetUserByEmailAsync(string email) =>
            _db.Table<User>().Where(x => x.Mail == email).FirstOrDefault();

        public User? FindUserById(int id) =>
            _db.Table<User>().FirstOrDefault(x => x.Id == id);

        public async Task<Department?> GetDepartmentByName(string name) =>
            _db.Table<Department>().FirstOrDefault(x => x.Name == name);

        public async Task<Department?> GetDepartment(int id) =>
            _db.Table<Department>().FirstOrDefault(x => x.Id == id);

        public async Task<List<Department>> GetAllDepartmentsAsync() =>
            _db.Table<Department>().ToList();

        public async Task<bool> TryCreateDepartmentAsync(string name){
            try{
                if((await GetDepartmentByName(name))!=null) return false;
                _db.Insert(new Department{ Name = name });
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Creating new department throw exception: " + e.Message);
                return false;
            }
        }

        public async Task<bool> TryUpdateDepartmentAsync(string name, string new_name){
            try{
                var dep = await GetDepartmentByName(name);
                if(dep==null) return false;
                if((await GetDepartmentByName(new_name))!=null) return false;
                dep.Name = new_name;
                _db.Update(dep);
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Updating new department throw exception: " + e.Message);
                return false;
            }
        }

        public async Task<bool> TryRemoveDepartmentAsync(string name){
            try{
                var dep = await GetDepartmentByName(name);
                if(dep==null) return false;
                _db.Delete(dep);
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Removing new department throw exception: " + e.Message);
                return false;
            }
        }

        public void Close() => _db?.Close();

        private void createOpeartor(){
            var pass = UniSchedule.Crypto.GeneratePassword(12);
            _db.Insert(new User(){
                Mail = "operator@mauniver.ru",
                Password = Crypto.MD5HashCreate(pass),
                Name = "Оператор",
                EngName = "Operator",
                Role = "operator",
                DepartmentId = null
            });
            Console.WriteLine("Created first user 'operator'\nFor login you can use UI with this data:\n\t" +
                    "Mail: operator@mauniver.ru\n\t" +
                    $"Password: {pass}");
        }
    }
}
