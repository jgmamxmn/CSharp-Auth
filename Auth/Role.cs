using System;
using System.Collections.Generic;
using System.Linq;

/*
 * PHP-Auth (https://github.com/delight-im/PHP-Auth)
 * Copyright (c) delight.im (https://www.delight.im/)
 * Licensed under the MIT License (https://opensource.org/licenses/MIT)
 */

namespace Delight.Auth {

	[System.Flags]
	public enum Roles
	{
		ADMIN = 1,
		AUTHOR = 2,
		COLLABORATOR = 4,
		CONSULTANT = 8,
		CONSUMER = 16,
		CONTRIBUTOR = 32,
		COORDINATOR = 64,
		CREATOR = 128,
		DEVELOPER = 256,
		DIRECTOR = 512,
		EDITOR = 1024,
		EMPLOYEE = 2048,
		MAINTAINER = 4096,
		MANAGER = 8192,
		MODERATOR = 16384,
		PUBLISHER = 32768,
		REVIEWER = 65536,
		SUBSCRIBER = 131072,
		SUPER_ADMIN = 262144,
		SUPER_EDITOR = 524288,
		SUPER_MODERATOR = 1048576,
		TRANSLATOR = 2097152,
		// XYZ = 4194304,
		// XYZ = 8388608,
		// XYZ = 16777216,
		// XYZ = 33554432,
		// XYZ = 67108864,
		// XYZ = 134217728,
		// XYZ = 268435456,
		// XYZ = 536870912,
	};


	sealed public class Role
	{

		/**
		 * Returns an array mapping the numerical role values to their descriptive names
		 *
		 * @return array
		 */
		public static Dictionary<Roles, string> getMap()
		{
			return new Dictionary<Roles, string>
			{
				{ Roles.ADMIN, "ADMIN" },
				{ Roles.AUTHOR, "AUTHOR" },
				{ Roles.COLLABORATOR, "COLLABORATOR" },
				{ Roles.CONSULTANT, "CONSULTANT" },
				{ Roles.CONSUMER, "CONSUMER" },
				{ Roles.CONTRIBUTOR, "CONTRIBUTOR" },
				{ Roles.COORDINATOR, "COORDINATOR" },
				{ Roles.CREATOR, "CREATOR" },
				{ Roles.DEVELOPER, "DEVELOPER" },
				{ Roles.DIRECTOR, "DIRECTOR" },
				{ Roles.EDITOR, "EDITOR" },
				{ Roles.EMPLOYEE, "EMPLOYEE" },
				{ Roles.MAINTAINER, "MAINTAINER" },
				{ Roles.MANAGER, "MANAGER" },
				{ Roles.MODERATOR, "MODERATOR" },
				{ Roles.PUBLISHER, "PUBLISHER" },
				{ Roles.REVIEWER, "REVIEWER" },
				{ Roles.SUBSCRIBER, "SUBSCRIBER" },
				{ Roles.SUPER_ADMIN, "SUPER_ADMIN" },
				{ Roles.SUPER_EDITOR, "SUPER_EDITOR" },
				{ Roles.SUPER_MODERATOR, "SUPER_MODERATOR" },
				{ Roles.TRANSLATOR, "TRANSLATOR" }

			};

		}

		/**
		 * Returns the descriptive role names
		 *
		 * @return string[]
		 */
		public static string[] getNames()
		{
			return getMap().Values.ToArray();
		}

		/**
		 * Returns the numerical role values
		 *
		 * @return int[]
		 */
		public static Roles[] getValues() 
		{
			return getMap().Keys.ToArray();
		}
	}
}
