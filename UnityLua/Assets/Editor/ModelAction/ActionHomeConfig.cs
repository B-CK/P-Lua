namespace CS.ModelAction
{
    using Csv;
    using UnityEngine;
    using Sirenix.Utilities;
    using Sirenix.OdinInspector;
    using Sirenix.Utilities.Editor;
    using System.Collections.Generic;
    using Lson.Skill;

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

        [FolderPath(AbsolutePath = true, RequireValidPath = true), BoxGroup("Config", showLabel: false)]
        [ShowInInspector, ReadOnly, LabelText("配置存储目录"), PropertyOrder(-100)]
        /// <summary>
        /// 路径相对于Asset
        /// </summary>
        public string ActionConfigPath { get { return Application.dataPath + "/../../Csv/Skill/ActionConfig/"; } }
        public Dictionary<GroupType, List<ModelActionEditor>> ModelGroupDict { get { return _modelGroupDict; } }
        public Dictionary<string, string> CheckResults = new Dictionary<string, string>();

        /// <summary>
        /// 剪切板
        /// </summary>
        private List<ModelActionEditor> _clipboard = new List<ModelActionEditor>();
        private Dictionary<GroupType, List<ModelActionEditor>> _modelGroupDict = new Dictionary<GroupType, List<ModelActionEditor>>();


        //1.加载所需配置                  --不在主页显示
        //2.显示配置错误信息              --在主页显示
        //3.创建配置
        //4.删除配置
        //5.检查配置

        [ButtonGroup("Config/Btns")]
        [Button("加载所有动作", ButtonSizes.Large)]
        /// <summary>
        /// 加载配置
        /// </summary>
        public void LoadAll()
        {
            ClearAll();
            CfgManager.ConfigDir = Application.dataPath + "/../../GamePlayer/Config/";
            CfgManager.LoadAll();
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
        /// 清除Xml加载数据
        /// </summary>
        public void ClearAll()
        {

        }
        public void ClearTemp()
        {

        }

        public void UpdateClipbord(List<ModelActionEditor> editors)
        {
            _clipboard = editors;
        }
    }
}