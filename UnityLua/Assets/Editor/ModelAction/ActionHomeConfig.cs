namespace CS.ModelAction
{
    using Csv;
    using System;
    using UnityEngine;
    using UnityEditor;
    using Sirenix.Utilities;
    using Sirenix.OdinInspector;
    using Sirenix.Utilities.Editor;
    using System.Collections.Generic;
    using Lson.Skill;
    using System.IO;

    [GlobalConfig("Editor/ModelAction/Config", UseAsset = true)]
    internal class ActionHomeConfig : GlobalConfig<ActionHomeConfig>
    {
        /// <summary>
        /// 分类名称
        /// </summary>
        public static readonly Dictionary<GroupType, string> MenuItems = new Dictionary<GroupType, string>()
        {
            {GroupType.None, "主页" },
            {GroupType.Player, "玩家" },
            {GroupType.Monster, "怪物" },
            {GroupType.NPC, "NPC" },
        };
        public static string[] MenuItemNames = { "主页", "玩家", "怪物", "NPC" };

        [FolderPath(AbsolutePath = true, RequireValidPath = true), BoxGroup("Config", showLabel: false)]
        [ShowInInspector, LabelText("配置存储目录"), PropertyOrder(-100)]
        /// <summary>
        /// 路径相对于Asset
        /// </summary>
        public string ActionConfigPath { get { return Application.dataPath + "/../../Csv/Skill/ActionConfig/"; } }
        [FolderPath(RequireValidPath = true), BoxGroup("Config", showLabel: false)]
        [LabelText("Csv存储目录"), PropertyOrder(-99)]
        public string ConfigRelativeDir = "";
        public Dictionary<string, string> CheckResults = new Dictionary<string, string>();
        public Dictionary<GroupType, List<ModelActionConfigEditor>> ModelGroupDict = new Dictionary<GroupType, List<ModelActionConfigEditor>>();
    }

    [Serializable]
    internal class HomeConfigPreview
    {
        /// <summary>
        /// 剪切板
        /// </summary>
        private List<ModelActionEditor> _clipboard = new List<ModelActionEditor>();
        private ActionHomeConfig _config;
        private static HomeConfigPreview _instance;

        public static HomeConfigPreview Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new HomeConfigPreview();
                return _instance;
            }
        }

        public void Init()
        {
            _config = ActionHomeConfig.Instance;
            LoadAll();
        }

        [FolderPath(AbsolutePath = true, RequireValidPath = true), BoxGroup("Config", showLabel: false)]
        [ShowInInspector, ReadOnly, LabelText("配置存储目录"), PropertyOrder(-100)]
        /// <summary>
        /// 路径相对于Asset
        /// </summary>
        public string ActionConfigPath { get { return _config.ActionConfigPath; } }
        [FolderPath(AbsolutePath = true, RequireValidPath = true), BoxGroup("Config", showLabel: false)]
        [ShowInInspector, ReadOnly, LabelText("Csv存储目录"), PropertyOrder(-99)]
        public string ConfigDir { get { return string.Format("{0}/../{1}/", Application.dataPath, _config.ConfigRelativeDir); } }

        public Dictionary<GroupType, List<ModelActionConfigEditor>> ModelGroupDict { get { return _config.ModelGroupDict; } }


        [ButtonGroup("Config/Btns")]
        [Button("加载所有动作", ButtonSizes.Large)]
        public void LoadAll()
        {
            ClearAll();
            CfgManager.ConfigDir = ConfigDir;
            CfgManager.LoadAll();
            string[] files = Directory.GetFiles(ActionConfigPath, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (var path in files)
            {
                AddModel(new ModelActionConfigEditor(path));
            }

            Debug.Log("加载所有动作 完毕!");
        }
        [ButtonGroup("Config/Btns")]
        [Button("保存所有配置", ButtonSizes.Large)]
        public void SaveAll()
        {
            foreach (var group in ModelGroupDict)
            {
                float count = 0;
                foreach (var cfg in group.Value)
                {
                    if (group.Key == GroupType.None)
                        cfg.Delete();
                    else
                        cfg.Save();
                    EditorUtility.DisplayProgressBar("导出所有配置>" + cfg.Group, cfg.Path, count / group.Value.Count);
                }
            }
            EditorUtility.ClearProgressBar();
            Debug.Log("所有配置保存完毕~~");
        }

        /// <summary>
        /// 检查配置
        /// </summary>
        [OnInspectorGUI, Title("动作配置校验结果", bold: true)]
        public void CheckError()
        {
            for (int i = 0; i < 5; i++)
            {

                SirenixEditorGUI.ErrorMessageBox("OnInspectorGUI Message Error");
            }
        }

        public void AddModel(ModelActionConfigEditor model)
        {
            if (ModelGroupDict.ContainsKey(model.GroupType))
                ModelGroupDict[model.GroupType].Add(model);
            else
            {
                ModelGroupDict.Add(model.GroupType, new List<ModelActionConfigEditor>());
                ModelGroupDict[model.GroupType].Add(model);
            }
        }
        public void RemoveModel(ModelActionConfigEditor model)
        {
            if (!ModelGroupDict[model.GroupType].Remove(model))
                Debug.LogErrorFormat("{0} 无法从分组中移除", model.MenuItemName);
        }

        public void ClearTemp()
        {

        }
        public void UpdateClipbord(List<ModelActionEditor> editors)
        {
            _clipboard = editors;
        }
        public void Destroy()
        {
            //保存所有配置
            SaveAll();
            ClearAll();

            _instance = null;
        }

        private void ClearAll()
        {
            ModelGroupDict.Clear();
            CfgManager.Clear();
            _clipboard.Clear();
        }
    }
}