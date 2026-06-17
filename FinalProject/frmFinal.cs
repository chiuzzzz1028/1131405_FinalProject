using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace FinalProject
{
    public partial class frmFinal : Form
    {
        List<TaskItem> taskList = new List<TaskItem>();
        TaskItem selectedTask = null;

        public frmFinal()
        {
            InitializeComponent();
        }

        private void frmFinal_Load(object sender, EventArgs e)
        {
            cmbType.SelectedIndex = 0;
            cmbPriority.SelectedIndex = 1;
            cmbStatus.SelectedIndex = 0;

            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterType.SelectedIndex = 0;

            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPriority.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilterStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilterType.DropDownStyle = ComboBoxStyle.DropDownList;

            dgvTasks.AllowUserToAddRows = false;
            dgvTasks.RowHeadersVisible = false;
            dgvTasks.ReadOnly = true;
            dgvTasks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTasks.MultiSelect = false;
            dgvTasks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            UpdateStatus();
        }
        private void ShowTasks(List<TaskItem> list)
        {
            dgvTasks.Rows.Clear();

            foreach (TaskItem item in list)
            {
                int rowIndex = dgvTasks.Rows.Add(
                    item.Subject,
                    item.Type,
                    item.TaskName,
                    item.DueDate.ToString("yyyy/MM/dd"),
                    item.Priority,
                    item.Status
                );

                DataGridViewRow row = dgvTasks.Rows[rowIndex];
                row.Tag = item;

                if (item.Status == "已完成")
                {
                    row.DefaultCellStyle.BackColor = Color.Honeydew;
                    row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                }
                else if (item.DueDate.Date < DateTime.Today)
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
                else if (item.DueDate.Date <= DateTime.Today.AddDays(3))
                {
                    row.DefaultCellStyle.BackColor = Color.LemonChiffon;
                    row.DefaultCellStyle.ForeColor = Color.DarkOrange;
                }
                else if (item.Status == "進行中")
                {
                    row.DefaultCellStyle.BackColor = Color.AliceBlue;
                    row.DefaultCellStyle.ForeColor = Color.DarkBlue;
                }
            }

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            int total = taskList.Count;
            int finished = taskList.Count(t => t.Status == "已完成");
            int unfinished = taskList.Count(t => t.Status != "已完成");

            DateTime today = DateTime.Today;

            int overdue = taskList.Count(t =>
                t.Status != "已完成" &&
                t.DueDate.Date < today
            );

            int soonDue = taskList.Count(t =>
                t.Status != "已完成" &&
                t.DueDate.Date >= today &&
                t.DueDate.Date <= today.AddDays(3)
            );

            sslTotal.Text = $"總項目數：{total}";
            sslUnfinished.Text = $"未完成：{unfinished}";
            sslFinished.Text = $"已完成：{finished}";
            sslOverdue.Text = $"已逾期：{overdue}";
            sslSoonDue.Text = $"即將到期：{soonDue}";

            if (overdue > 0)
            {
                sslReminder.Text = $"提醒：有 {overdue} 筆項目已逾期";
            }
            else if (soonDue > 0)
            {
                sslReminder.Text = $"提醒：有 {soonDue} 筆項目即將到期";
            }
            else
            {
                sslReminder.Text = "提醒：目前沒有提醒";
            }
        }
        private string CsvEscape(string text)
        {
            if (text.Contains(",") || text.Contains("\""))
            {
                text = text.Replace("\"", "\"\"");
                return "\"" + text + "\"";
            }

            return text;
        }

        private List<string> SplitCsvLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            string current = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            result.Add(current);
            return result;
        }

        private string TaskToCsv(TaskItem item)
        {
            return string.Join(",", new string[]
            {
              CsvEscape(item.Subject),
              CsvEscape(item.Type),
              CsvEscape(item.TaskName),
              item.DueDate.ToString("yyyy/MM/dd"),
              CsvEscape(item.Priority),
              CsvEscape(item.Status)
            });
        }
        private void ClearInput()
        {
            txtSubject.Clear();
            txtTaskName.Clear();

            cmbType.SelectedIndex = 0;
            cmbPriority.SelectedIndex = 1;
            cmbStatus.SelectedIndex = 0;

            dtpDueDate.Value = DateTime.Today;

            selectedTask = null;
            dgvTasks.ClearSelection();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (selectedTask != null)
            {
                MessageBox.Show(
                    "目前正在編輯已選取的資料。\n\n如果要修改這筆資料，請按「修改」。\n如果要新增新資料，請先按「清空」。",
                    "提醒",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            if (txtSubject.Text.Trim() == "")
            {
                MessageBox.Show("請輸入科目。", "提醒");
                return;
            }

            if (txtTaskName.Text.Trim() == "")
            {
                MessageBox.Show("請輸入名稱。", "提醒");
                return;
            }

            TaskItem item = new TaskItem()
            {
                Subject = txtSubject.Text.Trim(),
                Type = cmbType.Text,
                TaskName = txtTaskName.Text.Trim(),
                DueDate = dtpDueDate.Value.Date,
                Priority = cmbPriority.Text,
                Status = cmbStatus.Text
            };

            taskList.Add(item);

            ShowTasks(taskList);
            ClearInput();

            MessageBox.Show("新增成功！", "完成");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInput();
        }

        private void dgvTasks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvTasks.Rows[e.RowIndex];

            if (row.Tag == null)
            {
                return;
            }

            selectedTask = row.Tag as TaskItem;

            txtSubject.Text = selectedTask.Subject;
            cmbType.SelectedItem = selectedTask.Type;
            txtTaskName.Text = selectedTask.TaskName;
            dtpDueDate.Value = selectedTask.DueDate;
            cmbPriority.SelectedItem = selectedTask.Priority;
            cmbStatus.SelectedItem = selectedTask.Status;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedTask == null)
            {
                MessageBox.Show("請先選取要修改的項目。", "提醒");
                return;
            }

            if (txtSubject.Text.Trim() == "")
            {
                MessageBox.Show("請輸入科目。", "提醒");
                return;
            }

            if (txtTaskName.Text.Trim() == "")
            {
                MessageBox.Show("請輸入名稱。", "提醒");
                return;
            }

            selectedTask.Subject = txtSubject.Text.Trim();
            selectedTask.Type = cmbType.Text;
            selectedTask.TaskName = txtTaskName.Text.Trim();
            selectedTask.DueDate = dtpDueDate.Value.Date;
            selectedTask.Priority = cmbPriority.Text;
            selectedTask.Status = cmbStatus.Text;

            ShowTasks(taskList);
            ClearInput();

            MessageBox.Show("修改成功！", "完成");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedTask == null)
            {
                MessageBox.Show("請先選取要刪除的項目。", "提醒");
                return;
            }

            DialogResult result = MessageBox.Show(
                "確定要刪除這筆項目嗎？",
                "刪除確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                taskList.Remove(selectedTask);

                ShowTasks(taskList);
                ClearInput();

                MessageBox.Show("刪除成功！", "完成");
            }
        }
        private void ApplySearchAndFilter()
        {
            string keyword = txtSearch.Text.Trim();
            string filterStatus = cmbFilterStatus.Text;
            string filterType = cmbFilterType.Text;

            List<TaskItem> result = taskList;

            if (keyword != "")
            {
                result = result.Where(t =>
                    t.Subject.Contains(keyword) ||
                    t.TaskName.Contains(keyword)
                ).ToList();
            }

            if (filterStatus != "全部")
            {
                result = result.Where(t => t.Status == filterStatus).ToList();
            }

            if (filterType != "全部")
            {
                result = result.Where(t => t.Type == filterType).ToList();
            }

            ShowTasks(result);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            ApplySearchAndFilter();
        }

        private void btnShowAll_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();

            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterType.SelectedIndex = 0;

            ShowTasks(taskList);
        }

        private void cmbFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySearchAndFilter();
        }

        private void cmbFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySearchAndFilter();
        }

        private void btnSortByDate_Click(object sender, EventArgs e)
        {
            taskList = taskList.OrderBy(t => t.DueDate).ToList();

            ApplySearchAndFilter();

            MessageBox.Show("已依截止日期排序。", "排序完成");
        }
        private int GetPriorityOrder(string priority)
        {
            if (priority == "高")
            {
                return 1;
            }
            else if (priority == "中")
            {
                return 2;
            }
            else if (priority == "低")
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        private void btnSortByPriority_Click(object sender, EventArgs e)
        {
            taskList = taskList.OrderBy(t => GetPriorityOrder(t.Priority)).ThenBy(t => t.DueDate).ToList();

            ApplySearchAndFilter();

            MessageBox.Show("已依優先程度排序。", "排序完成");
        }

        private void tsmiSave_Click(object sender, EventArgs e)
        {
            if (taskList.Count == 0)
            {
                MessageBox.Show("目前沒有資料可以儲存。", "提醒");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV 檔案 (*.csv)|*.csv|文字檔案 (*.txt)|*.txt";
            saveFileDialog.Title = "儲存項目資料";
            saveFileDialog.FileName = "學生作業與考試管理.csv";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                List<string> lines = new List<string>();

                lines.Add("科目,類型,名稱,截止日期,優先程度,狀態");

                foreach (TaskItem item in taskList)
                {
                    lines.Add(TaskToCsv(item));
                }

                File.WriteAllLines(saveFileDialog.FileName, lines, Encoding.UTF8);

                MessageBox.Show("檔案儲存成功！", "完成");
            }
        }

        private void tsmiLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV 檔案 (*.csv)|*.csv|文字檔案 (*.txt)|*.txt";
            openFileDialog.Title = "讀取項目資料";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName, Encoding.UTF8);

                    if (lines.Length == 0)
                    {
                        MessageBox.Show("檔案內容是空的，無法讀取。", "讀取失敗");
                        return;
                    }

                    List<TaskItem> tempList = new List<TaskItem>();

                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i].Trim();

                        if (line == "")
                        {
                            continue;
                        }

                        if (i == 0 && line.TrimStart('\uFEFF').StartsWith("科目"))
                        {
                            continue;
                        }

                        List<string> parts = SplitCsvLine(line);

                        if (parts.Count != 6)
                        {
                            MessageBox.Show(
                                $"第 {i + 1} 行資料格式錯誤，欄位數量不正確。\n\n請確認檔案格式是否為：\n科目,類型,名稱,截止日期,優先程度,狀態",
                                "讀取失敗",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return;
                        }

                        DateTime dueDate;
                        if (!DateTime.TryParse(parts[3], out dueDate))
                        {
                            MessageBox.Show(
                                $"第 {i + 1} 行的截止日期格式錯誤。\n\n錯誤內容：{parts[3]}",
                                "讀取失敗",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return;
                        }

                        TaskItem item = new TaskItem()
                        {
                            Subject = parts[0],
                            Type = parts[1],
                            TaskName = parts[2],
                            DueDate = dueDate.Date,
                            Priority = parts[4],
                            Status = parts[5]
                        };

                        tempList.Add(item);
                    }

                    taskList = tempList;

                    ShowTasks(taskList);
                    ClearInput();

                    MessageBox.Show("檔案讀取成功！", "完成");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "讀取檔案時發生錯誤：\n\n" + ex.Message,
                        "讀取失敗",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void tsmiUserGuide_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
               "學生作業與考試管理系統操作說明：\n\n" +

               "1. 在左側資料輸入區輸入科目、類型、名稱、截止日期、優先程度與狀態。\n\n" +

               "2. 按下「新增項目」後，資料會加入中間的項目清單。\n\n" +

               "3. 點選項目清單中的資料後，資料會自動帶回左側欄位，可進行修改或刪除。\n\n" +

               "4. 若想快速完成某筆項目，可在項目清單中雙擊該筆資料，確認後系統會將狀態改為「已完成」。\n\n" +

               "5. 可使用右側的關鍵字搜尋、狀態篩選與類型篩選來查找資料。\n\n" +

               "6. 可使用「依截止日期排序」或「依優先程度排序」整理項目順序。\n\n" +

               "7. 下方狀態列會顯示總項目數、未完成、已完成、逾期與即將到期的項目數量。\n\n" +

               "8. 項目清單會依項目狀態與到期情況顯示不同顏色，方便使用者快速辨識。\n\n" +

               "9. 可透過上方「檔案」選單進行 CSV 檔案的儲存與讀取。",
               "操作說明",
               MessageBoxButtons.OK,
               MessageBoxIcon.Information
    );
        }

        private void tsmiAbout_Click(object sender, EventArgs e)
        {
            frmAbout aboutForm = new frmAbout();
            aboutForm.ShowDialog(this);
        }

        private void dgvTasks_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvTasks.Rows[e.RowIndex];

            if (row.Tag == null)
            {
                return;
            }

            TaskItem item = row.Tag as TaskItem;

            if (item == null)
            {
                return;
            }

            if (item.Status == "已完成")
            {
                MessageBox.Show("這筆項目已經是完成狀態。", "提醒");
                return;
            }

            DialogResult result = MessageBox.Show(
                "確定要將這筆項目標記為已完成嗎？",
                "標記完成確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                item.Status = "已完成";

                selectedTask = null;

                ApplySearchAndFilter();
                ClearInput();

                MessageBox.Show("已將項目標記為完成！", "完成");
            }
        }
    }
}
