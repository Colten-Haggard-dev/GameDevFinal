using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HudScript : MonoBehaviour, IControllable
{
    [SerializeField] private TextMeshProUGUI AmmoCount;
    [SerializeField] private TextMeshProUGUI HealthArmor;
    [SerializeField] private Camera ViewmodelCam;

    public void SendSignal(params object[] args)
    {
        switch ((int)args[0])
        {
            case 0:
                HealthArmor.text = (string)args[1];
                break;
            case 1:
                AmmoCount.text = (string)args[1];
                break;
            case 2:
                ViewmodelCam.enabled = (bool)args[1] ? !ViewmodelCam.enabled : ViewmodelCam.enabled;
                break;
        }
    }

    public object Report()
    {
        return null;
    }

    public string StringReport()
    {
        return "HudScript working...";
    }
}
