using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delight.Cookie
{
	/**
	 * Modern cookie management for PHP
	 *
	 * Cookies are a mechanism for storing data in the client"s web browser and identifying returning clients on subsequent visits
	 *
	 * All cookies that have successfully been set will automatically be included in the global `_COOKIE` array with future requests
	 *
	 * You can set a new cookie using the static method `Cookie::setcookie(...)` which is compatible to PHP"s built-in `setcookie(...)` function
	 *
	 * Alternatively, you can construct an instance of this class, set properties individually, and finally call `save()`
	 *
	 * Note that cookies must always be set before the HTTP headers are sent to the client, i.e. before the actual output starts
	 */
	public sealed class Cookie : Delight.Shim.Shimmed_Full
	{

		/** @var string name prefix indicating that the cookie must be from a secure origin (i.e. HTTPS) and the "secure" attribute must be set */
		public const string PREFIX_SECURE = "__Secure-";
		/** @var string name prefix indicating that the "domain" attribute must *not* be set, the "path" attribute must be "/" and the effects of {@see PREFIX_SECURE} apply as well */
		public const string PREFIX_HOST = "__Host-";
		public const string HEADER_PREFIX = "Set-Cookie: ";
		public const string SAME_SITE_RESTRICTION_NONE = "None";
		public const string SAME_SITE_RESTRICTION_LAX = "Lax";
		public const string SAME_SITE_RESTRICTION_STRICT = "Strict";

		/** @var string the name of the cookie which is also the key for future accesses via `_COOKIE[...]` */
		private string name;
		/** @var mixed|null the value of the cookie that will be stored on the client"s machine */
		private string value;
		/** @var int the Unix timestamp indicating the time that the cookie will expire at, i.e. usually `time() + seconds` */
		private DateTime? expiryTime;
		/** @var string the path on the server that the cookie will be valid for (including all sub-directories), e.g. an empty string for the current directory or `/` for the root directory */
		private string path;
		/** @var string|null the domain that the cookie will be valid for (including subdomains) or `null` for the current host (excluding subdomains) */
		private string domain;
		/** @var bool indicates that the cookie should be accessible through the HTTP protocol only and not through scripting languages */
		private bool httpOnly;
		/** @var bool indicates that the cookie should be sent back by the client over secure HTTPS connections only */
		private bool secureOnly;
		/** @var string|null indicates that the cookie should not be sent along with cross-site requests (either `null`, `None`, `Lax` or `Strict`) */
		private string sameSiteRestriction;

		/**
		 * Prepares a new cookie
		 *
		 * @param string name the name of the cookie which is also the key for future accesses via `_COOKIE[...]`
		 */
		public Cookie(string name, Shim._COOKIE cookieShim, Shim._SESSION sessionShim, Shim._SERVER serverShim)
			:base(cookieShim, sessionShim, serverShim)
		{
			this.name = name;
			this.value = null;
			this.expiryTime = null;
			this.path = "/";
			this.domain = null;
			this.httpOnly = true;
			this.secureOnly = false;
			this.sameSiteRestriction = SAME_SITE_RESTRICTION_LAX;
		}

		/**
		 * Returns the name of the cookie
		 *
		 * @return string the name of the cookie which is also the key for future accesses via `_COOKIE[...]`
		 */
		public string getName()
		{
			return this.name;
		}

		/**
		 * Returns the value of the cookie
		 *
		 * @return mixed|null the value of the cookie that will be stored on the client"s machine
		 */
		public string getValue()
		{
			return this.value;
		}

		/**
		 * Sets the value for the cookie
		 *
		 * @param mixed|null value the value of the cookie that will be stored on the client"s machine
		 * @return static this instance for chaining
		 */
		public Cookie setValue(string value)
		{
			this.value = value;

			return this;
		}

		/**
		 * Returns the expiry time of the cookie
		 *
		 * @return int the Unix timestamp indicating the time that the cookie will expire at, i.e. usually `time() + seconds`
		 */
		public DateTime? getExpiryTime()
		{
			return expiryTime;
		}

		/**
		 * Sets the expiry time for the cookie
		 *
		 * @param int expiryTime the Unix timestamp indicating the time that the cookie will expire at, i.e. usually `time() + seconds`
		 * @return static this instance for chaining
		 */
		public Cookie setExpiryTime(DateTime? expiryTime)
		{
			this.expiryTime = expiryTime;

			return this;
		}

		/**
		 * Returns the maximum age of the cookie (i.e. the remaining lifetime)
		 *
		 * @return int the maximum age of the cookie in seconds
		 */
		public int getMaxAge()
		{
			if (this.expiryTime == null) return 0;
			else return (int)((this.expiryTime - DateTime.Now).Value.TotalSeconds);
		}

		/**
		 * Sets the expiry time for the cookie based on the specified maximum age (i.e. the remaining lifetime)
		 *
		 * @param int maxAge the maximum age for the cookie in seconds
		 * @return static this instance for chaining
		 */
		public Cookie setMaxAge(int maxAge)
		{
			this.expiryTime = DateTime.Now.AddSeconds(maxAge);
			return this;
		}

		/**
		 * Returns the path of the cookie
		 *
		 * @return string the path on the server that the cookie will be valid for (including all sub-directories), e.g. an empty string for the current directory or `/` for the root directory
		 */
		public string getPath()
		{
			return this.path;
		}

		/**
		 * Sets the path for the cookie
		 *
		 * @param string path the path on the server that the cookie will be valid for (including all sub-directories), e.g. an empty string for the current directory or `/` for the root directory
		 * @return static this instance for chaining
		 */
		public Cookie setPath(string path)
		{
			this.path = path;

			return this;
		}

		/**
		 * Returns the domain of the cookie
		 *
		 * @return string|null the domain that the cookie will be valid for (including subdomains) or `null` for the current host (excluding subdomains)
		 */
		public string getDomain()
		{
			return this.domain;
		}

		/**
		 * Sets the domain for the cookie
		 *
		 * @param string|null domain the domain that the cookie will be valid for (including subdomains) or `null` for the current host (excluding subdomains)
		 * @return static this instance for chaining
		 */
		public Cookie setDomain(string domain = null)
		{
			this.domain = normalizeDomain(domain);

			return this;
		}

		/**
		 * Returns whether the cookie should be accessible through HTTP only
		 *
		 * @return bool whether the cookie should be accessible through the HTTP protocol only and not through scripting languages
		 */
		public bool isHttpOnly()
		{
			return this.httpOnly;
		}

		/**
		 * Sets whether the cookie should be accessible through HTTP only
		 *
		 * @param bool httpOnly indicates that the cookie should be accessible through the HTTP protocol only and not through scripting languages
		 * @return static this instance for chaining
		 */
		public Cookie setHttpOnly(bool httpOnly)
		{
			this.httpOnly = httpOnly;

			return this;
		}

		/**
		 * Returns whether the cookie should be sent over HTTPS only
		 *
		 * @return bool whether the cookie should be sent back by the client over secure HTTPS connections only
		 */
		public bool isSecureOnly()
		{
			return this.secureOnly;
		}

		/**
		 * Sets whether the cookie should be sent over HTTPS only
		 *
		 * @param bool secureOnly indicates that the cookie should be sent back by the client over secure HTTPS connections only
		 * @return static this instance for chaining
		 */
		public Cookie setSecureOnly(bool secureOnly)
		{
			this.secureOnly = secureOnly;

			return this;
		}

		/**
		 * Returns the same-site restriction of the cookie
		 *
		 * @return string|null whether the cookie should not be sent along with cross-site requests (either `null`, `None`, `Lax` or `Strict`)
		 */
		public string getSameSiteRestriction()
		{
			return this.sameSiteRestriction;
		}

		/**
		 * Sets the same-site restriction for the cookie
		 *
		 * @param string|null sameSiteRestriction indicates that the cookie should not be sent along with cross-site requests (either `null`, `None`, `Lax` or `Strict`)
		 * @return static this instance for chaining
		 */
		public Cookie setSameSiteRestriction(string sameSiteRestriction)
		{
			this.sameSiteRestriction = sameSiteRestriction;

			return this;
		}

		/**
		 * Saves the cookie
		 *
		 * @return bool whether the cookie header has successfully been sent (and will *probably* cause the client to set the cookie)
		 */
		public bool save()
		{
			return addHttpHeader(this._ToString());
		}

		/**
		 * Saves the cookie and immediately creates the corresponding variable in the superglobal `_COOKIE` array
		 *
		 * The variable would otherwise only be available starting from the next HTTP request
		 *
		 * @return bool whether the cookie header has successfully been sent (and will *probably* cause the client to set the cookie)
		 */
		public bool saveAndSet()
		{
			_COOKIE.Set(this.name, this);

			return this.save();
		}

		/**
		 * Deletes the cookie
		 *
		 * @return bool whether the cookie header has successfully been sent (and will *probably* cause the client to delete the cookie)
		 */
		public bool delete()
		{
			// create a temporary copy of this cookie so that it isn"t corrupted
			var copiedCookie = this.Clone();
			// set the copied cookie"s value to an empty string which internally sets the required options for a deletion
			copiedCookie.setValue("");

			// save the copied "deletion" cookie
			return copiedCookie.save();
		}

		Cookie Clone()
		{
			return new Cookie(this.name, _COOKIE, _SESSION, _SERVER)
			{
				domain = this.domain,
				expiryTime = this.expiryTime,
				httpOnly = this.httpOnly,
				name = this.name,
				path = this.path,
				sameSiteRestriction = this.sameSiteRestriction,
				secureOnly = this.secureOnly,
				value = this.value,
				//_COOKIE = this._COOKIE,
				//_SERVER = this._SERVER,
				//_SESSION = this._SESSION;
			};
		}

		/**
		 * Deletes the cookie and immediately removes the corresponding variable from the superglobal `_COOKIE` array
		 *
		 * The variable would otherwise only be deleted at the start of the next HTTP request
		 *
		 * @return bool whether the cookie header has successfully been sent (and will *probably* cause the client to delete the cookie)
		 */
		public bool deleteAndUnset()
		{
			unset(_COOKIE, this.name);

			return this.delete();
		}

		public string _ToString()
		{
			return buildCookieHeader(this.name, this.value, this.expiryTime, this.path, this.domain, this.secureOnly, this.httpOnly, this.sameSiteRestriction);
		}

		/**
		 * Sets a new cookie in a way compatible to PHP"s `setcookie(...)` function
		 *
		 * @param string name the name of the cookie which is also the key for future accesses via `_COOKIE[...]`
		 * @param mixed|null value the value of the cookie that will be stored on the client"s machine
		 * @param int expiryTime the Unix timestamp indicating the time that the cookie will expire at, i.e. usually `time() + seconds`
		 * @param string|null path the path on the server that the cookie will be valid for (including all sub-directories), e.g. an empty string for the current directory or `/` for the root directory
		 * @param string|null domain the domain that the cookie will be valid for (including subdomains) or `null` for the current host (excluding subdomains)
		 * @param bool secureOnly indicates that the cookie should be sent back by the client over secure HTTPS connections only
		 * @param bool httpOnly indicates that the cookie should be accessible through the HTTP protocol only and not through scripting languages
		 * @param string|null sameSiteRestriction indicates that the cookie should not be sent along with cross-site requests (either `null`, `None`, `Lax` or `Strict`)
		 * @return bool whether the cookie header has successfully been sent (and will *probably* cause the client to set the cookie)
		 */
		public bool setcookie(string name, string value = null, DateTime? expiryTime = null, string path = null, string domain = null, bool secureOnly = false, bool httpOnly = false, string sameSiteRestriction = null)
		{
			return addHttpHeader(
				buildCookieHeader(name, value, expiryTime, path, domain, secureOnly, httpOnly, sameSiteRestriction)
			);
		}

		/**
		 * Builds the HTTP header that can be used to set a cookie with the specified options
		 *
		 * @param string name the name of the cookie which is also the key for future accesses via `_COOKIE[...]`
		 * @param mixed|null value the value of the cookie that will be stored on the client"s machine
		 * @param int expiryTime the Unix timestamp indicating the time that the cookie will expire at, i.e. usually `time() + seconds`
		 * @param string|null path the path on the server that the cookie will be valid for (including all sub-directories), e.g. an empty string for the current directory or `/` for the root directory
		 * @param string|null domain the domain that the cookie will be valid for (including subdomains) or `null` for the current host (excluding subdomains)
		 * @param bool secureOnly indicates that the cookie should be sent back by the client over secure HTTPS connections only
		 * @param bool httpOnly indicates that the cookie should be accessible through the HTTP protocol only and not through scripting languages
		 * @param string|null sameSiteRestriction indicates that the cookie should not be sent along with cross-site requests (either `null`, `None`, `Lax` or `Strict`)
		 * @return string the HTTP header
		 */
		public static string buildCookieHeader(string name, string value = null, DateTime? expiryTime_ = null, string path = null, string domain = null, bool secureOnly = false, bool httpOnly = false, string sameSiteRestriction = null)
		{
			int expiryTime = (int)(expiryTime_ is DateTime dtExpiryTime
								? (dtExpiryTime-new DateTime(1970,1,1)).TotalSeconds
								: 0);

			if (!isNameValid(name))
			{
				return null;
			}

			if (!isExpiryTimeValid(expiryTime))
			{
				return null;
			}

			var forceShowExpiry = false;

			if (is_null(value) || value == "") {
				value = "deleted";
				expiryTime = 0;
				forceShowExpiry = true;
			}

			var maxAgeStr = formatMaxAge(expiryTime, forceShowExpiry);
			var expiryTimeStr = formatExpiryTime(expiryTime, forceShowExpiry);

			var headerStr = HEADER_PREFIX + name + "=" + urlencode(value);

			if (!is_null(expiryTimeStr)) {
				headerStr += "; expires=" + expiryTimeStr;
			}

			// The `Max-Age` property is supported on PHP 5.5+ only (https://bugs.php.net/bug.php?id=23955).
				if (!is_null(maxAgeStr)) {
					headerStr += "; Max-Age=" + maxAgeStr;
				}

			if (!empty(path)) {
				headerStr+= "; path="+path;
			}

			if (!empty(domain)) {
				headerStr+= "; domain="+domain;
			}

			if (secureOnly) {
				headerStr+= "; secure";
			}

			if (httpOnly) {
				headerStr+= "; httponly";
			}

			if (sameSiteRestriction == SAME_SITE_RESTRICTION_NONE) {
				// if the "secure" attribute is missing
				if (!secureOnly) {
					trigger_error("When the \"SameSite\" attribute is set to \"None\", the \"secure\" attribute should be set as well", eErrorLevel.E_USER_WARNING);
				}

				headerStr+= "; SameSite=None";
			}
			else if(sameSiteRestriction == SAME_SITE_RESTRICTION_LAX) {
				headerStr+= "; SameSite=Lax";
			}
			else if(sameSiteRestriction == SAME_SITE_RESTRICTION_STRICT) {
				headerStr+= "; SameSite=Strict";
			}

			return headerStr;
		}

		/**
		 * Parses the given cookie header and returns an equivalent cookie instance
		 *
		 * @param string cookieHeader the cookie header to parse
		 * @return \Delight\Cookie\Cookie|null the cookie instance or `null`
		 */
		public static Cookie parse(Shim.Shimmed_Full shim, string cookieHeader)
		{
			if (empty(cookieHeader))
			{
				return null;
			}

			if (string.Compare(cookieHeader.Substring(0, HEADER_PREFIX.Length), HEADER_PREFIX, true) == 0)
				cookieHeader = cookieHeader.Substring(HEADER_PREFIX.Length);

			var kvps = cookieHeader.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(S=>S.Trim()).ToList();

			var cookiepair = kvps.First().Split('=').Select(S => S.Trim()).ToArray();
			kvps.RemoveAt(0);

			var cookie = new Cookie(cookiepair[0], shim._COOKIE, shim._SESSION, shim._SERVER);
			cookie.setPath(null);
			cookie.setHttpOnly(false);
			cookie.setValue(urldecode(cookiepair[1]));
			cookie.setSameSiteRestriction(null);

			if (kvps.Any()) 
			{
				var attributes = kvps;

				foreach (var attribute in attributes) 
				{
					if (strcasecmp(attribute, "HttpOnly") == 0)
						cookie.setHttpOnly(true);
					else if(strcasecmp(attribute, "Secure") == 0)
						cookie.setSecureOnly(true);
					else if(stripos(attribute, "Expires=") == 0)
						cookie.setExpiryTime(DateTime.Parse(substr(attribute, 8)));
					else if(stripos(attribute, "Domain=") == 0)
						cookie.setDomain(substr(attribute, 7));
					else if(stripos(attribute, "Path=") == 0) 
						cookie.setPath(substr(attribute, 5));
					else if(stripos(attribute, "SameSite=") == 0) 
						cookie.setSameSiteRestriction(substr(attribute, 9));
				}
			}

			return cookie;
		}

		/**
		 * Checks whether a cookie with the specified name exists
		 *
		 * @param string name the name of the cookie to check
		 * @return bool whether there is a cookie with the specified name
		 */
		public bool exists(string name) 
		{
			return isset(_COOKIE, name);
		}

		/**
		 * Returns the value from the requested cookie or, if not found, the specified default value
		 *
		 * @param string name the name of the cookie to retrieve the value from
		 * @param mixed defaultValue the default value to return if the requested cookie cannot be found
		 * @return mixed the value from the requested cookie or the default value
		 */
		public string get(string name, string defaultValue = null) {
			if (isset(_COOKIE, name)) {
				return (string)_COOKIE[name].getValue();
			}
			else {
				return defaultValue;
			}
		}

		private static bool isNameValid(string name) {

			if (string.IsNullOrEmpty(name))
				return false;

			//if (!preg_match("/[=,; \\t\\r\\n\\013\\014]/", name, out System.Text.RegularExpressions.MatchCollection _))
			//	{
					return true;
			//	}

			//return false;
		}

		private static bool isExpiryTimeValid(int expiryTime) {
			return is_numeric(expiryTime) || is_null(expiryTime) || is_bool(expiryTime);
		}

		private static int calculateMaxAge(int expiryTime) {
			if (expiryTime == 0) {
				return 0;
			}
			else {
				var maxAge = expiryTime - time();

					if (maxAge < 0) {
						maxAge = 0;
					}

				return maxAge;
			}
		}

		private static string formatExpiryTime(int expiryTime, bool forceShow = false) {
			if (expiryTime > 0 || forceShow) {
				if (forceShow) {
					expiryTime = 1;
				}

				return gmdate("D, d-M-Y H:i:s T", expiryTime);
			}
			else {
				return null;
			}
		}

		private static string formatMaxAge(int expiryTime, bool forceShow = false) {
			if (expiryTime > 0 || forceShow) {
				return calculateMaxAge(expiryTime).ToString();
			}
			else {
				return null;
			}
		}

		private static string normalizeDomain(string domain = null) {

			// if the cookie should be valid for the current host only
			if (string.IsNullOrEmpty(domain)) {
				// no need for further normalization
				return null;
			}

			// if the provided domain is actually an IP address
			if (filter_var(domain, FILTER.FILTER_VALIDATE_IP) != false) {
				// let the cookie be valid for the current host
				return null;
			}

			// for local hostnames (which either have no dot at all or a leading dot only)
			if (strpos(domain, ".") == -1 || strrpos(domain, ".") == 0) {
				// let the cookie be valid for the current host while ensuring maximum compatibility
				return null;
			}

			// unless the domain already starts with a dot
			if (domain[0] != '.') {
				// prepend a dot for maximum compatibility (e.g. with RFC 2109)
				domain = "." + domain;
			}

			// return the normalized domain
			return domain;
		}

		private bool addHttpHeader(string _header) 
		{
			//if (!headers_sent()) {
			//	if (!empty(_header)) {
			
			header(_header, false);
			return true;
			
			//}
			//}
			//return false;
		}

	}
}