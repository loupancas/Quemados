using UnityEngine;
	
	public class BallVisual : MonoBehaviour
	{
		[Header("Ball Setup")]
		public float Speed = 80f;
		public float MaxDistance = 100f;
		public float LifeTimeAfterHit = 2f;

		[Header("Impact Setup")]
		public GameObject BallObject;
		

		private Vector3 _startPosition;
		private Vector3 _targetPosition;
		private Vector3 _hitNormal;		

		private float _startTime;
		private float _duration;

	
		public void SetHit(Vector3 hitPosition, Vector3 hitNormal)
		{
			_targetPosition = hitPosition;
			_hitNormal = hitNormal;
		}

		private void Start()
		{
			_startPosition = transform.position;

			if (_targetPosition == Vector3.zero)
			{
				_targetPosition = _startPosition + transform.forward * MaxDistance;
			}

			_duration = Vector3.Distance(_startPosition, _targetPosition) / Speed;
			_startTime = Time.timeSinceLevelLoad;
		}

		private void Update()
		{
			float time = Time.timeSinceLevelLoad - _startTime;

			if (time < _duration)
			{
				transform.position = Vector3.Lerp(_startPosition, _targetPosition, time / _duration);
			}
			else
			{
				transform.position = _targetPosition;
                Destroy(gameObject, LifeTimeAfterHit);
             }
		}

		
	}

