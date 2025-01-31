using System.Collections.Generic;
using UnityEngine;

public enum State { Wait = 0, Processing, End }

public class Board : MonoBehaviour
{
	[SerializeField]
	private NodeSpawner nodeSpawner;
	[SerializeField]
	private TouchController touchController;
	[SerializeField]
	private UIController uiController;
	[SerializeField]
	private GameObject blockPrefab;         // ��� ���� ������
	[SerializeField]
	private Transform blockRect;            // ������ ��ϵ��� �θ� Transform

	
	public List<Node> NodeList { private set; get; }
	public Vector2Int BlockCount { private set; get; }

	private List<Block> blockList;

	private State state = State.Wait;   // ���� ���� (���, �̵�or���� ����, ��ó��)
	private int currentScore; //���� ����
	private int highScore; //�ְ� ����
	private float blockSize; //����� ũ�� (�� ũ�⿡ ���� �ٸ��� ����)
	private void Awake()
    {
        // ����� ������ 4x4 ũ��� ����
        //BlockCount = new Vector2Int(4, 4);
        // ����� ������ "01Main"���� ������ ������ ����
        int count = PlayerPrefs.GetInt("BlockCount");
		BlockCount = new Vector2Int(count, count);

		// ��� ũ�� ����
		blockSize = (1080 - 85 - 25 * (BlockCount.x - 1)) / BlockCount.x;

        // ���� ����
        currentScore = 0;
		uiController.UpdateCurrentScore(currentScore);
		// �ְ� ��������
		highScore = PlayerPrefs.GetInt("HighScore");
		uiController.UpdateHighScore(highScore);
		// ��� ��� �� ����, ��� ����� ������ NodeList�� ����
		NodeList = nodeSpawner.SpawnNodes(this, BlockCount, blockSize);

		blockList = new List<Block>();
	}

	private void Start()
	{
		// GridLayoutGroup�� ���� ���ĵ� UI�� ���� ��ġ�� �ٲ� ���� �ƴ�
		// GridLayoutGroup�� ���� ��ġ�� �����ǰ� �ֱ� ������
		// �ڽ� UI���� ��ġ�� ������� �� ���� ȭ�鿡 ���̴� �Ͱ� �ٸ���
		// ���� �������� ��ġ�� �����ϰ� ��µȴ�.

		// ����� ��ġ�� ����� �����ϱ� ���� Rebuild�� ������ ��ġ�� ����
		UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(nodeSpawner.GetComponent<RectTransform>());

		foreach (Node node in NodeList)
		{
			node.localPositon = node.GetComponent<RectTransform>().localPosition;
		}

		// ���� ��� 2�� ����
		SpawnBlockToRandomNode();
		SpawnBlockToRandomNode();
	}

	private void Update()
	{
		// Debug Test��.. ���� 1�� ������ ��� ����
		// if ( Input.GetKeyDown("1") ) SpawnBlockToRondomNode();
		if (state == State.Wait)
		{
			Direction direction = touchController.UpdateTouch();

			if (direction != Direction.None)
			{
				AllBlocksProcess(direction);
			}
		}
		else
		{
			UpdateState();
		}
	}

	private void SpawnBlockToRandomNode()
	{
		// ��� ��带 Ž���ؼ� ����� ��ġ�Ǿ� ���� ���� ��� ��� ��ȯ
		List<Node> emptyNodes = NodeList.FindAll(x => x.placedBlock == null);

		// ����� ��ġ���� ���� ��尡 ������
		if (emptyNodes.Count != 0)
		{
			// ����� ��ġ���� ���� ��� �� ������ ��� Point ������ ������
			int index = Random.Range(0, emptyNodes.Count);
			Vector2Int point = emptyNodes[index].Point;
			// �ش� ��� ��ġ�� ��� ����
			SpawnBlock(point.x, point.y);
		}
		// ��� ��忡 ����� ��ġ�Ǿ� ������
		else
		{
			// ���ӿ��� ������ �˻��ϰ�, ���ӿ��� ó��
			if(IsGameOver())
            {
				OnGameOver();
            }
		}
	}

	private void SpawnBlock(int x, int y)
	{
		// �ش� ��ġ�� ����� ��ġ�Ǿ� ������ return
		if (NodeList[y * BlockCount.x + x].placedBlock != null) return;

		GameObject clone = Instantiate(blockPrefab, blockRect);
		Block block = clone.GetComponent<Block>();
		Node node = NodeList[y * BlockCount.x + x];

		// ��� ũ�� ����(�� ũ�⿡ ���� ���ϴ� ��� ũ��� �����ϰ� ����)
		clone.GetComponent<RectTransform>().sizeDelta = new Vector2(blockSize, blockSize);
		// ������ ����� ��ġ�� ����� ��ġ�� �����ϰ� ����
		clone.GetComponent<RectTransform>().localPosition = node.localPositon;

		// ����� ó�� ������ �� ���� ���� ( 2 or 4 )
		// block.SetupFirstNumeric();
		block.Setup();

		// ��� ������ ����� ��忡 ���
		node.placedBlock = block;

		// ����Ʈ�� ��� ���� ����
		blockList.Add(block);
	}

	/// <summary>
	/// ��� ����� direction �������� �̵� or ���� ó��
	/// </summary>
	private void AllBlocksProcess(Direction direction)
	{
		if (direction == Direction.Right)
		{
			for (int y = 0; y < BlockCount.y; ++y)
			{
				for (int x = (BlockCount.x - 2); x >= 0; --x)
				{
					BlockProcess(NodeList[y * BlockCount.x + x], direction);
				}
			}
		}
		else if (direction == Direction.Down)
		{
			for (int y = (BlockCount.y - 2); y >= 0; --y)
			{
				for (int x = 0; x < BlockCount.x; ++x)
				{
					BlockProcess(NodeList[y * BlockCount.x + x], direction);
				}
			}
		}
		else if (direction == Direction.Left)
		{
			for (int y = 0; y < BlockCount.y; ++y)
			{
				for (int x = 1; x < BlockCount.x; ++x)
				{
					BlockProcess(NodeList[y * BlockCount.x + x], direction);
				}
			}
		}
		else if (direction == Direction.Up)
		{
			for (int y = 1; y < BlockCount.y; ++y)
			{
				for (int x = 0; x < BlockCount.x; ++x)
				{
					BlockProcess(NodeList[y * BlockCount.x + x], direction);
				}
			}
		}

		// blockList�� �ִ� ��� ����� �˻��� Target�� �ִ� �����
		// StartMove()�� Target���� �̵��ϵ��� ����
		foreach (Block block in blockList)
		{
			if (block.Target != null)
			{
				state = State.Processing;
				block.StartMove();
			}
		}

		if(IsGameOver())
        {
			OnGameOver();
        }
	}

	/// <summary>
	/// node�� ��ġ�Ǿ� �ִ� ����� direction �������� �̵� or ���� ó��
	/// </summary>
	private void BlockProcess(Node node, Direction direction)
	{
		// ���� ��忡 ����� ������ ����
		if (node.placedBlock == null) return;

		// direction �������� �̵� or ������ �� �ִ��� �˻��ϱ� ���� �ش� ���⿡ �ִ� ��� �˻�
		Node neighborNode = node.FindTarget(node, direction);
		if (neighborNode != null)
		{
			// ���� ���� �̿� ��忡 ����� �ְ�, �� ����� ���� ������ "����(Combine)"
			if (node.placedBlock != null && neighborNode.placedBlock != null)
			{
				if (node.placedBlock.Numeric == neighborNode.placedBlock.Numeric)
				{
					Combine(node, neighborNode);
				}
			}
			// �̵��Ϸ��� ���⿡ ���� ������ ����� ������ ��尡 ����ֱ� ������ "�̵�(Move)"
			else if (neighborNode != null && neighborNode.placedBlock == null)
			{
				Move(node, neighborNode);
			}
		}
	}

	/// <summary>
	/// from ��忡 �ִ� ����� to ���� �̵�
	/// </summary>
	private void Move(Node from, Node to)
	{
		// from ��忡 �ִ� ����� ��ǥ ��带 to ���� ����
		from.placedBlock.MoveToNode(to);

		if (from.placedBlock != null)
		{
			// from ��忡 �־��� ����� to ��忡 �Ҽӵ� ������ ����
			to.placedBlock = from.placedBlock;
			// from ����� ��� ������ ����ش�
			from.placedBlock = null;
		}
	}

	/// <summary>
	/// from ��忡 �ִ� ����� to ��忡 �ִ� ��Ͽ� ����
	/// </summary>
	private void Combine(Node from, Node to)
	{
		// from ��忡 �ִ� ����� to ��忡 �ִ� ��Ͽ� ���յǵ��� ����
		from.placedBlock.CombineToNode(to);
		// from ����� ��� ������ ����ش�.
		from.placedBlock = null;
		// to ����� combined = true�� ������ ���յǴ� ���� ����
		to.combined = true;
	}

	/// <summary>
	/// ��ϵ��� �̵� or ���� ó���� �Ϸ�Ǿ��� �� ó��
	/// </summary>
	private void UpdateState()
	{
		// ��� ����� �̵� or ���� ó���� �������̸� targetAllNull�� false
		// ��� ����� �̵� or ���� ó���� �Ϸ�Ǹ� targetAllNull�� true
		bool targetAllNull = true;

		// blockList�� �ִ� ��� ����� �˻��� Target�� null�� �ƴ� ����� ������ targerAllNull = false
		foreach (Block block in blockList)
		{
			if (block.Target != null)
			{
				targetAllNull = false;
				break;
			}
		}

		// targetAllNull �� true�̰�, ���°� Processing�� ����
		// ��� ����� �̵� or ���� ó���� �Ϸ�� ����
		if (targetAllNull && state == State.Processing)
		{
			// ��� ����� Ž���� block.NeedDestroy�� true�� �����
			// removeBlocks ����Ʈ�� ����
			List<Block> removeBlocks = new List<Block>();
			foreach (Block block in blockList)
			{
				if (block.NeedDestroy)
				{
					removeBlocks.Add(block);
				}

				/*// ���� ���ӿ����� ������ ���� Ŭ��� ó������ �ʴµ�
				// ���� Ŭ��� �߰��Ϸ��� ���� ��ġ���� �߰��ص� �ȴ�.
				if ( block.Numeric == 2048 )
				{

				}*/
			}

			//  removeBlocks�� ��� ����� blockList���� �����ϰ�, ��� ����
			removeBlocks.ForEach(x =>
			{
				currentScore += x.Numeric * 2; //�������� ���� �����Ǵ� ����� (����x2)��ŭ ���� ȹ��
				blockList.Remove(x);        // blockList���� ��� ���� ����
				Destroy(x.gameObject);      // ��� ������Ʈ ����
			});

			state = State.End;
		}

		if (state == State.End)
		{
			// ���¸� ���(Wait)�� ������ �ٽ� ��ġ�� �����ϵ��� �Ѵ�
			state = State.Wait;

			// ����� ��ġ���� ���� �� ��鿡 ���ο� ��� ����
			SpawnBlockToRandomNode();

			// ��� ����� �̵� or ������ ����Ǹ� ��� ����� combined�� false�� ����
			NodeList.ForEach(x => x.combined = false);

			// ���� UI ������Ʈ�� ��� ����� ���� ó���� �Ϸ�� �� ȣ��
			uiController.UpdateCurrentScore(currentScore);
		}
	}

	private bool IsGameOver()
    {
		// ��� ��Ʈ Ž��
		foreach(Node node in NodeList)
        {
			// ���� ��忡 ����� ������ ���� ���� ����
			if (node.placedBlock == null) return false;

			// �� ����� �̿� ��� ������ŭ ����
			for(int i = 0; i<node.NeighborNodes.Length; ++i)
            {
				// �̿���尡 ������ �ǳʶڴ�. (�ٱ��� ����) 
				if (node.NeighborNodes[i] == null) continue;

				Vector2Int point = node.NeighborNodes[i].Value;
				Node neighborNode = NodeList[point.y * BlockCount.x + point.x];
				//���� ���� �̿� ��忡 ����� �ְ�
				if (node.placedBlock != null && neighborNode.placedBlock != null)
                {
					// �γ�忡 ���ڰ� ������ �������డ��
					if (node.placedBlock.Numeric == neighborNode.placedBlock.Numeric)
					{
						return false;
					}
				}
            }
        }
		return true;
	}

	private void OnGameOver()
    {
		//Debug.Log("GameOver");

		if(currentScore > highScore)
        {
			PlayerPrefs.SetInt("HighScore",currentScore);
        }

		uiController.OnGameOver();
    }
}