﻿namespace CS.ModelAction
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
            {GroupType.Base, "基础类型" },
            {GroupType.Player, "玩家" },
            {GroupType.Monster, "怪物" },
            {GroupType.NPC, "NPC" },
        };
        public static string[] MenuItemNames = { "主页", "基础类型", "玩家", "怪物", "NPC" };

        [FolderPath(RequireValidPath = true), BoxGroup("Config", showLabel: false)]
        [LabelText("Xml存储目录"), PropertyOrder(-100)]
        public string ActionConfigPath;
        [FolderPath(RequireValidPath = true), BoxGroup("Config", showLabel: false)]
        [LabelText("Csv存储目录"), PropertyOrder(-99)]
        public string ConfigRelativeDir = "";
        public Dictionary<string, string> CheckResults = new Dictionary<string, string>();
        public Dictionary<GroupType, List<ModelActionConfigEditor>> ModelGroupDict = new Dictionary<GroupType, List<ModelActionConfigEditor>>();
    }

    [Serializable]
    internal class HomeConfigPreview
    {
        private static HomeConfigPreview _instance;

        /// <summary>
        /// 剪切板
        /// </summary>
        private List<ModelActionEditor> _clipboard = new List<ModelActionEditor>();
        private Dictionary<string, ModelActionConfigEditor> _modelDict = new Dictionary<string, ModelActionConfigEditor>();
        private ActionHomeConfig _config;

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

        [FolderPath(RequireValidPath = true), BoxGroup("Config", showLabel: false)]
        [ShowInInspector, ReadOnly, LabelText("Xml存储目录"), PropertyOrder(-100)]
        public string ActionConfigPath { get { return string.Format("{0}/../{1}/", Application.dataPath, _config.ActionConfigPath); } }
        [FolderPath(RequireValidPath = true), BoxGroup("Config", showLabel: false)]
        [ShowInInspector, ReadOnly, LabelText("Csv存储目录"), PropertyOrder(-99)]
        public string ConfigDir { get { return string.Format("{0}/../{1}/", Application.dataPath, _config.ConfigRelativeDir); } }

        public Dictionary<GroupType, List<ModelActionConfigEditor>> ModelGroupDict
        {
            get
            {
                if (_config == null)
                    Init();
                return _config.ModelGroupDict;
            }
        }


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
            foreach (var item in _modelDict)
            {
                item.Value.Init();
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

        /// <summary>
        /// 创建角色动作行为配置文本
        /// 每个角色只能有一套配置
        /// </summary>
        /// <returns></returns>
        public void Create(Action<ModelActionConfigEditor> Result)
        {
            ModelActionConfigEditor model = null;
            var models = Csv.CfgManager.Model.Keys;
            SimplePopupCreator.ShowDialog(new List<string>(models), (name) =>
            {
                var config = new ModelActionConfig()
                {
                    ModelName = name,
                    GroupType = GroupType.None,
                };
                string path = string.Format("{0}/{1}.xml", ActionHomeConfig.Instance.ActionConfigPath, name);
                XmlUtil.Serialize(path, config);
                model = new ModelActionConfigEditor(path);
                if (Result != null) Result(model);
            });
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
            if (!_modelDict.ContainsKey(model.ModelName))
                _modelDict.Add(model.ModelName, model);
        }
        public void RemoveModel(ModelActionConfigEditor model)
        {
            if (!ModelGroupDict[model.GroupType].Remove(model))
                Debug.LogErrorFormat("{0} 无法从分组中移除", model.MenuItemName);
            if (_modelDict.ContainsKey(model.ModelName))
                _modelDict.Remove(model.ModelName);
        }
        public ModelActionConfigEditor GetModelEditor(string modelName)
        {
            return _modelDict[modelName];
        }
        public List<string> GetAllModelList()
        {
            return new List<string>(_modelDict.Keys);
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
            _modelDict.Clear();
        }
    }
}