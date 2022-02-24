using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Delight.Shim
{
	public abstract class Shimmed
	{
		protected const string __NAMESPACE__ = "Delight";
		protected const int PHP_INT_MAX = int.MaxValue; //~(int)0;

		protected enum PASSWORD_ALGO
		{
			PASSWORD_DEFAULT
		}
		protected string password_hash(string password, PASSWORD_ALGO algoType)
		{
			if (algoType != PASSWORD_ALGO.PASSWORD_DEFAULT) return null;
			string salt = BCrypt.Net.BCrypt.GenerateSalt(10);
			// Tested and works!
			return BCrypt.Net.BCrypt.HashPassword(password ?? string.Empty, salt);
		}
		protected bool password_verify(string password, string hash)
		{
			return !string.IsNullOrEmpty(hash) && BCrypt.Net.BCrypt.Verify(password, hash);
		}
		protected bool password_needs_rehash(string hash)
		{
			return true;
		}
		protected enum HASH_ALGO
		{
			sha256,
			md5
		}
		protected static string hash(HASH_ALGO algo, string text, bool binary=false)
		{
			byte[] ret = null;
			switch(algo)
			{
				case HASH_ALGO.md5:
					using(var md5 = System.Security.Cryptography.MD5.Create())
					{
						ret = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
					}
					break;
				case HASH_ALGO.sha256:
					using(var sha256=System.Security.Cryptography.SHA256.Create())
					{
						ret = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
					}
					break;
				default:
					throw new Exception("Unrecognized hash algorithm");
			}

			if (binary)
				return Encoding.ASCII.GetString(ret);
			else
				return Convert.ToHexString(ret).ToLower();
		}
		protected static string md5(string text, bool binary = false) => hash(HASH_ALGO.md5, text, binary);

		protected static string urldecode(string str) => System.Web.HttpUtility.UrlDecode(str);
		protected static string urlencode(string str) => System.Web.HttpUtility.UrlEncode(str);
		protected static bool empty(object o)
		{
			if (o is string s)
				return string.IsNullOrEmpty(s);
			else if (o is null)
				return false;
			else
				return true;
		}
		protected static bool is_null(object o) => (o is null);
		protected static bool is_numeric(int i) => true;
		protected static bool is_bool(int i) => false;
		public static bool isset(object o)
		{
			if (o is null || o==null)
				return false;
			else if (o is string s)
				return (!string.IsNullOrEmpty(s));
			return true;
		}
		public static bool isset<ValType>(Dictionary<string, ValType> enumerable, string key) => enumerable.ContainsKey(key);
		public static bool isset<ValType>(BasicDictionaryWrapped<string, ValType> enumerable, string key) => enumerable.Dict.ContainsKey(key);
		protected static bool isset(Shim._SESSION s) => true;
		protected static bool is_callable(object o) => (o != null);
		protected static bool empty<T>(IEnumerable<T> en) => (!en?.Any()) ?? true;
		protected static int count<T>(IEnumerable<T> en) => en?.Count() ?? 0;
		protected static string trim(string s) => s?.Trim();
		protected static string trim(string s, string trimChars) => s?.Trim(trimChars.ToArray());
		protected static int strlen(string s) => s?.Length ?? 0;

		/*protected static bool preg_match(string regex, string haystack, bool bIgnoreCase, out System.Text.RegularExpressions.MatchCollection matches)
		{
			var m = System.Text.RegularExpressions.Regex.Matches(haystack, regex,
				(bIgnoreCase ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : System.Text.RegularExpressions.RegexOptions.None));

			var c =m[0]..Captures[0];
			c.

			matches = m;
			return matches.Any();
		}*/
		protected static void unset<T1, T2>(Dictionary<T1, T2> dict, T1 key) => dict.Remove(key);
		protected static void unset<T1, T2>(BasicDictionaryWrapped<T1, T2> dict, T1 key) => dict.unset(key);

		protected static void ignore_user_abort(bool b) { } // NADA
		protected static int floor(int i) => i;
		protected static int floor(float f) => (int)Math.Floor(f);
		protected static int floor(double d) => (int)Math.Floor(d);
		protected static int ceil(float f) => (int)Math.Ceiling(f);
		protected static int ceil(double f) => (int)Math.Ceiling(f);
		protected static int max(int a, int b) => Math.Max(a, b);
		protected static float max(float a, float b) => Math.Max(a, b);
		protected static int min(int a, int b) => Math.Min(a, b);
		protected static float min(float a, float b) => Math.Min(a, b);
		protected static float fmin(double a, double b) => (float)Math.Min(a, b);
		protected static int strcasecmp(string a, string b) => string.Compare(a, b, true);
		protected static int strpos(string haystack, string needle, int offset = 0, StringComparison comp= StringComparison.InvariantCulture)
		{
			if (string.IsNullOrEmpty(haystack))
				return -1;
			else
				return haystack.IndexOf(needle, offset, comp);
		}
		protected static int strrpos(string haystack, string needle, int offset = 0, StringComparison comp = StringComparison.InvariantCulture)
		{
			if (string.IsNullOrEmpty(haystack))
				return -1;
			else
				return haystack.LastIndexOf(needle, offset, comp);
		}
		protected static int stripos(string haystack, string needle, int offset = 0)
			=> strpos(haystack, needle, offset, StringComparison.InvariantCultureIgnoreCase);
		protected static string strtr(string haystack, string from, string to)
		{
			if (string.IsNullOrEmpty(haystack))
				return haystack;
			else
				return haystack.Replace(from, to);
		}
		protected static string rtrim(string haystack, string chars = " \r\n\t\0\v")
		{
			if (string.IsNullOrEmpty(haystack))
				return haystack;
			else
				return haystack.TrimEnd(chars.ToArray());
		}
		protected static string substr(string haystack, int offset, int? length = null)
		{
			if (string.IsNullOrEmpty(haystack))
				return haystack;
			else
			{
				if (offset < 0)
					offset = haystack.Length + offset;

				if (length is int l && l != 0)
				{
					if (l < 0)
					{
						l = haystack.Length - offset + l;
					}
					return haystack.Substring(offset, l);
				}
				else
				{
					return haystack.Substring(offset);
				}
			}
		}
		protected static string str_replace(in string search, in string replace, in string subject)
		{
			if (string.IsNullOrEmpty(subject)) return subject;
			return subject.Replace(search, replace);
		}
		protected static string implode(string connector, IEnumerable<string> parts) => string.Join(connector, parts);
		protected static string[] explode(string separator, string str, int? limit = null)
		{
			if (string.IsNullOrEmpty(str))
				return new string[] { str };
			if (limit is int i)
				return str.Split(separator, i);
			else
				return str.Split(separator);
		}

		protected static byte[] openssl_random_pseudo_bytes(int len)
		{
			var ret = new Span<byte>(new byte[len]);
			System.Security.Cryptography.RandomNumberGenerator.Fill(ret);
			return ret.ToArray();
		}

		private static DateTime epoch = new DateTime(1970, 1, 1);
		protected static int time()
		{
			return (int)(DateTime.Now - epoch).TotalSeconds;
		}
		protected static double microtime(bool as_float)
		{
			return ((DateTime.Now - epoch).TotalMilliseconds)*1000.0; // Not precise, but it's what we have
		}


		public static void header(string headerString, bool replace = true, int responseCode = 0) 
		{
			var h = headerString.Split(new[] { ':' }, 2).Select(S => S.Trim()).ToArray();
			switch(h[0].ToLower())
			{
				case "set-cookie":
					var c = Delight.Cookie.Cookie.parse(headerString);
					_COOKIE.Set(c.getName(), c);
					break;
				default:
					// do nothing?
					break;
			}
		}
		public bool headers_sent() => false;
		public static List<string> headers_list()
		{
			var ret = new List<string>();
			foreach (var c in _COOKIE.Dict)
				ret.Add(c.Value._ToString());
			return ret;
		}
		public static void header_remove(string name)
		{
			if (name == "Set-Cookie")
			{
				Console.WriteLine("Clearing all cookies?");
				_COOKIE.Dict.Clear();
			}
			else
			{
				// N/A
			}
		}

		public static Shim._SERVER _SERVER = new Shim._SERVER();
		public static Shim._COOKIE _COOKIE = new Shim._COOKIE();
		public static Shim._SESSION _SESSION = new Shim._SESSION();

		public string session_name(string newSessionName = null)
		{
			if (newSessionName == null)
				return _SESSION._SessionName;
			else
			{
				var ret = _SESSION._SessionName;
				_SESSION._SessionName = newSessionName;
				return ret;
			}
		}

		protected enum FILTER
		{
			FILTER_VALIDATE_EMAIL,
			FILTER_VALIDATE_IP
		}
		protected static bool filter_var(string inp, FILTER filter)
		{
			switch(filter)
			{
				case FILTER.FILTER_VALIDATE_EMAIL:
					// https://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address
					if (inp.EndsWith("."))
						return false; 
					try
					{
						var addr = new System.Net.Mail.MailAddress(inp);
						return (addr.Address == inp);
					}
					catch
					{
						return false;
					}
				case FILTER.FILTER_VALIDATE_IP:
					try
					{
						_ = System.Net.IPAddress.Parse(inp);
						return true;
					}
					catch
					{
						return false;
					}
				default:
					return true;
			}
		}

		public void session_destroy() => _SESSION.session_destroy();
		public void session_regenerate_id(bool deleteOldSession) => _SESSION.session_regenerate_id(deleteOldSession);
		public _SESSION.CookieParams session_get_cookie_params() => _SESSION.session_get_cookie_params();


		protected enum ARRAY_FILTER_USE_VALUE { x }
		protected enum ARRAY_FILTER_USE_KEY { x }
		protected enum ARRAY_FILTER_USE_BOTH { x }

		protected static Dictionary<KT, VT> array_filter<KT, VT>(Dictionary<KT, VT> inp, Predicate<VT> test, ARRAY_FILTER_USE_VALUE _ = ARRAY_FILTER_USE_VALUE.x)
		{
			var ret = new Dictionary<KT, VT>();
			foreach (var X in inp.Where(KVP => test(KVP.Value)))
				ret.Add(X.Key, X.Value);
			return ret;
		}
		protected static Dictionary<KT,VT> array_filter<KT,VT>(Dictionary<KT,VT> inp, Predicate<KT> test, ARRAY_FILTER_USE_KEY _)
		{
			var ret = new Dictionary<KT, VT>();
			foreach (var X in inp.Where(KVP => test(KVP.Key)))
				ret.Add(X.Key, X.Value);
			return ret;
		}
		protected static Dictionary<KT, VT> array_filter<KT, VT>(Dictionary<KT, VT> inp, Func<VT, KT, bool> test, ARRAY_FILTER_USE_BOTH _)
		{
			var ret = new Dictionary<KT, VT>();
			foreach (var X in inp.Where(KVP => test(KVP.Value, KVP.Key)))
				ret.Add(X.Key, X.Value);
			return ret;
		}
		protected static List<KT> array_keys<KT, VT>(Dictionary<KT, VT> inp) => inp.Keys.ToList();
		protected static List<VT> array_values<KT, VT>(Dictionary<KT, VT> inp) => inp.Values.ToList();
		protected static List<T> array_fill<T>(int start_index_IGNORED, int count, T val)
		{
			var ret = new List<T>();
			for (int i = 0; i < count; ++i)
				ret.Add(val);
			return ret;
		}
		protected static List<TOut> array_map<TIn, TOut>(Func<TIn, TOut> callback, IEnumerable<TIn> array)
			=> array.Select(callback).ToList();
		protected static List<T> array_shift<T>(IEnumerable<T> array)
		{
			var ret = array.ToList();
			ret.RemoveAt(0);
			return ret;
		}

		private static readonly Dictionary<char, string> DateFormatMappings = new Dictionary<char, string>
		{
			// Day	---	---
			{ 'd', "dd"}, // Day of the month, 2 digits with leading zeros	01 to 31
			{ 'D', "ddd"}, // A textual representation of a day, three letters	Mon through Sun
			{ 'j', "d"}, // Day of the month without leading zeros	1 to 31
			{ 'l', "dddd"}, // (lowercase 'L')	A full textual representation of the day of the week	Sunday through Saturday
			// Month	---	---
			{ 'F', "MMMM"}, // A full textual representation of a month, such as January or March	January through December
			{ 'm', "MM"}, // Numeric representation of a month, with leading zeros	01 through 12
			{ 'M', "MMM"}, // A short textual representation of a month, three letters	Jan through Dec
			{ 'n', "M"}, // Numeric representation of a month, without leading zeros	1 through 12
			// Year	---	---
			{ 'o', "yyyy"}, // ISO 8601 week-numbering year. This has the same value as Y, except that if the ISO week number (W) belongs to the previous or next year, that year is used instead.	Examples: 1999 or 2003
			{ 'Y', "yyyy"}, // A full numeric representation of a year, 4 digits	Examples: 1999 or 2003
			{ 'y', "yy"}, // A two digit representation of a year	Examples: 99 or 03
			//Time	---	---
			{ 'A', "tt"}, // Uppercase Ante meridiem and Post meridiem	AM or PM
			{ 'B', "*"}, // Swatch Internet time	000 through 999 - piss off, I'm not supporting this and I don't know why PHP does
			{ 'g', "h"}, // 12-hour format of an hour without leading zeros	1 through 12
			{ 'G', "H"}, // 24-hour format of an hour without leading zeros	0 through 23
			{ 'h', "hh"}, // 12-hour format of an hour with leading zeros	01 through 12
			{ 'H', "HH"}, // 24-hour format of an hour with leading zeros	00 through 23
			{ 'i', "mm"}, // Minutes with leading zeros	00 to 59
			{ 's', "ss"}, // Seconds with leading zeros	00 through 59
			// Timezone	---	---
			// (haven't bothered to support these properly)
			{ 'e', "zzz"}, // Timezone identifier	Examples: UTC, GMT, Atlantic/Azores
			{ 'I', "zzz"}, // (capital i)	Whether or not the date is in daylight saving time	1 if Daylight Saving Time, 0 otherwise.
			{ 'O', "zzz"}, // Difference to Greenwich time (GMT) without colon between hours and minutes	Example: +0200
			{ 'P', "zzz"}, // Difference to Greenwich time (GMT) with colon between hours and minutes	Example: +02:00
			{ 'p', "zzz"}, // The same as P, but returns Z instead of +00:00	Example: +02:00
			{ 'T', "zzz"}, // Timezone abbreviation, if known; otherwise the GMT offset.	Examples: EST, MDT, +05
			{ 'Z', "*"}, // Timezone offset in seconds. The offset for timezones west of UTC is always negative, and for those east of UTC is always positive.	-43200 through 50400

			// Full Date/Time	---	---
			{ 'c', "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz"}, // ISO 8601 date	2004-02-12T15:19:21+00:00
			{ 'r', "ddd, dd MMM yyyy HH:mm:ss zzz"} // » RFC 2822 formatted date	Example: Thu, 21 Dec 2000 16:01:07 +0200
		};
		private static readonly Dictionary<char, Func<DateTime, string>> DateTimeMappingFunctions = new Dictionary<char, Func<DateTime, string>>
		{
			// Day	---	---
			{ 'N', (dt)=>dt.DayOfWeek==0 ? "7" : dt.DayOfWeek.ToString()}, // ISO 8601 numeric representation of the day of the week	1 (for Monday) through 7 (for Sunday)
			{ 'S', (dt)=>dt.Month switch{1=>"st", 2=>"nd", 3=>"rd", 21=>"st", 22=>"nd", 23=>"rd", 31=>"st", _ => "th" } }, // English ordinal suffix for the day of the month, 2 characters	st, nd, rd or th. Works well with j
			{ 'w', (dt)=>dt.DayOfWeek.ToString()}, // Numeric representation of the day of the week	0 (for Sunday) through 6 (for Saturday)
			{ 'z', (dt)=>dt.DayOfYear.ToString()}, // The day of the year (starting from 0)	0 through 365
			// Week	---	---
			{ 'W', (dt)=>
				{
					// Src: https://stackoverflow.com/questions/11154673/get-the-correct-week-number-of-a-given-date
					DayOfWeek day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dt);
					if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
						dt = dt.AddDays(3);
				    return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dt, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday).ToString();
				} }, // ISO 8601 week number of year, weeks starting on Monday	Example: 42 (the 42nd week in the year)
			// Month --- ---
			{ 't', (dt)=>DateTime.DaysInMonth(dt.Year, dt.Month).ToString() }, // Number of days in the given month	28 through 31
			// Year	---	---
			{ 'L', (dt)=>DateTime.IsLeapYear(dt.Year)?"1":"0"}, // Whether it's a leap year	1 if it is a leap year, 0 otherwise.
						//Time	---	---
			{ 'a', (dt)=>dt.Hour<12 ? "am":"pm"}, // Lowercase Ante meridiem and Post meridiem	am or pm
			{ 'u', (dt)=> "000000"}, // Microseconds. Note that date() will always generate 000000 since it takes an int parameter, whereas DateTime::format() does support microseconds if DateTime was created with microseconds.	Example: 654321
			{ 'v', (dt)=>dt.Millisecond.ToString()}, // Milliseconds. Same note applies as for u.	Example: 654
			// Full Date/Time	---	---
			{ 'U', (dt)=>(dt-epoch).TotalSeconds.ToString() } // Seconds since the Unix Epoch (January 1 1970 00:00:00 GMT)	See also time()
		};
		protected static string gmdate(string phpFmt, int unixTimestamp)
		{
			var dt = epoch.AddSeconds(unixTimestamp);
			var dotnetFmt = new StringBuilder();
			foreach(var c in phpFmt)
			{
				if (DateFormatMappings.TryGetValue(c, out string s))
					dotnetFmt.Append("''").Append(s).Append("''");
				else if (DateTimeMappingFunctions.TryGetValue(c, out Func<DateTime, string> fn))
					dotnetFmt.Append(fn(dt));
				else
					dotnetFmt.Append(c);
			}
			return dt.ToString(dotnetFmt.ToString());
		}

		protected enum debug_backtrace_opts
		{
			DEBUG_BACKTRACE_PROVIDE_OBJECT, DEBUG_BACKTRACE_IGNORE_ARGS
		}
		public class debug_backtrace_param
		{
			public string function, file, @class, type;
			public int line;
			public object @object;
			public List<object> args;
		}
		protected static List<debug_backtrace_param> debug_backtrace(debug_backtrace_opts opts_IGNORED= debug_backtrace_opts.DEBUG_BACKTRACE_PROVIDE_OBJECT, int limit_IGNORED = 0)
		{
			// TODO not implemented
			return new List<debug_backtrace_param>();
		}

		protected enum eErrorLevel
		{
			E_USER_NOTICE,
			E_USER_WARNING,
			E_USER_ERROR
		}
		protected static void trigger_error(string message, eErrorLevel error_level)
		{
			var map = new Dictionary<eErrorLevel, string>
			{
				{ eErrorLevel.E_USER_WARNING, "Warning" },
				{ eErrorLevel.E_USER_NOTICE, "Notice" }
			};

			if (error_level == eErrorLevel.E_USER_ERROR)
				throw new Exception(message);
			else
				Console.WriteLine($"{map[error_level]}: {message}");
		}

	}


}

