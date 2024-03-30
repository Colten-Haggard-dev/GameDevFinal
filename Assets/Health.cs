using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float MaxHealth = 100f;
    [SerializeField] private float MaxArmor = 100f;

    private float CurHealth = 100f;
    private float CurArmor = 0f;

    public void Heal(float hp)
    {
        CurHealth = Mathf.Clamp(CurHealth + hp, 0, MaxHealth);
    }

    public void AddArmor(float armor)
    {
        CurArmor = Mathf.Clamp(CurArmor + armor, 0f, MaxArmor);
    }

    public void Damage(Rigidbody rb, float dmg, Vector3 dir)
    {
        float protection = MaxArmor > 0f ? 1f - Mathf.Clamp(CurArmor / MaxArmor, 0f, 0.8f) : 1f;
        float final_dmg = dmg * protection;
        CurHealth -= final_dmg;
        CurArmor = Mathf.Clamp(CurArmor - final_dmg * protection, 0f, MaxArmor);

        if (rb != null)
            rb.AddRelativeForce(dir * final_dmg, ForceMode.Impulse);
    }

    public string GetHealthDisplay()
    {
        return string.Format("{0:F0} : {1:F0}", CurHealth, CurArmor);
    }

    public bool IsAlive()
    {
        return CurHealth > 0f;
    }

    private void Start()
    {
        CurHealth = MaxHealth;
        CurArmor = 0;
    }
}
