using System;

namespace HDV.UIBinding
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UIBindable : Attribute
    { 
        public UIBindable()
        {}
    }
}