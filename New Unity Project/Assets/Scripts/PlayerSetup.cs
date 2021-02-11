using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSetup : NetworkBehaviour
{
    private Camera sceneCamera;

    [SerializeField]
    Behaviour[] componentsToDisable;

    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer)
		{
            for (int i = 0; i < componentsToDisable.Length; i++)
			{
                componentsToDisable[i].enabled = false;
            }
		}
        else
		{
            sceneCamera = Camera.main;
            if (sceneCamera != null)
                sceneCamera.gameObject.SetActive(false);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnDisable()
	{
        if (sceneCamera == null)
            sceneCamera.gameObject.SetActive(true);
    }
}
