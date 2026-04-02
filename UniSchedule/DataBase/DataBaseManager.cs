using SQLite;
using UniSchedule.DataBase.Models;
using UniSchedule.System;

namespace UniSchedule.DataBase{
    public class DataBaseManager{
        private SQLiteConnection _db;
        private readonly string _dbPath;
        private readonly Debug _dbg;
        private readonly Localization _loc;

        public DataBaseManager(IConfiguration configuration, Debug dbg, Localization loc){
            _dbg = dbg;
            _loc = loc;
            _dbPath = configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Database path not found!");
            bool exist = File.Exists(_dbPath);
            _db = new SQLiteConnection(_dbPath);
            if (!exist)
            {
                _dbg.Warning(string.Format(_loc.Text["db_not_found"], _dbPath));
                _db.CreateTable<User>();
                _db.CreateTable<Department>();
                _dbg.Log(string.Format(_loc.Text["db_created"], _dbPath));
                createOpeartor();
            }
            _dbg.Log(string.Format(_loc.Text["db_connected"], _dbPath));
            _dbg.Log(string.Format(_loc.Text["db_stat"], this.GetAllUsers().Count, this.GetAllDepartments().Count));
        }

        public List<User> GetAllUsers() => _db.Table<User>().ToList();
        public async Task<List<User>> GetAllUsersAsync() => _db.Table<User>().ToList();
        public List<Department> GetAllDepartments() => _db.Table<Department>().ToList();
        public async Task<List<Department>> GetAllDepartmentsAsync() => _db.Table<Department>().ToList();

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
                _dbg.Log(string.Format(_loc.Text["db_user_r_err"], email));
                return true;
            }
            catch(Exception e){
                _dbg.Error(string.Format(_loc.Text["db_user_r_err"], email, e.Message));
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
                _dbg.Log(string.Format(_loc.Text["db_user_c"], email));
                return true;
            }
            catch(Exception e){
                _dbg.Error(string.Format(_loc.Text["db_user_c_err"], email, e.Message));
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
                _dbg.Log(string.Format(_loc.Text["db_user_u"], email));
                return true;
            }
            catch(Exception e){
                _dbg.Error(string.Format(_loc.Text["db_user_u_err"], email, e.Message));
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

        public async Task<bool> TryCreateDepartmentAsync(string name){
            try{
                if((await GetDepartmentByName(name))!=null) return false;
                _db.Insert(new Department{ Name = name });
                _dbg.Log(string.Format(_loc.Text["db_dep_c"], name));
                return true;
            }
            catch(Exception e){
                _dbg.Error(string.Format(_loc.Text["db_dep_c_err"], name, e.Message));
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
                _dbg.Log(string.Format(_loc.Text["db_dep_u"], name));
                return true;
            }
            catch(Exception e){
                _dbg.Error(string.Format(_loc.Text["db_dep_u_err"], name, e.Message));
                return false;
            }
        }

        public async Task<bool> TryRemoveDepartmentAsync(string name){
            try{
                var dep = await GetDepartmentByName(name);
                if(dep==null) return false;
                _db.Delete(dep);
                _dbg.Log(string.Format(_loc.Text["db_dep_r"], name));
                return true;
            }
            catch(Exception e){
                _dbg.Error(string.Format(_loc.Text["db_dep_r_err"], name, e.Message));
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
            _dbg.Log(_loc.Text["db_op_created"]);
            _dbg.Warning(string.Format(_loc.Text["db_op_info"], "operator@mauniver.ru", pass));
        }
    }
}
