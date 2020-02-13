using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace GenericRepository.Exceptions
{
    public class GRModelNotValidException : ApplicationException
    {
        public List<GRModelError> Errors { get; private set; }

        public GRModelNotValidException(List<GRModelError> errors) : base ("Model is not valid.")
        {
            this.Errors = errors;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(this.Message);

            if (Errors != null)
            {
                foreach (var error in Errors)
                {
                    sb.AppendLine(error.ToString());
                }
            }

            return sb.ToString();
        }
    }

    public class GRModelError
    {
        public PropertyInfo Property { get; private set; }
        public object Value { get; private set; }
        public ICollection<ValidationResult> Errors { get; private set; }

        public GRModelError(PropertyInfo property, object value, ICollection<ValidationResult> errors)
        {
            this.Property = property;
            this.Value = value;
            this.Errors = errors;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" - Property: '{0}'{1}", Property, Environment.NewLine);

            foreach (var err in Errors)
            {
                sb.AppendFormat("     - {0}{1}", err.ErrorMessage, Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}
