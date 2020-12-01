﻿namespace SolidUtilities.Editor.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

#if !UNITY_2020_1_OR_NEWER
    using System.Reflection;
#endif

    /// <summary>Different useful methods that simplify <see cref="EditorGUILayout"/> API.</summary>
    public static class EditorDrawHelper
    {
        /// <summary>
        /// Cache that creates <see cref="GUIContent"/> instances and keeps them, reducing the garbage
        /// collection overhead.
        /// </summary>
        public static readonly ContentCache ContentCache = new ContentCache();

        private const float PlaceholderIndent = 14f;

        private static readonly GUIStyle SearchToolbarStyle = new GUIStyle(EditorStyles.toolbar)
        {
            padding = new RectOffset(0, 0, 0, 0),
            stretchHeight = true,
            stretchWidth = true,
            fixedHeight = 0f
        };

        private static readonly GUIStyle InfoMessageStyle = new GUIStyle( "HelpBox")
        {
            margin = new RectOffset(4, 4, 2, 2),
            fontSize = 10,
            richText = true
        };

        private static readonly GUIStyle PlaceholderStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,
            margin = new RectOffset(4, 4, 4, 4)
        };

        /// <summary>Draws content in an automatically laid out scroll view.</summary>
        /// <param name="scrollPos">Position of the thumb.</param>
        /// <param name="drawContent">Action that draws the content in the scroll view.</param>
        /// <returns>The new thumb position.</returns>
        /// <example><code>
        /// _thumbPos = EditorDrawHelper.DrawInScrollView(_thumbPos, () =>
        /// {
        ///     float contentHeight = EditorDrawHelper.DrawVertically(_selectionTree.Draw, _preventExpandingHeight,
        ///         DropdownStyle.BackgroundColor);
        /// });
        /// </code></example>
        [PublicAPI] public static Vector2 DrawInScrollView(Vector2 scrollPos, Action drawContent)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            drawContent();
            EditorGUILayout.EndScrollView();
            return scrollPos;
        }

        /// <summary>Draws content in the vertical direction.</summary>
        /// <param name="drawContent">Action that draws the content.</param>
        /// <param name="option">Option to draw the vertical group with.</param>
        /// <param name="backgroundColor">Background of the vertical group rectangle.</param>
        /// <returns>Height of the vertical group rectangle.</returns>
        /// <example><code>
        /// float contentHeight = EditorDrawHelper.DrawVertically(_selectionTree.Draw, _preventExpandingHeight,
        ///     DropdownStyle.BackgroundColor);
        /// </code></example>
        [PublicAPI] public static float DrawVertically(Action drawContent, GUILayoutOption option, Color backgroundColor)
        {
            Rect rect = EditorGUILayout.BeginVertical(option);
            EditorGUI.DrawRect(rect, backgroundColor);
            drawContent();
            EditorGUILayout.EndVertical();
            return rect.height;
        }

        /// <summary>Draws content in the vertical direction.</summary>
        /// <param name="drawContent">Action that draws the content.</param>
        /// <returns>Rectangle of the vertical group.</returns>
        /// <example><code>
        /// Rect newWholeListRect = EditorDrawHelper.DrawVertically(() =>
        /// {
        ///     for (int index = 0; index &lt; nodes.Count; ++index)
        ///         nodes[index].DrawSelfAndChildren(0, visibleRect);
        /// });
        /// </code></example>
        [PublicAPI] public static Rect DrawVertically(Action drawContent)
        {
            Rect rect = EditorGUILayout.BeginVertical();
            drawContent();
            EditorGUILayout.EndVertical();
            return rect;
        }

        /// <summary>Draws content in the vertical direction.</summary>
        /// <param name="drawContent">Action that draws the content.</param>
        /// <example><code>
        /// EditorDrawHelper.DrawVertically(windowRect =>
        /// {
        ///     if (Event.current.type == EventType.Repaint)
        ///         _windowRect = windowRect;
        ///
        ///     for (int index = 0; index &lt; nodes.Count; ++index)
        ///         nodes[index].DrawSelfAndChildren(0, visibleRect);
        /// });
        /// </code></example>
        [PublicAPI] public static void DrawVertically(Action<Rect> drawContent)
        {
            Rect rect = EditorGUILayout.BeginVertical();
            drawContent(rect);
            EditorGUILayout.EndVertical();
        }

        /// <summary>Draws borders with a given color and width around a rectangle.</summary>
        /// <param name="rectWidth">Width of the rectangle.</param>
        /// <param name="rectHeight">Height of the rectangle.</param>
        /// <param name="color">Color of the borders.</param>
        /// <param name="borderWidth">Width of the borders.</param>
        /// <example><code>
        /// EditorDrawHelper.DrawBorders(position.width, position.height, DropdownStyle.BorderColor);
        /// </code></example>
        [PublicAPI] public static void DrawBorders(float rectWidth, float rectHeight, Color color, float borderWidth = 1f)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            var leftBorder = new Rect(0f, 0f, borderWidth, rectHeight);
            var topBorder = new Rect(0f, 0f, rectWidth, borderWidth);
            var rightBorder = new Rect(0f, 0f, rectWidth, borderWidth);
            var bottomBorder = new Rect(0f, rectHeight - borderWidth, rectWidth, borderWidth);


            EditorGUI.DrawRect(leftBorder, color);
            EditorGUI.DrawRect(topBorder, color);
            EditorGUI.DrawRect(rightBorder, color);
            EditorGUI.DrawRect(bottomBorder, color);
        }

        /// <summary>Draws search toolbar with the search toolbar style.</summary>
        /// <param name="drawToolbar">Action that draws the toolbar.</param>
        /// <param name="toolbarHeight">Height of the toolbar.</param>
        /// <example><code>
        /// EditorDrawHelper.DrawWithSearchToolbarStyle(DrawSearchToolbar, DropdownStyle.SearchToolbarHeight);
        /// </code></example>
        [PublicAPI] public static void DrawWithSearchToolbarStyle(Action drawToolbar, float toolbarHeight)
        {
            EditorGUILayout.BeginHorizontal(
                SearchToolbarStyle,
                GUILayout.Height(toolbarHeight),
                DrawHelper.ExpandWidth(false));

            drawToolbar();

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>Shows the info message.</summary>
        /// <param name="message">The message to output.</param>
        /// <example><code>EditorDrawHelper.DrawInfoMessage("No types to select.");</code></example>
        [PublicAPI] public static void DrawInfoMessage(string message)
        {
            var messageContent = new GUIContent(message, EditorIcons.Info);
            Rect labelPos = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(messageContent, InfoMessageStyle));
            GUI.Label(labelPos, messageContent, InfoMessageStyle);
        }

        /// <summary>Draws content and checks if it was changed.</summary>
        /// <param name="drawContent">Action that draws the content.</param>
        /// <returns>Whether the content was changed.</returns>
        /// <example><code>
        /// bool changed = EditorDrawHelper.CheckIfChanged(() =>
        /// {
        ///     _searchString = DrawSearchField(innerToolbarArea, _searchString);
        /// });
        /// </code></example>
        [Pure, PublicAPI] public static bool CheckIfChanged(Action drawContent)
        {
            EditorGUI.BeginChangeCheck();
            drawContent();
            return EditorGUI.EndChangeCheck();
        }

        /// <summary>Draws a text field that is always focused.</summary>
        /// <param name="rect">Rectangle to draw the field in.</param>
        /// <param name="text">The text to show in the field.</param>
        /// <param name="placeholder">Placeholder to show if the field is empty.</param>
        /// <param name="style">Style to draw the field with.</param>
        /// <param name="controlName">Unique control name of the field.</param>
        /// <returns>The text that was written to the field.</returns>
        /// <example><code>
        /// searchText = EditorDrawHelper.FocusedTextField(searchFieldArea, searchText, "Search",
        ///     DropdownStyle.SearchToolbarStyle, _searchFieldControlName);
        /// </code></example>
        [PublicAPI] public static string FocusedTextField(Rect rect, string text, string placeholder, GUIStyle style, string controlName)
        {
            GUI.SetNextControlName(controlName);
            text = EditorGUI.TextField(rect, text, style);
            EditorGUI.FocusTextInControl(controlName);

            if (Event.current.type == EventType.Repaint && string.IsNullOrEmpty(text))
            {
                var placeHolderArea = new Rect(rect.x + PlaceholderIndent, rect.y, rect.width - PlaceholderIndent, rect.height);
                GUI.Label(placeHolderArea, ContentCache.GetItem(placeholder), PlaceholderStyle);
            }

            return text;
        }

        /// <summary>
        /// Sets <see cref="EditorGUI.showMixedValue"/> to the needed value temporarily and draws the content.
        /// </summary>
        /// <param name="showMixedValue">Whether to show mixed value.</param>
        /// <param name="drawAction">
        /// The action to draw content while <see cref="EditorGUI.showMixedValue"/> is set to
        /// <paramref name="showMixedValue"/>.
        /// </param>
        /// <example><code>
        /// EditorDrawHelper.WhileShowingMixedValue(
        ///     _serializedTypeRef.TypeNameHasMultipleDifferentValues,
        ///     DrawTypeSelectionControl);
        /// </code></example>
        [PublicAPI] public static void WhileShowingMixedValue(bool showMixedValue, Action drawAction)
        {
            bool valueToRestore = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = showMixedValue;
            drawAction();
            EditorGUI.showMixedValue = valueToRestore;
        }

        /// <summary>
        /// Draw content in a property wrapper, useful for making regular GUI controls work with SerializedProperty.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the control, including label if applicable.</param>
        /// <param name="label">Optional label in front of the slider. Use null to use the name from the
        /// SerializedProperty. Use GUIContent.none to not display a label.</param>
        /// <param name="property">The SerializedProperty to use for the control.</param>
        /// <param name="drawContent">The action to draw content for the property.</param>
        [PublicAPI]
        public static void InPropertyWrapper(Rect position, GUIContent label, SerializedProperty property, Action drawContent)
        {
            EditorGUI.BeginProperty(position, label, property);
            drawContent();
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Draws content with the specified indent level.
        /// </summary>
        /// <param name="indentLevel">The indent level to set while drawing content.</param>
        /// <param name="drawContent">The action that draws content.</param>
        [PublicAPI]
        public static void DrawWithIndentLevel(int indentLevel, Action drawContent)
        {
            int previousIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel;
            drawContent();
            EditorGUI.indentLevel = previousIndentLevel;
        }

        /// <summary>Creates an editor of type <typeparamref name="T"/> for <paramref name="targetObject"/>.</summary>
        /// <param name="targetObject">Target object to create an editor for.</param>
        /// <typeparam name="T">Type of the editor to create.</typeparam>
        /// <returns>Editor of type <typeparamref name="T"/>.</returns>
        [PublicAPI, Pure]
        public static T CreateEditor<T>(Object targetObject)
            where T : Editor
        {
            return (T) Editor.CreateEditor(targetObject, typeof(T));
        }

        /// <summary>
        /// Returns the same value as <see cref="Screen.currentResolution.width"/> if one screen is used. Returns the
        /// sum of two screens' widths when two monitors are used and Unity is located on the second screen. It will
        /// only return the incorrect value when Unity is located on the second screen and is not fullscreen.
        /// </summary>
        /// <returns>
        /// Screen width if one monitor is used, or sum of screen widths if multiple monitors are used.
        /// </returns>
        /// <remarks>
        /// <see cref="Display.displays"/> always returns 1 in Editor, so there is no way to check the resolution of
        /// both monitors. This method uses a workaround but it does not work correctly when Unity is on the second
        /// screen and is not fullscreen. Any help to overcome this issue will be appreciated.
        /// </remarks>
        [PublicAPI, Pure]
        public static float GetScreenWidth()
        {
            return Mathf.Max(GetMainWindowPosition().xMax, Screen.currentResolution.width);
        }

        /// <summary>
        /// Returns the rectangle of the main Unity window.
        /// </summary>
        /// <returns>Rectangle of the main Unity window.</returns>
        /// <remarks>
        /// For Unity 2020.1 and above, this is just a wrapper of <see cref="EditorGUIUtility.GetMainWindowPosition"/>,
        /// but below 2020.1 this is a separate solution.
        /// </remarks>
        [PublicAPI, Pure]
        public static Rect GetMainWindowPosition()
        {
#if UNITY_2020_1_OR_NEWER
            return EditorGUIUtility.GetMainWindowPosition();
#else
            const int mainWindowIndex = 4;
            const string showModeName = "m_ShowMode";
            const string positionName = "position";
            const string containerWindowName = "ContainerWindow";

            Type containerWinType = AppDomain.CurrentDomain
                .GetAllDerivedTypes(typeof(ScriptableObject))
                .FirstOrDefault(type => type.Name == containerWindowName);

            if (containerWinType == null)
                throw new MissingMemberException($"Can't find internal type {containerWindowName}. Maybe something has changed inside Unity.");

            FieldInfo showModeField = containerWinType.GetField(showModeName, BindingFlags.NonPublic | BindingFlags.Instance);
            PropertyInfo positionProperty = containerWinType.GetProperty(positionName, BindingFlags.Public | BindingFlags.Instance);

            if (showModeField == null || positionProperty == null)
                throw new MissingFieldException($"Can't find internal fields '{showModeName}' or '{positionName}'. Maybe something has changed inside Unity.");

            var windows = Resources.FindObjectsOfTypeAll(containerWinType);

            foreach (Object win in windows)
            {
                if ((int) showModeField.GetValue(win) != mainWindowIndex)
                    continue;

                return (Rect)positionProperty.GetValue(win, null);
            }

            throw new NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
#endif
        }

        [UsedImplicitly]
        private static IEnumerable<Type> GetAllDerivedTypes(this AppDomain appDomain, Type parentType)
        {
            return from assembly in appDomain.GetAssemblies()
                from assemblyType in assembly.GetTypes()
                where assemblyType.IsSubclassOf(parentType)
                select assemblyType;
        }
    }
}