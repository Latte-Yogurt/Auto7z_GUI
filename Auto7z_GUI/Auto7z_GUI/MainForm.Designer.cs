
namespace Auto7z_GUI
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainPanel = new System.Windows.Forms.Panel();
            this.CheckBoxZstd = new System.Windows.Forms.CheckBox();
            this.ComboBoxFormat = new System.Windows.Forms.ComboBox();
            this.TextBoxPassword = new System.Windows.Forms.TextBox();
            this.LabelPassword = new System.Windows.Forms.Label();
            this.LabelFormat = new System.Windows.Forms.Label();
            this.LabelUnit = new System.Windows.Forms.Label();
            this.TextBoxSize = new System.Windows.Forms.TextBox();
            this.LabelSize = new System.Windows.Forms.Label();
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.LanguageMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.LanguageMenuSelect = new System.Windows.Forms.ToolStripMenuItem();
            this.LanguageMenuSelectzhCN = new System.Windows.Forms.ToolStripMenuItem();
            this.LanguageMenuSelectzhTW = new System.Windows.Forms.ToolStripMenuItem();
            this.LanguageMenuSelectenUS = new System.Windows.Forms.ToolStripMenuItem();
            this.OptionMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.OptionMenuDisableSplit = new System.Windows.Forms.ToolStripMenuItem();
            this.OptionMenuCreateMD5 = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutMenuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.ButtonConfig = new System.Windows.Forms.Button();
            this.CheckBoxAutoSave = new System.Windows.Forms.CheckBox();
            this.MainPanel.SuspendLayout();
            this.MenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.CheckBoxZstd);
            this.MainPanel.Controls.Add(this.ComboBoxFormat);
            this.MainPanel.Controls.Add(this.TextBoxPassword);
            this.MainPanel.Controls.Add(this.LabelPassword);
            this.MainPanel.Controls.Add(this.LabelFormat);
            this.MainPanel.Controls.Add(this.LabelUnit);
            this.MainPanel.Controls.Add(this.TextBoxSize);
            this.MainPanel.Controls.Add(this.LabelSize);
            this.MainPanel.Controls.Add(this.MenuStrip);
            this.MainPanel.Controls.Add(this.ButtonConfig);
            this.MainPanel.Controls.Add(this.CheckBoxAutoSave);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(478, 444);
            this.MainPanel.TabIndex = 0;
            // 
            // CheckBoxZstd
            // 
            this.CheckBoxZstd.AutoSize = true;
            this.CheckBoxZstd.Location = new System.Drawing.Point(315, 214);
            this.CheckBoxZstd.Name = "CheckBoxZstd";
            this.CheckBoxZstd.Size = new System.Drawing.Size(72, 28);
            this.CheckBoxZstd.TabIndex = 10;
            this.CheckBoxZstd.Text = "zstd";
            this.CheckBoxZstd.UseVisualStyleBackColor = true;
            this.CheckBoxZstd.CheckedChanged += new System.EventHandler(this.CHECKBOX_ZSTD_CHECKED_CHANGED);
            // 
            // ComboBoxFormat
            // 
            this.ComboBoxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboBoxFormat.FormattingEnabled = true;
            this.ComboBoxFormat.Location = new System.Drawing.Point(201, 211);
            this.ComboBoxFormat.Name = "ComboBoxFormat";
            this.ComboBoxFormat.Size = new System.Drawing.Size(104, 32);
            this.ComboBoxFormat.TabIndex = 8;
            this.ComboBoxFormat.SelectedIndexChanged += new System.EventHandler(this.COMBOBOX_FORMAT_SELECTED_INDEX_CHANGED);
            // 
            // TextBoxPassword
            // 
            this.TextBoxPassword.Location = new System.Drawing.Point(201, 265);
            this.TextBoxPassword.Name = "TextBoxPassword";
            this.TextBoxPassword.Size = new System.Drawing.Size(145, 31);
            this.TextBoxPassword.TabIndex = 7;
            this.TextBoxPassword.TextChanged += new System.EventHandler(this.TEXTBOX_PASSWORD_TEXT_CHANGED);
            // 
            // LabelPassword
            // 
            this.LabelPassword.AutoSize = true;
            this.LabelPassword.Location = new System.Drawing.Point(111, 267);
            this.LabelPassword.Name = "LabelPassword";
            this.LabelPassword.Size = new System.Drawing.Size(100, 24);
            this.LabelPassword.TabIndex = 6;
            this.LabelPassword.Text = "添加密码：";
            // 
            // LabelFormat
            // 
            this.LabelFormat.AutoSize = true;
            this.LabelFormat.Location = new System.Drawing.Point(111, 215);
            this.LabelFormat.Name = "LabelFormat";
            this.LabelFormat.Size = new System.Drawing.Size(100, 24);
            this.LabelFormat.TabIndex = 5;
            this.LabelFormat.Text = "生成格式：";
            // 
            // LabelUnit
            // 
            this.LabelUnit.AutoSize = true;
            this.LabelUnit.Location = new System.Drawing.Point(311, 161);
            this.LabelUnit.Name = "LabelUnit";
            this.LabelUnit.Size = new System.Drawing.Size(39, 24);
            this.LabelUnit.TabIndex = 4;
            this.LabelUnit.Text = "MB";
            // 
            // TextBoxSize
            // 
            this.TextBoxSize.Location = new System.Drawing.Point(201, 159);
            this.TextBoxSize.Name = "TextBoxSize";
            this.TextBoxSize.Size = new System.Drawing.Size(104, 31);
            this.TextBoxSize.TabIndex = 1;
            this.TextBoxSize.TextChanged += new System.EventHandler(this.TEXTBOX_SIZE_TEXT_CHANGED);
            this.TextBoxSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TEXTBOX_SIZE_KEYPRESS);
            // 
            // LabelSize
            // 
            this.LabelSize.AutoSize = true;
            this.LabelSize.Location = new System.Drawing.Point(111, 161);
            this.LabelSize.Name = "LabelSize";
            this.LabelSize.Size = new System.Drawing.Size(100, 24);
            this.LabelSize.TabIndex = 0;
            this.LabelSize.Text = "分卷大小：";
            // 
            // MenuStrip
            // 
            this.MenuStrip.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.MenuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.MenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LanguageMenu,
            this.OptionMenu,
            this.AboutMenu});
            this.MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Size = new System.Drawing.Size(478, 32);
            this.MenuStrip.TabIndex = 2;
            this.MenuStrip.Text = "menuStrip1";
            // 
            // LanguageMenu
            // 
            this.LanguageMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LanguageMenuSelect});
            this.LanguageMenu.Name = "LanguageMenu";
            this.LanguageMenu.Size = new System.Drawing.Size(62, 28);
            this.LanguageMenu.Text = "语言";
            // 
            // LanguageMenuSelect
            // 
            this.LanguageMenuSelect.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LanguageMenuSelectzhCN,
            this.LanguageMenuSelectzhTW,
            this.LanguageMenuSelectenUS});
            this.LanguageMenuSelect.Name = "LanguageMenuSelect";
            this.LanguageMenuSelect.Size = new System.Drawing.Size(182, 34);
            this.LanguageMenuSelect.Text = "选择语言";
            // 
            // LanguageMenuSelectzhCN
            // 
            this.LanguageMenuSelectzhCN.Name = "LanguageMenuSelectzhCN";
            this.LanguageMenuSelectzhCN.Size = new System.Drawing.Size(182, 34);
            this.LanguageMenuSelectzhCN.Text = "简体中文";
            this.LanguageMenuSelectzhCN.Click += new System.EventHandler(this.LANGUAGE_MENU_SELECT_zhCN_CLICK);
            // 
            // LanguageMenuSelectzhTW
            // 
            this.LanguageMenuSelectzhTW.Name = "LanguageMenuSelectzhTW";
            this.LanguageMenuSelectzhTW.Size = new System.Drawing.Size(182, 34);
            this.LanguageMenuSelectzhTW.Text = "繁體中文";
            this.LanguageMenuSelectzhTW.Click += new System.EventHandler(this.LANGUAGE_MENU_SELECT_zhTW_CLICK);
            // 
            // LanguageMenuSelectenUS
            // 
            this.LanguageMenuSelectenUS.Name = "LanguageMenuSelectenUS";
            this.LanguageMenuSelectenUS.Size = new System.Drawing.Size(182, 34);
            this.LanguageMenuSelectenUS.Text = "English";
            this.LanguageMenuSelectenUS.Click += new System.EventHandler(this.LANGUAGE_MENU_SELECT_enUS_CLICK);
            // 
            // OptionMenu
            // 
            this.OptionMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OptionMenuDisableSplit,
            this.OptionMenuCreateMD5});
            this.OptionMenu.Name = "OptionMenu";
            this.OptionMenu.Size = new System.Drawing.Size(62, 28);
            this.OptionMenu.Text = "选项";
            // 
            // OptionMenuDisableSplit
            // 
            this.OptionMenuDisableSplit.Name = "OptionMenuDisableSplit";
            this.OptionMenuDisableSplit.Size = new System.Drawing.Size(315, 34);
            this.OptionMenuDisableSplit.Text = "禁用分卷";
            this.OptionMenuDisableSplit.CheckedChanged += new System.EventHandler(this.OPTION_MENU_DISABLE_SPLIT_CHECKED_CHANGED);
            this.OptionMenuDisableSplit.Click += new System.EventHandler(this.OPTION_MENU_DISABLE_SPLIT_CLICK);
            // 
            // OptionMenuCreateMD5
            // 
            this.OptionMenuCreateMD5.Name = "OptionMenuCreateMD5";
            this.OptionMenuCreateMD5.Size = new System.Drawing.Size(315, 34);
            this.OptionMenuCreateMD5.Text = "压缩完成后生成MD5文件";
            this.OptionMenuCreateMD5.CheckedChanged += new System.EventHandler(this.OPTION_MENU_CREATE_MD5_CHECKED_CHANGED);
            this.OptionMenuCreateMD5.Click += new System.EventHandler(this.OPTION_MENU_CREATE_MD5_CLICK);
            // 
            // AboutMenu
            // 
            this.AboutMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AboutMenuAbout});
            this.AboutMenu.Name = "AboutMenu";
            this.AboutMenu.Size = new System.Drawing.Size(62, 28);
            this.AboutMenu.Text = "关于";
            // 
            // AboutMenuAbout
            // 
            this.AboutMenuAbout.Name = "AboutMenuAbout";
            this.AboutMenuAbout.Size = new System.Drawing.Size(172, 34);
            this.AboutMenuAbout.Text = "Auto7z";
            this.AboutMenuAbout.Click += new System.EventHandler(this.ABOUTMENU_ABOUT);
            // 
            // ButtonConfig
            // 
            this.ButtonConfig.Location = new System.Drawing.Point(346, 402);
            this.ButtonConfig.Name = "ButtonConfig";
            this.ButtonConfig.Size = new System.Drawing.Size(120, 30);
            this.ButtonConfig.TabIndex = 3;
            this.ButtonConfig.Text = "保存配置";
            this.ButtonConfig.UseVisualStyleBackColor = true;
            this.ButtonConfig.Click += new System.EventHandler(this.BUTTON_CONFIG_CLICK);
            // 
            // CheckBoxAutoSave
            // 
            this.CheckBoxAutoSave.AutoSize = true;
            this.CheckBoxAutoSave.Location = new System.Drawing.Point(12, 403);
            this.CheckBoxAutoSave.Name = "CheckBoxAutoSave";
            this.CheckBoxAutoSave.Size = new System.Drawing.Size(234, 28);
            this.CheckBoxAutoSave.TabIndex = 9;
            this.CheckBoxAutoSave.Text = "程序关闭时自动保存配置";
            this.CheckBoxAutoSave.UseVisualStyleBackColor = true;
            this.CheckBoxAutoSave.CheckedChanged += new System.EventHandler(this.CHECKBOX_AUTOSAVE_CHECKED_CHANGED);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(478, 444);
            this.Controls.Add(this.MainPanel);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MenuStrip;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(500, 500);
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.Name = "MainForm";
            this.Text = "Auto7z";
            this.Load += new System.EventHandler(this.MAINFORM_LOAD);
            this.Resize += new System.EventHandler(this.MAINFORM_RESIZE);
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.TextBox TextBoxSize;
        private System.Windows.Forms.Label LabelSize;
        private System.Windows.Forms.MenuStrip MenuStrip;
        private System.Windows.Forms.ToolStripMenuItem LanguageMenu;
        private System.Windows.Forms.ToolStripMenuItem AboutMenu;
        private System.Windows.Forms.ToolStripMenuItem AboutMenuAbout;
        private System.Windows.Forms.ComboBox ComboBoxFormat;
        private System.Windows.Forms.TextBox TextBoxPassword;
        private System.Windows.Forms.Label LabelPassword;
        private System.Windows.Forms.Label LabelFormat;
        private System.Windows.Forms.Label LabelUnit;
        private System.Windows.Forms.Button ButtonConfig;
        private System.Windows.Forms.ToolStripMenuItem LanguageMenuSelect;
        private System.Windows.Forms.ToolStripMenuItem LanguageMenuSelectzhCN;
        private System.Windows.Forms.ToolStripMenuItem LanguageMenuSelectzhTW;
        private System.Windows.Forms.ToolStripMenuItem LanguageMenuSelectenUS;
        private System.Windows.Forms.CheckBox CheckBoxAutoSave;
        private System.Windows.Forms.CheckBox CheckBoxZstd;
        private System.Windows.Forms.ToolStripMenuItem OptionMenu;
        private System.Windows.Forms.ToolStripMenuItem OptionMenuCreateMD5;
        private System.Windows.Forms.ToolStripMenuItem OptionMenuDisableSplit;
    }
}

