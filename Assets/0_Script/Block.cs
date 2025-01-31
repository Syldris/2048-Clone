using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class Block : MonoBehaviour
{
    [SerializeField]
    private Color[] blockColors; //����� ���� �� �ִ� ��� ����
    [SerializeField]
    private Image imageBlock; //��� ���� ������ ���� Image
    [SerializeField]
    private TextMeshProUGUI textBlockNumeric; //��� ���� �ؽ�Ʈ ������ ���� Text

    private int numeric; //����� ������ ����(2,4,8...2048)
    private bool combine = false; // �ش� ����� ���� ����
                                  // (��� �̵��� ����Ǿ��� �� ó���� ����)

    // �̵�or�����ϱ� ���� �̵��ϴ� ��ǥ Node
    public Node Target { private set; get; }

    // ��� ����� �̵� or ������ �Ϸ�� ���� �Ѳ����� �����ϱ� ����
    // �ٷ� ���������ʰ� ������ ����� NeeDDestroy=true�� �����صд�.
    public bool NeedDestroy { private set; get; } = false;

    public int Numeric
    {
        set
        {
            // ���� ���� �� ����
            numeric = value;
            // ��Ͽ� ��µǴ� ���� ����
            textBlockNumeric.text = value.ToString();
            //����� ���� ����
            imageBlock.color = blockColors[(int)Mathf.Log(value, 2) - 1];
        }
        get => numeric;
    }

    public void Setup()
    {
        // 0~99���� ������ 90�̸��� ������ 2, 90�̻��� ������ 4
        // �� 90%�� Ȯ���� 2, 10%�� Ȯ���� 4
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
    /// ����� �̵� or ������ ���� Target ��ġ���� �̵��� �� ȣ��
    /// ���� ��ġ���� Target.localPosition ��ġ���� 0.1�� ���� �̵� �ϰ�,
    /// �̵��� �Ϸ�Ǹ� OnEndMove() �޼ҵ� ȣ��
    /// </sumary>
    public void StartMove()
    {
        float moveTime = 0.1f;
        StartCoroutine(OnLocalMoveAnimation(Target.localPositon, moveTime, OnAfterMove));
    }

    private void OnAfterMove()
    {
        //�ش� ����� �ٸ� ��Ͽ� ���յǴ� ����̸�
        if(combine)
        {
            // ����� ���յǾ��� ������ ��ǥ ����� ���ڸ� x2
            Target.placedBlock.Numeric *= 2;
            // ��ǥ ����� Ȯ��/��� �ִϸ��̼� ���
            Target.placedBlock.StartPunchScale(Vector3.one * 0.25f, 0.15f, OnAfterPunchScale);
            // ���� ����� ��Ȱ��ȭ
            gameObject.SetActive(false);
        }
        // �ش� ����� �̵��ϴ� ����̸�
        else
        {
            // ��ǥ(Target) ��ġ���� �̵��� �Ϸ��߱� ������ ��ǥ ����
            Target = null;
        }
    }

    /// <sumary>
    /// ���� ũ��(Vector3.One)���� 1+punch ũ����� Ŀ���ٰ� ���� ũ���
    /// time �ð����� Ȯ�� ����ϰ�, �Ϸ��ϸ� action �޼ҵ� ����
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
    /// ����� ũ�⸦ start���� end���� time�ð����� Ȯ�� or ���
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
    /// ����� ���� ��ġ���� end ��ġ���� time �ð����� �̵�
    /// �̵��� �Ϸ�Ǹ� action �޼ҵ� ȣ��
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
