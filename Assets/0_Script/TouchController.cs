using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TouchController : MonoBehaviour
{
	[SerializeField]
	private float dragDistance = 25;        // 방향 인식을 위한 최소 드래그 거리
	private Vector3 touchStart, touchEnd;   // 터치 시작, 종료 위치
	private bool isTouch = false;       // 이동or병합을 터치할 때마다 1회만 처리하기 위해

	public Direction UpdateTouch()
	{
		Direction direction = Direction.None;

		if (Input.GetMouseButtonDown(0))
		{
			isTouch = true;
			touchStart = Input.mousePosition;
		}
		else if (Input.GetMouseButton(0))
		{
			if (isTouch == false) return Direction.None;

			touchEnd = Input.mousePosition;

			float deltaX = touchEnd.x - touchStart.x;
			float deltaY = touchEnd.y - touchStart.y;

			// 드래그한 거리가 dragDistance보다 작으면 종료
			if (Mathf.Abs(deltaX) < dragDistance && Mathf.Abs(deltaY) < dragDistance) return Direction.None;

			// 더 많이 드래그한 방향으로 이동
			// horizontal 방향으로 이동
			if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
			{
				if (Mathf.Sign(deltaX) >= 0) direction = Direction.Right;
				else direction = Direction.Left;
			}
			// vertical 방향으로 이동
			else
			{
				if (Mathf.Sign(deltaY) >= 0) direction = Direction.Up;
				else direction = Direction.Down;
			}
		}

		// 어느 방향으로든 이동하게 되면 isTouch를 false로 만들어
		// 터치를 종료하고, 다시 터치를 하기 전까지는 실행되지 않도록 한다
		if (direction != Direction.None) isTouch = false;

		return direction;
	}
}
