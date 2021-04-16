using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace HDV.UIBinding
{
    /// <summary>
    /// UI binder for Image component
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ImageBinder : UIBinder<Image>
    {
        protected override void Init()
        {
            UIComponent.sprite = PropertyInfo.GetValue(Target) as Sprite;
        }

        protected override void OnChangeValue(object newValue)
        {
            UIComponent.sprite = (Sprite)newValue;
        }

#if UNITY_EDITOR
        public override bool CanBind(Type type)
        {
            return type == typeof(Sprite);
        }
#endif
    }
}
