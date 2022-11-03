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

    int count;

    public async void OnLoginClick()
    {
        Debug.Log("Button clicked ");
        count++;
        NFT result = await SDK.GetNFT(count.ToString());
        Debug.Log("name: " + result.metadata.name);
        Debug.Log("owner: " + result.owner);
    }
}
