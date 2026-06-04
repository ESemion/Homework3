using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace BmstuHomework3
{
    // =========================================================================
    // 1. МОДЕЛИ ДАННЫХ (ENTITY КЛАССЫ)
    // =========================================================================

    /// <summary>Отдел (Справочник / Сторона «Один»)</summary>
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Навигационное свойство для связи один-ко-многим
        public virtual ICollection<Developer> Developers { get; set; } = new List<Developer>();

        // Переопределение для корректного отображения в ComboBox
        public override string ToString() => Name;
    }

    /// <summary>Разработчик (Основная таблица / Сторона «Много»)</summary>
    public class Developer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int DepartmentId { get; set; }
        public virtual Department? Department { get; set; }

        private int _commits;
        /// <summary>Количество коммитов с бизнес-валидацией поля</summary>
        public int Commits
        {
            get => _commits;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Количество коммитов не может быть отрицательным!");
                _commits = value;
            }
        }
    }

    // =========================================================================
    // 2. КОНТЕКСТ БАЗЫ ДАННЫХ (EF CORE + SQLITE)
    // =========================================================================

    public class AppDbContext : DbContext
    {
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Developer> Developers { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Используем локальную базу данных SQLite
            optionsBuilder.UseSqlite("Data Source=software_company.db");
        }

        /// <summary>Первичное автоматическое заполнение базы данных (минимум 4 отдела и 12 разработчиков)</summary>
        public void SeedDatabase()
        {
            Database.EnsureCreated();

            if (!Departments.Any())
            {
                var d1 = new Department { Name = "Backend Разработка" };
                var d2 = new Department { Name = "Frontend Разработка" };
                var d3 = new Department { Name = "Data Science & AI" };
                var d4 = new Department { Name = "DevOps & Инфраструктура" };

                Departments.AddRange(d1, d2, d3, d4);
                SaveChanges(); // Сохраняем, чтобы получить сгенерированные Id

                Developers.AddRange(new List<Developer>
                {
                    new Developer { Name = "Александр Иванов", Commits = 145, DepartmentId = d1.Id },
                    new Developer { Name = "Михаил Петров", Commits = 210, DepartmentId = d1.Id },
                    new Developer { Name = "Дмитрий Сидоров", Commits = 95, DepartmentId = d1.Id },

                    new Developer { Name = "Анна Кузнецова", Commits = 180, DepartmentId = d2.Id },
                    new Developer { Name = "Елена Павлова", Commits = 120, DepartmentId = d2.Id },
                    new Developer { Name = "Игорь Соколов", Commits = 65, DepartmentId = d2.Id },

                    new Developer { Name = "Владимир Волков", Commits = 320, DepartmentId = d3.Id },
                    new Developer { Name = "Светлана Орлова", Commits = 280, DepartmentId = d3.Id },
                    new Developer { Name = "Константин Морозов", Commits = 150, DepartmentId = d3.Id },

                    new Developer { Name = "Роман Федоров", Commits = 45, DepartmentId = d4.Id },
                    new Developer { Name = "Артем Попов", Commits = 90, DepartmentId = d4.Id },
                    new Developer { Name = "Николай Васильев", Commits = 115, DepartmentId = d4.Id }
                });
                SaveChanges();
            }
        }
    }

    // =========================================================================
    // 3. ТОЧКА ВХОДА В ПРИЛОЖЕНИЕ (ОПЕРАТОРЫ ВЕРХНЕГО УРОВНЯ НЕ ИСПОЛЬЗУЮТСЯ ДЛЯ СОВМЕСТИМОСТИ)
    // =========================================================================

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Инициализируем и заполняем БД перед запуском интерфейса
            using (var context = new AppDbContext())
            {
                context.SeedDatabase();
            }

            Application.Run(new MainForm());
        }
    }

    // =========================================================================
    // 4. ПОЛЬЗОВАТЕЛЬСКИЙ ИНТЕРФЕЙС (ПРОГРАММНЫЙ WINFORMS)
    // =========================================================================

    public class MainForm : Form
    {
        private TabControl tabControl;
        private TabPage tabPageDepartments;
        private TabPage tabPageDevelopers;
        private TabPage tabPageReports;

        // Элементы вкладки "Отделы"
        private DataGridView dgvDepartments;
        private TextBox txtDeptName;
        private Button btnAddDept;
        private Button btnDeleteDept;

        // Элементы вкладки "Разработчики"
        private DataGridView dgvDevelopers;
        private TextBox txtDevName;
        private NumericUpDown numDevCommits;
        private ComboBox cbDevDepartment;
        private Button btnAddDev;
        private Button btnDeleteDev;

        // Элементы вкладки "Отчеты"
        private DataGridView dgvReports;
        private Button btnReport1;
        private Button btnReport2;
        private Button btnReport3;

        public MainForm()
        {
            this.Text = "Управление ИТ-Департаментом (EF Core + SQLite)";
            this.Size = new Size(850, 550);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeComponents();
            LoadData();
        }

        private void InitializeComponents()
        {
            tabControl = new TabControl { Dock = DockStyle.Fill };
            tabPageDepartments = new TabPage { Text = "Справочник: Отделы" };
            tabPageDevelopers = new TabPage { Text = "Основная таблица: Разработчики" };
            tabPageReports = new TabPage { Text = "Аналитические Отчеты" };

            // -----------------------------------------------------------------
            // ВЕРСТКА: ОТДЕЛЫ
            // -----------------------------------------------------------------
            dgvDepartments = new DataGridView { Location = new Point(15, 15), Size = new Size(450, 430), AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };

            Label lblDeptName = new Label { Text = "Название отдела:", Location = new Point(480, 20), Size = new Size(120, 20) };
            txtDeptName = new TextBox { Location = new Point(480, 45), Size = new Size(320, 25) };

            btnAddDept = new Button { Text = "Добавить отдел", Location = new Point(480, 85), Size = new Size(150, 35) };
            btnDeleteDept = new Button { Text = "Удалить выбранный", Location = new Point(650, 85), Size = new Size(150, 35) };

            btnAddDept.Click += BtnAddDept_Click;
            btnDeleteDept.Click += BtnDeleteDept_Click;

            tabPageDepartments.Controls.AddRange(new Control[] { dgvDepartments, lblDeptName, txtDeptName, btnAddDept, btnDeleteDept });

            // -----------------------------------------------------------------
            // ВЕРСТКА: РАЗРАБОТЧИКИ
            // -----------------------------------------------------------------
            dgvDevelopers = new DataGridView { Location = new Point(15, 15), Size = new Size(450, 430), AutoGenerateColumns = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
            dgvDevelopers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
            dgvDevelopers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "ФИО Разработчика", Width = 160 });
            dgvDevelopers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Commits", HeaderText = "Коммиты", Width = 80 });
            dgvDevelopers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Department", HeaderText = "Отдел", Width = 130 });

            Label lblDevName = new Label { Text = "ФИО Разработчика:", Location = new Point(480, 20), Size = new Size(150, 20) };
            txtDevName = new TextBox { Location = new Point(480, 45), Size = new Size(320, 25) };

            Label lblDevCommits = new Label { Text = "Количество коммитов:", Location = new Point(480, 85), Size = new Size(150, 20) };
            numDevCommits = new NumericUpDown { Location = new Point(480, 110), Size = new Size(320, 25), Minimum = -100, Maximum = 100000 }; // Намеренно разрешаем -100 для проверки валидации

            Label lblDevDept = new Label { Text = "Выбрать отдел (Категория):", Location = new Point(480, 150), Size = new Size(200, 20) };
            cbDevDepartment = new ComboBox { Location = new Point(480, 175), Size = new Size(320, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            btnAddDev = new Button { Text = "Добавить сотрудника", Location = new Point(480, 220), Size = new Size(150, 35) };
            btnDeleteDev = new Button { Text = "Удалить выбранного", Location = new Point(650, 220), Size = new Size(150, 35) };

            btnAddDev.Click += BtnAddDev_Click;
            btnDeleteDev.Click += BtnDeleteDev_Click;

            tabPageDevelopers.Controls.AddRange(new Control[] { dgvDevelopers, lblDevName, txtDevName, lblDevCommits, numDevCommits, lblDevDept, cbDevDepartment, btnAddDev, btnDeleteDev });

            // -----------------------------------------------------------------
            // ВЕРСТКА: ОТЧЕТЫ (LINQ ЗАПРОСЫ)
            // -----------------------------------------------------------------
            dgvReports = new DataGridView { Location = new Point(15, 70), Size = new Size(800, 370), AutoGenerateColumns = true, ReadOnly = true };

            btnReport1 = new Button { Text = "Раздел 1: Все сотрудники с отделом", Location = new Point(15, 15), Size = new Size(250, 40) };
            btnReport2 = new Button { Text = "Раздел 2: Кол-во сотрудников по отделам", Location = new Point(280, 15), Size = new Size(270, 40) };
            btnReport3 = new Button { Text = "Раздел 3: Средние коммиты по отделам", Location = new Point(565, 15), Size = new Size(250, 40) };

            btnReport1.Click += BtnReport1_Click;
            btnReport2.Click += BtnReport2_Click;
            btnReport3.Click += BtnReport3_Click;

            tabPageReports.Controls.AddRange(new Control[] { dgvReports, btnReport1, btnReport2, btnReport3 });

            // Добавление вкладок в TabControl
            tabControl.TabPages.AddRange(new TabPage[] { tabPageDepartments, tabPageDevelopers, tabPageReports });
            this.Controls.Add(tabControl);
        }

        private void LoadData()
        {
            using (var context = new AppDbContext())
            {
                // Загрузка отделов
                var depts = context.Departments.OrderBy(d => d.Name).ToList();
                dgvDepartments.DataSource = depts;

                // Обновляем комбобокс для добавления разработчиков
                cbDevDepartment.DataSource = depts;
                cbDevDepartment.DisplayMember = "Name";
                cbDevDepartment.ValueMember = "Id";

                // Загрузка разработчиков с явным включением связанных сущностей (Include)
                var devs = context.Developers.Include(d => d.Department).ToList();
                dgvDevelopers.DataSource = devs;
            }
        }

        // =========================================================================
        // ЛОГИКА ВЗАИМОДЕЙСТВИЯ (ОБРАБОТЧИКИ СОБЫТИЙ)
        // =========================================================================

        private void BtnAddDept_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDeptName.Text))
            {
                MessageBox.Show("Заполните название отдела!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var context = new AppDbContext())
            {
                var dept = new Department { Name = txtDeptName.Text.Trim() };
                context.Departments.Add(dept);
                context.SaveChanges();
            }
            txtDeptName.Clear();
            LoadData();
        }

        private void BtnDeleteDept_Click(object? sender, EventArgs e)
        {
            if (dgvDepartments.CurrentRow == null) return;
            var selectedDept = (Department)dgvDepartments.CurrentRow.DataBoundItem;

            using (var context = new AppDbContext())
            {
                // Проверка бизнес-логики: Запрет удаления, если в отделе есть сотрудники
                bool hasDependencies = context.Developers.Any(d => d.DepartmentId == selectedDept.Id);
                if (hasDependencies)
                {
                    MessageBox.Show("Мягкая ошибка удаления: Нельзя удалить этот отдел, так как к нему привязаны сотрудники!",
                                    "Бизнес-логика", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var dept = context.Departments.Find(selectedDept.Id);
                if (dept != null)
                {
                    context.Departments.Remove(dept);
                    context.SaveChanges();
                }
            }
            LoadData();
        }

        private void BtnAddDev_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDevName.Text) || cbDevDepartment.SelectedItem == null)
            {
                MessageBox.Show("Заполните ФИО и выберите отдел!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var context = new AppDbContext())
                {
                    var dev = new Developer
                    {
                        Name = txtDevName.Text.Trim(),
                        DepartmentId = (int)cbDevDepartment.SelectedValue!,
                        // Здесь может сработать валидация свойства (отрицательное число)
                        Commits = (int)numDevCommits.Value
                    };

                    context.Developers.Add(dev);
                    context.SaveChanges();
                }
                txtDevName.Clear();
                numDevCommits.Value = 0;
                LoadData();
            }
            catch (ArgumentException ex)
            {
                // Перехват исключения валидации из бизнес-логики класса Developer
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Валидация данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnDeleteDev_Click(object? sender, EventArgs e)
        {
            if (dgvDevelopers.CurrentRow == null) return;
            var selectedDev = (Developer)dgvDevelopers.CurrentRow.DataBoundItem;

            using (var context = new AppDbContext())
            {
                var dev = context.Developers.Find(selectedDev.Id);
                if (dev != null)
                {
                    context.Developers.Remove(dev);
                    context.SaveChanges();
                }
            }
            LoadData();
        }

        // =========================================================================
        // ЛОГИКА ОТЧЕТОВ ПО ДОМАШНЕМУ ЗАДАНИЮ (ИСКЛЮЧИТЕЛЬНО LINQ TO ENTITIES)
        // =========================================================================

        private void BtnReport1_Click(object? sender, EventArgs e)
        {
            using (var context = new AppDbContext())
            {
                // Раздел 1. Полный список записей основной таблицы с названием категории
                var report1 = context.Developers
                    .Include(d => d.Department)
                    .OrderBy(d => d.Name)
                    .Select(d => new
                    {
                        Идентификатор = d.Id,
                        ФИО_Сотрудника = d.Name,
                        Коммиты = d.Commits,
                        Название_Отдела = d.Department != null ? d.Department.Name : "Нет отдела"
                    })
                    .ToList();

                dgvReports.DataSource = report1;
            }
        }

        private void BtnReport2_Click(object? sender, EventArgs e)
        {
            using (var context = new AppDbContext())
            {
                // Раздел 2. Группировка с агрегатной функцией Count()
                var report2 = context.Developers
                    .GroupBy(d => d.Department!.Name)
                    .Select(g => new
                    {
                        Название_Отдела = g.Key ?? "Без отдела",
                        Количество_Разработчиков = g.Count()
                    })
                    .OrderBy(r => r.Название_Отдела)
                    .ToList();

                dgvReports.DataSource = report2;
            }
        }

        private void BtnReport3_Click(object? sender, EventArgs e)
        {
            using (var context = new AppDbContext())
            {
                // Раздел 3. Группировка с функцией Average() и сортировкой по убыванию значения
                var report3 = context.Developers
                    .GroupBy(d => d.Department!.Name)
                    .Select(g => new
                    {
                        Название_Отдела = g.Key ?? "Без отдела",
                        Среднее_Количество_Коммитов = Math.Round(g.Average(d => d.Commits), 2)
                    })
                    .OrderByDescending(r => r.Среднее_Количество_Коммитов)
                    .ToList();

                dgvReports.DataSource = report3;
            }
        }
    }
}