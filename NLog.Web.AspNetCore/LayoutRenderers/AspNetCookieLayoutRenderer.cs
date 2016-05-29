﻿using System.Text;
#if !NETSTANDARD_1plus
using System.Web;
using System.Collections.Specialized;
#else
using Microsoft.Extensions.Primitives;
#endif
using NLog.LayoutRenderers;
using System.Collections.Generic;
using NLog.Config;
using NLog.Web.Enums;
using System;
using NLog.Web.Internal;

namespace NLog.Web.LayoutRenderers
{
    /// <summary>
    /// ASP.NET Request Cookie
    /// </summary>
    /// <para>Example usage of ${aspnet-request-cookie}</para>
    /// <example>
    /// <code lang="NLog Layout Renderer">
    /// ${aspnet-request-cookie:OutputFormat=Flat}
    /// ${aspnet-request-cookie:OutputFormat=Json}
    /// </code>
    /// </example>
    [LayoutRenderer("aspnet-request-cookie")]
    public class AspNetCookieLayoutRenderer : AspNetLayoutRendererBase
    {
        private const string doubleQuotes = "\"";
        private const string jsonStartBraces = "{";
        private const string jsonEndBraces = "}";
        private const string jsonElementSeparator = ",";

        private const string flatCookiesSeparator = "=";
        private const string flatItemSeperator = ",";


        /// <summary>
        /// List Cookie Key as String to be rendered from Request.
        /// </summary>
        public List<string> CookieNames { get; set; }

        /// <summary>
        /// Determines how the output is rendered. Possible Value: FLAT, JSON. Default is FLAT.
        /// </summary>
        [DefaultParameter]
        public AspNetLayoutOutputFormat OutputFormat { get; set; } = AspNetLayoutOutputFormat.Flat;

        /// <summary>
        /// Renders the ASP.NET Cookie appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void DoAppend(StringBuilder builder, LogEventInfo logEvent)
        {
            if (this.CookieNames?.Count > 0)
            {
                var httpRequest = HttpContextAccessor?.HttpContext?.TryGetRequest();

                if (httpRequest?.Cookies?.Count > 0)
                {
                    int i = 0;
                    foreach (var cookieName in this.CookieNames)
                    {
                        this.SerializeCookie(httpRequest.Cookies[cookieName], builder, i);
                        i++;
                    }
                }
            }
        }

#if !NETSTANDARD_1plus
        /// <summary>
        /// To Serialize the HttpCookie based on the configured output format.
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="builder"></param>
        /// <param name="index"></param>
        private void SerializeCookie(HttpCookie cookie, StringBuilder builder, int index)
        {
            if (cookie != null)
            {
                switch (this.OutputFormat)
                {
                    case AspNetLayoutOutputFormat.Flat:
                        if (index > 0)
                            builder.Append($"{flatItemSeperator}");

                        builder.Append($"{cookie.Name}{flatCookiesSeparator}{cookie.Value}");
                        break;
                    case AspNetLayoutOutputFormat.Json:
                        if (index > 0)
                            builder.Append($"{jsonElementSeparator}");

                        builder.Append($"{jsonStartBraces}{doubleQuotes}{cookie.Name}{flatCookiesSeparator}{cookie.Value}{doubleQuotes}{jsonEndBraces}");
                        break;
                }
            }
        }
#endif

#if NETSTANDARD_1plus
        private void SerializeCookie(StringValues cookie, StringBuilder builder, int index)
        {
            switch (this.OutputFormat)
            {
                case AspNetLayoutOutputFormat.Flat:
                    if (index > 0)
                        builder.Append($"{flatItemSeperator}");

                    builder.Append($"{cookie}");
                    break;
                case AspNetLayoutOutputFormat.Json:
                    if (index > 0)
                        builder.Append($"{jsonElementSeparator}");

                    builder.Append($"{jsonStartBraces}{doubleQuotes}{cookie}{doubleQuotes}{jsonEndBraces}");
                    break;
            }
        }
#endif
    }
}