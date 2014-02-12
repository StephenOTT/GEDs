using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Plugin;

namespace Service.DataSets
{
    internal class GedsEntity : IGedsEntity
    {
        private string _value;
        public string Value {
            get
            {
                if (_value == null)
                    return String.Empty;

                if (GedsUnit != null && GedsUnit.MaxLength < _value.Length)
                    return _value.Substring(0, GedsUnit.MaxLength);
                return _value;
            }
            set
            {
                if (value != null)
                {
                    _value = value.Replace("\n", "");
                    _value = _value.Replace("\r", "");
                    _value = _value.Trim();
                }
                else
                {
                    _value = String.Empty;
                }
            }
        }
        public IGedUnit GedsUnit { get; set; }

        public string ApplyGedsRule()
        {
            if (String.IsNullOrEmpty(Value.Trim()))
                return String.Empty;
            if (GedsUnit == null)
                return Value;

            if (Value.Length > GedsUnit.MaxLength)
                Value = Value.Substring(0, GedsUnit.MaxLength);

            return Value;
        }

        public bool SkipEntity()
        {
            if (GedsUnit != null && GedsUnit.Ignore)
                return true;
            return false;
        }

        public bool ValidateEntity()
        {
            if (GedsUnit == null)
                return false;

            if (GedsUnit.Mandatory)
            {
                if (String.IsNullOrEmpty(Value))
                {
                    if (String.IsNullOrEmpty(GedsUnit.DefaultValue))
                        return false; //must have something
                    Value = GedsUnit.DefaultValue;
                }
                else
                {
                    //check against regex validation
                    if (!String.IsNullOrEmpty(GedsUnit.Validation))
                    {
                        if (!Regex.Match(Value, GedsUnit.Validation).Success) // doesnt match
                        {
                            return false;
                            //Not using below because Default Value should only be inputed when the Value collected is empty, if we collected something better to skip row and log error
                            //check if matches default 
                            //if (!String.IsNullOrEmpty(GedsUnit.DefaultValue) && Regex.Match(GedsUnit.DefaultValue, GedsUnit.Validation).Success)
                            //    Value = GedsUnit.DefaultValue;
                        }
                    }
                }
            }

            return true;
        }

        public void Dispose()
        {
            if (GedsUnit != null)
                GedsUnit.Dispose();
        }
    }
}
