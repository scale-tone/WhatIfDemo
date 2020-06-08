using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace WhatIfDemo.Common
{
    public static class Helpers
    {
        // Saves changes to DB, ignoring potential Primary Key Violation exceptions
        public static async Task SaveChangesIdempotentlyAsync(this DbContext context, Action<SqlException> primaryKeyViolationHandler = null)
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var innerEx = ex.InnerException as SqlException;

                // if it was a violation of primary key constraint
                if (innerEx?.Number == 2627)
                {
                    primaryKeyViolationHandler?.Invoke(innerEx);
                }
                else
                {
                    throw;
                }
            }
        }

        // Gets the DNS name of current App Service (or current devbox)
        public static string GetHostName()
        {
            string hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

            // this was needed, probably, to overcome some bug in earlier versions of runtime
            if (hostName.StartsWith("0.0.0.0"))
            {
                return $"https://{hostName.Replace("0.0.0.0", "localhost")}";
            }
            else
            {
                return $"https://{hostName}";
            }
        }

        // Obtains information about accessing user by making a local HTTP call to /.auth/me endpoint.
        public static Task<string> GetAccessingUserIdAsync(HttpRequest request)
        {
            return GetAccessingUserIdAsync(request, null);
        }

        // Obtains information about accessing user by making a local HTTP call to /.auth/me endpoint.
        // acceptedAuthProviders, if specified, should contain the list of accepted authentication providers, e.g. "facebook", "aad" etc.
        public static async Task<string> GetAccessingUserIdAsync(HttpRequest request, string[] acceptedAuthProviders = null)
        {
            string userId = null;

            // Facebook claims are not passed via ClaimsPrincipal yet, unfortunately.
            // So we'll just make a local call to our own /.auth/me endpoint returning user info.
            string uri = GetHostName() + Constants.AuthMeEndpointUri;
            using (var client = new WebClient())
            {
                // Propagating the incoming token
                client.Headers.Add(Constants.AuthorizationHeaderName, request.Headers[Constants.AuthorizationHeaderName]);

                dynamic authMeResponse = (await client.DownloadStringTaskAsync(uri)).FromJson();

                // Checking the auth provider
                if(acceptedAuthProviders != null)
                {
                    string authProviderName = authMeResponse[0].provider_name;
                    if (!acceptedAuthProviders.Contains(authProviderName))
                    {
                        // TODO: Add better error handling
                        throw new HttpResponseException(HttpStatusCode.Unauthorized);
                    }
                }

                dynamic userClaims = authMeResponse[0].user_claims;

                foreach (dynamic claim in userClaims)
                {
                    if (claim.typ == ClaimTypes.NameIdentifier)
                    {
                        userId = claim.val;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                // TODO: Add better error handling
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            return userId;
        }

        public static byte[] ToByteArray<T>(this T msg)
        {
            using (var stream = new MemoryStream())
            {
                DataContractBinarySerializer<T>.Instance.WriteObject(stream, msg);
                return stream.ToArray();
            }
        }

        public static T ToObject<T>(this byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return (T)DataContractBinarySerializer<T>.Instance.ReadObject(stream);
            }
        }

        public static string ToJson(this object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public static dynamic FromJson(this string json)
        {
            return JsonConvert.DeserializeObject(json);
        }
    }
}