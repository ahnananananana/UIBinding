using System;
using System.Reflection;
using UnityEngine;

namespace HDV.UIBinding
{
    /// <summary>
    /// UI binder for Text component
    /// </summary>
    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class TextBinder : UIBinder<TMPro.TMP_Text>
    {
        protected override void Init()
        {
            object initValue = PropertyInfo.GetValue(Target);
            UIComponent.text = initValue?.ToString();
        }

        protected override void OnChangeValue(float newValue)
        {
            UIComponent.text = newValue.ToString();
        }

        protected override void OnChangeValue(int newValue)
        {
            UIComponent.text = newValue.ToString();
        }

        protected override void OnChangeValue(object newValue)
        {
            UIComponent.text = newValue.ToString();
        }

#if UNITY_EDITOR
        public override bool CanBind(Type type)
        {
            return type == typeof(int) || type == typeof(float) || type == typeof(string);
        }
#endif
    }
}
