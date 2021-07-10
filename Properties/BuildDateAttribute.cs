using System;
using System.Globalization;

[AttributeUsage(AttributeTargets.Assembly)]
internal class BuildDateAttribute : Attribute
{
    public DateTime DateTime { get; }

    public BuildDateAttribute(string value)
    {
        DateTime = DateTime.ParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
    }
}
