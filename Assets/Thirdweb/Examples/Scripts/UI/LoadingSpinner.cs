using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LoadingSpinner : MonoBehaviour
{
    public Image spinner;

    private void Awake()
    {
        spinner.type = Image.Type.Filled;
        spinner.fillMethod = Image.FillMethod.Radial360;
        spinner.fillAmount = 0;
        spinner.fillClockwise = true;
    }

    private void Update()
    {
        if (spinner.fillClockwise)
        {
            spinner.fillAmount += Time.deltaTime;
            if (spinner.fillAmount >= 1f)
                spinner.fillClockwise = false;
        }
        else
        {
            spinner.fillAmount -= Time.deltaTime;
            if (spinner.fillAmount <= 0f)
                spinner.fillClockwise = true;
        }
    }
}
