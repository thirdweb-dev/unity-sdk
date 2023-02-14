using UnityEngine;
using TMPro;

public class Debugger : MonoBehaviour
{
    public GameObject debuggerPanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    public static Debugger Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        debuggerPanel.SetActive(false);
    }

    public void Log(string title, string description)
    {
        debuggerPanel.SetActive(true);
        titleText.text = title;
        descriptionText.text = description;
    }
}
