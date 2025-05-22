using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float FireRate = 2.5f;
    public float damage = 23;
    public float range = 100;
    public GameObject muzzleFlash;
    public AudioClip shotFX;
    public AudioSource _audioSource;
    public Transform bulletSpawn;
    private float nextFire = 0;
    public GameObject hitEfect;
    public Camera _cam;
   

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetButton("Fire1") && Time.time > nextFire) 
        {
            nextFire = Time.time + 1f / FireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        _audioSource.PlayOneShot(shotFX);
        Instantiate(muzzleFlash, bulletSpawn.position, bulletSpawn.rotation);
        RaycastHit hit;

        

        if(Physics.Raycast(_cam.transform.position,_cam.transform.forward, out hit, range))
        {
            Debug.Log("Oh shit, and we go again ");
        }


        GameObject impact = Instantiate(hitEfect, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(impact, 10f);
    }

}
