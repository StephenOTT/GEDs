using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Service.DataReader;

using ExternalPlugin = Plugin;

namespace Service.DataSets
{
    internal class SortData
    {
        public string Value { get; set; }
        public int Order { get; set; }
    }

    internal class GedsSort
    {
        public List<SortData> StructureSort { get; set; }
        public List<SortData> ComponentSort { get; set; }
        public string StructureSortCompareAgainstFieldName { get; set; }
        public string ComponentSortCompareAgainstFieldName { get; set; }
        public string StructureSortFieldName { get; set; }
        public string ComponentSortFieldName { get; set; }

        private int defaultSortIndex;

        private int? maxStructureSortIndex;
        public int MaxStructureSortIndex
        {
            get
            {
                if (maxStructureSortIndex == null)
                    GenerateMaxSortIndex(ExternalPlugin.DataReaderType.Structure);
                return maxStructureSortIndex.GetValueOrDefault(defaultSortIndex);
            }
        }

        private int? maxComponentSortIndex;
        public int MaxComponentSortIndex
        {
            get
            {
                if (maxComponentSortIndex == null)
                    GenerateMaxSortIndex(ExternalPlugin.DataReaderType.Component);
                return maxComponentSortIndex.GetValueOrDefault(defaultSortIndex);
            }
        }

        public GedsSort()
        {
            maxStructureSortIndex = null;
            maxComponentSortIndex = null;
            defaultSortIndex = 1000;
        }

        private void GenerateMaxSortIndex(ExternalPlugin.DataReaderType type)
        {
            if (type == ExternalPlugin.DataReaderType.Component)
            {
                if (ComponentSort != null)
                {
                    maxComponentSortIndex = ComponentSort.OrderByDescending(o => o.Order)
                                                .Select(s => s.Order)
                                                .FirstOrDefault();

                    if (maxComponentSortIndex == default(int))
                        maxComponentSortIndex = defaultSortIndex;
                }
                else maxComponentSortIndex = defaultSortIndex;
            }
            else
            {
                if (StructureSort != null)
                {
                    maxStructureSortIndex = StructureSort.OrderByDescending(o => o.Order)
                                                .Select(s=> s.Order)
                                                .FirstOrDefault();

                    if (maxStructureSortIndex == default(int))
                        maxStructureSortIndex = defaultSortIndex;
                }
                else maxStructureSortIndex = defaultSortIndex;
            }
        }

        public int GetSortIndex(ExternalPlugin.DataReaderType type, string fieldName, string fieldValue)
        {
            if (type == ExternalPlugin.DataReaderType.Structure)
            {
                if (StructureSort == null)
                    return defaultSortIndex;
                if (String.IsNullOrEmpty(StructureSortCompareAgainstFieldName))
                    return defaultSortIndex;
                if (String.IsNullOrEmpty(fieldValue.Trim()))
                    return defaultSortIndex;

                if (fieldName.Equals(StructureSortCompareAgainstFieldName))
                {
                    var sortIndex = StructureSort.Find(s => s.Value.Contains(fieldValue));
                    if (sortIndex != null)
                        return sortIndex.Order;
                }

                return MaxStructureSortIndex;
            }
            else if (type == ExternalPlugin.DataReaderType.Component)
            {
                if (ComponentSort == null)
                    return defaultSortIndex;
                if (String.IsNullOrEmpty(ComponentSortCompareAgainstFieldName))
                    return defaultSortIndex;
                if (String.IsNullOrEmpty(fieldValue.Trim()))
                    return defaultSortIndex;

                if (fieldName.Equals(ComponentSortCompareAgainstFieldName))
                {
                    var sortIndex = ComponentSort.Find(s => s.Value.Contains(fieldValue));
                    if (sortIndex != null)
                        return sortIndex.Order;
                }

                return MaxComponentSortIndex;
            }

            return defaultSortIndex;
        }

        public string GetFieldNameForSort(ExternalPlugin.DataReaderType type)
        {
            if (type == ExternalPlugin.DataReaderType.Component)
                return ComponentSortFieldName;
            else if (type == ExternalPlugin.DataReaderType.Structure)
                return StructureSortFieldName;

            return String.Empty;
        }

        public string GetFieldNameForCompare(ExternalPlugin.DataReaderType type)
        {
            if (type == ExternalPlugin.DataReaderType.Component)
                return ComponentSortCompareAgainstFieldName;
            else if (type == ExternalPlugin.DataReaderType.Structure)
                return StructureSortCompareAgainstFieldName;

            return String.Empty;
        }

        internal int GetMaxSortingIndex(ExternalPlugin.DataReaderType dataReaderEntityType)
        {
            if (dataReaderEntityType == ExternalPlugin.DataReaderType.Component)
                return MaxComponentSortIndex;
            else if (dataReaderEntityType == ExternalPlugin.DataReaderType.Structure)
                return MaxStructureSortIndex;

            return defaultSortIndex;
        }
    }
}
