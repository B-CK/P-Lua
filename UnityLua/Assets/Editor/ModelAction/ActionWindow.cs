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
            window.minSize = new Vector2(400, 500);
            ActionHomeConfig.LoadInstanceIfAssetExists();
            HomeConfigPreview.Instance.Init();
        }

        GUIContent _createConfig = new GUIContent("创建动作配置");

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: false);
            tree.Config.DrawSearchToolbar = true;
            foreach (var item in ActionHomeConfig.MenuItems)
            {
                if (item.Key != GroupType.None)
                    tree.Add(item.Value, null, EditorIcons.Table);
                else
                    tree.Add("主页", HomeConfigPreview.Instance, EditorIcons.House);
            }
            foreach (var group in HomeConfigPreview.Instance.ModelGroupDict)
            {
                foreach (var item in group.Value)
                {
                    tree.Add(item.MenuItemName, item);
                }
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
                    ModelActionConfigEditor.Create((model) =>
                    {
                        HomeConfigPreview.Instance.AddModel(model);
                        MenuTree.AddObjectAtPath(model.MenuItemName, model);
                    });
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            HomeConfigPreview.Instance.Destroy();

            EditorUtility.UnloadUnusedAssetsImmediate(true);
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();

            Debug.Log("Close Action Windows~~");
        }
    }
}