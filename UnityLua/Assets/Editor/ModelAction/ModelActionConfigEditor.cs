namespace CS.ModelAction
{
    using System;
    using System.IO;
    using Lson.Skill;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;
    using Sirenix.OdinInspector.Editor;
    using System.Linq;

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
        }


        private ModelActionConfig _modelActionCfg;
        private string _path;

        public void Save()
        {
            XmlUtil.Serialize(_path, _modelActionCfg);
        }
        public void Delete()
        {
            if (File.Exists(_path))
                File.Delete(_path);
        }

        public string Path { get { return _path; } }
        public GroupType GroupType { get { return _modelActionCfg.GroupType; } }

        [LabelText("模型分组类型")]
        public string Group
        {
            get
            {
                return ActionHomeConfig.MenuItems[_modelActionCfg.GroupType];
            }
            set
            {
                if (value.Equals(ActionHomeConfig.MenuItems[_modelActionCfg.GroupType])) return;

                _modelActionCfg.GroupType = ActionHomeConfig.MenuItems.GetKey(value);

                //切换分组
                ActionWindow window = ActionWindow.GetWindow<ActionWindow>();
                OdinMenuItem item = window.MenuTree.Selection.FirstOrDefault();
                if (item != null)
                {
                    item.Parent.ChildMenuItems.Remove(item);
                    var groupItem = window.MenuTree.GetMenuItem(value);
                    groupItem.ChildMenuItems.Add(item);
                    item.Select();
                }
            }
        }

        [LabelText("模型名称")]
        public string Name { get { return _modelActionCfg.ModelName; } }
        [HorizontalGroup("BaseNameGroup/Left"), LabelText("基础模型")]
        public string BaseName { get { return _modelActionCfg.BaseModelName; } }

        [LabelText("普通动作列表"), ListDrawerSettings()]
        public List<ModelActionEditor> ModelActions = new List<ModelActionEditor>();

        [LabelText("技能动作列表"), ListDrawerSettings()]
        public List<ModelActionEditor> SkillActions = new List<ModelActionEditor>();

        [HorizontalGroup("BaseNameGroup/Left"), Button("修改", ButtonSizes.Small)]
        public void ModifyBaseName()
        {

        }
        [ButtonGroup("BaseNameGroup/Right"), Button("复制", ButtonSizes.Small)]
        public void CopyActions()
        {

        }
        [ButtonGroup("BaseNameGroup/Right"), Button("粘贴", ButtonSizes.Small)]
        public void PasteActions()
        {

        }

        private ModelActionEditor CreateModelAction()
        {
            return null;
        }
        private ModelActionEditor CreateSkillAction()
        {
            return null;
        }
        private void DeleteAction()
        {

        }
    }
}
