using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace ERPIndia.Class.Helper
{
    /// <summary>
    /// Paging parameters interface.
    /// </summary>
    public interface IPagingParams
    {
        /// <summary>
        /// Gets the page count.
        /// </summary>
        /// <value>
        /// The page count.
        /// </value>
        int PageCount { get; }

        /// <summary>
        /// Gets the total item count.
        /// </summary>
        /// <value>
        /// The total item count.
        /// </value>
        int TotalItemCount { get; }

        /// <summary>
        /// Gets the page number.
        /// </summary>
        /// <value>
        /// The page number.
        /// </value>
        int PageNumber { get; }

        /// <summary>
        /// Gets the size of the page.
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        int PageSize { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has previous page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has previous page; otherwise, <c>false</c>.
        /// </value>
        bool HasPreviousPage { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has next page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has next page; otherwise, <c>false</c>.
        /// </value>
        bool HasNextPage { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is first page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is first page; otherwise, <c>false</c>.
        /// </value>
        bool IsFirstPage { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is last page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is last page; otherwise, <c>false</c>.
        /// </value>
        bool IsLastPage { get; }

        /// <summary>
        /// Gets the first item on page.
        /// </summary>
        /// <value>
        /// The first item on page.
        /// </value>
        int FirstItemOnPage { get; }

        /// <summary>
        /// Gets the last item on page.
        /// </summary>
        /// <value>
        /// The last item on page.
        /// </value>
        int LastItemOnPage { get; }
    }

    /// <summary>
    /// HTML helper class.
    /// </summary>
    public static class PagingHelper
    {
        #region Public Methods

        /// <summary>
        /// To the static paged list.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="dataList">The data list.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalItems">The total items.</param>
        /// <returns>
        /// Returns static paged list.
        /// </returns>
        public static StaticPagedList<T> ToStaticPagedList<T>(this IEnumerable<T> dataList, int? page, int pageSize, int totalItems)
        {
            if (page.HasValue && page < 1)
            {
                return null;
            }

            if (pageSize == -1 || pageSize == 0)
            {
                pageSize = totalItems;
            }

            var listPaged = new StaticPagedList<T>(dataList, page ?? 1, pageSize, totalItems);

            return listPaged;
        }

        /// <summary>
        /// Gets pager HTML string.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="pagingParams">The paging parameters.</param>
        /// <param name="generatePageUrl">The generate page URL.</param>
        /// <returns>Returns pager HTML string.</returns>
        public static MvcHtmlString PagedListPager(this System.Web.Mvc.HtmlHelper html, IPagingParams pagingParams, Func<int, string> generatePageUrl)
        {
            return PagedListPager(html, pagingParams, generatePageUrl, new PagedListRenderOptions());
        }

        /// <summary>
        /// Gets pager HTML string.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="pagingParams">The paging parameters.</param>
        /// <param name="generatePageUrl">The generate page URL.</param>
        /// <param name="options">The options.</param>
        /// <returns>Returns pager HTML string.</returns>
        public static MvcHtmlString PagedListPager(this System.Web.Mvc.HtmlHelper html, IPagingParams pagingParams, Func<int, string> generatePageUrl, PagedListRenderOptions options)
        {
            if (options.Display == PagedListDisplayMode.Never || (options.Display == PagedListDisplayMode.IfNeeded && pagingParams.PageCount <= 1))
            {
                return null;
            }

            var listItemLinks = new List<TagBuilder>();

            ////calculate start and end of range of page numbers
            var firstPageToDisplay = 1;
            var lastPageToDisplay = pagingParams.PageCount;
            var pageNumbersToDisplay = lastPageToDisplay;

            if (options.MaximumPageNumbersToDisplay.HasValue && pagingParams.PageCount > options.MaximumPageNumbersToDisplay)
            {
                //// cannot fit all pages into pager
                var maxPageNumbersToDisplay = options.MaximumPageNumbersToDisplay.Value;
                firstPageToDisplay = pagingParams.PageNumber - (maxPageNumbersToDisplay / 2);

                if (firstPageToDisplay < 1)
                {
                    firstPageToDisplay = 1;
                }

                pageNumbersToDisplay = maxPageNumbersToDisplay;
                lastPageToDisplay = firstPageToDisplay + pageNumbersToDisplay - 1;

                if (lastPageToDisplay > pagingParams.PageCount)
                {
                    firstPageToDisplay = pagingParams.PageCount - maxPageNumbersToDisplay + 1;
                }
            }

            ////first
            if (options.DisplayLinkToFirstPage == PagedListDisplayMode.Always || (options.DisplayLinkToFirstPage == PagedListDisplayMode.IfNeeded && firstPageToDisplay > 1))
            {
                listItemLinks.Add(GetFirstTag(pagingParams, generatePageUrl, options));
            }

            ////previous
            if (options.DisplayLinkToPreviousPage == PagedListDisplayMode.Always || (options.DisplayLinkToPreviousPage == PagedListDisplayMode.IfNeeded && !pagingParams.IsFirstPage))
            {
                listItemLinks.Add(GetPreviousTag(pagingParams, generatePageUrl, options));
            }

            ////text
            if (options.DisplayPageCountAndCurrentLocation)
            {
                listItemLinks.Add(GetPageCountAndLocationText(pagingParams, options));
            }

            ////text
            if (options.DisplayItemSliceAndTotal)
            {
                listItemLinks.Add(GetItemSliceAndTotalText(pagingParams, options));
            }

            ////page
            if (options.DisplayLinkToIndividualPages)
            {
                ////if there are previous page numbers not displayed, show an ellipsis
                ////if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && firstPageToDisplay > 1)
                ////    listItemLinks.Add(Ellipses(options));

                foreach (var i in Enumerable.Range(firstPageToDisplay, pageNumbersToDisplay))
                {
                    ////show delimiter between page numbers
                    if (i > firstPageToDisplay && !string.IsNullOrWhiteSpace(options.DelimiterBetweenPageNumbers))
                    {
                        listItemLinks.Add(GetWrapInListItem(options.DelimiterBetweenPageNumbers));
                    }

                    ////show page number link
                    listItemLinks.Add(GetPageTag(i, pagingParams, generatePageUrl, options));
                }

                ////if there are subsequent page numbers not displayed, show an ellipsis
                ////if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && (firstPageToDisplay + pageNumbersToDisplay - 1) < list.PageCount)
                ////    listItemLinks.Add(Ellipses(options));
            }

            ////next
            if (options.DisplayLinkToNextPage == PagedListDisplayMode.Always || (options.DisplayLinkToNextPage == PagedListDisplayMode.IfNeeded && !pagingParams.IsLastPage))
            {
                listItemLinks.Add(GetNextTag(pagingParams, generatePageUrl, options));
            }

            ////last
            if (options.DisplayLinkToLastPage == PagedListDisplayMode.Always || (options.DisplayLinkToLastPage == PagedListDisplayMode.IfNeeded && lastPageToDisplay < pagingParams.PageCount))
            {
                listItemLinks.Add(GetLastTag(pagingParams, generatePageUrl, options));
            }

            if (listItemLinks.Any())
            {
                ////append class to first item in list?
                if (!string.IsNullOrWhiteSpace(options.ClassToApplyToFirstListItemInPager))
                {
                    listItemLinks.First().AddCssClass(options.ClassToApplyToFirstListItemInPager);
                }

                ////append class to last item in list?
                if (!string.IsNullOrWhiteSpace(options.ClassToApplyToLastListItemInPager))
                {
                    listItemLinks.Last().AddCssClass(options.ClassToApplyToLastListItemInPager);
                }

                ////append classes to all list item links
                foreach (var li in listItemLinks)
                {
                    foreach (var c in options.LiElementClasses ?? Enumerable.Empty<string>())
                    {
                        li.AddCssClass(c);
                    }
                }
            }

            ////collapse all of the list items into one big string
            var listItemLinksString = listItemLinks.Aggregate(new StringBuilder(), (sb, listItem) => sb.Append(listItem.ToString()), sb => sb.ToString());
            var ul = new TagBuilder("ul") { InnerHtml = listItemLinksString };

            foreach (var c in options.UlElementClasses ?? Enumerable.Empty<string>())
            {
                ul.AddCssClass(c);
            }

            var outerDiv = new TagBuilder("div");

            foreach (var c in options.ContainerDivClasses ?? Enumerable.Empty<string>())
            {
                outerDiv.AddCssClass(c);
            }

            outerDiv.InnerHtml = ul.ToString();

            return new MvcHtmlString(outerDiv.ToString());
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the wrap in list item.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Returns wrap in list item.</returns>
        private static TagBuilder GetWrapInListItem(string text)
        {
            var li = new TagBuilder("li");
            li.SetInnerText(text);

            return li;
        }

        /// <summary>
        /// Gets the wrap in list item.
        /// </summary>
        /// <param name="inner">The inner.</param>
        /// <param name="options">The options.</param>
        /// <param name="classes">The classes.</param>
        /// <returns>Returns wrap in list item.</returns>
        private static TagBuilder GetWrapInListItem(TagBuilder inner, PagedListRenderOptions options, params string[] classes)
        {
            var li = new TagBuilder("li");

            foreach (var @class in classes)
            {
                li.AddCssClass(@class);
            }

            if (options.FunctionToTransformEachPageLink != null)
            {
                return options.FunctionToTransformEachPageLink(li, inner);
            }

            li.InnerHtml = inner.ToString();

            return li;
        }

        /// <summary>
        /// Gets the first tag.
        /// </summary>
        /// <param name="pagingParams">The paging parameters.</param>
        /// <param name="generatePageUrl">The generate page URL.</param>
        /// <param name="options">The options.</param>
        /// <returns>Returns first tag.</returns>
        private static TagBuilder GetFirstTag(IPagingParams pagingParams, Func<int, string> generatePageUrl, PagedListRenderOptions options)
        {
            const int TargetPageNumber = 1;
            var first = new TagBuilder("a") { InnerHtml = string.Format(options.LinkToFirstPageFormat, TargetPageNumber) };

            if (pagingParams.IsFirstPage)
            {
                return GetWrapInListItem(first, options, "PagedList-skipToFirst", "disabled");
            }

            first.Attributes["href"] = generatePageUrl(TargetPageNumber);

            return GetWrapInListItem(first, options, "PagedList-skipToFirst");
        }

        /// <summary>
        /// Gets the previous tag.
        /// </summary>
        /// <param name="pagingParams">The paging parameters.</param>
        /// <param name="generatePageUrl">The generate page URL.</param>
        /// <param name="options">The options.</param>
        /// <returns>Returns previous tag.</returns>
        private static TagBuilder GetPreviousTag(IPagingParams pagingParams, Func<int, string> generatePageUrl, PagedListRenderOptions options)
        {
            var targetPageNumber = pagingParams.PageNumber - 1;
            var previous = new TagBuilder("a") { InnerHtml = string.Format(options.LinkToPreviousPageFormat, targetPageNumber) };
            previous.Attributes["rel"] = "prev";

            if (!pagingParams.HasPreviousPage)
            {
                return GetWrapInListItem(previous, options, "PagedList-skipToPrevious", "disabled");
            }

            previous.Attributes["href"] = generatePageUrl(targetPageNumber);

            return GetWrapInListItem(previous, options, "PagedList-skipToPrevious");
        }

        /// <summary>
        /// Gets the page tag.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="pagingParams">The paging parameters.</param>
        /// <param name="generatePageUrl">The generate page URL.</param>
        /// <param name="options">The options.</param>
        /// <returns>Returns page tag.</returns>
        private static TagBuilder GetPageTag(int i, IPagingParams pagingParams, Func<int, string> generatePageUrl, PagedListRenderOptions options)
        {
            var format = options.FunctionToDisplayEachPageNumber ?? (pageNumber => string.Format(options.LinkToIndividualPageFormat, pageNumber));
            var targetPageNumber = i;
            var page = new TagBuilder("a");
            page.SetInnerText(format(targetPageNumber));

            if (i == pagingParams.PageNumber)
            {
                return GetWrapInListItem(page, options, "active");
            }

            page.Attributes["href"] = generatePageUrl(targetPageNumber);

            return GetWrapInListItem(page, options);
        }

        /// <summary>
        /// Gets the next tag.
        /// </summary>
        /// <param name="pagingParams">The paging parameters.</param>
        /// <param name="generatePageUrl">The generate page URL.</param>
        /// <param name="options">The options.</param>
        /// <returns>Returns next tag.</returns>
        private static TagBuilder GetNextTag(IPagingParams pagingParams, Func<int, string> generatePageUrl, PagedListRenderOptions options)
        {
            var targetPageNumber = pagingParams.PageNumber + 1;
            var next = new TagBuilder("a") { InnerHtml = string.Format(options.LinkToNextPageFormat, targetPageNumber) };
            next.Attributes["rel"] = "next";

            if (!pagingParams.HasNextPage)
            {
                return GetWrapInListItem(next, options, "PagedList-skipToNext", "disabled");
            }

            next.Attributes["href"] = generatePageUrl(targetPageNumber);

            return GetWrapInListItem(next, options, "PagedList-skipToNext");
        }

        /// <summary>
        /// Gets the last tag.
        /// </summary>
        /// <param name="pagingParams">The paging parameters.</param>
        /// <param name="generatePageUrl">The generate page URL.</param>
        /// <param name="options">The options.</param>
        /// <returns>Returns last tag.</returns>
        private static TagBuilder GetLastTag(IPagingParams pagingParams, Func<int, string> generatePageUrl, PagedListRenderOptions options)
        {
            var targetPageNumber = pagingParams.PageCount;
            var last = new TagBuilder("a") { InnerHtml = string.Format(options.LinkToLastPageFormat, targetPageNumber) };

            if (pagingParams.IsLastPage)
            {
                return GetWrapInListItem(last, options, "PagedList-skipToLast", "disabled");
            }

            last.Attributes["href"] = generatePageUrl(targetPageNumber);

            return GetWrapInListItem(last, options, "PagedList-skipToLast");
        }

        /// <summary>
        /// Gets the page count and location text.
        /// </summary>
        /// <param name="pagingParams">The paging parameters.</param>
        /// <param name="options">The options.</param>
        /// <returns>Returns page count and location text.</returns>
        private static TagBuilder GetPageCountAndLocationText(IPagingParams pagingParams, PagedListRenderOptions options)
        {
            var text = new TagBuilder("a");
            text.SetInnerText(string.Format(options.PageCountAndCurrentLocationFormat, pagingParams.PageNumber, pagingParams.PageCount));

            return GetWrapInListItem(text, options, "PagedList-pageCountAndLocation", "disabled");
        }

        /// <summary>
        /// Gets the item slice and total text.
        /// </summary>
        /// <param name="pagingParams">The paging parameters.</param>
        /// <param name="options">The options.</param>
        /// <returns>Returns item slice and total text.</returns>
        private static TagBuilder GetItemSliceAndTotalText(IPagingParams pagingParams, PagedListRenderOptions options)
        {
            var text = new TagBuilder("a");
            text.SetInnerText(string.Format(options.ItemSliceAndTotalFormat, pagingParams.FirstItemOnPage, pagingParams.LastItemOnPage, pagingParams.TotalItemCount));

            return GetWrapInListItem(text, options, "PagedList-pageCountAndLocation", "disabled");
        }

        /// <summary>
        /// Gets the ellipses.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Returns ellipses.</returns>
        private static TagBuilder GetEllipses(PagedListRenderOptions options)
        {
            var a = new TagBuilder("a") { InnerHtml = options.EllipsesFormat };

            return GetWrapInListItem(a, options, "PagedList-ellipses", "disabled");
        }

        #endregion
    }

    /// <summary>
    /// Paged list render option class.
    /// </summary>
    public class PagedListRenderOptions
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedListRenderOptions"/> class.
        /// </summary>
        public PagedListRenderOptions()
        {
            this.DisplayLinkToFirstPage = PagedListDisplayMode.IfNeeded;
            this.DisplayLinkToLastPage = PagedListDisplayMode.IfNeeded;
            this.DisplayLinkToPreviousPage = PagedListDisplayMode.IfNeeded;
            this.DisplayLinkToNextPage = PagedListDisplayMode.IfNeeded;
            this.DisplayLinkToIndividualPages = true;
            this.DisplayPageCountAndCurrentLocation = false;
            this.MaximumPageNumbersToDisplay = 3;
            this.DisplayEllipsesWhenNotShowingAllPageNumbers = true;
            this.EllipsesFormat = "&#8230;";
            this.LinkToFirstPageFormat = "««";
            this.LinkToPreviousPageFormat = "«";
            this.LinkToIndividualPageFormat = "{0}";
            this.LinkToNextPageFormat = "»";
            this.LinkToLastPageFormat = "»»";
            this.PageCountAndCurrentLocationFormat = "Page {0} of {1}.";
            this.ItemSliceAndTotalFormat = "Showing items {0} through {1} of {2}.";
            this.FunctionToDisplayEachPageNumber = null;
            this.ClassToApplyToFirstListItemInPager = null;
            this.ClassToApplyToLastListItemInPager = null;
            this.ContainerDivClasses = new[] { "pagination-container" };
            this.UlElementClasses = new[] { "pagination" };
            this.LiElementClasses = Enumerable.Empty<string>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the container div classes.
        /// </summary>
        /// <value>
        /// The container div classes.
        /// </value>
        public IEnumerable<string> ContainerDivClasses { get; set; }

        /// <summary>
        /// Gets or sets the UL element classes.
        /// </summary>
        /// <value>
        /// The UL element classes.
        /// </value>
        public IEnumerable<string> UlElementClasses { get; set; }

        /// <summary>
        /// Gets or sets the li element classes.
        /// </summary>
        /// <value>
        /// The li element classes.
        /// </value>
        public IEnumerable<string> LiElementClasses { get; set; }

        /// <summary>
        /// Gets or sets the class to apply to first list item in pager.
        /// </summary>
        /// <value>
        /// The class to apply to first list item in pager.
        /// </value>
        public string ClassToApplyToFirstListItemInPager { get; set; }

        /// <summary>
        /// Gets or sets the class to apply to last list item in pager.
        /// </summary>
        /// <value>
        /// The class to apply to last list item in pager.
        /// </value>
        public string ClassToApplyToLastListItemInPager { get; set; }

        /// <summary>
        /// Gets or sets the display.
        /// </summary>
        /// <value>
        /// The display.
        /// </value>
        public PagedListDisplayMode Display { get; set; }

        /// <summary>
        /// Gets or sets the display link to first page.
        /// </summary>
        /// <value>
        /// The display link to first page.
        /// </value>
        public PagedListDisplayMode DisplayLinkToFirstPage { get; set; }

        /// <summary>
        /// Gets or sets the display link to last page.
        /// </summary>
        /// <value>
        /// The display link to last page.
        /// </value>
        public PagedListDisplayMode DisplayLinkToLastPage { get; set; }

        /// <summary>
        /// Gets or sets the display link to previous page.
        /// </summary>
        /// <value>
        /// The display link to previous page.
        /// </value>
        public PagedListDisplayMode DisplayLinkToPreviousPage { get; set; }

        /// <summary>
        /// Gets or sets the display link to next page.
        /// </summary>
        /// <value>
        /// The display link to next page.
        /// </value>
        public PagedListDisplayMode DisplayLinkToNextPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [display link to individual pages].
        /// </summary>
        /// <value>
        /// <c>true</c> if [display link to individual pages]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayLinkToIndividualPages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [display page count and current location].
        /// </summary>
        /// <value>
        /// <c>true</c> if [display page count and current location]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayPageCountAndCurrentLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [display item slice and total].
        /// </summary>
        /// <value>
        /// <c>true</c> if [display item slice and total]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayItemSliceAndTotal { get; set; }

        /// <summary>
        /// Gets or sets the maximum page numbers to display.
        /// </summary>
        /// <value>
        /// The maximum page numbers to display.
        /// </value>
        public int? MaximumPageNumbersToDisplay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [display ellipses when not showing all page numbers].
        /// </summary>
        /// <value>
        /// <c>true</c> if [display ellipses when not showing all page numbers]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayEllipsesWhenNotShowingAllPageNumbers { get; set; }

        /// <summary>
        /// Gets or sets the ellipses format.
        /// </summary>
        /// <value>
        /// The ellipses format.
        /// </value>
        public string EllipsesFormat { get; set; }

        /// <summary>
        /// Gets or sets the link to first page format.
        /// </summary>
        /// <value>
        /// The link to first page format.
        /// </value>
        public string LinkToFirstPageFormat { get; set; }

        /// <summary>
        /// Gets or sets the link to previous page format.
        /// </summary>
        /// <value>
        /// The link to previous page format.
        /// </value>
        public string LinkToPreviousPageFormat { get; set; }

        /// <summary>
        /// Gets or sets the link to individual page format.
        /// </summary>
        /// <value>
        /// The link to individual page format.
        /// </value>
        public string LinkToIndividualPageFormat { get; set; }

        /// <summary>
        /// Gets or sets the link to next page format.
        /// </summary>
        /// <value>
        /// The link to next page format.
        /// </value>
        public string LinkToNextPageFormat { get; set; }

        /// <summary>
        /// Gets or sets the link to last page format.
        /// </summary>
        /// <value>
        /// The link to last page format.
        /// </value>
        public string LinkToLastPageFormat { get; set; }

        /// <summary>
        /// Gets or sets the page count and current location format.
        /// </summary>
        /// <value>
        /// The page count and current location format.
        /// </value>
        public string PageCountAndCurrentLocationFormat { get; set; }

        /// <summary>
        /// Gets or sets the item slice and total format.
        /// </summary>
        /// <value>
        /// The item slice and total format.
        /// </value>
        public string ItemSliceAndTotalFormat { get; set; }

        /// <summary>
        /// Gets or sets the function to display each page number.
        /// </summary>
        /// <value>
        /// The function to display each page number.
        /// </value>
        public Func<int, string> FunctionToDisplayEachPageNumber { get; set; }

        /// <summary>
        /// Gets or sets the delimiter between page numbers.
        /// </summary>
        /// <value>
        /// The delimiter between page numbers.
        /// </value>
        public string DelimiterBetweenPageNumbers { get; set; }

        /// <summary>
        /// Gets or sets the function to transform each page link.
        /// </summary>
        /// <value>
        /// The function to transform each page link.
        /// </value>
        public Func<TagBuilder, TagBuilder, TagBuilder> FunctionToTransformEachPageLink { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Enables the unobtrusive ajax replacing.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="ajaxOptions">The ajax options.</param>
        /// <returns>Returns paged list render options.</returns>
        public static PagedListRenderOptions EnableUnobtrusiveAjaxReplacing(PagedListRenderOptions options, AjaxOptions ajaxOptions)
        {
            options.FunctionToTransformEachPageLink = (liTagBuilder, aTagBuilder) =>
            {
                var liClass = liTagBuilder.Attributes.ContainsKey("class") ? liTagBuilder.Attributes["class"] ?? string.Empty : string.Empty;

                if (ajaxOptions != null && !liClass.Contains("disabled") && !liClass.Contains("active"))
                {
                    foreach (var ajaxOption in ajaxOptions.ToUnobtrusiveHtmlAttributes())
                    {
                        aTagBuilder.Attributes.Add(ajaxOption.Key, ajaxOption.Value.ToString());
                    }
                }

                liTagBuilder.InnerHtml = aTagBuilder.ToString();

                return liTagBuilder;
            };

            return options;
        }

        /// <summary>
        /// Enables the unobtrusive ajax replacing.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Returns paged list render options.</returns>
        public static PagedListRenderOptions EnableUnobtrusiveAjaxReplacing(string id)
        {
            if (id.StartsWith("#"))
            {
                id = id.Substring(1);
            }

            var ajaxOptions = new AjaxOptions()
            {
                HttpMethod = "GET",
                InsertionMode = InsertionMode.Replace,
                UpdateTargetId = id
            };

            return EnableUnobtrusiveAjaxReplacing(new PagedListRenderOptions(), ajaxOptions);
        }

        /// <summary>
        /// Enables the unobtrusive ajax replacing.
        /// </summary>
        /// <param name="ajaxOptions">The ajax options.</param>
        /// <returns>Returns paged list render options.</returns>
        public static PagedListRenderOptions EnableUnobtrusiveAjaxReplacing(AjaxOptions ajaxOptions)
        {
            return EnableUnobtrusiveAjaxReplacing(new PagedListRenderOptions(), ajaxOptions);
        }

        #endregion
    }

    /// <summary>
    /// Static Paged list class.
    /// </summary>
    /// <typeparam name="T">Type of object.</typeparam>
    [Serializable]
    public class StaticPagedList<T> : IPagingParams, IEnumerable<T>
    {
        #region Variable Declaration

        protected readonly List<T> Subset = new List<T>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticPagedList{T}"/> class.
        /// </summary>
        /// <param name="subset">The subset.</param>
        /// <param name="metaData">The meta data.</param>
        public StaticPagedList(IEnumerable<T> subset, IPagingParams metaData)
            : this(subset, metaData.PageNumber, metaData.PageSize, metaData.TotalItemCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticPagedList{T}"/> class that contains the already divided subset and information about the size of the superset and the subset's position within it.
        /// </summary>
        /// <param name="subset">The single subset this collection should represent.</param>
        /// <param name="pageNumber">The one-based index of the subset of objects contained by this instance.</param>
        /// <param name="pageSize">The maximum size of any individual subset.</param>
        /// <param name="totalItemCount">The size of the superset.</param>
        /// <exception cref="ArgumentOutOfRangeException">The specified index cannot be less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified page size cannot be less than one.</exception>
        public StaticPagedList(IEnumerable<T> subset, int pageNumber, int pageSize, int totalItemCount)
        {
            this.Subset.AddRange(subset);

            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException("pageNumber", pageNumber, "PageNumber cannot be below 1.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException("pageSize", pageSize, "PageSize cannot be less than 1.");
            }

            //// set source to blank list if superset is null to prevent exceptions
            this.TotalItemCount = totalItemCount;
            this.PageSize = pageSize;
            this.PageNumber = pageNumber;
            this.PageCount = this.TotalItemCount > 0 ? (int)Math.Ceiling(this.TotalItemCount / (double)this.PageSize) : 0;
            this.HasPreviousPage = this.PageNumber > 1;
            this.HasNextPage = this.PageNumber < this.PageCount;
            this.IsFirstPage = this.PageNumber == 1;
            this.IsLastPage = this.PageNumber >= this.PageCount;
            this.FirstItemOnPage = ((this.PageNumber - 1) * this.PageSize) + 1;

            var numberOfLastItemOnPage = this.FirstItemOnPage + this.PageSize - 1;
            this.LastItemOnPage = numberOfLastItemOnPage > this.TotalItemCount ? this.TotalItemCount : numberOfLastItemOnPage;
        }

        #region Properties

        #region IPagedList Members

        /// <summary>
        /// Gets or sets the page count.
        /// </summary>
        /// <value>
        /// The page count.
        /// </value>
        public int PageCount { get; protected set; }

        /// <summary>
        /// Gets or sets the total item count.
        /// </summary>
        /// <value>
        /// The total item count.
        /// </value>
        public int TotalItemCount { get; protected set; }

        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        /// <value>
        /// The page number.
        /// </value>
        public int PageNumber { get; protected set; }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        public int PageSize { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has previous page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has previous page; otherwise, <c>false</c>.
        /// </value>
        public bool HasPreviousPage { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has next page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has next page; otherwise, <c>false</c>.
        /// </value>
        public bool HasNextPage { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is first page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is first page; otherwise, <c>false</c>.
        /// </value>
        public bool IsFirstPage { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is last page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is last page; otherwise, <c>false</c>.
        /// </value>
        public bool IsLastPage { get; protected set; }

        /// <summary>
        /// Gets or sets the first item on page.
        /// </summary>
        /// <value>
        /// The first item on page.
        /// </value>
        public int FirstItemOnPage { get; protected set; }

        /// <summary>
        /// Gets or sets the last item on page.
        /// </summary>
        /// <value>
        /// The last item on page.
        /// </value>
        public int LastItemOnPage { get; protected set; }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count
        {
            get
            {
                return this.Subset.Count;
            }
        }

        /// <summary>
        /// Gets the <see cref="T"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="T"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>Returns object.</returns>
        public T this[int index]
        {
            get
            {
                return this.Subset[index];
            }
        }

        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.Subset.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}