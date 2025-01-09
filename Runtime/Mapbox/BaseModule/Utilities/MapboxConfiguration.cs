using System;
using MapboxAccountsUnity;
using UnityEngine;

namespace Mapbox.BaseModule.Utilities
{
	public class MapboxConfiguration
	{
		[NonSerialized] private MapboxAccounts mapboxAccounts = new MapboxAccounts();

		public string AccessToken;
		public string GetMapsSkuToken()
		{
			return mapboxAccounts.ObtainMapsSkuUserToken(Application.persistentDataPath);
		}
	}
}
