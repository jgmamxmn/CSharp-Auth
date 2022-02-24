using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delight
{
	/** Utilities for encoding and decoding data using Base64 and variants thereof */
	public sealed partial class Base64 : Delight.Shim.Shimmed
	{
		/**
		 * The last three characters from the alphabet of the standard implementation
		 *
		 * @var string
		 */
		const string LAST_THREE_STANDARD = "+/=";

		/**
		 * The last three characters from the alphabet of the URL-safe implementation
		 *
		 * @var string
		 */
		const string LAST_THREE_URL_SAFE = "-_~";

		private static string base64_encode(byte[] data) => System.Convert.ToBase64String(data);
		private static byte[] base64_decode(string b64, bool strict) => System.Convert.FromBase64String(b64);

		/**
		 * Encodes the supplied data to Base64
		 *
		 * @param mixed data
		 * @return string
		 * @throws EncodingError if the input has been invalid
		 */
		public static string encode(byte[] data)
		{
			var encoded = base64_encode(data);

			if (encoded == null) {
				throw new EncodingError();
			}

			return encoded;
		}

		/**
		 * Decodes the supplied data from Base64
		 *
		 * @param string data
		 * @return mixed
		 * @throws DecodingError if the input has been invalid
		 */
		public static byte[] decode(string data)
		{
			var decoded = base64_decode(data, true);

			if (decoded == null) {
				throw new DecodingError();
			}

			return decoded;
		}

		/**
		 * Encodes the supplied data to a URL-safe variant of Base64
		 *
		 * @param mixed data
		 * @return string
		 * @throws EncodingError if the input has been invalid
		 */
		public static string encodeUrlSafe(byte[] data)
		{
			var encoded = encode(data);

			return strtr(
				encoded,
				LAST_THREE_STANDARD,
				LAST_THREE_URL_SAFE
			);
		}

		/**
		 * Decodes the supplied data from a URL-safe variant of Base64
		 *
		 * @param string data
		 * @return mixed
		 * @throws DecodingError if the input has been invalid
		 */
		public static byte[] decodeUrlSafe(string data)
		{
			data = strtr(
				data,
				LAST_THREE_URL_SAFE,
				LAST_THREE_STANDARD
			);

			return decode(data);
		}

		/**
		 * Encodes the supplied data to a URL-safe variant of Base64 without padding
		 *
		 * @param mixed data
		 * @return string
		 * @throws EncodingError if the input has been invalid
		 */
		public static string encodeUrlSafeWithoutPadding(byte[] data)
		{
			var encoded = encode(data);

			encoded = rtrim(
				encoded,
				substr(LAST_THREE_STANDARD, -1)
				);

			return strtr(
				encoded,
					substr(LAST_THREE_STANDARD, 0, -1),
					substr(LAST_THREE_URL_SAFE, 0, -1)
				);
		}
		public static string encodeUrlSafeWithoutPadding(string data)
			=> encodeUrlSafeWithoutPadding(Encoding.UTF8.GetBytes(data));

		/**
		 * Decodes the supplied data from a URL-safe variant of Base64 without padding
		 *
		 * @param string data
		 * @return mixed
		 * @throws DecodingError if the input has been invalid
		 */
		public static byte[] decodeUrlSafeWithoutPadding(string data)
		{
			return decodeUrlSafe(data);
		}
	}
}
