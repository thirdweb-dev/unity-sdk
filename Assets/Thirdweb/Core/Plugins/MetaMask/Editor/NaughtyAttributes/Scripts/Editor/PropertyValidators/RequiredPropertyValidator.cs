using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MetaMask.Editor.NaughtyAttributes.Editor
{
    public class RequiredPropertyValidator : PropertyValidatorBase
    {
        public override bool ValidateProperty(SerializedProperty property)
        {
            if (!PropertyUtility.IsEnabled(property))
                return true;

            var (messageType, error) = CheckProperty(property);

            if (messageType == MessageType.None) return true;
            
            NaughtyEditorGUI.HelpBox_Layout(error, messageType, context: property.serializedObject.targetObject);

            return false;

        }

        public static (MessageType, string) CheckProperty(SerializedProperty property)
        {
            RequiredAttribute requiredAttribute = PropertyUtility.GetAttribute<RequiredAttribute>(property);

            string defaultErrorMessage = requiredAttribute.Message;
            bool missing = false;
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue == null)
                {
                    missing = true;
                    defaultErrorMessage = property.name + " is required";
                }
            }
            else if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                if (property.managedReferenceValue == null)
                {
                    missing = true;
                    defaultErrorMessage = property.name + " is required";
                }
                if (property.managedReferenceValue is IValidatable validator)
                {
                    missing = validator.IsValid();
                    defaultErrorMessage = property.name + " is invalid";
                }
            }
            else if (property.propertyType == SerializedPropertyType.String)
            {
                if (string.IsNullOrWhiteSpace(property.stringValue))
                {
                    missing = true;
                    defaultErrorMessage = property.name + " is required";
                }
            }
            else if (property.isArray)
            {
                int arrayLength = property.arraySize;

                if (arrayLength == 0)
                {
                    missing = true;
                    defaultErrorMessage = "At least one value in " + property.name + " is required";
                }
            }
            else if (property.propertyType == SerializedPropertyType.Generic)
            {
                var validator = GetIValidatableInstance(property);

                if (validator != null)
                {
                    missing = !validator.IsValid();
                    defaultErrorMessage = property.name + " is invalid";
                }
                else
                {
                    return (MessageType.Warning, requiredAttribute.GetType().Name + " works only on reference types");
                }
            }
            else
            {
                return (MessageType.Warning, requiredAttribute.GetType().Name + " works only on reference types");
            }

            if (missing)
            {
                return (MessageType.Error,
                    string.IsNullOrWhiteSpace(requiredAttribute.Message)
                        ? defaultErrorMessage
                        : requiredAttribute.Message);
            }

            return (MessageType.None, null);
        }
        
        public static IValidatable GetIValidatableInstance(SerializedProperty property)
        {
            // Get the target object (e.g., MonoBehaviour, ScriptableObject) of the serialized property
            object targetObject = property.serializedObject.targetObject;

            // Use reflection to get the field info
            FieldInfo fieldInfo = targetObject.GetType().GetField(property.propertyPath, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (fieldInfo != null)
            {
                // Get the value of the field from the target object
                object fieldValue = fieldInfo.GetValue(targetObject);

                // Try to cast the field value to the IValidatable interface
                IValidatable validatableInstance = fieldValue as IValidatable;

                return validatableInstance;
            }

            return null;
        }
    }
}
