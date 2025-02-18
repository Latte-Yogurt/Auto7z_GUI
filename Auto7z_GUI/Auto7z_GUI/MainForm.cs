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
using System.Xml;

namespace Auto7z_GUI
{
    public partial class MainForm : Form
    {
        private Dictionary<string, Dictionary<string, string>> languageTexts;

        public string xmlPath;
        public bool isMultipleFiles;
        public string filePath;
        public string fileName;
        public string directoryPath;
        public string sevenZPath;
        public string sevenZDllPath;
        public string langPath;
        public string sevenZXADllPath;
        public string defaultSFXPath;
        public string default64SFXPath;
        public string rarPath;
        public string rarRegKeyPath;
        public string winConSFXPath;
        public string winCon64SFXPath;
        public string winRarPath;
        public string zipSFXPath;
        public string zip64SFXPath;
        public static string currentLanguage;
        public static float systemScale;
        public int oldScreenWidth;
        public int oldScreenHeight;
        public float oldSystemScale;
        public int locationX;
        public int locationY;
        public string partSize;
        public string format;
        public string password;
        public bool zstd;
        public bool disableTarSplit;
        public bool autoSave;
        public long sevenZUsageCount;
        public long rarUsageCount;
        public long auto7zUsageCount;
        public long fileSize;
        public long folderSize;
        public string newFolderPath;

        public MainForm(string[] args)
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;

            GET_SYSTEM_SCALE();
            SET_BUTTON_CONFIG_SIZE();
            SET_CLIENT_SIZE();
            SET_MAINFORM_LOCATION();

            string workPath = GET_WORK_PATH(); // 获取程序路径
            string xml = @"config.xml";
            xmlPath = Path.Combine(workPath, xml);
            CHECK_XML_LEGAL(xmlPath);

            currentLanguage = GET_CURRENT_LANGUAGE(xmlPath);

            oldScreenWidth = GET_SCREEN_WIDTH(xmlPath);
            oldScreenHeight = GET_SCREEN_HEIGHT(xmlPath);
            oldSystemScale = GET_SYSTEM_SCALE(xmlPath);

            sevenZUsageCount = GET_SEVENZ_USAGE_COUNT(xmlPath);
            rarUsageCount = GET_RAR_USAGE_COUNT(xmlPath);

            InitializeLanguageTexts();
            UpdateLanguage();

            this.AllowDrop = true; // 允许拖放
            this.DragEnter += new DragEventHandler(MAINFORM_DRAGENTER);
            this.DragDrop += new DragEventHandler(MAINFORM_DRAGDROP);
            this.DragLeave += new EventHandler(MAINFORM_DRAGLEAVE);
            this.FormClosing += MAINFORM_FORM_CLOSING;

            if (args.Length > 0)
            {
                if (args.Length > 1)
                {
                    isMultipleFiles = true;
                }

                else
                {
                    isMultipleFiles = false;
                }
            }

            string sevenZ = @"7z\\7z.exe";
            sevenZPath = Path.Combine(workPath, sevenZ);

            string sevenZDll = @"7z\\7z.dll";
            sevenZDllPath = Path.Combine(workPath, sevenZDll);

            string Lang = @"7z\\Lang";
            langPath = Path.Combine(workPath, Lang);

            string sevenZXADll = @"rar\\7zxa.dll";
            sevenZXADllPath = Path.Combine(workPath, sevenZXADll);

            string defaultSFX = @"rar\\Default.SFX";
            defaultSFXPath = Path.Combine(workPath, defaultSFX);

            string default64SFX = @"rar\\Default64.SFX";
            default64SFXPath = Path.Combine(workPath, default64SFX);

            string rar = @"rar\\Rar.exe";
            rarPath = Path.Combine(workPath, rar);

            string rarRegKey = @"rar\\rarreg.key";
            rarRegKeyPath = Path.Combine(workPath, rarRegKey);

            string winConSFX = @"rar\\WinCon.SFX";
            winConSFXPath = Path.Combine(workPath, winConSFX);

            string winCon64SFX = @"rar\\WinCon64.SFX";
            winCon64SFXPath = Path.Combine(workPath, winCon64SFX);

            string winRar = @"rar\\WinRAR.exe";
            winRarPath = Path.Combine(workPath, winRar);

            string zipSFX = @"rar\\Zip.SFX";
            zipSFXPath = Path.Combine(workPath, zipSFX);

            string zip64SFX = @"rar\\Zip64.SFX";
            zip64SFXPath = Path.Combine(workPath, zip64SFX);

            partSize = GET_PARTSIZE(xmlPath);
            format = GET_FORMAT(xmlPath);
            password = GET_PASSWORD(xmlPath);
            disableTarSplit = GET_DISABLE_TAR_SPLIT(xmlPath);
            autoSave = GET_AUTOSAVE(xmlPath);
            zstd = GET_ZSTD(xmlPath);

            DEFAULT_PARTSIZE_TEXTBOX();
            DEFAULT_FORMAT_MENU();
            DEFAULT_PASSWORD_TEXTBOX();
            DEFAULT_DISABLE_TAR_SPLIT();
            DEFAULT_AUTOSAVE();
            DEFAULT_ZSTD();

            if (args != null && args.Length > 0 && args.All(arg => !string.IsNullOrEmpty(arg)))
            {
                if (!isMultipleFiles)
                {
                    filePath = args[0];
                    fileName = Path.GetFileNameWithoutExtension(filePath);
                    directoryPath = Path.GetDirectoryName(filePath);

                    bool isLagelSize = GET_SIZE(filePath);

                    if (isLagelSize)
                    {
                        if (format == "7z" || format == "zip")
                        {
                            if (!CHECK_SEVENZ_EXIST())
                            {
                                ERROR_SEVENZ_EXIST();
                                this.Close();
                                return;
                            }

                            if (!CHECK_LANG_EXIST() && sevenZUsageCount < 1)
                            {
                                WARNING_LANG_EXIST();
                            }

                            string command = GENERATE_COMMAND();

                            if (command == null)
                            {
                                ERROR_EMPTY_PATH();
                                this.Close();
                                return;
                            }

                            DELETE_OLD_FILES();

                            bool finished = EXECUTE_COMMAND_BOOL(command);

                            if (!finished)
                            {
                                DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                            }

                            ADD_SEVENZ_USAGE_COUNT();
                            ADD_AUTO7Z_USAGE_COUNT();
                            this.Close();
                        }

                        if (format == "tar")
                        {
                            if (!CHECK_SEVENZ_EXIST())
                            {
                                ERROR_SEVENZ_EXIST();
                                this.Close();
                                return;
                            }

                            if (!CHECK_LANG_EXIST() && sevenZUsageCount < 1)
                            {
                                WARNING_LANG_EXIST();
                            }

                            string command = GENERATE_COMMAND();

                            if (command == null)
                            {
                                ERROR_EMPTY_PATH();
                                this.Close();
                                return;
                            }

                            DELETE_OLD_FILES();

                            bool tarFinished = EXECUTE_COMMAND_BOOL(command);

                            if (!tarFinished)
                            {
                                DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                            }

                            if (CheckBoxZstd.Checked)
                            {
                                while (tarFinished)
                                {
                                    string zstdCommand = ZSTD_COMMAND();

                                    DELETE_OLD_FILES();

                                    bool zstdFinished = EXECUTE_COMMAND_BOOL(zstdCommand);

                                    if (!zstdFinished)
                                    {
                                        DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                                    }

                                    while (zstdFinished)
                                    {
                                        DELETE_TEMP_TAR(newFolderPath);
                                        break;
                                    }

                                    break;
                                }
                            }

                            ADD_SEVENZ_USAGE_COUNT();
                            ADD_AUTO7Z_USAGE_COUNT();
                            this.Close();
                        }

                        if (format == "rar")
                        {
                            if (!CHECK_RAR_EXIST())
                            {
                                ERROR_RAR_EXIST();
                                this.Close();
                                return;
                            }

                            string command = GENERATE_COMMAND();

                            if (command == null)
                            {
                                ERROR_EMPTY_PATH();
                                this.Close();
                                return;
                            }

                            DELETE_OLD_FILES();

                            bool finished = EXECUTE_COMMAND_BOOL(command);

                            if (!finished)
                            {
                                DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                            }

                            ADD_RAR_USAGE_COUNT();
                            ADD_AUTO7Z_USAGE_COUNT();
                            this.Close();
                        }
                    }

                    else
                    {
                        ERROR_ILLAGEL_SIZE();
                        this.Close();
                    }
                }

                else
                {
                    foreach (var simpleFilePath in args)
                    {
                        filePath = simpleFilePath;
                        fileName = Path.GetFileNameWithoutExtension(filePath);
                        directoryPath = Path.GetDirectoryName(filePath);

                        bool isLagelSize = GET_SIZE(filePath);

                        if (isLagelSize)
                        {
                            if (format == "7z" || format == "zip")
                            {
                                if (!CHECK_SEVENZ_EXIST())
                                {
                                    ERROR_SEVENZ_EXIST();
                                }

                                if (!CHECK_LANG_EXIST() && sevenZUsageCount < 1)
                                {
                                    WARNING_LANG_EXIST();
                                }

                                string command = GENERATE_COMMAND();

                                if (command == null)
                                {
                                    ERROR_EMPTY_PATH();
                                }

                                DELETE_OLD_FILES();

                                bool finished = EXECUTE_COMMAND_BOOL(command);

                                if (!finished)
                                {
                                    DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                                }

                                ADD_SEVENZ_USAGE_COUNT();
                                ADD_AUTO7Z_USAGE_COUNT();
                            }

                            if (format == "tar")
                            {
                                if (!CHECK_SEVENZ_EXIST())
                                {
                                    ERROR_SEVENZ_EXIST();
                                }

                                if (!CHECK_LANG_EXIST() && sevenZUsageCount < 1)
                                {
                                    WARNING_LANG_EXIST();
                                }

                                string command = GENERATE_COMMAND();

                                if (command == null)
                                {
                                    ERROR_EMPTY_PATH();
                                }

                                DELETE_OLD_FILES();

                                bool tarFinished = EXECUTE_COMMAND_BOOL(command);

                                if (!tarFinished)
                                {
                                    DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                                }

                                if (CheckBoxZstd.Checked)
                                {
                                    while (tarFinished)
                                    {
                                        string zstdCommand = ZSTD_COMMAND();

                                        DELETE_OLD_FILES();

                                        bool zstdFinished = EXECUTE_COMMAND_BOOL(zstdCommand);

                                        if (!zstdFinished)
                                        {
                                            DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                                        }

                                        while (zstdFinished)
                                        {
                                            DELETE_TEMP_TAR(newFolderPath);
                                            break;
                                        }

                                        break;
                                    }
                                }

                                ADD_SEVENZ_USAGE_COUNT();
                                ADD_AUTO7Z_USAGE_COUNT();
                            }

                            if (format == "rar")
                            {
                                if (!CHECK_RAR_EXIST())
                                {
                                    ERROR_RAR_EXIST();
                                }

                                string command = GENERATE_COMMAND();

                                if (command == null)
                                {
                                    ERROR_EMPTY_PATH();
                                }

                                DELETE_OLD_FILES();

                                bool finished = EXECUTE_COMMAND_BOOL(command);

                                if (!finished)
                                {
                                    DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                                }

                                ADD_RAR_USAGE_COUNT();
                                ADD_AUTO7Z_USAGE_COUNT();
                            }
                        }
                    }

                    this.Close();
                }
            }
        }

        static string GET_WORK_PATH()
        {
            // 获取当前执行程序集
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            // 返回路径
            return Path.GetDirectoryName(assemblyPath);
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
                        { "CheckBoxDisableSplit","禁用分卷"},
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
                        { "CheckBoxDisableSplit","禁用分卷"},
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
                        { "CheckBoxDisableSplit","Disable split"},
                        { "CheckBoxAutoSave","save config while exit" },
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
            CheckBoxDisableSplit.Text = languageTexts[currentLanguage]["CheckBoxDisableSplit"];
            CheckBoxAutoSave.Text = languageTexts[currentLanguage]["CheckBoxAutoSave"];
            ButtonConfig.Text = languageTexts[currentLanguage]["ButtonConfig"];

            SET_COMPONENT_POSITION();
        }

        private bool EXECUTE_COMMAND_BOOL(string command)
        {
            var process = new Process();
            process.StartInfo.FileName = @"cmd.exe";
            process.StartInfo.Arguments = $"/C \"{command}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            try
            {
                process.Start();
                process.WaitForExit(); // 等待进程完成
                return process.ExitCode == 0; // 如果ExitCode为0，则返回true（表示成功）
            }

            catch (Exception ex)
            {
                ERROR_EXCEPTION_MESSAGE(ex);
                return false; // 发生异常时返回false
            }

            finally
            {
                process.Dispose(); // 清理资源
            }
        }

        private string GENERATE_COMMAND()
        {
            bool createSuccess = CREATE_NEW_FOLDER();

            if (!File.Exists(filePath) && !Directory.Exists(filePath))
            {
                return null; // 如果文件或目录不存在，返回 null
            }

            if (!createSuccess)
            {
                return null;
            }

            int targetSize = int.Parse(partSize);
            bool isFile = File.Exists(filePath);
            long size = isFile ? fileSize : folderSize; // 确定使用文件大小还是文件夹大小

            if (format == "7z" || format == "zip")
            {
                string command = $"@\"{sevenZPath}\" a -t{format} \"{newFolderPath}\\{fileName}.{format}\" \"{filePath}\"";

                // 添加分卷选项
                if (size > targetSize && targetSize > 0)
                {
                    command += $" -v{partSize}m";
                }

                // 添加密码选项
                if (!string.IsNullOrEmpty(password))
                {
                    command += $" -p{password}";
                }

                return command;
            }

            if (format == "rar")
            {
                string command = $"@\"{winRarPath}\" a -ep1 \"{newFolderPath}\\{fileName}.{format}\" \"{filePath}\"";

                if (size > targetSize && targetSize > 0)
                {
                    command += $" -v{partSize}m";
                }

                if (!string.IsNullOrEmpty(password))
                {
                    command += $" -p{password}";
                }

                return command;
            }

            if (format == "tar")
            {
                string command = $"@\"{sevenZPath}\" a -t{format} \"{newFolderPath}\\{fileName}.{format}\" \"{filePath}\"";

                if (size > targetSize && targetSize > 0 && !CheckBoxZstd.Checked)
                {
                    command += $" -v{partSize}m";
                }

                return command;
            }

            return null;
        }

        private bool CREATE_NEW_FOLDER()
        {
            newFolderPath = $"{directoryPath}\\{fileName}压缩文件";

            if (!Directory.Exists(newFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(newFolderPath);
                    return true;
                }

                catch (Exception ex)
                {
                    ERROR_CREATE_FOLDER_FAILED(ex);
                    return false;
                }
            }

            else
            {
                return true;
            }
        }

        private string ZSTD_COMMAND()
        {
            if (!File.Exists($"{newFolderPath}\\{fileName}.tar") || !Directory.Exists(newFolderPath))
            {
                return null; // 如果文件或目录不存在，返回 null
            }

            int targetSize = int.Parse(partSize);
            bool isFile = File.Exists(filePath);
            long size = isFile ? fileSize : folderSize; // 确定使用文件大小还是文件夹大小

            string command = $"@\"{sevenZPath}\" a -tzstd \"{newFolderPath}\\{fileName}.tar.zst\" \"{newFolderPath}\\{fileName}.tar\"";

            // 添加分卷选项
            if (size > targetSize && targetSize > 0 && !CheckBoxDisableSplit.Checked)
            {
                command += $" -v{partSize}m";
            }

            return command;
        }

        private void DELETE_OLD_FILES()
        {
            if (Directory.Exists($"{newFolderPath}") && format == "7z")
            {
                DELETE_OLD_SEVENZ(newFolderPath);
            }

            if (Directory.Exists($"{newFolderPath}") && format == "zip")
            {
                DELETE_OLD_ZIP(newFolderPath);
            }

            if (Directory.Exists($"{newFolderPath}") && format == "rar")
            {
                DELETE_OLD_RAR(newFolderPath);
            }

            if (Directory.Exists($"{newFolderPath}") && format == "tar" && !CheckBoxZstd.Checked)
            {
                DELETE_OLD_TAR(newFolderPath);
            }

            if (Directory.Exists($"{newFolderPath}") && format == "tar" && CheckBoxZstd.Checked)
            {
                DELETE_OLD_TAR_ZST(newFolderPath);
            }
        }

        private void DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED()
        {
            if (Directory.Exists($"{newFolderPath}"))
            {
                DELETE_DIRECTOR_CONTENTS(newFolderPath);
                Directory.Delete(newFolderPath);
            }
        }

        private void DELETE_OLD_SEVENZ(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                string[] files = Directory.GetFiles(dirPath);

                foreach (string file in files)
                {
                    if (Path.GetFileName(file).Contains("7z"))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            ERROR_EXCEPTION_MESSAGE(ex);
                        }
                    }
                }
            }
        }

        private void DELETE_OLD_ZIP(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                string[] files = Directory.GetFiles(dirPath);

                foreach (string file in files)
                {
                    if (Path.GetFileName(file).Contains("zip"))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            ERROR_EXCEPTION_MESSAGE(ex);
                        }
                    }
                }
            }
        }

        private void DELETE_OLD_RAR(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                string[] files = Directory.GetFiles(dirPath);

                foreach (string file in files)
                {
                    if (Path.GetFileName(file).Contains("rar"))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            ERROR_EXCEPTION_MESSAGE(ex);
                        }
                    }
                }
            }
        }

        private void DELETE_OLD_TAR(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                string[] files = Directory.GetFiles(dirPath);

                foreach (string file in files)
                {
                    if (Path.GetFileName(file).Contains("tar"))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            ERROR_EXCEPTION_MESSAGE(ex);
                        }
                    }
                }
            }
        }

        private void DELETE_OLD_TAR_ZST(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                string[] files = Directory.GetFiles(dirPath);

                foreach (string file in files)
                {
                    if (Path.GetFileName(file).Contains("tar.zst"))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            ERROR_EXCEPTION_MESSAGE(ex);
                        }
                    }
                }
            }
        }

        private void DELETE_TEMP_TAR(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                string file = $"{dirPath}\\{fileName}.tar";

                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        ERROR_EXCEPTION_MESSAGE(ex);
                    }
                }
            }
        }

        private void DELETE_DIRECTOR_CONTENTS(string dirPath)
        {
            foreach (string file in Directory.GetFiles(dirPath))
            {
                File.Delete(file); // 删除文件
            }

            foreach (string subDir in Directory.GetDirectories(dirPath))
            {
                DELETE_DIRECTOR_CONTENTS(subDir); // 递归调用删除子目录
                Directory.Delete(subDir); // 删除子目录
            }
        }

        private void GET_SYSTEM_SCALE()
        {
            float dpi;
            using (Graphics g = this.CreateGraphics())
            {
                dpi = g.DpiX;
            }

            systemScale = dpi / 96.0f;
        }

        private bool GET_SIZE(string filePath)
        {
            if (File.Exists(filePath))
            {
                fileSize = GET_FILE_SIZE(filePath);

                if (fileSize == -1)
                {
                    ERROR_EMPTY_PATH();
                    return false;
                }

                return true;
            }

            else if (Directory.Exists(filePath))
            {
                folderSize = GET_FOLDER_SIZE(filePath);

                if (folderSize == -1)
                {
                    ERROR_EMPTY_PATH();
                    return false;
                }

                return true;
            }

            return false;
        }

        private long GET_FILE_SIZE(string filePath)
        {
            if (filePath != null)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;

                long fileSizeMiB = fileSize / (1024 * 1024);

                return fileSizeMiB;
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

                long folderSizeMiB = folderSize / (1024 * 1024);

                return folderSizeMiB;
            }

            else
            {
                return -1;
            }
        }

        private void CHECK_XML_LEGAL(string configFilePath)
        {
            FileInfo configFile = new FileInfo(configFilePath);

            if (configFile.Exists && (configFile.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                configFile.Attributes = FileAttributes.Normal;
            }

            if (configFile.Exists && (configFile.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                configFile.Attributes = FileAttributes.Normal;
            }

            if (!configFile.Exists)
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }
        }

        private bool CHECK_SEVENZ_EXIST()
        {
            return File.Exists(sevenZPath) && File.Exists(sevenZDllPath);
        }

        private bool CHECK_LANG_EXIST()
        {
            return Directory.Exists(langPath);
        }

        private bool CHECK_RAR_EXIST()
        {
            return File.Exists(sevenZXADllPath) && File.Exists(defaultSFXPath) && File.Exists(default64SFXPath)
                && File.Exists(rarPath) && File.Exists(rarRegKeyPath) && File.Exists(winConSFXPath)
                && File.Exists(winCon64SFXPath) && File.Exists(winRarPath) && File.Exists(zipSFXPath) && File.Exists(zip64SFXPath);
        }

        private void CREATE_DEFAULT_CONFIG(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                try
                {
                    File.Delete(configFilePath);
                }

                catch (Exception ex)
                {
                    ERROR_EXCEPTION_MESSAGE(ex);
                    this.Close();
                }
            }

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

            if (supportedLanguages.Contains(currentCulture.Name))
            {
                if (currentLanguage != null)
                {
                    // 创建默认的 XML 结构
                    XElement defaultConfig = new XElement("Configuration",
                        new XElement("Language", $"{currentLanguage}"),
                        new XElement("ScreenWidth", "0"),
                        new XElement("ScreenHeight", "0"),
                        new XElement("SystemScale", "0"),
                        new XElement("LocationX", $"{newLocationX}"),
                        new XElement("LocationY", $"{newLocationY}"),
                        new XElement("PartSize", "2048"),
                        new XElement("Format", "7z"),
                        new XElement("Password", ""),
                        new XElement("Zstd", "True"),
                        new XElement("DisableTarSplit", "False"),
                        new XElement("AutoSave", "True"),
                        new XElement("SevenZUsageCount", "0"),
                        new XElement("RarUsageCount", "0"),
                        new XElement("Auto7zUsageCount", "0")
                    );

                    defaultConfig.Save(configFilePath);
                }

                else
                {
                    // 创建默认的 XML 结构
                    XElement defaultConfig = new XElement("Configuration",
                        new XElement("Language", $"{currentCulture.Name}"),
                        new XElement("ScreenWidth", "0"),
                        new XElement("ScreenHeight", "0"),
                        new XElement("SystemScale", "0"),
                        new XElement("LocationX", $"{newLocationX}"),
                        new XElement("LocationY", $"{newLocationY}"),
                        new XElement("PartSize", "2048"),
                        new XElement("Format", "7z"),
                        new XElement("Password", ""),
                        new XElement("Zstd", "True"),
                        new XElement("DisableTarSplit", "False"),
                        new XElement("AutoSave", "True"),
                        new XElement("SevenZUsageCount", "0"),
                        new XElement("RarUsageCount", "0"),
                        new XElement("Auto7zUsageCount", "0")
                    );

                    defaultConfig.Save(configFilePath);
                }
            }

            else
            {
                // 创建默认的 XML 结构
                XElement defaultConfig = new XElement("Configuration",
                    new XElement("Language", "en-US"),
                    new XElement("ScreenWidth", "0"),
                    new XElement("ScreenHeight", "0"),
                    new XElement("SystemScale", "0"),
                    new XElement("LocationX", $"{newLocationX}"),
                    new XElement("LocationY", $"{newLocationY}"),
                    new XElement("PartSize", "2048"),
                    new XElement("Format", "7z"),
                    new XElement("Password", ""),
                    new XElement("Zstd", "True"),
                    new XElement("DisableTarSplit", "False"),
                    new XElement("AutoSave", "True"),
                    new XElement("SevenZUsageCount", "0"),
                    new XElement("RarUsageCount", "0"),
                    new XElement("Auto7zUsageCount", "0")
                );

                defaultConfig.Save(configFilePath);
            }
        }

        private void UPDATE_CONFIG(string filePath, string key, string newValue)
        {
            try
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

            catch (Exception)
            {
                CREATE_DEFAULT_CONFIG(xmlPath);
            }
        }

        private string GET_CURRENT_LANGUAGE(string configFilePath)
        {
            // 检查文件是否存在
            if (!File.Exists(configFilePath))
            {
                // 如果不存在，创建默认配置文件
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

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

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                if (supportedLanguages.Contains(currentCulture.Name))
                {
                    return currentCulture.Name;
                }

                else
                {
                    return "en-US";
                }
            }

            // 检查 Language 节点是否存在
            var languageNode = xdoc.Descendants("Language").FirstOrDefault();

            if (languageNode == null)
            {
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

            // 检查语言是否在支持语言词典中
            if (!supportedLanguages.Contains(language))
            {
                // 如果在，返回当前的系统显示语言（如果在支持列表中）
                if (supportedLanguages.Contains(currentCulture.Name))
                {
                    return currentCulture.Name;
                }
                else
                {
                    return "en-US"; // 默认语言
                }
            }

            // 返回获取到的值
            return language;
        }

        private int GET_SCREEN_WIDTH(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            int newWidth = Screen.PrimaryScreen.Bounds.Width;

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return 0;
            }

            var widthNode = xdoc.Descendants("ScreenWidth").FirstOrDefault();

            if (widthNode == null)
            {

                XElement newNode = new XElement("ScreenWidth", newWidth);

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return 0;
            }

            var width = widthNode.Value;
            int widthToInt;

            if (string.IsNullOrEmpty(width))
            {
                return 0;
            }

            if (!int.TryParse(width, out widthToInt))
            {
                return 0;
            }

            if (widthToInt <= 0)
            {
                return 0;
            }

            if (widthToInt == Screen.PrimaryScreen.Bounds.Width)
            {
                return widthToInt;
            }

            return 0;
        }

        private int GET_SCREEN_HEIGHT(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            int newHeight = Screen.PrimaryScreen.Bounds.Height;

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return 0;
            }

            var heightNode = xdoc.Descendants("ScreenHeight").FirstOrDefault();

            if (heightNode == null)
            {

                XElement newNode = new XElement("ScreenHeight", newHeight);

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return 0;
            }

            var height = heightNode.Value;
            int heightToInt;

            if (string.IsNullOrEmpty(height))
            {
                return 0;
            }

            if (!int.TryParse(height, out heightToInt))
            {
                return 0;
            }

            if (heightToInt <= 0)
            {
                return 0;
            }

            if (heightToInt == Screen.PrimaryScreen.Bounds.Height)
            {
                return heightToInt;
            }

            return 0;
        }

        private float GET_SYSTEM_SCALE(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return 0;
            }

            var scaleNode = xdoc.Descendants("SystemScale").FirstOrDefault();

            if (scaleNode == null)
            {

                XElement newNode = new XElement("SystemScale", systemScale);

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return 0;
            }

            var scale = scaleNode.Value;
            float scaleToFloat;

            if (string.IsNullOrEmpty(scale))
            {
                return 0;
            }

            if (!float.TryParse(scale, out scaleToFloat))
            {
                return 0;
            }

            if (scaleToFloat <= 0)
            {
                return 0;
            }

            if (scaleToFloat == systemScale)
            {
                return scaleToFloat;
            }

            return 0;
        }

        private int GET_LOCATION_X(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            int newLocationX = Screen.PrimaryScreen.Bounds.Width / 2 - this.Size.Width / 2;

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return newLocationX;
            }

            var locationXNode = xdoc.Descendants("LocationX").FirstOrDefault();

            if (locationXNode == null)
            {

                XElement newNode = new XElement("LocationX", newLocationX);

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return newLocationX;
            }

            var locationX = locationXNode.Value;
            int locationXToInt;

            if (string.IsNullOrEmpty(locationX))
            {
                return newLocationX;
            }

            if (!int.TryParse(locationX, out locationXToInt))
            {
                return newLocationX;
            }

            if (locationXToInt > Screen.PrimaryScreen.Bounds.Width - this.Size.Width || locationXToInt < -10)
            {
                return newLocationX;
            }

            return locationXToInt;
        }

        private int GET_LOCATION_Y(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            int newLocationY = Screen.PrimaryScreen.Bounds.Height / 2 - this.Size.Height / 2;

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return newLocationY;
            }

            var locationYNode = xdoc.Descendants("LocationY").FirstOrDefault();

            if (locationYNode == null)
            {
                XElement newNode = new XElement("LocationY", newLocationY);

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return newLocationY;
            }

            var locationY = locationYNode.Value;
            int locationYToInt;

            if (string.IsNullOrEmpty(locationY))
            {
                return newLocationY;
            }

            if (!int.TryParse(locationY, out locationYToInt))
            {
                return newLocationY;
            }

            if (locationYToInt > Screen.PrimaryScreen.Bounds.Height - this.Size.Height || locationYToInt < 0)
            {
                return newLocationY;
            }

            return locationYToInt;
        }

        private string GET_PARTSIZE(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return "2048";
            }

            var partSizeNode = xdoc.Descendants("PartSize").FirstOrDefault();

            if (partSizeNode == null)
            {
                XElement newNode = new XElement("PartSize", "2048");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return "2048";
            }

            var partSize = partSizeNode.Value;
            int partSizeToInt;

            if (string.IsNullOrEmpty(partSize))
            {
                return "2048";
            }

            if (!int.TryParse(partSize, out partSizeToInt))
            {
                return "2048";
            }

            if (partSizeToInt < 0)
            {
                return "0";
            }

            return partSizeToInt.ToString();
        }

        private string GET_FORMAT(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return "7z";
            }

            var formatNode = xdoc.Descendants("Format").FirstOrDefault();

            var supportedFormat = new HashSet<string>
            {
                "7z",
                "zip",
                "rar",
                "tar"
            };

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

            if (!supportedFormat.Contains(format))
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

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return null;
            }

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

        private bool GET_ZSTD(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return true;
            }

            var zstdNode = xdoc.Descendants("Zstd").FirstOrDefault();

            var supportedBool = new HashSet<string>
            {
                "True",
                "False"
            };

            if (zstdNode == null)
            {
                XElement newNode = new XElement("Zstd", "True");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return true;
            }

            var zstd = zstdNode.Value;
            bool zstdToBool;

            if (string.IsNullOrEmpty(zstd))
            {
                return true;
            }

            if (!supportedBool.Contains(zstd))
            {
                return true;
            }

            if (!bool.TryParse(zstd, out zstdToBool))
            {
                return true;
            }

            return zstdToBool;
        }

        private bool GET_DISABLE_TAR_SPLIT(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return false;
            }

            var disableTarSplitNode = xdoc.Descendants("DisableTarSplit").FirstOrDefault();

            var supportedBool = new HashSet<string>
            {
                "True",
                "False"
            };

            if (disableTarSplitNode == null)
            {
                XElement newNode = new XElement("DisableTarSplit", "False");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return false;
            }

            var disableTarSplit = disableTarSplitNode.Value;
            bool disableTarSplitToBool;

            if (string.IsNullOrEmpty(disableTarSplit))
            {
                return false;
            }

            if (!supportedBool.Contains(disableTarSplit))
            {
                return false;
            }

            if (!bool.TryParse(disableTarSplit, out disableTarSplitToBool))
            {
                return false;
            }

            return disableTarSplitToBool;
        }

        private bool GET_AUTOSAVE(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return true;
            }

            var autoSaveNode = xdoc.Descendants("AutoSave").FirstOrDefault();

            var supportedBool = new HashSet<string>
            {
                "True",
                "False"
            };

            if (autoSaveNode == null)
            {
                XElement newNode = new XElement("AutoSave", "True");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return true;
            }

            var autoSave = autoSaveNode.Value;
            bool autoSaveToBool;

            if (string.IsNullOrEmpty(autoSave))
            {
                return true;
            }

            if (!supportedBool.Contains(autoSave))
            {
                return true;
            }

            if (!bool.TryParse(autoSave, out autoSaveToBool))
            {
                return true;
            }

            return autoSaveToBool;
        }

        private long GET_SEVENZ_USAGE_COUNT(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return 0;
            }

            var sevenZUsageCountNode = xdoc.Descendants("SevenZUsageCount").FirstOrDefault();

            if (sevenZUsageCountNode == null)
            {

                XElement newNode = new XElement("SevenZUsageCount", "0");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return 0;
            }

            var sevenZUsageCount = sevenZUsageCountNode.Value;
            long sevenZUsageCountToLong;

            if (string.IsNullOrEmpty(sevenZUsageCount))
            {
                return 0;
            }

            if (!long.TryParse(sevenZUsageCount, out sevenZUsageCountToLong))
            {
                return 0;
            }

            if (sevenZUsageCountToLong < 0)
            {
                return 0;
            }

            return sevenZUsageCountToLong;
        }

        private long GET_RAR_USAGE_COUNT(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                CREATE_DEFAULT_CONFIG(configFilePath);
            }

            XDocument xdoc;

            try
            {
                // 加载 XML 文档
                xdoc = XDocument.Load(configFilePath);
            }

            catch (XmlException)
            {
                // 如果加载失败，创建新的默认配置文件并返回默认值
                CREATE_DEFAULT_CONFIG(configFilePath);

                return 0;
            }

            var rarUsageCountNode = xdoc.Descendants("RarUsageCount").FirstOrDefault();

            if (rarUsageCountNode == null)
            {

                XElement newNode = new XElement("RarUsageCount", "0");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return 0;
            }

            var rarUsageCount = rarUsageCountNode.Value;
            long rarUsageCountToLong;

            if (string.IsNullOrEmpty(rarUsageCount))
            {
                return 0;
            }

            if (!long.TryParse(rarUsageCount, out rarUsageCountToLong))
            {
                return 0;
            }

            if (rarUsageCountToLong < 0)
            {
                return 0;
            }

            return rarUsageCountToLong;
        }

        private void DEFAULT_PARTSIZE_TEXTBOX()
        {
            TextBoxSize.Text = partSize;
        }

        private void DEFAULT_FORMAT_MENU()
        {
            ComboBoxFormat.Items.Add("7z");
            ComboBoxFormat.Items.Add("zip");
            ComboBoxFormat.Items.Add("rar");
            ComboBoxFormat.Items.Add("tar");

            if (format == "7z")
            {
                ComboBoxFormat.SelectedIndex = 0;
            }

            if (format == "zip")
            {
                ComboBoxFormat.SelectedIndex = 1;
            }

            if (format == "rar")
            {
                ComboBoxFormat.SelectedIndex = 2;
            }

            if (format == "tar")
            {
                ComboBoxFormat.SelectedIndex = 3;
            }

            ComboBoxFormat.SelectedIndexChanged += COMBOBOX_FORMAT_SELECTED_INDEX_CHANGED_UI;
        }

        private void DEFAULT_PASSWORD_TEXTBOX()
        {
            TextBoxPassword.Text = password;

            if (format == "tar")
            {
                TextBoxPassword.Enabled = false;
            }

            else
            {
                TextBoxPassword.Enabled = true;
            }
        }

        private void DEFAULT_ZSTD()
        {
            // 定义后台运行的默认显示状态
            if (zstd)
            {
                CheckBoxZstd.Checked = true;
            }

            if (!zstd)
            {
                CheckBoxZstd.Checked = false;
            }

            if (format == "tar")
            {
                CheckBoxZstd.Visible = true;

                if (CheckBoxZstd.Checked)
                {
                    CheckBoxDisableSplit.Visible = true;

                    if (!CheckBoxDisableSplit.Checked)
                    {
                        TextBoxSize.Enabled = true;
                    }

                    else
                    {
                        TextBoxSize.Enabled = false;
                    }
                }

                else
                {
                    CheckBoxDisableSplit.Visible = false;
                }

            }

            else
            {
                CheckBoxZstd.Visible = false;
            }
        }

        private void DEFAULT_DISABLE_TAR_SPLIT()
        {
            // 定义后台运行的默认显示状态
            if (disableTarSplit)
            {
                CheckBoxDisableSplit.Checked = true;
            }

            if (!disableTarSplit)
            {
                CheckBoxDisableSplit.Checked = false;
            }

            if (format == "tar")
            {
                CheckBoxDisableSplit.Visible = true;
            }

            else
            {
                CheckBoxDisableSplit.Visible = false;
            }
        }

        private void DEFAULT_AUTOSAVE()
        {
            // 定义后台运行的默认显示状态
            if (autoSave)
            {
                CheckBoxAutoSave.Checked = true;
            }

            else
            {
                CheckBoxAutoSave.Checked = false;
            }
        }

        private void SAVE_CONFIG()
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;

            UPDATE_CONFIG($"{xmlPath}", "Language", $"{currentLanguage}");
            UPDATE_CONFIG($"{xmlPath}", "ScreenWidth", $"{width}");
            UPDATE_CONFIG($"{xmlPath}", "ScreenHeight", $"{height}");
            UPDATE_CONFIG($"{xmlPath}", "SystemScale", $"{systemScale}");
            UPDATE_CONFIG($"{xmlPath}", "PartSize", $"{partSize}");
            UPDATE_CONFIG($"{xmlPath}", "Format", $"{format}");
            UPDATE_CONFIG($"{xmlPath}", "Password", $"{password}");
            UPDATE_CONFIG($"{xmlPath}", "Zstd", $"{zstd}");
            UPDATE_CONFIG($"{xmlPath}", "DisableTarSplit", $"{disableTarSplit}");
            UPDATE_CONFIG($"{xmlPath}", "AutoSave", $"{autoSave}");
            UPDATE_CONFIG($"{xmlPath}", "SevenZUsageCount", $"{sevenZUsageCount}");
            UPDATE_CONFIG($"{xmlPath}", "RarUsageCount", $"{rarUsageCount}");
            UPDATE_CONFIG($"{xmlPath}", "Auto7zUsageCount", $"{auto7zUsageCount}");
        }

        private void SAVE_LOCATION()
        {
            locationX = this.Location.X;
            locationY = this.Location.Y;

            UPDATE_CONFIG($"{xmlPath}", "LocationX", $"{locationX}");
            UPDATE_CONFIG($"{xmlPath}", "LocationY", $"{locationY}");
        }

        private void ADD_SEVENZ_USAGE_COUNT()
        {
            sevenZUsageCount++;
            UPDATE_CONFIG($"{xmlPath}", "SevenZUsageCount", $"{sevenZUsageCount}");
        }

        private void ADD_RAR_USAGE_COUNT()
        {
            rarUsageCount++;
            UPDATE_CONFIG($"{xmlPath}", "RarUsageCount", $"{rarUsageCount}");
        }

        private void ADD_AUTO7Z_USAGE_COUNT()
        {
            auto7zUsageCount = sevenZUsageCount + rarUsageCount;
            UPDATE_CONFIG($"{xmlPath}", "Auto7zUsageCount", $"{auto7zUsageCount}");
        }

        private void NOTICE_CONFIG_SAVED()
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
        }

        private void ERROR_EMPTY_PATH()
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
        }

        private void ERROR_SEVENZ_EXIST()
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show("程序工作路径下7z组件(7z.exe 7z.dll)不完整，无法启动7z处理流程。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "zh-TW":
                    MessageBox.Show("程式工作路徑下7z組件(7z.exe 7z.dll)不完整，無法啓動7z處理流程。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "en-US":
                    MessageBox.Show("The 7z components (7z.exe 7z.dll) in the program's working directory are incomplete and can not start the 7z processing flow.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void WARNING_LANG_EXIST()
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show("程序工作路径下7z组件(Lang)不完整，可能导致7z本地化翻译出现问题。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "zh-TW":
                    MessageBox.Show("程式工作路徑下7z組件(Lang)不完整，可能導致7z本地化翻譯出現問題。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "en-US":
                    MessageBox.Show("The 7z component (Lang) in the program working path is incomplete, which may result in issues with 7z localization translation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }

        private void ERROR_RAR_EXIST()
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show("程序工作路径下rar组件(7zxa.dll Default.SFX Default64.SFX Rar.exe rarreg.key WinCon.SFX WinCon64.SFX WinRAR.exe Zip.SFX Zip64.SFX)不完整，无法启动rar处理流程。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "zh-TW":
                    MessageBox.Show("程式工作路徑下rar組件(7zxa.dll Default.SFX Default64.SFX Rar.exe rarreg.key WinCon.SFX WinCon64.SFX WinRAR.exe Zip.SFX Zip64.SFX)不完整，無法啓動rar處理流程。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "en-US":
                    MessageBox.Show("The rar components (7zxa.dll Default.SFX Default64.SFX Rar.exe rarreg.key WinCon.SFX WinCon64.SFX WinRAR.exe Zip.SFX Zip64.SFX) in the program's working directory are incomplete and can not start the rar processing flow.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void ERROR_CREATE_FOLDER_FAILED(Exception error)
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show($"创建目标文件夹时出错，错误为: {error.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "zh-TW":
                    MessageBox.Show($"創建目標文件夾時出錯，錯誤為: {error.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "en-US":
                    MessageBox.Show($"The error occurred while creating the target folder,error message is: {error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void ERROR_ILLAGEL_SIZE()
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show("存在非法的文件大小。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "zh-TW":
                    MessageBox.Show("存在非法的文件大小。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "en-US":
                    MessageBox.Show("Invalid file size.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void ERROR_EXCEPTION_MESSAGE(Exception error)
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show($"出现错误: {error.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "zh-TW":
                    MessageBox.Show($"出現錯誤: {error.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "en-US":
                    MessageBox.Show($"An error occurred: {error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void SET_CLIENT_SIZE()

        {
            this.ClientSize = new Size((int)(20.0f * systemScale) * 2 + CheckBoxAutoSave.Width + ButtonConfig.Width + (int)(10.0f * systemScale) * 2, (int)(20.0f * systemScale) * 2 + CheckBoxAutoSave.Width + ButtonConfig.Width + (int)(10.0f * systemScale) * 2);
            this.MaximumSize = this.ClientSize;
            this.MinimumSize = this.MaximumSize;
        }

        private void SET_BUTTON_CONFIG_SIZE()
        {
            ButtonConfig.Size = new Size((int)(120.0f * systemScale), (int)(30.0f * systemScale));
        }

        private void SET_COMPONENT_POSITION()
        {
            LabelFormat.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 - LabelFormat.Width / 2 - (int)(20.0f * systemScale), MainPanel.Height / 2 - LabelFormat.Height / 2);
            ComboBoxFormat.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 - (int)(20.0f * systemScale), MainPanel.Height / 2 - ComboBoxFormat.Height / 2 - (int)(1.0f * systemScale));
            CheckBoxZstd.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 + (int)(5.0f * systemScale) - (int)(20.0f * systemScale) + ComboBoxFormat.Width, MainPanel.Height / 2 - CheckBoxZstd.Height / 2 + (int)(1.0f * systemScale));

            LabelSize.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 - LabelFormat.Width / 2 - (int)(20.0f * systemScale), MainPanel.Height / 2 - LabelFormat.Height / 2 - this.Height / 6);
            TextBoxSize.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 - (int)(20.0f * systemScale), MainPanel.Height / 2 - ComboBoxFormat.Height / 2 - this.Height / 6);
            LabelUnit.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 + (int)(5.0f * systemScale) - (int)(20.0f * systemScale) + TextBoxSize.Width, MainPanel.Height / 2 - ComboBoxFormat.Height / 2 - this.Height / 6 + (int)(3.0f * systemScale));

            LabelPassword.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 - LabelFormat.Width / 2 - (int)(20.0f * systemScale), MainPanel.Height / 2 - LabelFormat.Height / 2 + this.Height / 6);
            TextBoxPassword.Location = new Point(MainPanel.Width / 2 - ComboBoxFormat.Width / 2 + LabelFormat.Width / 2 - (int)(20.0f * systemScale), MainPanel.Height / 2 - ComboBoxFormat.Height / 2 + this.Height / 6);

            CheckBoxAutoSave.Location = new Point((int)(20.0f * systemScale), MainPanel.Height - CheckBoxAutoSave.Height - (int)(15.0f * systemScale));
            CheckBoxDisableSplit.Location = new Point((int)(20.0f * systemScale), MainPanel.Height - CheckBoxDisableSplit.Height - (int)(15.0f * systemScale) - (int)(25.0f * systemScale));
            ButtonConfig.Location = new Point(MainPanel.Width - ButtonConfig.Width - (int)(10.0f * systemScale), MainPanel.Height - ButtonConfig.Height - (int)(10.0f * systemScale));
        }

        private void SET_MAINFORM_LOCATION()
        {
            if (oldScreenWidth == Screen.PrimaryScreen.Bounds.Width && oldScreenHeight == Screen.PrimaryScreen.Bounds.Height && oldSystemScale == systemScale)
            {
                locationX = GET_LOCATION_X(xmlPath);
                locationY = GET_LOCATION_Y(xmlPath);

                this.Location = new Point(locationX, locationY);
            }

            else
            {
                locationX = Screen.PrimaryScreen.Bounds.Width / 2 - this.Size.Width / 2;
                locationY = Screen.PrimaryScreen.Bounds.Height / 2 - this.Size.Height / 2;

                this.Location = new Point(locationX, locationY);
            }
        }

        private void BUTTON_CONFIG_CLICK(object sender, EventArgs e)
        {
            NOTICE_CONFIG_SAVED();
            SAVE_CONFIG();
            SAVE_LOCATION();
        }

        private void CHECKBOX_AUTOSAVE_CHECKED_CHANGED(object sender, EventArgs e)
        {
            bool isAutoSave = CheckBoxAutoSave.Checked;
            if (isAutoSave)
            {
                autoSave = true;
            }

            else
            {
                autoSave = false;
            }
        }

        private void CHECKBOX_ZSTD_CHECKED_CHANGED(object sender, EventArgs e)
        {
            bool isZstd = CheckBoxZstd.Checked;
            if (isZstd)
            {
                zstd = true;
                if (format == "tar")
                {
                    CheckBoxDisableSplit.Visible = true;
                }
                
                if(CheckBoxDisableSplit.Checked)
                {
                    TextBoxSize.Enabled = false;
                }
            }

            else
            {
                zstd = false;
                CheckBoxDisableSplit.Visible = false;
                TextBoxSize.Enabled = true;
            }
        }

        private void CHECKBOX_TARSPLIT_CHECKED_CHANGED(object sender,EventArgs e)
        {
            bool isDisabled = CheckBoxDisableSplit.Checked;
            if (isDisabled)
            {
                disableTarSplit = true;
                TextBoxSize.Enabled = false;
            }

            else
            {
                disableTarSplit = false;
                TextBoxSize.Enabled = true;
            }
        }

        private void COMBOBOX_FORMAT_SELECTED_INDEX_CHANGED_UI(object sender, EventArgs e)
        {
            if (format=="tar")
            {
                CheckBoxZstd.Visible = true;
                TextBoxPassword.Enabled = false;

                if (CheckBoxZstd.Checked)
                {
                    CheckBoxDisableSplit.Visible = true;
                }

                if (!CheckBoxDisableSplit.Checked)
                {
                    TextBoxSize.Enabled = true;
                }

                else
                {
                    TextBoxSize.Enabled = false;
                }
            }

            else
            {
                CheckBoxZstd.Visible = false;
                CheckBoxDisableSplit.Visible = false;
                TextBoxPassword.Enabled = true;
                TextBoxSize.Enabled = true;
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
            SET_MAINFORM_LOCATION();
        }

        private void COMBOBOX_FORMAT_SELECTED_INDEX_CHANGED(object sender, EventArgs e)
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
                if (files.Length < 1)
                {
                    filePath = files[0];
                    fileName = Path.GetFileNameWithoutExtension(filePath);
                    directoryPath = Path.GetDirectoryName(filePath);

                    bool isLagelSize = GET_SIZE(filePath);

                    if (isLagelSize)
                    {
                        if (format == "7z" || format == "zip")
                        {
                            if (!CHECK_SEVENZ_EXIST())
                            {
                                ERROR_SEVENZ_EXIST();
                            }

                            if (!CHECK_LANG_EXIST() && sevenZUsageCount < 1)
                            {
                                WARNING_LANG_EXIST();
                            }

                            string command = GENERATE_COMMAND();

                            if (command == null)
                            {
                                ERROR_EMPTY_PATH();
                            }

                            DELETE_OLD_FILES();

                            bool finished = EXECUTE_COMMAND_BOOL(command);

                            if (!finished)
                            {
                                DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                            }

                            ADD_SEVENZ_USAGE_COUNT();
                            ADD_AUTO7Z_USAGE_COUNT();
                        }

                        if (format == "tar")
                        {
                            if (!CHECK_SEVENZ_EXIST())
                            {
                                ERROR_SEVENZ_EXIST();
                            }

                            if (!CHECK_LANG_EXIST() && sevenZUsageCount < 1)
                            {
                                WARNING_LANG_EXIST();
                            }

                            string command = GENERATE_COMMAND();

                            if (command == null)
                            {
                                ERROR_EMPTY_PATH();
                            }

                            DELETE_OLD_FILES();

                            bool tarFinished = EXECUTE_COMMAND_BOOL(command);

                            if (!tarFinished)
                            {
                                DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                            }

                            if (CheckBoxZstd.Checked)
                            {
                                while (tarFinished)
                                {
                                    string zstdCommand = ZSTD_COMMAND();

                                    DELETE_OLD_FILES();

                                    bool zstdFinished = EXECUTE_COMMAND_BOOL(zstdCommand);

                                    if (!zstdFinished)
                                    {
                                        DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                                    }

                                    while (zstdFinished)
                                    {
                                        DELETE_TEMP_TAR(newFolderPath);
                                        break;
                                    }

                                    break;
                                }
                            }

                            ADD_SEVENZ_USAGE_COUNT();
                            ADD_AUTO7Z_USAGE_COUNT();
                        }

                        if (format == "rar")
                        {
                            if (!CHECK_RAR_EXIST())
                            {
                                ERROR_RAR_EXIST();
                            }

                            string command = GENERATE_COMMAND();

                            if (command == null)
                            {
                                ERROR_EMPTY_PATH();
                            }

                            DELETE_OLD_FILES();

                            bool finished = EXECUTE_COMMAND_BOOL(command);

                            if (!finished)
                            {
                                DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                            }

                            ADD_RAR_USAGE_COUNT();
                            ADD_AUTO7Z_USAGE_COUNT();
                        }
                    }

                    else
                    {
                        ERROR_ILLAGEL_SIZE();
                    }
                }

                else
                {
                    foreach (var simpleFilePath in files)
                    {
                        filePath = simpleFilePath;
                        fileName = Path.GetFileNameWithoutExtension(filePath);
                        directoryPath = Path.GetDirectoryName(filePath);

                        bool isLagelSize = GET_SIZE(filePath);

                        if (isLagelSize)
                        {
                            if (format == "7z" || format == "zip")
                            {
                                if (!CHECK_SEVENZ_EXIST())
                                {
                                    ERROR_SEVENZ_EXIST();
                                    break;
                                }

                                if (!CHECK_LANG_EXIST() && sevenZUsageCount < 1)
                                {
                                    WARNING_LANG_EXIST();
                                }

                                string command = GENERATE_COMMAND();

                                if (command == null)
                                {
                                    ERROR_EMPTY_PATH();
                                }

                                DELETE_OLD_FILES();

                                bool finished = await Task.Run(() => EXECUTE_COMMAND_BOOL(command));

                                if (!finished)
                                {
                                    DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                                }

                                ADD_SEVENZ_USAGE_COUNT();
                                ADD_AUTO7Z_USAGE_COUNT();
                            }

                            if (format == "tar")
                            {
                                if (!CHECK_SEVENZ_EXIST())
                                {
                                    ERROR_SEVENZ_EXIST();
                                    break;
                                }

                                if (!CHECK_LANG_EXIST() && sevenZUsageCount < 1)
                                {
                                    WARNING_LANG_EXIST();
                                }

                                string command = GENERATE_COMMAND();

                                if (command == null)
                                {
                                    ERROR_EMPTY_PATH();
                                }

                                DELETE_OLD_FILES();

                                bool tarFinished = await Task.Run(() => EXECUTE_COMMAND_BOOL(command));

                                if (!tarFinished)
                                {
                                    DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                                }

                                if (CheckBoxZstd.Checked)
                                {
                                    while (tarFinished)
                                    {
                                        string zstdCommand = ZSTD_COMMAND();

                                        DELETE_OLD_FILES();

                                        bool zstdFinished = await Task.Run(() => EXECUTE_COMMAND_BOOL(zstdCommand));

                                        if (!zstdFinished)
                                        {
                                            DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                                        }

                                        while (zstdFinished)
                                        {
                                            DELETE_TEMP_TAR(newFolderPath);
                                            break;
                                        }

                                        break;
                                    }
                                }

                                ADD_SEVENZ_USAGE_COUNT();
                                ADD_AUTO7Z_USAGE_COUNT();
                            }

                            if (format == "rar")
                            {
                                if (!CHECK_RAR_EXIST())
                                {
                                    ERROR_RAR_EXIST();
                                }

                                string command = GENERATE_COMMAND();

                                if (command == null)
                                {
                                    ERROR_EMPTY_PATH();
                                }

                                DELETE_OLD_FILES();

                                bool finished = await Task.Run(() => EXECUTE_COMMAND_BOOL(command));

                                if (!finished)
                                {
                                    DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                                }

                                ADD_RAR_USAGE_COUNT();
                                ADD_AUTO7Z_USAGE_COUNT();
                            }
                        }
                    }
                }
            }

            // 恢复鼠标指针状态
            Cursor.Current = Cursors.Default; // 设置鼠标指针为默认状态
        }

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
            if (!CheckBoxAutoSave.Checked)
            {
                UPDATE_CONFIG($"{xmlPath}", "AutoSave", "False");
                SAVE_LOCATION();
            }

            else
            {
                SAVE_CONFIG();
                SAVE_LOCATION();
            }
        }
    }
}
