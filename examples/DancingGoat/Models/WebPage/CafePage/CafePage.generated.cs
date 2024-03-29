//--------------------------------------------------------------------------------------------------
// <auto-generated>
//
//     This code was generated by code generator tool.
//
//     To customize the code use your own partial class. For more info about how to use and customize
//     the generated code see the documentation at https://docs.xperience.io/.
//
// </auto-generated>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using CMS.Websites;

namespace DancingGoat.Models
{
	/// <summary>
	/// Represents a page of type <see cref="CafePage"/>.
	/// </summary>
	public partial class CafePage : IWebPageFieldsSource
	{
		/// <summary>
		/// Code name of the content type.
		/// </summary>
		public const string CONTENT_TYPE_NAME = "DancingGoat.CafePage";


		/// <summary>
		/// Represents system properties for a web page item.
		/// </summary>
		public WebPageFields SystemFields { get; set; }


		/// <summary>
		/// CafeTitle.
		/// </summary>
		public string CafeTitle { get; set; }


		/// <summary>
		/// CafePagePublishDate.
		/// </summary>
		public DateTime CafePagePublishDate { get; set; }


		/// <summary>
		/// CafePageTeaser.
		/// </summary>
		public IEnumerable<Image> CafePageTeaser { get; set; }


		/// <summary>
		/// CafePageText.
		/// </summary>
		public string CafePageText { get; set; }


		/// <summary>
		/// CafeLocation.
		/// </summary>
		public string CafeLocation { get; set; }


		/// <summary>
		/// CafeLocationLongitude.
		/// </summary>
		public decimal CafeLocationLongitude { get; set; }


		/// <summary>
		/// CafeLocationLatitude.
		/// </summary>
		public decimal CafeLocationLatitude { get; set; }
	}
}
