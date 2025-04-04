using Random = UnityEngine.Random;

	public static class LocalPlayerData
	{
		private static string _nickName;
		public static string NickName
		{
			set => _nickName = value;
			get
			{
				if (string.IsNullOrWhiteSpace(_nickName))
				{
					var rngPlayerNumber = Random.Range(0, 9999);
					_nickName = $"Player {rngPlayerNumber.ToString("0000")}";
				}
				return _nickName;
			}
		}
	}
