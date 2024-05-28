using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WalletConnectUnity.UI
{
    public class WCTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private RectTransform _rootTransform;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _hoverColor;
        [SerializeField] private Color _selectedColor;

        private bool _isSelected;

        public event EventHandler Clicked;

        public RectTransform RootTransform => _rootTransform;
        public TMP_Text Label => _label;

        public bool IsVisible => gameObject.activeSelf;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _label.color = _hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _label.color = _isSelected ? _selectedColor : _normalColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }

        public void Select()
        {
            _isSelected = true;
            _label.color = _selectedColor;
        }

        public void Deselect()
        {
            _isSelected = false;
            _label.color = _normalColor;
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);
    }
}