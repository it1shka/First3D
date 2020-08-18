using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[
    RequireComponent(typeof(PlayerController)),
    RequireComponent(typeof(PlayerControllerMobile))
]
public class ControllerSwitcher : MonoBehaviour
{
    private PlayerControllerMobile pcm;
    private PlayerController pc;
    private bool mobile;
    void Start()
    {
        pcm = GetComponent<PlayerControllerMobile>();
        pc = GetComponent<PlayerController>();
#if UNITY_ANDROID && !UNITY_EDITOR
        pcm.enabled = true;
        Destroy(pc);
        Destroy(this);
#endif
        mobile = true;
        pcm.enabled = true;
        pc.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (mobile)
            {
                pcm.enabled = false;
                pc.enabled = true;
                mobile = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                pcm.enabled = true;
                pc.enabled = false;
                mobile = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
