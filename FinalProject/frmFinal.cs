using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

                dgvTasks.Rows[rowIndex].Tag = item;
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

            sslTotal.Text = $"總事項數：{total}";
            sslUnfinished.Text = $"未完成：{unfinished}";
            sslFinished.Text = $"已完成：{finished}";
            sslOverdue.Text = $"已逾期：{overdue}";
            sslSoonDue.Text = $"即將到期：{soonDue}";

            if (overdue > 0)
            {
                sslReminder.Text = $"提醒：有 {overdue} 筆事項已逾期";
            }
            else if (soonDue > 0)
            {
                sslReminder.Text = $"提醒：有 {soonDue} 筆事項即將到期";
            }
            else
            {
                sslReminder.Text = "提醒：目前沒有提醒";
            }
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
                MessageBox.Show("請先選取要修改的事項。", "提醒");
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
                MessageBox.Show("請先選取要刪除的事項。", "提醒");
                return;
            }

            DialogResult result = MessageBox.Show(
                "確定要刪除這筆事項嗎？",
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
    }
}
