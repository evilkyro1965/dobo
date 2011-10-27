using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Kooboo.Collections;
using System.Web.Routing;


namespace Kooboo.Web.Mvc.Menu
{
    public class MenuFactory
    {
        private class MenuTemplate
        {
            public IEnumerable<IMenuItemContainer> ItemContainers { get; set; }
        }

        private class MenuItemTemplate : MenuItem, IMenuItemContainer
        {
            internal IEnumerable<IMenuItemContainer> ItemContainers { get; set; }

            public void Initialize(MenuItem menuItem, ControllerContext controllerContext)
            {
                Initializer.Initialize(menuItem, controllerContext);
            }

            public IEnumerable<MenuItem> GetItems(ControllerContext controllerContext)
            {
                List<MenuItem> items = new List<MenuItem>();
                items.Add(CreateMenuItemByTemplate(this, controllerContext));
                return items;
            }

            private MenuItem CreateMenuItemByTemplate(MenuItemTemplate template, ControllerContext controllerContext)
            {
                MenuItem item = new MenuItem()
                {
                    Action = template.Action,
                    Area = template.Area,
                    Controller = template.Controller,
                    HtmlAttributes = new RouteValueDictionary(template.HtmlAttributes),
                    Localizable = template.Localizable,
                    Name = template.Name,
                    ReadOnlyProperties = template.ReadOnlyProperties,
                    RouteValues = new RouteValueDictionary(template.RouteValues),
                    Text = template.Text,
                    Visible = template.Visible,
                    Initializer = template.Initializer
                };

                List<MenuItem> items = new List<MenuItem>();
                if (template.ItemContainers != null)
                {
                    foreach (var itemContainer in template.ItemContainers)
                    {
                        items.AddRange(itemContainer.GetItems(controllerContext));
                    }
                }

                item.Items = items;

                return item;
            }

        }

        static MenuTemplate defaultMenu = null;

        static IDictionary<string, MenuTemplate> areasMenu = new Dictionary<string, MenuTemplate>(StringComparer.OrdinalIgnoreCase);


        #region Menu Template
        static MenuFactory()
        {
            defaultMenu = new MenuTemplate();
            Configuration.MenuSection menuSection = Configuration.MenuSection.GetSection();
            if (menuSection != null)
            {
                defaultMenu.ItemContainers = CreateItems(menuSection.Items, new List<IMenuItemContainer>());
            }
        }
        static IEnumerable<IMenuItemContainer> CreateItems(Configuration.MenuItemElementCollection itemElementCollection, List<IMenuItemContainer> itemContainers)
        {
            if (itemElementCollection != null)
            {
                if (!string.IsNullOrEmpty(itemElementCollection.Type))
                {
                    itemContainers.Add((IMenuItemContainer)Activator.CreateInstance(Type.GetType(itemElementCollection.Type)));
                }
                else
                {
                    foreach (Configuration.MenuItemElement element in itemElementCollection)
                    {
                        MenuItemTemplate item = new MenuItemTemplate()
                        {
                            Name = element.Name,
                            Text = element.Text,
                            Action = element.Action,
                            Controller = element.Controller,
                            Visible = element.Visible,
                            Area = element.Area,
                            RouteValues = new System.Web.Routing.RouteValueDictionary(element.RouteValues.Attributes),
                            HtmlAttributes = new System.Web.Routing.RouteValueDictionary(element.HtmlAttributes.Attributes),
                            ReadOnlyProperties = element.UnrecognizedProperties
                        };


                        itemContainers.Add(item);
                        if (!string.IsNullOrEmpty(element.Initializer))
                        {
                            Type type = Type.GetType(element.Initializer);
                            item.Initializer = (IMenuItemInitializer)Activator.CreateInstance(type);
                        }

                        List<IMenuItemContainer> subItems = new List<IMenuItemContainer>();
                        if (element.Items != null)
                        {
                            item.ItemContainers = CreateItems(element.Items, subItems);
                        }
                    }
                }
            }
            return itemContainers;

        }

        public static void RegisterAreaMenu(string area, string menuFileName)
        {
            lock (areasMenu)
            {
                Configuration.MenuSection menuSection = Configuration.MenuSection.GetSection(menuFileName);
                if (menuSection != null)
                {
                    MenuTemplate areaMenu = new MenuTemplate();
                    areaMenu.ItemContainers = CreateItems(menuSection.Items, new List<IMenuItemContainer>());
                    areasMenu.Add(area, areaMenu);
                }
            }
        }
        public static bool ContainsAreaMenu(string area)
        {
            return areasMenu.ContainsKey(area);
        }
        #endregion

        public static Menu BuildMenu(ControllerContext controllerContext)
        {
            string areaName = AreaHelpers.GetAreaName(controllerContext.RouteData);
            return BuildMenu(controllerContext, areaName);
        }
        public static Menu BuildMenu(ControllerContext controllerContext, string areaName)
        {
            return BuildMenu(controllerContext, areaName, true);
        }
        public static Menu BuildMenu(ControllerContext controllerContext, string areaName, bool initialize)
        {
            Menu menu = new Menu();

            MenuTemplate menuTemplate = new MenuTemplate();
            if (!string.IsNullOrEmpty(areaName) && areasMenu.ContainsKey(areaName))
            {
                menuTemplate = areasMenu[areaName];
            }
            else
            {
                menuTemplate = defaultMenu;
            }

            menu.Items = GetItems(menuTemplate.ItemContainers, controllerContext);

            if (initialize)
            {
                menu.Initialize(controllerContext);
            }

            return menu;
        }

        #region IMenuItemContainer Members

        private static IList<MenuItem> GetItems(IEnumerable<IMenuItemContainer> itemContainers, ControllerContext controllerContext)
        {
            var items = new List<MenuItem>();

            if (itemContainers != null)
            {
                foreach (var item in itemContainers)
                {
                    items.AddRange(item.GetItems(controllerContext));
                }
            }

            return items;
        }


        #endregion
    }
    public class Menu
    {
        public Menu()
        {
            Items = new List<MenuItem>();
        }
        public IList<MenuItem> Items { get; set; }

        public void Initialize(ControllerContext controllerContext)
        {
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    item.Initialize(controllerContext);
                }
            }
        }
    }
}
