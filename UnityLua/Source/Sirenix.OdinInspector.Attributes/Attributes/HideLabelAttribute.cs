//-----------------------------------------------------------------------
// <copyright file="HideLabelAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>HideLabel is used on any property, and hides the label in the inspector.</para>
    /// <para>Use this to hide the label of properties in the inspector.</para>
    /// </summary>
	/// <example>
    /// <para>The following example show how HideLabel is used to hide the label of a game object property.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
	/// {
	///		[HideLabel]
	///		public GameObject MyGameObjectWithoutLabel;
	/// }
    /// </code>
    /// </example>
	/// <seealso cref="LabelTextAttribute"/>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class HideLabelAttribute : Attribute
    {
    }
}