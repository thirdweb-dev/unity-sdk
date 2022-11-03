using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Thirdweb;

public class SDKTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void OnLoginClick()
    {
        Debug.Log("Button clicked ");
        string result = await SDK.Initialize();
        Debug.Log("result" + result);
    }
}
