using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AirplaneManager.UI
{
    /// <summary>
    /// Элемент для отображения и выбора в многопользовательском модальном окне
    /// </summary>
    public class SelectableItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Модальное окно выбора сущностей (пилоты, стюардессы, пассажиры)
    /// с поддержкой быстрого поиска и множественного выбора через чекбоксы.
    /// </summary>
    public class MultiSelectDialog : Form
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(string x, string y);

        private readonly List<SelectableItem> _allItems;
        private readonly HashSet<int> _selectedIds;

        private TextBox _txtSearch = null!;
        private DataGridView _grid = null!;
        private Label _lblCount = null!;
        private Button _btnSelectAll = null!;
        private Button _btnDeselectAll = null!;
        private Button _btnConfirm = null!;
        private Button _btnCancel = null!;

        public IReadOnlyCollection<int> SelectedIds => _selectedIds;

        public MultiSelectDialog(string title, List<SelectableItem> allItems, IEnumerable<int> initiallySelectedIds)
        {
            _allItems = allItems ?? new List<SelectableItem>();
            _selectedIds = new HashSet<int>(initiallySelectedIds ?? Enumerable.Empty<int>());

            InitializeComponent(title);
            ApplyFilter();
        }

        private void InitializeComponent(string title)
        {
            this.Text = title;
            this.Size = new Size(720, 560);
            this.MinimumSize = new Size(580, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.FromArgb(245, 245, 248);

            // Верхняя панель: Заголовок + Поиск + Счетчик
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(15, 12, 15, 10),
                BackColor = Color.FromArgb(235, 238, 242)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Arial", 13, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 10)
            };

            _lblCount = new Label
            {
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                AutoSize = true,
                Location = new Point(15, 35)
            };

            _txtSearch = new TextBox
            {
                PlaceholderText = "Быстрый поиск по имени, фамилии или данным...",
                Font = new Font("Arial", 11),
                Size = new Size(320, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlHeader.Width - 340, 24)
            };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, _lblCount, _txtSearch });

            // Центральная таблица с чекбоксами
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 35,
                RowTemplate = { Height = 32 }
            };
            UiStyleHelper.EnableDoubleBuffering(_grid);

            // Естественная (Natural) сортировка чисел внутри строк
            _grid.SortCompare += (s, e) =>
            {
                if (e.Column.Name == "colId")
                {
                    int id1 = Convert.ToInt32(e.CellValue1);
                    int id2 = Convert.ToInt32(e.CellValue2);
                    e.SortResult = id1.CompareTo(id2);
                    e.Handled = true;
                }
                else if (e.Column.Name == "colCheck")
                {
                    bool b1 = Convert.ToBoolean(e.CellValue1);
                    bool b2 = Convert.ToBoolean(e.CellValue2);
                    e.SortResult = b1.CompareTo(b2);
                    e.Handled = true;
                }
                else
                {
                    string s1 = e.CellValue1?.ToString() ?? "";
                    string s2 = e.CellValue2?.ToString() ?? "";
                    e.SortResult = StrCmpLogicalW(s1, s2);
                    e.Handled = true;
                }
            };

            // Колонка чекбокса
            var chkCol = new DataGridViewCheckBoxColumn
            {
                Name = "colCheck",
                HeaderText = "✓",
                Width = 45,
                Resizable = DataGridViewTriState.False
            };
            _grid.Columns.Add(chkCol);

            // Колонка ID
            var idCol = new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "ID",
                Width = 55,
                ReadOnly = true
            };
            _grid.Columns.Add(idCol);

            // Колонка ФИО / Название
            var titleCol = new DataGridViewTextBoxColumn
            {
                Name = "colTitle",
                HeaderText = "ФИО / Наименование",
                Width = 220,
                ReadOnly = true
            };
            _grid.Columns.Add(titleCol);

            // Колонка деталей
            var detailsCol = new DataGridViewTextBoxColumn
            {
                Name = "colDetails",
                HeaderText = "Сведения",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            };
            _grid.Columns.Add(detailsCol);

            // Клик по ячейке или строке сразу переключает чекбокс
            _grid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 0)
                {
                    _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };

            _grid.CellValueChanged += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 0)
                {
                    bool isChecked = Convert.ToBoolean(_grid.Rows[e.RowIndex].Cells["colCheck"].Value);
                    int id = Convert.ToInt32(_grid.Rows[e.RowIndex].Cells["colId"].Value);

                    if (isChecked) _selectedIds.Add(id);
                    else _selectedIds.Remove(id);

                    UpdateCounterLabel();
                }
            };

            // Клик по строке (не чекбоксу) инвертирует чекбокс для удобства
            _grid.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex != 0)
                {
                    var cell = _grid.Rows[e.RowIndex].Cells["colCheck"];
                    bool current = Convert.ToBoolean(cell.Value);
                    cell.Value = !current;
                    _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };

            // Нижняя панель с кнопками
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(15, 10, 15, 10),
                BackColor = Color.FromArgb(235, 238, 242)
            };

            _btnSelectAll = new Button
            {
                Text = "Выбрать все",
                Font = new Font("Arial", 10),
                Size = new Size(115, 36),
                Location = new Point(15, 12),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnSelectAll.Click += (s, e) => SetAllFiltered(true);

            _btnDeselectAll = new Button
            {
                Text = "Снять все",
                Font = new Font("Arial", 10),
                Size = new Size(105, 36),
                Location = new Point(138, 12),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnDeselectAll.Click += (s, e) => SetAllFiltered(false);

            _btnCancel = new Button
            {
                Text = "Отмена",
                Font = new Font("Arial", 10),
                Size = new Size(105, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlBottom.Width - 120, 12),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            _btnConfirm = new Button
            {
                Text = "Готово",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Size = new Size(130, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlBottom.Width - 260, 12),
                BackColor = Color.FromArgb(60, 150, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnConfirm.Click += (s, e) => this.DialogResult = DialogResult.OK;

            pnlBottom.Controls.AddRange(new Control[] { _btnSelectAll, _btnDeselectAll, _btnConfirm, _btnCancel });

            this.Controls.Add(_grid);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlHeader);
        }

        private void ApplyFilter()
        {
            string query = _txtSearch.Text.Trim().ToLowerInvariant();

            var visibleItems = string.IsNullOrEmpty(query)
                ? _allItems
                : _allItems.Where(item =>
                    item.Title.ToLowerInvariant().Contains(query) ||
                    item.Details.ToLowerInvariant().Contains(query) ||
                    item.Id.ToString().Contains(query)).ToList();

            _grid.Rows.Clear();
            foreach (var item in visibleItems)
            {
                _grid.Rows.Add(
                    _selectedIds.Contains(item.Id),
                    item.Id,
                    item.Title,
                    item.Details
                );
            }

            UpdateCounterLabel();
        }

        private void SetAllFiltered(bool check)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                row.Cells["colCheck"].Value = check;
            }
            _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            UpdateCounterLabel();
        }

        private void UpdateCounterLabel()
        {
            _lblCount.Text = $"Выбрано: {_selectedIds.Count} из {_allItems.Count}";
            _btnConfirm.Text = $"Готово ({_selectedIds.Count})";
        }
    }
}
