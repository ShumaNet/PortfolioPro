using UnityEngine;

public class SpikeTrapMechanick : MonoBehaviour
{
   public float cooldownTime = 3f;
    bool isBusy = false;
    [SerializeField] GameObject Trap;

    private void OnTriggerEnter(Collider other)
    {
        if (!isBusy) 
        {
            ActivateTrap();
        } 
    }

    void ActivateTrap()
    {
        isBusy = true;
        Animator spikeTrapAnimator = Trap.GetComponent<Animator>();
        spikeTrapAnimator.SetTrigger("Activate");
        Invoke("ResetBusy", cooldownTime);
    }

    void ResetBusy()
    {
        isBusy = false; 
    }
}
