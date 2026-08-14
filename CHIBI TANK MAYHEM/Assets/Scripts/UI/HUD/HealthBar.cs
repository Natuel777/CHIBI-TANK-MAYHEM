using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HealthBar
{
    [SerializeField] private Slider _healthBar;
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _lerpSpeed = 5f;
    [SerializeField] private float _colorPulseDuration = 1f;
    [SerializeField] private Color _damageColor = Color.red;
    [SerializeField] private Color _healColor = Color.green;

    [Tooltip("A qué Value del Slider corresponde vida llena. Depende de cómo esté configurado su Direction (ej: con Right To Left, vida llena suele ser 0, no 1).")]
    [SerializeField] private float _fullHealthValue = 1f;
    [Tooltip("A qué Value del Slider corresponde vida vacía.")]
    [SerializeField] private float _emptyHealthValue = 0f;

    private float _maxHealth, _currentDisplayValue = 1f, _targetValue = 1f, _colorPulseTimer = 0f;
    private Color _fillBaseColor;
    private bool _isColorPulsing = false;

    public void Initialize(float maxHealth)
    {
        if(_healthBar == null) return;

        _maxHealth = maxHealth;

        if(_fillImage != null) _fillBaseColor = _fillImage.color;

        _targetValue = _fullHealthValue;
        _currentDisplayValue = _fullHealthValue;
        _healthBar.value = _fullHealthValue;
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if(_healthBar == null) return;

        float normalizedHealth = currentHealth / maxHealth;
        _targetValue = Mathf.Lerp(_emptyHealthValue, _fullHealthValue, normalizedHealth);
    }

    public void TakeDamage(float damage)
    {
        float currentHealth = Mathf.InverseLerp(_emptyHealthValue, _fullHealthValue, _healthBar.value) * _maxHealth;
        float newHealth = Mathf.Max(0f, currentHealth - damage);
        UpdateHealthBar(newHealth, _maxHealth);
        TriggerDamagePulse();
    }

    public void Heal(float amount)
    {
        float currentHealth = Mathf.InverseLerp(_emptyHealthValue, _fullHealthValue, _healthBar.value) * _maxHealth;
        float newHealth = Mathf.Min(_maxHealth, currentHealth + amount);
        UpdateHealthBar(newHealth, _maxHealth);
        TriggerHealPulse();
    }

    public void ArtificialUpdate()
    {
        if(_healthBar == null) return;

        _currentDisplayValue = Mathf.Lerp(_currentDisplayValue, _targetValue, Time.deltaTime * _lerpSpeed);

        if(Mathf.Abs(_currentDisplayValue - _targetValue) < 0.001f)
            _currentDisplayValue = _targetValue;

        _healthBar.value = _currentDisplayValue;

        if(_isColorPulsing)
        {
            _colorPulseTimer -= Time.deltaTime;
            
            if(_colorPulseTimer <= 0f)
            {
                _isColorPulsing = false;
                
                if(_fillImage != null) _fillImage.color = _fillBaseColor;
            }
        }
    }

    public void TriggerDamagePulse()
    {
        _isColorPulsing = true;
        _colorPulseTimer = _colorPulseDuration;
        
        if(_fillImage != null) _fillImage.color = _damageColor;
    }

    public void TriggerHealPulse()
    {
        _isColorPulsing = true;
        _colorPulseTimer = _colorPulseDuration;
        
        if(_fillImage != null) _fillImage.color = _healColor;
    }
}
