using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WalletConnectUnity.UI
{
    public class WCInputField : TMP_InputField
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _borderImage;
        [SerializeField] private Image _ringImage;

        [SerializeField] private ComponentState _defaultState;
        [SerializeField] private ComponentState _selectedState;
        [SerializeField] private ComponentState _highlightedState;

        public override void OnSelect(BaseEventData eventData)
        {
            // Unity automatically 'selects' the input field after pressed which we don't want when using a cursor
            var currentState = currentSelectionState;
            if (currentState is SelectionState.Highlighted) return;

            base.OnSelect(eventData);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            var targetState = state switch
            {
                SelectionState.Normal => _defaultState,
                SelectionState.Highlighted => _highlightedState,
                SelectionState.Selected => _selectedState,
                _ => _defaultState
            };

            ApplyComponentState(targetState);
        }

        private void ApplyComponentState(ComponentState state)
        {
            _backgroundImage.enabled = state.backgroundEnabled;
            _backgroundImage.color = state.backgroundColor;

            _borderImage.enabled = state.borderEnabled;
            _borderImage.color = state.borderColor;

            _ringImage.enabled = state.ringEnabled;
            _ringImage.color = state.ringColor;
        }
    }
}