/*
 **************************************************************
 * PreservePropertyControl Class
 **************************************************************
 *  Author: Rick Strahl 
 *          (c) West Wind Technologies
 *          http://www.west-wind.com/
 * Created: 12/23/2005
 * License: Free - provided as is, no warranties
 * 
 * More info on this class:
 * http://west-wind.com/weblog/posts/3988.aspx
 * 
 * Special thanks to:
 * ------------------
 * Peter Bromberg (http://petesbloggerama.blogspot.com/)
 * for creating a 1.1 version and providing a bunch of input
 * on the various persistence modes.
 * 
 * Bertrand LeRoy (http://weblogs.asp.net/bleroy)
 * for the original inspiration to write the control and some
 * suggestions for improvement.
 **************************************************************  
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Design;

using System.Reflection;
using System.Web;
using System.IO;
using System.Web.UI.Design;

namespace GalleryServerPro.WebControls
{
	/// <summary>
	/// Control used to automatically preserve properties on a page.
	/// By calling PreserveProperty you're instructing this control
	/// to persist and then restore the value of the specified property
	/// or field of a control and have it automatically restored on the
	/// next page access.
	/// 
	/// The difference of this mechanism is that it works with ViewState
	/// off, and automatically reassigns persisted values back to the 
	/// controls they belong to without any extra code. It greatly 
	/// simplifies persisting values like IDs or some ViewState based
	/// values without having to use Viewstate on a page.
	/// </summary>
	[NonVisualControl, Designer(typeof(PreservePropertyControlDesigner))]
	[ParseChildren(true)]
	[PersistChildren(false)]
	[DefaultProperty("PreservedProperties")]
	public class PreservePropertyControl : Control
	{
		private const char CTLID_PROPERTY_SEPERATOR = '|';

		///// <summary>
		///// Collection of all the preserved properties that are to
		///// be preserved/restored. Collection hold, ControlId, Property
		///// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public List<PreservedProperty> PreservedProperties
		{
			get
			{
				return _PreservedProperties;
			}
		}
		List<PreservedProperty> _PreservedProperties = new List<PreservedProperty>();

		/// <summary>
		/// Required to be able to properly PreservedProperty Collection 
		/// </summary>
		/// <param name="obj"></param>
		protected override void AddParsedSubObject(object obj)
		{
			if (obj is PreservedProperty)
				this.PreservedProperties.Add(obj as PreservedProperty);
		}

		/// <summary>
		/// Internal persistance object used to serialize
		/// into the state store. Hashtable is Serializable
		/// and can be serialized by the LosFormatter
		/// </summary>
		protected Hashtable SerialzedProperties = new Hashtable();

		/// <summary>
		/// Determines the storage mode for the control.
		/// </summary>
		[Description("Determines how the Property data is persisted.")]
		public PropertyStorageModes StorageMode
		{
			get
			{
				return _StorageMode;
			}
			set
			{
				_StorageMode = value;
			}
		}
		private PropertyStorageModes _StorageMode = PropertyStorageModes.ControlState;


		/// <summary>
		/// Determines whether the control preserves and restores values
		/// </summary>
		[Description("Determines whether the control preserves and restores values"), DefaultValue(true)]
		public bool Enabled
		{
			get
			{
				return _Enabled;
			}
			set
			{
				_Enabled = value;
			}
		}
		private bool _Enabled = true;

		/// <summary>
		/// Cache key value used
		/// </summary>
		protected string PreservePropertyKey = null;

		/// <summary>
		/// Adds a control to the collection. At this point only the
		/// control and property are stored.
		/// </summary>
		/// <param name="WebControl"></param>
		/// <param name="Property"></param>
		/// <returns></returns>
		public bool PreserveProperty(Control WebControl, string Property)
		{
			PreservedProperty pp = new PreservedProperty();
			pp.ControlId = WebControl.UniqueID;
			pp.ControlInstance = WebControl;
			pp.Property = Property;

			this.PreservedProperties.Add(pp);

			return true;
		}

		/// <summary>
		/// Adds a control to the collection. At this point only the
		/// control and property are stored.
		/// </summary>
		/// <param name="ControlId"></param>
		/// <param name="Property"></param>
		/// <returns></returns>
		public bool PreserveProperty(string ControlId, string Property)
		{
			Control ctl = this.Page.FindControl(ControlId);
			if (ctl == null)
				throw new ApplicationException("Can't persist control: " + ControlId + "." + Property);

			return this.PreserveProperty(ctl, Property);
		}

		/// <summary>
		/// Read in data of preserved properties in OnInit
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			if (this.Enabled)
			{
				if (this.StorageMode == PropertyStorageModes.ControlState)
					this.Page.RegisterRequiresControlState(this);
				else if (this.Page.IsPostBack)
					this.LoadStateFromLosStorage();
			}
		}

		/// <summary>
		/// Write out data for preserved properties in OnPreRender
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			if (this.Enabled && StorageMode != PropertyStorageModes.ControlState)
				this.SaveStateToLosStorage();

			base.OnPreRender(e);
		}

		/// <summary>
		/// Saves the preserved Properties into a Hashtabe where the key is
		/// a string containing the ControlID and Property name
		/// </summary>
		/// <returns></returns>
		protected override object SaveControlState()
		{
			foreach (PreservedProperty Property in this.PreservedProperties)
			{

				// *** Try to get a control instance
				Control Ctl = Property.ControlInstance;
				if (Ctl == null)
				{
					// *** Nope - user stored a string or declarative
					Ctl = this.Page.FindControl(Property.ControlId);
					if (Ctl == null)
						continue;
				}

				string Key = Ctl.UniqueID + CTLID_PROPERTY_SEPERATOR + Property.Property;

				// *** If the property was already added skip over it
				// *** Values are read now so duplicates are always the same
				if (this.SerialzedProperties.Contains(Key))
					continue;

				// *** Try to retrieve the property
				object Value = null;
				try
				{
					// *** Use Reflection to get the value out
					// *** Note: InvokeMember is easier here since
					//           we support both fields and properties
					Value = Ctl.GetType().InvokeMember(Property.Property,
																											 BindingFlags.GetField | BindingFlags.GetProperty |
																											 BindingFlags.Instance |
																											 BindingFlags.Public | BindingFlags.NonPublic |
																											 BindingFlags.IgnoreCase, null, Ctl, null);
				}
				catch
				{
					throw new ApplicationException("PreserveProperty() couldn't read property " + Property.ControlId + " " + Property.Property);
				}

				// *** Store into our hashtable to persist later
				this.SerialzedProperties.Add(Key, Value);
			}

			// *** store the hashtable in control state (or return it
			return this.SerialzedProperties;
		}


		/// <summary>
		/// Overridden to store a HashTable of preserved properties.
		/// Key: CtlID + "|" + Property
		/// Value: Value of the control
		/// </summary>
		/// <param name="savedState"></param>
		protected override void LoadControlState(object savedState)
		{
			Hashtable Properties = (Hashtable)savedState;

			IDictionaryEnumerator Enum = Properties.GetEnumerator();
			while (Enum.MoveNext())
			{
				string Key = (string)Enum.Key;
				string[] Tokens = Key.Split(CTLID_PROPERTY_SEPERATOR);

				string ControlId = Tokens[0];
				string Property = Tokens[1];

				Control Ctl = this.Page.FindControl(ControlId);
				if (Ctl == null)
					continue;

				Ctl.GetType().InvokeMember(Property,
														BindingFlags.SetField | BindingFlags.SetProperty |
														BindingFlags.Instance |
														BindingFlags.Public | BindingFlags.NonPublic |
														BindingFlags.IgnoreCase, null, Ctl, new object[1] { Enum.Value });
			}
		}

		/// <summary>
		/// Saves state the specified storage mechanism by
		/// first serializing to a string with the LosFormatter
		/// </summary>
		private void SaveStateToLosStorage()
		{
			string Serialized = LosSerializeObject(this.SaveControlState());

			if (this.StorageMode == PropertyStorageModes.HiddenVariable)
				this.Page.ClientScript.RegisterHiddenField("__" + this.UniqueID, Serialized);
			else if (this.StorageMode == PropertyStorageModes.SessionVariable)
				HttpContext.Current.Session["__" + this.UniqueID] = Serialized;
			//else if (this.StorageMode == PropertyStorageModes.CachePerUser)
			//{
			//    if (this.PreservePropertyKey == null)
			//        this.PreservePropertyKey = HttpContext.Current.Request.UserHostAddress;
			//    HttpContext.Current.Cache[this.PreservePropertyKey] = Serialized;
			//    this.Page.ClientScript.RegisterHiddenField("__PreservePropertyKey", this.PreservePropertyKey);
			//}
			else if (this.StorageMode == PropertyStorageModes.CachePerPage)
			{
				if (this.PreservePropertyKey == null)
					this.PreservePropertyKey = Guid.NewGuid().ToString().GetHashCode().ToString("x");

				HttpContext.Current.Cache[this.PreservePropertyKey] = Serialized;
				this.Page.ClientScript.RegisterHiddenField("__PreservePropertyKey", this.PreservePropertyKey);
			}
		}

		/// <summary>
		/// Retrieves the serialized data from the Storage medium
		/// as string using LosFormatter formatting.
		/// </summary>
		private void LoadStateFromLosStorage()
		{
			string RawBuffer = null;
			if (this.StorageMode == PropertyStorageModes.HiddenVariable)
			{
				RawBuffer = HttpContext.Current.Request.Form["__" + this.UniqueID];
				if (RawBuffer == null)
					return;
			}
			else if (this.StorageMode == PropertyStorageModes.SessionVariable)
			{
				RawBuffer = HttpContext.Current.Session["__" + this.UniqueID] as string;
				if (RawBuffer == null)
					return;
			}
			else if (this.StorageMode == PropertyStorageModes.CachePerPage)
			{
				this.PreservePropertyKey = HttpContext.Current.Request.Form["__PreservePropertyKey"];
				if (this.PreservePropertyKey == null)
					return;

				RawBuffer = HttpContext.Current.Cache[this.PreservePropertyKey] as string;
			}

			if (RawBuffer == null)
				return;

			// *** Retrieve the persisted HashTable and pass to LoadControlState
			// *** to handle the assignment of property values
			this.LoadControlState(LosDeserializeObject(RawBuffer));
		}


		private string LosSerializeObject(object obj)
		{
			LosFormatter output = new LosFormatter();
			StringWriter writer = new StringWriter();
			output.Serialize(writer, obj);
			return writer.ToString();
		}


		private object LosDeserializeObject(string inputString)
		{
			LosFormatter input = new LosFormatter();
			return input.Deserialize(inputString);
		}


		protected override void Render(HtmlTextWriter writer)
		{
			if (this.DesignMode)
				writer.Write("[ *** wwPersister: " + this.ID + " *** ]");

			base.Render(writer);
		}

	}

	/// <summary>
	/// An individual Preserved Property. Contains
	/// a ControlId and Property name and optional (on preserve)
	/// an instance of a control.
	/// </summary>
	[ToolboxData("<{0}:PreservedProperty runat=server></{0}:PreservedProperty")]
	[Browsable(false)]
	public class PreservedProperty : Control
	{
		public PreservedProperty()
		{
		}

		[NotifyParentProperty(true)]
		[Browsable(true), Description("The ID of the control that is to be preserved.")]
		public string ControlId
		{
			get
			{
				return _ControlId;
			}
			set
			{
				_ControlId = value;
			}
		}
		private string _ControlId = "";

		[NotifyParentProperty(true)]
		[Browsable(true), Description("The property on the control to preserve")]
		public string Property
		{
			get
			{
				return _Property;
			}
			set
			{
				_Property = value;
			}
		}
		private string _Property = "";

		/// <summary>
		/// An optional instance of the control that can be assigned
		/// </summary>
		[NotifyParentProperty(true)]
		[Browsable(false)]
		public Control ControlInstance
		{
			get
			{
				return _ControlInstance;
			}
			set
			{
				_ControlInstance = value;
			}
		}
		private Control _ControlInstance = null;
	}

	/// <summary>
	/// Determines how preserved properties are stored on the page
	/// </summary>
	public enum PropertyStorageModes
	{
		ControlState,
		HiddenVariable,
		SessionVariable,
		CachePerPage
		//        CachePerUser
	}

	internal class PreservePropertyControlDesigner : ControlDesigner
	{
		public override string GetDesignTimeHtml()
		{
			return base.CreatePlaceHolderDesignTimeHtml("");
		}
	}
}
