using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AirplaneManager.Managers;
using AirplaneManager.Managers.Crud;
using AirplaneManager.Models;

namespace AirplaneManager.UI
{
    public class EntityEditForm : Form
    {
        private readonly EntityType _entityType;
        private readonly int? _editId;

        private TableLayoutPanel _fieldsGrid = null!;
        private Label _lblValidationStatus = null!;
        private Button _btnSave = null!;

        // Поля ввода для разных сущностей
        private TextBox _txt1 = new TextBox { Width = 250, Font = new Font("Arial", 11) };
        private TextBox _txt2 = new TextBox { Width = 250, Font = new Font("Arial", 11) };
        private TextBox _txt3 = new TextBox { Width = 250, Font = new Font("Arial", 11) };
        private TextBox _txt4 = new TextBox { Width = 250, Font = new Font("Arial", 11) };
        private NumericUpDown _num1 = new NumericUpDown { Width = 250, Font = new Font("Arial", 11), Maximum = 1000000 };
        private NumericUpDown _num2 = new NumericUpDown { Width = 250, Font = new Font("Arial", 11), Maximum = 1000000 };
        private NumericUpDown _num3 = new NumericUpDown { Width = 250, Font = new Font("Arial", 11), Minimum = 1, Maximum = 2 };

        private ComboBox _cmb1 = new ComboBox { Width = 250, Font = new Font("Arial", 11), DropDownStyle = ComboBoxStyle.DropDownList };
        private ComboBox _cmb2 = new ComboBox { Width = 250, Font = new Font("Arial", 11), DropDownStyle = ComboBoxStyle.DropDownList };

        private CheckBox _chkAssignPlane = new CheckBox { Text = "Назначить самолет на рейс", AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold), Checked = true };
        private DateTimePicker _dtpFlight = new DateTimePicker { Width = 250, Font = new Font("Arial", 11), Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm" };

        private CheckedListBox _clbPilots = new CheckedListBox { Width = 250, Height = 80, CheckOnClick = true };
        private CheckedListBox _clbAttendants = new CheckedListBox { Width = 250, Height = 80, CheckOnClick = true };
        private CheckedListBox _clbPassengers = new CheckedListBox { Width = 250, Height = 80, CheckOnClick = true };

        public EntityEditForm(EntityType entityType, int? editId = null)
        {
            _entityType = entityType;
            _editId = editId;

            BuildFormUi();
            SetupReactiveEvents();
            if (_editId.HasValue) LoadExistingData(_editId.Value);
            ValidateFormInputs();
        }

        private void BuildFormUi()
        {
            string mode = _editId.HasValue ? $"Редактирование (ID: {_editId})" : "Добавление новой записи";
            this.Text = $"{mode} — {_entityType}";
            this.Size = new Size(520, _entityType == EntityType.Flight ? 640 : 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 245);

            _fieldsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(20, 20, 20, 10)
            };
            _fieldsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            _fieldsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));

            switch (_entityType)
            {
                case EntityType.Airport:
                    AddRow("Название аэропорта:", _txt1);
                    AddRow("Код IATA (3 буквы):", _txt2);
                    AddRow("Город:", _txt3);
                    AddRow("Страна:", _txt4);
                    break;

                case EntityType.Airplane:
                    AddRow("Модель самолета:", _txt1);
                    _num1.Minimum = 1; _num1.Value = 150;
                    AddRow("Вместимость (пасс.):", _num1);
                    break;

                case EntityType.Passenger:
                    AddRow("Имя:", _txt1);
                    AddRow("Фамилия:", _txt2);
                    _num1.Minimum = 1; _num1.Maximum = 120; _num1.Value = 25;
                    AddRow("Возраст:", _num1);
                    AddRow("Паспорт:", _txt3);
                    AddRow("Бонусные мили:", _num2);
                    break;

                case EntityType.Pilot:
                    AddRow("Имя:", _txt1);
                    AddRow("Фамилия:", _txt2);
                    _num1.Minimum = 18; _num1.Maximum = 90; _num1.Value = 30;
                    AddRow("Возраст:", _num1);
                    AddRow("Паспорт:", _txt3);
                    AddRow("Налет часов:", _num2);
                    AddRow("Номер лицензии:", _txt4);
                    AddRow("Ранг (1 или 2):", _num3);
                    break;

                case EntityType.FlightAttendant:
                    AddRow("Имя:", _txt1);
                    AddRow("Фамилия:", _txt2);
                    _num1.Minimum = 18; _num1.Maximum = 90; _num1.Value = 25;
                    AddRow("Возраст:", _num1);
                    AddRow("Паспорт:", _txt3);
                    AddRow("Стаж работы (лет):", _num2);
                    AddRow("Языки (через запятую):", _txt4);
                    break;

                case EntityType.Route:
                    AddRow("Номер маршрута:", _txt1);
                    AddRow("Длительность (часов):", _txt2);
                    var airports1 = AirportCrudManager.GetAll();
                    var airports2 = AirportCrudManager.GetAll();
                    _cmb1.DataSource = airports1;
                    _cmb2.DataSource = airports2;
                    AddRow("Аэропорт вылета:", _cmb1);
                    AddRow("Аэропорт прилета:", _cmb2);
                    break;

                case EntityType.Flight:
                    AddRow("Дата и время вылета:", _dtpFlight);
                    _cmb1.DataSource = RouteCrudManager.GetAll();
                    AddRow("Маршрут:", _cmb1);
                    AddRow("Опция борта:", _chkAssignPlane);
                    _cmb2.DataSource = AirplaneCrudManager.GetAll();
                    AddRow("Самолет:", _cmb2);

                    foreach (var p in PilotCrudManager.GetAll())
                        _clbPilots.Items.Add(new ListItem(p.Id, $"{p.FirstName} {p.LastName} (Ранг {p.Rang})"));
                    foreach (var a in FlightAttendantCrudManager.GetAll())
                        _clbAttendants.Items.Add(new ListItem(a.Id, $"{a.FirstName} {a.LastName}"));
                    foreach (var pass in PassengerCrudManager.GetAll())
                        _clbPassengers.Items.Add(new ListItem(pass.Id, $"{pass.FirstName} {pass.LastName}"));

                    AddRow("Пилоты:", _clbPilots);
                    AddRow("Бортпроводники:", _clbAttendants);
                    AddRow("Пассажиры:", _clbPassengers);
                    break;
            }

            _lblValidationStatus = new Label
            {
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            var pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(15, 10, 20, 10),
                BackColor = Color.FromArgb(230, 230, 230)
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                Size = new Size(120, 40),
                Font = new Font("Arial", 11),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            _btnSave = new Button
            {
                Text = "Сохранить",
                Size = new Size(150, 40),
                Font = new Font("Arial", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 150, 80),
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 10, 0)
            };
            _btnSave.Click += BtnSave_Click;

            pnlButtons.Controls.AddRange(new Control[] { btnCancel, _btnSave });

            this.Controls.Add(_lblValidationStatus);
            this.Controls.Add(_fieldsGrid);
            this.Controls.Add(pnlButtons);
        }

        private void AddRow(string labelText, Control control)
        {
            int row = _fieldsGrid.RowCount++;
            _fieldsGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var lbl = new Label
            {
                Text = labelText,
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 8, 3, 8)
            };
            control.Margin = new Padding(3, 5, 3, 5);
            _fieldsGrid.Controls.Add(lbl, 0, row);
            _fieldsGrid.Controls.Add(control, 1, row);
        }

        // =========================================================================
        // РЕАКТИВНОСТЬ ИНТЕРФЕЙСА (СОБЫТИЯ TextChanged, SelectedIndexChanged, CheckedChanged)
        // =========================================================================
        private void SetupReactiveEvents()
        {
            // Реактивная валидация при вводе любого текста
            _txt1.TextChanged += (s, e) => ValidateFormInputs();
            _txt2.TextChanged += (s, e) =>
            {
                // Автоматический перевод кода IATA в верхний регистр
                if (_entityType == EntityType.Airport && _txt2.Text != _txt2.Text.ToUpper())
                {
                    int pos = _txt2.SelectionStart;
                    _txt2.Text = _txt2.Text.ToUpper();
                    _txt2.SelectionStart = pos;
                }
                ValidateFormInputs();
            };
            _txt3.TextChanged += (s, e) => ValidateFormInputs();
            _txt4.TextChanged += (s, e) => ValidateFormInputs();

            // Реактивная проверка совпадения аэропортов при выборе из списка
            _cmb1.SelectedIndexChanged += (s, e) => ValidateFormInputs();
            _cmb2.SelectedIndexChanged += (s, e) => ValidateFormInputs();

            // Реактивное включение/отключение выбора самолета по чекбоксу
            _chkAssignPlane.CheckedChanged += (s, e) =>
            {
                _cmb2.Enabled = _chkAssignPlane.Checked;
                ValidateFormInputs();
            };

            _txt3.KeyPress += (s, e) =>
            {
                // Разрешаем только цифры и клавишу Backspace
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                    return;
                }

                // Автоматическая вставка пробела после 4-й цифры
                if (_entityType == EntityType.Passenger || _entityType == EntityType.Pilot || _entityType == EntityType.FlightAttendant)
                {
                    string text = _txt3.Text;
                    if (text.Length == 4 && !text.EndsWith(" ") && !char.IsControl(e.KeyChar))
                    {
                        _txt3.Text = text + " ";
                        _txt3.SelectionStart = _txt3.Text.Length; // Переносим курсор в конец
                    }
                }
            };
        }

        private void ValidateFormInputs()
        {
            string error = "";

            switch (_entityType)
            {
                case EntityType.Airport:
                    if (string.IsNullOrWhiteSpace(_txt1.Text)) error = "Введите название аэропорта";
                    else if (_txt2.Text.Trim().Length != 3) error = "Код IATA должен состоять ровно из 3 букв!";
                    else if (string.IsNullOrWhiteSpace(_txt3.Text) || string.IsNullOrWhiteSpace(_txt4.Text)) error = "Заполните город и страну";
                    break;

                case EntityType.Airplane:
                    if (string.IsNullOrWhiteSpace(_txt1.Text)) error = "Введите модель самолета";
                    break;

                case EntityType.Passenger:
                case EntityType.FlightAttendant:
                    if (string.IsNullOrWhiteSpace(_txt1.Text) || string.IsNullOrWhiteSpace(_txt2.Text)) error = "Заполните имя и фамилию";
                    else if (!_txt1.Text.All(char.IsLetter) || !_txt2.Text.All(char.IsLetter)) error = "Имя и фамилия должны состоять только из букв!";
                    else if (string.IsNullOrWhiteSpace(_txt3.Text) || _txt3.Text.Trim().Length != 11) error = "Введите паспорт в формате: XXXX XXXXXX";
                    break;

                case EntityType.Pilot:
                    if (string.IsNullOrWhiteSpace(_txt1.Text) || string.IsNullOrWhiteSpace(_txt2.Text)) error = "Заполните имя и фамилию";
                    else if (!_txt1.Text.All(char.IsLetter) || !_txt2.Text.All(char.IsLetter)) error = "Имя и фамилия должны состоять только из букв!";
                    else if (string.IsNullOrWhiteSpace(_txt3.Text) || _txt3.Text.Trim().Length != 11) error = "Введите паспорт в формате: XXXX XXXXXX";
                    else if (!int.TryParse(_txt4.Text, out int lic) || lic < 100000 || lic > 999999) error = "Лицензия пилота должна состоять ровно из 6 цифр!";
                    break;

                case EntityType.Route:
                    if (string.IsNullOrWhiteSpace(_txt1.Text))
                        error = "Введите номер маршрута";
                    else if (!double.TryParse(_txt2.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double d) || d <= 0)
                        error = "Введите корректную длительность полета (> 0)";
                    else if (_cmb1.SelectedItem == null || _cmb2.SelectedItem == null)
                        error = "Выберите аэропорт вылета и прилета!";
                    else if (_cmb1.SelectedItem is Airport a1 && _cmb2.SelectedItem is Airport a2 && a1.Id == a2.Id)
                        error = "Аэропорт вылета и прилета не могут совпадать!";
                    break;

                case EntityType.Flight:
                    if (_cmb1.SelectedItem == null) error = "Выберите маршрут";
                    break;
            }

            if (string.IsNullOrEmpty(error))
            {
                _lblValidationStatus.Text = "✓ Все поля заполнены корректно";
                _lblValidationStatus.ForeColor = Color.FromArgb(40, 140, 60);
                _btnSave.Enabled = true;
                _btnSave.BackColor = Color.FromArgb(60, 150, 80);
            }
            else
            {
                _lblValidationStatus.Text = "⚠ " + error;
                _lblValidationStatus.ForeColor = Color.Crimson;
                _btnSave.Enabled = false;
                _btnSave.BackColor = Color.Gray;
            }
        }

        private void LoadExistingData(int id)
        {
            switch (_entityType)
            {
                case EntityType.Airport:
                    var ap = AirportCrudManager.GetById(id);
                    if (ap != null) { _txt1.Text = ap.Name; _txt2.Text = ap.Code; _txt3.Text = ap.City; _txt4.Text = ap.Country; }
                    break;
                case EntityType.Airplane:
                    var pl = AirplaneCrudManager.GetById(id);
                    if (pl != null) { _txt1.Text = pl.Model; _num1.Value = pl.Capacity; }
                    break;
                case EntityType.Passenger:
                    var ps = PassengerCrudManager.GetById(id);
                    if (ps != null) { _txt1.Text = ps.FirstName; _txt2.Text = ps.LastName; _num1.Value = ps.Age; _txt3.Text = ps.Passport; _num2.Value = ps.Bonus; }
                    break;
                case EntityType.Pilot:
                    var pi = PilotCrudManager.GetById(id);
                    if (pi != null) { _txt1.Text = pi.FirstName; _txt2.Text = pi.LastName; _num1.Value = pi.Age; _txt3.Text = pi.Passport; _num2.Value = pi.FlightHours; _txt4.Text = pi.LicenseNumber.ToString(); _num3.Value = pi.Rang; }
                    break;
                case EntityType.FlightAttendant:
                    var fa = FlightAttendantCrudManager.GetById(id);
                    if (fa != null) { _txt1.Text = fa.FirstName; _txt2.Text = fa.LastName; _num1.Value = fa.Age; _txt3.Text = fa.Passport; _num2.Value = fa.Service; _txt4.Text = string.Join(", ", fa.Languages); }
                    break;
                case EntityType.Route:
                    var rt = RouteCrudManager.GetById(id);
                    if (rt != null)
                    {
                        _txt1.Text = rt.RouteNumber;
                        _txt2.Text = rt.FlightDurationHours.ToString(CultureInfo.InvariantCulture);
                        SelectComboById(_cmb1, rt.DepartureAirport.Id);
                        SelectComboById(_cmb2, rt.ArrivalAirport.Id);
                    }
                    break;
                case EntityType.Flight:
                    var fl = FlightCrudManager.GetById(id);
                    if (fl != null)
                    {
                        _dtpFlight.Value = fl.FlightDate;
                        SelectComboById(_cmb1, fl.Route.Id);
                        _chkAssignPlane.Checked = fl.Airplane != null;
                        if (fl.Airplane != null) SelectComboById(_cmb2, fl.Airplane.Id);
                        CheckListItems(_clbPilots, fl.Pilots.Select(x => x.Id).ToList());
                        CheckListItems(_clbAttendants, fl.FlightAttendants.Select(x => x.Id).ToList());
                        CheckListItems(_clbPassengers, fl.Passengers.Select(x => x.Id).ToList());
                    }
                    break;
            }
        }

        private void SelectComboById(ComboBox cmb, int id)
        {
            for (int i = 0; i < cmb.Items.Count; i++)
            {
                var prop = cmb.Items[i]?.GetType().GetProperty("Id");
                if (prop != null && Convert.ToInt32(prop.GetValue(cmb.Items[i])) == id)
                {
                    cmb.SelectedIndex = i;
                    break;
                }
            }
        }

        private void CheckListItems(CheckedListBox clb, List<int> ids)
        {
            for (int i = 0; i < clb.Items.Count; i++)
            {
                if (clb.Items[i] is ListItem item && ids.Contains(item.Id))
                    clb.SetItemChecked(i, true);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                int id = _editId ?? 0;
                switch (_entityType)
                {
                    case EntityType.Airport:
                        var airport = _editId.HasValue
                            ? new Airport(id, _txt1.Text.Trim(), _txt2.Text.Trim(), _txt3.Text.Trim(), _txt4.Text.Trim())
                            : new Airport(_txt1.Text.Trim(), _txt2.Text.Trim(), _txt3.Text.Trim(), _txt4.Text.Trim());
                        if (_editId.HasValue) AirportCrudManager.Update(airport); else AirportCrudManager.Add(airport);
                        break;

                    case EntityType.Airplane:
                        var plane = _editId.HasValue
                            ? new Airplane(id, _txt1.Text.Trim(), (int)_num1.Value)
                            : new Airplane(_txt1.Text.Trim(), (int)_num1.Value);
                        if (_editId.HasValue) AirplaneCrudManager.Update(plane); else AirplaneCrudManager.Add(plane);
                        break;

                    case EntityType.Passenger:
                        var pass = _editId.HasValue
                            ? new Passenger(id, _txt1.Text.Trim(), _txt2.Text.Trim(), (int)_num1.Value, _txt3.Text.Trim(), (int)_num2.Value)
                            : new Passenger(_txt1.Text.Trim(), _txt2.Text.Trim(), (int)_num1.Value, _txt3.Text.Trim(), (int)_num2.Value);
                        if (_editId.HasValue) PassengerCrudManager.Update(pass); else PassengerCrudManager.Add(pass);
                        break;

                    case EntityType.Pilot:
                        int lic = int.Parse(_txt4.Text.Trim());
                        var pilot = _editId.HasValue
                            ? new Pilot(id, _txt1.Text.Trim(), _txt2.Text.Trim(), (int)_num1.Value, _txt3.Text.Trim(), (int)_num2.Value, lic, (int)_num3.Value)
                            : new Pilot(_txt1.Text.Trim(), _txt2.Text.Trim(), (int)_num1.Value, _txt3.Text.Trim(), (int)_num2.Value, lic, (int)_num3.Value);
                        if (_editId.HasValue) PilotCrudManager.Update(pilot); else PilotCrudManager.Add(pilot);
                        break;

                    case EntityType.FlightAttendant:
                        var langs = _txt4.Text.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                        var att = _editId.HasValue
                            ? new FlightAttendant(id, _txt1.Text.Trim(), _txt2.Text.Trim(), (int)_num1.Value, _txt3.Text.Trim(), (int)_num2.Value, langs)
                            : new FlightAttendant(_txt1.Text.Trim(), _txt2.Text.Trim(), (int)_num1.Value, _txt3.Text.Trim(), (int)_num2.Value, langs);
                        if (_editId.HasValue) FlightAttendantCrudManager.Update(att); else FlightAttendantCrudManager.Add(att);
                        break;

                    case EntityType.Route:
                        double dur = double.Parse(_txt2.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                        var dep = (Airport)_cmb1.SelectedItem!;
                        var arr = (Airport)_cmb2.SelectedItem!;
                        var route = _editId.HasValue
                            ? new Route(id, _txt1.Text.Trim(), dur, dep, arr)
                            : new Route(_txt1.Text.Trim(), dur, dep, arr);
                        if (_editId.HasValue) RouteCrudManager.Update(route); else RouteCrudManager.Add(route);
                        break;

                    case EntityType.Flight:
                        var rt = (Route)_cmb1.SelectedItem!;
                        var pln = _chkAssignPlane.Checked ? (Airplane?)_cmb2.SelectedItem : null;
                        var pilotIds = _clbPilots.CheckedItems.Cast<ListItem>().Select(x => x.Id).ToList();
                        var attIds = _clbAttendants.CheckedItems.Cast<ListItem>().Select(x => x.Id).ToList();
                        var passList = _clbPassengers.CheckedItems.Cast<ListItem>()
                            .Select(x => PassengerCrudManager.GetById(x.Id)).Where(x => x != null).Cast<Passenger>().ToList();

                        var flight = _editId.HasValue
                            ? new Flight(id, _dtpFlight.Value, rt, pln, passList)
                            : new Flight(_dtpFlight.Value, rt, pln, passList);

                        if (_editId.HasValue) FlightCrudManager.Update(flight, pilotIds, attIds);
                        else FlightCrudManager.Add(flight, pilotIds, attIds);
                        break;
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class ListItem
        {
            public int Id { get; }
            public string Text { get; }
            public ListItem(int id, string text) { Id = id; Text = text; }
            public override string ToString() => Text;
        }
    }
}