namespace CS.ModelAction
{
    using System;
    using Lson.Skill;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class ModelActionEditor
    {
        private readonly Dictionary<ActionSourceType, string> _actionSources = new Dictionary<ActionSourceType, string>
        {
            {ActionSourceType.SelfModel, "自己" },
            {ActionSourceType.OtherModel, "其他模型" },
        };
        private ModelAction _modelAction;
        private bool _isOverride = false;
        private bool _hasClip = true;
        private bool _isSkillAction = false;

        public ModelActionEditor() { }
        public ModelActionEditor(ModelAction modelAction, bool isSkill)
        {
            _modelAction = modelAction;
            _isSkillAction = isSkill;
        }
        [HideInInspector]
        public bool IsSkillAction { get { return _isSkillAction; } }

        [Toggle(""), HideLabel]
        public bool IsSelected { get; set; }
        [HideLabel]
        public string Name { get { return _modelAction.ActionFile; } }

        [ShowIf("_isOverride"), GUIColor(1, 0, 0)]
        public string Override { get { return "重写"; } }
        [HideIf("_isOverride"), GUIColor(1, 0.76f, 0.16f)]
        public string Inherit { get { return "继承"; } }
        public string ActionSource
        {
            get
            {
                return _actionSources[_modelAction.ActionSource];
            }
            set
            {
                _modelAction.ActionSource = _actionSources.GetKey(value);
            }
        }


        [Button("加载", ButtonSizes.Small)]
        public void LoadTimeline()
        {

        }
        [Button("修改", ButtonSizes.Small)]
        public void Modifier()
        {

        }
        [Button("删除", ButtonSizes.Small)]
        public void Delete()
        {

        }
        public void Refresh()
        {
            _isOverride = false;//重写必须修改动作
            _hasClip = true;//检查动作文件是否存在,Unity内部检查
        }
    }
}
