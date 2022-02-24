using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delight.Cookie
{
	public class Session : Shim.Shimmed_Full
	{
		public Session(Shim._COOKIE cookieShim, Shim._SESSION sessionShim, Shim._SERVER serverShim)
			: base(cookieShim, sessionShim, serverShim)
		{ }

		public static void regenerate(Shim.Shimmed_Full shim, bool deleteOldSession = false, string sameSiteRestriction = Cookie.SAME_SITE_RESTRICTION_LAX)
		{
			// run PHP's built-in equivalent
			shim.session_regenerate_id(deleteOldSession);

			// intercept the cookie header (if any) and rewrite it
			rewriteCookieHeader(shim, sameSiteRestriction);
		}

		private static void rewriteCookieHeader(Shim.Shimmed_Full shim, string sameSiteRestriction = Cookie.SAME_SITE_RESTRICTION_LAX)
		{				
			// get and remove the original cookie header set by PHP
			var originalCookieHeader = Delight.Http.ResponseHeader.take(shim, "Set-Cookie", shim.session_name() + "=");

			// if a cookie header has been found
			if (Shim.Shimmed_PHPOnly.isset(originalCookieHeader))
			{
				// parse it into a cookie instance
				var parsedCookie = Cookie.parse(shim, originalCookieHeader);

				// if the cookie has successfully been parsed
				if (parsedCookie is object)
				{
					// apply the supplied same-site restriction
					parsedCookie.setSameSiteRestriction(sameSiteRestriction);

					if (parsedCookie.getSameSiteRestriction() == Cookie.SAME_SITE_RESTRICTION_NONE 
						&& !parsedCookie.isSecureOnly()) 
					{
						throw new Exception("You may have to enable the 'session.cookie_secure' directive in the configuration in 'php.ini' or via the 'ini_set' function");
					}

					// save the cookie
					parsedCookie.save();
				}
			}
		}
	}
}
