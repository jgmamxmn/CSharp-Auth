using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delight.Shim
{
	public abstract class BasicDictionaryWrapped<KeyType, ValType>
	{
		internal Dictionary<KeyType, ValType> Dict;
		protected BasicDictionaryWrapped()
		{
			Dict = new Dictionary<KeyType, ValType>();
		}
		public virtual ValType this[KeyType key]
		{
			get
			{
				if (Dict.TryGetValue(key, out ValType ret))
					return ret;
				return default(ValType);
			}
			set
			{
				if (Dict.ContainsKey(key))
					Dict.Remove(key);
				Dict.Add(key, value);
			}
		}
		public void unset(KeyType key)
		{
			if (Dict.ContainsKey(key))
				Dict.Remove(key);
		}
	}

	public class _COOKIE : BasicDictionaryWrapped<string,Delight.Cookie.Cookie>
	{
		public _COOKIE() : base()
		{
		}
		public Dictionary<string, Delight.Cookie.Cookie> GetLiveCollection() => Dict;
		public void Set(string key, Delight.Cookie.Cookie cookieEntry)
		{
			if (Dict.ContainsKey(key))
				Dict.Remove(key);
			Dict.Add(key, cookieEntry);
		}
	}
}
