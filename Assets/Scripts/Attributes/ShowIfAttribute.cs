using System;
using UnityEngine;


namespace Fire_Pixel.Utility
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ShowIfAttribute : PropertyAttribute
    {
        public readonly string condition;
        public readonly bool compareState;

        public ShowIfAttribute(string condition, bool compareState = true)
        {
            this.condition = condition;
            this.compareState = compareState;
        }
    }
}