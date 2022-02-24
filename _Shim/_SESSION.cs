using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delight.Shim
{
	public class _SESSION : BasicDictionaryWrapped<string, object>
	{
		/** @var string session field for whether the client is currently signed in */
		public bool? SESSION_FIELD_LOGGED_IN = null;
		/** @var string session field for the ID of the user who is currently signed in (if any) */
		public int? SESSION_FIELD_USER_ID = null;
		/** @var string session field for the email address of the user who is currently signed in (if any) */
		public string SESSION_FIELD_EMAIL = null;
		/** @var string session field for the display name (if any) of the user who is currently signed in (if any) */
		public string SESSION_FIELD_USERNAME = null;
		/** @var string session field for the status of the user who is currently signed in (if any) as one of the const stringants from the {@see Status} class */
		public Delight.Auth.Status? SESSION_FIELD_STATUS = null;
		/** @var string session field for the roles of the user who is currently signed in (if any) as a bitmask using const stringants from the {@see Role} class */
		public Delight.Auth.Roles? SESSION_FIELD_ROLES = null;
		/** @var string session field for whether the user who is currently signed in (if any) has been remembered (instead of them having authenticated actively) */
		public bool? SESSION_FIELD_REMEMBERED = null;
		/** @var string session field for the UNIX timestamp in seconds of the session data"s last resynchronization with its authoritative source in the database */
		public int? SESSION_FIELD_LAST_RESYNC = null;
		/** @var string session field for the counter that keeps track of forced logouts that need to be performed in the current session */
		public int? SESSION_FIELD_FORCE_LOGOUT = null;

		public _SESSION() : base() { }

		public string _SessionName = null;

		public void Clear()
		{
			Dict.Clear();
		}

		public void session_destroy()
		{
			// TODO
		}

		public void session_regenerate_id(bool deleteOldSession)
		{
			// TODO
		}


		public class CookieParams
		{
			public int lifetime; // - The lifetime of the cookie in seconds.
			public string path; // - The path where information is stored.
			public string domain; // - The domain of the cookie.
			public bool secure; // - The cookie should only be sent over secure connections.
			public bool httponly; // - The cookie can only be accessed through the HTTP protocol.
			public string samesite; // - Controls the cross-domain sending of the cookie.
		}

		public CookieParams session_get_cookie_params()
		{
			return new CookieParams
			{
				lifetime = 0,
				path = "",
				domain = "",
				secure = false,
				httponly = false,
				samesite = ""
			};
		}
	}
}
