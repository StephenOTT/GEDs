using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using GEDs.Enums;
using System.Reflection;

namespace GEDs.Helpers
{
    public static class HTMLEntities
    {
        public static SelectList GenerateGedsTypeList(GedsType selected)
        {
            SelectListItem component = new SelectListItem();
            component.Text = GedsType.Component.ToString();
            component.Value = GedsType.Component.ToString();
            if (selected == GedsType.Component)
                component.Selected = true;

            SelectListItem structure = new SelectListItem();
            structure.Text = GedsType.Structure.ToString();
            structure.Value = GedsType.Structure.ToString();
            if (selected == GedsType.Structure)
                structure.Selected = true;

            return new SelectList(new List<SelectListItem>() { component, structure });
        }



        public static List<SelectListItem> GenerateSelectList<T>(IEnumerable<T> entities, string text, string value, string selected = null)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            Type entityType = typeof(T);
            FieldInfo textField = entityType.GetField(text);
            FieldInfo valueField = entityType.GetField(value);
            
            foreach (var entity in entities)
            {
                SelectListItem item = new SelectListItem();
                item.Text = textField.GetValue(entity).ToString();
                item.Value = valueField.GetValue(entity).ToString();

                if(selected != null && ((string)selected).Equals(item.Value))
                    item.Selected = true;
            }

            return items;
        }
    }
}