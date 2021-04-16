using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HDV.UIBinding
{
    /// <summary>
    /// Base Generic class of all UI component binder.
    /// Process binding with UIBindProxy
    /// T is type of ui component
    /// </summary>
    public abstract class UIBinder : MonoBehaviour
    {

#if UNITY_EDITOR
        public abstract bool CanBind(Type type);
#endif
    }


    public abstract class UIBinder<T> : UIBinder where T : Component
    {
        private T _uiComponent;
        private Object _target;
        private string _key;
        private Type _targetType;
        private PropertyInfo _propertyInfo;

        [SerializeField] MonoBehaviour _monoBehaviourModel;
        [SerializeField] ScriptableObject _scriptableObjectModel;
        [SerializeField] string _propertyName;

        protected Object Target => _target;
        protected T UIComponent => _uiComponent;
        protected PropertyInfo PropertyInfo => _propertyInfo;

        private void Awake()
        {
            _target = _monoBehaviourModel;
            if (!_target)
                _target = _scriptableObjectModel;

            if(!_target)
            {
                Debug.LogError(name + " has no target!");
                return;
            }

            if (string.IsNullOrEmpty(_propertyName))
            {
                Debug.LogError(name + " has no property name!");
                return;
            }

            _targetType = _target.GetType().UnderlyingSystemType;
            _propertyInfo = _targetType.GetProperty(_propertyName);
            if (_propertyInfo == null)
            {
                Debug.LogError(_target.name + " has no such property " + _propertyName);
                return;
            }

            _uiComponent = GetComponent<T>();
            _key = _target.GetInstanceID().ToString() + '.' + _propertyName;
        }

        private void OnEnable()
        {
            if (_propertyInfo == null)
                return;

            switch (_propertyInfo.PropertyType.Name)
            {
                //TODO: Need Generic!
                case "String":
                case "Sprite":
                case "Object":
                    {
                        UIBindProxy.BindObjectField(_key, OnChangeValue);
                        break;
                    }
                case "Single":
                    {
                        UIBindProxy.BindFloatField(_key, OnChangeValue);
                        break;
                    }
                case "Int32":
                    {
                        UIBindProxy.BindIntField(_key, OnChangeValue);
                        break;
                    }
                default:
                    {
                        Debug.LogWarning("Type Missed " + _targetType.Name);
                        break;
                    }
            }

            Init();
        }

        protected abstract void Init();

        private void OnDisable()
        {
            if (_propertyInfo == null)
                return;

            switch (_propertyInfo.PropertyType.Name)
            {
                //TODO: Need Generic!
                case "String":
                case "Sprite":
                case "Object":
                    {
                        UIBindProxy.ReleaseObjectField(_key, OnChangeValue);
                        break;
                    }
                case "Single":
                    {
                        UIBindProxy.ReleaseFloatField(_key, OnChangeValue);
                        break;
                    }
                case "Int32":
                    {
                        UIBindProxy.ReleaseIntField(_key, OnChangeValue);
                        break;
                    }
                default:
                    {
                        Debug.LogWarning("Type Missed " + _targetType.Name);
                        break;
                    }
            }
        }

        protected virtual void OnChangeValue(float newValue) => throw new NotImplementedException();
        protected virtual void OnChangeValue(int newValue) => throw new NotImplementedException();
        protected virtual void OnChangeValue(object newValue)  => throw new NotImplementedException();
    }
}
