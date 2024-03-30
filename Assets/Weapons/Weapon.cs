using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct WeaponStats
{
    public float damage;
    public string ammo_type;
    public float fire_rate;
    public float reload_speed;
    public Vector3 recoil;
    public Vector3 max_dev;
    public uint cur_mag;
    public uint max_mag;
    public float range;
    public int slot;
    public int index;
}

public class Weapon : MonoBehaviour, IControllable
{
    private float _curFire = 0f;
    private float _curReload = 0f;
    private WeaponStats WepStats;
    private readonly (WeaponStats stats, int model_idx)[][] WeaponSlots = new (WeaponStats stats, int model_idx)[10][];
    private readonly Dictionary<string, uint> AmmoReserve = new();
    private int _curSlot = 0;
    private int _curSlotIndex = 0;
    private int _lastWeapon = 9;
    private int _numSlots = 0;

    [SerializeField] private string WeaponName;
    [SerializeField] private List<GameObject> WeaponModels = new();

    private static readonly Dictionary<string, WeaponStats> Weapons = new();
    private static LayerMask WeaponMask = new();

    private int _curState = 0;

    private bool _shootSignal = false;
    private bool _reloadSignal = false;

    private Vector3 _drawPos = Vector3.zero;
    private Ray _dbgRay = new();

    static Weapon()
    {
        Weapons["badger"] = new()
        {
            damage = 50f,
            ammo_type = "357",
            fire_rate = 1f,
            reload_speed = 2f,
            recoil = new Vector3(-15f, 0f, 0f),
            max_dev = Vector3.zero,
            cur_mag = 1,
            max_mag = 1,
            range = 1000f
        };

        Weapons["pistol"] = new()
        {
            damage = 10f,
            ammo_type = "9mm",
            fire_rate = 0.2f,
            reload_speed = 1f,
            recoil = new Vector3(-1f, 0f, 0f),
            max_dev = new Vector3(0.1f, 0.05f, 0f),
            cur_mag = 10,
            max_mag = 10,
            range = 1000f
        };

        Weapons["smg"] = new()
        {
            damage = 10f,
            ammo_type = "10mm",
            fire_rate = 0.1f,
            reload_speed = 2f,
            recoil = new Vector3(-2.5f, 0f, 0f),
            max_dev = new Vector3(0.1f, 0.05f, 0f),
            cur_mag = 30,
            max_mag = 30,
            range = 1000f
        };
    }

    public void ResupplyAmmo(string ammo_str, uint amt)
    {
        if (AmmoReserve.ContainsKey(ammo_str))
            AmmoReserve[ammo_str] += amt;
        else
            AmmoReserve[ammo_str] = amt;
    }

    public void AddWeapon(string wname, GameObject model)
    {
        WeaponStats stats = Weapons[wname];
        WeaponModels.Add(model);
        WeaponSlots[stats.slot][stats.index] = (stats, WeaponModels.Count - 1);
    }

    private static (bool, RaycastHit) DamageCast(Vector3 start_pos, Vector3 direction, WeaponStats ws)
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);

        direction += new Vector3(x * ws.max_dev.x, y * ws.max_dev.y, z * ws.max_dev.z);

        bool ret = Physics.Raycast(start_pos, direction, out RaycastHit hit, ws.range, WeaponMask, QueryTriggerInteraction.Ignore);
        return (ret, hit);
    }

    public void Attack()
    {
        (bool hit, RaycastHit ray_hit) = DamageCast(transform.position, transform.forward, WepStats);

        if (transform.parent.TryGetComponent(out PlayerScript ps))
        {
            ps.AddRecoil(WepStats.recoil);
        }

        if (!hit)
        {
            return;
        }

        _drawPos = ray_hit.point;

        GameObject go = ray_hit.collider.gameObject;
        Health target = null;

        while (go != null && go.TryGetComponent(out target) != true)
        {
            go = go.transform.parent == null ? null : go.transform.parent.gameObject;
        }

        if (target != null)
        {
            target.Damage(ray_hit.rigidbody, WepStats.damage, -ray_hit.normal);
        }
        else if (ray_hit.rigidbody != null)
        {
            ray_hit.rigidbody.AddRelativeForce(WepStats.damage * 0.2f * -ray_hit.normal, ForceMode.Impulse);
        }
    }

    public string GetAmmoDisplay()
    {
        return string.Format("{0}/{1}\n{2}", WepStats.cur_mag, WepStats.max_mag, AmmoReserve[WepStats.ammo_type]);
    }

    private void SwitchWeapons(int slot)
    {
        if (slot > _numSlots - 1)
        {
            return;
        }
        
        WeaponSlots[_curSlot][_curSlotIndex].stats = WepStats;

        if (_lastWeapon == slot)
            _curSlotIndex = _curSlotIndex < WeaponSlots[_curSlot].Length - 1 ? _curSlotIndex + 1 : 0;
        else
        {
            _curSlotIndex = 0;
        }

        WepStats = WeaponSlots[slot][_curSlotIndex].stats;
        _curSlot = slot;
        _curFire = WepStats.fire_rate;
        _lastWeapon = _curSlot;

        int mdl_idx = WeaponSlots[_curSlot][_curSlotIndex].model_idx;
        WeaponModels[mdl_idx].SetActive(true);
        for (int i = 0; i < WeaponModels.Count; ++i)
        {
            if (i == mdl_idx)
            {
                continue;
            }

            WeaponModels[i].SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        AmmoReserve["357"] = 36;
        AmmoReserve["9mm"] = 32;
        AmmoReserve["10mm"] = 48;

        WeaponMask.value = LayerMask.GetMask("Default");

        for (int i = 0; i < WeaponSlots.Length; ++i)
        {
            WeaponSlots[i] = new (WeaponStats stats, int model_idx)[10];
        }

        WeaponSlots[0][0].stats = Weapons["badger"];
        WeaponSlots[1][0].stats = Weapons["pistol"];
        WeaponSlots[1][1].stats = Weapons["smg"];

        WeaponSlots[0][0].model_idx = 0;
        WeaponSlots[1][0].model_idx = 1;
        WeaponSlots[1][1].model_idx = 2;

        _numSlots = 2;

        WepStats = WeaponSlots[_curSlot][_curSlotIndex].stats;

        SwitchWeapons(0);
    }

    // Update is called once per frame
    void Update()
    {
        switch (_curState)
        {
            case 0:
                if (_shootSignal && WepStats.cur_mag > 0 && _curFire >= WepStats.fire_rate)
                {
                    _curState = 1;
                    break;
                }

                if (_reloadSignal && (WepStats.cur_mag < WepStats.max_mag) || WepStats.cur_mag == 0)
                {
                    _curState = 2;
                    break;
                }

                _curFire += Time.deltaTime;
                break;
            case 1:
                _curFire = 0f;
                WepStats.cur_mag -= 1;
                Attack();
                _curState = 0;
                break;
            case 2:
                _curReload = 0f;
                _curFire = WepStats.fire_rate;
                _curState = 3;
                break;
            case 3:
                if (_curReload >= WepStats.reload_speed)
                {
                    uint temp = AmmoReserve[WepStats.ammo_type] >= WepStats.max_mag ? WepStats.max_mag - WepStats.cur_mag : AmmoReserve[WepStats.ammo_type];
                    WepStats.cur_mag += temp;
                    AmmoReserve[WepStats.ammo_type] -= temp;
                    _curState = 0;
                    break;
                }

                _curReload += Time.deltaTime;
                break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_drawPos, 0.25f);
        Gizmos.DrawRay(_dbgRay);
        Gizmos.color = Color.white;
    }

    public void SendSignal(params object[] args)
    {
        switch ((int)args[0])
        {
            case 0:
                _shootSignal = (bool)args[1];
                break;
            case 1:
                _reloadSignal = (bool)args[1];
                break;
            case 2:
                SwitchWeapons((int)args[1]);
                break;
        }
    }

    public object Report()
    {
        return null;
    }

    public string StringReport()
    {
        return WeaponName;
    }
}
