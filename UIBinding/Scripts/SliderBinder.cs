using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace HDV.UIBinding
{
    /// <summary>
    /// UI binder for Slider component
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class SliderBinder : UIBinder<Slider>
    {
        private Action<float> _setFloatFunc;
        private Action<int> _setIntFunc;

        protected override void Init()
        {
            UIComponent.onValueChanged.AddListener(OnModifyValue);

            object initValue = PropertyInfo.GetValue(Target);

            if (initValue is float)
            {
                _setFloatFunc = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), Target, PropertyInfo.SetMethod);
                UIComponent.value = (float)initValue;
            }
            else if(initValue is int)
            {
                _setIntFunc = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), Target, PropertyInfo.SetMethod);
                UIComponent.value = (int)initValue;
            }
        }

        protected override void OnChangeValue(float newValue)
        {
            UIComponent.value = newValue;
        }

        protected override void OnChangeValue(int newValue)
        {
            UIComponent.value = newValue;
        }

        private void OnModifyValue(float newValue)
        {
            if(_setFloatFunc != null)
            {
                _setFloatFunc(newValue);
            }
            else if(_setIntFunc != null)
            {
                _setIntFunc((int)newValue);
            }
        }

#if UNITY_EDITOR
        public override bool CanBind(Type type)
        {
            return type == typeof(int) || type == typeof(float);
        }
#endif
    }
}
