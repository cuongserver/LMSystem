using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using LMSystem.Resource;
namespace LMSystem.HTMLHelper
{
    public static class ExtraHTMLHelpers
    {
        public static IHtmlString OperatorDropdown(string className, string commonName)
        {
            var element = new TagBuilder("select");
            string[] operationList = { "0", "1", "2", "3" };

            var opt0 = new TagBuilder("option");
            opt0.MergeAttribute("value", operationList[0]);
            opt0.InnerHtml = RegionSetting.equal;

            var opt1 = new TagBuilder("option");
            opt1.MergeAttribute("value", operationList[1]);
            opt1.InnerHtml = RegionSetting.notEqual;

            var opt2 = new TagBuilder("option");
            opt2.MergeAttribute("value", operationList[2]);
            opt2.MergeAttribute("selected", null);
            opt2.InnerHtml = RegionSetting.contain;

            var opt3 = new TagBuilder("option");
            opt3.MergeAttribute("value", operationList[3]);
            opt3.InnerHtml = RegionSetting.notContain;

            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.Append(opt0.ToString(TagRenderMode.Normal));
            htmlBuilder.Append(opt1.ToString(TagRenderMode.Normal));
            htmlBuilder.Append(opt2.ToString(TagRenderMode.Normal));
            htmlBuilder.Append(opt3.ToString(TagRenderMode.Normal));

            element.InnerHtml = htmlBuilder.ToString();
            element.AddCssClass("text-size-10 h-23px");
            element.AddCssClass(className);
            element.MergeAttribute("name", commonName);
            var html = element.ToString(TagRenderMode.Normal);
            return MvcHtmlString.Create(html);
        }
    }
}