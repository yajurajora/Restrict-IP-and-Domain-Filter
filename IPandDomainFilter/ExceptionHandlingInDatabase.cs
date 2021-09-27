using IPandDomainFilter.Abstraction;
using IPandDomainFilter.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace IPandDomainFilter
{
    public class ExceptionHandlingInDatabase : IExceptionHandlingInDatabase
    {
        public IConfiguration _iconfiguration;
        private readonly string connection;
        public ExceptionHandlingInDatabase(IConfiguration iconfiguration)
        {
            _iconfiguration = iconfiguration;
            connection = iconfiguration["ConnectionString:MyConnection"];
        }
        public void StoreException(ErrorMessage errorMessage)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connection))
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand("insertException",sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("ErrorMessage", errorMessage.errorMessage);
                sqlCommand.Parameters.AddWithValue("StatusCode", errorMessage.statusCode);
                int rows = sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
            }
        }
    }
}
