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
	private GameObject blockPrefab;         // 블록 원본 프리팹
	[SerializeField]
	private Transform blockRect;            // 생성한 블록들의 부모 Transform

	
	public List<Node> NodeList { private set; get; }
	public Vector2Int BlockCount { private set; get; }

	private List<Block> blockList;

	private State state = State.Wait;   // 현재 상태 (대기, 이동or병합 진행, 후처리)
	private int currentScore; //현재 점수
	private int highScore; //최고 점수
	private float blockSize; //블록의 크기 (맵 크기에 따라 다르게 설정)
	private void Awake()
    {
        // 블록의 개수를 4x4 크기로 설정
        //BlockCount = new Vector2Int(4, 4);
        // 블록의 개수는 "01Main"에서 선택한 개수로 서정
        int count = PlayerPrefs.GetInt("BlockCount");
		BlockCount = new Vector2Int(count, count);

		// 블록 크기 설정
		blockSize = (1080 - 85 - 25 * (BlockCount.x - 1)) / BlockCount.x;

        // 점수 설정
        currentScore = 0;
		uiController.UpdateCurrentScore(currentScore);
		// 최고 점수설정
		highScore = PlayerPrefs.GetInt("HighScore");
		uiController.UpdateHighScore(highScore);
		// 노드 블록 판 생성, 모든 노드의 정보를 NodeList에 저장
		NodeList = nodeSpawner.SpawnNodes(this, BlockCount, blockSize);

		blockList = new List<Block>();
	}

	private void Start()
	{
		// GridLayoutGroup에 의해 정렬된 UI는 실제 위치가 바뀐 것이 아닌
		// GridLayoutGroup에 의해 위치가 통제되고 있기 때문에
		// 자식 UI들의 위치를 출력했을 때 실제 화면에 보이는 것과 다르게
		// 원본 프리팹의 위치와 동일하게 출력된다.

		// 노드의 위치에 블록을 생성하기 위해 Rebuild로 노드들의 위치를 갱신
		UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(nodeSpawner.GetComponent<RectTransform>());

		foreach (Node node in NodeList)
		{
			node.localPositon = node.GetComponent<RectTransform>().localPosition;
		}

		// 숫자 블록 2개 생성
		SpawnBlockToRandomNode();
		SpawnBlockToRandomNode();
	}

	private void Update()
	{
		// Debug Test용.. 숫자 1을 누르면 블록 생성
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
		// 모든 노드를 탐색해서 블록이 배치되어 있지 않은 노드 목록 반환
		List<Node> emptyNodes = NodeList.FindAll(x => x.placedBlock == null);

		// 블록이 배치되지 않은 노드가 있으면
		if (emptyNodes.Count != 0)
		{
			// 블록이 배치되지 않은 노드 중 임의의 노드 Point 정보를 가져와
			int index = Random.Range(0, emptyNodes.Count);
			Vector2Int point = emptyNodes[index].Point;
			// 해당 노드 위치에 블록 생성
			SpawnBlock(point.x, point.y);
		}
		// 모든 노드에 블록이 배치되어 있으면
		else
		{
			// 게임오버 조건을 검사하고, 게임오버 처리
			if(IsGameOver())
            {
				OnGameOver();
            }
		}
	}

	private void SpawnBlock(int x, int y)
	{
		// 해당 위치에 블록이 배치되어 있으면 return
		if (NodeList[y * BlockCount.x + x].placedBlock != null) return;

		GameObject clone = Instantiate(blockPrefab, blockRect);
		Block block = clone.GetComponent<Block>();
		Node node = NodeList[y * BlockCount.x + x];

		// 블록 크기 설정(맵 크기에 따라 변하는 노드 크기와 동일하게 설정)
		clone.GetComponent<RectTransform>().sizeDelta = new Vector2(blockSize, blockSize);
		// 생성한 블록의 위치를 노드의 위치와 동일하게 설정
		clone.GetComponent<RectTransform>().localPosition = node.localPositon;

		// 블록이 처음 생설될 때 숫자 생성 ( 2 or 4 )
		// block.SetupFirstNumeric();
		block.Setup();

		// 방금 생성한 블록을 노드에 등록
		node.placedBlock = block;

		// 리스트에 블록 정보 저장
		blockList.Add(block);
	}

	/// <summary>
	/// 모든 블록을 direction 방향으로 이동 or 병합 처리
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

		// blockList에 있는 모든 블록을 검사해 Target이 있는 블록은
		// StartMove()로 Target까지 이동하도록 설정
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
	/// node에 배치되어 있는 블록을 direction 방향으로 이동 or 병합 처리
	/// </summary>
	private void BlockProcess(Node node, Direction direction)
	{
		// 현재 노드에 블록이 없으면 종료
		if (node.placedBlock == null) return;

		// direction 방향으로 이동 or 병합할 수 있는지 검사하기 위해 해당 방향에 있는 노드 검사
		Node neighborNode = node.FindTarget(node, direction);
		if (neighborNode != null)
		{
			// 현재 노드와 이웃 노드에 블록이 있고, 두 블록의 값이 같으면 "병합(Combine)"
			if (node.placedBlock != null && neighborNode.placedBlock != null)
			{
				if (node.placedBlock.Numeric == neighborNode.placedBlock.Numeric)
				{
					Combine(node, neighborNode);
				}
			}
			// 이동하려는 방향에 노드는 있지만 블록이 없으면 노드가 비어있기 때문에 "이동(Move)"
			else if (neighborNode != null && neighborNode.placedBlock == null)
			{
				Move(node, neighborNode);
			}
		}
	}

	/// <summary>
	/// from 노드에 있는 블록을 to 노드로 이동
	/// </summary>
	private void Move(Node from, Node to)
	{
		// from 노드에 있는 블록의 목표 노드를 to 노드로 설정
		from.placedBlock.MoveToNode(to);

		if (from.placedBlock != null)
		{
			// from 노드에 있었던 블록을 to 노드에 소속된 것으로 설정
			to.placedBlock = from.placedBlock;
			// from 노드의 블록 정보를 비워준다
			from.placedBlock = null;
		}
	}

	/// <summary>
	/// from 노드에 있는 블록을 to 노드에 있는 블록에 병합
	/// </summary>
	private void Combine(Node from, Node to)
	{
		// from 노드에 있는 블록이 to 노드에 있는 블록에 병합되도록 설정
		from.placedBlock.CombineToNode(to);
		// from 노드의 블록 정보를 비워준다.
		from.placedBlock = null;
		// to 노드의 combined = true로 설정해 병합되는 노드로 설정
		to.combined = true;
	}

	/// <summary>
	/// 블록들의 이동 or 병합 처리가 완료되었을 때 처리
	/// </summary>
	private void UpdateState()
	{
		// 모든 블록의 이동 or 병합 처리가 진행중이면 targetAllNull은 false
		// 모든 블록의 이동 or 병합 처리가 완료되면 targetAllNull은 true
		bool targetAllNull = true;

		// blockList에 있는 모든 블록을 검사해 Target이 null이 아닌 블록이 있으면 targerAllNull = false
		foreach (Block block in blockList)
		{
			if (block.Target != null)
			{
				targetAllNull = false;
				break;
			}
		}

		// targetAllNull 이 true이고, 상태가 Processing일 때는
		// 모든 블록의 이동 or 병합 처리가 완료된 직후
		if (targetAllNull && state == State.Processing)
		{
			// 모든 블록을 탐색해 block.NeedDestroy가 true인 블록을
			// removeBlocks 리스트에 저장
			List<Block> removeBlocks = new List<Block>();
			foreach (Block block in blockList)
			{
				if (block.NeedDestroy)
				{
					removeBlocks.Add(block);
				}

				/*// 현재 게임에서는 별도의 게임 클리어를 처리하지 않는데
				// 게임 클리어를 추가하려면 현재 위치에서 추가해도 된다.
				if ( block.Numeric == 2048 )
				{

				}*/
			}

			//  removeBlocks의 모든 블록을 blockList에서 제외하고, 블록 삭제
			removeBlocks.ForEach(x =>
			{
				currentScore += x.Numeric * 2; //병합으로 인해 삭제되는 블록의 (숫자x2)만큼 점수 획득
				blockList.Remove(x);        // blockList에서 블록 정보 삭제
				Destroy(x.gameObject);      // 블록 오브젝트 삭제
			});

			state = State.End;
		}

		if (state == State.End)
		{
			// 상태를 대기(Wait)로 설정해 다시 터치가 가능하도록 한다
			state = State.Wait;

			// 블록이 배치되지 않은 빈 노들에 새로운 블록 생성
			SpawnBlockToRandomNode();

			// 모든 블록의 이동 or 병합이 종료되면 모든 노드의 combined를 false로 설정
			NodeList.ForEach(x => x.combined = false);

			// 점수 UI 업데이트는 모든 블록의 병합 처리가 완료된 후 호출
			uiController.UpdateCurrentScore(currentScore);
		}
	}

	private bool IsGameOver()
    {
		// 모든 노트 탐색
		foreach(Node node in NodeList)
        {
			// 현재 노드에 블록이 없으면 게임 진행 가능
			if (node.placedBlock == null) return false;

			// 각 노드의 이웃 노드 개수만큼 번복
			for(int i = 0; i<node.NeighborNodes.Length; ++i)
            {
				// 이웃노드가 없으면 건너뛴다. (바깥쪽 라인) 
				if (node.NeighborNodes[i] == null) continue;

				Vector2Int point = node.NeighborNodes[i].Value;
				Node neighborNode = NodeList[point.y * BlockCount.x + point.x];
				//현재 노드와 이웃 노드에 블록이 있고
				if (node.placedBlock != null && neighborNode.placedBlock != null)
                {
					// 두노드에 숫자가 같으면 게임진행가능
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