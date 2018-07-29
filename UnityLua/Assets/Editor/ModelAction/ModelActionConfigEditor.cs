namespace CS.ModelAction
{
    using System;
    using System.IO;
    using Lson.Skill;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;
    using Sirenix.OdinInspector.Editor;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Utilities.Editor;

    [Serializable]
    public class ModelActionConfigEditor
    {
        /// <summary>
        /// 创建角色动作行为配置文本
        /// 每个角色只能有一套配置
        /// </summary>
        /// <returns></returns>
        public static void Create(Action<ModelActionConfigEditor> Result)
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
                string path = string.Format("{0}{1}.xml", ActionHomeConfig.Instance.ActionConfigPath, name);
                XmlUtil.Serialize(path, config);
                model = new ModelActionConfigEditor(path);
                if (Result != null) Result(model);
            });
        }
        public ModelActionConfigEditor() { }
        public ModelActionConfigEditor(string path)
        {
            _path = path;
            _modelActionCfg = XmlUtil.Deserialize(path, typeof(ModelActionConfig)) as ModelActionConfig;
            foreach (var item in _modelActionCfg.ModelActionList)
                ModelActions.Add(new ModelActionEditor(item, false));
            foreach (var item in _modelActionCfg.SkillActionList)
                SkillActions.Add(new ModelActionEditor(item, true));
        }


        private ModelActionConfig _modelActionCfg;
        private string _path;


        public string Path { get { return _path; } }
        public string MenuItemName { get { return string.Format("{0}/{1}", ActionHomeConfig.MenuItems[GroupType], Name); } }

        [BoxGroup("BaseGroup", showLabel: false, order: -100)]
        [VerticalGroup("BaseGroup/Info"), CustomValueDrawer("DrawGroupType")]
        public GroupType GroupType
        {
            get { return _modelActionCfg.GroupType; }
            set
            {
                ActionWindow window = ActionWindow.GetWindow<ActionWindow>();
                OdinMenuItem item = window.MenuTree.Selection.FirstOrDefault();
                if (item != null)
                {
                    ModelActionConfigEditor model = item.ObjectInstance as ModelActionConfigEditor;
                    HomeConfigPreview.Instance.RemoveModel(model);
                    item.Parent.ChildMenuItems.Remove(item);

                    _modelActionCfg.GroupType = value;
                    HomeConfigPreview.Instance.AddModel(model);
                    var group = window.MenuTree.GetMenuItem(Group);
                    group.ChildMenuItems.Add(item);
                    item.MenuTree.Selection.Clear();
                    item.Select();
                    item.MenuTree.UpdateMenuTree();
                    item.MenuTree.DrawMenuTree();
                }
            }
        }
        public string Group
        {
            get
            {
                return ActionHomeConfig.MenuItems[_modelActionCfg.GroupType];
            }
            //set
            //{
            //    if (value.Equals(ActionHomeConfig.MenuItems[_modelActionCfg.GroupType])) return;

            //    _modelActionCfg.GroupType = ActionHomeConfig.MenuItems.GetKey(value);

            //    //切换分组
            //    ActionWindow window = ActionWindow.GetWindow<ActionWindow>();
            //    OdinMenuItem item = window.MenuTree.Selection.FirstOrDefault();
            //    if (item != null)
            //    {
            //        item.Parent.ChildMenuItems.Remove(item);
            //        var groupItem = window.MenuTree.GetMenuItem(value);
            //        groupItem.ChildMenuItems.Add(item);
            //        item.Select();
            //    }
            //}
        }
        [VerticalGroup("BaseGroup/Info"), LabelText("模型名称"), InlineButton("ModifyName", "更换")]
        public string Name { get { return _modelActionCfg.ModelName; } set { } }
        [VerticalGroup("BaseGroup/Info"), LabelText("基础模型"), InlineButton("ModifyBaseName", "更换")]
        public string BaseName { get { return _modelActionCfg.BaseModelName; } set { } }



        [ButtonGroup("Button"), Button("复制", ButtonSizes.Large)]
        private void CopyActions()
        {

        }
        [ButtonGroup("Button"), Button("粘贴", ButtonSizes.Large)]
        private void PasteActions()
        {

        }
        [ButtonGroup("Button"), Button("保存", ButtonSizes.Large)]
        public void Save()
        {
            _modelActionCfg.ModelActionList.Clear();
            _modelActionCfg.SkillActionList.Clear();
            foreach (var item in ModelActions)
                _modelActionCfg.ModelActionList.Add(item.ModelAction);
            foreach (var item in SkillActions)
                _modelActionCfg.SkillActionList.Add(item.ModelAction as SkillAction);
            XmlUtil.Serialize(_path, _modelActionCfg);
        }
        [ButtonGroup("Button"), Button("删除", ButtonSizes.Large)]
        public void Delete()
        {
            if (EditorUtility.DisplayDialog("删除操作", "确定要删除文件 -> " + Name, "确定", "取消"))
            {
                if (File.Exists(_path))
                    File.Delete(_path);

                ActionWindow window = ActionWindow.GetWindow<ActionWindow>();
                OdinMenuItem item = window.MenuTree.Selection.FirstOrDefault();
                if (item != null)
                {
                    ModelActionConfigEditor model = item.ObjectInstance as ModelActionConfigEditor;
                    HomeConfigPreview.Instance.RemoveModel(model);
                    item.Parent.ChildMenuItems.Remove(item);
                    item.MenuTree.Selection.Clear();
                    item.Parent.Select();
                    item.MenuTree.UpdateMenuTree();
                    item.MenuTree.DrawMenuTree();
                }
            }
        }

        private static GroupType DrawGroupType(GroupType type, GUIContent content)
        {
            return (GroupType)EditorGUILayout.Popup(content.text, (int)type, ActionHomeConfig.MenuItemNames);
        }
        private void ModifyName()
        {
            var models = Csv.CfgManager.Model.Keys;
            SimplePopupCreator.ShowDialog(new List<string>(models), (name) =>
            {
                _modelActionCfg.ModelName = name;
            });
        }
        public void ModifyBaseName()
        {

        }

        [Title("普通动作列表", bold: true), OnInspectorGUI, PropertyOrder(10)]
        private void OnModelActionsHeader() { OnHeaderGUI(); }
        [LabelText(" "), Space(-5f), PropertyOrder(15)]
        [ListDrawerSettings(OnTitleBarGUI = "OnGUIModelAction", CustomAddFunction = "CreateModelAction")]
        public List<ModelActionEditor> ModelActions = new List<ModelActionEditor>();
        [Title("技能动作列表", bold: true), OnInspectorGUI, PropertyOrder(20)]
        private void OnSkillActionsHeader() { OnHeaderGUI(); }
        [LabelText(" "), Space(-5f), ListDrawerSettings(OnTitleBarGUI = "OnGUISkilllAction", CustomAddFunction = "CreateSkillAction"), PropertyOrder(25)]
        public List<ModelActionEditor> SkillActions = new List<ModelActionEditor>();

        bool _isSelected_Model = false;
        bool _isSelected_Skill = false;
        private void CreateModelAction()
        {
            ModelAction action = new ModelAction();
            ModelActions.Add(new ModelActionEditor(action, false));
        }
        private void CreateSkillAction()
        {
            SkillAction action = new SkillAction();
            SkillActions.Add(new ModelActionEditor(action, true));
        }
        private void OnGUIModelAction()
        {
            OnTitleBarGUI(ModelActions, ref _isSelected_Model);
        }
        private void OnGUISkilllAction()
        {
            OnTitleBarGUI(SkillActions, ref _isSelected_Skill);
        }
        private void OnHeaderGUI()
        {
            SirenixEditorGUI.BeginHorizontalToolbar(SirenixGUIStyles.BoxHeaderStyle);
            GUILayout.Label(" ", GUILayout.Width(17f));
            GUILayout.Label("继承", SirenixGUIStyles.LabelCentered, GUILayout.Width(ModelActionEditor.INHERIT_WIDTH));
            GUILayout.Label("动作名称", SirenixGUIStyles.LabelCentered, GUILayout.Width(ModelActionEditor.ACT_NAME_WIDTH));
            GUILayout.Label("动作来源", SirenixGUIStyles.LabelCentered, GUILayout.Width(ModelActionEditor.ACT_SRC_WIDTH));
            GUILayout.Label("动画文件", SirenixGUIStyles.LabelCentered, GUILayout.Width(ModelActionEditor.ACT_CLIP_WIDTH));
            GUILayout.Label("操作", SirenixGUIStyles.LabelCentered, GUILayout.ExpandWidth(true));
            SirenixEditorGUI.EndHorizontalToolbar();
        }
        private void OnTitleBarGUI(List<ModelActionEditor> actionEditors, ref bool state)
        {
            EditorIcon icon = state ? EditorIcons.X : EditorIcons.Checkmark;
            if (SirenixEditorGUI.ToolbarButton(icon))
            {
                state = !state;
                foreach (var item in actionEditors)
                    item.IsSelected = state;
            }
        }

    }
}
