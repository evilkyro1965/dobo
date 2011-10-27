using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Globalization;
using Kooboo.Globalization;
using Kooboo.Extensions;

namespace Kooboo.CMS.Sites.Services
{
	public class LabelManager
	{
		public IEnumerable<ElementCategory> GetCategories(Site site)
		{
			return SiteLabel.GetElementRepository(site).Categories();
		}
		public IQueryable<Element> GetLabels(Site site, string category)
		{
			return SiteLabel.GetElementRepository(site).Elements()
				.Where(it => it.Category.EqualsOrNullEmpty(category, StringComparison.CurrentCultureIgnoreCase));
		}

		public void Add(Site site, Element element)
		{
			var oldElement = Get(site, element.Category, element.Name, element.Culture);
			if (oldElement != null)
			{
				throw new ItemAlreadyExistsException();
			}
			SiteLabel.GetElementRepository(site).Add(element);
		}
		public Element Get(Site site, string category, string name, string culture)
		{
			return SiteLabel.GetElementRepository(site).Get(name, category, culture);
		}
		public void Update(Site site, Element element)
		{
			SiteLabel.GetElementRepository(site).Update(element);
		}
		public void Remove(Site site, Element element)
		{
			element.Culture = site.AsActual().Culture;
			SiteLabel.GetElementRepository(site).Remove(element);
		}
		public void RemoveCategory(Site site, string category)
		{
			SiteLabel.GetElementRepository(site).RemoveCategory(category, site.AsActual().Culture);
		}
		public void AddCategory(Site site, string category)
		{
			SiteLabel.GetElementRepository(site).AddCategory(category, site.AsActual().Culture);
		}
	}
}
