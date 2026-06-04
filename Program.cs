using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace BmstuHomework3
{
    // =========================================================================
    // 1. МОДЕЛИ ДАННЫХ (ENTITY-КЛАССЫ) ПО ВАРИАНТУ №7
    // =========================================================================

    /// <summary>
    /// Музыкальный альбом (Справочная таблица - сторона "один")
    /// </summary>
    public class Album
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        // Навигационное свойство для связи с песнями
        public ICollection<Song> Songs { get; set; } = new List<Song>();
    }

    /// <summary>
    /// Песня / Трек (Основная таблица - сторона "много")
    /// </summary>
    public class Song
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        // Числовое поле по варианту №7 (длительность в секундах)
        public int DurationSec { get; set; }

        // Связь со справочной таблицей
        public int AlbumId { get; set; }
        public Album? Album { get; set; }
    }

    // =========================================================================
    // 2. КОНТЕКСТ БАЗЫ ДАННЫХ (Entity Framework Core)
    // =========================================================================
    public class AppDbContext : DbContext
    {
        public DbSet<Album> Albums { get; set; } = null!;
        public DbSet<Song> Songs { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // База данных SQLite в папке с программой
            optionsBuilder.UseSqlite("Data Source=music_collection.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) // Исправлено здесь
        {
            // Заполнение базы данных начальными тестовыми данными (Seed Data)
            var album1 = new Album { Id = 1, Name = "The Dark Side of the Moon" };
            var album2 = new Album { Id = 2, Name = "Abbey Road" };
            var album3 = new Album { Id = 3, Name = "Back in Black" };

            modelBuilder.Entity<Album>().HasData(album1, album2, album3);

            modelBuilder.Entity<Song>().HasData(
                new Song { Id = 1, AlbumId = 1, Name = "Time", DurationSec = 401 },
                new Song { Id = 2, AlbumId = 1, Name = "Money", DurationSec = 382 },
                new Song { Id = 3, AlbumId = 1, Name = "Us and Them", DurationSec = 462 },
                new Song { Id = 4, AlbumId = 2, Name = "Come Together", DurationSec = 259 },
                new Song { Id = 5, AlbumId = 2, Name = "Something", DurationSec = 182 },
                new Song { Id = 6, AlbumId = 3, Name = "Hells Bells", DurationSec = 312 },
                new Song { Id = 7, AlbumId = 3, Name = "Back in Black", DurationSec = 255 },
                new Song { Id = 8, AlbumId = 3, Name = "You Shook Me All Night Long", DurationSec = 210 }
            );
        }
    }

    // =========================================================================
    // 3. ГРАФИЧЕСКИЙ ИНТЕРФЕЙС (WINDOWS FORMS)
    // =========================================================================
    public class MainForm : Form
    {
        private readonly AppDbContext _context;

        private DataGridView _dataGridViewMain = null!;
        private DataGridView _dataGridViewReport = null!;
        private TextBox _textBoxSearch = null!;
        private Button _buttonSearch = null!;
        private Button _buttonReport1 = null!;
        private Button _buttonReport2 = null!;

        public MainForm()
        {
            _context = new AppDbContext();
            _context.Database.EnsureCreated(); // Создает БД, если её нет

            InitializeInterface();
            LoadMainData();
        }

        private void InitializeInterface()
        {
            this.Text = "Вариант 7: Музыкальная Коллекция (ИУ5)";
            this.Width = 1000;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Панель фильтрации
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60 };
            var labelSearch = new Label { Text = "Поиск песни:", Left = 20, Top = 22, Width = 100 };
            _textBoxSearch = new TextBox { Left = 120, Top = 20, Width = 200 };
            _buttonSearch = new Button { Text = "Найти", Left = 330, Top = 18, Width = 100 };
            _buttonSearch.Click += ButtonSearch_Click;

            panelTop.Controls.AddRange(new Control[] { labelSearch, _textBoxSearch, _buttonSearch });

            // Левая панель с основной таблицей
            _dataGridViewMain = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true };

            var groupBoxLeft = new GroupBox { Text = "Раздел 1. Все песни (Основная + Справочная)", Dock = DockStyle.Left, Width = 500 };
            groupBoxLeft.Controls.Add(_dataGridViewMain);

            // Правая панель с кнопками аналитики и таблицей отчетов
            var panelRight = new Panel { Dock = DockStyle.Fill };

            var panelRightButtons = new Panel { Dock = DockStyle.Top, Height = 60 };
            _buttonReport1 = new Button { Text = "Отчет 1: Треков в альбоме", Left = 10, Top = 15, Width = 210 };
            _buttonReport1.Click += ButtonReport1_Click;

            _buttonReport2 = new Button { Text = "Отчет 2: Средняя длительность", Left = 230, Top = 15, Width = 230 };
            _buttonReport2.Click += ButtonReport2_Click;

            panelRightButtons.Controls.AddRange(new Control[] { _buttonReport1, _buttonReport2 });

            _dataGridViewReport = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true };

            var groupBoxRight = new GroupBox { Text = "Аналитические отчеты (Разделы 2 и 3)", Dock = DockStyle.Fill };
            groupBoxRight.Controls.Add(_dataGridViewReport);
            groupBoxRight.Controls.Add(panelRightButtons);

            panelRight.Controls.Add(groupBoxRight);

            // Сборка формы
            this.Controls.Add(panelRight);
            this.Controls.Add(groupBoxLeft);
            this.Controls.Add(panelTop);
        } // Скобка восстановлена здесь!

        // РАЗДЕЛ 1: Загрузка основных данных (Жадная загрузка .Include)
        private void LoadMainData()
        {
            var data = _context.Songs
                .Include(s => s.Album)
                .Select(s => new
                {
                    ID = s.Id,
                    Название = s.Name,
                    Длительность_Сек = s.DurationSec,
                    Альбом = s.Album != null ? s.Album.Name : "Нет альбома"
                })
                .ToList();

            _dataGridViewMain.DataSource = data;
        }

        // РАЗДЕЛ 1 (Фильтрация): Поиск по названию песни
        private void ButtonSearch_Click(object? sender, EventArgs e)
        {
            string searchTxt = _textBoxSearch.Text.Trim().ToLower();

            var query = _context.Songs.Include(s => s.Album).AsQueryable();

            if (!string.IsNullOrEmpty(searchTxt))
            {
                query = query.Where(s => s.Name.ToLower().Contains(searchTxt));
            }

            _dataGridViewMain.DataSource = query
                .Select(s => new
                {
                    ID = s.Id,
                    Название = s.Name,
                    Длительность_Сек = s.DurationSec,
                    Альбом = s.Album != null ? s.Album.Name : "Нет альбома"
                })
                .ToList();
        }

        // РАЗДЕЛ 2: Группировка с агрегатной функцией Count(), сортировка по названию
        private void ButtonReport1_Click(object? sender, EventArgs e)
        {
            var report1 = _context.Songs
                .GroupBy(s => s.Album!.Name)
                .Select(g => new
                {
                    Альбом = g.Key,
                    Количество_Треков = g.Count()
                })
                .OrderBy(r => r.Альбом)
                .ToList();

            _dataGridViewReport.DataSource = report1;
        }

        // РАЗДЕЛ 3: Группировка с агрегатной функцией Average(), сортировка по убыванию среднего
        private void ButtonReport2_Click(object? sender, EventArgs e)
        {
            var report2 = _context.Songs
                .GroupBy(s => s.Album!.Name)
                .Select(g => new
                {
                    Альбом = g.Key,
                    Средняя_Длительность_Сек = Math.Round(g.Average(s => s.DurationSec), 1)
                })
                .OrderByDescending(r => r.Средняя_Длительность_Сек)
                .ToList();

            _dataGridViewReport.DataSource = report2;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _context.Dispose(); // Закрываем подключение к БД при выходе
        }
    }

    // =========================================================================
    // 4. ТОЧКА ВХОДА В ПРОГРАММУ
    // =========================================================================
    
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Стандартный способ инициализации для Windows Forms
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}