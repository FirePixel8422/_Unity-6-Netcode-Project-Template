using System;
using UnityEngine;


namespace Fire_Pixel.Utility
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class DisplayNameAttribute : PropertyAttribute
    {
        public readonly string label;

        public DisplayNameAttribute(string label)
        {
            this.label = label;
        }
    }
}