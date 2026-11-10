using System;
using System.Drawing;
using System.Windows.Forms;
using AirplaneManager.Managers;
using AirplaneManager.UI;

namespace AirplaneManager
{
    // Вторая часть класса MainForm, отвечающая только за верстку элементов
    public partial class MainForm
    {
        // Основные экранные слои
        private BufferedPanel pnlSplash = null!;
        private BufferedPanel pnlDbError = null!;
        private BufferedPanel pnlSuccess = null!;
        private BufferedPanel pnlDashboard = null!;

        // Внутренние под-панели рабочей области 
        private BufferedPanel pnlEntityChoice = null!;
        private BufferedPanel pnlEntityTableContainer = null!;
        private BufferedPanel pnlBottomActions = null!;

        // Таблица и элементы управления
        private DataGridView gridMain = null!;
        private Label lblDashTitle = null!;
        private Label lblLoadingDots = null!;
        private Button btnBackHeader = null!;

        // Реактивный поиск
        private TextBox txtSearch = null!;

        // Кнопки работы с XML (располагаются на центральном экране выбора рейса)
        private Button btnExportXml = null!;
        private Button btnImportXml = null!;

        // Боковые кнопки
        private Button btnRoutes = null!;
        private Button btnFlights = null!;
        private Button btnAirports = null!;
        private Button btnPilots = null!;
        private Button btnAirplanes = null!;
        private Button btnAttendants = null!;
        private Button btnPassengers = null!;
        private Button btnSettings = null!;

        // Включаем аппаратную композитную отрисовку окна (убирает мерцание при ресайзе и смене панелей)
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        private void BuildUiElements()
        {
            this.SuspendLayout();

            this.Size = new Size(1280, 720);
            this.MinimumSize = new Size(960, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "AirplaneManager — Управление авиакомпанией";
            this.Load += MainForm_Load;

            // Фон формы
            Image? bgImage = UiStyleHelper.LoadImageFromViewFolder("Background.png");
            if (bgImage != null)
            {
                this.BackgroundImage = bgImage;
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }

            Image? iconImage = UiStyleHelper.LoadImageFromViewFolder("Icon.png");

            // 1. ЭКРАН ЗАСТАВКИ
            pnlSplash = new BufferedPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var splashLayout = new BufferedTableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 3
            };
            splashLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            splashLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
            splashLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 56F));
            splashLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));

            var lblTitle = new Label
            {
                Text = "Управление авиакомпанией",
                Font = new Font("Arial", 28, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomCenter,
                BackColor = Color.Transparent
            };

            var picSplashLogo = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = iconImage,
                Margin = new Padding(20)
            };

            lblLoadingDots = new Label
            {
                Text = "Загрузка.",
                Font = new Font("Arial", 20, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.Transparent
            };

            splashLayout.Controls.Add(lblTitle, 0, 0);
            splashLayout.Controls.Add(picSplashLogo, 0, 1);
            splashLayout.Controls.Add(lblLoadingDots, 0, 2);
            pnlSplash.Controls.Add(splashLayout);

            // 2. ЭКРАН ОШИБКИ БАЗЫ ДАННЫХ
            pnlDbError = new BufferedPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };

            var errorCenterLayout = new BufferedTableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 3,
                RowCount = 3
            };
            errorCenterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            errorCenterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            errorCenterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            errorCenterLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            errorCenterLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            errorCenterLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var errorCard = new BufferedPanel
            {
                Size = new Size(540, 240),
                BackColor = Color.FromArgb(235, 255, 255, 255)
            };

            var lblErrorMsg = new Label
            {
                Text = "Ошибка подключения к базе данных!\nУбедитесь, что служба MySQL запущена.",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 110
            };

            var btnRetry = new Button
            {
                Text = "Повторить подключение",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(260, 48),
                BackColor = Color.FromArgb(220, 220, 220),
                FlatStyle = FlatStyle.Flat
            };
            btnRetry.Location = new Point((errorCard.Width - btnRetry.Width) / 2, 130);
            btnRetry.Click += async (s, e) => await RunStartupSequence();

            errorCard.Controls.Add(lblErrorMsg);
            errorCard.Controls.Add(btnRetry);
            errorCenterLayout.Controls.Add(errorCard, 1, 1);
            pnlDbError.Controls.Add(errorCenterLayout);

            // 3. ЭКРАН УСПЕШНОГО ПОДКЛЮЧЕНИЯ
            pnlSuccess = new BufferedPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };

            var picSuccessLogo = new PictureBox
            {
                Size = new Size(80, 80),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(25, 20),
                BackColor = Color.Transparent,
                Image = iconImage
            };

            var lblSuccess = new Label
            {
                Text = "Успешно",
                Font = new Font("Arial", 40, FontStyle.Bold),
                ForeColor = Color.LimeGreen,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            pnlSuccess.Controls.Add(picSuccessLogo);
            pnlSuccess.Controls.Add(lblSuccess);

            // 4. ПАНЕЛЬ УПРАВЛЕНИЯ (DASHBOARD)
            pnlDashboard = new BufferedPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };

            var pnlSidebar = new BufferedPanel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = UiStyleHelper.SidebarBgColor
            };

            var picDashLogo = new PictureBox
            {
                Size = new Size(75, 75),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(15, 10),
                BackColor = Color.Transparent,
                Image = iconImage
            };
            pnlSidebar.Controls.Add(picDashLogo);

            int btnY = 100;
            btnRoutes = UiStyleHelper.CreateSidebarButton("Маршруты", btnY, (s, e) => OnSidebarButtonClick(btnRoutes, EntityType.Route));
            btnFlights = UiStyleHelper.CreateSidebarButton("Рейсы", btnY += 48, (s, e) => OnSidebarButtonClick(btnFlights, EntityType.Flight));
            btnAirports = UiStyleHelper.CreateSidebarButton("Аэропорты", btnY += 48, (s, e) => OnSidebarButtonClick(btnAirports, EntityType.Airport));
            btnPilots = UiStyleHelper.CreateSidebarButton("Пилоты", btnY += 48, (s, e) => OnSidebarButtonClick(btnPilots, EntityType.Pilot));
            btnAirplanes = UiStyleHelper.CreateSidebarButton("Самолеты", btnY += 48, (s, e) => OnSidebarButtonClick(btnAirplanes, EntityType.Airplane));
            btnAttendants = UiStyleHelper.CreateSidebarButton("Стюардессы", btnY += 48, (s, e) => OnSidebarButtonClick(btnAttendants, EntityType.FlightAttendant));
            btnPassengers = UiStyleHelper.CreateSidebarButton("Пассажиры", btnY += 48, (s, e) => OnSidebarButtonClick(btnPassengers, EntityType.Passenger));
            btnSettings = UiStyleHelper.CreateSidebarButton("Настройки", btnY += 65, (s, e) => MessageBox.Show("Это настройки"));

            pnlSidebar.Controls.AddRange(new Control[] {
                btnRoutes, btnFlights, btnAirports, btnPilots, btnAirplanes, btnAttendants, btnPassengers, btnSettings
            });

            var pnlHeader = new BufferedPanel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = UiStyleHelper.HeaderBgColor
            };

            lblDashTitle = new Label
            {
                Text = "Панель управления",
                Font = new Font("Arial", 22, FontStyle.Bold),
                Location = new Point(25, 22),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            txtSearch = new TextBox
            {
                PlaceholderText = "Быстрый поиск по таблице...",
                Font = new Font("Arial", 12),
                Size = new Size(280, 34),
                Location = new Point(pnlHeader.Width - 465, 26),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Visible = false
            };
            txtSearch.TextChanged += (s, e) => ApplyColumnSorting();

            btnBackHeader = new Button
            {
                Text = "Назад",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Size = new Size(130, 36),
                Location = new Point(pnlHeader.Width - 155, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            btnBackHeader.Click += (s, e) =>
            {
                this.SuspendLayout();
                pnlEntityTableContainer.Visible = false;
                pnlBottomActions.Visible = false;
                btnBackHeader.Visible = false;
                pnlEntityChoice.Visible = true;
                this.ResumeLayout();
                txtSearch.Visible = false;
            };

            pnlHeader.Controls.AddRange(new Control[] { lblDashTitle, txtSearch, btnBackHeader });

            // Меню выбора по центру
            pnlEntityChoice = new BufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = true
            };

            var choiceCenterGrid = new BufferedTableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 3,
                RowCount = 3
            };
            choiceCenterGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            choiceCenterGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            choiceCenterGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            choiceCenterGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            choiceCenterGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            choiceCenterGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var choiceButtonsFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var btnChoiceAdd = UiStyleHelper.CreateStyledButton("Добавить новую запись", new Size(360, 65));
            btnChoiceAdd.Margin = new Padding(0, 0, 0, 16);
            btnChoiceAdd.Click += btnAdd_Click;

            var btnChoiceView = UiStyleHelper.CreateStyledButton("Просмотреть таблицу", new Size(360, 65));
            btnChoiceView.Margin = new Padding(0, 0, 0, 16);
            btnChoiceView.Click += (s, e) => OpenTableView(true);

            btnExportXml = UiStyleHelper.CreateStyledButton("Экспорт рейсов в XML", new Size(360, 55));
            btnExportXml.Margin = new Padding(0, 0, 0, 16);
            btnExportXml.Click += btnExportXml_Click;
            btnExportXml.Visible = false;

            btnImportXml = UiStyleHelper.CreateStyledButton("Импорт рейсов из XML", new Size(360, 55));
            btnImportXml.Margin = new Padding(0, 0, 0, 16);
            btnImportXml.Click += btnImportXml_Click;
            btnImportXml.Visible = false;

            choiceButtonsFlow.Controls.AddRange(new Control[] { btnChoiceAdd, btnChoiceView, btnExportXml, btnImportXml });
            choiceCenterGrid.Controls.Add(choiceButtonsFlow, 1, 1);
            pnlEntityChoice.Controls.Add(choiceCenterGrid);

            // Контейнер таблицы
            pnlEntityTableContainer = new BufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = false
            };

            pnlBottomActions = new BufferedPanel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = UiStyleHelper.BottomBarBgColor,
                Visible = false
            };

            var btnUpdateRow = new Button
            {
                Text = "Обновить выбранное",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Size = new Size(210, 42),
                Location = new Point(20, 11),
                BackColor = Color.FromArgb(80, 140, 210),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUpdateRow.Click += btnUpdate_Click;

            var btnDeleteRow = new Button
            {
                Text = "Удалить выбранное",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Size = new Size(210, 42),
                Location = new Point(250, 11),
                BackColor = Color.FromArgb(220, 80, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDeleteRow.Click += btnDelete_Click;

            pnlBottomActions.Controls.AddRange(new Control[] { btnUpdateRow, btnDeleteRow });

            gridMain = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            UiStyleHelper.EnableDoubleBuffering(gridMain);

            gridMain.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    pnlBottomActions.Visible = true;
                    if (e.ColumnIndex >= 0)
                    {
                        OnCellClickedColumn(e.ColumnIndex);
                    }
                }
            };

            gridMain.ColumnHeaderMouseClick += GridMain_ColumnHeaderMouseClick;

            pnlEntityTableContainer.Controls.Add(gridMain);
            pnlEntityTableContainer.Controls.Add(pnlBottomActions);

            var pnlContent = new BufferedPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            pnlContent.Controls.Add(pnlEntityChoice);
            pnlContent.Controls.Add(pnlEntityTableContainer);
            pnlContent.Controls.Add(pnlHeader);

            pnlDashboard.Controls.Add(pnlContent);
            pnlDashboard.Controls.Add(pnlSidebar);

            this.Controls.AddRange(new Control[] { pnlSplash, pnlDbError, pnlSuccess, pnlDashboard });

            this.ResumeLayout(false);
        }
    }
}
