namespace CS.ModelAction
{
    using Lson.Skill;
    using UnityEngine;
    using UnityEditor;
    using System.Linq;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Collections.Generic;
    using Sirenix.OdinInspector.Editor;

    internal class ActionWindow : OdinMenuEditorWindow
    {
        [MenuItem("Window/Action Designer/Load Action")]
        private static void Open()
        {
            var window = GetWindow<ActionWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
            ActionHomeConfig.LoadInstanceIfAssetExists();
            ActionHomeConfig.Instance.LoadAll();
        }

        GUIContent _createConfig = new GUIContent("创建动作配置");

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: false)
            {
                { "主页", ActionHomeConfig.Instance, EditorIcons.House },
            };
            tree.Config.DrawSearchToolbar = true;
            foreach (var item in ActionHomeConfig.MenuItems)
            {
                if (item.Key != GroupType.None)
                    tree.Add(item.Value, null, EditorIcons.Table);
            }

            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            var selected = this.MenuTree.Selection.FirstOrDefault();
            var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;

            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                if (selected != null)
                {
                    GUILayout.Label(selected.Name);
                }

                if (SirenixEditorGUI.ToolbarButton(_createConfig))
                {
                    var configEditor = ModelActionConfigEditor.Create();
                    if (configEditor != null)
                        MenuTree.Add("主页/" + configEditor.Name, configEditor);
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Csv.CfgManager.Clear();
            EditorUtility.UnloadUnusedAssetsImmediate(true);
            ActionHomeConfig.Instance.ClearAll();
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }
}