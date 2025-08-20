using System;
using System.Collections.Generic;

namespace ERPIndia.Models
{
    /// <summary>
    /// Site menu model class.
    /// </summary>
    public class SiteMenuModel
    {
        /// <summary>
        /// Gets or sets the menu id.
        /// </summary>
        /// <value>
        /// The menu id.
        /// </value>
        public long MenuId { get; set; }

        /// <summary>
        /// Gets or sets the parent menu id.
        /// </summary>
        /// <value>
        /// The parent menu id.
        /// </value>
        public long ParentMenuId { get; set; }

        /// <summary>
        /// Gets or sets the menu code.
        /// </summary>
        /// <value>
        /// The menu code.
        /// </value>
        public string MenuCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the menu.
        /// </summary>
        /// <value>
        /// The name of the menu.
        /// </value>
        public string MenuName { get; set; }

        /// <summary>
        /// Gets or sets the name of the menu page.
        /// </summary>
        /// <value>
        /// The name of the menu page.
        /// </value>
        public string MenuPageName { get; set; }

        /// <summary>
        /// Gets or sets the name of the menu image.
        /// </summary>
        /// <value>
        /// The name of the menu image.
        /// </value>
        public string MenuImageName { get; set; }

        /// <summary>
        /// Gets or sets the menu order no.
        /// </summary>
        /// <value>
        /// The menu order no.
        /// </value>
        public int MenuOrderNo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the sub menu list.
        /// </summary>
        /// <value>
        /// The sub menu list.
        /// </value>
        public List<SiteMenuModel> SubMenuList { get; set; }

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();

        /// <summary>
        /// Gets the color of the get menu.
        /// </summary>
        /// <value>
        /// The color of the get menu.
        /// </value>
        public string GetMenuColor
        {
            get
            {

                int rndNum;

                lock (syncLock)
                {
                    rndNum = random.Next(1, 6);
                }

                if (rndNum == 1)
                {
                    return "bg-blue";
                }
                else if (rndNum == 2)
                {
                    return "bg-aqua";
                }
                else if (rndNum == 3)
                {
                    return "bg-red";
                }
                else if (rndNum == 4)
                {
                    return "bg-yellow";
                }
                else
                {
                    return "bg-green";
                }




                //if (this.MenuId % 5 == 0)
                //{
                //    return "bg-blue";
                //}
                //else if (this.MenuId % 4 == 0)
                //{
                //    return "bg-aqua";
                //}
                //else if (this.MenuId % 3 == 0)
                //{
                //    return "bg-red";
                //}
                //else if (this.MenuId % 2 == 0)
                //{
                //    return "bg-yellow";
                //}
                //else
                //{
                //    return "bg-green";
                //}
            }
        }
    }
}