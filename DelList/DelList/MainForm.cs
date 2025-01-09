using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DelList
{
    public partial class MainForm : Form
    {
        public static string currentLanguage;
        public string xmlPath;
        public string auto7zPath;

        public MainForm()
        {
            InitializeComponent();
            this.Hide(); // 隐藏窗口

            string workPath = GET_WORK_PATH(); // 获取程序路径
            string xml = @"Auto7z\\config.xml";
            xmlPath = Path.Combine(workPath, xml);

            string auto7z = @"Auto7z";
            auto7zPath = Path.Combine(workPath, auto7z);

            if (Directory.Exists(auto7zPath))
            {
                currentLanguage = GET_CURRENT_LANGUAGE(xmlPath);

                bool auto7zForFile = RemoveContextMenuItem(@"*\\shell\\auto7z");
                bool auto7zForFolder = RemoveContextMenuItem(@"Directory\\shell\\auto7z");

                if (auto7zForFile == true && auto7zForFolder == true)
                {
                    switch (currentLanguage)
                    {
                        case "zh-CN":
                            MessageBox.Show("右键菜单 <Auto7z> 已移除。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case "zh-TW":
                            MessageBox.Show("右鍵菜單 <Auto7z> 已移除。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case "en-US":
                            MessageBox.Show("Right-click menu <Auto7z> has been deleted.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                    }

                    this.Close();
                }

                else
                {
                    switch (currentLanguage)
                    {
                        case "zh-CN":
                            MessageBox.Show($"右键菜单 <Auto7z> 移除失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "zh-TW":
                            MessageBox.Show($"右鍵菜單 <Auto7z> 移除失敗。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "en-US":
                            MessageBox.Show($"Right-click menu <Auto7z> delete failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }

                    this.Close();
                }
            }

            else
            {
                string newXml = @"config.xml";
                xmlPath = Path.Combine(workPath, newXml);

                currentLanguage = GET_CURRENT_LANGUAGE(xmlPath);

                switch (currentLanguage)
                {
                    case "zh-CN":
                        MessageBox.Show("没有找到有关Auto7z程序的内容，任务终止。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "zh-TW":
                        MessageBox.Show("沒有找到有關Auto7z程序的内容，任務終止。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "en-US":
                        MessageBox.Show("No content found related to the Auto7z program, task terminated..", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }

                this.Close();
            }
        }

        private bool RemoveContextMenuItem(string keyPath)
        {
            try
            {
                // 打开注册表项
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64).OpenSubKey(keyPath, true);

                if (key != null)
                {
                    // 删除整个子项
                    key.DeleteSubKeyTree(""); // 请检查该方法里的字符串是否是有效的子项名称
                    key.Close(); // 确保关闭注册表项

                    return true; // 返回删除成功
                }

                else
                {
                    // 如果没有找到注册表项
                    return false; // 返回删除失败
                }
            }
            catch (UnauthorizedAccessException)
            {
                switch (currentLanguage)
                {
                    case "zh-CN":
                        MessageBox.Show("没有权限访问注册表。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "zh-TW":
                        MessageBox.Show("沒有權限訪問註冊表。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "en-US":
                        MessageBox.Show("No permission to access the registry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }

                return false; // 返回没有权限的错误
            }

            catch (Exception ex)
            {
                switch (currentLanguage)
                {
                    case "zh-CN":
                        MessageBox.Show($"出现错误: {ex.Message}。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "zh-TW":
                        MessageBox.Show($"出現錯誤: {ex.Message}。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "en-US":
                        MessageBox.Show($"An error occurred: {ex.Message}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }

                return false; // 返回其他错误
            }
        }


        static string GET_WORK_PATH()
        {
            // 获取当前执行程序集
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            // 返回路径
            return Path.GetDirectoryName(assemblyPath);
        }

        private void CREATE_DEFAULT_CONFIG(string configFilePath)
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
                // 创建默认的 XML 结构
                XElement defaultConfig = new XElement("Configuration",
                    new XElement("Language", $"{currentCulture.Name}")
                );

                defaultConfig.Save(configFilePath);
            }

            else
            {
                // 创建默认的 XML 结构
                XElement defaultConfig = new XElement("Configuration",
                    new XElement("Language", "en-US")
                );

                defaultConfig.Save(configFilePath);
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
    }
}
