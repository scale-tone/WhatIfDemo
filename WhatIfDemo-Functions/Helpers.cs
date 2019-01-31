using System;
using System.Data.SqlClient;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace WhatIfDemo
{
    static class Helpers
    {
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
        public static string GetHostName()
        {
            string hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

            if (hostName.StartsWith("0.0.0.0"))
            {
                return $"https://{hostName.Replace("0.0.0.0", "localhost")}";
            }
            else
            {
                return $"https://{hostName}";
            }
        }

        private const string SessionTokenHeaderName = "X-ZUMO-AUTH";
        private const string AuthMeEndpointUri = "/.auth/me";

        public static async Task<string> GetAccessingUserId(HttpRequest request)
        {
            string userId = null;

            // Facebook claims are not passed via ClaimsPrincipal yet, unfortunately.
            // So we'll just make a local call to our own /.auth/me endpoint returning user info.
            string uri = GetHostName() + AuthMeEndpointUri;
            using (var client = new WebClient())
            {
                // Propagating the incoming session token, passed via X-ZUMO-AUTH header
                client.Headers.Add(SessionTokenHeaderName, request.Headers[SessionTokenHeaderName]);

                dynamic authMeResponse = JsonConvert.DeserializeObject(await client.DownloadStringTaskAsync(uri));
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
    }
}