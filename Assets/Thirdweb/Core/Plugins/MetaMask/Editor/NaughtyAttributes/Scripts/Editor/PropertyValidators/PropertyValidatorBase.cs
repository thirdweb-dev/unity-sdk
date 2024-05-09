using System;
using System.Collections.Generic;
using UnityEditor;

namespace MetaMask.Editor.NaughtyAttributes.Editor
{
    public abstract class PropertyValidatorBase
    {
        public abstract bool ValidateProperty(SerializedProperty property);
    }

    public static class ValidatorAttributeExtensions
    {
        private static Dictionary<Type, PropertyValidatorBase> _validatorsByAttributeType;

        static ValidatorAttributeExtensions()
        {
            _validatorsByAttributeType = new Dictionary<Type, PropertyValidatorBase>();
            _validatorsByAttributeType[typeof(RequiredAttribute)] = new RequiredPropertyValidator();
        }

        public static PropertyValidatorBase GetValidator(this ValidatorAttribute attr)
        {
            PropertyValidatorBase validator;
            if (_validatorsByAttributeType.TryGetValue(attr.GetType(), out validator))
            {
                return validator;
            }
            else
            {
                return null;
            }
        }
    }
}
