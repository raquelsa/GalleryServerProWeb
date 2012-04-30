using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// A collection of <see cref="IUserAccount" /> objects.
	/// </summary>
	public class UserAccountCollection : Collection<IUserAccount>, IUserAccountCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAccountCollection"/> class.
		/// </summary>
		public UserAccountCollection()
			: base(new List<IUserAccount>())
		{
		}

		/// <summary>
		/// Gets a list of user names for accounts in the collection. This is equivalent to iterating through each <see cref="IUserAccount" />
		/// and compiling a string array of the <see cref="IUserAccount.UserName" /> properties.
		/// </summary>
		/// <returns>Returns a string array of user names of accounts in the collection.</returns>
		public string[] GetUserNames()
		{
			List<String> users = new List<string>(Items.Count);

			foreach (IUserAccount user in (List<IUserAccount>)Items)
			{
				users.Add(user.UserName);
			}

			return users.ToArray();
		}

		/// <summary>
		/// Sort the objects in this collection based on the <see cref="IUserAccount.UserName" /> property.
		/// </summary>
		public void Sort()
		{
			// We know userAccounts is actually a List<IUserAccount> because we passed it to the constructor.
			List<IUserAccount> userAccounts = (List<IUserAccount>)Items;

			userAccounts.Sort();
		}

		/// <summary>
		/// Adds the user accounts to the current collection.
		/// </summary>
		/// <param name="userAccounts">The user accounts to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="userAccounts" /> is null.</exception>
		public void AddRange(IEnumerable<IUserAccount> userAccounts)
		{
			if (userAccounts == null)
				throw new ArgumentNullException("userAccounts");

			foreach (IUserAccount userAccount in userAccounts)
			{
				this.Add(userAccount);
			}
		}

		/// <overloads>
		/// Determines whether a user is a member of the collection.
		/// </overloads>
		/// <summary>
		/// Determines whether the <paramref name="item"/> is a member of the collection. An object is considered a member
		/// of the collection if they both have the same <see cref="IUserAccount.UserName"/>.
		/// </summary>
		/// <param name="item">An <see cref="IUserAccount"/> to determine whether it is a member of the current collection.</param>
		/// <returns>
		/// Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		/// otherwise returns <c>false</c>.
		/// </returns>
		public new bool Contains(IUserAccount item)
		{
			if (item == null)
				return false;

			foreach (IUserAccount userAccountInCollection in (List<IUserAccount>)Items)
			{
				if (userAccountInCollection.UserName.Equals(item.UserName, StringComparison.Ordinal))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether a user account with the specified <paramref name="userName"/> is a member of the collection.
		/// </summary>
		/// <param name="userName">The user name that uniquely identifies the user.</param>
		/// <returns>Returns <c>true</c> if <paramref name="userName"/> is a member of the current collection;
		/// otherwise returns <c>false</c>.</returns>
		public bool Contains(string userName)
		{
			return Contains(new UserAccount(userName));
		}

		/// <summary>
		/// Adds the specified user account.
		/// </summary>
		/// <param name="item">The user account to add.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		public new void Add(IUserAccount item)
		{
			if (item == null)
				throw new ArgumentNullException("item", "Cannot add null to an existing UserAccountCollection. Items.Count = " + Items.Count);

			base.Add(item);
		}

		/// <summary>
		/// Find the user account in the collection that matches the specified <paramref name="userName" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="userName">The user name that uniquely identifies the user.</param>
		/// <returns>Returns an <see cref="IUserAccount" />object from the collection that matches the specified <paramref name="userName" />,
		/// or null if no matching object is found.</returns>
		public IUserAccount FindByUserName(string userName)
		{
			List<IUserAccount> userAccounts = (List<IUserAccount>)Items;

			return userAccounts.Find(delegate(IUserAccount gallery)
			{
				return (gallery.UserName == userName);
			});
		}

		/// <summary>
		/// Finds the users whose <see cref="IUserAccount.UserName" /> begins with the specified <paramref name="userNameSearchString" />. 
		/// This method can be used to find a set of users that match the first few characters of a string. Returns an empty collection if 
		/// no matches are found. The match is case-insensitive. Example: If <paramref name="userNameSearchString" />="Rob", this method 
		/// returns users with names like "Rob", "Robert", and "robert" but not names such as "Boston Rob".
		/// </summary>
		/// <param name="userNameSearchString">A string to match against the beginning of a <see cref="IUserAccount.UserName" />. Do not
		/// specify a wildcard character. If value is null or an empty string, all users are returned.</param>
		/// <returns>Returns an <see cref="IUserAccountCollection" />object from the collection where the <see cref="IUserAccount.UserName" /> 
		/// begins with the specified <paramref name="userNameSearchString" />, or an empty collection if no matching object is found.</returns>
		public IUserAccountCollection FindAllByUserName(string userNameSearchString)
		{
			IUserAccountCollection matchingUsers = new UserAccountCollection();

			if (String.IsNullOrEmpty(userNameSearchString))
			{
				matchingUsers.AddRange(Items);
				return matchingUsers;
			}

			foreach (IUserAccount user in Items)
			{
				if (user.UserName.StartsWith(userNameSearchString, StringComparison.OrdinalIgnoreCase))
				{
					matchingUsers.Add(user);
				}
			}

			return matchingUsers;
		}
	}
}
