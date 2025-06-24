using System.Globalization;

namespace System
{
    /// <summary>
    /// Extension methods for all objects.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Used to simplify and beautify casting an object to a type. 
        /// </summary>
        /// <typeparam name="T">Type to be casted</typeparam>
        /// <param name="obj">Object to cast</param>
        /// <returns>Casted object</returns>
        public static T As<T>(this object obj)
            where T : class
        {
            return (T)obj;
        }

        /// <summary>
        /// Converts given object to a value type using <see cref="Convert.ChangeType(object,System.Type)"/> method.
        /// </summary>
        /// <param name="obj">Object to be converted</param>
        /// <typeparam name="T">Type of the target object</typeparam>
        /// <returns>Converted object</returns>
        public static T To<T>(this object obj)
            where T : struct
        {
            return (T)Convert.ChangeType(obj, typeof(T), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Check if an item is in a list.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <param name="list">List of items</param>
        /// <typeparam name="T">Type of the items</typeparam>
        public static bool IsIn<T>(this T item, params T[] list)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// Check given object to a value is null
        /// </summary>
        /// <param name="obj">Object to be checked</param>
        /// <returns>Bool</returns>
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        /// <summary>
        /// Check given object to a value is not null
        /// </summary>
        /// <param name="obj">Object to be checked</param>
        /// <returns>Bool</returns>
        public static bool IsNotNull(this object obj)
        {
            return !obj.IsNull();
        }

        public static T GetValue<T>(this object obj, string propertyName)
        {
            if (obj.IsNull())
                return default;

            var objClass = obj.GetType();
            var propList = objClass.GetProperties().ToList();
            foreach (var item in propList)
            {
                if (item.Name == propertyName)
                {
                    var value = item.GetValue(obj);
                    if (value is T tValue)
                    {
                        return tValue;
                    }
                    return default;

                }
            }
            return default;
        }
    }
}
