// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEditorInternal.VR;
using System.Linq;

namespace UnityEditor
{
    /// <summary>
    /// Editor class used to edit UI Canvases.
    /// </summary>

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Canvas))]
    internal class CanvasEditor : Editor
    {
        SerializedProperty m_RenderMode;
        SerializedProperty m_Camera;
        SerializedProperty m_PixelPerfect;
        SerializedProperty m_PixelPerfectOverride;
        SerializedProperty m_PlaneDistance;
        SerializedProperty m_SortingLayerID;
        SerializedProperty m_SortingOrder;
        SerializedProperty m_TargetDisplay;
        SerializedProperty m_OverrideSorting;
        SerializedProperty m_ShaderChannels;
        SerializedProperty m_UpdateRectTransformForStandalone;
        SerializedProperty m_VertexColorAlwaysGammaSpace;

        AnimBool m_OverlayMode;
        AnimBool m_CameraMode;
        AnimBool m_WorldMode;

        AnimBool m_SortingOverride;

        private static class Styles
        {
            public static GUIContent eventCamera = EditorGUIUtility.TrTextContent("Event Camera", "The Camera which the events are triggered through. This is used to determine clicking and hover positions if the Canvas is in World Space render mode.");
            public static GUIContent renderCamera = EditorGUIUtility.TrTextContent("Render Camera", "The Camera which will render the canvas. This is also the camera used to send events.");
            public static GUIContent sortingOrder = EditorGUIUtility.TrTextContent("Sort Order", "The order in which Screen Space - Overlay canvas will render");
            public static string s_RootAndNestedMessage = "Cannot multi-edit root Canvas together with nested Canvas.";
            public static GUIContent m_SortingLayerStyle = EditorGUIUtility.TrTextContent("Sorting Layer", "Name of the Renderer's sorting layer");
            public static GUIContent targetDisplay = EditorGUIUtility.TrTextContent("Target Display", "Display on which to render the canvas when in overlay mode");
            public static GUIContent m_SortingOrderStyle = EditorGUIUtility.TrTextContent("Order in Layer", "Renderer's order within a sorting layer");
            public static GUIContent m_ShaderChannel = EditorGUIUtility.TrTextContent("Additional Shader Channels");
            public static GUIContent pixelPerfectContent = EditorGUIUtility.TrTextContent("Pixel Perfect");
            public static GUIContent standaloneRenderResize = EditorGUIUtility.TrTextContent("Resize Canvas", "For manual Camera.Render calls should the canvas resize to match the destination target.");
            public static GUIContent vertexColorAlwaysGammaSpace = EditorGUIUtility.TrTextContent("Vertex Color Always In Gamma Color Space", "UI vertex colors are always in gamma color space disregard of the player settings");
        }

        private bool m_AllNested = false;
        private bool m_AllRoot = false;

        private bool m_AllOverlay = false;
        private bool m_NoneOverlay = false;

        private string[] shaderChannelOptions = { "TexCoord1", "TexCoord2", "TexCoord3", "Normal", "Tangent" };


        enum PixelPerfect
        {
            Inherit,
            On,
            Off
        }

        private PixelPerfect pixelPerfect = PixelPerfect.Inherit;

        void OnEnable()
        {
            m_RenderMode = serializedObject.FindProperty("m_RenderMode");
            m_Camera = serializedObject.FindProperty("m_Camera");
            m_PixelPerfect = serializedObject.FindProperty("m_PixelPerfect");
            m_PlaneDistance = serializedObject.FindProperty("m_PlaneDistance");

            m_SortingLayerID = serializedObject.FindProperty("m_SortingLayerID");
            m_SortingOrder = serializedObject.FindProperty("m_SortingOrder");
            m_TargetDisplay = serializedObject.FindProperty("m_TargetDisplay");
            m_OverrideSorting = serializedObject.FindProperty("m_OverrideSorting");
            m_PixelPerfectOverride = serializedObject.FindProperty("m_OverridePixelPerfect");
            m_ShaderChannels = serializedObject.FindProperty("m_AdditionalShaderChannelsFlag");
            m_UpdateRectTransformForStandalone = serializedObject.FindProperty("m_UpdateRectTransformForStandalone");
            m_VertexColorAlwaysGammaSpace = serializedObject.FindProperty("m_VertexColorAlwaysGammaSpace");

            m_OverlayMode = new AnimBool(m_RenderMode.intValue == 0);
            m_OverlayMode.valueChanged.AddListener(Repaint);

            m_CameraMode = new AnimBool(m_RenderMode.intValue == 1);
            m_CameraMode.valueChanged.AddListener(Repaint);

            m_WorldMode = new AnimBool(m_RenderMode.intValue == 2);
            m_WorldMode.valueChanged.AddListener(Repaint);

            m_SortingOverride = new AnimBool(m_OverrideSorting.boolValue);
            m_SortingOverride.valueChanged.AddListener(Repaint);

            if (m_PixelPerfectOverride.boolValue)
                pixelPerfect = m_PixelPerfect.boolValue ? PixelPerfect.On : PixelPerfect.Off;
            else
                pixelPerfect = PixelPerfect.Inherit;

            m_AllNested = true;
            m_AllRoot = true;
            m_AllOverlay = true;
            m_NoneOverlay = true;

            for (int i = 0; i < targets.Length; i++)
            {
                Canvas canvas = targets[i] as Canvas;
                Canvas[] parentCanvas = canvas.transform.parent != null ? canvas.transform.parent.GetComponentsInParent<Canvas>(true) : null;

                if (canvas.transform.parent == null || (parentCanvas != null && parentCanvas.Length == 0))
                    m_AllNested = false;
                else
                    m_AllRoot = false;

                RenderMode renderMode = canvas.renderMode;

                if (parentCanvas != null && parentCanvas.Length > 0)
                    renderMode = parentCanvas[parentCanvas.Length - 1].renderMode;

                if (renderMode == RenderMode.ScreenSpaceOverlay)
                    m_NoneOverlay = false;
                else
                    m_AllOverlay = false;
            }
        }

        void OnDisable()
        {
            m_OverlayMode.valueChanged.RemoveListener(Repaint);
            m_CameraMode.valueChanged.RemoveListener(Repaint);
            m_WorldMode.valueChanged.RemoveListener(Repaint);
            m_SortingOverride.valueChanged.RemoveListener(Repaint);
        }

        private void AllRootCanvases()
        {
            if (VREditor.GetVREnabledOnTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)) && (m_RenderMode.enumValueIndex == (int)RenderMode.ScreenSpaceOverlay))
            {
                EditorGUILayout.HelpBox("Using a render mode of ScreenSpaceOverlay while VR is enabled will cause the Canvas to continue to incur a rendering cost, even though the Canvas will not be visible in VR.", MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_RenderMode);
            if (EditorGUI.EndChangeCheck())
            {
                var rectTransforms = targets.Select(c => (c as Canvas).transform).ToArray();
                Undo.RegisterCompleteObjectUndo(rectTransforms, "Modified RectTransform Values");
                serializedObject.ApplyModifiedProperties();
                foreach (Canvas canvas in targets)
                {
                    canvas.UpdateCanvasRectTransform(true);
                }
                GUIUtility.ExitGUI();
            }

            m_OverlayMode.target = m_RenderMode.intValue == 0;
            m_CameraMode.target = m_RenderMode.intValue == 1;
            m_WorldMode.target = m_RenderMode.intValue == 2;

            EditorGUI.indentLevel++;
            if (EditorGUILayout.BeginFadeGroup(m_OverlayMode.faded))
            {
                DoPixelPerfectGUIForRoot();

                EditorGUILayout.PropertyField(m_SortingOrder, Styles.sortingOrder);
                GUIContent[] displayNames = DisplayUtility.GetDisplayNames();
                EditorGUILayout.IntPopup(m_TargetDisplay, displayNames, DisplayUtility.GetDisplayIndices(), Styles.targetDisplay);
            }
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(m_CameraMode.faded))
            {
                DoPixelPerfectGUIForRoot();

                EditorGUILayout.PropertyField(m_Camera, Styles.renderCamera);

                if (m_Camera.objectReferenceValue == null)
                    EditorGUILayout.HelpBox("A Screen Space Canvas with no specified camera acts like an Overlay Canvas.",
                        MessageType.Warning);

                if (m_Camera.objectReferenceValue != null)
                {
                    EditorGUILayout.PropertyField(m_PlaneDistance);
                    EditorGUILayout.PropertyField(m_UpdateRectTransformForStandalone, Styles.standaloneRenderResize);
                }

                EditorGUILayout.Space();

                if (m_Camera.objectReferenceValue != null)
                    EditorGUILayout.SortingLayerField(Styles.m_SortingLayerStyle, m_SortingLayerID, EditorStyles.popup, EditorStyles.label);
                EditorGUILayout.PropertyField(m_SortingOrder, Styles.m_SortingOrderStyle);
            }
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(m_WorldMode.faded))
            {
                EditorGUILayout.PropertyField(m_Camera, Styles.eventCamera);

                if (m_Camera.objectReferenceValue == null)
                    EditorGUILayout.HelpBox("A World Space Canvas with no specified Event Camera may not register UI events correctly.",
                        MessageType.Warning);

                EditorGUILayout.Space();
                EditorGUILayout.SortingLayerField(Styles.m_SortingLayerStyle, m_SortingLayerID, EditorStyles.popup);
                EditorGUILayout.PropertyField(m_SortingOrder, Styles.m_SortingOrderStyle);
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUI.indentLevel--;
        }

        private void DoPixelPerfectGUIForRoot()
        {
            bool pixelPerfectValue = m_PixelPerfect.boolValue;

            EditorGUI.BeginChangeCheck();
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(rect, Styles.pixelPerfectContent, m_PixelPerfect);
            pixelPerfectValue = EditorGUI.Toggle(rect, Styles.pixelPerfectContent, pixelPerfectValue);
            EditorGUI.EndProperty();

            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    Canvas canvas = targets[i] as Canvas;

                    Undo.RecordObject(canvas, "Set Pixel Perfect");
                    canvas.pixelPerfect = pixelPerfectValue;
                }
            }
        }

        private void DoPixelPerfectGUIForNested()
        {
            EditorGUI.BeginChangeCheck();
            pixelPerfect = (PixelPerfect)EditorGUILayout.EnumPopup(Styles.pixelPerfectContent, pixelPerfect);

            if (EditorGUI.EndChangeCheck())
            {
                if (pixelPerfect == PixelPerfect.Inherit)
                {
                    m_PixelPerfectOverride.boolValue = false;
                }
                else
                {
                    m_PixelPerfectOverride.boolValue = true;
                    for (int i = 0; i < targets.Length; i++)
                    {
                        Canvas canvas = targets[i] as Canvas;

                        Undo.RecordObject(canvas, "Set Pixel Perfect");
                        canvas.pixelPerfect = pixelPerfect == PixelPerfect.On;
                    }
                }
            }
        }

        private void AllNestedCanvases()
        {
            DoPixelPerfectGUIForNested();

            EditorGUILayout.PropertyField(m_OverrideSorting);
            m_SortingOverride.target = m_OverrideSorting.boolValue;

            if (EditorGUILayout.BeginFadeGroup(m_SortingOverride.faded))
            {
                GUIContent sortingOrderStyle = null;
                if (m_AllOverlay)
                {
                    sortingOrderStyle = Styles.sortingOrder;
                }
                else if (m_NoneOverlay)
                {
                    sortingOrderStyle = Styles.m_SortingOrderStyle;
                    EditorGUILayout.SortingLayerField(Styles.m_SortingLayerStyle, m_SortingLayerID, EditorStyles.popup);
                }
                if (sortingOrderStyle != null)
                {
                    EditorGUILayout.PropertyField(m_SortingOrder, sortingOrderStyle);
                }
            }
            EditorGUILayout.EndFadeGroup();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (m_AllRoot || m_AllNested)
            {
                if (m_AllRoot)
                {
                    AllRootCanvases();
                }
                else if (m_AllNested)
                {
                    AllNestedCanvases();
                }

                EditorGUI.BeginChangeCheck();
                var rect = EditorGUILayout.GetControlRect();
                EditorGUI.BeginProperty(rect, Styles.m_ShaderChannel, m_ShaderChannels);
                var newShaderChannelValue = EditorGUI.MaskField(rect, Styles.m_ShaderChannel, m_ShaderChannels.intValue, shaderChannelOptions);
                EditorGUI.EndProperty();

                if (EditorGUI.EndChangeCheck())
                    m_ShaderChannels.intValue = newShaderChannelValue;

                if (m_RenderMode.intValue == 0) // Overlay canvas
                {
                    if (((newShaderChannelValue & (int)AdditionalCanvasShaderChannels.Normal) | (newShaderChannelValue & (int)AdditionalCanvasShaderChannels.Tangent)) != 0)
                    {
                        var helpMessage = "Shader channels Normal and Tangent are most often used with lighting, which an Overlay canvas does not support. Its likely these channels are not needed.";
                        rect = GUILayoutUtility.GetRect(EditorGUIUtility.TempContent(helpMessage, EditorGUIUtility.GetHelpIcon(MessageType.Warning)), EditorStyles.helpBox);
                        EditorGUI.HelpBox(rect, helpMessage, MessageType.Warning);
                    }
                }

                EditorGUILayout.PropertyField(m_VertexColorAlwaysGammaSpace, Styles.vertexColorAlwaysGammaSpace);

                if (PlayerSettings.colorSpace == ColorSpace.Linear)
                {
                    if (!m_VertexColorAlwaysGammaSpace.boolValue)
                        EditorGUILayout.HelpBox( "Keep vertex color in Gamma space to allow gamma to linear color space conversion to happen in UI shaders. This will enhance UI color precision in linear color space.", MessageType.Warning);
                }
            }
            else
            {
                GUILayout.Label(Styles.s_RootAndNestedMessage, EditorStyles.helpBox);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
