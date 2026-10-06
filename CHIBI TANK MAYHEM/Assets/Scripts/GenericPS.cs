using UnityEngine;

public enum PSType
{
    BoomText
}

public class GenericPS : MonoBehaviour
{
    private ParticleSystem _ps;
    [SerializeField] private PSType _type;

    public PSType PSType => _type;

    private void Awake() => _ps = GetComponentInChildren<ParticleSystem>(true);

    public void Initialize() => _ps?.Play();
}
