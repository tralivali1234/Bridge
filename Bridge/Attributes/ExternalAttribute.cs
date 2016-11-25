﻿using System;

namespace Bridge
{
    /// <summary>
    /// Makes it so the code with this attribute is not built into the assembly files.
    /// Useful for stubbed out code to match JavaScript.
    /// </summary>
    [External]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    [NonScriptable]
    public sealed class ExternalAttribute : Attribute
    {
    }

    /// <summary>
    /// Applies to interface if it's implementation is done outside Bridge type system (class implementation doesn't provide aliases for interface members implementations)
    /// </summary>
    [External]
    [AttributeUsage(AttributeTargets.Interface)]
    [NonScriptable]
    public sealed class ExternalInterfaceAttribute : Attribute
    {
        public extern ExternalInterfaceAttribute();
        public extern ExternalInterfaceAttribute(bool nativeImplementation);
    }

    /// <summary>
	/// This attribute can be placed on types in system script assemblies that should not
	/// be imported. It is only meant to be used within Bridge.dll.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    [NonScriptable]
    public sealed class NonScriptableAttribute : Attribute
    {
    }
}