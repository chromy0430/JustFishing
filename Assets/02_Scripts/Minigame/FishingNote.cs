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
    
    private bool  _bossPattern   = false;
    private float _baseSpeed;
    private float _speedTimer    = 0f;
    private float _speedInterval = 0f;
    private float _currentSpeedMult = 1f;
    private bool  _reversing     = false;
    private float _reverseTimer  = 0f;

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
        float minSpeed, float maxSpeed, FishingMinigame minigame)
    {
        _rt           = GetComponent<RectTransform>();
        _image        = GetComponent<Image>();
        _targetPos    = targetPos;
        _minigame     = minigame;
        _bossPattern  = true;
        _fadeEffect   = true;
        _spawnRadius  = 280f;

        // 랜덤 속도
        _baseSpeed    = Random.Range(minSpeed, maxSpeed);
        _speed        = _baseSpeed;

        // 불규칙 속도 변화 타이머 초기화
        ResetSpeedInterval();

        _rt.anchoredPosition = startPos;
        _rt.sizeDelta        = new Vector2(_startSize, _startSize);
    }
    
    private void ResetSpeedInterval()
    {
        // 0.3 ~ 0.8초마다 속도 변화
        _speedInterval = Random.Range(0.3f, 0.8f);
        _speedTimer    = 0f;
    }

    private void Update()
    {
        if (!_isActive) return;

        // 보스 패턴: 불규칙 속도 + 역방향
        if (_bossPattern)
        {
            _speedTimer += Time.deltaTime;

            if (_speedTimer >= _speedInterval)
            {
                ResetSpeedInterval();

                // 20% 확률로 역방향
                if (!_reversing && Random.value < 0.2f)
                {
                    _reversing    = true;
                    _reverseTimer = Random.Range(0.15f, 0.35f); // 역방향 지속 시간
                    _speed        = _baseSpeed * 0.8f;
                }
                else
                {
                    _reversing = false;
                    // 속도를 50% ~ 150% 범위로 랜덤 변화
                    _currentSpeedMult = Random.Range(0.5f, 1.5f);
                    _speed            = _baseSpeed * _currentSpeedMult;
                }
            }

            // 역방향 처리
            if (_reversing)
            {
                _reverseTimer -= Time.deltaTime;

                // 반대 방향으로 이동
                Vector2 awayDir = (_rt.anchoredPosition - _targetPos).normalized;
                _rt.anchoredPosition += awayDir * _speed * Time.deltaTime;

                if (_reverseTimer <= 0f)
                    _reversing = false;
            }
            else
            {
                _rt.anchoredPosition = Vector2.MoveTowards(
                    _rt.anchoredPosition, _targetPos, _speed * Time.deltaTime);
            }
        }
        else
        {
            // 일반 노트
            _rt.anchoredPosition = Vector2.MoveTowards(
                _rt.anchoredPosition, _targetPos, _speed * Time.deltaTime);
        }

        float dist  = Vector2.Distance(_rt.anchoredPosition, _targetPos);
        float ratio = 1f - Mathf.Clamp01(dist / _spawnRadius);
        float size  = Mathf.Lerp(_startSize, _endSize, ratio);
        _rt.sizeDelta = new Vector2(size, size);

        // 페이드 효과
        if (_fadeEffect && _image != null)
            _image.color = new Color(1f, 1f, 1f, 1f - ratio * 0.8f);

        // Miss 유예
        if (!_reversing && dist < 5f)
        {
            if (!_inMissZone) { _inMissZone = true; _missTimer = 0f; }
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