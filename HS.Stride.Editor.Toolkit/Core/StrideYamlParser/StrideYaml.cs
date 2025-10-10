using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using HS.Stride.Editor.Toolkit.Core.DataTypes;

namespace HS.Stride.Editor.Toolkit.Core.StrideYamlParser
{
    /// <summary>
    /// Base class for Stride YAML generation with common formatting utilities.
    /// Provides indent, value formatting, and structure helpers.
    /// </summary>
    public class StrideYaml
    {
        // Common indent size (4 spaces per level)
        protected const int IndentSize = 4;

        /// <summary>
        /// Creates indent string for specified level (level * 4 spaces).
        /// </summary>
        protected static string Indent(int level)
        {
            return new string(' ', level * IndentSize);
        }

        /// <summary>
        /// Formats a value for YAML output based on its type.
        /// Handles floats, doubles, bools, nulls, and strings.
        /// </summary>
        protected static string FormatValue(object? value)
        {
            if (value == null) return "null";

            return value switch
            {
                bool b => b ? "true" : "false",
                float f => FormatFloat(f),
                double d => FormatFloat(d),
                string s => s,
                _ => value.ToString() ?? "null"
            };
        }

        /// <summary>
        /// Formats floating-point numbers to match Stride's format.
        /// Uses 0.0 for zero, includes trailing .0 for whole numbers.
        /// </summary>
        protected static string FormatFloat(double value)
        {
            if (value == 0.0) return "0.0";

            string str = value.ToString("0.0#######", System.Globalization.CultureInfo.InvariantCulture);

            // Add .0 if it's a whole number without decimal point
            if (!str.Contains('.') && !str.Contains('E') && !str.Contains('e'))
            {
                str += ".0";
            }

            return str;
        }

        /// <summary>
        /// Writes a Vector3 structure inline: {X: 0.0, Y: 0.0, Z: 0.0}
        /// </summary>
        protected static string FormatVector3(Vector3 vector)
        {
            return $"{{X: {FormatFloat(vector.X)}, Y: {FormatFloat(vector.Y)}, Z: {FormatFloat(vector.Z)}}}";
        }

        /// <summary>
        /// Writes a Quaternion structure inline: {X: 0.0, Y: 0.0, Z: 0.0, W: 1.0}
        /// </summary>
        protected static string FormatQuaternion(Quaternion quaternion)
        {
            return $"{{X: {FormatFloat(quaternion.X)}, Y: {FormatFloat(quaternion.Y)}, Z: {FormatFloat(quaternion.Z)}, W: {FormatFloat(quaternion.W)}}}";
        }

        /// <summary>
        /// Writes a Color structure inline: {R: 1.0, G: 1.0, B: 1.0, A: 1.0}
        /// </summary>
        protected static string FormatColor(ColorData color)
        {
            return $"{{R: {FormatFloat(color.R)}, G: {FormatFloat(color.G)}, B: {FormatFloat(color.B)}, A: {FormatFloat(color.A)}}}";
        }

        /// <summary>
        /// Checks if a dictionary contains only simple values (no nested dicts or lists).
        /// Used to determine if it should be formatted inline like {X: 0.0, Y: 0.0, Z: 0.0}
        /// </summary>
        protected static bool IsSimpleValueDict(Dictionary<string, object> dict)
        {
            return dict.Values.All(v => v is not Dictionary<string, object> && v is not System.Collections.IList);
        }
    }

}
