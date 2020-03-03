using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleControlsUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    Text _header = default;

    [SerializeField]
    GameObject _content = default;

    string _originalText;

    void OnEnable()
    {
        _originalText = _header.text;

        UpdateText();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _content.SetActive(!_content.activeInHierarchy);

        UpdateText();
    }

    void UpdateText()
    {
        if (_content.activeInHierarchy)
            _header.text = "▼ " + _originalText;
        else
            _header.text = "► " + _originalText;
    }
}
