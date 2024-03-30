using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IControllable
{
    void SendSignal(params object[] args);
    object Report();
    string StringReport();
}
