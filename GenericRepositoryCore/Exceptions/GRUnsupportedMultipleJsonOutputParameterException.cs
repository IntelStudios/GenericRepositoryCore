using System;

namespace GenericRepository.Exceptions
{
    public class GRUnsupportedMultipleJsonOutputParameterException : ApplicationException
    {
        public GRUnsupportedMultipleJsonOutputParameterException(string storedProcedureName) : base($"Unsupported multiple json output parameters for stored procedure {storedProcedureName}")
        {

        }
    }
}