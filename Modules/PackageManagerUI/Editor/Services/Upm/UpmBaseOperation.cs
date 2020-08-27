// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using UnityEngine;
using UnityEditor.PackageManager.Requests;

namespace UnityEditor.PackageManager.UI
{
    internal abstract class UpmBaseOperation : IOperation
    {
        public abstract event Action<IOperation, UIError> onOperationError;
        public abstract event Action<IOperation> onOperationSuccess;
        public abstract event Action<IOperation> onOperationFinalized;
        public abstract event Action<IOperation> onOperationProgress;

        [SerializeField]
        protected string m_PackageName = string.Empty;
        public string packageName
        {
            get
            {
                if (!string.IsNullOrEmpty(m_PackageName))
                    return m_PackageName;
                if (!string.IsNullOrEmpty(m_PackageId))
                    return m_PackageId.Split(new[] { '@' }, 2)[0];
                return string.Empty;
            }
        }

        [SerializeField]
        protected string m_PackageId = string.Empty;
        public string packageId { get { return m_PackageId; } }

        [SerializeField]
        protected string m_PackageUniqueId = string.Empty;
        public string packageUniqueId { get { return m_PackageUniqueId; } }
        public string versionUniqueId { get { return packageId; } }

        public virtual string specialUniqueId { get { return string.Empty; } }

        [SerializeField]
        protected long m_Timestamp = 0;
        public long timestamp { get { return m_Timestamp; } }

        [SerializeField]
        protected long m_LastSuccessTimestamp = 0;
        public long lastSuccessTimestamp { get { return m_LastSuccessTimestamp; } }

        [SerializeField]
        protected bool m_OfflineMode = false;
        public bool isOfflineMode { get { return m_OfflineMode; } }

        public abstract bool isInProgress { get; }

        public bool isProgressTrackable => false;

        public float progressPercentage => 0;

        public UIError error { get; protected set; }        // Keep last error

        public abstract RefreshOptions refreshOptions { get; }
    }

    internal abstract class UpmBaseOperation<T> : UpmBaseOperation where T : Request
    {
        public override event Action<IOperation, UIError> onOperationError = delegate {};
        public override event Action<IOperation> onOperationFinalized = delegate {};
        public override event Action<IOperation> onOperationSuccess = delegate {};
        public override event Action<IOperation> onOperationProgress = delegate {};
        public Action<T> onProcessResult = delegate {};

        [SerializeField]
        protected T m_Request;
        [SerializeField]
        protected bool m_IsCompleted;

        public override bool isInProgress { get { return m_Request != null && m_Request.Id != 0 && !m_IsCompleted; } }

        protected abstract T CreateRequest();

        protected void Start()
        {
            if (isInProgress)
            {
                Debug.LogError(ApplicationUtil.instance.GetTranslationForText("Unable to start the operation again while it's in progress. " +
                    "Please cancel the operation before re-start or wait until the operation is completed."));
                return;
            }

            if (!isOfflineMode)
                m_Timestamp = DateTime.Now.Ticks;
            // Usually the timestamp for an offline operation is the last success timestamp of its online equivalence (to indicate the freshness of the data)
            // But in the rare case where we start an offline operation before an online one, we use the start timestamp of the editor instead of 0,
            // because we consider a `0` refresh timestamp as `not initialized`/`no refreshes have been done`.
            else if (m_Timestamp == 0)
                m_Timestamp = DateTime.Now.Ticks - (long)(EditorApplication.timeSinceStartup * TimeSpan.TicksPerSecond);

            error = null;
            try
            {
                m_Request = CreateRequest();
            }
            catch (ArgumentException e)
            {
                OnError(new UIError(UIErrorCode.UpmError, e.Message));
                return;
            }
            m_IsCompleted = false;
            EditorApplication.update += Progress;
        }

        protected void CancelInternal()
        {
            OnFinalize();
            m_Request = null;
        }

        // Common progress code for all classes
        protected void Progress()
        {
            m_IsCompleted = m_Request.IsCompleted;
            if (m_IsCompleted)
            {
                if (m_Request.Status == StatusCode.Success)
                    OnSuccess();
                else if (m_Request.Status >= StatusCode.Failure)
                    OnError((UIError)m_Request.Error);
                else
                    Debug.LogError(string.Format(ApplicationUtil.instance.GetTranslationForText("Unsupported progress state {0}"), m_Request.Status));
                OnFinalize();
            }
        }

        private void OnError(UIError error)
        {
            this.error = error;
            var message = ApplicationUtil.instance.GetTranslationForText("Cannot perform upm operation");
            message += string.IsNullOrEmpty(error.message) ? "." : $": {error.message} [{error.errorCode}].";
            message += ApplicationUtil.instance.GetTranslationForText("See console for more details");

            Debug.LogError(message);
            onOperationError?.Invoke(this, error);
        }

        private void OnSuccess()
        {
            onProcessResult(m_Request);
            m_LastSuccessTimestamp = m_Timestamp;
            onOperationSuccess?.Invoke(this);
        }

        private void OnFinalize()
        {
            EditorApplication.update -= Progress;
            onOperationFinalized?.Invoke(this);

            onOperationError = delegate {};
            onOperationFinalized = delegate {};
            onOperationSuccess = delegate {};
            onProcessResult = delegate {};
        }
    }
}
