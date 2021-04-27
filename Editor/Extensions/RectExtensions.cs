﻿namespace SolidUtilities.Editor.Extensions
{
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEngine;

    public static class RectExtensions
    {
        [PublicAPI]
        public static Rect ShiftOneLineDown(this Rect rect, int indent = -1, float lineHeight = 0f)
        {
            const float paddingBetweenFields = 2f;
            const float indentPerLevel = 15f;

            if (lineHeight == 0f)
                lineHeight = EditorGUIUtility.singleLineHeight;

            if (indent == -1)
                indent = EditorGUI.indentLevel;

            rect.xMin += indent * indentPerLevel;
            rect.y += lineHeight + paddingBetweenFields;

            return rect;
        }
    }
}