using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
    }
}