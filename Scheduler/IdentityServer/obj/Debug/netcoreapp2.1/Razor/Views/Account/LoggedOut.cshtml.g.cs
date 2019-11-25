#pragma checksum "C:\Projects\Scheduler\Scheduler\IdentityServer\Views\Account\LoggedOut.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "a6dc8779b884d4aa6f14e5b455e65f369424a978"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Account_LoggedOut), @"mvc.1.0.view", @"/Views/Account/LoggedOut.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Account/LoggedOut.cshtml", typeof(AspNetCore.Views_Account_LoggedOut))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "C:\Projects\Scheduler\Scheduler\IdentityServer\Views\_ViewImports.cshtml"
using IdentityServer;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a6dc8779b884d4aa6f14e5b455e65f369424a978", @"/Views/Account/LoggedOut.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"bf452908527b88b10d0dce3da0d69e0799b5f0fd", @"/Views/_ViewImports.cshtml")]
    public class Views_Account_LoggedOut : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<LoggedOutViewModel>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("src", new global::Microsoft.AspNetCore.Html.HtmlString("~/js/signout-redirect.js"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(27, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 3 "C:\Projects\Scheduler\Scheduler\IdentityServer\Views\Account\LoggedOut.cshtml"
   
    // set this so the layout rendering sees an anonymous user
    ViewData["signed-out"] = true;

#line default
#line hidden
            BeginContext(137, 110, true);
            WriteLiteral("\r\n<div class=\"page-header logged-out\">\r\n    <h1>\r\n        <small>You are now logged out</small>\r\n    </h1>\r\n\r\n");
            EndContext();
#line 13 "C:\Projects\Scheduler\Scheduler\IdentityServer\Views\Account\LoggedOut.cshtml"
     if (Model.PostLogoutRedirectUri != null)
    {

#line default
#line hidden
            BeginContext(301, 59, true);
            WriteLiteral("        <div>\r\n            <a class=\"PostLogoutRedirectUri\"");
            EndContext();
            BeginWriteAttribute("href", " href=\"", 360, "\"", 395, 1);
#line 16 "C:\Projects\Scheduler\Scheduler\IdentityServer\Views\Account\LoggedOut.cshtml"
WriteAttributeValue("", 367, Model.PostLogoutRedirectUri, 367, 28, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(396, 38, true);
            WriteLiteral(">Log in</a> to the\r\n            <span>");
            EndContext();
            BeginContext(435, 16, false);
#line 17 "C:\Projects\Scheduler\Scheduler\IdentityServer\Views\Account\LoggedOut.cshtml"
             Write(Model.ClientName);

#line default
#line hidden
            EndContext();
            BeginContext(451, 44, true);
            WriteLiteral("</span> application again.\r\n        </div>\r\n");
            EndContext();
#line 19 "C:\Projects\Scheduler\Scheduler\IdentityServer\Views\Account\LoggedOut.cshtml"
    }

#line default
#line hidden
            BeginContext(502, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 21 "C:\Projects\Scheduler\Scheduler\IdentityServer\Views\Account\LoggedOut.cshtml"
     if (Model.SignOutIframeUrl != null)
    {

#line default
#line hidden
            BeginContext(553, 52, true);
            WriteLiteral("        <iframe width=\"0\" height=\"0\" class=\"signout\"");
            EndContext();
            BeginWriteAttribute("src", " src=\"", 605, "\"", 634, 1);
#line 23 "C:\Projects\Scheduler\Scheduler\IdentityServer\Views\Account\LoggedOut.cshtml"
WriteAttributeValue("", 611, Model.SignOutIframeUrl, 611, 23, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(635, 12, true);
            WriteLiteral("></iframe>\r\n");
            EndContext();
#line 24 "C:\Projects\Scheduler\Scheduler\IdentityServer\Views\Account\LoggedOut.cshtml"
    }

#line default
#line hidden
            BeginContext(654, 10, true);
            WriteLiteral("</div>\r\n\r\n");
            EndContext();
            DefineSection("scripts", async() => {
                BeginContext(683, 2, true);
                WriteLiteral("\r\n");
                EndContext();
#line 29 "C:\Projects\Scheduler\Scheduler\IdentityServer\Views\Account\LoggedOut.cshtml"
     if (Model.AutomaticRedirectAfterSignOut)
    {

#line default
#line hidden
                BeginContext(739, 8, true);
                WriteLiteral("        ");
                EndContext();
                BeginContext(747, 48, false);
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("script", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "2bd0aab3afe045fba4aa807698947cb3", async() => {
                }
                );
                __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper>();
                __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper);
                __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
                await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                if (!__tagHelperExecutionContext.Output.IsContentModified)
                {
                    await __tagHelperExecutionContext.SetOutputContentAsync();
                }
                Write(__tagHelperExecutionContext.Output);
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                EndContext();
                BeginContext(795, 2, true);
                WriteLiteral("\r\n");
                EndContext();
#line 32 "C:\Projects\Scheduler\Scheduler\IdentityServer\Views\Account\LoggedOut.cshtml"
    }

#line default
#line hidden
            }
            );
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<LoggedOutViewModel> Html { get; private set; }
    }
}
#pragma warning restore 1591
