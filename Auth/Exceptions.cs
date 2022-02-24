using System;

/*
 * PHP-Auth (https://github.com/delight-im/PHP-Auth)
 * Copyright (c) delight.im (https://www.delight.im/)
 * Licensed under the MIT License (https://opensource.org/licenses/MIT)
 */

namespace Delight.Auth
{
	/** base class for all (checked) exceptions */
	public class AuthException : System.Exception
	{
		public AuthException() : base() { }
		public AuthException(string message) : base(message) { }
	}


	public class AmbiguousUsernameException : AuthException { }
	public class AttemptCancelledException : AuthException { }
	public class ConfirmationRequestNotFound : AuthException { }
	public class DuplicateUsernameException : AuthException
	{
		public DuplicateUsernameException(string message = null) : base(message) { }
	}
	public class EmailNotVerifiedException : AuthException { }
	public class InvalidEmailException : AuthException { }
	public class InvalidPasswordException : AuthException { }
	public class InvalidSelectorTokenPairException : AuthException
	{
		public InvalidSelectorTokenPairException() : base() { }
		public InvalidSelectorTokenPairException(string message) : base(message) { }
	}
	public class NotLoggedInException : AuthException { }
	public class ResetDisabledException : AuthException { }
	public class TokenExpiredException : AuthException { }
	public class TooManyRequestsException : AuthException 
	{
		public TooManyRequestsException() : base() { }
		public TooManyRequestsException(string m) : base(m) { }
		public TooManyRequestsException(string m, int i) : base(m) { }
	}
	public class UnknownIdException : AuthException { }
	public class UnknownUsernameException : AuthException { }
	public class UserAlreadyExistsException : AuthException { }


	/** base class for all (unchecked) errors */
	public class AuthError : System.Exception
	{
		public AuthError() : base() { }
		public AuthError(string message) : base(message) { }
	}
	public class DatabaseError : AuthError
	{
		public DatabaseError(string message) : base(message) { }
	}
	public class EmailOrUsernameRequiredError : AuthError { }
	public class HeadersAlreadySentError : AuthError { }
	public class MissingCallbackError : AuthError { }
}