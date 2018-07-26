namespace CS.ModelAction
{
    using System;
    using Lson.Skill;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;
    using Sirenix.OdinInspector.Editor;
    using System.IO;
    using System.Xml;

    [Serializable]
    internal class ModelActionConfigEditor
    {
        /// <summary>
        /// 创建角色动作行为配置文本
        /// 每个角色只能有一套配置
        /// </summary>
        /// <returns></returns>
        public static ModelActionConfigEditor Create()
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
                model = new ModelActionConfigEditor(config);
            });
            return model;
        }

        private ModelActionConfig _modelActionCfg;
        ModelActionConfigEditor(ModelActionConfig modelActionCfg)
        {
            _modelActionCfg = modelActionCfg;
        }

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
                //调整分组
                //TODO
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
