using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NodeSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject nodePrefab;  //��� ��� ���� ������
    [SerializeField]
    private RectTransform nodeRect; //������ ��Ʈ���� �θ� RectTransfrom
    [SerializeField]
    private GridLayoutGroup gridLayoutGroup; //��ũ�⿡ ���� ��� ũ�� ����
    public List<Node> SpawnNodes(Board board,Vector2Int blockCount, float blockSize)
    {
        //Vector2Int blockCount = new Vector2Int(4, 4);
        gridLayoutGroup.cellSize = Vector2.one * blockSize;

        List<Node> nodeList = new List<Node>(blockCount.x * blockCount.y);

        for(int y = 0; y<blockCount.y; ++y)
        {
            for(int x = 0; x< blockCount.x; ++x)
            {
                //��� �ǿ� ��ġ�Ǵ� ��� ����
                GameObject clone = Instantiate(nodePrefab, nodeRect.transform);
                // ���� ����� x, y ���� ��ǥ ����
                Vector2Int point = new Vector2Int(x, y);

                // ���� ��� ���� ����(���� ��尡 ������ null ����)
                Vector2Int?[] neighborNodes = new Vector2Int?[4];
                //GridLayoutGroup�� StartCorner�� UpperLeft�� ��� y���� up������ -, down ������ + �̱� ������
                //down ������ Vector2Int.up ���� up ������ Vector2Int.down ���� �����ش�.
                Vector2Int right = point + Vector2Int.right;
                Vector2Int down = point + Vector2Int.up;
                Vector2Int left = point + Vector2Int.left;
                Vector2Int up = point + Vector2Int.down;

                if (IsValid(right, blockCount)) neighborNodes[0] = right;
                if (IsValid(down, blockCount)) neighborNodes[1] = down;
                if (IsValid(left, blockCount)) neighborNodes[2] = left;
                if (IsValid(up, blockCount)) neighborNodes[3] = up;

                // ��� ������ ����� Setup() �޼ҵ� ȣ��
                Node node = clone.GetComponent<Node>();
                node.Setup(board,neighborNodes,point);

                // ����� �̸��� x,y ���� ��ǥ�� ����
                clone.name = $"[{node.Point.y},{node.Point.x}]";

                // nodeList�� ��� ������ ��� ���� ����
                nodeList.Add(node);
            }
        }

        return nodeList;
    }

    private bool IsValid(Vector2Int point, Vector2Int blockCount)
    {
        if(point.x == -1 || point.x == blockCount.x || point.y == blockCount.y || point.y == -1)
        {
            return false;
        }
        return true;
    }
}
