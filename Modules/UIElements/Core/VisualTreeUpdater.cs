// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using Unity.Profiling;

namespace UnityEngine.UIElements
{
    // Editor update phases, the order of the enum define the updater order
    internal enum VisualTreeEditorUpdatePhase
    {
        AssetChange,
        Count
    }

    internal interface IVisualTreeEditorUpdater : IDisposable
    {
        IVisualTreeUpdater GetUpdater(VisualTreeEditorUpdatePhase phase);
        void SetUpdater(IVisualTreeUpdater updater, VisualTreeEditorUpdatePhase phase);
        void Update();
        void UpdateVisualTreePhase(VisualTreeEditorUpdatePhase phase);
        void OnVersionChanged(VisualElement ve, VersionChangeType versionChangeType);
    }

    // Update phases, the order of the enum define the updater order
    internal enum VisualTreeUpdatePhase
    {
        ViewData,
        Bindings,
        DataBinding,
        Animation,
        Styles,
        Layout,
        TransformClip,
        Repaint,
        Count
    }

    internal sealed class VisualTreeUpdater : IDisposable
    {
        class UpdaterArray
        {
            private IVisualTreeUpdater[] m_VisualTreeUpdaters;

            public UpdaterArray()
            {
                m_VisualTreeUpdaters = new IVisualTreeUpdater[(int)VisualTreeUpdatePhase.Count];
            }

            public IVisualTreeUpdater this[VisualTreeUpdatePhase phase]
            {
                set { m_VisualTreeUpdaters[(int)phase] = value; }
                get { return m_VisualTreeUpdaters[(int)phase]; }
            }

            public IVisualTreeUpdater this[int index]
            {
                set { m_VisualTreeUpdaters[index] = value; }
                get { return m_VisualTreeUpdaters[index]; }
            }
        }

        private BaseVisualElementPanel m_Panel;
        private UpdaterArray m_UpdaterArray;
        public IVisualTreeEditorUpdater visualTreeEditorUpdater { get; set; }

        public VisualTreeUpdater(BaseVisualElementPanel panel)
        {
            m_Panel = panel;
            m_UpdaterArray = new UpdaterArray();

            SetDefaultUpdaters();
        }

        public void Dispose()
        {
            visualTreeEditorUpdater.Dispose();
            for (int i = 0; i < (int)VisualTreeUpdatePhase.Count; i++)
            {
                var updater = m_UpdaterArray[i];
                updater.Dispose();
            }
        }

        //Note: used in tests
        public void UpdateVisualTree()
        {
            visualTreeEditorUpdater.Update();

            for (int i = 0; i < (int)VisualTreeUpdatePhase.Count; i++)
            {
                var updater = m_UpdaterArray[i];

                using (updater.profilerMarker.Auto())
                {
                    updater.Update();
                }
            }
        }

        public void UpdateVisualTreePhase(VisualTreeUpdatePhase phase)
        {
            var updater = m_UpdaterArray[phase];

            using (updater.profilerMarker.Auto())
            {
                updater.Update();
            }
        }

        public void OnVersionChanged(VisualElement ve, VersionChangeType versionChangeType)
        {
            visualTreeEditorUpdater.OnVersionChanged(ve, versionChangeType);

            for (int i = 0; i < (int)VisualTreeUpdatePhase.Count; i++)
            {
                var updater = m_UpdaterArray[i];
                updater.OnVersionChanged(ve, versionChangeType);
            }
        }

        public void DirtyStyleSheets()
        {
            var styleUpdater = m_UpdaterArray[VisualTreeUpdatePhase.Styles] as VisualTreeStyleUpdater;
            styleUpdater.DirtyStyleSheets();
        }


        public void SetUpdater(IVisualTreeUpdater updater, VisualTreeUpdatePhase phase)
        {
            m_UpdaterArray[phase]?.Dispose();
            updater.panel = m_Panel;
            m_UpdaterArray[phase] = updater;
        }

        public void SetUpdater<T>(VisualTreeUpdatePhase phase) where T : IVisualTreeUpdater, new()
        {
            m_UpdaterArray[phase]?.Dispose();
            var updater = new T() {panel = m_Panel};
            m_UpdaterArray[phase] = updater;
        }

        public IVisualTreeUpdater GetUpdater(VisualTreeUpdatePhase phase)
        {
            return m_UpdaterArray[phase];
        }

        private void SetDefaultUpdaters()
        {
            SetUpdater<VisualTreeViewDataUpdater>(VisualTreeUpdatePhase.ViewData);
            SetUpdater<VisualTreeBindingsUpdater>(VisualTreeUpdatePhase.Bindings);
            SetUpdater<VisualTreeDataBindingsUpdater>(VisualTreeUpdatePhase.DataBinding);
            SetUpdater<VisualElementAnimationSystem>(VisualTreeUpdatePhase.Animation);
            SetUpdater<VisualTreeStyleUpdater>(VisualTreeUpdatePhase.Styles);
            SetUpdater<UIRLayoutUpdater>(VisualTreeUpdatePhase.Layout);
            SetUpdater<VisualTreeHierarchyFlagsUpdater>(VisualTreeUpdatePhase.TransformClip);
            SetUpdater<UIRRepaintUpdater>(VisualTreeUpdatePhase.Repaint);
        }
    }

    internal interface IVisualTreeUpdater : IDisposable
    {
        BaseVisualElementPanel panel { get; set; }

        ProfilerMarker profilerMarker { get; }

        void Update();
        void OnVersionChanged(VisualElement ve, VersionChangeType versionChangeType);
    }

    internal abstract class BaseVisualTreeUpdater : IVisualTreeUpdater
    {
        public event Action<BaseVisualElementPanel> panelChanged;

        private BaseVisualElementPanel m_Panel;
        public BaseVisualElementPanel panel
        {
            get { return m_Panel; }
            set
            {
                m_Panel = value;
                if (panelChanged != null) panelChanged(value);
            }
        }

        public VisualElement visualTree { get { return panel.visualTree; } }

        public abstract ProfilerMarker profilerMarker { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {}

        public abstract void Update();
        public abstract void OnVersionChanged(VisualElement ve, VersionChangeType versionChangeType);
    }
}
