using System;
using System.Collections.Generic;
using System.Net;

namespace consolestatisticsappcsharp
{
    public class CookieManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static CookieContainer cookies = new CookieContainer();

        public static CookieContainer Cookies
        {
            get 
            {
                return cookies;
            }
        }

        public static void AddCookie(Cookie cookie) 
        {
            //log.Debug("\n>>>>Storing Cookie to CookieManager>>>>: \n" +
                      //"    Domain: " + cookie.Domain + "\n" +
                      //"    Path: " + cookie.Path + "\n" +
                      //"    Expires: " + cookie.Expires.ToUniversalTime() + "\n" +
                      //"    Name: " + cookie.Name + "\n" +
                      //"    Value: " + cookie.Value + "\n");
            
            cookies.Add(cookie);
        }

        public static void SaveCookies(Uri uri, IDictionary<string, string> headers)
        {
            foreach (string key in headers.Keys)
            {
                if (key.ToUpper().Equals("SET-COOKIE"))
                {
                    string cookieValue = headers[key];

                    CookieManager.AddCookies(uri, cookieValue);
                }
            }
        }

        public static void AddCookies(Uri uri, string cookieHeader)
        {
            cookies.SetCookies(uri, cookieHeader);
        }

        public static CookieCollection GetCookieCollection(Uri uri)
        {
            return cookies.GetCookies(uri);    
        }

        public static void DumpCookies(String baseUrl)
        {
            CookieCollection cookieCollection = cookies.GetCookies(new Uri(baseUrl + "/statistics/v3"));
            log.Debug("\nCookies for " + baseUrl + ": " + cookies.Count);

            foreach (Cookie cookie in cookieCollection)
            {
                log.Debug("\n<<<<Cookie from CookieContainer<<<<: \n" +
                          "    Domain: " + cookie.Domain + "\n" +
                          "    Path: " + cookie.Path + "\n" +
                          "    Expires: " + cookie.Expires.ToUniversalTime() + "\n" +
                          "    Name: " + cookie.Name + "\n" +
                          "    Value: " + cookie.Value + "\n");log.Debug(cookie.Name + ": " + cookie.Value);
            }
        }
    }
}
