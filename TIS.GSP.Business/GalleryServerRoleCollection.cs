using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// A collection of <see cref="IGalleryServerRole" /> objects.
	/// </summary>
	public class GalleryServerRoleCollection : Collection<IGalleryServerRole>, IGalleryServerRoleCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryServerRoleCollection"/> class.
		/// </summary>
		public GalleryServerRoleCollection()
			: base(new System.Collections.Generic.List<IGalleryServerRole>())
		{
		}

		/// <summary>
		/// Adds the roles to the current collection.
		/// </summary>
		/// <param name="roles">The roles to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="roles" /> is null.</exception>
		public void AddRange(IEnumerable<IGalleryServerRole> roles)
		{
			if (roles == null)
				throw new ArgumentNullException("roles");
			
			foreach (IGalleryServerRole role in roles)
			{
				this.Add(role);
			}
		}

		/// <summary>
		/// Sort the objects in this collection by the <see cref="IGalleryServerRole.RoleName" /> property.
		/// </summary>
		public void Sort()
		{
			// We know galleryServerRoles is actually a List<IGalleryServerRole> because we passed it to the constructor.
			System.Collections.Generic.List<IGalleryServerRole> galleryServerRoles = (System.Collections.Generic.List<IGalleryServerRole>)Items;

			galleryServerRoles.Sort();
		}

		/// <summary>
		/// Creates a new collection containing deep copies of the items it contains.
		/// </summary>
		/// <returns>
		/// Returns a new collection containing deep copies of the items it contains.
		/// </returns>
		public IGalleryServerRoleCollection Copy()
		{
			IGalleryServerRoleCollection copy = new GalleryServerRoleCollection();

			foreach (IGalleryServerRole role in Items)
			{
				copy.Add(role.Copy());
			}

			return copy;
		}

		/// <summary>
		/// Verify the roles in the collection conform to business rules. Specificially, if any of the roles have administrative permissions
		/// (AllowAdministerSite = true or AllowAdministerGallery = true):
		/// 1. Make sure the role permissions - except HideWatermark - are set to true.
		/// 2. Make sure the root album IDs are a list containing the root album ID for each affected gallery.
		/// If anything needs updating, update the object and persist the changes to the data store. This helps keep the data store
		/// valid in cases where the user is directly editing the tables (for example, adding/deleting records from the gs_Role_Album table).
		/// </summary>
		public void ValidateIntegrity()
		{
			foreach (IGalleryServerRole role in Items)
			{
				role.ValidateIntegrity();
			}
		}

		/// <summary>
		/// Return the role that matches the specified <paramref name="roleName"/>. It is not case sensitive, so that 
		/// "ReadAll" matches "readall". Returns null if no match is found.
		/// </summary>
		/// <param name="roleName">The name of the role to return.</param>
		/// <returns>
		/// Returns the role that matches the specified role name. Returns null if no match is found.
		/// </returns>
		public IGalleryServerRole GetRole(string roleName)
		{
			// We know galleryServerRoles is actually a List<IGalleryServerRole> because we passed it to the constructor.
			List<IGalleryServerRole> galleryServerRoles = (List<IGalleryServerRole>)Items;

			return galleryServerRoles.Find(delegate(IGalleryServerRole galleryServerRole)
			{
				return (String.Compare(galleryServerRole.RoleName, roleName, StringComparison.OrdinalIgnoreCase) == 0);
			});
		}

		/// <summary>
		/// Gets the Gallery Server roles that match the specified <paramref name="roleName"/>. It is not case sensitive,
		/// so that "ReadAll" matches "readall". Will return multiple roles with the same name when the gallery is assigned
		/// to more than one gallery.
		/// </summary>
		/// <param name="roleName">The name of the role to return.</param>
		/// <returns>
		/// Returns the Gallery Server roles that match the specified <paramref name="roleName"/>.
		/// </returns>
		/// <overloads>
		/// Gets the Gallery Server roles that match the specified parameters.
		/// </overloads>
		public IGalleryServerRoleCollection GetRoles(string roleName)
		{
			return GetRoles(new string[] { roleName });
		}

		/// <overloads>
		/// Gets the Gallery Server roles that match the specified parameters.
		/// </overloads>
		/// <summary>
		/// Gets the Gallery Server roles that match the specified <paramref name="roleNames"/>. It is not case sensitive,
		/// so that "ReadAll" matches "readall".
		/// </summary>
		/// <param name="roleNames">The name of the roles to return.</param>
		/// <returns>
		/// Returns the Gallery Server roles that match the specified <paramref name="roleNames"/>.
		/// </returns>
		/// <exception cref="InvalidGalleryServerRoleException">Thrown when one or more of the requested role names could not be found
		/// in the current collection.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="roleNames" /> is null.</exception>
		public IGalleryServerRoleCollection GetRoles(IEnumerable<string> roleNames)
		{
			if (roleNames == null)
				throw new ArgumentNullException("roleNames");

			// We know galleryServerRoles is actually a List<IGalleryServerRole> because we passed it to the constructor.
			List<IGalleryServerRole> galleryServerRoles = (List<IGalleryServerRole>)Items;

			IGalleryServerRoleCollection roles = new GalleryServerRoleCollection();
			foreach (string roleName in roleNames)
			{
				IGalleryServerRole role = galleryServerRoles.Find(delegate(IGalleryServerRole galleryServerRole)
				{
					return (String.Compare(galleryServerRole.RoleName, roleName, StringComparison.OrdinalIgnoreCase) == 0);
				});

				if (role == null)
				{
					throw new InvalidGalleryServerRoleException(String.Format(CultureInfo.CurrentCulture, "Could not find a Gallery Server role named '{0}'. Verify the data table contains a record for this role, and that the cache is being properly managed.", roleName));
				}
				else
				{
					roles.Add(role);
				}
			}

			return roles;
		}

		/// <summary>
		/// Gets the Gallery Server roles with AllowAdministerGallery permission, including roles with AllowAdministerSite permission.
		/// </summary>
		/// <returns>Returns the Gallery Server roles with AllowAdministerGallery permission.</returns>
		public IGalleryServerRoleCollection GetRolesWithGalleryAdminPermission()
		{
			List<IGalleryServerRole> galleryServerRoles = (List<IGalleryServerRole>)Items;

			IGalleryServerRoleCollection roles = new GalleryServerRoleCollection();

			roles.AddRange(galleryServerRoles.FindAll(delegate(IGalleryServerRole galleryServerRole)
																											{
																												return (galleryServerRole.AllowAdministerGallery == true);
																											}));

			return roles;
		}
	}
}
