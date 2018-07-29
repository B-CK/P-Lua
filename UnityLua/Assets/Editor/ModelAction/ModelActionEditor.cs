namespace CS.ModelAction
{
    using System;
    using Lson.Skill;
    using Sirenix.OdinInspector;
    using Sirenix.Utilities.Editor;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [Serializable]
    public class ModelActionEditor
    {
        public const int INHERIT_WIDTH = 50;
        public const int ACT_NAME_WIDTH = 100;
        public const int ACT_SRC_WIDTH = 100;
        public const int ACT_CLIP_WIDTH = 100;

        private readonly Dictionary<ActionSourceType, string> _actionSources = new Dictionary<ActionSourceType, string>
        {
            {ActionSourceType.SelfModel, "自己" },
            {ActionSourceType.OtherModel, "其他模型" },
        };
        private ModelAction _modelAction;
        private bool _hasClip = true;
        private bool _isSkillAction = false;

        [OnInspectorGUI]
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            bool isInherit = IsInherit();
            GUIHelper.PushColor(isInherit ? Color.yellow : Color.red);
            GUILayout.Label(isInherit ? "重写" : "继承", SirenixGUIStyles.LabelCentered, GUILayout.Width(INHERIT_WIDTH));
            GUIHelper.PopColor();

            GUILayout.Label(Name, GUILayout.Width(ACT_NAME_WIDTH));
            GUIHelper.PushColor(Color.cyan);
            GUILayout.Label(ActionSource.ToString(), SirenixGUIStyles.LabelCentered, GUILayout.Width(ACT_SRC_WIDTH));
            GUIHelper.PopColor();

            bool isClipMiss = IsClipMiss();
            GUIHelper.PushColor(isClipMiss ? EditorStyles.label.onNormal.textColor : Color.red);
            GUILayout.Label(AnimationClip, SirenixGUIStyles.LabelCentered, GUILayout.Width(ACT_CLIP_WIDTH));
            GUIHelper.PopColor();

            GUIHelper.PushGUIPositionOffset(Vector2.up * -4f);
            if (GUILayout.Button("加载", SirenixGUIStyles.ButtonLeft))
            {
                LoadTimeline();
            }
            if (GUILayout.Button("修改", SirenixGUIStyles.ButtonRight))
            {
                Modifier();
            }
            bool isSelected = IsSelected;
            if (isSelected)
                GUIHelper.PushColor(Color.green);
            if (GUILayout.Button("选择", SirenixGUIStyles.Button))
            {
                IsSelected = !IsSelected;
            }
            if (isSelected)
                GUIHelper.PopColor();
            GUIHelper.PopGUIPositionOffset();

            EditorGUILayout.EndHorizontal();
        }
        private bool IsInherit()
        {
            return false;
        }
        private bool IsClipMiss()
        {
            return true;
        }
        private void LoadTimeline()
        {

        }
        private void Modifier()
        {

        }



        public ModelActionEditor() { }
        public ModelActionEditor(ModelAction modelAction, bool isSkill)
        {
            _modelAction = modelAction;
            _isSkillAction = isSkill;
        }
        public ModelAction ModelAction { get { return _modelAction; } }
        public string Name { get { return _modelAction.ActionFile; } set { _modelAction.ActionFile = value; } }
        public ActionSourceType ActionSource
        {
            get
            {
                return _modelAction.ActionSource;
            }
            set
            {
                _modelAction.ActionSource = value;
            }
        }
        public string AnimationClip { get { return _modelAction.ActionFile; } set { _modelAction.ActionFile = value; } }
        public bool IsSelected { get; set; }
        public bool IsSkillAction { get { return _isSkillAction; } }



        private string DrawName(string name, GUIContent content)
        {
            List<string> array = new List<string> { "跑", "站立", "攻击" };
            int index = array.FindIndex(a => a == name);
            index = EditorGUILayout.Popup("", 0, array.ToArray());
            return array[index];
        }

        public void Refresh()
        {
            _hasClip = true;//检查动作文件是否存在,Unity内部检查
        }

    }
}
