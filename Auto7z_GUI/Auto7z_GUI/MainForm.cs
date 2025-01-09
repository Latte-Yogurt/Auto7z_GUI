using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Auto7z_GUI
{
    public partial class MainForm : Form
    {
        private Dictionary<string, Dictionary<string, string>> languageTexts;

        public static string currentLanguage;
        public string xmlPath;
        public int locationX;
        public int locationY;
        public string partSize;
        public string format;
        public string password;
        public string autoSave;
        public string filePath;
        public string fileName;
        public string directoryPath;
        public long fileSize;
        public long folderSize;
        public string sevenZPath;
        public string sevenZDllPath;
        public string newFolderPath;

        public MainForm(string[] args)
        {
            InitializeComponent();

            SET_COMPONENT_POSITION();

            string workPath = GET_WORK_PATH(); // 获取程序路径
            string xml = @"config.xml";
            xmlPath = Path.Combine(workPath, xml);

            currentLanguage = GET_CURRENT_LANGUAGE(xmlPath);

            InitializeLanguageTexts();
            UpdateLanguage();

            this.AllowDrop = true; // 允许拖放
            this.DragEnter += new DragEventHandler(MAINFORM_DRAGENTER);
            this.DragDrop += new DragEventHandler(MAINFORM_DRAGDROP);
            this.DragLeave += new EventHandler(MAINFORM_DRAGLEAVE);
            this.FormClosing += MAINFORM_FORM_CLOSING;

            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                filePath = args[0];

                if (File.Exists(filePath))
                {
                    fileSize = GET_FILE_SIZE(filePath);

                    if (fileSize == -1)
                    {
                        switch (currentLanguage)
                        {
                            case "zh-CN":
                                MessageBox.Show("传入路径为空路径。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            case "zh-TW":
                                MessageBox.Show("傳入路徑為空路徑。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            case "en-US":
                                MessageBox.Show("The input path is an empty path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                        }

                        this.Close();
                        return;
                    }
                }

                else if (Directory.Exists(filePath))
                {
                    folderSize = GET_FOLDER_SIZE(filePath);

                    if (folderSize == -1)
                    {
                        switch (currentLanguage)
                        {
                            case "zh-CN":
                                MessageBox.Show("传入路径为空路径。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            case "zh-TW":
                                MessageBox.Show("傳入路徑為空路徑。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            case "en-US":
                                MessageBox.Show("The input path is an empty path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                        }

                        this.Close();
                        return;
                    }
                }

                fileName = Path.GetFileNameWithoutExtension(filePath);
                directoryPath = Path.GetDirectoryName(filePath);
            }

            string sevenZ = @"7z.exe";
            sevenZPath = Path.Combine(workPath, sevenZ);

            string sevenZDll = @"7z.dll";
            sevenZDllPath = Path.Combine(workPath, sevenZDll);

            partSize = GET_PARTSIZE(xmlPath);
            format = GET_FORMAT(xmlPath);
            password = GET_PASSWORD(xmlPath);
            autoSave = GET_AUTOSAVE(xmlPath);

            DEFAULT_PARTSIZE_TEXTBOX();
            DEFAULT_FORMAT_MENU();
            DEFAULT_PASSWORD_TEXTBOX();
            DEFAULT_AUTOSAVE();

            if (filePath != null)
            {
                if (!CHECK_SEVENZ_EXIST())
                {
                    switch (currentLanguage)
                    {
                        case "zh-CN":
                            MessageBox.Show("程序工作路径下7z组件(7z.exe 7z.dll Lang)不完整，无法启动7z处理流程。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "zh-TW":
                            MessageBox.Show("程式工作路徑下7z組件(7z.exe 7z.dll Lang)不完整，無法啓動7z處理流程。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "en-US":
                            MessageBox.Show("The 7z components (7z.exe 7z.dll Lang) in the program's working directory are incomplete and can not start the 7z processing flow.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }

                    this.Close();
                    return;
                }

                else
                {
                    string command = GENERATE_COMMAND();
                    EXECUTE_COMMAND(command);
                    this.Close();
                }
            }
        }

        private void InitializeLanguageTexts()
        {
            languageTexts = new Dictionary<string, Dictionary<string, string>>
            {
                { "zh-CN", new Dictionary<string, string>
                    {
                        { "LanguageMenu","语言" },
                        { "LanguageMenuSelect","选择语言" },
                        { "AboutMenu","关于" },
                        { "LabelSize","分卷大小：" },
                        { "LabelFormat","生成格式：" },
                        { "LabelPassword","添加密码：" },
                        { "CheckBoxAutoSave","程序关闭时自动保存配置" },
                        { "ButtonConfig","保存配置" }
                    }
                },
                { "zh-TW", new Dictionary<string, string>
                    {
                        { "LanguageMenu","語言" },
                        { "LanguageMenuSelect","選擇語言" },
                        { "AboutMenu","關於" },
                        { "LabelSize","分卷大小：" },
                        { "LabelFormat","生成格式：" },
                        { "LabelPassword","添加密碼：" },
                        { "CheckBoxAutoSave","程式關閉時自動保存配置" },
                        { "ButtonConfig","保存配置" }
                    }
                },
                { "en-US", new Dictionary<string, string>
                    {
                        { "LanguageMenu","Language" },
                        { "LanguageMenuSelect","Select Language" },
                        { "AboutMenu","About" },
                        { "LabelSize","Part Size：" },
                        { "LabelFormat","Generate Format：" },
                        { "LabelPassword","Add Password：" },
                        { "CheckBoxAutoSave","Auto save config while exit" },
                        { "ButtonConfig","Save Config" }
                    }
                }
            };
        }

        private void UpdateLanguage()
        {
            LanguageMenu.Text = languageTexts[currentLanguage]["LanguageMenu"];
            LanguageMenuSelect.Text = languageTexts[currentLanguage]["LanguageMenuSelect"];
            AboutMenu.Text = languageTexts[currentLanguage]["AboutMenu"];
            LabelSize.Text = languageTexts[currentLanguage]["LabelSize"];
            LabelFormat.Text = languageTexts[currentLanguage]["LabelFormat"];
            LabelPassword.Text = languageTexts[currentLanguage]["LabelPassword"];
            CheckBoxAutoSave.Text = languageTexts[currentLanguage]["CheckBoxAutoSave"];
            ButtonConfig.Text = languageTexts[currentLanguage]["ButtonConfig"];

            SET_COMPONENT_POSITION();
        }

        private void EXECUTE_COMMAND(string command)
        {
            var process = new Process();
            process.StartInfo.FileName = @"cmd.exe";
            process.StartInfo.Arguments = $"/C \"{command}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
        }

        private string GENERATE_COMMAND()
        {
            CREATE_NEW_FOLDER();

            if (File.Exists(filePath))
            {
                int targetSize = int.Parse(partSize);

                if (fileSize <= targetSize || targetSize == 0)
                {
                    if (password == null)
                    {
                        string command = $"@\"{sevenZPath}\" a -t{format} \"{newFolderPath}\\{fileName}.{format}\" \"{filePath}\"";
                        return command;
                    }

                    else
                    {
                        string command = $"@\"{sevenZPath}\" a -t{format} \"{newFolderPath}\\{fileName}.{format}\" \"{filePath}\" -p{password}";
                        return command;
                    }
                }

                else if (fileSize > targetSize)
                {
                    if (password == null)
                    {
                        string command = $"@\"{sevenZPath}\" a -t{format} \"{newFolderPath}\\{fileName}.{format}\" \"{filePath}\" -v{partSize}m";
                        return command;
                    }

                    else
                    {
                        string command = $"@\"{sevenZPath}\" a -t{format} \"{newFolderPath}\\{fileName}.{format}\" \"{filePath}\" -v{partSize}m -p{password}";
                        return command;
                    }
                }
            }

            else if(Directory.Exists(filePath))
            {
                int targetSize = int.Parse(partSize);

                if (folderSize <= targetSize || targetSize == 0)
                {
                    if (password == null)
                    {
                        string command = $"@\"{sevenZPath}\" a -t{format} \"{newFolderPath}\\{fileName}.{format}\" \"{filePath}\"";
                        return command;
                    }

                    else
                    {
                        string command = $"@\"{sevenZPath}\" a -t{format} \"{newFolderPath}\\{fileName}.{format}\" \"{filePath}\" -p{password}";
                        return command;
                    }
                }

                else if (folderSize > targetSize)
                {
                    if (password == null)
                    {
                        string command = $"@\"{sevenZPath}\" a -t{format} \"{newFolderPath}\\{fileName}.{format}\" \"{filePath}\" -v{partSize}m";
                        return command;
                    }

                    else
                    {
                        string command = $"@\"{sevenZPath}\" a -t{format} \"{newFolderPath}\\{fileName}.{format}\" \"{filePath}\" -v{partSize}m -p{password}";
                        return command;
                    }
                }
            }

            return null;
        }

        private void CREATE_NEW_FOLDER()
        {
            newFolderPath = $"{directoryPath}\\{fileName}压缩文件";

            if (!Directory.Exists(newFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(newFolderPath);
                }

                catch (Exception)
                {
                    switch (currentLanguage)
                    {
                        case "zh-CN":
                            MessageBox.Show("创建目标文件夹时出错。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "zh-TW":
                            MessageBox.Show("創建目標文件夾時出錯。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "en-US":
                            MessageBox.Show("The error occurred while creating the target folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }

                    this.Close();
                    return;
                }
            }
        }

        private long GET_FILE_SIZE(string filePath)
        {
            if (filePath != null)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;

                return fileSize / (1024 * 1024);
            }

            else
            {
                return -1;
            }
        }

        private long GET_FOLDER_SIZE(string filePath)
        {
            if (filePath != null)
            {
                long folderSize = 0;

                // 获取文件夹中的所有文件和子文件夹
                DirectoryInfo directoryInfo = new DirectoryInfo(filePath);

                // 计算文件大小
                FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
                foreach (FileInfo file in files)
                {
                    folderSize += file.Length;
                }

                return folderSize / (1024 * 1024);
            }

            else
            {
                return -1;
            }
        }

        static string GET_WORK_PATH()
        {
            // 获取当前执行程序集
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            // 返回路径
            return Path.GetDirectoryName(assemblyPath);
        }

        private bool CHECK_SEVENZ_EXIST()
        {
            return File.Exists(sevenZPath) && File.Exists(sevenZDllPath);
        }

        private void CREATE_DEFAULT_CONFIG(string configFilePath)
        {
            int newLocationX = Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2;
            int newLocationY = Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2;

            // 获取当前系统的显示语言
            var currentCulture = CultureInfo.CurrentUICulture;

            // 创建一个示例词典，包含支持的语言
            var supportedLanguages = new HashSet<string>
            {
                "zh-CN", // 中文 (简体)
                "zh-TW", // 中文 (繁体)
                "en-US", // 英语 (美国)
                // 其他语言...
            };

            // 检查当前语言是否在词典中
            if (supportedLanguages.Contains(currentCulture.Name))
            {
                // 创建默认的 XML 结构
                XElement defaultConfig = new XElement("Configuration",
                    new XElement("Language", $"{currentCulture.Name}"),
                    new XElement("LocationX", $"{newLocationX}"),
                    new XElement("LocationY", $"{newLocationY}"),
                    new XElement("PartSize", "2000"),
                    new XElement("Format", "7z"),
                    new XElement("Password", ""),
                    new XElement("AutoSave", "true")
                );

                defaultConfig.Save(configFilePath);
            }

            else
            {
                // 创建默认的 XML 结构
                XElement defaultConfig = new XElement("Configuration",
                    new XElement("Language", "en-US"),
                    new XElement("LocationX", $"{newLocationX}"),
                    new XElement("LocationY", $"{newLocationY}"),
                    new XElement("PartSize", "2000"),
                    new XElement("Format", "7z"),
                    new XElement("Password", ""),
                    new XElement("AutoSave", "true")
                );

                defaultConfig.Save(configFilePath);
            }
        }

        private void UPDATE_CONFIG(string filePath, string key, string newValue)
        {
            XDocument xdoc = XDocument.Load(filePath); // 加载 XML 文件

            var element = xdoc.Descendants(key).FirstOrDefault(); // 查找指定节点
            if (element != null)
            {
                element.Value = newValue; // 修改节点值
            }
            else
            {
                // 创建新节点并设置值
                xdoc.Root.Add(new XElement(key, newValue));
            }

            xdoc.Save(filePath); // 保存文件
        }

        private string GET_CURRENT_LANGUAGE(string configFilePath)
        {
            // 检查文件是否存在
            if (!File.Exists(configFilePath))
            {
                // 如果不存在，创建默认配置文件
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            // 加载 XML 文档
            XDocument xdoc = XDocument.Load(configFilePath);

            // 检查 Language 节点是否存在
            var languageNode = xdoc.Descendants("Language").FirstOrDefault();

            if (languageNode == null)
            {
                // 获取当前系统的显示语言
                var currentCulture = CultureInfo.CurrentUICulture;

                // 创建一个示例词典，包含支持的语言
                var supportedLanguages = new HashSet<string>
                {
                    "zh-CN", // 中文 (简体)
                    "zh-TW", // 中文 (繁体)
                    "en-US", // 英语 (美国)
                    // 其他语言...
                };

                // 检查当前语言是否在词典中
                if (supportedLanguages.Contains(currentCulture.Name))
                {
                    // 如果没有找到 Language 节点，创建新的 XML 节点
                    XElement newNode = new XElement("Language", $"{currentCulture.Name}");

                    // 将新节点添加到根节点
                    xdoc.Root.Add(newNode);

                    // 保存更改
                    xdoc.Save(configFilePath);

                    return currentCulture.Name;
                }

                else
                {
                    XElement newNode = new XElement("Language", "en-US");

                    xdoc.Root.Add(newNode);

                    xdoc.Save(configFilePath);

                    return "en-US";
                }
            }

            // 获取 Language 节点的值
            var language = languageNode.Value;

            // 如果获取到的值为空字符串
            if (string.IsNullOrEmpty(language))
            {
                // 获取当前系统的显示语言
                var currentCulture = CultureInfo.CurrentUICulture;

                // 创建一个示例词典，包含支持的语言
                var supportedLanguages = new HashSet<string>
                {
                    "zh-CN", // 中文 (简体)
                    "zh-TW", // 中文 (繁体)
                    "en-US", // 英语 (美国)
                    // 其他语言...
                };

                // 检查当前语言是否在词典中
                if (supportedLanguages.Contains(currentCulture.Name))
                {
                    return currentCulture.Name;
                }

                else
                {
                    return "en-US";
                }
            }

            // 返回获取到的值
            return language;
        }

        private string GET_LOCATION_X(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc = XDocument.Load(configFilePath);

            var locationXNode = xdoc.Descendants("LocationX").FirstOrDefault();

            if (locationXNode == null)
            {
                int newLocationX = Screen.PrimaryScreen.Bounds.Width / 2 - this.Size.Width / 2;

                XElement newNode = new XElement("LocationX", newLocationX);

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return newLocationX.ToString();
            }

            var locationX = locationXNode.Value;

            if (string.IsNullOrEmpty(locationX))
            {
                int newLocationX = Screen.PrimaryScreen.Bounds.Width / 2 - this.Size.Width / 2;

                return newLocationX.ToString();
            }

            return locationX;
        }

        private string GET_LOCATION_Y(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc = XDocument.Load(configFilePath);

            var locationYNode = xdoc.Descendants("LocationY").FirstOrDefault();

            if (locationYNode == null)
            {
                int newLocationY = Screen.PrimaryScreen.Bounds.Height / 2 - this.Size.Height / 2;

                XElement newNode = new XElement("LocationY", newLocationY);

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return newLocationY.ToString();
            }

            var locationY = locationYNode.Value;

            if (string.IsNullOrEmpty(locationY))
            {
                int newLocationY = Screen.PrimaryScreen.Bounds.Height / 2 - this.Size.Height / 2;

                return newLocationY.ToString();
            }

            return locationY;
        }

        private string GET_PARTSIZE(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc = XDocument.Load(configFilePath);

            var partSizeNode = xdoc.Descendants("PartSize").FirstOrDefault();

            if (partSizeNode == null)
            {
                XElement newNode = new XElement("PartSize", "2000");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return "2000";
            }

            var partSize = partSizeNode.Value;

            if (string.IsNullOrEmpty(partSize))
            {
                return "2000";
            }

            return partSize;
        }

        private string GET_FORMAT(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc = XDocument.Load(configFilePath);

            var formatNode = xdoc.Descendants("Format").FirstOrDefault();

            if (formatNode == null)
            {
                XElement newNode = new XElement("Format", "7z");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return "7z";
            }

            var format = formatNode.Value;

            if (string.IsNullOrEmpty(format))
            {
                return "7z";
            }

            return format;
        }

        private string GET_PASSWORD(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc = XDocument.Load(configFilePath);

            var passwordNode = xdoc.Descendants("Password").FirstOrDefault();

            if (passwordNode == null)
            {
                XElement newNode = new XElement("Password", "");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return null;
            }

            var password = passwordNode.Value;

            if (string.IsNullOrEmpty(password))
            {
                return null;
            }

            return password;
        }

        private string GET_AUTOSAVE(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc = XDocument.Load(configFilePath);

            var autoSaveNode = xdoc.Descendants("AutoSave").FirstOrDefault();

            if (autoSaveNode == null)
            {
                XElement newNode = new XElement("AutoSave", "true");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return "true";
            }

            var autoSave = autoSaveNode.Value;

            if (string.IsNullOrEmpty(autoSave))
            {
                return "true";
            }

            return autoSave;
        }

        private void DEFAULT_PARTSIZE_TEXTBOX()
        {
            int sizeNumber= int.Parse(partSize);
            TextBoxSize.Text = sizeNumber.ToString();
        }

        private void DEFAULT_FORMAT_MENU()
        {
            ComboBoxFormat.Items.Add("7z");
            ComboBoxFormat.Items.Add("zip");

            if (format == "7z")
            {
                ComboBoxFormat.SelectedIndex = 0;
            }

            if (format == "zip")
            {
                ComboBoxFormat.SelectedIndex = 1;
            }
        }

        private void DEFAULT_PASSWORD_TEXTBOX()
        {
            TextBoxPassword.Text = password;
        }

        private void DEFAULT_AUTOSAVE()
        {
            // 定义后台运行的默认显示状态
            if (autoSave == "false")
            {
                CheckBoxAutoSave.Checked = false;
            }

            if (autoSave == "true")
            {
                CheckBoxAutoSave.Checked = true;
            }
        }

        private void SAVE_CONFIG()
        {
            UPDATE_CONFIG($"{xmlPath}", "Language", $"{currentLanguage}");
            UPDATE_CONFIG($"{xmlPath}", "PartSize", $"{partSize}");
            UPDATE_CONFIG($"{xmlPath}", "Format", $"{format}");
            UPDATE_CONFIG($"{xmlPath}", "Password", $"{password}");
            UPDATE_CONFIG($"{xmlPath}", "AutoSave", $"{autoSave}");
        }

        private void BUTTON_CONFIG_CLICK(object sender, EventArgs e)
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show("配置已保存。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case "zh-TW":
                    MessageBox.Show("配置已保存。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case "en-US":
                    MessageBox.Show("Configuration Saved.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }

            SAVE_CONFIG();
            SAVE_LOCATION();
        }

        private void SET_COMPONENT_POSITION()
        {
            if (currentLanguage == "en-US")
            {
                LabelFormat.Location = new Point(MainPanel.Width / 2 - LabelFormat.Width / 2 - ComboBoxFormat.Width / 2 - 10 - 20, MainPanel.Height / 2 - LabelFormat.Height / 2);
                ComboBoxFormat.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 + 10 - 20, MainPanel.Height / 2 - ComboBoxFormat.Height / 2 - 1);

                LabelSize.Location = new Point(MainPanel.Width / 2 - LabelFormat.Width / 2 - ComboBoxFormat.Width / 2 - 10 - 20, MainPanel.Height / 2 - LabelFormat.Height / 2 - this.Height / 6);
                TextBoxSize.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 + 10 - 20, MainPanel.Height / 2 - ComboBoxFormat.Height / 2 - this.Height / 6);
                LabelUnit.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 + 15 - 20 + TextBoxSize.Width, MainPanel.Height / 2 - ComboBoxFormat.Height / 2 - this.Height / 6 + 3);

                LabelPassword.Location = new Point(MainPanel.Width / 2 - LabelFormat.Width / 2 - ComboBoxFormat.Width / 2 - 10 - 20, MainPanel.Height / 2 - LabelFormat.Height / 2 + this.Height / 6);
                TextBoxPassword.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 + 10 - 20, MainPanel.Height / 2 - ComboBoxFormat.Height / 2 + this.Height / 6);
            }

            else
            {
                LabelFormat.Location = new Point(MainPanel.Width / 2 - LabelFormat.Width / 2 - ComboBoxFormat.Width / 2 - 10 - 30, MainPanel.Height / 2 - LabelFormat.Height / 2);
                ComboBoxFormat.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 + 10 - 30, MainPanel.Height / 2 - ComboBoxFormat.Height / 2 - 1);

                LabelSize.Location = new Point(MainPanel.Width / 2 - LabelFormat.Width / 2 - ComboBoxFormat.Width / 2 - 10 - 30, MainPanel.Height / 2 - LabelFormat.Height / 2 - this.Height / 6);
                TextBoxSize.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 + 10 - 30, MainPanel.Height / 2 - ComboBoxFormat.Height / 2 - this.Height / 6);
                LabelUnit.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 + 15 - 30 + TextBoxSize.Width, MainPanel.Height / 2 - ComboBoxFormat.Height / 2 - this.Height / 6 + 3);

                LabelPassword.Location = new Point(MainPanel.Width / 2 - LabelFormat.Width / 2 - ComboBoxFormat.Width / 2 - 10 - 30, MainPanel.Height / 2 - LabelFormat.Height / 2 + this.Height / 6);
                TextBoxPassword.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 + 10 - 30, MainPanel.Height / 2 - ComboBoxFormat.Height / 2 + this.Height / 6);
            }

            CheckBoxAutoSave.Location = new Point(25, MainPanel.Height - CheckBoxAutoSave.Height - 20);
            ButtonConfig.Location = new Point(MainPanel.Width - ButtonConfig.Width - 10, MainPanel.Height - ButtonConfig.Height - 10);
        }

        private void CHECKBOX_AUTOSAVE_CHECKED_CHANGED(object sender, EventArgs e)
        {
            bool isAutoSave = CheckBoxAutoSave.Checked;
            if (isAutoSave)
            {
                autoSave = "true";
            }

            else
            {
                autoSave = "false";
            }
        }

        private void LANGUAGE_MENU_SELECT_zhCN_CLICK(object sender, EventArgs e)
        {
            currentLanguage = "zh-CN";
            UpdateLanguage();
            UPDATE_CONFIG($"{xmlPath}", "Language", $"{currentLanguage}");
        }

        private void LANGUAGE_MENU_SELECT_zhTW_CLICK(object sender, EventArgs e)
        {
            currentLanguage = "zh-TW";
            UpdateLanguage();
            UPDATE_CONFIG($"{xmlPath}", "Language", $"{currentLanguage}");
        }

        private void LANGUAGE_MENU_SELECT_enUS_CLICK(object sender, EventArgs e)
        {
            currentLanguage = "en-US";
            UpdateLanguage();
            UPDATE_CONFIG($"{xmlPath}", "Language", $"{currentLanguage}");
        }

        private void MAINFORM_LOAD(object sender, EventArgs e)
        {
            locationX = int.Parse(GET_LOCATION_X(xmlPath));
            locationY = int.Parse(GET_LOCATION_Y(xmlPath));

            this.Location = new Point(locationX, locationY);
        }

        private void COMBOBOX_FORMAT_SELECTED_INDEX_CHANGNED(object sender, EventArgs e)
        {
            string selectedFormat = ComboBoxFormat.SelectedItem.ToString();
            format = selectedFormat;
        }

        private void TEXTBOX_SIZE_TEXT_CHANGED(object sender, EventArgs e)
        {
            string newSize = TextBoxSize.Text;
            partSize = newSize;
        }

        private void TEXTBOX_PASSWORD_TEXT_CHANGED(object sender, EventArgs e)
        {
            string newPassword = TextBoxPassword.Text;
            password = newPassword;
        }

        private void TEXTBOX_SIZE_KEYPRESS(object sender, KeyPressEventArgs e)
        {
            // 检查输入的字符是否是数字或控制字符（如退格键）
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // 如果不是，则拦截该字符
            }
        }

        private void ABOUTMENU_ABOUT(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

        private void MAINFORM_DRAGENTER(object sender, DragEventArgs e)
        {
            // 检测数据是否可以处理
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy; // 设置拖拽效果为复制
            }
            else
            {
                e.Effect = DragDropEffects.None; // 不支持的拖拽效果
            }
        }

        private async void MAINFORM_DRAGDROP(object sender, DragEventArgs e)
        {
            // 处理拖拽的数据
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null && files.Length > 0)
            {
                filePath = files[0];

                if (File.Exists(filePath))
                {
                    fileSize = GET_FILE_SIZE(filePath);

                    if (fileSize == -1)
                    {
                        switch (currentLanguage)
                        {
                            case "zh-CN":
                                MessageBox.Show("传入路径为空路径。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            case "zh-TW":
                                MessageBox.Show("傳入路徑為空路徑。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            case "en-US":
                                MessageBox.Show("The input path is an empty path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                        }

                        return;
                    }
                }

                else if (Directory.Exists(filePath))
                {
                    folderSize = GET_FOLDER_SIZE(filePath);

                    if (folderSize == -1)
                    {
                        switch (currentLanguage)
                        {
                            case "zh-CN":
                                MessageBox.Show("传入路径为空路径。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            case "zh-TW":
                                MessageBox.Show("傳入路徑為空路徑。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            case "en-US":
                                MessageBox.Show("The input path is an empty path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                        }

                        return;
                    }
                }

                fileName = Path.GetFileNameWithoutExtension(filePath);
                directoryPath = Path.GetDirectoryName(filePath);

                if (!CHECK_SEVENZ_EXIST())
                {
                    switch (currentLanguage)
                    {
                        case "zh-CN":
                            MessageBox.Show("程序工作路径下7z组件(7z.exe 7z.dll Lang)不完整，无法启动7z处理流程。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "zh-TW":
                            MessageBox.Show("程式工作路徑下7z組件(7z.exe 7z.dll Lang)不完整，無法啓動7z處理流程。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "en-US":
                            MessageBox.Show("The 7z components (7z.exe 7z.dll Lang) in the program's working directory are incomplete and can not start the 7z processing flow.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }
                }

                else
                {
                    string command = GENERATE_COMMAND();
                    await Task.Run(() => EXECUTE_COMMAND(command));
                }
            }

            // 恢复鼠标指针状态
            Cursor.Current = Cursors.Default; // 设置鼠标指针为默认状态
        }

        // 鼠标移动离开时恢复指针状态
        private void MAINFORM_DRAGLEAVE(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.Default; // 设置鼠标指针为默认状态
        }


        private void MAINFORM_RESIZE(object sender, EventArgs e)
        {
            SET_COMPONENT_POSITION();
        }

        private void MAINFORM_FORM_CLOSING(object sender, FormClosingEventArgs e)
        {
            if (autoSave == "false")
            {
                SAVE_LOCATION();
            }

            if (autoSave == "true")
            {
                SAVE_CONFIG();
                SAVE_LOCATION();
            }
        }

        private void SAVE_LOCATION()
        {
            locationX = this.Location.X;
            locationY = this.Location.Y;

            UPDATE_CONFIG($"{xmlPath}", "LocationX", $"{locationX}");
            UPDATE_CONFIG($"{xmlPath}", "LocationY", $"{locationY}");
        }
    }
}
