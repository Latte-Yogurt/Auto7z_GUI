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
using System.Text;

namespace Auto7z_GUI
{
    public partial class MainForm : Form
    {
        private Dictionary<string, Dictionary<string, string>> languageTexts;

        private string workPath;
        private string extractPath;
        private bool hasPermission;
        private string desktopPath;
        private string logFileName;
        private string logFilePath;
        private bool isCreateAuto7zFolder = true;
        private bool isCreate7zFolder = true;
        private bool isCreateLangFolder = true;
        private bool packedOneFile = false;
        private string xmlPath;
        private bool isHandleSeparately;
        private string filePath;
        private string fileName;
        private string directoryPath;
        private string[] filePaths;
        private string auto7zPath;
        private string sevenZPath;
        private string sevenZExePath;
        private string sevenZDllPath;
        private string langPath;
        private string zhCNPath;
        private string zhTWPath;
        private string md5CalculaterPath;
        public static string currentLanguage;
        public static float systemScale;
        private int oldScreenWidth;
        private int oldScreenHeight;
        private float oldSystemScale;
        private int locationX;
        private int locationY;
        private string partSize;
        private string format;
        private string password;
        private bool zstd;
        private bool disableSplit;
        private bool createMd5;
        private bool autoSave;
        private bool portable;
        private long sevenZUsageCount;
        private long fileSize;
        private long folderSize;
        private long fileSizes;
        private long folderSizes;
        private long totalSize;
        private string newFolderPath;

        public MainForm(string[] args)
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;

            GET_SCALE();
            SET_BUTTON_CONFIG_SIZE();
            SET_CLIENT_SIZE();
            SET_MAINFORM_LOCATION();

            desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            logFileName = @"Auto7z_GUI.log";
            logFilePath = Path.Combine(desktopPath, logFileName);

            workPath = GET_WORK_PATH();
            hasPermission = CHECK_PATH_READ_WRITE(workPath, out Exception noPermissionEx);

            if (!hasPermission)
            {
                ERROR_APPLICATION_NO_PREMISSION(workPath, noPermissionEx);
                this.Close();
                return;
            }

            string xml = @"Auto7z_GUI.xml";
            xmlPath = Path.Combine(workPath, xml);
            CHECK_XML_LEGAL(xmlPath);

            currentLanguage = GET_CURRENT_LANGUAGE(xmlPath);
            oldScreenWidth = GET_SCREEN_WIDTH(xmlPath);
            oldScreenHeight = GET_SCREEN_HEIGHT(xmlPath);
            oldSystemScale = GET_SYSTEM_SCALE(xmlPath);
            sevenZUsageCount = GET_SEVENZ_USAGE_COUNT(xmlPath);

            InitializeLanguage();
            InitializeEvents();
            InitializePaths();
            InitializeConfig();

            if (args != null && args.Length > 0)
            {
                TASK(args);
            }
        }

        private void TASK(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                if (args.Length > 1)
                {
                    QUESTION_PACKED_IN_ONE_FILE();
                    isHandleSeparately = !packedOneFile;
                }

                else
                {
                    isHandleSeparately = false;
                }
            }

            if (!CHECK_AUTO7Z_EXIST() || !CHECK_SEVENZ_EXIST())
            {
                CREATE_COMPONENTS();
                if (!isCreateAuto7zFolder || !isCreate7zFolder || !isCreateLangFolder)
                {
                    this.Close();
                    return;
                }
            }

            if (args != null && args.Length > 0 && args.ToList().TrueForAll(arg => !string.IsNullOrEmpty(arg)))
            {
                if (!isHandleSeparately)
                {
                    filePaths = !packedOneFile ? new string[] { Path.GetFullPath(args[0]) } : args;
                    fileName = Path.GetFileNameWithoutExtension(Path.GetFullPath(args[0]));
                    directoryPath = Path.GetDirectoryName(Path.GetFullPath(args[0]));

                    bool isFileInUse = IS_FILE_IN_USE(filePaths, out Exception ex);

                    if (isFileInUse)
                    {
                        ERROR_FILE_IN_USE(ex);
                        this.Close();
                        return;
                    }

                    bool getSizeSuccess = !packedOneFile ? GET_SINGLE_FILE_SIZE(filePaths[0]) : GET_MULTIPLE_FILES_SIZE(filePaths);

                    if (!getSizeSuccess)
                    {
                        DELETE_EXTRACT_RESOURCE();
                        this.Close();
                        return;
                    }

                    if (format == "7z" || format == "zip")
                    {
                        if (!CHECK_SEVENZ_EXIST())
                        {
                            CREATE_COMPONENTS();
                            if (!isCreate7zFolder)
                            {
                                DELETE_EXTRACT_RESOURCE();
                                this.Close();
                                return;
                            }
                        }

                        string command = GENERATE_COMMAND(filePaths);

                        if (command == null)
                        {
                            DELETE_EXTRACT_RESOURCE();
                            this.Close();
                            return;
                        }

                        bool finished = EXECUTE_COMMAND_BOOL(command);

                        if (!finished)
                        {
                            DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                        }

                        if (createMd5 && Directory.Exists(newFolderPath))
                        {
                            GENERATE_MD5(md5CalculaterPath, newFolderPath);
                        }

                        ADD_SEVENZ_USAGE_COUNT();
                        DELETE_EXTRACT_RESOURCE();
                        this.Close();
                        return;
                    }

                    if (format == "tar")
                    {
                        if (!CHECK_SEVENZ_EXIST())
                        {
                            CREATE_COMPONENTS();
                            if (!isCreate7zFolder)
                            {
                                DELETE_EXTRACT_RESOURCE();
                                this.Close();
                                return;
                            }
                        }

                        string command = GENERATE_COMMAND(filePaths);

                        if (command == null)
                        {
                            DELETE_EXTRACT_RESOURCE();
                            this.Close();
                            return;
                        }

                        bool tarFinished = EXECUTE_COMMAND_BOOL(command);

                        if (!tarFinished)
                        {
                            DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                        }

                        if (zstd)
                        {
                            string zstdCommand = ZSTD_COMMAND();
                            bool zstdFinished = EXECUTE_COMMAND_BOOL(zstdCommand);

                            if (!zstdFinished)
                            {
                                DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                            }

                            else
                            {
                                DELETE_TEMP_TAR(newFolderPath);
                            }
                        }

                        if (createMd5 && Directory.Exists(newFolderPath))
                        {
                            GENERATE_MD5(md5CalculaterPath, newFolderPath);
                        }

                        ADD_SEVENZ_USAGE_COUNT();
                        DELETE_EXTRACT_RESOURCE();
                        this.Close();
                        return;
                    }
                }

                else
                {
                    foreach (var singleFilePath in args)
                    {
                        filePath = Path.GetFullPath(singleFilePath);
                        fileName = Path.GetFileNameWithoutExtension(filePath);
                        directoryPath = Path.GetDirectoryName(filePath);

                        bool isFileInUse = IS_FILE_IN_USE(new string[] { filePath }, out Exception ex);

                        if (isFileInUse)
                        {
                            WRANING_FILE_IN_USE(fileName, ex);
                            return;
                        }

                        bool getSizeSuccess = GET_SINGLE_FILE_SIZE(filePath);

                        if (!getSizeSuccess)
                        {
                            return;
                        }

                        if (format == "7z" || format == "zip")
                        {
                            if (!CHECK_SEVENZ_EXIST())
                            {
                                CREATE_COMPONENTS();
                                if (!isCreate7zFolder)
                                {
                                    break;
                                }
                            }

                            string command = GENERATE_COMMAND(new string[] { filePath });

                            bool finished = EXECUTE_COMMAND_BOOL(command);

                            if (!finished)
                            {
                                DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                            }

                            if (createMd5 && Directory.Exists(newFolderPath))
                            {
                                GENERATE_MD5(md5CalculaterPath, newFolderPath);
                            }

                            ADD_SEVENZ_USAGE_COUNT();
                        }

                        if (format == "tar")
                        {
                            if (!CHECK_SEVENZ_EXIST())
                            {
                                CREATE_COMPONENTS();
                                if (!isCreate7zFolder)
                                {
                                    break;
                                }
                            }

                            string command = GENERATE_COMMAND(new string[] { filePath });

                            bool tarFinished = EXECUTE_COMMAND_BOOL(command);

                            if (!tarFinished)
                            {
                                DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                            }

                            if (zstd)
                            {
                                string zstdCommand = ZSTD_COMMAND();
                                bool zstdFinished = EXECUTE_COMMAND_BOOL(zstdCommand);

                                if (!zstdFinished)
                                {
                                    DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                                }
                                else
                                {
                                    DELETE_TEMP_TAR(newFolderPath);
                                }
                            }

                            if (createMd5 && Directory.Exists(newFolderPath))
                            {
                                GENERATE_MD5(md5CalculaterPath, newFolderPath);
                            }

                            ADD_SEVENZ_USAGE_COUNT();
                        }
                    }

                    DELETE_EXTRACT_RESOURCE();
                    this.Close();
                    return;
                }
            }
        }

        #region Get System Parameters
        static string GET_WORK_PATH()
        {
            // 获取当前执行程序集
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            // 返回路径
            return Path.GetDirectoryName(assemblyPath);
        }

        private string GET_EXTRACT_PATH()
        {
            string appDataLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return appDataLocalPath;
        }

        private void GET_SCALE()
        {
            float dpi;
            using (Graphics g = this.CreateGraphics())
            {
                dpi = g.DpiX;
            }

            systemScale = dpi / 96.0f;
        }
        #endregion

        #region Initialize Language
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
                        { "OptionMenu","选项"},
                        { "OptionMenuDisableSplit","禁用分卷"},
                        { "OptionMenuCreateMD5","压缩完成后生成MD5文件"},
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
                        { "OptionMenu","選項"},
                        { "OptionMenuDisableSplit","禁用分卷"},
                        { "OptionMenuCreateMD5","壓縮完成后生成MD5文件"},
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
                        { "OptionMenu","Option"},
                        { "OptionMenuDisableSplit","Disable Split"},
                        { "OptionMenuCreateMD5","Generate MD5 file after compression is complete"},
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
            OptionMenu.Text = languageTexts[currentLanguage]["OptionMenu"];
            OptionMenuDisableSplit.Text = languageTexts[currentLanguage]["OptionMenuDisableSplit"];
            OptionMenuCreateMD5.Text = languageTexts[currentLanguage]["OptionMenuCreateMD5"];
            CheckBoxAutoSave.Text = languageTexts[currentLanguage]["CheckBoxAutoSave"];
            ButtonConfig.Text = languageTexts[currentLanguage]["ButtonConfig"];

            SET_COMPONENT_POSITION();
        }

        private void InitializeLanguage()
        {
            InitializeLanguageTexts();
            UpdateLanguage();
        }
        #endregion

        #region Initialize Program Parameters
        private void InitializeEvents()
        {
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(MAINFORM_DRAGENTER);
            this.DragDrop += new DragEventHandler(MAINFORM_DRAGDROP);
            this.DragLeave += new EventHandler(MAINFORM_DRAGLEAVE);
            this.FormClosing += MAINFORM_FORM_CLOSING;
            ComboBoxFormat.SelectedIndexChanged += COMBOBOX_FORMAT_SELECTED_INDEX_CHANGED_UI;
        }

        private void InitializePaths()
        {
            extractPath = GET_EXTRACT_PATH();

            auto7zPath = Path.Combine(extractPath, "Auto7z_GUI");
            sevenZPath = Path.Combine(auto7zPath, "7z");
            sevenZExePath = Path.Combine(sevenZPath, "7z.exe");
            sevenZDllPath = Path.Combine(sevenZPath, "7z.dll");
            langPath = Path.Combine(sevenZPath, "Lang");
            zhCNPath = Path.Combine(langPath, "zh-cn.txt");
            zhTWPath = Path.Combine(langPath, "zh-tw.txt");
            md5CalculaterPath = Path.Combine(auto7zPath, "MD5Calculater.exe");
        }

        private void InitializeConfig()
        {
            partSize = GET_PARTSIZE(xmlPath);
            format = GET_FORMAT(xmlPath);
            password = GET_PASSWORD(xmlPath);
            zstd = GET_ZSTD(xmlPath);
            disableSplit = GET_DISABLE_SPLIT(xmlPath);
            createMd5 = GET_CREATE_MD5(xmlPath);
            autoSave = GET_AUTOSAVE(xmlPath);
            portable = GET_PORTABLE(xmlPath);

            DEFAULT_PARTSIZE_TEXTBOX();
            DEFAULT_FORMAT_MENU();
            DEFAULT_PASSWORD_TEXTBOX();
            DEFAULT_ZSTD();
            DEFAULT_OPTION_MENU_DISABLE_SPLIT();
            DEFAULT_OPTION_MENU_CREATE_MD5();
            DEFAULT_AUTOSAVE();
        }
        #endregion

        #region Extract Resources
        private void CREATE_COMPONENTS()
        {
            if (!Directory.Exists(auto7zPath))
            {
                isCreateAuto7zFolder = CREATE_RESOURCE_FOLDER(auto7zPath);
            }

            if (Directory.Exists(auto7zPath))
            {
                CHECK_FOLDER_LEGAL(auto7zPath);

                if (!File.Exists(md5CalculaterPath))
                {
                    CREATE_MD5_CALCULATER_EXE();
                }

                if (!Directory.Exists(sevenZPath))
                {
                    isCreate7zFolder = CREATE_RESOURCE_FOLDER(sevenZPath);
                }

                if (Directory.Exists(sevenZPath))
                {
                    CHECK_FOLDER_LEGAL(sevenZPath);

                    if (!File.Exists(sevenZExePath))
                    {
                        CREATE_SEVENZ_EXE();
                    }

                    if (!File.Exists(sevenZDllPath))
                    {
                        CREATE_SEVENZ_DLL();
                    }

                    if (!Directory.Exists(langPath))
                    {
                        isCreateLangFolder = CREATE_RESOURCE_FOLDER(langPath);
                    }

                    if (Directory.Exists(langPath))
                    {
                        CHECK_FOLDER_LEGAL(langPath);

                        if (!File.Exists(zhCNPath))
                        {
                            CREATE_ZHCN_TXT();
                        }

                        if (!File.Exists(zhTWPath))
                        {
                            CREATE_ZHTW_TXT();
                        }
                    }
                }
            }
        }

        private void EXTRACT_RESOURCE(string resourceName, string outputPath)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    ERROR_RESOURCE_EXIST(resourceName);
                    return;
                }

                using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }

        private bool CREATE_RESOURCE_FOLDER(string newFolderPath)
        {
            if (!Directory.Exists(newFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(newFolderPath);
                    return true;
                }

                catch (Exception createFailureEx)
                {
                    ERROR_CREATE_FOLDER_FAILED(createFailureEx);
                    return false;
                }
            }

            else
            {
                return true;
            }
        }

        private void CREATE_MD5_CALCULATER_EXE()
        {
            string resourceName = "Auto7z_GUI.Resource.MD5Calculater.exe";
            string outputFileName = resourceName.Replace("Auto7z_GUI.Resource.", "");
            string outputPath = Path.Combine(auto7zPath, outputFileName);
            EXTRACT_RESOURCE(resourceName, outputPath);
        }

        private void CREATE_SEVENZ_EXE()
        {
            string resourceName = "Auto7z_GUI.Resource.sevenz.7z.exe";
            string outputFileName = resourceName.Replace("Auto7z_GUI.Resource.sevenz.", "");//将原始嵌入资源的文件名中的命名空间前缀替换为空字符串
            string outputPath = Path.Combine(sevenZPath, outputFileName);
            EXTRACT_RESOURCE(resourceName, outputPath);
        }

        private void CREATE_SEVENZ_DLL()
        {
            string resourceName = "Auto7z_GUI.Resource.sevenz.7z.dll";
            string outputFileName = resourceName.Replace("Auto7z_GUI.Resource.sevenz.", "");
            string outputPath = Path.Combine(sevenZPath, outputFileName);
            EXTRACT_RESOURCE(resourceName, outputPath);
        }

        private void CREATE_ZHCN_TXT()
        {
            string resourceName = "Auto7z_GUI.Resource.sevenz.Lang.zh-cn.txt";
            string outputFileName = resourceName.Replace("Auto7z_GUI.Resource.sevenz.Lang.", "");
            string outputPath = Path.Combine(langPath, outputFileName);
            EXTRACT_RESOURCE(resourceName, outputPath);
        }

        private void CREATE_ZHTW_TXT()
        {
            string resourceName = "Auto7z_GUI.Resource.sevenz.Lang.zh-tw.txt";
            string outputFileName = resourceName.Replace("Auto7z_GUI.Resource.sevenz.Lang.", "");
            string outputPath = Path.Combine(langPath, outputFileName);
            EXTRACT_RESOURCE(resourceName, outputPath);
        }
        #endregion

        #region Generate MD5
        private void GENERATE_MD5(string md5Exe, string path)
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = $"\"{md5Exe}\"";
                process.StartInfo.Arguments = $"\"{path}\"";
                process.StartInfo.UseShellExecute = true;

                process.Start();
                process.WaitForExit(); // 等待进程完成
            }
            catch (Exception ex)
            {
                ERROR_EXCEPTION_MESSAGE(ex);
            }
        }
        #endregion

        #region Generate Command
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

            catch (Exception unknownEx)
            {
                ERROR_EXCEPTION_MESSAGE(unknownEx);
                return false; // 发生异常时返回false
            }

            finally
            {
                process.Dispose(); // 清理资源
            }
        }

        private bool CREATE_NEW_FOLDER()
        {
            string folderCodeName = null;
            string nowTime = DateTime.Now.ToString("HH-mm-ss");

            switch (currentLanguage)
            {
                case "zh-CN":
                    folderCodeName = $"压缩文件_{nowTime}";
                    break;
                case "zh-TW":
                    folderCodeName = $"壓縮檔_{nowTime}";
                    break;
                case "en-US":
                    folderCodeName = $"Compressed File_{nowTime}";
                    break;
            }

            newFolderPath = $"{directoryPath}\\{fileName}_{folderCodeName}";

            if (!Directory.Exists(newFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(newFolderPath);
                    return true;
                }

                catch (Exception unknownEx)
                {
                    ERROR_CREATE_FOLDER_FAILED(unknownEx);
                    return false;
                }
            }

            else
            {
                return true;
            }
        }

        private string GENERATE_COMMAND(string[] paths)
        {
            if (!CREATE_NEW_FOLDER())
            {
                return null;
            }

            if (!packedOneFile)
            {
                return GENERATE_SINGLE_FILE_COMMAND(paths[0]);
            }

            else
            {
                return GENERATE_MULTIPLE_FILES_COMMAND(paths);
            }
        }

        private string GENERATE_SINGLE_FILE_COMMAND(string path)
        {
            string fullPath = Path.GetFullPath(path);

            if (!IS_VALID_PATH(fullPath))
            {
                return null;
            }

            long size = GET_FILE_SIZE_OR_FOLDER_SIZE(fullPath);
            string command = BUILD_COMMAND(fullPath, size);

            return command;
        }

        private string GENERATE_MULTIPLE_FILES_COMMAND(string[] paths)
        {
            foreach (var path in paths)
            {
                string fullPath = Path.GetFullPath(path);

                if (!IS_VALID_PATH(fullPath))
                {
                    return null;
                }
            }

            string command = BUILD_COMMAND_FOR_MULTIPLE_FILES(paths);

            return command;
        }

        private bool IS_VALID_PATH(string fullPath)
        {
            if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
            {
                if (fullPath.Length >= 260)
                {
                    ERROR_TOO_LONG_PATH(fullPath);
                }
                else
                {
                    ERROR_FILE_MAYBE_NOT_EXIST(fullPath, newFolderPath);
                }

                return false;
            }

            return true;
        }

        private long GET_FILE_SIZE_OR_FOLDER_SIZE(string path)
        {
            bool isFile = File.Exists(path);
            return isFile ? fileSize : folderSize;
        }

        private string BUILD_COMMAND(string fullPath, long size)
        {
            bool isNumber = int.TryParse(partSize, out int targetSize);

            if (!isNumber)
            {
                targetSize = 0;
            }

            if (format == "7z" || format == "zip" || format == "tar")
            {
                string command = $"@\"{sevenZExePath}\" a -aoa -t{format} \"{newFolderPath}\\{fileName}.{format}\" \"{fullPath}\"";

                if (ADD_VOLUME_CONDITION(size, targetSize))
                {
                    command += $" -v{targetSize}m";
                }

                if (ADD_PASSWORD_CONDITION())
                {
                    command += $" -p{password}";
                }

                if (ADD_MAXLEVEL_COMPRESSION_RATIO())
                {
                    command += @" -mx=9 -ms=on";
                }

                return command;
            }

            return null;
        }

        private string BUILD_COMMAND_FOR_MULTIPLE_FILES(string[] paths)
        {
            bool isNumber = int.TryParse(partSize, out int targetSize);

            if (!isNumber)
            {
                targetSize = 0;
            }

            if (format == "7z" || format == "zip" || format == "tar")
            {
                StringBuilder command = new StringBuilder();
                command.Append($"@\"{sevenZExePath}\" a -aoa -t{format} \"{newFolderPath}\\{fileName}.{format}\"");

                foreach (var path in paths)
                {
                    string fullPath = Path.GetFullPath(path);
                    command.Append($" \"{fullPath}\"");
                }

                if (ADD_VOLUME_CONDITION(totalSize, targetSize))
                {
                    command.Append($" -v{targetSize}m");
                }

                if (ADD_PASSWORD_CONDITION())
                {
                    command.Append($" -p{password}");
                }

                if (ADD_MAXLEVEL_COMPRESSION_RATIO())
                {
                    command.Append(@" -mx=9 -ms=on");
                }

                return command.ToString();
            }

            return null;
        }

        private bool ADD_VOLUME_CONDITION(long size, int targetSize)
        {
            if (format != "tar")
            {
                return size > targetSize && targetSize > 0 && !OptionMenuDisableSplit.Checked;
            }

            else
            {
                return size > targetSize && targetSize > 0 && !OptionMenuDisableSplit.Checked && !CheckBoxZstd.Checked;
            }
        }

        private bool ADD_PASSWORD_CONDITION()
        {
            return !string.IsNullOrEmpty(password) && format != "tar";
        }

        private bool ADD_MAXLEVEL_COMPRESSION_RATIO()
        {
            return format == "7z";
        }

        private string ZSTD_COMMAND()
        {
            string targetTar = $"{newFolderPath}\\{fileName}.tar";

            if (!File.Exists(targetTar) || !Directory.Exists(newFolderPath))
            {
                return null; // 如果文件或目录不存在，返回 null
            }

            bool isNumber = int.TryParse(partSize, out int targetSize);

            if (!isNumber)
            {
                targetSize = 0;
            }

            bool isFile = File.Exists(filePath);
            long size = isFile ? fileSize : folderSize; // 确定使用文件大小还是文件夹大小

            string command = $"@\"{sevenZExePath}\" a -aoa -tzstd \"{newFolderPath}\\{fileName}.tar.zst\" \"{newFolderPath}\\{fileName}.tar\"";

            // 添加分卷选项
            if (size > targetSize && targetSize > 0 && !OptionMenuDisableSplit.Checked)
            {
                command += $" -v{targetSize}m";
            }

            return command;
        }
        #endregion

        #region Delete Files
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

        private void DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED()
        {
            if (Directory.Exists($"{newFolderPath}"))
            {
                DELETE_DIRECTOR_CONTENTS(newFolderPath);
                Directory.Delete(newFolderPath);
            }
        }

        private void DELETE_FILES_CAUSE_PORTABLE_MODE()
        {
            if (Directory.Exists($"{auto7zPath}"))
            {
                DELETE_DIRECTOR_CONTENTS(auto7zPath);
                Directory.Delete(auto7zPath);
            }
        }

        private void DELETE_EXTRACT_RESOURCE()
        {
            if (portable && !IS_PROCESS_RUNNING("7z"))
            {
                DELETE_FILES_CAUSE_PORTABLE_MODE();
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
                    catch (Exception unknownEx)
                    {
                        ERROR_EXCEPTION_MESSAGE(unknownEx);
                    }
                }
            }
        }
        #endregion

        #region Get Size
        private bool GET_SINGLE_FILE_SIZE(string path)
        {
            string fullPath = Path.GetFullPath(path);
            string name = Path.GetFileNameWithoutExtension(fullPath);
            string dirertory = Path.GetDirectoryName(fullPath);

            if (File.Exists(fullPath))
            {
                fileSize = GET_FILE_SIZE(fullPath);

                if (fileSize == -1)
                {
                    ERROR_EMPTY_SIZE(name, fullPath);
                    return false;
                }

                return true;
            }

            else if (Directory.Exists(fullPath))
            {
                folderSize = GET_FOLDER_SIZE(fullPath);

                if (folderSize == -1)
                {
                    ERROR_EMPTY_SIZE(name, fullPath);
                    return false;
                }

                return true;
            }

            else if (!File.Exists(fullPath) || !Directory.Exists(fullPath))
            {
                bool canReadWrite = CHECK_PATH_READ_WRITE(dirertory, out Exception noPermissionEx);

                if (fullPath.Length >= 260)
                {
                    ERROR_TOO_LONG_PATH(fullPath);
                }

                if (!canReadWrite)
                {
                    ERROR_APPLICATION_NO_PREMISSION(dirertory, noPermissionEx);
                }

                return false;
            }

            return false;
        }

        private bool GET_MULTIPLE_FILES_SIZE(string[] paths)
        {
            List<string> fileList = new List<string>(paths);

            foreach (var path in paths)
            {
                string fullPath = Path.GetFullPath(path);
                string name = Path.GetFileNameWithoutExtension(fullPath);
                string dirertory = Path.GetDirectoryName(fullPath);

                if (File.Exists(fullPath))
                {
                    fileSize = GET_FILE_SIZE(fullPath);

                    if (fileSize != -1)
                    {
                        fileSizes += fileSize;
                    }

                    else
                    {
                        fileList.Remove(fullPath);
                        WARNING_REMOVE_ELEMENT(name, fullPath);
                    }
                }

                else if (Directory.Exists(fullPath))
                {
                    folderSize = GET_FOLDER_SIZE(fullPath);

                    if (folderSize != -1)
                    {
                        folderSizes += folderSize;
                    }

                    else
                    {
                        fileList.Remove(fullPath);
                        WARNING_REMOVE_ELEMENT(name, fullPath);
                    }
                }

                else if (!File.Exists(fullPath) || !Directory.Exists(fullPath))
                {
                    bool canReadWrite = CHECK_PATH_READ_WRITE(dirertory, out Exception noPermissionEx);

                    if (fullPath.Length >= 260)
                    {
                        ERROR_TOO_LONG_PATH(fullPath);
                    }

                    if (!canReadWrite)
                    {
                        ERROR_APPLICATION_NO_PREMISSION(dirertory, noPermissionEx);
                    }
                }
            }

            filePaths = fileList.ToArray();

            if (filePaths != null)
            {
                totalSize = fileSizes + folderSizes;
                return true;
            }

            return false;
        }

        private long GET_FILE_SIZE(string filePath)
        {
            if (filePath != null)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                long fileSizeBytes = fileInfo.Length;

                long fileSizeMiB = fileSizeBytes / (1024 * 1024);

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
                long folderSizeBytes = 0;

                // 获取文件夹中的所有文件和子文件夹
                DirectoryInfo directoryInfo = new DirectoryInfo(filePath);

                // 计算文件大小
                FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
                foreach (FileInfo file in files)
                {
                    folderSizeBytes += file.Length;
                }

                long folderSizeMiB = folderSizeBytes / (1024 * 1024);

                return folderSizeMiB;
            }

            else
            {
                return -1;
            }
        }
        #endregion

        #region Check Parameters
        private bool CHECK_PATH_READ_WRITE(string path, out Exception noPermissionEx)
        {
            noPermissionEx = null; // 初始化异常为 null
            try
            {
                // 检查可写性
                string testFilePath = Path.Combine(path, "test");

                // 尝试写入
                using (FileStream testFile = File.Create(testFilePath))
                {
                    // 写入一些数据（随意）
                    byte[] info = new UTF8Encoding(true).GetBytes("test");
                    testFile.Write(info, 0, info.Length);
                }

                // 尝试读取
                using (FileStream testFile = File.OpenRead(testFilePath))
                {
                    // 尝试读取数据
                    byte[] buffer = new byte[1024];
                    testFile.Read(buffer, 0, buffer.Length);
                }

                // 删除测试文件
                File.Delete(testFilePath);

                return true; // 两者都成功
            }
            catch (UnauthorizedAccessException unauthorizedEx)
            {
                noPermissionEx = unauthorizedEx;
                return false; // 不具备权限
            }
            catch (Exception otherEx)
            {
                noPermissionEx = otherEx;
                return false; // 发生其他异常
            }
        }

        private void CHECK_FOLDER_LEGAL(string directoryPath)
        {
            DirectoryInfo directory = new DirectoryInfo(directoryPath);

            if (directory.Exists && (directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                directory.Attributes = FileAttributes.Normal;
            }

            if (directory.Exists && (directory.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                directory.Attributes = FileAttributes.Normal;
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

        private void CHECK_LOG_LEGAL(string configFilePath)
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
        }

        private bool CHECK_AUTO7Z_EXIST()
        {
            return Directory.Exists(auto7zPath);

        }

        private bool CHECK_SEVENZ_EXIST()
        {
            return Directory.Exists(sevenZPath) && File.Exists(sevenZExePath) && File.Exists(sevenZDllPath) 
                && Directory.Exists(langPath) && File.Exists(zhCNPath) && File.Exists(zhTWPath);
        }

        private bool IS_PROCESS_RUNNING(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0; // 如果找到进程，则返回 true
        }

        private string[] GET_DIRECTOR_CONTENTS(string dirPath)
        {
            List<string> paths = new List<string>();

            foreach (string file in Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories))
            {
                paths.Add(file);
            }

            return paths.ToArray();
        }

        private bool IS_FILE_IN_USE(string[] filePaths, out Exception ex)
        {
            ex = null;
            try
            {
                foreach (var file in filePaths)
                {
                    if (File.Exists(file))
                    {
                        // 尝试以独占方式打开文件
                        using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            return false; // 文件未被占用
                        }
                    }

                    else
                    {
                        string[] paths = GET_DIRECTOR_CONTENTS(file);

                        foreach (var path in paths)
                        {
                            try
                            {
                                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                                {
                                    // 如果能成功打开，不做任何操作
                                }
                            }
                            catch (IOException ioEx)
                            {
                                ex = ioEx;
                                // 如果抛出IOException，表示文件被占用，跳出循环并返回true
                                return true;
                            }
                        }
                    }
                }
            }
            catch (IOException ioEx)
            {
                ex = ioEx;
                // 如果抛出Exception，表示文件被占用，跳出循环并返回true
                return true;
            }

            return false; // 文件夹内部的所有文件均未被占用
        }
        #endregion

        #region Initialize Configurations
        private void CREATE_DEFAULT_CONFIG(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                try
                {
                    File.WriteAllText(configFilePath, string.Empty);
                }

                catch (Exception unknownEx)
                {
                    ERROR_EXCEPTION_MESSAGE(unknownEx);
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

            XElement defaultConfig;

            if (supportedLanguages.Contains(currentCulture.Name))
            {
                if (currentLanguage != null)
                {
                    defaultConfig = new XElement("Configuration",
                        new XElement("Language", $"{currentLanguage}"),
                        new XElement("ScreenWidth", "0"),
                        new XElement("ScreenHeight", "0"),
                        new XElement("SystemScale", "0"),
                        new XElement("LocationX", $"{newLocationX}"),
                        new XElement("LocationY", $"{newLocationY}"),
                        new XElement("PartSize", "2000"),
                        new XElement("Format", "7z"),
                        new XElement("Password", ""),
                        new XElement("Zstd", "True"),
                        new XElement("DisableSplit", "False"),
                        new XElement("CreateMD5", "True"),
                        new XElement("AutoSave", "True"),
                        new XElement("Portable", "True"),
                        new XElement("SevenZUsageCount", "0")
                    );
                }

                else
                {
                    defaultConfig = new XElement("Configuration",
                        new XElement("Language", $"{currentCulture.Name}"),
                        new XElement("ScreenWidth", "0"),
                        new XElement("ScreenHeight", "0"),
                        new XElement("SystemScale", "0"),
                        new XElement("LocationX", $"{newLocationX}"),
                        new XElement("LocationY", $"{newLocationY}"),
                        new XElement("PartSize", "2000"),
                        new XElement("Format", "7z"),
                        new XElement("Password", ""),
                        new XElement("Zstd", "True"),
                        new XElement("DisableSplit", "False"),
                        new XElement("CreateMD5", "True"),
                        new XElement("AutoSave", "True"),
                        new XElement("Portable", "True"),
                        new XElement("SevenZUsageCount", "0")
                    );
                }
            }

            else
            {
                defaultConfig = new XElement("Configuration",
                    new XElement("Language", "en-US"),
                    new XElement("ScreenWidth", "0"),
                    new XElement("ScreenHeight", "0"),
                    new XElement("SystemScale", "0"),
                    new XElement("LocationX", $"{newLocationX}"),
                    new XElement("LocationY", $"{newLocationY}"),
                    new XElement("PartSize", "2000"),
                    new XElement("Format", "7z"),
                    new XElement("Password", ""),
                    new XElement("Zstd", "True"),
                    new XElement("DisableSplit", "False"),
                    new XElement("CreateMD5", "True"),
                    new XElement("AutoSave", "True"),
                    new XElement("Portable", "True"),
                    new XElement("SevenZUsageCount", "0")
                );
            }

            try
            {
                defaultConfig.Save(configFilePath);
            }
            catch (Exception unknownEx)
            {
                ERROR_EXCEPTION_MESSAGE(unknownEx);
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

            const float epsilon = 0.00001f; // 定义一个浮点值的误差范围

            if (Math.Abs(scaleToFloat - systemScale) < epsilon)
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

            var locationXValue = locationXNode.Value;
            int locationXToInt;

            if (string.IsNullOrEmpty(locationXValue))
            {
                return newLocationX;
            }

            if (!int.TryParse(locationXValue, out locationXToInt))
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

            var locationYValue = locationYNode.Value;
            int locationYToInt;

            if (string.IsNullOrEmpty(locationYValue))
            {
                return newLocationY;
            }

            if (!int.TryParse(locationYValue, out locationYToInt))
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

                return "2000";
            }

            var partSizeNode = xdoc.Descendants("PartSize").FirstOrDefault();

            if (partSizeNode == null)
            {
                XElement newNode = new XElement("PartSize", "2000");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return "2000";
            }

            var partSizeValue = partSizeNode.Value;
            int partSizeToInt;

            if (string.IsNullOrEmpty(partSizeValue))
            {
                return "2000";
            }

            if (!int.TryParse(partSizeValue, out partSizeToInt))
            {
                return "2000";
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
                "tar"
            };

            if (formatNode == null)
            {
                XElement newNode = new XElement("Format", "7z");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return "7z";
            }

            var formatValue = formatNode.Value;

            if (string.IsNullOrEmpty(formatValue))
            {
                return "7z";
            }

            if (!supportedFormat.Contains(formatValue))
            {
                return "7z";
            }

            return formatValue;
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

            var passwordValue = passwordNode.Value;

            if (string.IsNullOrEmpty(passwordValue))
            {
                return null;
            }

            return passwordValue;
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

            var zstdValue = zstdNode.Value;
            bool zstdToBool;

            if (string.IsNullOrEmpty(zstdValue))
            {
                return true;
            }

            if (!supportedBool.Contains(zstdValue))
            {
                return true;
            }

            if (!bool.TryParse(zstdValue, out zstdToBool))
            {
                return true;
            }

            return zstdToBool;
        }

        private bool GET_DISABLE_SPLIT(string configFilePath)
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

            var disableSplitNode = xdoc.Descendants("DisableSplit").FirstOrDefault();

            var supportedBool = new HashSet<string>
            {
                "True",
                "False"
            };

            if (disableSplitNode == null)
            {
                XElement newNode = new XElement("DisableSplit", "False");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return false;
            }

            var disableSplitValue = disableSplitNode.Value;
            bool disableSplitToBool;

            if (string.IsNullOrEmpty(disableSplitValue))
            {
                return false;
            }

            if (!supportedBool.Contains(disableSplitValue))
            {
                return false;
            }

            if (!bool.TryParse(disableSplitValue, out disableSplitToBool))
            {
                return false;
            }

            return disableSplitToBool;
        }

        private bool GET_CREATE_MD5(string configFilePath)
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

            var createMd5Node = xdoc.Descendants("CreateMD5").FirstOrDefault();

            var supportedBool = new HashSet<string>
            {
                "True",
                "False"
            };

            if (createMd5Node == null)
            {
                XElement newNode = new XElement("CreateMD5", "True");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return true;
            }

            var createMd5Value = createMd5Node.Value;
            bool createMd5ToBool;

            if (string.IsNullOrEmpty(createMd5Value))
            {
                return true;
            }

            if (!supportedBool.Contains(createMd5Value))
            {
                return true;
            }

            if (!bool.TryParse(createMd5Value, out createMd5ToBool))
            {
                return true;
            }

            return createMd5ToBool;
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

            var autoSaveValue = autoSaveNode.Value;
            bool autoSaveToBool;

            if (string.IsNullOrEmpty(autoSaveValue))
            {
                return true;
            }

            if (!supportedBool.Contains(autoSaveValue))
            {
                return true;
            }

            if (!bool.TryParse(autoSaveValue, out autoSaveToBool))
            {
                return true;
            }

            return autoSaveToBool;
        }

        private bool GET_PORTABLE(string configFilePath)
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

            var portableNode = xdoc.Descendants("Portable").FirstOrDefault();

            var supportedBool = new HashSet<string>
            {
                "True",
                "False"
            };

            if (portableNode == null)
            {
                XElement newNode = new XElement("Portable", "True");

                xdoc.Root.Add(newNode);

                xdoc.Save(configFilePath);

                return true;
            }

            var portableValue = portableNode.Value;
            bool portableToBool;

            if (string.IsNullOrEmpty(portableValue))
            {
                return true;
            }

            if (!supportedBool.Contains(portableValue))
            {
                return true;
            }

            if (!bool.TryParse(portableValue, out portableToBool))
            {
                return true;
            }

            return portableToBool;
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

            var sevenZUsageCountValue = sevenZUsageCountNode.Value;
            long sevenZUsageCountToLong;

            if (string.IsNullOrEmpty(sevenZUsageCountValue))
            {
                return 0;
            }

            if (!long.TryParse(sevenZUsageCountValue, out sevenZUsageCountToLong))
            {
                return 0;
            }

            if (sevenZUsageCountToLong < 0)
            {
                return 0;
            }

            return sevenZUsageCountToLong;
        }
        #endregion

        #region Initialize UI Contents
        private void DEFAULT_PARTSIZE_TEXTBOX()
        {
            TextBoxSize.Text = partSize.ToString();
        }

        private void DEFAULT_FORMAT_MENU()
        {
            ComboBoxFormat.Items.Add("7z");
            ComboBoxFormat.Items.Add("zip");
            ComboBoxFormat.Items.Add("tar");

            if (format == "7z")
            {
                ComboBoxFormat.SelectedIndex = 0;
            }

            if (format == "zip")
            {
                ComboBoxFormat.SelectedIndex = 1;
            }

            if (format == "tar")
            {
                ComboBoxFormat.SelectedIndex = 2;
            }
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

        private void DEFAULT_OPTION_MENU_DISABLE_SPLIT()
        {
            OptionMenuDisableSplit.Checked = disableSplit;
        }

        private void DEFAULT_OPTION_MENU_CREATE_MD5()
        {
            OptionMenuCreateMD5.Checked = createMd5;
        }

        private void DEFAULT_ZSTD()
        {
            CheckBoxZstd.Checked = zstd;
        }

        private void DEFAULT_AUTOSAVE()
        {
            CheckBoxAutoSave.Checked = autoSave;
        }
        #endregion

        #region Config Save Functions
        private void SAVE_CONFIG()
        {
            CHECK_XML_LEGAL(xmlPath);
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
            UPDATE_CONFIG($"{xmlPath}", "DisableSplit", $"{disableSplit}");
            UPDATE_CONFIG($"{xmlPath}", "CreateMD5", $"{createMd5}");
            UPDATE_CONFIG($"{xmlPath}", "AutoSave", $"{autoSave}");
            UPDATE_CONFIG($"{xmlPath}", "SevenZUsageCount", $"{sevenZUsageCount}");
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
        #endregion

        #region Message and Log
        private void WRITE_ERROR_LOG(string message, Exception error)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            CHECK_LOG_LEGAL(logFilePath);

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{nowTime}: {message},{error.Message}");
                writer.WriteLine();
            }
        }

        private void WRITE_MESSAGE_LOG(string message)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            CHECK_LOG_LEGAL(logFilePath);

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{nowTime}: {message}");
                writer.WriteLine();
            }
        }

        private void QUESTION_PACKED_IN_ONE_FILE()
        {
            DialogResult result = DialogResult.None;

            switch (currentLanguage)
            {
                case "zh-CN":
                    result = MessageBox.Show("检测到多个文件传入，是否需要将多个文件打包为单个压缩文件中？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    break;
                case "zh-TW":
                    result = MessageBox.Show("檢測到多個文件傳入，是否需要將多個文件打包為單個壓縮檔？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    break;
                case "en-US":
                    result = MessageBox.Show("Multiple files have been detected. Do you want to package them into a single compressed file?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    break;
            }

            if (result == DialogResult.Yes)
            {
                packedOneFile = true;
            }

            else
            {
                packedOneFile = false;
            }
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

        private void ERROR_APPLICATION_NO_PREMISSION(string directoryPath, Exception error)
        {
            if (currentLanguage == null)
            {
                var currentCulture = CultureInfo.CurrentUICulture;

                var supportedLanguages = new HashSet<string>
                {
                    "zh-CN", // 中文 (简体)
                    "zh-TW", // 中文 (繁体)
                    "en-US", // 英语 (美国)
                };

                if (supportedLanguages.Contains(currentCulture.Name))
                {
                    currentLanguage = currentCulture.Name;
                }

                else
                {
                    currentLanguage = "en-US";
                }
            }

            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show($"应用程序没有在{directoryPath}内进行读写的权限，错误为: {error.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "zh-TW":
                    MessageBox.Show($"應用程式沒有在{directoryPath}内進行讀寫的權限，錯誤為: {error.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "en-US":
                    MessageBox.Show($"The application does not have read/write permissions in {directoryPath}, the error is: {error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            string message = $"应用程序没有在{directoryPath}内进行读写的权限";
            WRITE_ERROR_LOG(message, error);
        }

        private void ERROR_RESOURCE_EXIST(string resourceName)
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show("没有找到资源: " + resourceName, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "zh-TW":
                    MessageBox.Show("沒有找到資源: " + resourceName, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "en-US":
                    MessageBox.Show("Resource not found: " + resourceName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            string message = $"没有找到资源:{resourceName}";
            WRITE_MESSAGE_LOG(message);
        }

        private void ERROR_FILE_MAYBE_NOT_EXIST(string incomingPath, string targetPath)
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show("传入路径中包含的文件可能不存在或者程序没有读写它的权限。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "zh-TW":
                    MessageBox.Show("傳入路徑中包含的文件可能不存在或者程式沒有讀寫它的權限。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "en-US":
                    MessageBox.Show("The file in the specified path may not exist, or the program may not have permission to read or write to it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            string message = $"传入文件路径为:{incomingPath},传出的目标路径为{targetPath},错误为:传入路径中包含的文件可能不存在或者程序没有读写它的权限。";
            WRITE_MESSAGE_LOG(message);
        }

        private void ERROR_TOO_LONG_PATH(string incomingPath)
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show("传入路径超过260个字符，无法处理。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "zh-TW":
                    MessageBox.Show("傳入路徑超過260個字符，無法處理。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "en-US":
                    MessageBox.Show("The path exceeds 260 characters and cannot be processed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            string message = $"传入文件路径为:{incomingPath},错误为:传入路径超过260个字符，无法处理。";
            WRITE_MESSAGE_LOG(message);
        }

        private void ERROR_EMPTY_SIZE(string name, string incomingPath)
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show($"没有获取到{name}的大小。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "zh-TW":
                    MessageBox.Show($"沒有獲取到{name}的大小。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "en-US":
                    MessageBox.Show($"The size of {name} could not be obtained.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            string message = $"没有获取到{name}的大小，传入的路径为{incomingPath}。";
            WRITE_MESSAGE_LOG(message);
        }

        private void WARNING_REMOVE_ELEMENT(string name, string incomingPath)
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show($"没有获取到{name}的大小，所以该文件并没有被添加到压缩文件中。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "zh-TW":
                    MessageBox.Show($"沒有獲取到{name}的大小，所以該文件并沒有被添加到壓縮檔中。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "en-US":
                    MessageBox.Show($"The size of {name} could not be obtained, so it has not been added to the compressed file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }

            string message = $"传入文件路径为:{incomingPath},错误为:没有获取到{name}的大小，所以该文件并没有被添加到压缩文件中。";
            WRITE_MESSAGE_LOG(message);
        }

        private void WRANING_FILE_IN_USE(string name, Exception error)
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show($"{name}被其他进程占用，所有该文件没有被添加到压缩任务中，错误为: {error.Message}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "zh-TW":
                    MessageBox.Show($"{name}被其他進程占用，所以該文件沒有被添加到壓縮任務中，錯誤為: {error.Message}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case "en-US":
                    MessageBox.Show($"The file {name} is being used by another process, and therefore it has not been added to the compression task. The error is: {error.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }

            string message = $"{name}被其他进程占用，所有该文件没有被添加到压缩任务中，错误为: {error.Message}";
            WRITE_ERROR_LOG(message, error);
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

            string message = $"创建目标文件夹时出错";
            WRITE_ERROR_LOG(message, error);
        }

        private void ERROR_FILE_IN_USE(Exception error)
        {
            switch (currentLanguage)
            {
                case "zh-CN":
                    MessageBox.Show($"选中文件中存在被其他进程占用的文件，压缩任务中止，错误为: {error.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "zh-TW":
                    MessageBox.Show($"選中文件中存在被其他進程占用的文件，壓縮任務中止，錯誤為: {error.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "en-US":
                    MessageBox.Show($"The selected file is currently in use by another process, and the compression task has been aborted. The error is: {error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            string message = $"选中文件中存在被其他进程占用的文件，压缩任务中止";
            WRITE_ERROR_LOG(message, error);
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

            string message = $"出现错误";
            WRITE_ERROR_LOG(message, error);
        }
        #endregion

        #region Initialize MainForm
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
            ButtonConfig.Location = new Point(MainPanel.Width - ButtonConfig.Width - (int)(10.0f * systemScale), MainPanel.Height - ButtonConfig.Height - (int)(10.0f * systemScale));
        }

        private void SET_MAINFORM_LOCATION()
        {
            const float epsilon = 0.00001f; // 定义一个浮点值的误差范围

            if (oldScreenWidth == Screen.PrimaryScreen.Bounds.Width && oldScreenHeight == Screen.PrimaryScreen.Bounds.Height && Math.Abs(oldSystemScale - systemScale) < epsilon)
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
        #endregion

        #region Initialize MainForm Components Function
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
            }

            else
            {
                zstd = false;
            }
        }

        private void OPTION_MENU_DISABLE_SPLIT_CHECKED_CHANGED(object sender, EventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var checkedIconName = "Auto7z_GUI.Resource.icon.Checked.png";
            var unCheckedIconName = "Auto7z_GUI.Resource.icon.Null.png";

            bool isDisable = OptionMenuDisableSplit.Checked;

            if (isDisable)
            {
                using (Stream stream = assembly.GetManifestResourceStream(checkedIconName))
                {
                    if (stream != null)
                    {
                        OptionMenuDisableSplit.Image = Image.FromStream(stream);
                    }
                    else
                    {
                        ERROR_RESOURCE_EXIST(checkedIconName);
                    }
                }

                disableSplit = true;
                TextBoxSize.Enabled = false;
            }

            else
            {
                using (Stream stream = assembly.GetManifestResourceStream(unCheckedIconName))
                {
                    if (stream != null)
                    {
                        OptionMenuDisableSplit.Image = Image.FromStream(stream);
                    }
                    else
                    {
                        ERROR_RESOURCE_EXIST(unCheckedIconName);
                    }
                }

                disableSplit = false;
                TextBoxSize.Enabled = true;
            }
        }

        private void OPTION_MENU_DISABLE_SPLIT_CLICK(object sender, EventArgs e)
        {
            ToolStripMenuItem disableSplit = sender as ToolStripMenuItem;

            if (disableSplit != null)
            {
                disableSplit.Checked = !disableSplit.Checked;
            }
        }

        private void OPTION_MENU_CREATE_MD5_CHECKED_CHANGED(object sender, EventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var checkedIconName = "Auto7z_GUI.Resource.icon.Checked.png";
            var unCheckedIconName = "Auto7z_GUI.Resource.icon.Null.png";

            bool isCreateMd5 = OptionMenuCreateMD5.Checked;

            if (isCreateMd5)
            {
                using (Stream stream = assembly.GetManifestResourceStream(checkedIconName))
                {
                    if (stream != null)
                    {
                        OptionMenuCreateMD5.Image = Image.FromStream(stream);
                    }
                    else
                    {
                        ERROR_RESOURCE_EXIST(checkedIconName);
                    }
                }

                createMd5 = true;
            }

            else
            {
                using (Stream stream = assembly.GetManifestResourceStream(unCheckedIconName))
                {
                    if (stream != null)
                    {
                        OptionMenuCreateMD5.Image = Image.FromStream(stream);
                    }
                    else
                    {
                        ERROR_RESOURCE_EXIST(unCheckedIconName);
                    }
                }

                createMd5 = false;
            }
        }

        private void OPTION_MENU_CREATE_MD5_CLICK(object sender, EventArgs e)
        {
            ToolStripMenuItem createMd5 = sender as ToolStripMenuItem;

            if (createMd5 != null)
            {
                createMd5.Checked = !createMd5.Checked;
            }
        }

        private void COMBOBOX_FORMAT_SELECTED_INDEX_CHANGED_UI(object sender, EventArgs e)
        {
            if (format=="tar")
            {
                TextBoxPassword.Enabled = false;
                CheckBoxZstd.Visible = true;
                
                if (zstd)
                {
                    CheckBoxZstd.Checked = true;
                }
            }

            else
            {
                CheckBoxZstd.Visible = false;
                TextBoxPassword.Enabled = true;
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
        #endregion

        #region Initialize MainForm Function
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

            if (files.Length > 0)
            {
                if (files.Length > 1)
                {
                    QUESTION_PACKED_IN_ONE_FILE();
                    isHandleSeparately = !packedOneFile;
                }

                else
                {
                    isHandleSeparately = false;
                }
            }

            if (files != null && files.Length > 0)
            {
                if (!isHandleSeparately)
                {
                    filePaths = !packedOneFile ? new string[] { Path.GetFullPath(files[0]) } : files;
                    fileName = Path.GetFileNameWithoutExtension(Path.GetFullPath(files[0]));
                    directoryPath = Path.GetDirectoryName(Path.GetFullPath(files[0]));

                    bool isFileInUse = IS_FILE_IN_USE(filePaths, out Exception ex);

                    if (isFileInUse)
                    {
                        ERROR_FILE_IN_USE(ex);
                        return;
                    }

                    bool getSizeSuccess = !packedOneFile ? GET_SINGLE_FILE_SIZE(filePaths[0]):GET_MULTIPLE_FILES_SIZE(filePaths);

                    if (!getSizeSuccess)
                    {
                        PIN_MAINFORM();
                        return;
                    }

                    if (format == "7z" || format == "zip")
                    {
                        if (!CHECK_SEVENZ_EXIST())
                        {
                            CREATE_COMPONENTS();
                            if (!isCreate7zFolder)
                            {
                                PIN_MAINFORM();
                                return;
                            }
                        }

                        string command = GENERATE_COMMAND(filePaths);

                        if (command == null)
                        {
                            PIN_MAINFORM();
                            return;
                        }

                        bool finished = await Task.Run(() => EXECUTE_COMMAND_BOOL(command));

                        if (!finished)
                        {
                            PIN_MAINFORM();
                            DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                        }

                        if (createMd5 && Directory.Exists(newFolderPath))
                        {
                            await Task.Run(() => GENERATE_MD5(md5CalculaterPath, newFolderPath));
                        }

                        PIN_MAINFORM();
                        ADD_SEVENZ_USAGE_COUNT();
                    }

                    if (format == "tar")
                    {
                        if (!CHECK_SEVENZ_EXIST())
                        {
                            CREATE_COMPONENTS();
                            if (!isCreate7zFolder)
                            {
                                PIN_MAINFORM();
                                return;
                            }
                        }

                        string command = GENERATE_COMMAND(filePaths);

                        if (command == null)
                        {
                            PIN_MAINFORM();
                            return;
                        }

                        bool tarFinished = await Task.Run(() => EXECUTE_COMMAND_BOOL(command));

                        if (!tarFinished)
                        {
                            PIN_MAINFORM();
                            DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                        }

                        if (CheckBoxZstd.Checked)
                        {
                            string zstdCommand = ZSTD_COMMAND();
                            bool zstdFinished = await Task.Run(() => EXECUTE_COMMAND_BOOL(zstdCommand));

                            if (!zstdFinished)
                            {
                                PIN_MAINFORM();
                                DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                            }
                            else
                            {
                                DELETE_TEMP_TAR(newFolderPath);
                            }
                        }

                        if (createMd5 && Directory.Exists(newFolderPath))
                        {
                            await Task.Run(() => GENERATE_MD5(md5CalculaterPath, newFolderPath));
                        }

                        PIN_MAINFORM();
                        ADD_SEVENZ_USAGE_COUNT();
                    }
                }

                else
                {
                    foreach (var singleFilePath in files)
                    {
                        filePath = Path.GetFullPath(singleFilePath);
                        fileName = Path.GetFileNameWithoutExtension(filePath);
                        directoryPath = Path.GetDirectoryName(filePath);

                        bool isFileInUse = IS_FILE_IN_USE(new string[] { filePath }, out Exception ex);

                        if (isFileInUse)
                        {
                            WRANING_FILE_IN_USE(fileName, ex);
                            return;
                        }

                        bool getSizeSuccess = GET_SINGLE_FILE_SIZE(filePath);

                        if (!getSizeSuccess)
                        {
                            return;
                        }

                        if (format == "7z" || format == "zip")
                        {
                            if (!CHECK_SEVENZ_EXIST())
                            {
                                CREATE_COMPONENTS();
                                if (!isCreate7zFolder)
                                {
                                    break;
                                }
                            }

                            string command = GENERATE_COMMAND(new string[] { filePath });

                            bool finished = await Task.Run(() => EXECUTE_COMMAND_BOOL(command));

                            if (!finished)
                            {
                                DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                            }

                            if (createMd5 && Directory.Exists(newFolderPath))
                            {
                                await Task.Run(() => GENERATE_MD5(md5CalculaterPath, newFolderPath));
                            }

                            ADD_SEVENZ_USAGE_COUNT();
                        }

                        if (format == "tar")
                        {
                            if (!CHECK_SEVENZ_EXIST())
                            {
                                CREATE_COMPONENTS();
                                if (!isCreate7zFolder)
                                {
                                    break;
                                }
                            }

                            string command = GENERATE_COMMAND(new string[] { filePath });

                            bool tarFinished = await Task.Run(() => EXECUTE_COMMAND_BOOL(command));

                            if (!tarFinished)
                            {
                                DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                            }

                            if (CheckBoxZstd.Checked)
                            {
                                string zstdCommand = ZSTD_COMMAND();
                                bool zstdFinished = await Task.Run(() => EXECUTE_COMMAND_BOOL(zstdCommand));

                                if (!zstdFinished)
                                {
                                    DELETE_FILES_AND_FOLDER_WHILE_UNFINISHED();
                                }
                                else
                                {
                                    DELETE_TEMP_TAR(newFolderPath);
                                }
                            }

                            if (createMd5 && Directory.Exists(newFolderPath))
                            {
                                await Task.Run(() => GENERATE_MD5(md5CalculaterPath, newFolderPath));
                            }

                            ADD_SEVENZ_USAGE_COUNT();
                        }
                    }

                    PIN_MAINFORM();
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

        private void PIN_MAINFORM()
        {
            this.TopMost = true;
            this.TopMost = false;
        }

        private void MAINFORM_FORM_CLOSING(object sender, FormClosingEventArgs e)
        {
            DELETE_EXTRACT_RESOURCE();

            if (!CheckBoxAutoSave.Checked)
            {
                UPDATE_CONFIG($"{xmlPath}", "AutoSave", "False");
            }

            else
            {
                SAVE_CONFIG();
            }

            SAVE_LOCATION();
        }
        #endregion
    }
}
