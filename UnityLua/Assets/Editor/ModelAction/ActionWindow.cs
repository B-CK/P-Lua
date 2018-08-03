namespace CS.ModelAction
{
    using System;
    using UnityEngine;
    using UnityEditor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;

    internal class ActionWindow : EditorWindow
    {
        public static void Init(ModelActionConfigEditor modelName, ModelActionEditor modelAction, Action<ModelActionEditor> result = null)
        {
            var window = GetWindow<ActionWindow>(true, "行为配置窗口", true);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(400, 250);
            window.minSize = new Vector2(400, 250);
            window.maxSize = new Vector2(400, 250);

            window._modelEditor = modelName;
            window._nameStyle = SirenixGUIStyles.BoldTitleCentered;
            window._nameStyle.fontSize = 30;
            window._nameStyle.fontStyle = FontStyle.Bold;
            window._nameStyle.normal.textColor = Color.white;

            window._modelAction = modelAction;
            window._actionName = modelAction.ActionName;
            window._isFromOther = modelAction.IsFromOther;
            window._actClipList = modelAction.GetActionClips();
            window._clipIndex = -1;
            int length = window._actClipList.Length;
            for (int i = 0; i < length; i++)
            {
                string item = window._actClipList[i];
                if (item.Equals(modelAction.ActionClip))
                {
                    window._clipIndex = i;
                    break;
                }
            }
            window._otherModel = string.Empty;
            window._result = result;
        }

        readonly Color _color = new Color(0.3f, 0.8f, 0.8f);

        ModelActionConfigEditor _modelEditor;
        GUIStyle _nameStyle;
        Action<ModelActionEditor> _result;

        ModelActionEditor _modelAction;
        string _actionName;
        bool _isFromOther;
        string[] _actClipList;
        int _clipIndex;
        string _otherModel = string.Empty;
        void OnGUI()
        {
            SirenixEditorGUI.BeginBox();
            {
                SirenixEditorGUI.BeginBoxHeader();
                GUILayout.Label(_modelEditor.ModelName, _nameStyle);
                SirenixEditorGUI.EndBoxHeader();
                switch (_modelAction.ActState)
                {
                    case ModelActionEditor.ActionState.New:
                        _actionName = EditorGUILayout.TextField("动作行为", _actionName);
                        break;
                    case ModelActionEditor.ActionState.Inherit:
                    case ModelActionEditor.ActionState.Override:
                        GUILayout.Label("动作行为");
                        GUILayout.Label(_actionName, EditorStyles.textField, GUILayout.Width(EditorGUIUtility.fieldWidth));
                        break;
                }

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("动画来源", GUILayout.Width(145));
                    if (GUILayout.Button("自己", _isFromOther ? SirenixGUIStyles.ButtonLeft : SirenixGUIStyles.ButtonLeftSelected))
                        _isFromOther = false;
                    if (GUILayout.Button("其他", _isFromOther ? SirenixGUIStyles.ButtonRightSelected : SirenixGUIStyles.ButtonRight))
                    {
                        _isFromOther = true;
                        _clipIndex = -1;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    _clipIndex = EditorGUILayout.Popup("动画文件", _clipIndex, _actClipList);
                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                        _clipIndex = -1;
                }
                EditorGUILayout.EndHorizontal();

                if (_isFromOther)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUIHelper.PushColor(_color);
                        GUILayout.Label("其他角色", SirenixGUIStyles.BoldLabel, GUILayout.Width(147));
                        GUILayout.Label(_otherModel, EditorStyles.textField);
                        if (GUILayout.Button("修改", GUILayout.Width(45)))
                        {
                            var list = HomeConfigPreview.Instance.GetAllModelList();
                            list.Remove(_modelEditor.ModelName);
                            SimplePopupCreator.ShowDialog(list, (name) => _otherModel = name);
                        }
                        if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                            _otherModel = string.Empty;
                        GUIHelper.PopColor();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("修改行为", GUILayout.Height(50)))
                    {
                        _modelAction.ActionName = _actionName;
                        _modelAction.IsFromOther = _isFromOther;
                        bool hasClip = !(_clipIndex < 0 || _actClipList.Length == 0);
                        _modelAction.ActionClip = hasClip ? _actClipList[_clipIndex] : string.Empty;
                        _modelAction.OtherModelName = _otherModel;

                        bool hasSame = false;
                        if (!_modelAction.IsSkillAction)
                        {
                            foreach (var item in _modelEditor.ModelActions)
                            {
                                if (item.ActionName.Equals(_actionName))
                                {
                                    hasSame = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in _modelEditor.SkillActions)
                            {
                                if (item.ActionName.Equals(_actionName))
                                {
                                    hasSame = true;
                                    break;
                                }
                            }
                        }
                        if (!hasSame)
                        {
                            if (_result != null) _result(_modelAction);
                            Close();
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("动作配置错误", "动作名重复,请重新配置!!!", "确定");
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();
        }

        void OnDestroy()
        {
            _modelAction = null;
            _modelEditor = null;
        }
    }
}