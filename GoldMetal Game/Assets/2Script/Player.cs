using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    // Start is called before the first frame update
    public float speed;
    public GameObject[] Weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public int hasGrenades;
    public Camera followCamera;
    public GameObject grenadeObject;


    public int ammo;
    public int coin;
    public int health;


    public int maxammo;
    public int maxcoin;
    public int maxhealth;
    public int maxhasGrenades;

    float hAxis;
    float vAxis;


    bool rdown;
    bool wdown;
    bool jdown;
    bool fDown;
    bool gDown;
    bool isjump;
    bool isDodge;
    bool isReload;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool isSwap;
    bool isFireReady = true;
    bool isBorder;

    Vector3 moveVec;
    Vector3 DodgeVec;
    Animator anim;
    Rigidbody rigid;

    GameObject nearObject;
    Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay;

    void Start()
    {

    }
    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        Getinput();
        Move();
        Turn();
        Jump();
        Dodge();
        Interation();
        Swap();
        Attack();
        Reload();
        Grenade();

    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }
    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }
    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }
    void Getinput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wdown = Input.GetButton("Walk");
        jdown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        rdown = Input.GetButtonDown("Reload");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
        {
            moveVec = DodgeVec;
        }

        if (isSwap || isReload || !isFireReady)
        {
            moveVec = Vector3.zero;
        }

        if (!isBorder)
        {
            transform.position += moveVec * speed * (wdown ? 0.3f : 1) * Time.deltaTime;
        }
        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wdown);
    }

    void Turn()
    {
        //1 Ű���忡 ���� ȸ��
        transform.LookAt(transform.position + moveVec);
        //2 ���콺�� ���� ȸ��
        if (fDown)
        {
            moveVec = Vector3.zero;
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayhit;
            if (Physics.Raycast(ray, out rayhit, 100))
            {
                Vector3 nextVec = rayhit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if (jdown && moveVec == Vector3.zero && !isjump && !isDodge && !isSwap)
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
            isjump = true;
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");

        }
    }
    void Grenade()
    {
        if (hasGrenades == 0)
            return;

        if (gDown && !isReload && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayhit;
            if (Physics.Raycast(ray, out rayhit, 100))
            {
                Vector3 nextVec = rayhit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(grenadeObject, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }
    void Attack()
    {
        if (equipWeapon == null)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;

        }
    }
    void Reload()
    {
        if (equipWeapon == null)
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;

        if (ammo == 0)
            return;

        if (rdown && !isjump && !isDodge && !isSwap && isFireReady && !isReload)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 3f);
        }
    }
    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }
    void Dodge()
    {
        if (jdown && moveVec != Vector3.zero && !isjump && !isDodge && !isSwap && !fDown)
        {
            DodgeVec = moveVec;
            speed *= 2;
            isDodge = true;
            anim.SetBool("isJump", true);
            anim.SetTrigger("doDodge");


            Invoke("DodgeOut", 0.5f);

        }

    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;

    }

    void SwapOut()
    {
        isSwap = false;
    }
    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;
        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if (sDown1 || sDown2 || sDown3 && !isjump && !isDodge)
        {
            if (equipWeapon != null)
            {
                equipWeapon.gameObject.SetActive(false);
            }


            equipWeaponIndex = weaponIndex;
            equipWeapon = Weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);
            anim.SetTrigger("doSwap");


            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }
    void Interation()
    {
        if (iDown && nearObject != null && !isjump && !isDodge)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int WeaponIndex = item.value;
                hasWeapons[WeaponIndex] = true;

                Destroy(nearObject);
            }

        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            isjump = false;
            anim.SetBool("isJump", false);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxammo)
                        ammo = maxammo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxcoin)
                        coin = maxcoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxhealth)
                        health = maxhealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxhasGrenades)
                        hasGrenades = maxhasGrenades;
                    break;

            }
            Destroy(other.gameObject);
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = other.gameObject;
        }


    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
    }
}