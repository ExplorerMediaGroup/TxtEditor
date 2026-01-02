using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TxtEditor
{
    public class MainForm : Form
    {
        private string currentFilePath = string.Empty;
        private bool isModified = false;

        // 控件声明
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;
        private TextBox textBox;
        private ToolStripStatusLabel statusLabel;

        public MainForm()
        {
            InitializeControls();
            UpdateTitle();
            UpdateStatusBar();
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Explorer TXT Editor";
        }

        private void InitializeControls()
        {
            // 创建菜单栏
            menuStrip = new MenuStrip();

            // 文件菜单
            var fileMenu = new ToolStripMenuItem("文件(&F)");
            var newMenuItem = new ToolStripMenuItem("新建(&N)", null, NewFile_Click, Keys.Control | Keys.N);
            var openMenuItem = new ToolStripMenuItem("打开(&O)...", null, OpenFile_Click, Keys.Control | Keys.O);
            var saveMenuItem = new ToolStripMenuItem("保存(&S)", null, SaveFile_Click, Keys.Control | Keys.S);
            var saveAsMenuItem = new ToolStripMenuItem("另存为(&A)...", null, SaveAsFile_Click);
            var exitMenuItem = new ToolStripMenuItem("退出(&X)", null, Exit_Click);

            fileMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                newMenuItem,
                openMenuItem,
                saveMenuItem,
                saveAsMenuItem,
                new ToolStripSeparator(),
                exitMenuItem
            });

            // 编辑菜单
            var editMenu = new ToolStripMenuItem("编辑(&E)");
            var undoMenuItem = new ToolStripMenuItem("撤销(&U)", null, Undo_Click, Keys.Control | Keys.Z);
            var redoMenuItem = new ToolStripMenuItem("重做(&R)", null, Redo_Click, Keys.Control | Keys.Y);
            var cutMenuItem = new ToolStripMenuItem("剪切(&T)", null, Cut_Click, Keys.Control | Keys.X);
            var copyMenuItem = new ToolStripMenuItem("复制(&C)", null, Copy_Click, Keys.Control | Keys.C);
            var pasteMenuItem = new ToolStripMenuItem("粘贴(&P)", null, Paste_Click, Keys.Control | Keys.V);
            var selectAllMenuItem = new ToolStripMenuItem("全选(&A)", null, SelectAll_Click, Keys.Control | Keys.A);

            editMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                undoMenuItem,
                redoMenuItem,
                new ToolStripSeparator(),
                cutMenuItem,
                copyMenuItem,
                pasteMenuItem,
                new ToolStripSeparator(),
                selectAllMenuItem
            });

            // 帮助菜单
            var helpMenu = new ToolStripMenuItem("帮助(&H)");
            var aboutMenuItem = new ToolStripMenuItem("关于(&A)", null, About_Click);
            helpMenu.DropDownItems.Add(aboutMenuItem);

            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, helpMenu });

            // 创建工具栏
            toolStrip = new ToolStrip();

            // 使用文本代替图标
            var newButton = new ToolStripButton("新建");
            newButton.Click += NewFile_Click;
            newButton.ToolTipText = "新建文档 (Ctrl+N)";

            var openButton = new ToolStripButton("打开");
            openButton.Click += OpenFile_Click;
            openButton.ToolTipText = "打开文件 (Ctrl+O)";

            var saveButton = new ToolStripButton("保存");
            saveButton.Click += SaveFile_Click;
            saveButton.ToolTipText = "保存文件 (Ctrl+S)";

            toolStrip.Items.AddRange(new ToolStripItem[] { newButton, openButton, saveButton });

            // 创建状态栏
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);

            // 创建文本编辑框
            textBox = new TextBox();
            textBox.Multiline = true;
            textBox.Dock = DockStyle.Fill;
            textBox.ScrollBars = ScrollBars.Both;
            textBox.Font = new Font("微软雅黑", 10);
            textBox.AcceptsTab = true;
            textBox.WordWrap = false;
            textBox.TextChanged += TextBox_TextChanged;

            // 设置布局
            menuStrip.Dock = DockStyle.Top;
            toolStrip.Dock = DockStyle.Top;
            statusStrip.Dock = DockStyle.Bottom;

            // 添加到窗体
            this.Controls.Add(textBox);
            this.Controls.Add(statusStrip);
            this.Controls.Add(toolStrip);
            this.Controls.Add(menuStrip);

            // 设置主菜单
            this.MainMenuStrip = menuStrip;
        }

        private void UpdateTitle()
        {
            string filename = string.IsNullOrEmpty(currentFilePath) ? "未命名.txt" : Path.GetFileName(currentFilePath);
            string modified = isModified ? "*" : "";
            this.Text = $"{filename}{modified} - Explorer TXT Editor";
        }

        private void UpdateStatusBar()
        {
            int lineCount = textBox.Lines.Length;
            int charCount = textBox.Text.Length;
            int wordCount = CountWords(textBox.Text);

            statusLabel.Text = $"行数: {lineCount} | 字符数: {charCount} | 单词数: {wordCount} | 编码: UTF-8";
        }

        private int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            char[] delimiters = new char[] { ' ', '\r', '\n', '\t' };
            return text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private bool ConfirmSaveChanges()
        {
            if (isModified)
            {
                DialogResult result = MessageBox.Show(
                    "文档已修改，是否保存更改？",
                    "保存更改",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    return SaveFile();
                }
                else if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }

        private bool SaveFile()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                return SaveAsFile();
            }
            else
            {
                try
                {
                    File.WriteAllText(currentFilePath, textBox.Text, System.Text.Encoding.UTF8);
                    isModified = false;
                    UpdateTitle();
                    MessageBox.Show("保存成功！", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存文件时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private bool SaveAsFile()
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "文本文档 (*.txt)|*.txt|所有文件 (*.*)|*.*";
                saveDialog.FilterIndex = 1;
                saveDialog.RestoreDirectory = true;
                saveDialog.DefaultExt = "txt";
                saveDialog.AddExtension = true;

                if (string.IsNullOrEmpty(currentFilePath))
                {
                    saveDialog.FileName = "未命名.txt";
                }
                else
                {
                    saveDialog.FileName = currentFilePath;
                }

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = saveDialog.FileName;
                    return SaveFile();
                }
                return false;
            }
        }

        // 事件处理方法
        private void NewFile_Click(object sender, EventArgs e)
        {
            if (!ConfirmSaveChanges())
                return;

            textBox.Clear();
            currentFilePath = string.Empty;
            isModified = false;
            UpdateTitle();
            UpdateStatusBar();
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            if (!ConfirmSaveChanges())
                return;

            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "文本文档 (*.txt)|*.txt|所有文件 (*.*)|*.*";
                openDialog.FilterIndex = 1;
                openDialog.RestoreDirectory = true;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string content = File.ReadAllText(openDialog.FileName, System.Text.Encoding.UTF8);
                        textBox.Text = content;
                        currentFilePath = openDialog.FileName;
                        isModified = false;
                        UpdateTitle();
                        UpdateStatusBar();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"打开文件时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveFile_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void SaveAsFile_Click(object sender, EventArgs e)
        {
            SaveAsFile();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Undo_Click(object sender, EventArgs e)
        {
            if (textBox.CanUndo)
                textBox.Undo();
        }

        private void Redo_Click(object sender, EventArgs e)
        {
            SendKeys.Send("^Y");
        }

        private void Cut_Click(object sender, EventArgs e)
        {
            textBox.Cut();
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            textBox.Copy();
        }

        private void Paste_Click(object sender, EventArgs e)
        {
            textBox.Paste();
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {
            textBox.SelectAll();
        }

        private void About_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Explorer TXT Editor v1.0\n\n一个简单的文本编辑器，支持创建、编辑和保存TXT文件。\n完全由DeepSeek v3.2制作。",
                "关于",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (!isModified)
            {
                isModified = true;
                UpdateTitle();
            }
            UpdateStatusBar();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!ConfirmSaveChanges())
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
    }
}