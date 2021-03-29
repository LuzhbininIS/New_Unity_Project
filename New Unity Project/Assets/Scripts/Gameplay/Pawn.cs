using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Pawn : NetworkBehaviour
{
    public PlayerController playerController;

    [SerializeField] private GameObject playerStateUIPrefab;
    private GameObject currentPlayerStateUIGameObject;

    [SerializeField] private GameObject teamADeathParticles;
    [SerializeField] private GameObject teamBDeathParticles;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject mesh;
    [SerializeField] private GameObject markerMesh;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Slider slider;

    [SyncVar(hook =nameof(OnColourChange))] public Color teamColor;

    public Transform muzzleTransform;
    public Transform gunTransform;
    public GameObject gunGfx;
    public GameObject muzzelFlashParticleObject;
    public Material teamAmaterial;
    public Material teamBmaterial;

    [SyncVar] public bool dead;
    [SyncVar(hook =nameof(PlayerHealthUIChanged))] public int health;

    public static int bulletsPerMag = 10;
    [SyncVar] public bool reloading;
    [SyncVar(hook =nameof(BulletsInMagChanged))] public int bulletsInMag;
    [SyncVar] public int totalBulletsLeft;

    public float recoilAnimDuration;
    public float reloadAnimDuration;
    public AnimationCurve recoilEasingFunction;
    public AnimationCurve reloadEasingFunction;

    [SyncVar(hook = nameof(OnColourChange))] public Color teamColour;
    private void OnColourChange(Color oldColor, Color newColor)
    {
        bool isATeam = newColor == Color.red;
        Material mat = (isATeam) ? teamAmaterial : teamBmaterial;
        mesh.GetComponentInChildren<SkinnedMeshRenderer>().material = mat;
        markerMesh.GetComponent<MeshRenderer>().material = mat;
        healthBar.GetComponent<Image>().color = (isATeam) ? Color.red : Color.blue;
    }

    public AudioSource movementCueAudioSource;
    public AudioSource sfxAudioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    public void Shoot(bool hasHitTarget, Vector3 hitPosition)
    {
        totalBulletsLeft--;
        bulletsInMag--;

        GameObject newBullet = Instantiate(bulletPrefab, muzzleTransform.position, muzzleTransform.rotation);
        if(hasHitTarget) newBullet.transform.rotation = Quaternion.LookRotation(hitPosition - newBullet.transform.position);

        NetworkServer.Spawn(newBullet);

        RpcShoot();
    }
    public void Reload()
    {
        int nBulletsCanBeLoaded = Mathf.Min(totalBulletsLeft, bulletsPerMag);
        bulletsInMag = nBulletsCanBeLoaded;
        RpcReload();
    }

    public void TakeDamage(int amount, Vector3 position, Vector3 normal)
    {
        if (dead) return;
        health -= amount;
        RpcTakeDamage(position, normal);

        if(health <= 0)
        {
            playerController.NotifyGameManagerOfDeath();
            dead = true;
        }
    }

    public void RegenHealth()
    {
        health = 100;
    }

    public void ResupplyAmmo()
    {
        bulletsInMag = 10;
        totalBulletsLeft = 100;
    }

    [ClientRpc] void RpcShoot()
    {
        Quaternion localRotate = gunGfx.transform.localRotation;

        LeanTween.rotateLocal(gunGfx, localRotate.eulerAngles + Vector3.right * 30, recoilAnimDuration).setEase(recoilEasingFunction).setOnComplete(() =>
        {
            gunGfx.transform.localRotation = localRotate;
        });
        Instantiate(muzzelFlashParticleObject, muzzleTransform.position, muzzleTransform.rotation);
        sfxAudioSource.PlayOneShot(shootSound);
    }

    [ClientRpc] void RpcReload()
    {
        sfxAudioSource.PlayOneShot(reloadSound);

        Quaternion localRotate = gunGfx.transform.localRotation;

        LeanTween.rotateLocal(gunGfx, localRotate.eulerAngles + Vector3.right * 30 + Vector3.down * 45, reloadAnimDuration).setEase(reloadEasingFunction).setOnComplete(() =>
        {
            gunGfx.transform.localRotation = localRotate;
        });
    }

    [ClientRpc] void RpcTakeDamage(Vector3 position, Vector3 normal)
    { }

    void PlayerHealthUIChanged(int oldValue, int newValue)
    {
        if (currentPlayerStateUIGameObject == null) return;
        currentPlayerStateUIGameObject.GetComponent<PlayerStateUIScript>().SetHealthText(newValue);
        slider.value = newValue;
    }
    void BulletsInMagChanged(int oldValue, int newValue)
    {
        if (currentPlayerStateUIGameObject == null) return;
        currentPlayerStateUIGameObject.GetComponent<PlayerStateUIScript>().SetBulletsText(bulletsInMag, totalBulletsLeft - bulletsInMag);
    }

    public override void OnStartClient()
    {
        if (!hasAuthority) return;
        base.OnStartClient();
        GameObject uiCanvas = GameObject.FindGameObjectWithTag("GameCanvas");
        currentPlayerStateUIGameObject = Instantiate(playerStateUIPrefab, uiCanvas.transform);

        currentPlayerStateUIGameObject.GetComponent<PlayerStateUIScript>().SetHealthText(health);
        currentPlayerStateUIGameObject.GetComponent<PlayerStateUIScript>().SetBulletsText(bulletsInMag, totalBulletsLeft - bulletsInMag);
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        GameObject particleToSpawn = teamColour == Color.red ? teamADeathParticles : teamBDeathParticles;
        Instantiate(particleToSpawn, transform.position, transform.rotation);

        if (!hasAuthority) return;

        if (currentPlayerStateUIGameObject == null) return;
        Destroy(currentPlayerStateUIGameObject);
    }
    
}
