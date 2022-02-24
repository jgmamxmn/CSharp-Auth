using System;
using System.Collections.Generic;
using System.Text;
using Delight.Db;
using System.Linq;

/*
 * PHP-Auth (https://github.com/delight-im/PHP-Auth)
 * Copyright (c) delight.im (https://www.delight.im/)
 * Licensed under the MIT License (https://opensource.org/licenses/MIT)
 */

namespace Delight.Auth
{ 

	/** Component that can be used for administrative tasks by privileged and authorized users */
	sealed public class Administration : UserManager {

		/**
		 * @param PdoDatabase|PdoDsn|\PDO databaseConnection the database connection to operate on
		 * @param string|null dbTablePrefix (optional) the prefix for the names of all database tables used by this component
		 * @param string|null dbSchema (optional) the schema name for all database tables used by this component
		 */
		public Administration(PdoDatabase databaseConnection, string dbTablePrefix, string dbSchema,
			Shim._COOKIE cookieShim, Shim._SESSION sessionShim, Shim._SERVER serverShim)
			: base(databaseConnection, dbTablePrefix, dbSchema,
				  cookieShim, sessionShim, serverShim)
		{ }

		/**
		 * Creates a new user
		 *
		 * @param string email the email address to register
		 * @param string password the password for the new account
		 * @param string|null username (optional) the username that will be displayed
		 * @return int the ID of the user that has been created (if any)
		 * @throws InvalidEmailException if the email address was invalid
		 * @throws InvalidPasswordException if the password was invalid
		 * @throws UserAlreadyExistsException if a user with the specified email address already exists
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 */
		public int createUser(string email, string password, string username = null) {
			return this.createUserInternal(false, email, password, username, null);
		}

		/**
		 * Creates a new user while ensuring that the username is unique
		 *
		 * @param string email the email address to register
		 * @param string password the password for the new account
		 * @param string|null username (optional) the username that will be displayed
		 * @return int the ID of the user that has been created (if any)
		 * @throws InvalidEmailException if the email address was invalid
		 * @throws InvalidPasswordException if the password was invalid
		 * @throws UserAlreadyExistsException if a user with the specified email address already exists
		 * @throws DuplicateUsernameException if the specified username wasn"t unique
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 */
		public int createUserWithUniqueUsername(string email, string password, string username = null) {
			return this.createUserInternal(true, email, password, username, null);
		}

		/**
		 * Deletes the user with the specified ID
		 *
		 * This action cannot be undone
		 *
		 * @param int id the ID of the user to delete
		 * @throws UnknownIdException if no user with the specified ID has been found
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 */
		public void deleteUserById(int id) {
			var numberOfDeletedUsers = this.deleteUsersByColumnValue("id", id);

			if (numberOfDeletedUsers == 0) {
				throw new UnknownIdException();
			}
		}

		/**
		 * Deletes the user with the specified email address
		 *
		 * This action cannot be undone
		 *
		 * @param string email the email address of the user to delete
		 * @throws InvalidEmailException if no user with the specified email address has been found
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 */
		public void deleteUserByEmail(string email) {
			email = validateEmailAddress(email);

			var numberOfDeletedUsers = this.deleteUsersByColumnValue("email", email);

			if (numberOfDeletedUsers == 0) {
				throw new InvalidEmailException();
			}
		}

		/**
		 * Deletes the user with the specified username
		 *
		 * This action cannot be undone
		 *
		 * @param string username the username of the user to delete
		 * @throws UnknownUsernameException if no user with the specified username has been found
		 * @throws AmbiguousUsernameException if multiple users with the specified username have been found
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 */
		public void deleteUserByUsername(string username) {
			var userData = this.getUserDataByUsername(
				trim(username),
				new[] { "id" }
			);

			this.deleteUsersByColumnValue("id", userData.id);
		}

		/**
		 * Assigns the specified role to the user with the given ID
		 *
		 * A user may have any number of roles (i.e. no role at all, a single role, or any combination of roles)
		 *
		 * @param int userId the ID of the user to assign the role to
		 * @param Roles role the role as one of the constants from the {@see Role} class
		 * @throws UnknownIdException if no user with the specified ID has been found
		 *
		 * @see Role
		 */
		public void addRoleForUserById(int userId, Roles role) {
			var userFound = this.addRoleForUserByColumnValue(
				"id",
				userId,
				role
			);

			if (userFound == false) {
				throw new UnknownIdException();
			}
		}

		/**
		 * Assigns the specified role to the user with the given email address
		 *
		 * A user may have any number of roles (i.e. no role at all, a single role, or any combination of roles)
		 *
		 * @param string userEmail the email address of the user to assign the role to
		 * @param Roles role the role as one of the constants from the {@see Role} class
		 * @throws InvalidEmailException if no user with the specified email address has been found
		 *
		 * @see Role
		 */
		public void addRoleForUserByEmail(string userEmail, Roles role) {
			userEmail = validateEmailAddress(userEmail);

			var userFound = this.addRoleForUserByColumnValue(
				"email",
				userEmail,
				role
			);

			if (userFound == false) {
				throw new InvalidEmailException();
			}
		}

		/**
		 * Assigns the specified role to the user with the given username
		 *
		 * A user may have any number of roles (i.e. no role at all, a single role, or any combination of roles)
		 *
		 * @param string username the username of the user to assign the role to
		 * @param Roles role the role as one of the constants from the {@see Role} class
		 * @throws UnknownUsernameException if no user with the specified username has been found
		 * @throws AmbiguousUsernameException if multiple users with the specified username have been found
		 *
		 * @see Role
		 */
		public void addRoleForUserByUsername(string username, Roles role) {
			var userData = this.getUserDataByUsername(
				trim(username),
				new[] { "id" }
			);

			this.addRoleForUserByColumnValue(
				"id",
				userData.id,
				role
			);
		}

		/**
		 * Takes away the specified role from the user with the given ID
		 *
		 * A user may have any number of roles (i.e. no role at all, a single role, or any combination of roles)
		 *
		 * @param int userId the ID of the user to take the role away from
		 * @param Roles role the role as one of the constants from the {@see Role} class
		 * @throws UnknownIdException if no user with the specified ID has been found
		 *
		 * @see Role
		 */
		public void removeRoleForUserById(int userId, Roles role) {
			var userFound = this.removeRoleForUserByColumnValue(
				"id",
				userId,
				role
			);

			if (userFound == false) {
				throw new UnknownIdException();
			}
		}

		/**
		 * Takes away the specified role from the user with the given email address
		 *
		 * A user may have any number of roles (i.e. no role at all, a single role, or any combination of roles)
		 *
		 * @param string userEmail the email address of the user to take the role away from
		 * @param Roles role the role as one of the constants from the {@see Role} class
		 * @throws InvalidEmailException if no user with the specified email address has been found
		 *
		 * @see Role
		 */
		public void removeRoleForUserByEmail(string userEmail, Roles role) {
			userEmail = validateEmailAddress(userEmail);

			var userFound = this.removeRoleForUserByColumnValue(
				"email",
				userEmail,
				role
			);

			if (userFound == false) {
				throw new InvalidEmailException();
			}
		}

		/**
		 * Takes away the specified role from the user with the given username
		 *
		 * A user may have any number of roles (i.e. no role at all, a single role, or any combination of roles)
		 *
		 * @param string username the username of the user to take the role away from
		 * @param Roles role the role as one of the constants from the {@see Role} class
		 * @throws UnknownUsernameException if no user with the specified username has been found
		 * @throws AmbiguousUsernameException if multiple users with the specified username have been found
		 *
		 * @see Role
		 */
		public void removeRoleForUserByUsername(string username, Roles role) {
			var userData = this.getUserDataByUsername(
				trim(username),
				new[] { "id" }
			);

			this.removeRoleForUserByColumnValue(
				"id",
				userData.id,
				role
			);
		}

		/**
		 * Returns whether the user with the given ID has the specified role
		 *
		 * @param int userId the ID of the user to check the roles for
		 * @param Roles role the role as one of the constants from the {@see Role} class
		 * @return bool
		 * @throws UnknownIdException if no user with the specified ID has been found
		 *
		 * @see Role
		 */
		public bool doesUserHaveRole(int userId, Roles role) {

			var rolesBitmask = (Roles) this.db.selectValue(
				"SELECT roles_mask FROM " + this.makeTableName("users") + " WHERE id = @id",
				new BindValues { { "@id", userId } }
			);

			//if (rolesBitmask == null) {
			//	throw new UnknownIdException();
			//}

			return (rolesBitmask & role) == role;
		}

		/**
		 * Returns the roles of the user with the given ID, mapping the numerical values to their descriptive names
		 *
		 * @param int userId the ID of the user to return the roles for
		 * @return array
		 * @throws UnknownIdException if no user with the specified ID has been found
		 *
		 * @see Role
		 */
		public List<Roles> getRolesForUserById(int userId) {

			var rolesBitmask = (Roles?) this.db.selectValue(
				"SELECT roles_mask FROM " + this.makeTableName("users") + " WHERE id = @id",
				new BindValues { { "@id", userId } }
			);

			if (rolesBitmask == null) {
				throw new UnknownIdException();
			}

			return array_filter(
				Role.getMap(),
				(each) => ((rolesBitmask & each) == each),
				ARRAY_FILTER_USE_KEY.x
				)
				.Values.Select(M => (Roles)int.Parse(M)).ToList();
		}

		/**
		 * Signs in as the user with the specified ID
		 *
		 * @param int id the ID of the user to sign in as
		 * @throws UnknownIdException if no user with the specified ID has been found
		 * @throws EmailNotVerifiedException if the user has not verified their email address via a confirmation method yet
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 */
		public void logInAsUserById(int id) {
			var numberOfMatchedUsers = this.logInAsUserByColumnValue("id", id);

			if (numberOfMatchedUsers == 0) {
				throw new UnknownIdException();
			}
		}

		/**
		 * Signs in as the user with the specified email address
		 *
		 * @param string email the email address of the user to sign in as
		 * @throws InvalidEmailException if no user with the specified email address has been found
		 * @throws EmailNotVerifiedException if the user has not verified their email address via a confirmation method yet
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 */
		public void logInAsUserByEmail(string email) {
			email = validateEmailAddress(email);

			var numberOfMatchedUsers = this.logInAsUserByColumnValue("email", email);

			if (numberOfMatchedUsers == 0) {
				throw new InvalidEmailException();
			}
		}

		/**
		 * Signs in as the user with the specified display name
		 *
		 * @param string username the display name of the user to sign in as
		 * @throws UnknownUsernameException if no user with the specified username has been found
		 * @throws AmbiguousUsernameException if multiple users with the specified username have been found
		 * @throws EmailNotVerifiedException if the user has not verified their email address via a confirmation method yet
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 */
		public void logInAsUserByUsername(string username) {
			var numberOfMatchedUsers = this.logInAsUserByColumnValue("username", trim(username));

			if (numberOfMatchedUsers == 0) {
				throw new UnknownUsernameException();
			}
			else if (numberOfMatchedUsers > 1) {
				throw new AmbiguousUsernameException();
			}
		}

		/**
		 * Changes the password for the user with the given ID
		 *
		 * @param int userId the ID of the user whose password to change
		 * @param string newPassword the new password to set
		 * @throws UnknownIdException if no user with the specified ID has been found
		 * @throws InvalidPasswordException if the desired new password has been invalid
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 */
		public void changePasswordForUserById(int userId, string newPassword) {
			newPassword = validatePassword(newPassword);

			this.updatePasswordInternal(
				userId,
				newPassword
			);

			this.forceLogoutForUserById(userId);
		}

		/**
		 * Changes the password for the user with the given username
		 *
		 * @param string username the username of the user whose password to change
		 * @param string newPassword the new password to set
		 * @throws UnknownUsernameException if no user with the specified username has been found
		 * @throws AmbiguousUsernameException if multiple users with the specified username have been found
		 * @throws InvalidPasswordException if the desired new password has been invalid
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 */
		public void changePasswordForUserByUsername(string username, string newPassword) {
			var userData = this.getUserDataByUsername(
				trim(username),
				new[] { "id" }
			);

			this.changePasswordForUserById(
				userData.id,
				newPassword
			);
		}

		/**
		 * Deletes all existing users where the column with the specified name has the given value
		 *
		 * You must never pass untrusted input to the parameter that takes the column name
		 *
		 * @param string columnName the name of the column to filter by
		 * @param mixed columnValue the value to look for in the selected column
		 * @return int the number of deleted users
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 */
		private int deleteUsersByColumnValue(string columnName, object columnValue) {
			try {
				return this.db.delete(
					this.makeTableNameComponents_("users"),
					new Dictionary<string, object>
					{
						{  columnName , columnValue }
					}
				);
			}
			catch (Exception e) {
				throw new DatabaseError(e.Message);
			}
		}

		/**
		 * Modifies the roles for the user where the column with the specified name has the given value
		 *
		 * You must never pass untrusted input to the parameter that takes the column name
		 *
		 * @param string columnName the name of the column to filter by
		 * @param mixed columnValue the value to look for in the selected column
		 * @param callable modification the modification to apply to the existing bitmask of roles
		 * @return bool whether any user with the given column constraints has been found
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 *
		 * @see Role
		 */
		private bool modifyRolesForUserByColumnValue(string columnName, object columnValue, Func<Roles,Roles> modification) {
			DatabaseResultRow userData1;
			try {
				userData1 = this.db.selectRow(
					"SELECT id, roles_mask FROM " + this.makeTableName("users") + " WHERE " + columnName + " = @columnname",
					new BindValues { { "@columnname", columnValue } }
				);
			}
			catch (Exception e) {
				throw new DatabaseError(e.Message);
			}

			if (userData1 == null) {
				return false;
			}

			var userData = new UserDataRow(userData1);

			var newRolesBitmask = modification(userData.roles_mask);

			try {
				this.db.exec( Database.ExecType.NonQuery,
					"UPDATE " + this.makeTableName("users") + " SET roles_mask = @rolesmask WHERE id = @id",
					new BindValues
					{
						{ "@rolesmask", newRolesBitmask },
						{"@id", userData.id }
					}
				);

				return true;
			}
			catch (Exception e) {
				throw new DatabaseError(e.Message);
			}
		}

		/**
		 * Assigns the specified role to the user where the column with the specified name has the given value
		 *
		 * You must never pass untrusted input to the parameter that takes the column name
		 *
		 * @param string columnName the name of the column to filter by
		 * @param mixed columnValue the value to look for in the selected column
		 * @param Roles role the role as one of the constants from the {@see Role} class
		 * @return bool whether any user with the given column constraints has been found
		 *
		 * @see Role
		 */
		private bool addRoleForUserByColumnValue(string columnName, object columnValue, Roles role) {

			return this.modifyRolesForUserByColumnValue(
				columnName,
				columnValue,
				(oldRolesBitmask) => (oldRolesBitmask | role)
			);
		}

		/**
		 * Takes away the specified role from the user where the column with the specified name has the given value
		 *
		 * You must never pass untrusted input to the parameter that takes the column name
		 *
		 * @param string columnName the name of the column to filter by
		 * @param mixed columnValue the value to look for in the selected column
		 * @param Roles role the role as one of the constants from the {@see Role} class
		 * @return bool whether any user with the given column constraints has been found
		 *
		 * @see Role
		 */
		private bool removeRoleForUserByColumnValue(string columnName, object columnValue, Roles role) {

			return this.modifyRolesForUserByColumnValue(
				columnName,
				columnValue,
				(oldRolesBitmask) => (oldRolesBitmask & ~role)
			);
		}

		/**
		 * Signs in as the user for which the column with the specified name has the given value
		 *
		 * You must never pass untrusted input to the parameter that takes the column name
		 *
		 * @param string columnName the name of the column to filter by
		 * @param mixed columnValue the value to look for in the selected column
		 * @return int the number of matched users (where only a value of one means that the login may have been successful)
		 * @throws EmailNotVerifiedException if the user has not verified their email address via a confirmation method yet
		 * @throws AuthError if an internal problem occurred (do *not* catch)
		 */
		private int logInAsUserByColumnValue(string columnName, object columnValue) {

			List<DatabaseResultRow> users;

			try {
				users = this.db.select(
					"SELECT verified, id, email, username, status, roles_mask FROM " + this.makeTableName("users") 
					+ " WHERE " + columnName + " = @columnname LIMIT 2 OFFSET 0",
					new BindValues { { "@columnname", columnValue } }
				);
			}
			catch (Exception e) {
				throw new DatabaseError(e.Message);
			}

			var numberOfMatchingUsers = (users != null) ? count(users) : 0;

			if (numberOfMatchingUsers == 1)
			{
				var user = users[0];

				if ((int) user["verified"] == 1) 
				{
					this.onLoginSuccessful((int)user["id"], (string)user["email"], (string)user["username"],
						(Status)(int)user["status"], (Roles)(int)user["roles_mask"], 
						PHP_INT_MAX,
						false);
				}
				else 
				{
					throw new EmailNotVerifiedException();
				}
			}

			return numberOfMatchingUsers;
		}

	}
}
