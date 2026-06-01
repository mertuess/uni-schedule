using SQLite;
using UniSchedule.API;
using UniSchedule.DataBase.Models;
using UniSchedule.Json.Models;
using UniSchedule.System;

namespace UniSchedule.DataBase;

public class DataBaseManager
{
    private readonly SQLiteConnection _db;
    private readonly Debug _dbg;
    private readonly string _dbPath;
    private readonly Localization _loc;
    private readonly OutAPI _outApi;

    public DataBaseManager(IConfiguration configuration, Debug dbg, Localization loc, OutAPI outApi )
    {
        _dbg = dbg;
        _loc = loc;
        _outApi = outApi;
        _dbPath = configuration.GetConnectionString("DefaultConnection") ??
                  throw new Exception("Database path not found!");
        var exist = File.Exists(_dbPath);
        _db = new SQLiteConnection(_dbPath);
        if (!exist)
        {
            _dbg.Warning(string.Format(_loc.Text["db_not_found"], _dbPath));
            _db.CreateTable<User>();
            _db.CreateTable<Department>();
            _dbg.Log(string.Format(_loc.Text["db_created"], _dbPath));
            createOpeartor();
        }

        _db.CreateTable<TeacherBinding>();
        _dbg.Log("Таблица TeacherBindings проверена/создана");

        _dbg.Log(string.Format(_loc.Text["db_connected"], _dbPath));
        _dbg.Log(string.Format(_loc.Text["db_stat"], GetAllUsers().Count, GetAllDepartments().Count));
    }

    public List<User> GetAllUsers()
    {
        return _db.Table<User>().ToList();
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return _db.Table<User>().ToList();
    }

    public List<Department> GetAllDepartments()
    {
        return _db.Table<Department>().ToList();
    }

    public async Task<List<Department>> GetAllDepartmentsAsync()
    {
        return _db.Table<Department>().ToList();
    }

    public async Task<User?> AuthenticateUserAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null) return null;
        if (!VerifyPassword(password, user.Password))
            return null;
        return user;
    }

    public async Task<bool> TryRemoveUserAsync(string email)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null) return false;

        try
        {
            _db.Delete(user);
            _dbg.Log(string.Format(_loc.Text["db_user_r_err"], email));
            return true;
        }
        catch (Exception e)
        {
            _dbg.Error(string.Format(_loc.Text["db_user_r_err"], email, e.Message));
            return false;
        }
    }

    public async Task<bool> TryCreateUserAsync(
        string email,
        string password,
        string name,
        string engName,
        string role)
    {
        try
        {
            if (GetUserByEmail(email) != null) return false;
            _db.Insert(new User
            {
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
        catch (Exception e)
        {
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
        int? department)
    {
        try
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null) return false;
            if (new_email != null && GetUserByEmail(new_email) != null)
                return false;

            if (new_email != null) user.Mail = new_email;
            if (new_password != null) user.Password = Crypto.MD5HashCreate(new_password);
            if (new_name != null) user.Name = new_name;
            if (new_engName != null) user.EngName = new_engName;
            if (new_role != null) user.Role = new_role;
            if (department != null) user.DepartmentId = department;

            _db.Update(user);
            _dbg.Log(string.Format(_loc.Text["db_user_u"], email));
            return true;
        }
        catch (Exception e)
        {
            _dbg.Error(string.Format(_loc.Text["db_user_u_err"], email, e.Message));
            return false;
        }
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return Crypto.MD5HashCreate(password) == passwordHash;
    }

    public User? GetUserByEmail(string email)
    {
        return _db.Table<User>().FirstOrDefault(x => x.Mail == email);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return _db.Table<User>().Where(x => x.Mail == email).FirstOrDefault();
    }

    public User? FindUserById(int id)
    {
        return _db.Table<User>().FirstOrDefault(x => x.Id == id);
    }

    public async Task<Department?> GetDepartmentByName(string name)
    {
        return _db.Table<Department>().FirstOrDefault(x => x.Name == name);
    }

    public async Task<Department?> GetDepartment(int id)
    {
        return _db.Table<Department>().FirstOrDefault(x => x.Id == id);
    }

    public async Task<bool> TryCreateDepartmentAsync(string name)
    {
        try
        {
            if (await GetDepartmentByName(name) != null) return false;
            _db.Insert(new Department { Name = name });
            _dbg.Log(string.Format(_loc.Text["db_dep_c"], name));
            return true;
        }
        catch (Exception e)
        {
            _dbg.Error(string.Format(_loc.Text["db_dep_c_err"], name, e.Message));
            return false;
        }
    }

    public async Task<bool> TryUpdateDepartmentAsync(string name, string new_name)
    {
        try
        {
            var dep = await GetDepartmentByName(name);
            if (dep == null) return false;
            if (await GetDepartmentByName(new_name) != null) return false;
            dep.Name = new_name;
            _db.Update(dep);
            _dbg.Log(string.Format(_loc.Text["db_dep_u"], name));
            return true;
        }
        catch (Exception e)
        {
            _dbg.Error(string.Format(_loc.Text["db_dep_u_err"], name, e.Message));
            return false;
        }
    }

    public async Task<bool> TryRemoveDepartmentAsync(string name)
    {
        try
        {
            var dep = await GetDepartmentByName(name);
            if (dep == null) return false;
            _db.Delete(dep);
            _dbg.Log(string.Format(_loc.Text["db_dep_r"], name));
            return true;
        }
        catch (Exception e)
        {
            _dbg.Error(string.Format(_loc.Text["db_dep_r_err"], name, e.Message));
            return false;
        }
    }

    public void Close()
    {
        _db?.Close();
    }

    private void createOpeartor()
    {
        var pass = Crypto.GeneratePassword(12);
        _db.Insert(new User
        {
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

    /// <summary>
    /// Инициализация таблицы TeacherBindings
    /// </summary>
    public void EnsureTeacherBindingsTable()
    {
        try
        {
            _db.CreateTable<TeacherBinding>();
            _dbg.Log("Таблица TeacherBindings готова");
        }
        catch (Exception ex)
        {
            _dbg.Error("Ошибка при создании таблицы TeacherBindings: " + ex.Message);
        }
    }

    /// <summary>
    /// Модель привязки преподавателя к кафедре
    /// </summary>
    [Table("TeacherBindings")]
    public class TeacherBinding
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string UniversityUid { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        [Indexed]
        public int? DepartmentId { get; set; }
    }

    /// <summary>
    /// Получить всех преподавателей из внешнего API
    /// </summary>
    public async Task<List<ExternalTeacher>> GetAllTeachersExternalAsync()
    {
        try
        {
            return await _outApi.SendRequest<ExternalTeacher>("/teachers", "teachers");
        }
        catch (Exception ex)
        {
            _dbg.Error("Ошибка получения списка преподавателей: " + ex.Message);
            return new List<ExternalTeacher>();
        }
    }

    /// <summary>
    /// Модель преподавателя из внешнего API
    /// </summary>
    public class ExternalTeacher
    {
        public string UID { get; set; } = string.Empty;
        public string teacher { get; set; } = string.Empty;
        public string faculty { get; set; } = string.Empty;
    }

    /// <summary>
    /// Привязать преподавателя к кафедре (или отвязать, если departmentId = null)
    /// </summary>
    public bool BindTeacher(string universityUid, string name, int? departmentId)
    {
        try
        {
            var binding = _db.Table<TeacherBinding>().FirstOrDefault(x => x.UniversityUid == universityUid);

            if (binding == null)
            {
                binding = new TeacherBinding
                {
                    UniversityUid = universityUid,
                    Name = name,
                    DepartmentId = departmentId
                };
                _db.Insert(binding);
                _dbg.Log($"Создана привязка: {name} на кафедре {departmentId}");
            }
            else
            {
                binding.Name = name;
                binding.DepartmentId = departmentId;
                _db.Update(binding);
                _dbg.Log($"Обновлена привязка: {name} на кафедре {departmentId}");
            }
            return true;
        }
        catch (Exception ex)
        {
            _dbg.Error($"Ошибка привязки {universityUid}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Получить преподавателей, привязанных к кафедре
    /// </summary>
    public List<TeacherBinding> GetTeachersByDepartment(int departmentId)
    {
        return _db.Table<TeacherBinding>().Where(x => x.DepartmentId == departmentId).ToList();
    }

    /// <summary>
    /// Получить расписание кафедры: агрегировать расписания всех привязанных преподавателей
    /// </summary>
    public async Task<List<AggregatedScheduleItem>> GetDepartmentScheduleAsync(int departmentId, string start, string end)
    {
        var bindings = GetTeachersByDepartment(departmentId);
        var result = new List<AggregatedScheduleItem>();

        foreach (var binding in bindings)
        {
            try
            {
                var schedule = await _outApi.SendRequest<TeacherSchedule>(
                    $"/teachers/{binding.UniversityUid}/schedule/{start}/{end}",
                    "schedule");

                if (schedule != null)
                {
                    foreach (var item in schedule)
                    {
                        result.Add(new AggregatedScheduleItem
                        {
                            Date = item.date,
                            Slot = item.slot,
                            DayOfWeek = item.day_of_week,
                            Type = item.type,
                            Disciplines = item.disciplines,
                            Room = item.room,
                            TeacherName = binding.Name,
                            TeacherUid = binding.UniversityUid,
                            DepartmentId = departmentId
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _dbg.Warning($"Не удалось получить расписание для {binding.Name}: {ex.Message}");
            }
        }

        return result.OrderBy(x => x.Date).ThenBy(x => x.Slot).ToList();
    }

    /// <summary>
    /// Агрегированный элемент расписания кафедры
    /// </summary>
    public class AggregatedScheduleItem
    {
        public string Date { get; set; } = string.Empty;
        public string Slot { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Disciplines { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string TeacherUid { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
    }

};