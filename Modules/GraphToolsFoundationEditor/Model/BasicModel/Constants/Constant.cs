// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;

namespace Unity.GraphToolsFoundation.Editor
{
    /// <summary>
    /// Base class for constants.
    /// </summary>
    [Serializable]
    abstract class Constant
    {
        GraphElementModel m_OwnerModel;

        public virtual GraphElementModel OwnerModel
        {
            get => m_OwnerModel;
            set => m_OwnerModel = value;
        }

        /// <summary>
        /// The current value.
        /// </summary>
        public abstract object ObjectValue { get; set; }

        /// <summary>
        /// The default value.
        /// </summary>
        public abstract object DefaultValue { get; }

        /// <summary>
        /// The type of the value.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Initializes the constant after creation.
        /// </summary>
        /// <param name="constantTypeHandle">The type of value held by this constant.</param>
        public virtual void Initialize(TypeHandle constantTypeHandle)
        {
            // We ignore constantTypeHandle. Our type is solely determined by T.
            ObjectValue = DefaultValue;
        }

        /// <summary>
        /// Clones the constant.
        /// </summary>
        /// <returns>The cloned constant.</returns>
        public abstract Constant Clone();

        /// <summary>
        /// Gets the <see cref="TypeHandle"/> of the value.
        /// </summary>
        /// <returns>The <see cref="TypeHandle"/> of the value.</returns>
        public virtual TypeHandle GetTypeHandle()
        {
            return Type.GenerateTypeHandle();
        }

        /// <summary>
        /// Tells whether this constant can accept values to type <paramref name="t"/>.
        /// </summary>
        /// <param name="t">The type of value.</param>
        /// <returns>True if this constant can accept values to type <paramref name="t"/>, false otherwise.</returns>
        public virtual bool IsAssignableFrom(Type t)
        {
            return Type.IsAssignableFrom(t);
        }
    }
}
