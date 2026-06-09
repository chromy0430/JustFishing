using UnityEngine;
using UnityEngine.UI;

public class FishingNote : MonoBehaviour
{
    [SerializeField] private Image _noteImage;
    
    private Vector2 _targetPos;
    private float _speed;
    private FishingMinigame _minigame;
    private bool _isActive = true;
    private RectTransform _rt;

    private bool _inPerfectZone = false;
    private bool _inGoodZone = false;

    private float _spawnRadius = 280f;
    private float _startSize = 60f;
    private float _endSize = 90f;

    // Miss 유예 시간
    private bool _inMissZone = false;
    private float _missTimer = 0f;
    private float _missDelay = 0.1f;
    
    // 보스 페이드 효과 패턴
    private bool  _fadeEffect = false;
    private Image _image;

    public void Init(Vector2 startPos, Vector2 targetPos, float speed, FishingMinigame minigame)
    {
        _rt = GetComponent<RectTransform>();
        _image        = GetComponent<Image>();
        _targetPos = targetPos;
        _speed = speed;
        _minigame = minigame;
        _fadeEffect   = false;

        _rt.anchoredPosition = startPos;
        _rt.sizeDelta = new Vector2(_startSize, _startSize);
    }
    
    public void InitBoss(Vector2 startPos, Vector2 targetPos,
        BossNoteData bossData, FishingMinigame minigame)
    {
        _rt           = GetComponent<RectTransform>();
        _image        = GetComponent<Image>();
        _targetPos    = targetPos;
        _minigame     = minigame;
        _fadeEffect   = bossData.fadeEffect;
        _speed        = Random.Range(bossData.minSpeed, bossData.maxSpeed);

        // 랜덤 속도
        _speed = Random.Range(bossData.minSpeed, bossData.maxSpeed);

        _rt.anchoredPosition = startPos;
        _rt.sizeDelta        = new Vector2(_startSize, _startSize);

        if (_image != null)
            _image.color = Color.white;
    }

    private void Update()
    {
        if (!_isActive) return;

        _rt.anchoredPosition = Vector2.MoveTowards(
            _rt.anchoredPosition,
            _targetPos,
            _speed * Time.deltaTime
        );

        // 중심 가까울수록 크기 증가
        float dist = Vector2.Distance(_rt.anchoredPosition, _targetPos);
        float ratio = 1f - Mathf.Clamp01(dist / _spawnRadius);
        
        // 크기 변화
        float size = Mathf.Lerp(_startSize, _endSize, ratio);
        _rt.sizeDelta = new Vector2(size, size);
        
        // 페이드 효과
        if (_fadeEffect && _image != null)
            _image.color = new Color(1f, 1f, 1f, 1f - ratio * 0.8f);

        // 중심 도달 시 Miss 유예
        if (dist < 5f)
        {
            if (!_inMissZone)
            {
                _inMissZone = true;
                _missTimer = 0f;
            }

            _missTimer += Time.deltaTime;

            if (_missTimer >= _missDelay)
            {
                _isActive = false;
                _minigame.RemoveNote(this);
                _minigame.OnNoteJudged(NoteJudgement.Miss);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PerfectZone")) _inPerfectZone = true;
        else if (other.CompareTag("GoodZone")) _inGoodZone = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PerfectZone")) _inPerfectZone = false;
        else if (other.CompareTag("GoodZone")) _inGoodZone = false;
    }

    public NoteJudgement GetCurrentJudgement()
    {
        if (_inPerfectZone) return NoteJudgement.Perfect;
        if (_inGoodZone) return NoteJudgement.Good;
        return NoteJudgement.Miss;
    }

    public float GetDistanceToCenter()
    {
        return Vector2.Distance(_rt.anchoredPosition, _targetPos);
    }

    public void Judge()
    {
        _isActive = false;
        Destroy(gameObject);
    }
    
    public void SetSprite(Sprite sprite)
    {
        if (_noteImage != null)
            _noteImage.sprite = sprite;
    }
}