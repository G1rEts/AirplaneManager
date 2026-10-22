using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using AirplaneManager.Managers;
using AirplaneManager.Managers.Crud;
using AirplaneManager.UI;
using MySql.Data.MySqlClient;

namespace AirplaneManager
{
    // Наследование от Form и вся логика работы
    public partial class MainForm : Form
    {
        private Button? _currentActiveButton = null;
        private EntityType _currentEntity = EntityType.Route;
        private List<object> _currentDataList = new List<object>();
        private string _currentSortColumn = "Id";
        private bool _isSortAscending = true;

        public MainForm()
        {
            this.DoubleBuffered = true;
            BuildUiElements();
        }

        private async void MainForm_Load(object? sender, EventArgs e)
        {
            await RunStartupSequence();
        }

        private async Task RunStartupSequence()
        {
            ShowView(AppView.Splash);

            var dbTask = Task.Run(() => DatabaseHelper.TestConnection());

            string[] dots = { "Загрузка.", "Загрузка..", "Загрузка..." };
            for (int i = 0; i < 5; i++)
            {
                lblLoadingDots.Text = dots[i % 3];
                await Task.Delay(500);
            }

            bool isConnected = await dbTask;

            if (isConnected)
            {
                ShowView(AppView.Success);
                await Task.Delay(1500);
                ShowView(AppView.Dashboard);
                OnSidebarButtonClick(btnRoutes, EntityType.Route);
            }
            else
            {
                ShowView(AppView.DbError);
            }
        }

        private void ShowView(AppView view)
        {
            this.SuspendLayout();
            pnlSplash.Visible = (view == AppView.Splash);
            pnlDbError.Visible = (view == AppView.DbError);
            pnlSuccess.Visible = (view == AppView.Success);
            pnlDashboard.Visible = (view == AppView.Dashboard);
            this.ResumeLayout();
        }

        private enum AppView { Splash, DbError, Success, Dashboard }

        private void OnSidebarButtonClick(Button clickedButton, EntityType entity)
        {
            this.SuspendLayout();

            if (_currentActiveButton != null)
            {
                _currentActiveButton.BackColor = UiStyleHelper.SidebarBtnDefault;
                _currentActiveButton.ForeColor = Color.Black;
            }

            _currentActiveButton = clickedButton;
            _currentActiveButton.BackColor = UiStyleHelper.SidebarBtnActive;
            _currentActiveButton.ForeColor = Color.White;

            _currentEntity = entity;

            pnlEntityTableContainer.Visible = false;
            pnlBottomActions.Visible = false;
            btnBackHeader.Visible = false;
            pnlEntityChoice.Visible = true;

            this.ResumeLayout();

            txtSearch.Visible = false;
        }

        private void OpenTableView()
        {
            this.SuspendLayout();

            pnlEntityChoice.Visible = false;
            pnlBottomActions.Visible = false;
            pnlEntityTableContainer.Visible = true;
            btnBackHeader.Visible = true;

            _currentSortColumn = "Id";
            _isSortAscending = true;

            LoadEntityData(_currentEntity);
            ApplyColumnSorting();
            gridMain.ClearSelection();

            this.ResumeLayout();

            txtSearch.Text = ""; txtSearch.Visible = true;
        }

        private void LoadEntityData(EntityType entity)
        {
            try
            {
                _currentDataList = entity switch
                {
                    EntityType.Passenger => PassengerCrudManager.GetAll().Cast<object>().ToList(),
                    EntityType.Pilot => PilotCrudManager.GetAll().Cast<object>().ToList(),
                    EntityType.FlightAttendant => FlightAttendantCrudManager.GetAll().Cast<object>().ToList(),
                    EntityType.Airplane => AirplaneCrudManager.GetAll().Cast<object>().ToList(),
                    EntityType.Airport => AirportCrudManager.GetAll().Cast<object>().ToList(),
                    EntityType.Route => RouteCrudManager.GetAll().Cast<object>().ToList(),
                    EntityType.Flight => FlightCrudManager.GetAll().Cast<object>().ToList(),
                    _ => new List<object>()
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки таблицы: {ex.Message}", "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GridMain_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            string clickedProp = gridMain.Columns[e.ColumnIndex].DataPropertyName;
            if (string.IsNullOrEmpty(clickedProp))
                clickedProp = gridMain.Columns[e.ColumnIndex].Name;

            if (_currentSortColumn == clickedProp)
                _isSortAscending = !_isSortAscending;
            else
            {
                _currentSortColumn = clickedProp;
                _isSortAscending = true;
            }

            ApplyColumnSorting();
        }

        private void ApplyColumnSorting()
        {
            if (_currentDataList.Count == 0)
            {
                gridMain.DataSource = null;
                return;
            }

            Type itemType = _currentDataList[0].GetType();
            PropertyInfo? prop = itemType.GetProperty(_currentSortColumn);

            IEnumerable<object> filtered = _currentDataList;
            if (txtSearch != null && !string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string q = txtSearch.Text.Trim().ToLowerInvariant();
                filtered = _currentDataList.Where(item =>
                    item.GetType().GetProperties().Any(p =>
                        (p.GetValue(item)?.ToString() ?? "").ToLowerInvariant().Contains(q)));
            }

            IEnumerable<object> sorted = filtered;
            if (prop != null)
            {
                sorted = _isSortAscending
                    ? filtered.OrderBy(x => GetComparableValue(prop.GetValue(x)))
                    : filtered.OrderByDescending(x => GetComparableValue(prop.GetValue(x)));
            }

            gridMain.DataSource = sorted.ToList();

            foreach (DataGridViewColumn col in gridMain.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Programmatic;
                if (col.DataPropertyName == _currentSortColumn || col.Name == _currentSortColumn)
                {
                    col.HeaderCell.SortGlyphDirection = _isSortAscending ? SortOrder.Ascending : SortOrder.Descending;
                }
                else
                {
                    col.HeaderCell.SortGlyphDirection = SortOrder.None;
                }
            }
        }

        private object? GetComparableValue(object? val)
        {
            if (val == null) return "";
            if (val is IComparable) return val;
            return val.ToString() ?? "";
        }

        private void btnAdd_Click(object? sender, EventArgs e)
        {
            using (var form = new EntityEditForm(_currentEntity))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    OpenTableView();
                }
            }
        }

        private void btnUpdate_Click(object? sender, EventArgs e)
        {
            if (gridMain.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(gridMain.SelectedRows[0].Cells["Id"].Value);

            using (var form = new EntityEditForm(_currentEntity, id))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadEntityData(_currentEntity);
                    ApplyColumnSorting();
                }
            }
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (gridMain.SelectedRows.Count == 0) return;

            int id = Convert.ToInt32(gridMain.SelectedRows[0].Cells["Id"].Value);
            string entityName = _currentActiveButton?.Text ?? "запись";

            string confirmMessage = $"Вы уверены безвозвратно удалить «{entityName}» с id {id}?";
            DialogResult result = MessageBox.Show(confirmMessage, "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    switch (_currentEntity)
                    {
                        case EntityType.Passenger: PassengerCrudManager.Delete(id); break;
                        case EntityType.Pilot: PilotCrudManager.Delete(id); break;
                        case EntityType.FlightAttendant: FlightAttendantCrudManager.Delete(id); break;
                        case EntityType.Airplane: AirplaneCrudManager.Delete(id); break;
                        case EntityType.Airport: AirportCrudManager.Delete(id); break;
                        case EntityType.Route: RouteCrudManager.Delete(id); break;
                        case EntityType.Flight: FlightCrudManager.Delete(id); break;
                    }

                    MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadEntityData(_currentEntity);
                    ApplyColumnSorting();
                    pnlBottomActions.Visible = false;
                }
                catch (MySqlException sqlEx) when (sqlEx.Number == 1451)
                {
                    MessageBox.Show(
                        "Невозможно удалить эту запись, так как она используется в других таблицах!\n",
                        "Защита связей базы данных",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}