using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerController : MonoBehaviour
{
    private PlayerScript _playerScript;
    private Health _healthScript;
    private Weapon _weapon;
    private HudScript _hudScript;

    // Start is called before the first frame update
    void Start()
    {
        _playerScript = GetComponent<PlayerScript>();
        _healthScript = GetComponent<Health>();
        _weapon = GetComponentInChildren<Weapon>();
        _hudScript = GetComponentInChildren<HudScript>();
    }

    // Update is called once per frame
    void Update()
    {
        _playerScript.SendSignal(0, Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _playerScript.SendSignal(1, Input.GetButton("Jump"));
        _playerScript.SendSignal(2, Input.GetButton("Crouch"));

        _weapon.SendSignal(0, Input.GetButton("Fire1"));
        _weapon.SendSignal(1, Input.GetButton("Reload"));

        _hudScript.SendSignal(0, _healthScript.GetHealthDisplay());
        _hudScript.SendSignal(1, _weapon.GetAmmoDisplay());
        _hudScript.SendSignal(2, Input.GetButtonUp("HideViewmodels"));

        if (Input.GetKeyUp(KeyCode.Escape))
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
            Application.Quit();
#endif
        }

        if (!_healthScript.IsAlive())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }
    }

    private void OnGUI()
    {
        if (Event.current.isKey && !Event.current.isMouse && Event.current.type == EventType.KeyDown)
        {
            int keycode = (int)Event.current.keyCode;
            keycode -= (int)KeyCode.Alpha0;

            if (keycode >= 0 && keycode <= 9)
            {
                _weapon.SendSignal(2, keycode - 1);
            }
        }
    }
}
