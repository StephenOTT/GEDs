using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin
{
    public enum DataReaderType
    {
        Component,
        Structure,
        Unknown
    }

    public interface IDataReader
    {
        /// <summary>
        /// Will pass in the Settings connection string "CONNECTION_STRING"
        /// </summary>
        /// <param name="connectionString"></param>
        void Connect(string connectionString);

        /// <summary>
        /// Generate the list of data for retreival. Allow the parent application to determine when this is done 
        /// by calling this method.
        /// </summary>
        /// <returns></returns>
        bool BuildDataList();

        /// <summary>
        /// Return back the string for GEDs from the dataset
        /// </summary>
        /// <param name="dataRecordId"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        string Fetch(int dataRecordId, string fieldName);

        IDataReader FetchParent(int dataRecordId);
        IDataReader[] FetchChildren(int dataRecordId);

        /// <summary>
        /// Return back the type of set the parent application is referencing
        /// </summary>
        /// <param name="dataRecordId"></param>
        /// <returns></returns>
        DataReaderType GetType(int dataRecordId);

        /// <summary>
        /// Allow the parent application to iterate the datalist
        /// </summary>
        /// <returns></returns>
        IEnumerable<int> DataList();

        void Dispose();
    }
}
