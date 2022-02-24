using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delight.Http
{
	public class ResponseHeader : Shim.Shimmed_Full
	{
		public ResponseHeader(Shim._COOKIE cookieShim, Shim._SESSION sessionShim, Shim._SERVER serverShim) 
			: base(cookieShim, sessionShim, serverShim)
		{ }

		/**
		 * Returns the header with the specified name (and optional value prefix)
		 *
		 * @param string $name the name of the header
		 * @param string $valuePrefix the optional string to match at the beginning of the header's value
		 * @return string|null the header (if found) or `null`
		 */
		public string get(string name, string valuePrefix = "")
		{
			if (empty(name))
			{
				return null;
			}

			var nameLength = strlen(name);
			var headers = headers_list();

			foreach (var header in headers) 
			{
				if (strcasecmp(substr(header, 0, nameLength + 1), (name + ":")) == 0) 
				{
					var headerValue = trim(substr(header, nameLength + 1), "\t ");

					if (empty(valuePrefix) 
						|| substr(headerValue, 0, strlen(valuePrefix)) == valuePrefix) 
					{
						return header;
					}
				}
			}

			return null;
		}

		/**
		 * Returns the value of the header with the specified name (and optional value prefix)
		 *
		 * @param string $name the name of the header
		 * @param string $valuePrefix the optional string to match at the beginning of the header's value
		 * @return string|null the value of the header (if found) or `null`
		 */
		public string getValue(string name, string valuePrefix = "")
		{
			var header = get(name, valuePrefix);

			if (!empty(header))
			{
				var nameLength = strlen(name);
				var headerValue = substr(header, nameLength + 1);
				headerValue = trim(headerValue, "\t ");

				return headerValue;
			}
			else
			{
				return null;
			}
		}

		/**
		 * Sets the header with the specified name and value
		 *
		 * If another header with the same name has already been set previously, that header will be overwritten
		 *
		 * @param string $name the name of the header
		 * @param string $value the corresponding value for the header
		 */
		public void set(string name, string value)
		{
			header(name + ": " + value, true);
		}

		/**
		 * Adds the header with the specified name and value
		 *
		 * If another header with the same name has already been set previously, both headers (or header values) will be sent
		 *
		 * @param string $name the name of the header
		 * @param string $value the corresponding value for the header
		 */
		public void add(string name, string value)
		{
			header(name + ": " + value, false);
		}

		/**
		 * Removes the header with the specified name (and optional value prefix)
		 *
		 * @param string $name the name of the header
		 * @param string $valuePrefix the optional string to match at the beginning of the header's value
		 * @return bool whether a header, as specified, has been found and removed
		 */
		public bool remove(string name, string valuePrefix = "")
		{
			return take(this, name, valuePrefix) != null;
		}

		/**
		 * Returns and removes the header with the specified name (and optional value prefix)
		 *
		 * @param string $name the name of the header
		 * @param string $valuePrefix the optional string to match at the beginning of the header's value
		 * @return string|null the header (if found) or `null`
		 */
		public static string take(Shim.Shimmed_Full shim, string name, string valuePrefix = "")
		{
			if (empty(name))
			{
				return null;
			}

			var nameLength = strlen(name);
			var headers = shim.headers_list();

			string first = null;
			var homonyms = new List<string>();

			foreach (var header in headers) 
			{
				if (strcasecmp(substr(header, 0, nameLength + 1), (name + ":")) == 0) 
				{
					var headerValue = trim(substr(header, nameLength + 1), "\t ");

					if ((empty(valuePrefix) || substr(headerValue, 0, strlen(valuePrefix)) == valuePrefix) && first == null) 
					{
						first = header;
					}
					else
					{
						homonyms.Add(header);
					}
				}
			}

			if (first != null) 
			{
				shim.header_remove(name);

				foreach (var homonym in homonyms)
				{
					shim.header(homonym, false);
				}
			}

			return first;
		}

		/**
		 * Returns the value of and removes the header with the specified name (and optional value prefix)
		 *
		 * @param string $name the name of the header
		 * @param string $valuePrefix the optional string to match at the beginning of the header's value
		 * @return string|null the value of the header (if found) or `null`
		 */
		public string takeValue(string name, string valuePrefix = "")
		{
			var header = take(this, name, valuePrefix);

			if (!empty(header))
			{
				var nameLength = strlen(name);
				var headerValue = substr(header, nameLength + 1);
				headerValue = trim(headerValue, "\t ");

				return headerValue;
			}
			else
			{
				return null;
			}
		}

	}
}
