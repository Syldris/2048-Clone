using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class Block : MonoBehaviour
{
    [SerializeField]
    private Color[] blockColors; //블록이 가질 수 있는 모든 색상
    [SerializeField]
    private Image imageBlock; //블록 색상 변경을 위한 Image
    [SerializeField]
    private TextMeshProUGUI textBlockNumeric; //블록 숫자 텍스트 변경을 위한 Text

    private int numeric; //블록이 가지는 숫자(2,4,8...2048)
    private bool combine = false; // 해당 블록의 병합 여부
                                  // (블록 이동이 종료되었을 때 처리를 위해)

    // 이동or병합하기 위해 이동하는 목표 Node
    public Node Target { private set; get; }

    // 모든 블록의 이동 or 병합이 완료된 이후 한꺼번에 삭제하기 위해
    // 바로 삭제하지않고 삭제할 블록은 NeeDDestroy=true로 설정해둔다.
    public bool NeedDestroy { private set; get; } = false;

    public int Numeric
    {
        set
        {
            // 실제 숫자 값 변경
            numeric = value;
            // 블록에 출력되는 숫자 설정
            textBlockNumeric.text = value.ToString();
            //블록의 색상 설정
            imageBlock.color = blockColors[(int)Mathf.Log(value, 2) - 1];
        }
        get => numeric;
    }

    public void Setup()
    {
        // 0~99까지 숫자중 90미만이 나오면 2, 90이상이 나오면 4
        // 즉 90%의 확률로 2, 10%의 확률로 4
        Numeric = Random.Range(0, 100) < 90 ? 2 : 4;

        StartCoroutine(OnScaleAnimation(Vector3.one * 0.5f, Vector3.one, 0.15f));
    }

    public void MoveToNode(Node to)
    {
        Target = to;
        combine = false;
    }

    public void CombineToNode(Node to)
    {
        Target = to;
        combine = true;
    }

    /// <sumary>
    /// 블록이 이동 or 병합을 위해 Target 위치까지 이동할 때 호출
    /// 현재 위치에서 Target.localPosition 위치까지 0.1초 동안 이동 하고,
    /// 이동이 완료되면 OnEndMove() 메소드 호출
    /// </sumary>
    public void StartMove()
    {
        float moveTime = 0.1f;
        StartCoroutine(OnLocalMoveAnimation(Target.localPositon, moveTime, OnAfterMove));
    }

    private void OnAfterMove()
    {
        //해당 블록이 다른 블록에 병합되는 블록이면
        if(combine)
        {
            // 블록이 병합되었기 때문에 목표 블록의 숫자를 x2
            Target.placedBlock.Numeric *= 2;
            // 목표 블록의 확대/축소 애니메이션 재생
            Target.placedBlock.StartPunchScale(Vector3.one * 0.25f, 0.15f, OnAfterPunchScale);
            // 현재 블록을 비활성화
            gameObject.SetActive(false);
        }
        // 해당 블록이 이동하는 블록이면
        else
        {
            // 목표(Target) 위치까지 이동을 완료했기 때문에 목표 해제
            Target = null;
        }
    }

    /// <sumary>
    /// 현재 크기(Vector3.One)에서 1+punch 크기까지 커졌다가 원래 크기로
    /// time 시간동안 확대 축소하고, 완료하면 action 메소드 실행
    /// </sumary>
    public void StartPunchScale(Vector3 punch, float time, UnityAction action = null)
    {
        StartCoroutine(OnPunchScale(punch, time, action));
    }
    private void OnAfterPunchScale()
    {
        Target = null;
        NeedDestroy = true;
    }

    private IEnumerator OnPunchScale(Vector3 punch, float time, UnityAction action)
    {
        Vector3 current = Vector3.one;

        yield return StartCoroutine(OnScaleAnimation(current, current + punch, time * 0.5f));

        yield return StartCoroutine(OnScaleAnimation(current + punch, current,time * 0.5f));

        if (action != null) action.Invoke();
    }


    /// <sumary>
    /// 블록의 크기를 start부터 end까지 time시간동안 확대 or 축소
    /// </sumary>
    IEnumerator OnScaleAnimation(Vector3 start, Vector3 end, float time)
    {
        float current = 0;
        float percent = 0;

        while(percent < 1)
        {
            current += Time.deltaTime;
            percent = current / time;

            transform.localScale = Vector3.Lerp(start, end, percent);
            yield return null;
        }

    }

    /// <summary>
    /// 블록을 현재 위치에서 end 위치까지 time 시간동안 이동
    /// 이동이 완료되면 action 메소드 호출
    /// </summary>
    private IEnumerator OnLocalMoveAnimation(Vector3 end, float time, UnityAction action)
    {
        float current = 0;
        float percent = 0;
        Vector3 start = GetComponent<RectTransform>().localPosition;

        while(percent < 1)
        {
            current += Time.deltaTime;
            percent = current / time;

            transform.localPosition = Vector3.Lerp(start, end, percent);
            yield return null;
        }

        if (action != null) action.Invoke();
    }
}
