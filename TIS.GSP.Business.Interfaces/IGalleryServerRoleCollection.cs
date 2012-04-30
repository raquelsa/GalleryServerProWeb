using System;
using System.Collections.Generic;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IGalleryServerRole" /> objects.
	/// </summary>
	public interface IGalleryServerRoleCollection : System.Collections.Generic.ICollection<IGalleryServerRole>
	{
		/// <summary>
		/// Adds the roles to the current collection.
		/// </summary>
		/// <param name="roles">The roles to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="roles" /> is null.</exception>
		void AddRange(System.Collections.Generic.IEnumerable<IGalleryServerRole> roles);
		
		/// <summary>
		/// Sort the objects in this collection by the <see cref="IGalleryServerRole.RoleName" /> property.
		/// </summary>
		void Sort();

		/// <summary>
		/// Creates a new collection containing deep copies of the items it contains.
		/// </summary>
		/// <returns>Returns a new collection containing deep copies of the items it contains.</returns>
		IGalleryServerRoleCollection Copy();

		/// <summary>
		/// Verify the roles in the collection conform to business rules. Specificially, if any of the roles have administrative permissions
		/// (AllowAdministerSite = true or AllowAdministerGallery = true):
		/// 1. Make sure the role permissions - except HideWatermark - are set to true.
		/// 2. Make sure the root album IDs are a list containing the root album ID for each affected gallery.
		/// If anything needs updating, update the object and persist the changes to the data store. This helps keep the data store 
		/// valid in cases where the user is directly editing the tables (for example, adding/deleting records from the gs_Role_Album table).
		/// </summary>
		void ValidateIntegrity();

		/// <summary>
		/// Return the role that matches the specified <paramref name="roleName"/>. It is not case sensitive, so
		///  that "ReadAll" matches "readall". Returns null if no match is found.
		/// </summary>
		/// <param name="roleName">The name of the role to return.</param>
		/// <returns>
		/// Returns the role that matches the specified role name. Returns null if no match is found.
		/// </returns>
		IGalleryServerRole GetRole(string roleName);

		/// <summary>
		/// Gets the Gallery Server roles that match the specified <paramref name="roleNames" />. It is not case sensitive, 
		/// so that "ReadAll" matches "readall".
		/// </summary>
		/// <param name="roleNames">The name of the roles to return.</param>
		/// <returns>
		/// Returns the Gallery Server roles that match the specified <paramref name="roleNames" />.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="roleNames" /> is null.</exception>
		IGalleryServerRoleCollection GetRoles(IEnumerable<string> roleNames);

		/// <summary>
		/// Gets the Gallery Server roles with AllowAdministerGallery permission, including roles with AllowAdministerSite permission.
		/// </summary>
		/// <returns>Returns the Gallery Server roles with AllowAdministerGallery permission.</returns>
		IGalleryServerRoleCollection GetRolesWithGalleryAdminPermission();

		/// <summary>
		/// Gets a reference to the IGalleryServerRole object at the specified index position.
		/// </summary>
		/// <param name="indexPosition">An integer specifying the position of the object within this collection to
		/// return. Zero returns the first item.</param>
		/// <returns>Returns a reference to the IGalleryServerRole object at the specified index position.</returns>
		IGalleryServerRole this[Int32 indexPosition]
		{
			get;
			set;
		}

		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence within the collection.  
		/// </summary>
		/// <param name="galleryServerRole">The gallery server role to locate in the collection. The value can be a null 
		/// reference (Nothing in Visual Basic).</param>
		/// <returns>The zero-based index of the first occurrence of galleryServerRole within the collection, if found; 
		/// otherwise, –1. </returns>
		Int32 IndexOf(IGalleryServerRole galleryServerRole);

	}
}
