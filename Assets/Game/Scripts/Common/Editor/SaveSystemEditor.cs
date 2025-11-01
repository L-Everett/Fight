#if UNITY_EDITOR
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class SaveSystemEditor : EditorWindow
{
    [MenuItem("存档系统/清除所有存档")]
    private static void ClearAllSaves()
    {
        bool confirm = EditorUtility.DisplayDialog(
            "清除存档确认",
            "确定要永久删除所有存档数据吗？此操作不可撤销！",
            "确定删除",
            "取消"
        );

        if (confirm)
        {
            SaveSystem.ClearAllSaves();
            EditorUtility.DisplayDialog("存档已清除", "所有存档数据已成功删除。", "确定");
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("存档系统/打开存档目录")]
    private static void OpenSaveFolder()
    {
        if (Directory.Exists(SaveSystem.SAVE_FOLDER))
        {
            System.Diagnostics.Process.Start(SaveSystem.SAVE_FOLDER);
        }
        else
        {
            EditorUtility.DisplayDialog("目录不存在", "存档目录尚未创建", "确定");
        }
    }

    [MenuItem("存档系统/备份当前存档")]
    private static void BackupSave()
    {
        string backupPath = EditorUtility.SaveFilePanel(
            "备份存档",
            Application.dataPath,
            "save_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"),
            "dat"
        );

        if (!string.IsNullOrEmpty(backupPath))
        {
            if (File.Exists(SaveSystem.SAVE_FILE))
            {
                File.Copy(SaveSystem.SAVE_FILE, backupPath, true);
                EditorUtility.DisplayDialog("备份成功", $"存档已备份到:\n{backupPath}", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("备份失败", "找不到存档文件", "确定");
            }
        }
    }

    [MenuItem("存档系统/查看存档信息")]
    private static void ShowSaveInfo()
    {
        if (File.Exists(SaveSystem.SAVE_FILE))
        {
            string encryptedData = File.ReadAllText(SaveSystem.SAVE_FILE, Encoding.UTF8);
            string jsonData = SaveSystem.DecryptData(encryptedData);

            // 使用Newtonsoft.Json反序列化
            var settings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };

            var data = JsonConvert.DeserializeObject<DataModel>(jsonData, settings);

            string info = $"存档信息:\n" +
                         $"游玩时间: {data.mPlayTime:F1}秒\n" +
                         $"无名剑客神赐等级: Lv.{data.mTalentLv["C001"]:F0}\n" +
                         $"流浪法师神赐等级: Lv.{data.mTalentLv["C002"]:F0}\n" +
                         $"特种战士神赐等级: Lv.{data.mTalentLv["C003"]:F0}\n" +
                         $"天赋石数量: {data.mGemCount}";

            EditorUtility.DisplayDialog("存档信息", info, "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("存档信息", "没有找到存档文件", "确定");
        }
    }

    [MenuItem("存档系统/创建测试存档")]
    private static void CreateTestSave()
    {
        //SaveSystem.GameData testData = new SaveSystem.GameData
        //{
        //    playerLevel = 5,
        //    playerCoins = 1000,
        //    playerHealth = 80,
        //    currentSceneIndex = 3,
        //    playTime = 3560.5f,
        //    isHardMode = true
        //};

        //testData.unlockedLevels.Add("level1", true);
        //testData.unlockedLevels.Add("level2", true);

        //testData.collectedItems.Add("sword");
        //testData.collectedItems.Add("shield");

        //string jsonData = JsonUtility.ToJson(testData);
        //string encryptedData = SaveSystem.EncryptData(jsonData);
        //File.WriteAllText(SaveSystem.SAVE_FILE, encryptedData);

        //EditorUtility.DisplayDialog("测试存档", "测试存档已创建", "确定");
    }

    [MenuItem("存档系统/存档管理器")]
    private static void ShowWindow()
    {
        var window = GetWindow<SaveSystemEditor>("存档管理器");
        window.minSize = new Vector2(300, 250);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("存档管理器", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 存档状态
        EditorGUILayout.LabelField("存档状态", EditorStyles.boldLabel);
        if (File.Exists(SaveSystem.SAVE_FILE))
        {
            EditorGUILayout.HelpBox("存档存在", MessageType.Info);

            FileInfo fileInfo = new FileInfo(SaveSystem.SAVE_FILE);
            EditorGUILayout.LabelField("最后修改时间", fileInfo.LastWriteTime.ToString());
            EditorGUILayout.LabelField("文件大小", $"{fileInfo.Length / 1024} KB");
        }
        else
        {
            EditorGUILayout.HelpBox("没有存档", MessageType.Warning);
        }

        EditorGUILayout.Space();

        // 操作按钮
        EditorGUILayout.LabelField("操作", EditorStyles.boldLabel);
        if (GUILayout.Button("清除所有存档", GUILayout.Height(30)))
        {
            ClearAllSaves();
        }

        if (GUILayout.Button("打开存档目录", GUILayout.Height(30)))
        {
            OpenSaveFolder();
        }

        if (GUILayout.Button("创建测试存档", GUILayout.Height(30)))
        {
            CreateTestSave();
        }

        if (GUILayout.Button("备份当前存档", GUILayout.Height(30)))
        {
            BackupSave();
        }
    }
}
#endif