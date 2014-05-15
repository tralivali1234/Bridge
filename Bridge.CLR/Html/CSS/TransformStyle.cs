﻿namespace Bridge.CLR.Html
{
    /// <summary>
    /// The transform-style CSS property determines if the children of the element are positioned in the 3D-space or are flattened in the plane of the element.
    /// </summary>
    [Bridge.CLR.Ignore]
    [Bridge.CLR.EnumEmit(EnumEmit.StringNameLowerCase)]
    [Bridge.CLR.Name("String")]
    public enum TransformStyle
    {
        /// <summary>
        /// 
        /// </summary>
        Inherit,

        /// <summary>
        /// Indicates that the children of the element should be positioned in the 3D-space.
        /// </summary>
        [Bridge.CLR.Name("preserve-3d")]
        Preserve3D, 
        
        /// <summary>
        /// Indicates that the children of the element are lying in the plane of the element itself.
        /// </summary>
        Flat
    }
}