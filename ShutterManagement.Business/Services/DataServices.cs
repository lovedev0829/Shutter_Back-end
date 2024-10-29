using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json;

namespace ShutterManagement.Business.Services
{
    public class DataServices
    {
        private readonly ShutterDbContext dbContext;
        public DataServices(ShutterDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task CallSP(string storedProcedure, object data, params SqlParameter[] parameters)
        {
            using (var con = dbContext.Database.GetDbConnection())
            {
                try
                {
                    con.Open();

                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = storedProcedure;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }


                        cmd.ExecuteNonQuery();
                        con.Close();
                        Console.WriteLine("Stored procedure executed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<List<T>> CallSPtoRead<T>(string storedProcedure, Func<SqlDataReader, T> mapFunc, params SqlParameter[] parameters) where T : class
        {
            var result = new List<T>();

            using (var con = dbContext.Database.GetDbConnection())
            {
                await con.OpenAsync();

                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = storedProcedure;
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    try
                    {
                        using (var dbReader = await cmd.ExecuteReaderAsync())
                        {
                            var sqlReader = dbReader as SqlDataReader;

                            while (await sqlReader.ReadAsync())
                            {
                                result.Add(mapFunc(sqlReader));
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine($"SQL Error: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }

            return result;
        }

        public T MapClass<T>(SqlDataReader reader)
        {
            object f = "";
            return (T)f;
        }
    }
}
