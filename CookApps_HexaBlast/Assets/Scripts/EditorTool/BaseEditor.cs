using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace ProjectPuzzle
{
    public class BaseEditor : OdinMenuEditorWindow
    {
        protected static GUIStyle redLabel;
        public static GUIStyle RedLabel
        {
            get
            {
                if (redLabel == null)
                {
                    redLabel = new GUIStyle(EditorStyles.label) { margin = new RectOffset(0, 0, 0, 0) };
                    redLabel.fontStyle = FontStyle.Bold;
                    redLabel.normal.textColor = Color.red;
                    redLabel.onNormal.textColor = Color.red;
                }
                return redLabel;
            }
        }
        protected static OdinMenuStyle menuErrorStyle;
        public static OdinMenuStyle MenuErrorStyle
        {
            get
            {
                if (menuErrorStyle == null)
                {
                    menuErrorStyle = new OdinMenuStyle();
                    menuErrorStyle.DefaultLabelStyle = RedLabel;
                    menuErrorStyle.SelectedLabelStyle = RedLabel;
                }
                return menuErrorStyle;
            }
        }
        protected static OdinMenuStyle menuDefaultStyle;

        public static OdinMenuStyle MenuDefaultStyle
        {
            get
            {
                if (menuDefaultStyle == null)
                {
                    menuDefaultStyle = new OdinMenuStyle();
                }

                return menuDefaultStyle;
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true);
            tree.DefaultMenuStyle.IconSize = 50f;
            tree.Config.DrawSearchToolbar = true;

            return tree;
        }
    }
}
