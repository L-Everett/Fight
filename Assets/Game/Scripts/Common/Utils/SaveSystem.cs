using Newtonsoft.Json;
using System.IO;
using System.Text;
using UnityEngine;

public static class SaveSystem
{
    // 存档路径（与exe同级目录）
    public static string SAVE_FOLDER
    {
        get
        {
#if UNITY_EDITOR
            // 编辑器模式下，放在项目根目录的Saves文件夹
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Saves");
#else
            // 打包后，放在exe同级目录的Saves文件夹
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Saves");
#endif
        }
    }

    public static string SAVE_FILE => Path.Combine(SAVE_FOLDER, "save.dat");

    // 初始化系统
    static SaveSystem()
    {
        // 确保保存目录存在
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
            Debug.Log($"存档目录已创建: {SAVE_FOLDER}");
        }
    }

    // === 基础存档操作 ===
    public static void SaveGame()
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string jsonData = JsonConvert.SerializeObject(DataModel.Instance, settings);
            string encryptedData = EncryptData(jsonData);
            File.WriteAllText(SAVE_FILE, encryptedData, Encoding.UTF8);

            Debug.Log($"存档成功! 位置: {SAVE_FILE}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"存档失败: {e.Message}");
        }
    }

    public static bool LoadGame()
    {
        if (File.Exists(SAVE_FILE))
        {
            try
            {
                string encryptedData = File.ReadAllText(SAVE_FILE, Encoding.UTF8);
                string jsonData = DecryptData(encryptedData);

                // 使用Newtonsoft.Json反序列化
                var settings = new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };

                DataModel savedData = JsonConvert.DeserializeObject<DataModel>(jsonData, settings);
                DataModel.Instance.LoadGame(savedData);

                Debug.Log("存档加载成功!");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载存档失败: {e.Message}");
                // 失败时创建新存档
                DataModel.Instance.Init();
                return false;
            }
        }
        else
        {
            // 创建新存档
            DataModel.Instance.Init();
            Debug.Log("创建新存档");
            return false;
        }
    }

    // 清空存档
    public static void ClearAllSaves()
    {
        // 删除主存档文件
        if (File.Exists(SAVE_FILE))
        {
            File.Delete(SAVE_FILE);
        }

        // 删除多存档槽文件
        for (int i = 1; i <= 3; i++)
        {
            string slotFile = Path.Combine(SAVE_FOLDER, $"save_slot_{i}.dat");
            if (File.Exists(slotFile))
            {
                File.Delete(slotFile);
            }
        }

        Debug.Log("所有存档已清除!");
    }

    // 获取存档文件夹的完整路径（用于在资源管理器中打开）
    public static void OpenSaveFolder()
    {
        if (Directory.Exists(SAVE_FOLDER))
        {
            System.Diagnostics.Process.Start(SAVE_FOLDER);
        }
        else
        {
            Debug.LogWarning("存档目录不存在: " + SAVE_FOLDER);
        }
    }

    // 检查存档是否存在
    public static bool SaveExists()
    {
        return File.Exists(SAVE_FILE);
    }

    // 获取存档文件信息
    public static FileInfo GetSaveFileInfo()
    {
        if (File.Exists(SAVE_FILE))
        {
            return new FileInfo(SAVE_FILE);
        }
        return null;
    }

    // === 加密 ===
    public static string EncryptData(string data)
    {
        // 简单异或加密
        char[] chars = data.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = (char)(chars[i] ^ 0x55);
        }
        return new string(chars);
    }

    public static string DecryptData(string data)
    {
        // 同样使用异或解密（对称加密）
        return EncryptData(data);
    }
}